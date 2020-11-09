using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AddressesAPI.Infrastructure;
using AddressesAPI.Tests.V2.Helper;
using AddressesAPI.V1.Boundary.Responses;
using AddressesAPI.V1.Boundary.Responses.Metadata;
using AutoFixture;
using Bogus;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace AddressesAPI.Tests.V2.E2ETests
{
    public class GetAddressCrossReferenceIntegrationTests : IntegrationTests<Startup>
    {
        private readonly Faker _faker = new Faker();
        private readonly Fixture _fixture = new Fixture();
        [Test]
        public async Task GetCrossReferenceAddressReturns200()
        {
            var uprn = _faker.Random.Int();
            TestEfDataHelper.InsertCrossReference(DatabaseContext, uprn);

            var url = new Uri($"api/v2/properties/{uprn}/crossreferences", UriKind.Relative);
            var response = await Client.GetAsync(url).ConfigureAwait(true);
            response.StatusCode.Should().Be(200);

            var convertedResponse = await ConvertToCrossReferenceResponseObject(response).ConfigureAwait(true);

            convertedResponse.StatusCode.Should().Be(200);
        }

        [Test]
        public async Task GetDetailedCrossReferenceAddressRecord()
        {
            var uprn = _faker.Random.Int();
            var record = _fixture.Build<CrossReference>()
                                               .With(add => add.UPRN, uprn)
                                               .Create();
            TestEfDataHelper.InsertCrossReference(DatabaseContext, uprn, record);

            var url = new Uri($"api/v2/properties/{uprn}/crossreferences", UriKind.Relative);
            var response = await Client.GetAsync(url).ConfigureAwait(true);
            var apiResponse = await ConvertToCrossReferenceResponseObject(response).ConfigureAwait(true);

            var recordReturned = apiResponse.Data.AddressCrossReferences.FirstOrDefault();
            recordReturned.UPRN.Should().Be(uprn);
            recordReturned.Name.Should().Be(record.Name);
            recordReturned.Value.Should().Be(record.Value);
            recordReturned.Code.Should().Be(record.Code);
            recordReturned.CrossRefKey.Should().Be(record.CrossRefKey);
            recordReturned.EndDate.Value.Date.Should().Be(record.EndDate.Value.Date);
        }
        [Test]
        public async Task Get404WhenUPRNIsNotProvided()
        {
            var uprn = _faker.Random.Int();
            TestEfDataHelper.InsertCrossReference(DatabaseContext, uprn);

            var url = new Uri($"api/v2/properties/crossreferences", UriKind.Relative);
            var response = await Client.GetAsync(url).ConfigureAwait(true);
            response.StatusCode.Should().Be(404);
        }

        [Test]
        public async Task Get400WhenUPRNIsNotNumeric()
        {
            var uprn = _faker.Random.Int();
            TestEfDataHelper.InsertCrossReference(DatabaseContext, uprn);

            var url = new Uri($"api/v2/properties/1937dhhu/crossreferences", UriKind.Relative);
            var response = await Client.GetAsync(url).ConfigureAwait(true);
            response.StatusCode.Should().Be(400);
        }

        private static async Task<APIResponse<GetAddressCrossReferenceResponse>> ConvertToCrossReferenceResponseObject(HttpResponseMessage response)
        {
            var data = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            return JsonConvert.DeserializeObject<APIResponse<GetAddressCrossReferenceResponse>>(data);
        }
    }
}
