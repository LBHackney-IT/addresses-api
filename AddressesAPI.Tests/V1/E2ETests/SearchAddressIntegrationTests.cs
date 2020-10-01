using System;
using System.Net.Http;
using System.Threading.Tasks;
using AddressesAPI.Tests.V1.Helper;
using AddressesAPI.V1.Boundary.Responses;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace AddressesAPI.Tests.V1.E2ETests
{
    public class SearchAddressIntegrationTests : IntegrationTests<Startup>
    {
        [Ignore("In progress")]
        [Test]
        public async Task SearchAddressReturns200()
        {
            var response = await CallEndpointWithQueryParameters().ConfigureAwait(true);
            response.StatusCode.Should().Be(200);
        }

        private async Task<HttpResponseMessage> CallEndpointWithQueryParameters()
        {
            var url = new Uri("api/v1/addresses", UriKind.Relative);
            return await Client.GetAsync(url).ConfigureAwait(true);
        }

        private static async Task<SearchAddressResponse> ConvertToResponseObject(HttpResponseMessage response)
        {
            var data = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            return JsonConvert.DeserializeObject<SearchAddressResponse>(data);
        }
    }
}
