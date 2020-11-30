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

        [TestCase("340", "green road", "hackney", "london", "green road")]
        [TestCase("340", "green road", "hackney", "london", "hackney")]
        [TestCase("340", "green road", "hackney", "london", "london")]
        [TestCase("340 green road", null, "hackney", "london", "green road")]
        [TestCase("340", "green road", "hackney", "london", "green road hackney")]
        [TestCase("340", "green road", "hackney", "london", "green street")]
        [TestCase("340", "green road", "hackney", "london", "grean road")]
        [TestCase("Flat A", "100 Mare Street", "Islington", "LDN", "100A Mare Street")]
        [TestCase("6", "St James's road", "Islington", "LDN", "6 St Jamess road")]
        [TestCase("6", "St James's road", "Islington", "LDN", "6 St James road")]
        [TestCase("GROUND FLOOR FLAT", "210 Mare Street", "Hackney", "London", "210 Mare Street")]
        [TestCase("Flat 7", "Allerton House", "Hackney", "London", "flat 7 alerton house")]
        [TestCase("Flat 1", "29 Oxford Road", "MOSELEY AND KINGS HEATH", "BIRMINGHAM", "1 29 oxord road")]
        public async Task WillMatchPartialAndFuzzySearches(string line1, string line2, string line3, string line4, string searchTerm)
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
            await TestDataHelper.InsertAddressInEs(ElasticsearchClient,
                addressConfig: new QueryableAddress { OutOfBoroughAddress = true, Gazetteer = "Hackney" }
            ).ConfigureAwait(true);
            await TestDataHelper.InsertAddressInEs(ElasticsearchClient,
                addressConfig: new QueryableAddress { OutOfBoroughAddress = false, Gazetteer = "Hackney" }
            ).ConfigureAwait(true);
            await TestDataHelper.InsertAddressInEs(ElasticsearchClient,
                addressConfig: new QueryableAddress { OutOfBoroughAddress = false, Gazetteer = "National" }
            ).ConfigureAwait(true);
            await TestDataHelper.InsertAddressInEs(ElasticsearchClient,
                addressConfig: new QueryableAddress { OutOfBoroughAddress = true, Gazetteer = "National" }
            ).ConfigureAwait(true);
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
        public async Task WillFirstlyOrderByTown()
        {
            var addressOne = await TestDataHelper
                .InsertAddressInEs(ElasticsearchClient, addressConfig: new QueryableAddress { Town = "town a" })
                .ConfigureAwait(true);
            var addressTwo = await TestDataHelper
                .InsertAddressInEs(ElasticsearchClient, addressConfig: new QueryableAddress { Town = "town b" })
                .ConfigureAwait(true);
            var addressThree = await TestDataHelper
                .InsertAddressInEs(ElasticsearchClient, addressConfig: new QueryableAddress { Town = "hackney" })
                .ConfigureAwait(true);

            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both,
            };
            var (addresses, _) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(3);
            addresses.ElementAt(0).Should().BeEquivalentTo(addressThree.AddressKey);
            addresses.ElementAt(1).Should().BeEquivalentTo(addressOne.AddressKey);
            addresses.ElementAt(2).Should().BeEquivalentTo(addressTwo.AddressKey);
        }

        [Test]
        public async Task WillSecondlyOrderByPostcodePresence()
        {
            var addressOne = await TestDataHelper
                .InsertAddressInEs(ElasticsearchClient, addressConfig: new QueryableAddress { Town = "town a", Postcode = "" })
                .ConfigureAwait(true);
            var addressTwo = await TestDataHelper
                .InsertAddressInEs(ElasticsearchClient, addressConfig: new QueryableAddress { Town = "town a", Postcode = "E3 4TT" })
                .ConfigureAwait(true);
            var addressThree = await TestDataHelper
                .InsertAddressInEs(ElasticsearchClient, addressConfig: new QueryableAddress { Town = "town b" })
                .ConfigureAwait(true);

            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both,
            };
            var (addresses, _) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(3);
            addresses.ElementAt(0).Should().BeEquivalentTo(addressTwo.AddressKey);
            addresses.ElementAt(1).Should().BeEquivalentTo(addressOne.AddressKey);
            addresses.ElementAt(2).Should().BeEquivalentTo(addressThree.AddressKey);
        }

        [Test]
        public async Task WillThirdlyOrderByStreet()
        {
            var addressOne = await TestDataHelper
                .InsertAddressInEs(ElasticsearchClient, addressConfig: new QueryableAddress { Town = "town b" })
                .ConfigureAwait(true);
            var addressTwo = await TestDataHelper
                .InsertAddressInEs(ElasticsearchClient, addressConfig: new QueryableAddress { Town = "town a", Postcode = "", Street = "B Street" })
                .ConfigureAwait(true);
            var addressThree = await TestDataHelper
                .InsertAddressInEs(ElasticsearchClient, addressConfig: new QueryableAddress { Town = "town a", Postcode = "", Street = "A Street" })
                .ConfigureAwait(true);
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both,
            };
            var (addresses, _) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(3);
            addresses.ElementAt(0).Should().BeEquivalentTo(addressThree.AddressKey);
            addresses.ElementAt(1).Should().BeEquivalentTo(addressTwo.AddressKey);
            addresses.ElementAt(2).Should().BeEquivalentTo(addressOne.AddressKey);
        }

        [Test]
        public async Task WillFourthlyOrderByPresenceAndOrderOfPaonStartNumber()
        {
            var addressOne = await TestDataHelper.InsertAddressInEs(ElasticsearchClient, addressConfig: new QueryableAddress
            {
                Town = "town a",
                Postcode = "",
                Street = "B Street",
                PaonStartNumber = 3
            }).ConfigureAwait(true);
            var addressTwo = await TestDataHelper.InsertAddressInEs(ElasticsearchClient, addressConfig: new QueryableAddress
            {
                Town = "town a",
                Postcode = "",
                Street = "B Street",
                PaonStartNumber = 0
            }).ConfigureAwait(true);
            var addressThree = await TestDataHelper.InsertAddressInEs(ElasticsearchClient, addressConfig: new QueryableAddress
            {
                Town = "town a",
                Postcode = "",
                Street = "B Street",
                PaonStartNumber = 5
            }).ConfigureAwait(true);
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both,
            };
            var (addresses, _) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(3);
            addresses.ElementAt(0).Should().BeEquivalentTo(addressOne.AddressKey);
            addresses.ElementAt(1).Should().BeEquivalentTo(addressThree.AddressKey);
            addresses.ElementAt(2).Should().BeEquivalentTo(addressTwo.AddressKey);
        }

        [Test]
        public async Task WillFifthlyOrderByPresenceAndOrderOfBuildingNumber()
        {
            var addressOne = await TestDataHelper.InsertAddressInEs(ElasticsearchClient, addressConfig: new QueryableAddress
            {
                Town = "town a",
                Postcode = "",
                Street = "B Street",
                PaonStartNumber = 1,
                BuildingNumber = ""
            }).ConfigureAwait(true);
            var addressTwo = await TestDataHelper.InsertAddressInEs(ElasticsearchClient, addressConfig: new QueryableAddress
            {
                Town = "town a",
                Postcode = "",
                Street = "B Street",
                PaonStartNumber = 1,
                BuildingNumber = "78"
            }).ConfigureAwait(true);
            var addressThree = await TestDataHelper.InsertAddressInEs(ElasticsearchClient, addressConfig: new QueryableAddress
            {
                Town = "town a",
                Postcode = "",
                Street = "B Street",
                PaonStartNumber = 1,
                BuildingNumber = "99"
            }).ConfigureAwait(true);
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both,
            };
            var (addresses, _) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(3);
            addresses.ElementAt(0).Should().BeEquivalentTo(addressTwo.AddressKey);
            addresses.ElementAt(1).Should().BeEquivalentTo(addressThree.AddressKey);
            addresses.ElementAt(2).Should().BeEquivalentTo(addressOne.AddressKey);
        }

        [Test]
        public async Task WillSixthOrderByPresenceAndOrderOfUnitNumber()
        {
            var addressOne = await TestDataHelper.InsertAddressInEs(ElasticsearchClient, addressConfig: new QueryableAddress
            {
                Town = "town a",
                Postcode = "",
                Street = "B Street",
                PaonStartNumber = 1,
                BuildingNumber = "78",
                UnitNumber = "43"
            }).ConfigureAwait(true);
            var addressTwo = await TestDataHelper.InsertAddressInEs(ElasticsearchClient, addressConfig: new QueryableAddress
            {
                Town = "town a",
                Postcode = "",
                Street = "B Street",
                PaonStartNumber = 1,
                BuildingNumber = "78",
                UnitNumber = ""
            }).ConfigureAwait(true);
            var addressThree = await TestDataHelper.InsertAddressInEs(ElasticsearchClient, addressConfig: new QueryableAddress
            {
                Town = "town a",
                Postcode = "",
                Street = "B Street",
                PaonStartNumber = 1,
                BuildingNumber = "78",
                UnitNumber = "23"
            }).ConfigureAwait(true);
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both,
            };
            var (addresses, _) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(3);
            addresses.ElementAt(0).Should().BeEquivalentTo(addressThree.AddressKey);
            addresses.ElementAt(1).Should().BeEquivalentTo(addressOne.AddressKey);
            addresses.ElementAt(2).Should().BeEquivalentTo(addressTwo.AddressKey);
        }

        [Test]
        public async Task WillInTheSeventhCaseOrderByPresenceAndOrderOfUnitName()
        {
            var addressOne = await TestDataHelper.InsertAddressInEs(ElasticsearchClient, addressConfig: new QueryableAddress
            {
                Town = "town a",
                Postcode = "",
                Street = "B Street",
                PaonStartNumber = 1,
                BuildingNumber = "78",
                UnitNumber = "43",
                UnitName = "J name"
            }).ConfigureAwait(true);
            var addressTwo = await TestDataHelper.InsertAddressInEs(ElasticsearchClient, addressConfig: new QueryableAddress
            {
                Town = "town a",
                Postcode = "",
                Street = "B Street",
                PaonStartNumber = 1,
                BuildingNumber = "78",
                UnitNumber = "43",
                UnitName = "A name"
            }).ConfigureAwait(true);
            var addressThree = await TestDataHelper.InsertAddressInEs(ElasticsearchClient, addressConfig: new QueryableAddress
            {
                Town = "town a",
                Postcode = "",
                Street = "B Street",
                PaonStartNumber = 1,
                BuildingNumber = "78",
                UnitNumber = "43",
                UnitName = ""
            }).ConfigureAwait(true);
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both,
            };
            var (addresses, _) = await _classUnderTest.SearchAddresses(request).ConfigureAwait(true);

            addresses.Count.Should().Be(3);
            addresses.ElementAt(0).Should().BeEquivalentTo(addressTwo.AddressKey);
            addresses.ElementAt(1).Should().BeEquivalentTo(addressOne.AddressKey);
            addresses.ElementAt(2).Should().BeEquivalentTo(addressThree.AddressKey);
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
            var unitNumbers = GenerateRandomOrderedListOfWordsOfMaxLength(count, 17);
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
                    UnitNumber = unitNumbers.ElementAt(i),
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
