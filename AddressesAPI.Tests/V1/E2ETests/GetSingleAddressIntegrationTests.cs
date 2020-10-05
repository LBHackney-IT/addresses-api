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
        public async Task GetAddressReturns200WithValidAddressIdParameter()
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

            var returnedRecord = apiResponse.Data.Addresses.FirstOrDefault();

            returnedRecord.AddressKey.Should().Be(addressId);
            returnedRecord.USRN.Should().Be(addressRecord.usrn);
            returnedRecord.UPRN.Should().Be(addressRecord.uprn);
            returnedRecord.Line1.Should().BeEquivalentTo(addressRecord.line1);
            returnedRecord.Line2.Should().BeEquivalentTo(addressRecord.line2);
            returnedRecord.Line3.Should().BeEquivalentTo(addressRecord.line3);
            returnedRecord.Line4.Should().BeEquivalentTo(addressRecord.line4);
            returnedRecord.Town.Should().BeEquivalentTo(addressRecord.town);
            returnedRecord.Postcode.Should().BeEquivalentTo(addressRecord.postcode);
        }

        [Test]
        public async Task GetAddressReturns500WhenAddressIdParameterIsNot14Characters()
        {

            var addressId = _faker.Random.String2(14);
            TestDataHelper.InsertAddress(addressId, Db);

            var incorrectLength = addressId.Substring(0, 10);

            var url = new Uri($"api/v1/addresses/{incorrectLength}", UriKind.Relative);

            var response = await Client.GetAsync(url).ConfigureAwait(true);

            response.StatusCode.Should().Be(500);
            response.ReasonPhrase.Should().Be("Internal Server Error");
        }

        [Test]
        public async Task GetAddressReturns400IfAddressIdParameterIsEmpty()
        {

            var addressId = _faker.Random.String2(14);
            TestDataHelper.InsertAddress(addressId, Db);

            const string emptyId = "";
            var url = new Uri($"api/v1/addresses/{emptyId}", UriKind.Relative);

            var response = await Client.GetAsync(url).ConfigureAwait(true);

            response.StatusCode.Should().Be(400);
            response.ReasonPhrase.Should().Be("Bad Request");
        }
    }
}
