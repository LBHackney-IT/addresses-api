using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AddressesAPI.Tests.V1.Helper;
using AddressesAPI.V1.Boundary.Responses;
using AddressesAPI.V1.Boundary.Responses.Metadata;
using AutoFixture;
using Bogus;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace AddressesAPI.Tests.V1.E2ETests
{
    public class GetSingleAddressIntegrationTests : IntegrationTests<Startup>
    {
        private readonly Faker _faker = new Faker();
        private readonly IFixture _fixture = new Fixture();

        [Test]
        public async Task GetAddressReturns200()
        {
            var addressId = _faker.Random.String2(14);
            TestDataHelper.InsertAddress(addressId, Db);

            var url = new Uri($"api/v1/addresses/{addressId}", UriKind.Relative);

            var response = await Client.GetAsync(url).ConfigureAwait(true);

            response.StatusCode.Should().Be(200);
        }

        [Test]
        public async Task GetAddressReturnsADetailedAddressRecordInTheResponse()
        {
            var addressId = _faker.Random.String2(14);
            var addressRecord = _fixture.Build<DatabaseAddressRecord>()
                .With(add => add.lpi_key, addressId)
                .Create();

            TestDataHelper.InsertAddress(addressId, Db, addressRecord);

            var url = new Uri($"api/v1/addresses/{addressId}", UriKind.Relative);

            var response = await Client.GetAsync(url).ConfigureAwait(true);

            response.StatusCode.Should().Be(200);

            var apiResponse = await ConvertToResponseObject(response).ConfigureAwait(true);
            apiResponse.Error.Should().BeNull();

            var singleAddress = apiResponse.Data.Addresses.FirstOrDefault();

            singleAddress.AddressKey.Should().Be(addressId);
            singleAddress.USRN.Should().Be(addressRecord.usrn);
            singleAddress.UPRN.Should().Be(addressRecord.uprn);
            singleAddress.Line1.Should().BeEquivalentTo(addressRecord.line1);
            singleAddress.Line2.Should().BeEquivalentTo(addressRecord.line2);
            singleAddress.Line3.Should().BeEquivalentTo(addressRecord.line3);
            singleAddress.Line4.Should().BeEquivalentTo(addressRecord.line4);
            singleAddress.Town.Should().BeEquivalentTo(addressRecord.town);
            singleAddress.Postcode.Should().BeEquivalentTo(addressRecord.postcode);
        }


        private static async Task<APIResponse<SearchAddressResponse>> ConvertToResponseObject(HttpResponseMessage response)
        {
            var data = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            return JsonConvert.DeserializeObject<APIResponse<SearchAddressResponse>>(data);
        }
    }
}
