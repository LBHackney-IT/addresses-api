using System;
using System.Linq;
using System.Threading.Tasks;
using AddressesAPI.Infrastructure;
using AddressesAPI.Tests.V1.Helper;
using AutoFixture;
using Bogus;
using FluentAssertions;
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
            TestEfDataHelper.InsertAddress(DatabaseContext, addressId);

            var url = new Uri($"api/v1/addresses/{addressId}", UriKind.Relative);

            var response = await Client.GetAsync(url).ConfigureAwait(true);

            response.StatusCode.Should().Be(200);
        }

        [Test]
        public async Task GetAddressReturnsADetailedAddressRecordInTheResponse()
        {
            var addressId = _faker.Random.String2(14);
            var addressRecord = _fixture.Build<NationalAddress>()
                .With(add => add.AddressKey, addressId)
                .Create();

            TestEfDataHelper.InsertAddress(DatabaseContext, addressId, addressRecord);

            var url = new Uri($"api/v1/addresses/{addressId}", UriKind.Relative);

            var response = await Client.GetAsync(url).ConfigureAwait(true);

            response.StatusCode.Should().Be(200);

            var apiResponse = await response.ConvertToSearchAddressResponseObject().ConfigureAwait(true);

            var returnedRecord = apiResponse.Data.Addresses.FirstOrDefault();

            returnedRecord.AddressKey.Should().Be(addressId);
            returnedRecord.USRN.Should().Be(addressRecord.USRN);
            returnedRecord.UPRN.Should().Be(addressRecord.UPRN);
            returnedRecord.Line1.Should().BeEquivalentTo(addressRecord.Line1);
            returnedRecord.Line2.Should().BeEquivalentTo(addressRecord.Line2);
            returnedRecord.Line3.Should().BeEquivalentTo(addressRecord.Line3);
            returnedRecord.Line4.Should().BeEquivalentTo(addressRecord.Line4);
            returnedRecord.Town.Should().BeEquivalentTo(addressRecord.Town);
            returnedRecord.Postcode.Should().BeEquivalentTo(addressRecord.Postcode);
        }

        //TODO: We would want this to return 400 rather than 500

        [Test]
        public async Task GetAddressReturns500WhenAddressIdParameterIsNot14Characters()
        {

            var addressId = _faker.Random.String2(14);
            TestEfDataHelper.InsertAddress(DatabaseContext, addressId);

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
            TestEfDataHelper.InsertAddress(DatabaseContext, addressId);

            const string emptyId = "";
            var url = new Uri($"api/v1/addresses/{emptyId}", UriKind.Relative);

            var response = await Client.GetAsync(url).ConfigureAwait(true);

            response.StatusCode.Should().Be(400);
            response.ReasonPhrase.Should().Be("Bad Request");
        }

        //TODO: We would want this to return a 404 rather than 200
        [Test]
        public async Task GetAddressReturnsNoAddressesIfAddressIdParameterDoesNotMatchARecordInTheDatabase()
        {

            var addressId = _faker.Random.String2(14);
            TestEfDataHelper.InsertAddress(DatabaseContext, addressId);

            var differentId = _faker.Random.String2(14);

            var url = new Uri($"api/v1/addresses/{differentId}", UriKind.Relative);

            var response = await Client.GetAsync(url).ConfigureAwait(true);

            response.StatusCode.Should().Be(200);

            var convertedResponse = await response.ConvertToSearchAddressResponseObject().ConfigureAwait(true);

            convertedResponse.Data.Addresses.Should().BeNull();
        }
    }
}
