using System;
using System.Threading.Tasks;
using AddressesAPI.Tests.V1.Helper;
using Bogus;
using FluentAssertions;
using NUnit.Framework;

namespace AddressesAPI.Tests.V1.E2ETests
{
    public class GetSingleAddressIntegrationTests : IntegrationTests<Startup>
    {
        [Test]
        public async Task GetAddressReturns200()
        {
            var addressId = new Faker().Random.String(14);
            TestDataHelper.InsertAddress(addressId, Db);

            var url = new Uri($"api/v1/addresses/{addressId}", UriKind.Relative);

            var response = await Client.GetAsync(url).ConfigureAwait(true);

            response.StatusCode.Should().Be(200);
        }
    }
}
