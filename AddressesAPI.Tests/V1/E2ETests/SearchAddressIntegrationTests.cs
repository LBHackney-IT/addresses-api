using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AddressesAPI.Tests.V1.Helper;
using AddressesAPI.V1.Boundary.Requests;
using AddressesAPI.V1.Boundary.Responses;
using AddressesAPI.V1.Boundary.Responses.Metadata;
using AutoFixture;
using Bogus;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace AddressesAPI.Tests.V1.E2ETests
{
    public class SearchAddressIntegrationTests : IntegrationTests<Startup>
    {
        private readonly Faker _faker = new Faker();
        private readonly IFixture _fixture = new Fixture();

        [Test]
        public async Task SearchAddressReturns200()
        {
            var addressKey = "eytshdnshsuahs";
            TestDataHelper.InsertAddress(addressKey, Db);

            var queryString = "PostCode=E8";

            var response = await CallEndpointWithQueryParameters(queryString).ConfigureAwait(true);
            response.StatusCode.Should().Be(200);
        }

        [Test]
        public async Task SearchAddressReturnsAnAddressWithMatchingPostcode()
        {
            var addressKey = _faker.Random.String2(14);
            TestDataHelper.InsertAddress(addressKey, Db);
            AddSomeRandomAddressToTheDatabase();

            var queryString = "PostCode=E82LX&AddressStatus=Historical&Format=Detailed";

            var response = await CallEndpointWithQueryParameters(queryString).ConfigureAwait(true);
            response.StatusCode.Should().Be(200);
            var returnedAddress = await ConvertToResponseObject(response).ConfigureAwait(true);
            returnedAddress.Data.Addresses.Count.Should().Be(1);
            returnedAddress.Data.Addresses.First().AddressKey.Should().Be(addressKey);
        }

        [Test]
        public async Task CanSearchAddressesByUprn()
        {
            var addressKey = _faker.Random.String2(14);
            var queryParameters = new DatabaseAddressRecord
            {
                Uprn = _faker.Random.Int(),
            };
            TestDataHelper.InsertAddress(addressKey, Db, queryParameters);
            AddSomeRandomAddressToTheDatabase();

            var queryString = $"UPRN={queryParameters.Uprn}&AddressStatus=Historical&Format=Detailed";

            var response = await CallEndpointWithQueryParameters(queryString).ConfigureAwait(true);
            response.StatusCode.Should().Be(200);

            var returnedAddress = await ConvertToResponseObject(response).ConfigureAwait(true);
            returnedAddress.Data.Addresses.Count.Should().Be(1);
            returnedAddress.Data.Addresses.First().AddressKey.Should().Be(addressKey);
        }

        [Test]
        public async Task CanSearchAddressesByUsrn()
        {
            var addressKey = _faker.Random.String2(14);
            var queryParameters = new DatabaseAddressRecord
            {
                Usrn = _faker.Random.Int(),
            };
            TestDataHelper.InsertAddress(addressKey, Db, queryParameters);
            AddSomeRandomAddressToTheDatabase();

            var queryString = $"USRN={queryParameters.Usrn}&AddressStatus=Historical&Format=Detailed";

            var response = await CallEndpointWithQueryParameters(queryString).ConfigureAwait(true);
            response.StatusCode.Should().Be(200);

            var returnedAddress = await ConvertToResponseObject(response).ConfigureAwait(true);
            returnedAddress.Data.Addresses.Count.Should().Be(1);
            returnedAddress.Data.Addresses.First().AddressKey.Should().Be(addressKey);
        }

        [Test]
        public async Task UsingTheSimpleFlagOnlyReturnsBasicAddressInformation()
        {
            var addressKey = _faker.Random.String2(14);
            var addressDetails = new DatabaseAddressRecord
            {
                Uprn = _faker.Random.Int(),
                Line1 = _faker.Address.StreetName(),
                Line2 = _faker.Address.StreetAddress(),
                Line3 = _faker.Address.County(),
                Line4 = _faker.Address.Country(),
                Town = _faker.Address.City(),
                Postcode = "E41JJ",
            };
            TestDataHelper.InsertAddress(addressKey, Db, addressDetails);
            AddSomeRandomAddressToTheDatabase();
            var queryString = $"UPRN={addressDetails.Uprn}&AddressStatus=Historical&Format=Simple";

            var response = await CallEndpointWithQueryParameters(queryString).ConfigureAwait(true);
            response.StatusCode.Should().Be(200);

            var returnedAddress = await ConvertToResponseObject(response).ConfigureAwait(true);
            returnedAddress.Data.Addresses.Count.Should().Be(1);
            var receivedAddress = returnedAddress.Data.Addresses.First();
            receivedAddress.Should().BeEquivalentTo(new AddressResponse
            {
                Line1 = addressDetails.Line1,
                Line2 = addressDetails.Line2,
                Line3 = addressDetails.Line3,
                Line4 = addressDetails.Line4,
                Town = addressDetails.Town,
                Postcode = addressDetails.Postcode,
                UPRN = addressDetails.Uprn
            });
        }

        [Test]
        public async Task SettingGazetteerToBothWillReturnNationalAddresses()
        {
            var addressKey = _faker.Random.String2(14);
            var queryParameters = new DatabaseAddressRecord
            {
                Uprn = _faker.Random.Int(),
            };
            TestDataHelper.InsertAddress(addressKey, Db, queryParameters, localOnly: false);

            AddSomeRandomAddressToTheDatabase(count: 3);
            AddSomeRandomAddressToTheDatabase(count: 3, gazetteer: "national");
            var queryString = $"UPRN={queryParameters.Uprn}&AddressStatus=Historical&Format=Detailed&Gazetteer=Both";

            var response = await CallEndpointWithQueryParameters(queryString).ConfigureAwait(true);
            response.StatusCode.Should().Be(200);

            var returnedAddress = await ConvertToResponseObject(response).ConfigureAwait(true);
            returnedAddress.Data.Addresses.Count.Should().Be(1);
            returnedAddress.Data.Addresses.First().AddressKey.Should().Be(addressKey);
        }

        [Test]
        public async Task MustQueryTheEndpointBySomethingWhenSearchingLocalAddresses()
        {
            var addressKey = "eytshdnshsuahs";
            TestDataHelper.InsertAddress(addressKey, Db);

            var queryString = "Gazetteer=Local";

            var response = await CallEndpointWithQueryParameters(queryString).ConfigureAwait(true);
            response.StatusCode.Should().Be(400);

            var data = await ConvertToResponseObject(response).ConfigureAwait(true);
            data.Error.IsValid.Should().BeFalse();
            data.Error.ValidationErrors.Should()
                .Contain(x => x.Message == "You must provide at least one of (uprn, usrn, postcode, street, usagePrimary, usageCode), when gazeteer is 'local'.");
        }

        [Test]
        public async Task MustProvideAUprnUsrnOrPostcodeWhenSearchingNationalAddresses()
        {
            var addressKey = "eytshdnshsuahs";
            TestDataHelper.InsertAddress(addressKey, Db);

            var queryString = "Gazetteer=Both&street=hackneyroad";

            var response = await CallEndpointWithQueryParameters(queryString).ConfigureAwait(true);
            response.StatusCode.Should().Be(400);

            var data = await ConvertToResponseObject(response).ConfigureAwait(true);
            data.Error.IsValid.Should().BeFalse();
            data.Error.ValidationErrors.Should()
                .Contain(x => x.Message == "You must provide at least one of (uprn, usrn, postcode), when gazetteer is 'both'.");
        }

        [Test]
        public async Task WillReturnABadRequestForAnInvalidPostcode()
        {
            var addressKey = "eytshdnshsuahs";
            TestDataHelper.InsertAddress(addressKey, Db);

            var queryString = "Gazetteer=Local&PostCode=12376";

            var response = await CallEndpointWithQueryParameters(queryString).ConfigureAwait(true);
            response.StatusCode.Should().Be(400);

            var data = await ConvertToResponseObject(response).ConfigureAwait(true);
            data.Error.IsValid.Should().BeFalse();
            data.Error.ValidationErrors.Should()
                .Contain(x => x.Message == "Must provide at least the first part of the postcode.");
        }

        private void AddSomeRandomAddressToTheDatabase(int? count = null, string gazetteer = "local")
        {
            var number = count ?? _faker.Random.Int(2, 7);
            for (var i = 0; i < number; i++)
            {
                var addressKey = _faker.Random.String2(14);
                var randomAddress = _fixture.Create<DatabaseAddressRecord>();
                TestDataHelper.InsertAddress(addressKey, Db, randomAddress, gazetteer == "local");
            }
        }

        private async Task<HttpResponseMessage> CallEndpointWithQueryParameters(string query)
        {
            var url = new Uri($"api/v1/addresses?{query}", UriKind.Relative);
            return await Client.GetAsync(url).ConfigureAwait(true);
        }
    }
}
