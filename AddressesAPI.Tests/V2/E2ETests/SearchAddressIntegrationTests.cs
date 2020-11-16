using System;
using System.Linq;
using System.Net.Http;
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
    public class SearchAddressIntegrationTests : IntegrationTests<Startup>
    {
        private readonly Faker _faker = new Faker();
        private readonly IFixture _fixture = new Fixture();

        [Test]
        public async Task SearchAddressReturns200()
        {
            var addressKey = "eytshdnshsuahs";
            TestEfDataHelper.InsertAddress(DatabaseContext, addressKey);

            var queryString = "postcode=E8";

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

            var queryString = $"postcode={postcode}&address_status={record.AddressStatus}&format=Detailed&address_scope=national";

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

            var queryString = $"uprn={queryParameters.UPRN}&address_status={record.AddressStatus}&format=Detailed&address_scope=national";

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

            var queryString = $"USRN={queryParameters.USRN}&address_status=Historical&Format=Detailed&address_scope=national";

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
            var queryString = $"uprn={addressDetails.UPRN}&address_status=Historical&format=Simple&address_scope=national";

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
                "],\"pageCount\":1,\"totalCount\":1},\"statusCode\":200}");
        }

        [Test]
        public async Task SettingAddressScopeToNationalWillReturnAllAddresses()
        {
            var addressKey = _faker.Random.String2(14);
            var dbOptions = new NationalAddress
            {
                UPRN = _faker.Random.Int(),
                Gazetteer = "National"
            };
            var record = TestEfDataHelper.InsertAddress(DatabaseContext, addressKey, dbOptions);

            AddSomeRandomAddressToTheDatabase(count: 3);
            AddSomeRandomAddressToTheDatabase(count: 3, gazetteer: "National");
            var queryString = $"uprn={dbOptions.UPRN}&address_status={record.AddressStatus}&format=Detailed&address_scope=national";

            var response = await CallEndpointWithQueryParameters(queryString).ConfigureAwait(true);
            response.StatusCode.Should().Be(200);

            var returnedAddress = await response.ConvertToSearchAddressResponseObject().ConfigureAwait(true);
            returnedAddress.Data.Addresses.Count.Should().Be(1);
            returnedAddress.Data.Addresses.First().AddressKey.Should().Be(addressKey);
        }

        [TestCase("hackneyborough")]
        [TestCase("hackney borough")]
        [TestCase("Hackney Borough")]
        public async Task SettingAddressScopeToHackneyBoroughWillOnlyReturnAddressesInHackney(string addressScope)
        {
            var hackneyOutOfBorough = TestEfDataHelper.InsertAddress(DatabaseContext,
                request: new NationalAddress { Postcode = "N1 3JH", Gazetteer = "Hackney", NeverExport = true });

            var hackneyInBorough = TestEfDataHelper.InsertAddress(DatabaseContext,
                request: new NationalAddress { Postcode = "N1 7YH", Gazetteer = "Hackney", NeverExport = false });

            var nationalAddress = TestEfDataHelper.InsertAddress(DatabaseContext,
                request: new NationalAddress { Postcode = "N1 7UK", Gazetteer = "National" });

            var queryString = $"postcode=N1&format=Detailed&address_scope={addressScope}";
            var response = await CallEndpointWithQueryParameters(queryString).ConfigureAwait(true);
            response.StatusCode.Should().Be(200);
            var returnedAddress = await response.ConvertToSearchAddressResponseObject().ConfigureAwait(true);
            returnedAddress.Data.Addresses.Count.Should().Be(1);
            returnedAddress.Data.Addresses.First().AddressKey.Should().Be(hackneyInBorough.AddressKey);
        }

        [Test]
        public async Task CanIncludeParentShellsInARequest()
        {
            var blockAddressKey = _faker.Random.String2(14);
            var blockOfFlats = new NationalAddress
            {
                UPRN = _faker.Random.Int(1, 287987129),
            };
            TestEfDataHelper.InsertAddress(DatabaseContext, blockAddressKey, blockOfFlats);

            var flatAddressKey = _faker.Random.String2(14);
            var flat = new NationalAddress
            {
                ParentUPRN = blockOfFlats.UPRN,
            };
            var flatRecord = TestEfDataHelper.InsertAddress(DatabaseContext, flatAddressKey, flat);

            AddSomeRandomAddressToTheDatabase();

            var queryString = $"UPRN={flatRecord.UPRN}&Format=Detailed&include_parent_shells=true&address_scope=national";

            var response = await CallEndpointWithQueryParameters(queryString).ConfigureAwait(true);
            response.StatusCode.Should().Be(200);

            var returnedAddress = await response.ConvertToSearchAddressResponseObject()
                .ConfigureAwait(true);
            returnedAddress.Data.Addresses.Count.Should().Be(2);

            var returnedUprns = returnedAddress.Data.Addresses
                .Select(x => x.UPRN).ToList();
            returnedUprns.Should().Contain(blockOfFlats.UPRN);
            returnedUprns.Should().Contain(flatRecord.UPRN);
        }

        [Test]
        public async Task PassingAnIncorrectFormat()
        {
            var queryString = "street=hackneyroad&gazetteer=local&format=yes";
            var response = await CallEndpointWithQueryParameters(queryString).ConfigureAwait(true);
            response.StatusCode.Should().Be(400);
        }

        [TestCase("page_size=100", "PageSize", "PageSize cannot exceed 50")]
        [TestCase("postcode=12376", "Postcode", "Must provide at least the first part of the postcode.")]
        [TestCase("address_scope=national&street=hackneyroad", "", "You must provide at least one of (uprn, usrn, postcode), when address_scope is 'national'.")]
        [TestCase("address_scope=hackneygazetteer", "", "You must provide at least one of (uprn, usrn, postcode, street, usagePrimary, usageCode), when address_scope is 'hackney borough' or 'hackney gazetteer'.")]
        public async Task ValidationErrors(string queryString, string fieldName, string message)
        {
            var response = await CallEndpointWithQueryParameters(queryString).ConfigureAwait(true);
            response.StatusCode.Should().Be(400);
            var errors = await response.ConvertToErrorResponseObject().ConfigureAwait(true);
            errors.StatusCode.Should().Be(400);
            errors.Errors.Should().ContainEquivalentOf(
                new Error { Message = message, FieldName = fieldName }
            );
        }

        [Test]
        public async Task SearchAddressReturnsAnAddressMatchingASpecificCrossReference()
        {
            var uprnOne = _faker.Random.Int();
            var uprnTwo = _faker.Random.Int();

            var crossReferenceOne = new CrossReference { Code = "000ABC", Value = "100000" };
            var crossReferenceTwo = new CrossReference { Code = "123XYZ", Value = "100000" };

            var record = TestEfDataHelper.InsertAddress(DatabaseContext, request: new NationalAddress { UPRN = uprnOne, Postcode = "N1 7UK" });
            TestEfDataHelper.InsertCrossReference(DatabaseContext, uprnOne, crossReferenceOne);


            TestEfDataHelper.InsertAddress(DatabaseContext, request: new NationalAddress { UPRN = uprnTwo, Postcode = "N1 7UK" });
            TestEfDataHelper.InsertCrossReference(DatabaseContext, uprnTwo, crossReferenceTwo);

            AddSomeRandomAddressToTheDatabase();

            var queryString = $"cross_ref_code={crossReferenceOne.Code}&cross_ref_value={crossReferenceOne.Value}&postcode=N1&format=Detailed&address_scope=national";

            var response = await CallEndpointWithQueryParameters(queryString).ConfigureAwait(true);
            response.StatusCode.Should().Be(200);

            var returnedAddress = await response.ConvertToSearchAddressResponseObject().ConfigureAwait(true);
            returnedAddress.Data.Addresses.Count.Should().Be(1);
            returnedAddress.Data.Addresses.First().AddressKey.Should().Be(record.AddressKey);
        }

        private void AddSomeRandomAddressToTheDatabase(int? count = null, string gazetteer = "Local")
        {
            var number = count ?? _faker.Random.Int(2, 7);
            for (var i = 0; i < number; i++)
            {
                var addressKey = _faker.Random.String2(14);
                var randomAddress = _fixture.Build<NationalAddress>()
                    .With(a => a.Gazetteer, gazetteer)
                    .Without(a => a.ParentUPRN)
                    .Create();
                TestEfDataHelper.InsertAddress(DatabaseContext, addressKey, randomAddress);
            }
        }

        private async Task<HttpResponseMessage> CallEndpointWithQueryParameters(string query)
        {
            var url = new Uri($"api/v2/addresses?{query}", UriKind.Relative);
            return await Client.GetAsync(url).ConfigureAwait(true);
        }
    }
}
