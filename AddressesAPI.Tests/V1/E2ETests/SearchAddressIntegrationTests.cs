using AddressesAPI.Infrastructure;
using AddressesAPI.Tests.V1.Helper;
using AddressesAPI.V1.Boundary.Responses.Metadata;
using AutoFixture;
using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

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
            TestEfDataHelper.InsertAddress(DatabaseContext, addressKey);

            var queryString = "PostCode=E8";

            var response = await CallEndpointWithQueryParameters(queryString).ConfigureAwait(true);
            response.StatusCode.Should().Be(200);
        }

        [Test]
        public async Task SearchAddressReturnsAnAddressWithMatchingPostcode()
        {
            var addressKey = _faker.Random.String2(14);
            var postcode = "E4 2JH";
            var record = TestEfDataHelper.InsertAddress(DatabaseContext, addressKey,
                new NationalAddress { Postcode = postcode });
            AddSomeRandomAddressToTheDatabase();

            var queryString = $"PostCode={postcode}&AddressStatus={record.AddressStatus}&Format=Detailed";

            var response = await CallEndpointWithQueryParameters(queryString).ConfigureAwait(true);
            response.StatusCode.Should().Be(200);
            var returnedAddress = await response.ConvertToSearchAddressResponseObject().ConfigureAwait(true);
            returnedAddress.Data.Addresses.Count.Should().Be(1);
            returnedAddress.Data.Addresses.First().AddressKey.Should().Be(addressKey);
        }

        [Test]
        public async Task CanSearchAddressesByUprn()
        {
            var addressKey = _faker.Random.String2(14);
            var queryParameters = new NationalAddress
            {
                UPRN = _faker.Random.Int(),
            };
            var record = TestEfDataHelper.InsertAddress(DatabaseContext, addressKey, queryParameters);
            AddSomeRandomAddressToTheDatabase();

            var queryString = $"UPRN={queryParameters.UPRN}&AddressStatus={record.AddressStatus}&Format=Detailed";

            var response = await CallEndpointWithQueryParameters(queryString).ConfigureAwait(true);
            response.StatusCode.Should().Be(200);

            var returnedAddress = await response.ConvertToSearchAddressResponseObject().ConfigureAwait(true);
            returnedAddress.Data.Addresses.Count.Should().Be(1);
            returnedAddress.Data.Addresses.First().AddressKey.Should().Be(addressKey);
        }

        [Test]
        public async Task CanSearchAddressesByUsrn()
        {
            var addressKey = _faker.Random.String2(14);
            var queryParameters = new NationalAddress
            {
                USRN = _faker.Random.Int(),
                AddressStatus = "Historical"
            };
            TestEfDataHelper.InsertAddress(DatabaseContext, addressKey, queryParameters);
            AddSomeRandomAddressToTheDatabase();

            var queryString = $"USRN={queryParameters.USRN}&AddressStatus=Historical&Format=Detailed";

            var response = await CallEndpointWithQueryParameters(queryString).ConfigureAwait(true);
            response.StatusCode.Should().Be(200);

            var returnedAddress = await response.ConvertToSearchAddressResponseObject().ConfigureAwait(true);
            returnedAddress.Data.Addresses.Count.Should().Be(1);
            returnedAddress.Data.Addresses.First().AddressKey.Should().Be(addressKey);
        }

        [Test]
        public async Task UsingTheSimpleFlagOnlyReturnsBasicAddressInformation()
        {
            var addressKey = _faker.Random.String2(14);
            var addressDetails = new NationalAddress
            {
                UPRN = _faker.Random.Int(),
                Line1 = _faker.Address.StreetName(),
                Line2 = _faker.Address.StreetAddress(),
                Line3 = _faker.Address.County(),
                Line4 = _faker.Address.Country(),
                Town = _faker.Address.City(),
                Postcode = "E41JJ",
                AddressStatus = "Historical"
            };
            TestEfDataHelper.InsertAddress(DatabaseContext, addressKey, addressDetails);
            AddSomeRandomAddressToTheDatabase();
            var queryString = $"UPRN={addressDetails.UPRN}&AddressStatus=Historical&Format=Simple";

            var response = await CallEndpointWithQueryParameters(queryString).ConfigureAwait(true);
            response.StatusCode.Should().Be(200);

            var data = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            data.Should().BeEquivalentTo(
                "{\"data\":" +
                "{\"address\":[" +
                    $"{{\"line1\":\"{addressDetails.Line1}\",\"line2\":\"{addressDetails.Line2}\"," +
                    $"\"line3\":\"{addressDetails.Line3}\",\"line4\":\"{addressDetails.Line4}\"," +
                    $"\"town\":\"{addressDetails.Town}\",\"postcode\":\"{addressDetails.Postcode}\"," +
                    $"\"UPRN\":{addressDetails.UPRN}}}" +
                "],\"page_count\":1,\"total_count\":1},\"statusCode\":200}");
        }

        [Test]
        public async Task SettingGazetteerToBothWillReturnNationalAddresses()
        {
            var addressKey = _faker.Random.String2(14);
            var queryParameters = new NationalAddress
            {
                UPRN = _faker.Random.Int(),
                Gazetteer = "National"
            };
            var record = TestEfDataHelper.InsertAddress(DatabaseContext, addressKey, queryParameters);

            AddSomeRandomAddressToTheDatabase(count: 3);
            AddSomeRandomAddressToTheDatabase(count: 3, gazetteer: "National");
            var queryString = $"UPRN={queryParameters.UPRN}&AddressStatus={record.AddressStatus}&Format=Detailed&Gazetteer=Both";

            var response = await CallEndpointWithQueryParameters(queryString).ConfigureAwait(true);
            response.StatusCode.Should().Be(200);

            var returnedAddress = await response.ConvertToSearchAddressResponseObject().ConfigureAwait(true);
            returnedAddress.Data.Addresses.Count.Should().Be(1);
            returnedAddress.Data.Addresses.First().AddressKey.Should().Be(addressKey);
        }

        [Test]
        public async Task PassingAnIncorrectFormat()
        {
            var queryString = "street=hackneyroad&gazetteer=local&Format=yes";
            var response = await CallEndpointWithQueryParameters(queryString).ConfigureAwait(true);
            response.StatusCode.Should().Be(400);
        }

        [TestCase("PageSize=100", "PageSize", "PageSize cannot exceed 50")]
        [TestCase("PostCode=12376", "PostCode", "Must provide at least the first part of the postcode.")]
        [TestCase("Gazetteer=Both&street=hackneyroad", "", "You must provide at least one of (uprn, usrn, postcode), when gazetteer is 'both'.")]
        [TestCase("Gazetteer=Local", "", "You must provide at least one of (uprn, usrn, postcode, street, usagePrimary, usageCode), when gazeteer is 'local'.")]
        public async Task ValidationErrors(string queryString, string fieldName, string message)
        {
            var response = await CallEndpointWithQueryParameters(queryString).ConfigureAwait(true);
            response.StatusCode.Should().Be(400);
            var errors = await response.ConvertToErrorResponseObject().ConfigureAwait(true);
            errors.Error.IsValid.Should().BeFalse();
            errors.Error.ValidationErrors.Should().ContainEquivalentOf(
                new ValidationError { Message = message, FieldName = fieldName }
            );
        }

        [Test]
        public async Task SettingStructureToHierarchyReturnsResultsInUPRNHierarchyFormat()
        {
            var parentUPRN = 11111111;
            var postcode = "A1 2B";

            var parentAddress = new NationalAddress
            {
                UPRN = parentUPRN,
                Postcode = postcode,
            };

            var childAddress = new NationalAddress
            {
                ParentUPRN = parentUPRN,
                Postcode = postcode,
                UPRN = 22222222
            };

            TestEfDataHelper.InsertAddress(DatabaseContext, request: parentAddress);
            TestEfDataHelper.InsertAddress(DatabaseContext, request: childAddress);

            var querystring = $"Postcode={postcode}&Structure=Hierarchy";

            var response = await CallEndpointWithQueryParameters(querystring).ConfigureAwait(true);

            response.StatusCode.Should().Be(StatusCodes.Status200OK);

            var returnedAddresses = await response.ConvertToSearchAddressResponseObject().ConfigureAwait(true);

            returnedAddresses.Data.Addresses.Count.Should().Be(1);
            returnedAddresses.Data.TotalCount.Should().Be(2);
            returnedAddresses.Data.Addresses.First().UPRN.Should().Be(parentUPRN);
            returnedAddresses.Data.Addresses.First().ChildAddresses.Count.Should().Be(1);
            returnedAddresses.Data.Addresses.First().ChildAddresses.First().UPRN.Should().Be(childAddress.UPRN);
        }

        [TestCase("")]
        [TestCase("Flat")]
        public async Task SettingStructureToFlatOrNotProvidingItReturnsResultsInFlatFormat(string structure)
        {
            var parentUPRN = 11111111;
            var postcode = "A1 2B";

            var parentAddress = new NationalAddress
            {
                UPRN = parentUPRN,
                Postcode = postcode,
            };

            var childAddress = new NationalAddress
            {
                ParentUPRN = parentUPRN,
                Postcode = postcode,
                UPRN = 22222222,
            };

            TestEfDataHelper.InsertAddress(DatabaseContext, request: parentAddress);
            TestEfDataHelper.InsertAddress(DatabaseContext, request: childAddress);

            var structureParam = string.IsNullOrEmpty(structure) ? "" : $"Structure={structure}";

            var querystring = $"Postcode={postcode}&{structureParam}";

            var response = await CallEndpointWithQueryParameters(querystring).ConfigureAwait(true);

            response.StatusCode.Should().Be(StatusCodes.Status200OK);

            var returnedAddresses = await response.ConvertToSearchAddressResponseObject().ConfigureAwait(true);

            returnedAddresses.Data.Addresses.Count.Should().Be(2);
            returnedAddresses.Data.TotalCount.Should().Be(2);
            returnedAddresses.Data.Addresses.All(x => x.ChildAddresses == null);
        }

        private void AddSomeRandomAddressToTheDatabase(int? count = null, string gazetteer = "Local")
        {
            var number = count ?? _faker.Random.Int(2, 7);
            for (var i = 0; i < number; i++)
            {
                var addressKey = _faker.Random.String2(14);
                var randomAddress = _fixture.Build<NationalAddress>()
                    .With(a => a.Gazetteer, gazetteer).Create();
                TestEfDataHelper.InsertAddress(DatabaseContext, addressKey, randomAddress);
            }
        }

        private async Task<HttpResponseMessage> CallEndpointWithQueryParameters(string query)
        {
            var url = new Uri($"api/v1/addresses?{query}", UriKind.Relative);
            return await Client.GetAsync(url).ConfigureAwait(true);
        }
    }
}
