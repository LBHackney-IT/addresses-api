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
        private const int MinimumQueueDelaySeconds = 600;

        // This is an AWS maximum
        private const int MaximumDelaySeconds = 900;

        private const int MaxAttempts = 10;

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
            try
            {
                var alias = request.alias;
                var indexName = request.fromIndex
                                ?? (await _elasticSearchClient.GetIndicesPointingToAliasAsync(alias))?.First();

                this.Log($"Current Index name for alias {alias}: {indexName}");

                var indexConfig = request.config ?? await File.ReadAllTextAsync(_indexFilePath);

                var newIndexName = alias + "_" + DateTime.Now.ToString("yyyyMMddhhmm");

                _elasticSearchClient.LowLevel.Indices.Create<BytesResponse>(newIndexName, indexConfig);

                this.Log($"Created new index with name {newIndexName}");

                var response = _elasticSearchClient.ReindexOnServer(r => r
                    .Source(s => s.Index(Indices.Index(indexName)))
                    .Destination(d => d.Index(newIndexName))
                    .WaitForCompletion(false));
                this.Log($"{response.DebugInformation}");
                this.Log($"Re-indexed task ID {response.Task.FullyQualifiedId}");

                var sqsMessage = new SqsMessage
                {
                    alias = alias,
                    newIndex = newIndexName,
                    taskId = response.Task.FullyQualifiedId,
                    deleteAfterReindex = request.deleteAfterReindex,
                    timeCreated = DateTime.Now,
                    attempts = 1
                };

                var sqsResponse = await SendSqsMessageToQueue(JsonConvert.SerializeObject(sqsMessage));
                this.Log($"Sent task ID to sqs queue messageId: {sqsResponse?.MessageId}");
            }
            catch (Exception ex)
            {
                // We must handle exceptions to prevent AWS retrying on error
                this.Log($"Exception caught in ReindexAlias {ex.Message}, {ex.StackTrace}, {ex.Source}");
            }
        }

        public async Task SwitchAlias(SQSEvent sqsEvent, ILambdaContext context)
        {
            try
            {
                var message = sqsEvent.Records.FirstOrDefault()?.Body;

                if (message == null) return;

                var data = JsonConvert.DeserializeObject<SqsMessage>(message);
                this.Log($"Received SQS message {message}");

                // Ensure that we have a gap between messages to prevent a possible billing issue
                if (!MessageTimingIsValid(data))
                {
                    this.Log($"Received SQS message too early - message timestamp {data.timeCreated}, time now {DateTime.Now}. Terminating to prevent possible message build up");
                    return;
                }

                // Ensure we don't try too many times
                if (data.attempts > MaxAttempts)
                {
                    this.Log($"Too many attempts have been detected - maximum number is {MaxAttempts}. Stopping. Terminating to prevent possible message build up");
                    return;
                }

                var task = await _elasticSearchClient.Tasks.GetTaskAsync(new TaskId(data.taskId));
                this.Log(JsonConvert.SerializeObject(task));

                if (task.ApiCall.HttpStatusCode == 404) return;
                if (!task.Completed)
                {
                    this.Log("Task has not completed: re-adding message to queue");
                    data.timeCreated = DateTime.Now;
                    data.attempts += 1;
                    var sqsResponse = await SendSqsMessageToQueue(JsonConvert.SerializeObject(data));
                    this.Log($"Re-sent task ID to sqs queue messageId: {sqsResponse.MessageId}");
                    return;
                }

                if (task.GetResponse<ReindexOnServerResponse>().Failures.Any())
                {
                    var response = task.GetResponse<ReindexOnServerResponse>();
                    this.Log("Failures when reindxing, IsValid: {response.IsValid}, Error: {response.ServerError}, Updated: {response.Updated}, DebugInfo: {response.DebugInformation}");
                    if (response.Failures != null)
                    {
                        foreach (var bulkIndexByScrollFailure in task.GetResponse<ReindexOnServerResponse>().Failures)
                        {
                            this.Log(bulkIndexByScrollFailure.Cause.Reason);
                        }
                    }
                    else
                    {
                        this.Log($"No detailed failure reasons given for reindexing failure");
                    }
                }

                this.Log($"Removing references to alias {data.alias}:");
                var indices = await RemoveAllReferencesToAlias(data.alias);

                var putResponse = await _elasticSearchClient.Indices.PutAliasAsync(Indices.Index(data.newIndex), data.alias);
                this.Log($"Added address alias {data.alias} to index {data.newIndex}, IsValid: {putResponse.IsValid}, Error: {putResponse.ServerError}, DebugInfo: {putResponse.DebugInformation}");

                var deleteResponse = await _elasticSearchClient.DeleteAsync<TaskId>(data.taskId, d => d.Index(".tasks"));
                this.Log($"Deleted Task document with ID {data.taskId}, IsValid: {deleteResponse.IsValid}, Error: {deleteResponse.ServerError}, DebugInfo: {deleteResponse.DebugInformation}");

                if (data.deleteAfterReindex)
                {
                    this.Log($"Removing indices for alias {data.alias}:");
                    await RemoveAllIndicesForAlias(indices);
                }
            }
            catch (Exception ex)
            {
                // We must handle exceptions to prevent AWS retrying on error
                this.Log($"Exception caught in SwitchAlias {ex.Message}, {ex.StackTrace}, {ex.Source}");
            }
        }

        private async Task RemoveAllIndicesForAlias(IEnumerable<string> indices)
        {
            foreach (var index in indices)
            {
                var response = await _elasticSearchClient.Indices.DeleteAsync(Indices.Index(index));
                this.Log($"    Removed index {index}, IsValid: {response.IsValid}, Error: {response.ServerError}, DebugInfo: {response.DebugInformation}");
            }
        }

        private async Task<IReadOnlyCollection<string>> RemoveAllReferencesToAlias(string alias)
        {
            var indicesForAlias = await _elasticSearchClient.GetIndicesPointingToAliasAsync(alias);
            foreach (var index in indicesForAlias)
            {
                await _elasticSearchClient.Indices.DeleteAliasAsync(Indices.Index(index), alias);
                this.Log($"    Removed alias {alias} from index {index}");
            }

            return indicesForAlias;
        }

        private async Task<SendMessageResponse> SendSqsMessageToQueue(string message)
        {
            var sqsRequest = new SendMessageRequest
            {
                DelaySeconds = GetSqsMessageDelaySeconds(),
                MessageBody = message,
                QueueUrl = _sqsQueue,
            };
            return await _sqsClient.SendMessageAsync(sqsRequest);
        }

        /// <summary>
        /// Calculates the delay in seconds for the queue message that checks if the indexing is complete.
        /// There was a bug where delaySeconds was being set to zero becuase the SQS_MESSAGE_DELAY
        /// environment variable was missing. This led to a very high AWS bill from message spamming.
        /// Thus, this code has been refactored to ensure a minimum delay of 600 seconds (10 minutes).
        /// </summary>
        /// <returns></returns>
        public int GetSqsMessageDelaySeconds()
        {
            var delaySeconds = MinimumQueueDelaySeconds;
            try
            {
                var configuredMessageDelay = Environment.GetEnvironmentVariable("SQS_MESSAGE_DELAY");
                // Must check for null, as  Convert.ToInt32(null) does NOT throw, it returns 0.
                if (configuredMessageDelay != null)
                {
                    delaySeconds = Convert.ToInt32(configuredMessageDelay);
                }
            }
            catch (Exception)
            {
                this.Log($"SQS_MESSAGE_DELAY either not found or not an integer, using default of {MinimumQueueDelaySeconds}s");
                delaySeconds = MinimumQueueDelaySeconds;
            }

            // Check min and max
            if (delaySeconds < MinimumQueueDelaySeconds)
            {
                delaySeconds = MinimumQueueDelaySeconds;
            }
            if (delaySeconds > MaximumDelaySeconds)
            {
                delaySeconds = MaximumDelaySeconds;
            }

            return delaySeconds;
        }

        public bool MessageTimingIsValid(SqsMessage message)
        {
            // Ensure that we have a gap between messages to prevent a possible billing issue
            if (message.timeCreated > DateTime.Now.AddSeconds(-MinimumQueueDelaySeconds))
            {
                return false;
            }
            return true;
        }

        protected virtual void Log(string message)
        {
            LambdaLogger.Log(message);
        }
    }
}
