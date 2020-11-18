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
    public class ElasticsearchAddressIntegrationTests : IntegrationTests<Startup>
    {
        private readonly Faker _faker = new Faker();
        private readonly IFixture _fixture = new Fixture();

        [Test]
        public async Task SearchAddressReturns200()
        {
            var addressKey = "eytshdnshsuahs";
            await TestEfDataHelper.InsertAddressInDbAndEs(DatabaseContext, ElasticsearchClient, addressKey).ConfigureAwait(true);

            var queryString = "postcode=E8";

            var response = await CallEndpointWithQueryParameters(queryString).ConfigureAwait(true);
            response.StatusCode.Should().Be(200);
        }

        [Test]
        public async Task SearchAddressReturnsAllAddresses()
        {
            var recordOne = await TestEfDataHelper.InsertAddressInDbAndEs(DatabaseContext, ElasticsearchClient, request:
                new NationalAddress { Postcode = "E8 5TH" }).ConfigureAwait(true);
            var recordTwo = await TestEfDataHelper.InsertAddressInDbAndEs(DatabaseContext, ElasticsearchClient, request:
                new NationalAddress { Postcode = "E8 6YR" }).ConfigureAwait(true);

            var queryString = "postcode=E8&format=Detailed";

            var response = await CallEndpointWithQueryParameters(queryString).ConfigureAwait(true);
            response.StatusCode.Should().Be(200);
            var returnedAddress = await response.ConvertToSearchAddressResponseObject().ConfigureAwait(true);

            returnedAddress.Data.Addresses.Count.Should().Be(2);
            returnedAddress.Data.Addresses.First().AddressKey.Should().Be(recordOne.AddressKey);
            returnedAddress.Data.Addresses.Last().AddressKey.Should().Be(recordTwo.AddressKey);
        }

        [Test]
        public async Task SearchAddressReturnsAnAddressWithMatchingPostcode()
        {
            var addressKey = _faker.Random.String2(14);
            var postcode = "E4 2JH";
            var record = await TestEfDataHelper.InsertAddressInDbAndEs(DatabaseContext, ElasticsearchClient, addressKey,
                new NationalAddress { Postcode = postcode }).ConfigureAwait(true);
            await AddSomeRandomAddressToTheDatabase().ConfigureAwait(true);

            var queryString = $"postcode={postcode}&address_status={record.AddressStatus}&format=Detailed";

            var response = await CallEndpointWithQueryParameters(queryString).ConfigureAwait(true);
            response.StatusCode.Should().Be(200);
            var returnedAddress = await response.ConvertToSearchAddressResponseObject().ConfigureAwait(true);
            returnedAddress.Data.Addresses.Count.Should().Be(1);
            returnedAddress.Data.Addresses.First().AddressKey.Should().Be(addressKey);
        }
        //
        // [Ignore("Pending")]
        // [Test]
        // public async Task CanSearchAddressesByUprn()
        // {
        //     var addressKey = _faker.Random.String2(14);
        //     var queryParameters = new NationalAddress
        //     {
        //         UPRN = _faker.Random.Int(),
        //     };
        //     var record = await TestEfDataHelper.InsertAddressToPgAndEs(DatabaseContext, ElasticsearchClient, addressKey, queryParameters);
        //     AddSomeRandomAddressToTheDatabase();
        //
        //     var queryString = $"uprn={queryParameters.UPRN}&address_status={record.AddressStatus}&format=Detailed";
        //
        //     var response = await CallEndpointWithQueryParameters(queryString).ConfigureAwait(true);
        //     response.StatusCode.Should().Be(200);
        //
        //     var returnedAddress = await response.ConvertToSearchAddressResponseObject().ConfigureAwait(true);
        //     returnedAddress.Data.Addresses.Count.Should().Be(1);
        //     returnedAddress.Data.Addresses.First().AddressKey.Should().Be(addressKey);
        // }
        //
        // [Ignore("Pending")]
        // [Test]
        // public async Task CanSearchAddressesByUsrn()
        // {
        //     var addressKey = _faker.Random.String2(14);
        //     var queryParameters = new NationalAddress
        //     {
        //         USRN = _faker.Random.Int(),
        //         AddressStatus = "Historical"
        //     };
        //     await TestEfDataHelper.InsertAddressToPgAndEs(DatabaseContext, ElasticsearchClient, addressKey, queryParameters);
        //     AddSomeRandomAddressToTheDatabase();
        //
        //     var queryString = $"USRN={queryParameters.USRN}&address_status=Historical&Format=Detailed";
        //
        //     var response = await CallEndpointWithQueryParameters(queryString).ConfigureAwait(true);
        //     response.StatusCode.Should().Be(200);
        //
        //     var returnedAddress = await response.ConvertToSearchAddressResponseObject().ConfigureAwait(true);
        //     returnedAddress.Data.Addresses.Count.Should().Be(1);
        //     returnedAddress.Data.Addresses.First().AddressKey.Should().Be(addressKey);
        // }
        //
        // [Ignore("Pending")]
        // [Test]
        // public async Task UsingTheSimpleFlagOnlyReturnsBasicAddressInformation()
        // {
        //     var addressKey = _faker.Random.String2(14);
        //     var addressDetails = new NationalAddress
        //     {
        //         UPRN = _faker.Random.Int(),
        //         Line1 = _faker.Address.StreetName(),
        //         Line2 = _faker.Address.StreetAddress(),
        //         Line3 = _faker.Address.County(),
        //         Line4 = _faker.Address.Country(),
        //         Town = _faker.Address.City(),
        //         Postcode = "E41JJ",
        //         AddressStatus = "Historical"
        //     };
        //     await TestEfDataHelper.InsertAddressToPgAndEs(DatabaseContext, ElasticsearchClient, addressKey, addressDetails);
        //     AddSomeRandomAddressToTheDatabase();
        //     var queryString = $"uprn={addressDetails.UPRN}&address_status=Historical&format=Simple";
        //
        //     var response = await CallEndpointWithQueryParameters(queryString).ConfigureAwait(true);
        //     response.StatusCode.Should().Be(200);
        //
        //     var data = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
        //     data.Should().BeEquivalentTo(
        //         "{\"data\":" +
        //         "{\"address\":[" +
        //             $"{{\"line1\":\"{addressDetails.Line1}\",\"line2\":\"{addressDetails.Line2}\"," +
        //             $"\"line3\":\"{addressDetails.Line3}\",\"line4\":\"{addressDetails.Line4}\"," +
        //             $"\"town\":\"{addressDetails.Town}\",\"postcode\":\"{addressDetails.Postcode}\"," +
        //             $"\"UPRN\":{addressDetails.UPRN}}}" +
        //         "],\"pageCount\":1,\"totalCount\":1},\"statusCode\":200}");
        // }
        //
        // [Ignore("Pending")]
        // [Test]
        // public async Task SettingGazetteerToBothWillReturnNationalAddresses()
        // {
        //     var addressKey = _faker.Random.String2(14);
        //     var queryParameters = new NationalAddress
        //     {
        //         UPRN = _faker.Random.Int(),
        //         Gazetteer = "National"
        //     };
        //     var record = await TestEfDataHelper.InsertAddressToPgAndEs(DatabaseContext, ElasticsearchClient, addressKey, queryParameters);
        //
        //     AddSomeRandomAddressToTheDatabase(count: 3);
        //     AddSomeRandomAddressToTheDatabase(count: 3, gazetteer: "National");
        //     var queryString = $"uprn={queryParameters.UPRN}&address_status={record.AddressStatus}&format=Detailed&gazetteer=Both";
        //
        //     var response = await CallEndpointWithQueryParameters(queryString).ConfigureAwait(true);
        //     response.StatusCode.Should().Be(200);
        //
        //     var returnedAddress = await response.ConvertToSearchAddressResponseObject().ConfigureAwait(true);
        //     returnedAddress.Data.Addresses.Count.Should().Be(1);
        //     returnedAddress.Data.Addresses.First().AddressKey.Should().Be(addressKey);
        // }
        //
        // [Ignore("Pending")]
        // [Test]
        // public async Task CanQueryForNoOutOfBoroughAddresses()
        // {
        //     var hackneyOutOfBorough = await TestEfDataHelper.InsertAddressToPgAndEs(DatabaseContext, ElasticsearchClient,
        //         request: new NationalAddress { Postcode = "N1 3JH", Gazetteer = "Hackney", NeverExport = true });
        //
        //     var hackneyInBorough = await TestEfDataHelper.InsertAddressToPgAndEs(DatabaseContext, ElasticsearchClient,
        //         request: new NationalAddress { Postcode = "N1 7YH", Gazetteer = "Hackney", NeverExport = false });
        //
        //     var nationalAddress = await TestEfDataHelper.InsertAddressToPgAndEs(DatabaseContext, ElasticsearchClient,
        //         request: new NationalAddress { Postcode = "N1 7UK", Gazetteer = "National" });
        //
        //     var queryString = "postcode=N1&format=Detailed&gazetteer=Both&out_of_borough=false";
        //     var response = await CallEndpointWithQueryParameters(queryString).ConfigureAwait(true);
        //     response.StatusCode.Should().Be(200);
        //     var returnedAddress = await response.ConvertToSearchAddressResponseObject().ConfigureAwait(true);
        //     returnedAddress.Data.Addresses.Count.Should().Be(1);
        //     returnedAddress.Data.Addresses.First().AddressKey.Should().Be(hackneyInBorough.AddressKey);
        // }
        //
        // [Ignore("Pending")]
        // [Test]
        // public async Task CanIncludeParentShellsInARequest()
        // {
        //     var blockAddressKey = _faker.Random.String2(14);
        //     var blockOfFlats = new NationalAddress
        //     {
        //         UPRN = _faker.Random.Int(1, 287987129),
        //     };
        //     await TestEfDataHelper.InsertAddressToPgAndEs(DatabaseContext, ElasticsearchClient, blockAddressKey, blockOfFlats).ConfigureAwait(true);
        //
        //     var flatAddressKey = _faker.Random.String2(14);
        //     var flat = new NationalAddress
        //     {
        //         ParentUPRN = blockOfFlats.UPRN,
        //     };
        //     var flatRecord = await TestEfDataHelper.InsertAddressToPgAndEs(DatabaseContext, ElasticsearchClient, flatAddressKey, flat).ConfigureAwait(true);
        //
        //     await AddSomeRandomAddressToTheDatabase().ConfigureAwait(true);
        //
        //     var queryString = $"UPRN={flatRecord.UPRN}&Format=Detailed&include_parent_shells=true";
        //
        //     var response = await CallEndpointWithQueryParameters(queryString).ConfigureAwait(true);
        //     response.StatusCode.Should().Be(200);
        //
        //     var returnedAddress = await response.ConvertToSearchAddressResponseObject()
        //         .ConfigureAwait(true);
        //     returnedAddress.Data.Addresses.Count.Should().Be(2);
        //
        //     var returnedUprns = returnedAddress.Data.Addresses
        //         .Select(x => x.UPRN).ToList();
        //     returnedUprns.Should().Contain(blockOfFlats.UPRN);
        //     returnedUprns.Should().Contain(flatRecord.UPRN);
        // }
        //
        // [Ignore("Pending")]
        // [Test]
        // public async Task PassingAnIncorrectFormat()
        // {
        //     var queryString = "street=hackneyroad&gazetteer=local&format=yes";
        //     var response = await CallEndpointWithQueryParameters(queryString).ConfigureAwait(true);
        //     response.StatusCode.Should().Be(400);
        // }
        //
        // [Ignore("Pending")]
        // [TestCase("page_size=100", "PageSize", "PageSize cannot exceed 50")]
        // [TestCase("postcode=12376", "Postcode", "Must provide at least the first part of the postcode.")]
        // [TestCase("Gazetteer=Both&street=hackneyroad", "", "You must provide at least one of (uprn, usrn, postcode), when gazetteer is 'both'.")]
        // [TestCase("Gazetteer=Local", "", "You must provide at least one of (uprn, usrn, postcode, street, usagePrimary, usageCode), when gazeteer is 'local'.")]
        // public async Task ValidationErrors(string queryString, string fieldName, string message)
        // {
        //     var response = await CallEndpointWithQueryParameters(queryString).ConfigureAwait(true);
        //     response.StatusCode.Should().Be(400);
        //     var errors = await response.ConvertToErrorResponseObject().ConfigureAwait(true);
        //     errors.StatusCode.Should().Be(400);
        //     errors.Errors.Should().ContainEquivalentOf(
        //         new Error { Message = message, FieldName = fieldName }
        //     );
        // }

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
                await TestEfDataHelper.InsertAddressInDbAndEs(DatabaseContext, ElasticsearchClient, addressKey, randomAddress).ConfigureAwait(true);
            }
        }

        private async Task<HttpResponseMessage> CallEndpointWithQueryParameters(string query)
        {
            var url = new Uri($"api/v2/addresses/elasticsearch?{query}", UriKind.Relative);
            return await Client.GetAsync(url).ConfigureAwait(true);
        }
    }
}
