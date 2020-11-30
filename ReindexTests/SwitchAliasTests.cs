using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.SQSEvents;
using Amazon.SQS;
using Amazon.SQS.Model;
using AutoFixture;
using Elasticsearch.Net;
using FluentAssertions;
using Moq;
using Nest;
using Newtonsoft.Json;
using NUnit.Framework;
using Reindex;

namespace ReindexTests
{
    public class SwitchAliasTests : ElasticsearchTests
    {
        private readonly IFixture _fixture = new Fixture();
        private Handler _classUnderTest;
        private Mock<IAmazonSQS> _sqsClientMock;

        [SetUp]
        public void SetUp()
        {
            _sqsClientMock = new Mock<IAmazonSQS>();
            _classUnderTest = new Handler(_sqsClientMock.Object, "./../../../../data/elasticsearch/index.json");
        }

        [Test]
        public async Task RemovesAliasFromOldIndexAndAddsToNewIndex()
        {
            CreateIndexWithConfig("initialindex", "{}");
            var alias = _fixture.Create<string>();
            await AssignToAlias(alias, "initialindex");

            var sqsMessage = new SqsMessage();
            _sqsClientMock.Setup(s => s.SendMessageAsync(It.Is<SendMessageRequest>(
                    s => StoreMessage(s, out sqsMessage)), default))
                .ReturnsAsync(new SendMessageResponse());

            await _classUnderTest.ReindexAlias(new ReindexRequest { alias = alias }, null);

            sqsMessage.alias.Should().Be(alias);

            System.Threading.Thread.Sleep(1000);
            await _classUnderTest.SwitchAlias(SqsEvent(sqsMessage), null);

            var indices = await ElasticsearchClient.GetIndicesPointingToAliasAsync(alias);
            indices.Count.Should().Be(1);
            indices.First().Should().Contain(sqsMessage.newIndex);
        }

        [Test]
        public async Task RemovesTheTaskDocument()
        {
            CreateIndexWithConfig("initialindex", "{}");
            var alias = _fixture.Create<string>();
            await AssignToAlias(alias, "initialindex");

            var sqsMessage = new SqsMessage();
            _sqsClientMock.Setup(s => s.SendMessageAsync(It.Is<SendMessageRequest>(
                    s => StoreMessage(s, out sqsMessage)), default))
                .ReturnsAsync(new SendMessageResponse());

            await _classUnderTest.ReindexAlias(new ReindexRequest { alias = alias }, null);
            System.Threading.Thread.Sleep(1000);
            await _classUnderTest.SwitchAlias(SqsEvent(sqsMessage), null);

            var task = await ElasticsearchClient.Tasks.GetTaskAsync(new TaskId(sqsMessage.taskId));
            task.ApiCall.HttpStatusCode.Should().Be(404);
        }

        [Test]
        public async Task IfSpecifiedDeleteIndexAfterReindexing()
        {
            CreateIndexWithConfig("initialindex", "{}");
            var alias = _fixture.Create<string>();
            await AssignToAlias(alias, "initialindex");

            var sqsMessage = new SqsMessage();
            _sqsClientMock.Setup(s => s.SendMessageAsync(It.Is<SendMessageRequest>(
                    s => StoreMessage(s, out sqsMessage)), default))
                .ReturnsAsync(new SendMessageResponse());

            await _classUnderTest.ReindexAlias(new ReindexRequest { alias = alias }, null);
            System.Threading.Thread.Sleep(1000);
            sqsMessage.deleteAfterReindex = true;
            await _classUnderTest.SwitchAlias(SqsEvent(sqsMessage), null);

            var index = await ElasticsearchClient.Indices.GetAsync(Indices.Index("initialindex"));
            index.Indices.Count.Should().Be(0);
        }

        private static SQSEvent SqsEvent(SqsMessage sqsMessage)
        {
            var sqsEvent = new SQSEvent
            {
                Records = new List<SQSEvent.SQSMessage>
                {
                    new SQSEvent.SQSMessage {Body = JsonConvert.SerializeObject(sqsMessage)}
                }
            };
            return sqsEvent;
        }

        private static bool StoreMessage(SendMessageRequest sqsRequest, out SqsMessage message)
        {
            message = JsonConvert.DeserializeObject<SqsMessage>(sqsRequest.MessageBody);
            return true;
        }

        private void CreateIndexWithConfig(string indexName, string config)
        {
            ElasticsearchClient.LowLevel.Index<BytesResponse>(indexName, PostData.String(config));
        }

        private async Task AssignToAlias(string alias, string indexName)
        {
            await ElasticsearchClient.Indices.PutAliasAsync(Indices.Index(indexName), alias);
        }
    }
}
