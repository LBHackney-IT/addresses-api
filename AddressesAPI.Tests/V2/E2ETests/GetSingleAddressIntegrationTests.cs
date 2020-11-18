using System;
using System.Linq;
using System.Threading.Tasks;
using AddressesAPI.Infrastructure;
using AddressesAPI.Tests.V2.Helper;
using AddressesAPI.V2.Boundary.Responses.Metadata;
using AutoFixture;
using Bogus;
using FluentAssertions;
using NUnit.Framework;

namespace AddressesAPI.Tests.V2.E2ETests
{
    public class GetSingleAddressIntegrationTests : IntegrationTests<Startup>
    {
        private readonly Faker _faker = new Faker();
        private readonly IFixture _fixture = new Fixture();

        [Test]
        public async Task GetAddressReturns200WithValidAddressIdParameter()
        {
            var addressId = _faker.Random.String2(14);
            TestEfDataHelper.InsertAddressInDb(DatabaseContext, addressId);

            var url = new Uri($"api/v2/addresses/{addressId}", UriKind.Relative);

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

            TestEfDataHelper.InsertAddressInDb(DatabaseContext, addressId, addressRecord);

            var url = new Uri($"api/v2/addresses/{addressId}", UriKind.Relative);

            var response = await Client.GetAsync(url).ConfigureAwait(true);

            response.StatusCode.Should().Be(200);

            var apiResponse = await response.ConvertToGetAddressResponseObject().ConfigureAwait(true);

            var returnedRecord = apiResponse.Data.Address;

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

        [Test]
        public async Task GetAddressReturns400WhenAddressIdParameterIsNot14Characters()
        {

            var addressId = _faker.Random.String2(14);
            TestEfDataHelper.InsertAddressInDb(DatabaseContext, addressId);

            var incorrectLength = addressId.Substring(0, 10);

            var url = new Uri($"api/v2/addresses/{incorrectLength}", UriKind.Relative);

            var response = await Client.GetAsync(url).ConfigureAwait(true);
            response.StatusCode.Should().Be(400);

            var data = await response.ConvertToErrorResponseObject().ConfigureAwait(true);
            data.StatusCode.Should().Be(400);
            data.Errors.Count().Should().Be(1);
            data.Errors.First().Message.Should().BeEquivalentTo("Address Key is invalid. It should by 14 characters long string.");
        }

        [Test]
        public async Task GetAddressReturnsNoAddressesIfAddressIdParameterDoesNotMatchARecordInTheDatabase()
        {

            var addressId = _faker.Random.String2(14);
            TestEfDataHelper.InsertAddressInDb(DatabaseContext, addressId);

            var differentId = _faker.Random.String2(14);

            var url = new Uri($"api/v2/addresses/{differentId}", UriKind.Relative);

            var response = await Client.GetAsync(url).ConfigureAwait(true);

            response.StatusCode.Should().Be(404);

            var data = await response.ConvertToErrorResponseObject().ConfigureAwait(true);
            data.StatusCode.Should().Be(404);
            data.Errors.Count().Should().Be(1);
            data.Errors.First().Message.Should().BeEquivalentTo("An address could not be found for the given key");
        }
    }
}
