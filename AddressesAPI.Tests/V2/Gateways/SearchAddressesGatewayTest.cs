using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AddressesAPI.Infrastructure;
using AddressesAPI.Tests.V2.Helper;
using AddressesAPI.V2;
using AddressesAPI.V2.Domain;
using AddressesAPI.V2.Gateways;
using Bogus;
using FluentAssertions;
using Nest;
using NUnit.Framework;

namespace AddressesAPI.Tests.V2.Gateways
{
    public class SearchAddressesGatewayTest : ElasticsearchTests
    {
        private ElasticGateway _classUnderTest { get; set; }
        private readonly Faker _faker = new Faker();

        [SetUp]
        public void SetUp()
        {
            _classUnderTest = new ElasticGateway(ElasticsearchClient);
        }

        #region SearchAddress

        [Test]
        public async Task ItWillGetAnAddressFromTheDatabase()
        {
            var addressKey = _faker.Random.String2(14);
            await TestDataHelper.InsertAddressInEs(ElasticsearchClient, addressKey).ConfigureAwait(true);
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both
            };
            var (addresses, _) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(1);
            addresses.First().Should().Be(addressKey);
        }

        #region querying

        [TestCase("E8 4TT", "E84TT")]
        [TestCase("E1 4JH", "E1  4JH")]
        [TestCase("E8 4TT", "e8 4tt")]
        [TestCase("E7 4TT", "e7")]
        [TestCase("E7 4TT", "e7 4")]
        public async Task WillSearchPostcodeForAMatch(string savedPostcode, string postcodeSearch)
        {
            var savedAddress = await TestDataHelper.InsertAddressInEs(ElasticsearchClient,
                addressConfig: new QueryableAddress { Postcode = savedPostcode }
                ).ConfigureAwait(true);

            var randomPostcode = $"NW{_faker.Random.Int(1, 9)} {_faker.Random.Int(1, 9)}TY";
            await TestDataHelper.InsertAddressInEs(ElasticsearchClient, addressConfig: new QueryableAddress
            {
                Postcode = randomPostcode
            }).ConfigureAwait(true);
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                Postcode = postcodeSearch
            };
            var (addresses, _) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(1);
            addresses.First().Should().BeEquivalentTo(savedAddress.AddressKey);
        }

        [TestCase("LE1 3TT", "E1")]
        [TestCase("SW1 7YU", "W1 7")]
        [TestCase("E7 4TT", "e8")]
        [TestCase("E7 4TT", "e7 5tt")]
        public async Task PostcodeThatShouldNotMatch(string savedPostcode, string postcodeSearch)
        {
            await TestDataHelper.InsertAddressInEs(ElasticsearchClient,
                addressConfig: new QueryableAddress { Postcode = savedPostcode }
            ).ConfigureAwait(true);
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                Postcode = postcodeSearch
            };
            var (addresses, _) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(0);
        }

        [TestCase("123", "123")]
        [TestCase("123383833", "383")]
        public async Task WillSearchBuildingsNumbersForAMatch(string savedBuildingNumber, string buildingNumberSearch)
        {
            var savedAddress = await TestDataHelper.InsertAddressInEs(ElasticsearchClient,
                addressConfig: new QueryableAddress { BuildingNumber = savedBuildingNumber }
            ).ConfigureAwait(true);
            await TestDataHelper.InsertAddressInEs(ElasticsearchClient).ConfigureAwait(true);
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                BuildingNumber = buildingNumberSearch
            };
            var (addresses, _) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(1);
            addresses.First().Should().BeEquivalentTo(savedAddress.AddressKey);
        }

        [TestCase("hackney road", "hackney road")]
        [TestCase("green Road", "green road")]
        [TestCase("yellow street", "YELLOW STREET")]
        [TestCase("yellow street", "YELLOW")]
        [TestCase("yellow street", "YELLOWstreet")]
        public async Task WillSearchStreetForAMatch(string savedStreet, string streetSearch)
        {
            var savedAddress = await TestDataHelper.InsertAddressInEs(ElasticsearchClient,
                addressConfig: new QueryableAddress { Street = savedStreet }
            ).ConfigureAwait(true);
            await TestDataHelper.InsertAddressInEs(ElasticsearchClient).ConfigureAwait(true);
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                Street = streetSearch
            };
            var (addresses, _) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(1);
            addresses.First().Should().BeEquivalentTo(savedAddress.AddressKey);
        }

        //Including the town and postcode fields
        [TestCase("7 Eton House", "Leigh Road", "Islington", null, "London", "N5 1SR", "Lonon")]  //Town
        [TestCase("7 Eton House", "Leigh Road", "Islington", null, "London", "N5 1SR", "N5 1SR")]  //Postcode
        [TestCase("7 Eton House", "Leigh Road", "Islington", null, "London", "N5 1SR", "N5")]  //Partial
        [TestCase("7 Eton House", "Leigh Road", "Islington", null, "London", "N5 1SR", "7 eton road, London")]  //Line1 + Town
        [TestCase("16 Holly Court", "Surrograte Street", null, null, "Attleborough", "NR17 2AW", "16 Holly Court, Surrograte Street, Attleborough")] //Line1 + Line2 + Town
        [TestCase("7 Eton House", "Leigh Road", "Islington", null, "London", "N5 1SR", "7 eton road, N5 1SR")] //Line1 + Postcode
        [TestCase("7 Eton House", "Leigh Road", "Islington", null, "London", "N5 1SR", "7 eton road, N5")] //Line1 + Postcode
        [TestCase("16 Holly Court", "Surrograte Street", null, null, "Attleborough", "NR17 2AW", "Surrograte Street, NR17")] //Line2 + Partial Postcode
        [TestCase("7 Eton House", "Leigh Road", "Islington", null, "London", "N5 1SR", "London, N5 1SR")] //Town + Postcode
        [TestCase("16 Holly Court", "Surrograte Street", null, null, "Attleborough", "NR17 2AW", "Attleborough, NR17 2AW")]//Town + Postcode
        [TestCase("16 Holly Court", "Surrograte Street", null, null, "Attleborough", "NR17 2AW", "Attleborough, NR17")]//Town + Partial Postcode
        [TestCase("16 Holly Court", "Surrograte Street", null, null, "Attleborough", "NR17 2AW", "Attleborough, 2AW")]//Town + Partial Postcode
        [TestCase("16 Holly Court", "Surrograte Street", null, null, "Attleborough", "NR17 2AW", "Attleborough, NR172AW")]//Town + Postcode (No spaces)
        [TestCase("16 Holly Court", "Surrograte Street", null, null, "Attleborough", "NR17 2AW", "Attleborough, NR1")]//Town + Partial Postcode
        [TestCase("16 Holly Court", "Surrograte Street", null, null, "Attleborough", "NR17 2AW", "Attleborough, NR17 2")]//Town + Partial Postcode
        public async Task WillSearchTownAndOrPostcodeFieldsForAMatch(string savedLine1, string savedLine2, string savedLine3, string savedLine4, string savedTown, string savedPostcode, string searchTerm)
        {
            var savedAddress = await TestDataHelper.InsertAddressInEs(ElasticsearchClient,
                addressConfig: new QueryableAddress
                {
                    Line1 = savedLine1,
                    Line2 = savedLine2,
                    Line3 = savedLine3,
                    Line4 = savedLine4,
                    Town = savedTown,
                    Postcode = savedPostcode,
                }
            ).ConfigureAwait(true);
            await TestDataHelper.InsertAddressInEs(ElasticsearchClient).ConfigureAwait(true);
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                AddressQuery = searchTerm
            };
            var (addresses, _) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(1);
            addresses.First().Should().BeEquivalentTo(savedAddress.AddressKey);
        }

        [TestCase("340", "green road", "hackney", "london", null, null, "green road")]
        [TestCase("340", "green road", "hackney", "london", null, null, "hackney")]
        [TestCase("340", "green road", "hackney", "london", null, null, "london")]
        [TestCase("340 green road", null, "hackney", "london", null, null, "green road")]
        [TestCase("340", "green road", "hackney", "london", null, null, "green road hackney")]
        [TestCase("340", "green road", "hackney", "london", null, null, "green street")]
        [TestCase("340", "green road", "hackney", "london", null, null, "grean road")]
        [TestCase("Flat A", "100 Mare Street", "Islington", "LDN", null, null, "100A Mare Street")] 
        [TestCase("6", "St James's road", "Islington", "LDN", null, null, "6 St Jamess road")]
        [TestCase("6", "St James's road", "Islington", "LDN", null, null, "6 St James road")]
        [TestCase("GROUND FLOOR FLAT", "210 Mare Street", "Hackney", "London", null, null, "210 Mare Street")]
        [TestCase("Flat 7", "Allerton House", "Hackney", "London", null, null, "flat 7 alerton house")]
        [TestCase("Flat 1", "29 Oxford Road", "MOSELEY AND KINGS HEATH", "BIRMINGHAM", null, null, "1 29 oxord road")]
        [TestCase("7 Eton House", "Leigh Road", "Islington", null, "London", "N5 1SR", "7 eto house, Lonon")]
        [TestCase("7 Eton Road", null, null, null, "Birmingham", "B12 8AY", "7 etn road, birminghm")]
        [TestCase("16 Holly Court", "Surrograte Street", null, null, "Attleborough", "NR17 2AW", "Attlborogh, NR17 2AW")]
        public async Task WillMatchPartialAndFuzzySearches(string line1, string line2, string line3, string line4, string town, string postcode, string searchTerm) 
        {
            var savedAddress = await TestDataHelper.InsertAddressInEs(ElasticsearchClient,
                addressConfig: new QueryableAddress
                {
                    Line1 = line1,
                    Line2 = line2,
                    Line3 = line3,
                    Line4 = line4,
                    Town = town,
                    Postcode = postcode,
                }
            ).ConfigureAwait(true);
            await TestDataHelper.InsertAddressInEs(ElasticsearchClient).ConfigureAwait(true);
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                AddressQuery = searchTerm
            };
            var (addresses, _) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(1);
            addresses.First().Should().BeEquivalentTo(savedAddress.AddressKey);
        }

        [TestCase("340", "yellow road", "hackney", "london", "green road")]
        [TestCase("340", "yellow road", "hackney", "london", "engre road")]
        [TestCase("Flat A", "100 Mare Street", "Islington", "LDN", "8 Mare Street")]
        [TestCase("Flat A", "10 DISPENSARY LANE", "Mare Street", "Hackney", "210 Mare Street")]
        public async Task WillNotMatchDifferentSearches(string line1, string line2, string line3, string line4, string searchTerm)
        {
            var savedAddress = await TestDataHelper.InsertAddressInEs(ElasticsearchClient,
                addressConfig: new QueryableAddress
                {
                    Line1 = line1,
                    Line2 = line2,
                    Line3 = line3,
                    Line4 = line4,
                }
            ).ConfigureAwait(true);
            await TestDataHelper.InsertAddressInEs(ElasticsearchClient).ConfigureAwait(true);
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                AddressQuery = searchTerm
            };
            var (addresses, _) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(0);
        }

        [TestCase("Alternative", new[] { "Alternative" })]
        [TestCase("Approved", new[] { "Approved" })]
        [TestCase("Approved Preferred", new[] { "Approved" })]
        [TestCase("Historical", new[] { "Historical" })]
        [TestCase("Provisional", new[] { "Provisional" })]
        [TestCase("Alternative", new[] { "Alternative", "Approved" })]
        [TestCase("alternative", new[] { "Alternative", "Approved" })]
        [TestCase("Historical", new[] { "Alternative", "Approved", "Historical" })]
        [TestCase("Provisional", new[] { "Historical", "Provisional" })]
        [TestCase("Provisional", new[] { "Historical", "provisional" })]
        public async Task WillSearchForAddressesWithStatus(string savedStatus, IEnumerable<string> statusSearchTerm)
        {
            var savedAddress = await TestDataHelper.InsertAddressInEs(ElasticsearchClient,
                addressConfig: new QueryableAddress { AddressStatus = savedStatus }
            ).ConfigureAwait(true);

            await TestDataHelper.InsertAddressInEs(ElasticsearchClient, addressConfig: new QueryableAddress
            {
                AddressStatus = savedStatus == "Provisional" ? "Alternative" : "Provisional"
            }).ConfigureAwait(true);
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                AddressStatus = statusSearchTerm
            };
            var (addresses, _) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(1);
            addresses.First().Should().BeEquivalentTo(savedAddress.AddressKey);
        }

        [Test]
        public async Task IfNoAddressStatusSearchGivenDefaultsToApprovedPreferred()
        {
            var savedAddress = await TestDataHelper.InsertAddressInEs(ElasticsearchClient).ConfigureAwait(true);

            await TestDataHelper.InsertAddressInEs(ElasticsearchClient,
                addressConfig: new QueryableAddress { AddressStatus = "Historical" }).ConfigureAwait(true);
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both,
            };
            var (addresses, _) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(1);
            addresses.First().Should().BeEquivalentTo(savedAddress.AddressKey);
        }

        [Test]
        public async Task WillSearchUprnsForAMatch()
        {
            var uprn = _faker.Random.Number(10000000, 99999999);
            var savedAddress = await TestDataHelper.InsertAddressInEs(ElasticsearchClient,
                addressConfig: new QueryableAddress { UPRN = uprn }
            ).ConfigureAwait(true);
            await TestDataHelper.InsertAddressInEs(ElasticsearchClient).ConfigureAwait(true);
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                Uprn = uprn
            };
            var (addresses, _) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(1);
            addresses.First().Should().BeEquivalentTo(savedAddress.AddressKey);
        }

        [Test]
        public async Task WillSearchUsrnsForAMatch()
        {
            var uprn = _faker.Random.Number(10000000, 99999999);
            var savedAddress = await TestDataHelper.InsertAddressInEs(ElasticsearchClient,
                addressConfig: new QueryableAddress { USRN = uprn }
            ).ConfigureAwait(true);
            await TestDataHelper.InsertAddressInEs(ElasticsearchClient).ConfigureAwait(true);
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                Usrn = uprn
            };
            var (addresses, _) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(1);
            addresses.First().Should().BeEquivalentTo(savedAddress.AddressKey);
        }

        [TestCase("Commercial", "Commercial")]
        [TestCase("Dual Use", "Dual Use")]
        [TestCase("Features", "Features")]
        [TestCase("Land", "Land")]
        [TestCase("Military", "Military")]
        [TestCase("Object of Interest", "Object of Interest")]
        [TestCase("Residential", "Residential")]
        [TestCase("Unclassified", "Unclassified")]
        [TestCase("Commercial", "Land,Commercial,Features")]
        [TestCase("Unclassified", "Residential,Unclassified")]
        [TestCase("Military", "Military,Dual Use")]
        public async Task WillSearchForAnAddressesPrimaryUsage(string savedUsage, string usageSearchTerm)
        {
            var savedAddress = await TestDataHelper.InsertAddressInEs(ElasticsearchClient,
                addressConfig: new QueryableAddress { UsagePrimary = savedUsage }
            ).ConfigureAwait(true);
            await TestDataHelper.InsertAddressInEs(ElasticsearchClient, addressConfig: new QueryableAddress
            {
                UsagePrimary = savedUsage == "Commercial" ? "Military" : "Commercial"
            }).ConfigureAwait(true);
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                UsagePrimary = usageSearchTerm
            };
            var (addresses, _) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(1);
            addresses.First().Should().BeEquivalentTo(savedAddress.AddressKey);
        }

        [TestCase("A1", "A1")]
        [TestCase("A3", "A")]
        [TestCase("C5", "B6,C5,C8")]
        [TestCase("A2", "a2,C5")]
        public async Task WillSearchForAnAddressesUsageCodeFromAList(string savedUsage, string usageSearchTerm)
        {
            var savedAddress = await TestDataHelper.InsertAddressInEs(ElasticsearchClient,
                addressConfig: new QueryableAddress { UsageCode = savedUsage }
            ).ConfigureAwait(true);

            await TestDataHelper.InsertAddressInEs(ElasticsearchClient, addressConfig: new QueryableAddress
            {
                UsageCode = "B7"
            }).ConfigureAwait(true);
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                UsageCode = usageSearchTerm
            };
            var (addresses, _) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(1);
            addresses.First().Should().BeEquivalentTo(savedAddress.AddressKey);
        }

        [TestCase("Local")]
        [TestCase("local")]
        [TestCase("Hackney")]
        [TestCase("hackney")]
        public async Task CanSearchOnlyLocalHackneyAddressesCaseInsensitively(string gazetteer)
        {
            var savedAddress = await TestDataHelper.InsertAddressInEs(ElasticsearchClient,
                addressConfig: new QueryableAddress { Gazetteer = gazetteer }
            ).ConfigureAwait(true);
            await TestDataHelper.InsertAddressInEs(ElasticsearchClient,
                addressConfig: new QueryableAddress { Gazetteer = "National" }
            ).ConfigureAwait(true);
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Hackney,
            };
            var (addresses, _) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(1);
            addresses.First().Should().BeEquivalentTo(savedAddress.AddressKey);
        }

        [Test]
        public async Task WillFilterHackneyAddressByOutOfBoroughAddresses()
        {
            var outOfBorough = await TestDataHelper.InsertAddressInEs(ElasticsearchClient,
                addressConfig: new QueryableAddress { OutOfBoroughAddress = true, Gazetteer = "Hackney" }
            ).ConfigureAwait(true);
            var hackneyAddress = await TestDataHelper.InsertAddressInEs(ElasticsearchClient,
                addressConfig: new QueryableAddress { OutOfBoroughAddress = false, Gazetteer = "Hackney" }
            ).ConfigureAwait(true);
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                OutOfBoroughAddress = false,
            };
            var (addresses, _) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(1);
            addresses.First().Should().BeEquivalentTo(hackneyAddress.AddressKey);
        }

        [Test]
        public async Task WillFilterOutAllQueryableAddressesWhenQueryingForNoOutOfBoroughAddresses()
        {
            var outOfBorough1 = await TestDataHelper.InsertAddressInEs(ElasticsearchClient,
                addressConfig: new QueryableAddress { OutOfBoroughAddress = true, Gazetteer = "National" }
            ).ConfigureAwait(true);
            var outOfBorough2 = await TestDataHelper.InsertAddressInEs(ElasticsearchClient,
                addressConfig: new QueryableAddress { OutOfBoroughAddress = false, Gazetteer = "National" }
            ).ConfigureAwait(true);
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                OutOfBoroughAddress = false,
            };
            var (addresses, _) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(0);
        }

        [Test]
        public async Task IfOutOfBoroughFlagIsTrueReturnsAllAddresses()
        {
            var savedAddresses = new List<QueryableAddress>
            {
                new QueryableAddress {OutOfBoroughAddress = true, Gazetteer = "Hackney"},
                new QueryableAddress {OutOfBoroughAddress = false, Gazetteer = "Hackney"},
                new QueryableAddress {OutOfBoroughAddress = false, Gazetteer = "National"},
                new QueryableAddress {OutOfBoroughAddress = true, Gazetteer = "National"}
            };
            await IndexAddresses(savedAddresses).ConfigureAwait(true);

            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                OutOfBoroughAddress = true,
            };
            var (addresses, _) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(4);
        }

        [Test]
        public async Task WillSearchForAllAddressesUsingCrossReferencedUprns()
        {
            //--- Cross reference for address one
            var uprnOne = _faker.Random.Long(10000000, 99999999);

            var addressOne = await TestDataHelper.InsertAddressInEs(ElasticsearchClient,
                addressConfig: new QueryableAddress { UPRN = uprnOne }
            ).ConfigureAwait(true);

            //--- Cross reference for address two
            var uprnTwo = _faker.Random.Long(10000000, 99999999);

            var addressTwo = await TestDataHelper.InsertAddressInEs(ElasticsearchClient,
                addressConfig: new QueryableAddress { UPRN = uprnTwo }
            ).ConfigureAwait(true);

            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                CrossReferencedUprns = new List<long> { uprnOne, uprnTwo },
            };

            var (addresses, _) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(2);
            addresses.Should().ContainEquivalentOf(addressOne.AddressKey);
            addresses.Should().ContainEquivalentOf(addressTwo.AddressKey);
        }

        [Test]
        public async Task WillOnlyReturnAddressesModifiedSinceADate()
        {
            var savedAddresses = new List<QueryableAddress>
            {
                new QueryableAddress {PropertyChangeDate = 20200523},
                new QueryableAddress {PropertyChangeDate = 20191204},
                new QueryableAddress {PropertyChangeDate = 20200801},
                new QueryableAddress {PropertyChangeDate = 20201203}
            };
            savedAddresses = await IndexAddresses(savedAddresses).ConfigureAwait(true);

            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                ModifiedSince = new DateTime(2020, 06, 01),
            };
            var (addressKeys, _) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addressKeys.Count.Should().Be(2);
            Assert.Contains(savedAddresses.ElementAt(2).AddressKey, addressKeys);
            Assert.Contains(savedAddresses.ElementAt(3).AddressKey, addressKeys);
        }

        [Test]
        public async Task WillOnlyReturnAddressesModifiedSinceADateIfTheyAreNewlyCreated()
        {
            var savedAddresses = new List<QueryableAddress>
            {
                new QueryableAddress {PropertyChangeDate = 20200523, PropertyStartDate = 20190523},
                new QueryableAddress {PropertyChangeDate = 20191204, PropertyStartDate = 20190523},
                new QueryableAddress {PropertyStartDate = 20200801, PropertyChangeDate = null},
                new QueryableAddress {PropertyStartDate = 20201203, PropertyChangeDate = null}
            };
            savedAddresses = await IndexAddresses(savedAddresses).ConfigureAwait(true);

            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                ModifiedSince = new DateTime(2020, 06, 01),
            };
            var (addressKeys, _) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addressKeys.Count.Should().Be(2);
            Assert.Contains(savedAddresses.ElementAt(2).AddressKey, addressKeys);
            Assert.Contains(savedAddresses.ElementAt(3).AddressKey, addressKeys);
        }
        #endregion

        #region parentShells

        [Test]
        public async Task IfSetToExcludeParentShellsWillOnlyReturnsResultsOfSearch()
        {
            var parentShell = await TestDataHelper.InsertAddressInEs(ElasticsearchClient).ConfigureAwait(true);
            var addressToMatch = await TestDataHelper.InsertAddressInEs(ElasticsearchClient,
                addressConfig: new QueryableAddress { ParentUPRN = parentShell.UPRN }
            ).ConfigureAwait(true);
            await TestDataHelper.InsertAddressInEs(ElasticsearchClient).ConfigureAwait(true);

            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                Postcode = addressToMatch.Postcode,
                IncludeParentShells = false
            };
            var (addresses, _) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(1);
            addresses.Should().ContainEquivalentOf(addressToMatch.AddressKey);
        }

        [Test]
        public async Task IfSetToIncludeParentShellsIncludeImmediateParentShells()
        {
            var parentShell = await TestDataHelper.InsertAddressInEs(ElasticsearchClient).ConfigureAwait(true);
            var addressToMatch = await TestDataHelper.InsertAddressInEs(ElasticsearchClient,
                addressConfig: new QueryableAddress { ParentUPRN = parentShell.UPRN }
            ).ConfigureAwait(true);
            await TestDataHelper.InsertAddressInEs(ElasticsearchClient).ConfigureAwait(true);

            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                Postcode = addressToMatch.Postcode,
                IncludeParentShells = true
            };
            var (addresses, _) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(2);
            addresses.Should().ContainEquivalentOf(addressToMatch.AddressKey);
            addresses.Should().ContainEquivalentOf(parentShell.AddressKey);
        }

        [Test]
        public async Task IfSetToIncludeParentShellsWillIncludeParentShellsInTotalCount()
        {
            var parentShell = await TestDataHelper.InsertAddressInEs(ElasticsearchClient).ConfigureAwait(true);
            var addressOneToMatch = await TestDataHelper.InsertAddressInEs(ElasticsearchClient,
                addressConfig: new QueryableAddress { ParentUPRN = parentShell.UPRN }
            ).ConfigureAwait(true);
            var parentShellTwo = await TestDataHelper.InsertAddressInEs(ElasticsearchClient).ConfigureAwait(true);
            var addressTwoToMatch = await TestDataHelper.InsertAddressInEs(ElasticsearchClient,
                addressConfig: new QueryableAddress { ParentUPRN = parentShellTwo.UPRN, Postcode = addressOneToMatch.Postcode }
            ).ConfigureAwait(true);

            await TestDataHelper.InsertAddressInEs(ElasticsearchClient).ConfigureAwait(true);

            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 2,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                Postcode = addressOneToMatch.Postcode,
                IncludeParentShells = true
            };
            var (addresses, totalCount) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(2);
            totalCount.Should().Be(4);
        }

        [Test]
        public async Task IfSetToIncludeParentShellsIncludeParentShellsOfParentsShells()
        {
            var grandParentShell = await TestDataHelper.InsertAddressInEs(ElasticsearchClient).ConfigureAwait(true);
            var parentShell = await TestDataHelper.InsertAddressInEs(ElasticsearchClient,
                addressConfig: new QueryableAddress { ParentUPRN = grandParentShell.UPRN }).ConfigureAwait(true);
            var addressToMatch = await TestDataHelper.InsertAddressInEs(ElasticsearchClient,
                addressConfig: new QueryableAddress { ParentUPRN = parentShell.UPRN }
            ).ConfigureAwait(true);
            await TestDataHelper.InsertAddressInEs(ElasticsearchClient).ConfigureAwait(true);

            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                Postcode = addressToMatch.Postcode,
                IncludeParentShells = true
            };
            var (addresses, _) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(3);
            addresses.Should().ContainEquivalentOf(addressToMatch.AddressKey);
            addresses.Should().ContainEquivalentOf(parentShell.AddressKey);
            addresses.Should().ContainEquivalentOf(grandParentShell.AddressKey);
        }

        [TestCase("E8 7JH")]
        [TestCase("N1 5TH")]
        public async Task IfSetToExcludeParentShellsWillNotReturnAnyParentShells(string postcode)
        {
            var parentShell = await TestDataHelper.InsertAddressInEs(ElasticsearchClient,
                addressConfig: new QueryableAddress { PropertyShell = true, Postcode = postcode }
            ).ConfigureAwait(true);
            var notAParentShell = await TestDataHelper.InsertAddressInEs(ElasticsearchClient,
                addressConfig: new QueryableAddress { PropertyShell = false, Postcode = postcode }
            ).ConfigureAwait(true);

            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                Postcode = postcode,
                IncludeParentShells = false
            };
            var (addresses, _) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(1);
            addresses.Should().ContainEquivalentOf(notAParentShell.AddressKey);
        }

        #endregion

        #region ordering

        [Test]
        public async Task WillOrderCorrectStreetNamesOverCommonSynonyms()
        {
            var savedAddresses = new List<QueryableAddress>
            {
                new QueryableAddress
                {
                    Street = "Elton Road", Line1 = "107 Elton Road", Line2 = "hackney", Line3 = null, Line4 = null, Town = "hackney", Postcode = "E3 4TT"
                },
                new QueryableAddress
                {
                    Street = "Elton Close", Line1 = "12 Elton Close", Line2 = "hackney", Line3 = null, Line4 = null, Town = "hackney", Postcode = "E3 4TT"
                },
            };
            savedAddresses = await IndexAddresses(savedAddresses).ConfigureAwait(true);

            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                AddressQuery = "Elton Road"
            };
            var (addresses, _) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(2);
            Assert.Contains(savedAddresses.ElementAt(0).AddressKey, addresses);
            Assert.Contains(savedAddresses.ElementAt(1).AddressKey, addresses);
        }

        [Test]
        public async Task ItWillOrderCorrectMatchesOverFuzzyMatches()
        {
            var savedAddresses = new List<QueryableAddress>
            {
                new QueryableAddress
                {
                    Street = "GUNNERSBURY AVENUE",
                    Line1 = "EALING RIDING SCHOOL",
                    Line2 = " 17-19 GUNNERSBURY AVENUE",
                    Line3 = "EALING",
                    Line4 = "LONDON",
                    PaonStartNumber = 17,
                    UnitNumber = null,
                    UnitName = "EALING RIDING SCHOOL",
                    BuildingNumber = "17 -19",
                    Town = "EALING",
                    Postcode = "W5 3XD"
                },
                new QueryableAddress
                {
                    Street = "READING LANE",
                    Line1 = "FLAT D",
                    Line2 = "7 READING LANE",
                    PaonStartNumber = 7,
                    UnitNumber = null,
                    UnitName = "D",
                    BuildingNumber = "7",
                    Line3 = "HACKNEY",
                    Line4 = "LONDON",
                    Town = "LONDON",
                    Postcode = "E8 1DS"
                },
            };
            savedAddresses = await IndexAddresses(savedAddresses).ConfigureAwait(true);

            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                AddressQuery = "reading lane, london"
            };
            var (addresses, _) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(2);
            Assert.Contains(savedAddresses.ElementAt(0).AddressKey, addresses);
            Assert.Contains(savedAddresses.ElementAt(1).AddressKey, addresses);
        }

        [Test]
        public async Task WillNotBoostResultsWithMultipleSearchTermsReferenced()
        {
            var savedAddresses = new List<QueryableAddress>
            {
                new QueryableAddress
                {
                    Street = "Greenwood Drive",
                    Line1 = "Flat 10",
                    Line2 = "10 Greenwood Drive",
                    Line3 = null,
                    Line4 = null,
                    PaonStartNumber = 10,
                    UnitNumber = 10,
                    BuildingNumber = "10",
                    Town = "hackney",
                    Postcode = "E3 4TT"
                },
                new QueryableAddress
                {
                    Street = "Greenwood Drive",
                    Line1 = "Flat 2",
                    Line2 = "10 Greenwood Drive",
                    PaonStartNumber = 10,
                    UnitNumber = 2,
                    BuildingNumber = "10",
                    Line3 = null,
                    Line4 = null,
                    Town = "hackney",
                    Postcode = "E3 4TT"
                },
            };
            savedAddresses = await IndexAddresses(savedAddresses).ConfigureAwait(true);

            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                AddressQuery = "10 Greenwood Drive"
            };
            var (addresses, _) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(2);
            Assert.Contains(savedAddresses.ElementAt(0).AddressKey, addresses);
            Assert.Contains(savedAddresses.ElementAt(1).AddressKey, addresses);
        }

        [TestCase("eton road")]
        [TestCase("12 eton road london")]
        [TestCase("12 eton road")]
        public async Task ItWillOrderMatchesInTheCorrectOrderOverMatchesWillSearchTermsSpreadOut(string query)
        {
            var savedAddresses = new List<QueryableAddress>
            {
                new QueryableAddress
                {
                    Street = "ETON COLLEGE ROAD",
                    Line1 = "FLAT 12",
                    Line2 = " ETON RISE",
                    Line3 = " ETON COLLEGE ROAD",
                    Line4 = "LONDON",
                    PaonStartNumber = null,
                    UnitNumber = 12,
                    UnitName = "Flat 12",
                    BuildingNumber = "",
                    Town = "LONDON",
                    Postcode = "NW3 2DE"
                },
                new QueryableAddress
                {
                    Street = "READING LANE",
                    Line1 = "FLAT A",
                    Line2 = "12 ETON ROAD",
                    PaonStartNumber = 12,
                    UnitNumber = null,
                    UnitName = "A",
                    BuildingNumber = "12",
                    Line3 = "LONDON",
                    Line4 = "NW3 4SS",
                    Town = "LONDON",
                    Postcode = "NW3 4SS"
                },
            };
            savedAddresses = await IndexAddresses(savedAddresses).ConfigureAwait(true);

            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                AddressQuery = query
            };
            var (addresses, _) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(2);
            Assert.Contains(savedAddresses.ElementAt(0).AddressKey, addresses);
            Assert.Contains(savedAddresses.ElementAt(1).AddressKey, addresses);
        }

        [Test]
        public async Task WontOrderMatchedWithMultipleFuzzyTermsHigherThenThoseWithOneTerm()
        {
            var savedAddresses = new List<QueryableAddress>
            {
                new QueryableAddress
                {
                    Street = "MARE STREET",
                    Town = "LONDON",
                    PaonStartNumber = 2,
                    Line1 = "MINI MART",
                    Line2 = " 185-187 MARE STREET",
                    Line3 = " HACKNEY",
                    Line4 = " LONDON"
                },
                new QueryableAddress
                {
                    Street = "MARE STREET",
                    Town = "LONDON",
                    PaonStartNumber = 1,
                    Line1 = "ST JOHNS GARDENS",
                    Line2 = " MARE STREET",
                    Line3 = " HACKNEY",
                    Line4 = " LONDON"
                }
            };
            savedAddresses = await IndexAddresses(savedAddresses).ConfigureAwait(true);

            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                AddressQuery = "mare street"
            };
            var (addresses, _) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(2);
            Assert.Contains(savedAddresses.ElementAt(0).AddressKey, addresses);
            Assert.Contains(savedAddresses.ElementAt(1).AddressKey, addresses);
        }
        [Test]
        public async Task WillFirstlyOrderByTown()
        {
            var savedAddresses = new List<QueryableAddress>
            {
                new QueryableAddress { Town = "town a" },
                new QueryableAddress { Town = "town b" },
                new QueryableAddress { Town = "hackney" }
            };
            savedAddresses = await IndexAddresses(savedAddresses).ConfigureAwait(true);

            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both,
            };
            var (addresses, _) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(3);
            Assert.Contains(savedAddresses.ElementAt(0).AddressKey, addresses);
            Assert.Contains(savedAddresses.ElementAt(1).AddressKey, addresses);
            Assert.Contains(savedAddresses.ElementAt(2).AddressKey, addresses);
        }

        [Test]
        public async Task WillSecondlyOrderByStreet()
        {
            var savedAddresses = new List<QueryableAddress>
            {
                new QueryableAddress { Town = "town b" },
                new QueryableAddress { Town = "town a", Street = "B Street" },
                new QueryableAddress { Town = "town a", Street = "A Street" }
            };
            savedAddresses = await IndexAddresses(savedAddresses).ConfigureAwait(true);

            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both,
            };
            var (addresses, _) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(3);
            Assert.Contains(savedAddresses.ElementAt(0).AddressKey, addresses);
            Assert.Contains(savedAddresses.ElementAt(1).AddressKey, addresses);
            Assert.Contains(savedAddresses.ElementAt(2).AddressKey, addresses);
        }

        [Test]
        public async Task WillThirdlyOrderByPresenceAndOrderOfPaonStartNumber()
        {
            var savedAddresses = new List<QueryableAddress>
            {
                new QueryableAddress { Town = "town a", Street = "B Street", PaonStartNumber = 30 },
                new QueryableAddress { Town = "town a", Street = "B Street", PaonStartNumber = 0 },
                new QueryableAddress { Town = "town a", Street = "B Street", PaonStartNumber = 5 }
            };
            savedAddresses = await IndexAddresses(savedAddresses).ConfigureAwait(true);

            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both,
            };
            var (addresses, _) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(3);
            Assert.Contains(savedAddresses.ElementAt(0).AddressKey, addresses);
            Assert.Contains(savedAddresses.ElementAt(1).AddressKey, addresses);
            Assert.Contains(savedAddresses.ElementAt(2).AddressKey, addresses);
        }

        [Test]
        public async Task WillFourthlyOrderByPresenceAndOrderOfUnitNumber()
        {
            var savedAddresses = new List<QueryableAddress>
            {
                new QueryableAddress {
                    Town = "town a",
                    Street = "B Street",
                    PaonStartNumber = 1,
                    UnitNumber = 4
                },
                new QueryableAddress {
                    Town = "town a",
                    Street = "B Street",
                    PaonStartNumber = 1,
                    UnitNumber = null
                },
                new QueryableAddress {
                    Town = "town a",
                    Street = "B Street",
                    PaonStartNumber = 1,
                    UnitNumber = 23
                }
            };
            savedAddresses = await IndexAddresses(savedAddresses).ConfigureAwait(true);

            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both,
            };
            var (addresses, _) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(3);
            Assert.Contains(savedAddresses.ElementAt(0).AddressKey, addresses);
            Assert.Contains(savedAddresses.ElementAt(1).AddressKey, addresses);
            Assert.Contains(savedAddresses.ElementAt(2).AddressKey, addresses);
        }

        [Test]
        public async Task WillFifthlyOrderByPresenceAndOrderOfUnitName()
        {
            var savedAddresses = new List<QueryableAddress>
            {
                new QueryableAddress {
                    Town = "town a",
                    Street = "B Street",
                    PaonStartNumber = 1,
                    UnitNumber = 43,
                    UnitName = "J name"
                },
                new QueryableAddress {
                    Town = "town a",
                    Street = "B Street",
                    PaonStartNumber = 1,
                    UnitNumber = 43,
                    UnitName = "A name"
                },
                new QueryableAddress {
                    Town = "town a",
                    Street = "B Street",
                    PaonStartNumber = 1,
                    UnitNumber = 43,
                    UnitName = ""
                }
            };
            savedAddresses = await IndexAddresses(savedAddresses).ConfigureAwait(true);

            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both,
            };
            var (addresses, _) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(3);
            Assert.Contains(savedAddresses.ElementAt(0).AddressKey, addresses);
            Assert.Contains(savedAddresses.ElementAt(1).AddressKey, addresses);
            Assert.Contains(savedAddresses.ElementAt(2).AddressKey, addresses);
        }

        private async Task<List<QueryableAddress>> IndexAddresses(IEnumerable<QueryableAddress> addresses)
        {
            var newAddresses = new List<QueryableAddress>();
            foreach (var queryableAddress in addresses)
            {
                newAddresses.Add(await TestDataHelper
                    .InsertAddressInEs(ElasticsearchClient, addressConfig: queryableAddress)
                    .ConfigureAwait(true));
            }

            return newAddresses;
        }
        #endregion

        #region pagination

        [Test]
        public async Task ItWillReturnTheNumberOfRecordsRequestedInPageSize()
        {
            await AddOrderedRecordsToDatabase(3).ConfigureAwait(true);

            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 2,
                Gazetteer = GlobalConstants.Gazetteer.Both
            };

            var (addresses, _) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(2);
        }

        [Test]
        public async Task ItWillReturnASecondPageOfResults()
        {
            var records = await AddOrderedRecordsToDatabase(10).ConfigureAwait(true);

            var request = new SearchParameters
            {
                Page = 2,
                PageSize = 3,
                Gazetteer = GlobalConstants.Gazetteer.Both
            };

            var (addresses, _) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(3);
            addresses.Should().ContainEquivalentOf(records.ElementAt(3).AddressKey);
            addresses.Should().ContainEquivalentOf(records.ElementAt(4).AddressKey);
            addresses.Should().ContainEquivalentOf(records.ElementAt(5).AddressKey);
        }

        [Test]
        public async Task ItWillReturnAThirdPageOfResults()
        {
            var records = await AddOrderedRecordsToDatabase(13).ConfigureAwait(true);

            var request = new SearchParameters
            {
                Page = 3,
                PageSize = 4,
                Gazetteer = GlobalConstants.Gazetteer.Both
            };

            var (addresses, _) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(4);
            addresses.Should().ContainEquivalentOf(records.ElementAt(8).AddressKey);
            addresses.Should().ContainEquivalentOf(records.ElementAt(9).AddressKey);
            addresses.Should().ContainEquivalentOf(records.ElementAt(10).AddressKey);
            addresses.Should().ContainEquivalentOf(records.ElementAt(11).AddressKey);
        }

        [Test]
        public async Task ItWillAcceptZeroAsTheFirstPage()
        {
            var records = await AddOrderedRecordsToDatabase(13).ConfigureAwait(true);

            var request = new SearchParameters
            {
                Page = 0,
                PageSize = 4,
                Gazetteer = GlobalConstants.Gazetteer.Both
            };

            var (addresses, _) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(4);
            addresses.Should().ContainEquivalentOf(records.ElementAt(0).AddressKey);
            addresses.Should().ContainEquivalentOf(records.ElementAt(1).AddressKey);
            addresses.Should().ContainEquivalentOf(records.ElementAt(2).AddressKey);
            addresses.Should().ContainEquivalentOf(records.ElementAt(3).AddressKey);
        }

        private async Task<List<QueryableAddress>> AddOrderedRecordsToDatabase(int count)
        {
            var towns = GenerateRandomOrderedListOfWordsOfMaxLength(count, 100);
            var postcodes = GenerateRandomOrderedListOfWordsOfMaxLength(count, 8);
            var streets = GenerateRandomOrderedListOfWordsOfMaxLength(count, 100);
            var buildingNumbers = GenerateRandomOrderedListOfWordsOfMaxLength(count, 17);
            var unitNames = GenerateRandomOrderedListOfWordsOfMaxLength(count, 90);
            var records = new List<QueryableAddress>();
            for (var i = 0; i < count; i++)
            {
                records.Add(await TestDataHelper.InsertAddressInEs(ElasticsearchClient, addressConfig: new QueryableAddress
                {
                    Town = towns.ElementAt(i),
                    Postcode = postcodes.ElementAt(i),
                    Street = streets.ElementAt(i),
                    BuildingNumber = buildingNumbers.ElementAt(i),
                    UnitNumber = Convert.ToInt16($"{i}{_faker.Random.Int(10, 99)}"),
                    UnitName = unitNames.ElementAt(i),
                    PaonStartNumber = Convert.ToInt16($"{i}{_faker.Random.Int(10, 99)}")
                }).ConfigureAwait(true));
            }

            return records;
        }

        private IOrderedEnumerable<string> GenerateRandomOrderedListOfWordsOfMaxLength(int count, int maxLength)
        {
            return _faker.Lorem.Words(count)
                .Select(w => w.Substring(0, w.Length < maxLength ? w.Length : maxLength))
                .OrderBy(w => w);
        }

        #endregion

        #region totalCount

        [TestCase(20)]
        [TestCase(7)]
        public async Task ItWillReturnTheTotalCountIfThereIsOnlyOnePageOfRecords(int count)
        {
            await AddOrderedRecordsToDatabase(count).ConfigureAwait(true);
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = count + 1,
                Gazetteer = GlobalConstants.Gazetteer.Both
            };
            var (addresses, totalCount) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(count);
            totalCount.Should().Be(count);
        }

        [TestCase(20)]
        [TestCase(7)]
        public async Task ItWillReturnTheTotalCountIfThereAreMultiplePagesOfRecords(int count)
        {
            await AddOrderedRecordsToDatabase(count).ConfigureAwait(true);
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = count - 5,
                Gazetteer = GlobalConstants.Gazetteer.Both
            };
            var (addresses, totalCount) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(count - 5);
            totalCount.Should().Be(count);
        }

        #endregion
        #endregion
    }
}
