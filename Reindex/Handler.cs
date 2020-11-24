using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.SQS;
using Amazon.SQS.Model;
using Elasticsearch.Net;
using Nest;
using Nest.JsonNetSerializer;
using Newtonsoft.Json;
using JsonConverter = System.Text.Json.Serialization.JsonConverter;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace Reindex
{
    public class Handler
    {
        private ElasticClient _elasticSearchClient;
        private string _indexFilePath;
        private IAmazonSQS _sqsClient;
        private string _sqsQueue;

        public Handler()
        {
            SetupElasticsearch();
            _indexFilePath = "./index.json";
            _sqsQueue = Environment.GetEnvironmentVariable("SQS_QUEUE_URL");
            _sqsClient = new AmazonSQSClient();

        }

        //This constructor is used for testing
        public Handler(IAmazonSQS sqsClient, string indexFilePath)
        {
            SetupElasticsearch();
            _indexFilePath = indexFilePath;
            _sqsQueue = Environment.GetEnvironmentVariable("SQS_QUEUE_URL");
            _sqsClient = sqsClient;

        }

        private void SetupElasticsearch()
        {
            var esDomain = Environment.GetEnvironmentVariable("ELASTICSEARCH_DOMAIN_URL")
                           ?? "http://localhost:9202";
            var pool = new SingleNodeConnectionPool(new Uri(esDomain));
            var settings = new ConnectionSettings(pool, JsonNetSerializer.Default)
                .PrettyJson().ThrowExceptions().DisableDirectStreaming();
            _elasticSearchClient = new ElasticClient(settings);
        }

        public async Task ReindexAlias(ReindexRequest request, ILambdaContext context)
        {
            var alias = request.alias;
            var indexName = request.fromIndex
                            ?? (await _elasticSearchClient.GetIndicesPointingToAliasAsync(alias))?.First();

            LambdaLogger.Log($"Current Index name for alias {alias}: {indexName}");

            var indexConfig = request.config ?? await File.ReadAllTextAsync(_indexFilePath);

            var newIndexName = alias + "_" + DateTime.Now.ToString("yyyyMMddhhmm");

            _elasticSearchClient.LowLevel.Indices.Create<BytesResponse>(newIndexName, indexConfig);

            LambdaLogger.Log($"Created new index with name {newIndexName}");

            var response = _elasticSearchClient.ReindexOnServer(r => r
                .Source(s => s.Index(Indices.Index(indexName)))
                .Destination(d => d.Index(newIndexName))
                .WaitForCompletion(false));
            LambdaLogger.Log($"{response.DebugInformation}");
            LambdaLogger.Log($"Re-indexed task ID {response.Task.FullyQualifiedId}");

            var sqsMessage = new SqsMessage
            {
                alias = alias,
                newIndex = newIndexName,
                taskId = response.Task.FullyQualifiedId
            };

            var sqsResponse = await SendSqsMessageToQueue(JsonConvert.SerializeObject(sqsMessage));
            LambdaLogger.Log($"Sent task ID to sqs queue messageId: {sqsResponse?.MessageId}");
        }

        public async Task SwitchAlias(SQSEvent sqsEvent, ILambdaContext context)
        {
            var message = sqsEvent.Records.FirstOrDefault()?.Body;
            if (message == null) return;
            var data = JsonConvert.DeserializeObject<SqsMessage>(message);
            LambdaLogger.Log($"Received SQS message {message}");
            var task = await _elasticSearchClient.Tasks.GetTaskAsync(new TaskId(data.taskId));

            if (!task.Completed)
            {
                LambdaLogger.Log("Task has not completed: re-adding message to queue with 10 minute delay");
                var sqsResponse = await SendSqsMessageToQueue(message);
                LambdaLogger.Log($"Re-sent task ID to sqs queue messageId: {sqsResponse.MessageId}");
                return;
            }

            if (task.GetResponse<ReindexOnServerResponse>().Failures.Any())
            {
                LambdaLogger.Log("Failures when reindxing");
                foreach (var bulkIndexByScrollFailure in task.GetResponse<ReindexOnServerResponse>().Failures)
                {
                    LambdaLogger.Log(bulkIndexByScrollFailure.Cause.Reason);
                }
            }

            await RemoveAllReferencesToAlias(data.alias);

            await _elasticSearchClient.Indices.PutAliasAsync(Indices.Index(data.newIndex), data.alias);
            LambdaLogger.Log($"Address alias {data.alias} to index {data.newIndex}");

            await _elasticSearchClient.DeleteAsync<TaskId>(data.taskId, d => d.Index(".tasks"));
            LambdaLogger.Log($"Deleted Task document with ID {data.taskId}");

            //Delete old index?
        }

        private async Task RemoveAllReferencesToAlias(string alias)
        {
            var indicesForAlias = await _elasticSearchClient.GetIndicesPointingToAliasAsync(alias);
            foreach (var index in indicesForAlias)
            {
                await _elasticSearchClient.Indices.DeleteAliasAsync(Indices.Index(index), alias);
                LambdaLogger.Log($"Removed alias {alias} from index {index}");
            }
        }

        private async Task<SendMessageResponse> SendSqsMessageToQueue(string message)
        {
            var delaySeconds = 600;
            try
            {
                delaySeconds = Convert.ToInt32(Environment.GetEnvironmentVariable("SQS_MESSAGE_DELAY"));
            }
            catch (Exception)
            {
                LambdaLogger.Log("SQS_MESSAGE_DELAY either not found or not an integer, using default of 600s");
            }

            var sqsRequest = new SendMessageRequest
            {
                DelaySeconds = delaySeconds,
                MessageBody = message,
                QueueUrl = _sqsQueue,
            };
            return await _sqsClient.SendMessageAsync(sqsRequest);
        }
    }
}
