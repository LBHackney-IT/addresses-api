using AddressesAPI.Tests.V1.Helper;
using AddressesAPI.V1.Boundary.Responses;
using AddressesAPI.V1.Boundary.Responses.Metadata;
using AutoFixture;
using Bogus;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AddressesAPI.Tests.V1.E2ETests
{
    public class GetAddressCrossReferenceIntegrationTests : IntegrationTests<Startup>
    {
        private readonly Faker _faker = new Faker();
        private readonly Fixture _fixture = new Fixture();
        [Test]
        public async Task GetCrossReferenceAddressReturns200()
        {
            var uprn = _faker.Random.Int();
            TestDataHelper.InsertCrossRef(uprn, Db);

            var url = new Uri($"api/v1/properties/{uprn}/crossreferences", UriKind.Relative);
            var response = await Client.GetAsync(url).ConfigureAwait(true);
            response.StatusCode.Should().Be(200);

            var convertedResponse = await ConvertToResponseObject(response).ConfigureAwait(true);

            convertedResponse.Error.Should().BeNull();
        }

        [Test]
        public async Task GetDetailedCrossReferenceAddressRecord()
        {
            var uprn = _faker.Random.Int();
            var record = _fixture.Build<DatabaseCrossRefAddressRecord>()
                                               .With(add => add.UPRN, uprn)
                                               .Create();
            TestDataHelper.InsertCrossRef(uprn, Db, record);

            var url = new Uri($"api/v1/properties/{uprn}/crossreferences", UriKind.Relative);
            var response = await Client.GetAsync(url).ConfigureAwait(true);
            response.StatusCode.Should().Be(200);

            var apiResponse = await ConvertToResponseObject(response).ConfigureAwait(true);

            apiResponse.Error.Should().BeNull();

            var recordReturned = apiResponse.Data.AddressCrossReferences.FirstOrDefault();
            recordReturned.UPRN.Should().Be(uprn);
            recordReturned.Name.Should().Be(record.Name);
            recordReturned.Value.Should().Be(record.Value);
            recordReturned.Code.Should().Be(record.Code);
            recordReturned.CrossRefKey.Should().Be(record.CrossRefKey);
            recordReturned.EndDate.Should().Be(record.EndDate.Value.Date);
        }
        [Test]
        public async Task Get404WhenUPRNIsNotProvided()
        {
            var uprn = _faker.Random.Int();
            TestDataHelper.InsertCrossRef(uprn, Db);

            var url = new Uri($"api/v1/properties/crossreferences", UriKind.Relative);
            var response = await Client.GetAsync(url).ConfigureAwait(true);
            response.StatusCode.Should().Be(404);
        }

        [Test]
        public async Task Get404WhenAddressCannotBeFound()
        {
            var uprn = _faker.Random.Int();
            var uprnempty = "";
            var url = new Uri($"api/v1/properties/{uprnempty}/crossreferences", UriKind.Relative);
            var response = await Client.GetAsync(url).ConfigureAwait(true);
            response.StatusCode.Should().Be(404);

        }

        private static async Task<APIResponse<GetAddressCrossReferenceResponse>> ConvertToResponseObject(HttpResponseMessage response)
        {
            var data = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            return JsonConvert.DeserializeObject<APIResponse<GetAddressCrossReferenceResponse>>(data);
        }



    }
}
