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
            await TestDataHelper.InsertAddressInDbAndEs(DatabaseContext, ElasticsearchClient, addressKey).ConfigureAwait(true);

            var queryString = "postcode=E8";

            var response = await CallEndpointWithQueryParameters(queryString).ConfigureAwait(true);
            response.StatusCode.Should().Be(200);
        }

        [Test]
        public async Task SearchAddressReturnsAnAddressWithMatchingPostcode()
        {
            var addressKey = _faker.Random.String2(14);
            var postcode = "E4 2JH";
            var record = await TestDataHelper.InsertAddressInDbAndEs(DatabaseContext, ElasticsearchClient, addressKey,
                new NationalAddress { Postcode = postcode }).ConfigureAwait(true);
            await AddSomeRandomAddressToTheDatabase().ConfigureAwait(true);

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
            var record = await TestDataHelper.InsertAddressInDbAndEs(DatabaseContext, ElasticsearchClient, addressKey, queryParameters)
                .ConfigureAwait(true);
            await AddSomeRandomAddressToTheDatabase().ConfigureAwait(true);

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
            await TestDataHelper.InsertAddressInDbAndEs(DatabaseContext, ElasticsearchClient, addressKey, queryParameters)
                .ConfigureAwait(true);
            await AddSomeRandomAddressToTheDatabase().ConfigureAwait(true);

            var queryString = $"USRN={queryParameters.USRN}&address_status=Historical&Format=Detailed&address_scope=national";

            var response = await CallEndpointWithQueryParameters(queryString).ConfigureAwait(true);
            response.StatusCode.Should().Be(200);

            var returnedAddress = await response.ConvertToSearchAddressResponseObject().ConfigureAwait(true);
            returnedAddress.Data.Addresses.Count.Should().Be(1);
            returnedAddress.Data.Addresses.First().AddressKey.Should().Be(addressKey);
        }

        [Test]
        public async Task CanSearchAddressesThatHaveBeenModifiedSinceADate()
        {
            var randomDate = _faker.Date.Past(3);

            var addressModifiedAfterOne = await AddAddressModifiedOnDate("E8", Convert.ToInt32(randomDate.AddDays(32).ToString("yyyyMMdd")))
                .ConfigureAwait(true);
            var addressModifiedAfterTwo = await AddAddressModifiedOnDate("E8", Convert.ToInt32(randomDate.AddDays(1).ToString("yyyyMMdd")))
                    .ConfigureAwait(true);
            var addressModifiedBeforeOne = await AddAddressModifiedOnDate("E8", Convert.ToInt32(randomDate.AddDays(-106).ToString("yyyyMMdd")))
                .ConfigureAwait(true);
            var addressModifiedBeforeTwo = await AddAddressModifiedOnDate("E8", Convert.ToInt32(randomDate.AddDays(-50).ToString("yyyyMMdd")))
                .ConfigureAwait(true);

            var queryString = $"postcode=E8&modified_since={randomDate:yyyy-MM-dd}&Format=Detailed&address_scope=national";

            var response = await CallEndpointWithQueryParameters(queryString).ConfigureAwait(true);
            response.StatusCode.Should().Be(200);

            var returnedAddress = await response.ConvertToSearchAddressResponseObject().ConfigureAwait(true);
            returnedAddress.Data.Addresses.Count.Should().Be(2);
            var returnedKeys = returnedAddress.Data.Addresses.Select(a => a.AddressKey).ToList();
            returnedKeys.Should().Contain(addressModifiedAfterOne.AddressKey);
            returnedKeys.Should().Contain(addressModifiedAfterTwo.AddressKey);
        }

        [TestCase("green street")]
        [TestCase("green str")]
        [TestCase("green road")]
        [TestCase("green")]
        [TestCase("290 green street")]
        [TestCase("290C green street")]
        public async Task CanQueryTheAddressText(string query)
        {
            var addressKey = _faker.Random.String2(14);
            var addressLines = new[] { "Flat C", "290 Green Street", "Hackney", "London" };

            var queryParameters = new NationalAddress
            {
                Line1 = addressLines.ElementAt(0),
                Line2 = addressLines.ElementAt(1),
                Line3 = addressLines.ElementAt(2),
                Line4 = addressLines.ElementAt(3),
                Gazetteer = "hackney"
            };
            await TestDataHelper.InsertAddressInDbAndEs(DatabaseContext, ElasticsearchClient, addressKey, queryParameters)
                .ConfigureAwait(true);
            await AddSomeRandomAddressToTheDatabase().ConfigureAwait(true);

            var queryString = $"query={query}&format=detailed";

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
            await TestDataHelper.InsertAddressInDbAndEs(DatabaseContext, ElasticsearchClient, addressKey, addressDetails)
                .ConfigureAwait(true);
            await AddSomeRandomAddressToTheDatabase().ConfigureAwait(true);
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
            var record = await TestDataHelper.InsertAddressInDbAndEs(DatabaseContext, ElasticsearchClient, addressKey, dbOptions)
                .ConfigureAwait(true);

            await AddSomeRandomAddressToTheDatabase(count: 3).ConfigureAwait(true);
            await AddSomeRandomAddressToTheDatabase(count: 3, gazetteer: "National").ConfigureAwait(true);
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
            var hackneyOutOfBorough = await TestDataHelper.InsertAddressInDbAndEs(DatabaseContext, ElasticsearchClient,
                request: new NationalAddress { Postcode = "N1 3JH", Gazetteer = "Hackney", NeverExport = true }).ConfigureAwait(true);

            var hackneyInBorough = await TestDataHelper.InsertAddressInDbAndEs(DatabaseContext, ElasticsearchClient,
                request: new NationalAddress { Postcode = "N1 7YH", Gazetteer = "Hackney", NeverExport = false }).ConfigureAwait(true);

            var nationalAddress = await TestDataHelper.InsertAddressInDbAndEs(DatabaseContext, ElasticsearchClient,
                request: new NationalAddress { Postcode = "N1 7UK", Gazetteer = "National" }).ConfigureAwait(true);

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
            await TestDataHelper.InsertAddressInDbAndEs(DatabaseContext, ElasticsearchClient, blockAddressKey, blockOfFlats)
                .ConfigureAwait(true);

            var flatAddressKey = _faker.Random.String2(14);
            var flat = new NationalAddress
            {
                ParentUPRN = blockOfFlats.UPRN,
            };
            var flatRecord = await TestDataHelper.InsertAddressInDbAndEs(DatabaseContext, ElasticsearchClient, flatAddressKey, flat)
                .ConfigureAwait(true);

            await AddSomeRandomAddressToTheDatabase().ConfigureAwait(true);

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
        [TestCase("address_scope=national&street=hackneyroad", "", "You must provide at least one of (query, uprn, usrn, postcode), when address_scope is 'national'.")]
        [TestCase("address_scope=hackneygazetteer", "", "You must provide at least one of (query, uprn, usrn, postcode, street, usagePrimary, usageCode), when address_scope is 'hackney borough' or 'hackney gazetteer'.")]
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
        public async Task SearchAddressReturnsAddressesInHackneyBoroughForAGivenCrossReference()
        {
            var uprnOne = _faker.Random.Int();
            var uprnTwo = _faker.Random.Int();

            var crossReferenceOne = new CrossReference { Code = "000ABC", Value = "100000" };
            var crossReferenceTwo = new CrossReference { Code = "123XYZ", Value = "100000" };

            var hackneyBoroughOne = new NationalAddress { UPRN = uprnOne, Gazetteer = "Hackney", NeverExport = false };
            var hackneyBoroughTwo = new NationalAddress { UPRN = uprnTwo, Gazetteer = "Hackney", NeverExport = false };

            var record = await TestDataHelper.InsertAddressInDbAndEs(DatabaseContext, ElasticsearchClient,
                request: hackneyBoroughOne).ConfigureAwait(true);

            TestDataHelper.InsertCrossReference(DatabaseContext, uprnOne, crossReferenceOne);

            await TestDataHelper.InsertAddressInDbAndEs(DatabaseContext, ElasticsearchClient, request: hackneyBoroughTwo).ConfigureAwait(true);

            TestDataHelper.InsertCrossReference(DatabaseContext, uprnTwo, crossReferenceTwo);

            await AddSomeRandomAddressToTheDatabase().ConfigureAwait(true);

            var queryString = $"cross_ref_code={crossReferenceOne.Code}&cross_ref_value={crossReferenceOne.Value}&format=Detailed";

            var response = await CallEndpointWithQueryParameters(queryString).ConfigureAwait(true);
            response.StatusCode.Should().Be(200);

            var returnedAddress = await response.ConvertToSearchAddressResponseObject().ConfigureAwait(true);
            returnedAddress.Data.Addresses.Count.Should().Be(1);
            returnedAddress.Data.Addresses.First().AddressKey.Should().Be(record.AddressKey);
        }

        private async Task AddSomeRandomAddressToTheDatabase(int? count = null, string gazetteer = "Local")
        {
            var number = count ?? _faker.Random.Int(2, 7);
            for (var i = 0; i < number; i++)
            {
                var addressKey = _faker.Random.String2(14);
                var randomAddress = _fixture.Build<NationalAddress>()
                    .With(a => a.Gazetteer, gazetteer)
                    .Without(a => a.ParentUPRN)
                    .Create();
                await TestDataHelper.InsertAddressInDbAndEs(DatabaseContext, ElasticsearchClient, addressKey, randomAddress)
                    .ConfigureAwait(true);
            }
        }

        private async Task<NationalAddress> AddAddressModifiedOnDate(string postcode, int date)
        {
            return await TestDataHelper.InsertAddressInDbAndEs(DatabaseContext, ElasticsearchClient, request: new NationalAddress
            {
                PropertyChangeDate = date,
                Postcode = postcode
            })
                .ConfigureAwait(true);
        }

        private async Task<HttpResponseMessage> CallEndpointWithQueryParameters(string query)
        {
            var url = new Uri($"api/v2/addresses?{query}", UriKind.Relative);
            return await Client.GetAsync(url).ConfigureAwait(true);
        }
    }
}
