using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
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
    public class ReindexAliasTests : ElasticsearchTests
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
        public async Task CreatesANewIndexWithCorrectName()
        {
            CreateIndexWithConfig("initialindex", "{}");
            var alias = _fixture.Create<string>();
            await AssignToAlias(alias, "initialindex");

            var settings = @"{""settings"": {}, ""mappings"": {} }";

            var expectedIndexName = alias + "_" + DateTime.Now.ToString("yyyyMMddhhmm");
            await _classUnderTest.ReindexAlias(new ReindexRequest{alias = alias, config = settings}, null);

            var indexExists = (await ElasticsearchClient.Indices.ExistsAsync(Indices.Index(expectedIndexName))).Exists;
            indexExists.Should().BeTrue();
        }

        [Test]
        public async Task CreatesANewIndexWithGivenSettings()
        {
            CreateIndexWithConfig("initialindex", "{}");
            var alias = _fixture.Create<string>();
            await AssignToAlias(alias, "initialindex");

            var settings = @"{""settings"": {""analysis"": {""analyzer"": {""my-custom-analyzer"": {""tokenizer"": ""keyword""}}}},""mappings"": {} }";

            var expectedIndexName = alias + "_" + DateTime.Now.ToString("yyyyMMddhhmm");
            await _classUnderTest.ReindexAlias(new ReindexRequest{alias = alias, config = settings}, null);

            var indexState = (await ElasticsearchClient.Indices.GetAsync(Indices.Index(expectedIndexName)))
                .Indices[expectedIndexName];
            var analyzerExists = indexState.Settings.Analysis.Analyzers.TryGetValue("my-custom-analyzer", out _);
            analyzerExists.Should().BeTrue();
        }

        [Test]
        public async Task CreateANewIndexWithConfigFromFileIfNoneGiven()
        {
            CreateIndexWithConfig("initialindex", "{}");
            var alias = _fixture.Create<string>();
            await AssignToAlias(alias, "initialindex");

            var expectedIndexName = alias + "_" + DateTime.Now.ToString("yyyyMMddhhmm");
            await _classUnderTest.ReindexAlias(new ReindexRequest{alias = alias}, null);

            var indexState = (await ElasticsearchClient.Indices.GetAsync(Indices.Index(expectedIndexName)))
                .Indices[expectedIndexName];
            indexState.Settings.Analysis.Analyzers.TryGetValue("whitespace_removed", out _).Should().BeTrue();
            indexState.Settings.Analysis.CharFilters.TryGetValue("remove_whitespace", out _).Should().BeTrue();
            indexState.Mappings.Properties.TryGetValue("postcode", out _).Should().BeTrue();
        }

        [Test]
        public async Task StartsReindexOfDataFromInitialIndexToNewIndex()
        {
            CreateIndexWithConfig("initialindex", "{}");
            var alias = _fixture.Create<string>();
            var documents = _fixture.CreateMany<QueryableAddress>();
            await AssignToAlias(alias, "initialindex");
            await IndexDocuments(alias, documents);

            var expectedIndexName = alias + "_" + DateTime.Now.ToString("yyyyMMddhhMM");
            await _classUnderTest.ReindexAlias(new ReindexRequest{alias = alias}, null);

            _sqsClientMock.Setup(a => a.SendMessageAsync(It.Is<SendMessageRequest>(
                    r => CheckSqsMessage(r, alias, expectedIndexName)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SendMessageResponse {MessageId = "ID"});
            _sqsClientMock.Verify();
        }

        private void CreateIndexWithConfig(string indexName, string config)
        {
            ElasticsearchClient.LowLevel.Index<BytesResponse>(indexName, PostData.String(config));
        }

        private async Task AssignToAlias(string alias, string indexName)
        {
            await ElasticsearchClient.Indices.PutAliasAsync(Indices.Index(indexName), alias);
        }

        private async Task IndexDocuments(string index, IEnumerable<QueryableAddress> documents)
        {
            foreach (var document in documents)
            {
                var indexed = await ElasticsearchClient.IndexAsync(document,
                    i => i.Index(index).Refresh(Refresh.WaitFor));
            }
        }

        private static bool CheckSqsMessage(SendMessageRequest request, string alias, string indexName)
        {
            var data = JsonConvert.DeserializeObject<SqsMessage>(request.MessageBody);
            return data.alias.Equals(alias) && data.newIndex.Equals(indexName) && data.taskId != null;
        }
    }
}
