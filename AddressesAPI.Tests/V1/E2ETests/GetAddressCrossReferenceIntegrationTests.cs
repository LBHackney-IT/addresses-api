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
        //private readonly IFixture _fixture = new Fixture();
        [Test]
        public async Task GetCrossReferenceAddressReturns200()
        {
            var uprn = _faker.Random.Int();
            TestDataHelper.InsertCrossRef(uprn, Db);
            var response = await CallEndpointWithQueryParameters().ConfigureAwait(true);
            response.StatusCode.Should().Be(200);
        }

        private async Task<HttpResponseMessage> CallEndpointWithQueryParameters()
        {
            var url = new Uri("api/v1/properties/{uprn}/crossreferences", UriKind.Relative);
            return await Client.GetAsync(url).ConfigureAwait(true);
        }

        private static async Task<APIResponse<GetAddressCrossReferenceResponse>> ConvertToResponseObject(HttpResponseMessage response)
        {
            var data = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            return JsonConvert.DeserializeObject<APIResponse<GetAddressCrossReferenceResponse>>(data);
        }
    }
}
