using System;
using System.Collections.Generic;
using System.Linq;
using AddressesAPI.Infrastructure;
using AddressesAPI.Tests.V2.Helper;
using AddressesAPI.V2;
using AddressesAPI.V2.Domain;
using AddressesAPI.V2.Factories;
using AddressesAPI.V2.Gateways;
using Bogus;
using FluentAssertions;
using NUnit.Framework;
using Address = AddressesAPI.V2.Domain.Address;

namespace AddressesAPI.Tests.V2.Gateways
{
    public class AddressGatewayTest : DatabaseTests
    {
        private AddressesGateway _classUnderTest { get; set; }
        private readonly Faker _faker = new Faker();

        [SetUp]
        public void SetUp()
        {
            _classUnderTest = new AddressesGateway(DatabaseContext);
        }
        #region SearchAddress

        [Test]
        public void ItWillGetADetailedAddressFromTheDatabase()
        {
            var addressKey = _faker.Random.String2(14);
            var savedAddress = TestEfDataHelper.InsertAddressInDb(DatabaseContext, addressKey);
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Format = GlobalConstants.Format.Detailed,
                Gazetteer = GlobalConstants.Gazetteer.Both
            };
            var (addresses, _) = _classUnderTest.SearchAddresses(request);

            addresses.Count.Should().Be(1);
            addresses.First().AddressKey.Should().Be(addressKey);
            addresses.First().Should().BeEquivalentTo(savedAddress.ToDomain());
        }

        [Test]
        public void ItWillGetASimpleAddressFromTheDatabase()
        {
            var addressKey = _faker.Random.String2(14);
            var savedAddress = TestEfDataHelper.InsertAddressInDb(DatabaseContext, addressKey);
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Format = GlobalConstants.Format.Simple,
                Gazetteer = GlobalConstants.Gazetteer.Both
            };
            var (addresses, _) = _classUnderTest.SearchAddresses(request);

            var expectedAddress = new Address
            {
                Line1 = savedAddress.Line1,
                Line2 = savedAddress.Line2,
                Line3 = savedAddress.Line3,
                Line4 = savedAddress.Line4,
                Town = savedAddress.Town,
                UPRN = savedAddress.UPRN,
                Postcode = savedAddress.Postcode
            };

            addresses.Count.Should().Be(1);
            addresses.First().Should().BeEquivalentTo(expectedAddress);
        }

        #region querying

        [TestCase("E8 4TT", "E84TT")]
        [TestCase("E1 4JH", "E1 4JH")]
        [TestCase("E8 4TT", "e8 4tt")]
        [TestCase("E7 4TT", "e7")]
        public void WillSearchPostcodeForAMatch(string savedPostcode, string postcodeSearch)
        {
            var savedAddress = TestEfDataHelper.InsertAddressInDb(DatabaseContext,
                request: new NationalAddress { Postcode = savedPostcode }
                );
            var randomPostcode = $"NW{_faker.Random.Int(1, 9)} {_faker.Random.Int(1, 9)}TY";
            TestEfDataHelper.InsertAddressInDb(DatabaseContext, request: new NationalAddress
            {
                Postcode = randomPostcode
            });
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Format = GlobalConstants.Format.Detailed,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                Postcode = postcodeSearch
            };
            var (addresses, _) = _classUnderTest.SearchAddresses(request);

            addresses.Count.Should().Be(1);
            addresses.First().Should().BeEquivalentTo(savedAddress.ToDomain());
        }

        [TestCase("LE1 3TT", "E1")]
        [TestCase("SW1 7YU", "W1 7")]
        public void WillOnlyGetPostcodesWhichMatchAtTheStart(string savedPostcode, string postcodeSearch)
        {
            var savedAddress = TestEfDataHelper.InsertAddressInDb(DatabaseContext,
                request: new NationalAddress { Postcode = savedPostcode }
            );
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Format = GlobalConstants.Format.Detailed,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                Postcode = postcodeSearch
            };
            var (addresses, _) = _classUnderTest.SearchAddresses(request);

            addresses.Count.Should().Be(0);
        }

        [TestCase("123", "123")]
        [TestCase("123383833", "383")]
        public void WillSearchBuildingsNumbersForAMatch(string savedBuildingNumber, string buildingNumberSearch)
        {
            var savedAddress = TestEfDataHelper.InsertAddressInDb(DatabaseContext,
                request: new NationalAddress { BuildingNumber = savedBuildingNumber }
            );
            TestEfDataHelper.InsertAddressInDb(DatabaseContext);
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Format = GlobalConstants.Format.Detailed,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                BuildingNumber = buildingNumberSearch
            };
            var (addresses, _) = _classUnderTest.SearchAddresses(request);

            addresses.Count.Should().Be(1);
            addresses.First().Should().BeEquivalentTo(savedAddress.ToDomain());
        }

        [TestCase("hackney road", "hackney road")]
        [TestCase("green Road", "green road")]
        [TestCase("yellow street", "YELLOW STREET")]
        [TestCase("yellow street", "YELLOW")]
        [TestCase("yellow street", "YELLOWstreet")]
        public void WillSearchStreetForAMatch(string savedStreet, string streetSearch)
        {
            var savedAddress = TestEfDataHelper.InsertAddressInDb(DatabaseContext,
                request: new NationalAddress { Street = savedStreet }
            );
            TestEfDataHelper.InsertAddressInDb(DatabaseContext);
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Format = GlobalConstants.Format.Detailed,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                Street = streetSearch
            };
            var (addresses, _) = _classUnderTest.SearchAddresses(request);

            addresses.Count.Should().Be(1);
            addresses.First().Should().BeEquivalentTo(savedAddress.ToDomain());
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
        public void WillSearchForAddressesWithStatus(string savedStatus, IEnumerable<string> statusSearchTerm)
        {
            var savedAddress = TestEfDataHelper.InsertAddressInDb(DatabaseContext,
                request: new NationalAddress { AddressStatus = savedStatus }
            );

            TestEfDataHelper.InsertAddressInDb(DatabaseContext, request: new NationalAddress
            {
                AddressStatus = savedStatus == "Provisional" ? "Alternative" : "Provisional"
            });
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Format = GlobalConstants.Format.Detailed,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                AddressStatus = statusSearchTerm
            };
            var (addresses, _) = _classUnderTest.SearchAddresses(request);

            addresses.Count.Should().Be(1);
            addresses.First().Should().BeEquivalentTo(savedAddress.ToDomain());
        }

        [Test]
        public void IfNoAddressStatusSearchGivenDefaultsToApprovedPreferred()
        {
            var savedAddress = TestEfDataHelper.InsertAddressInDb(DatabaseContext);

            TestEfDataHelper.InsertAddressInDb(DatabaseContext, request: new NationalAddress { AddressStatus = "Historical" });
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Format = GlobalConstants.Format.Detailed,
                Gazetteer = GlobalConstants.Gazetteer.Both,
            };
            var (addresses, _) = _classUnderTest.SearchAddresses(request);

            addresses.Count.Should().Be(1);
            addresses.First().Should().BeEquivalentTo(savedAddress.ToDomain());
        }

        [Test]
        public void WillSearchUprnsForAMatch()
        {
            var uprn = _faker.Random.Number(10000000, 99999999);
            var savedAddress = TestEfDataHelper.InsertAddressInDb(DatabaseContext,
                request: new NationalAddress { UPRN = uprn }
            );
            TestEfDataHelper.InsertAddressInDb(DatabaseContext);
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Format = GlobalConstants.Format.Detailed,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                Uprn = uprn
            };
            var (addresses, _) = _classUnderTest.SearchAddresses(request);

            addresses.Count.Should().Be(1);
            addresses.First().Should().BeEquivalentTo(savedAddress.ToDomain());
        }

        [Test]
        public void WillSearchUsrnsForAMatch()
        {
            var uprn = _faker.Random.Number(10000000, 99999999);
            var savedAddress = TestEfDataHelper.InsertAddressInDb(DatabaseContext,
                request: new NationalAddress { USRN = uprn }
            );
            TestEfDataHelper.InsertAddressInDb(DatabaseContext);
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Format = GlobalConstants.Format.Detailed,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                Usrn = uprn
            };
            var (addresses, _) = _classUnderTest.SearchAddresses(request);

            addresses.Count.Should().Be(1);
            addresses.First().Should().BeEquivalentTo(savedAddress.ToDomain());
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
        public void WillSearchForAnAddressesPrimaryUsage(string savedUsage, string usageSearchTerm)
        {
            var savedAddress = TestEfDataHelper.InsertAddressInDb(DatabaseContext,
                request: new NationalAddress { UsagePrimary = savedUsage }
            );
            TestEfDataHelper.InsertAddressInDb(DatabaseContext, request: new NationalAddress
            {
                UsagePrimary = savedUsage == "Commercial" ? "Military" : "Commercial"
            });
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Format = GlobalConstants.Format.Detailed,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                UsagePrimary = usageSearchTerm
            };
            var (addresses, _) = _classUnderTest.SearchAddresses(request);

            addresses.Count.Should().Be(1);
            addresses.First().Should().BeEquivalentTo(savedAddress.ToDomain());
        }

        [TestCase("A1", "A1")]
        [TestCase("A3", "A")]
        [TestCase("C5", "B6,C5,C8")]
        [TestCase("A2", "a2,C5")]
        public void WillSearchForAnAddressesUsageCodeFromAList(string savedUsage, string usageSearchTerm)
        {
            var savedAddress = TestEfDataHelper.InsertAddressInDb(DatabaseContext,
                request: new NationalAddress { UsageCode = savedUsage }
            );

            TestEfDataHelper.InsertAddressInDb(DatabaseContext, request: new NationalAddress
            {
                UsageCode = "B7"
            });
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Format = GlobalConstants.Format.Detailed,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                UsageCode = usageSearchTerm
            };
            var (addresses, _) = _classUnderTest.SearchAddresses(request);

            addresses.Count.Should().Be(1);
            addresses.First().Should().BeEquivalentTo(savedAddress.ToDomain());
        }

        [TestCase("Local")]
        [TestCase("local")]
        [TestCase("Hackney")]
        [TestCase("hackney")]
        public void CanSearchOnlyLocalHackneyAddressesCaseInsensitively(string gazetteer)
        {
            var savedAddress = TestEfDataHelper.InsertAddressInDb(DatabaseContext,
                request: new NationalAddress { Gazetteer = gazetteer }
            );
            TestEfDataHelper.InsertAddressInDb(DatabaseContext,
                request: new NationalAddress { Gazetteer = "National" }
            );
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Format = GlobalConstants.Format.Detailed,
                Gazetteer = GlobalConstants.Gazetteer.Hackney,
            };
            var (addresses, _) = _classUnderTest.SearchAddresses(request);

            addresses.Count.Should().Be(1);
            addresses.First().Should().BeEquivalentTo(savedAddress.ToDomain());
        }

        [Test]
        public void WillFilterHackneyAddressByOutOfBoroughAddresses()
        {
            var outOfBorough = TestEfDataHelper.InsertAddressInDb(DatabaseContext,
                request: new NationalAddress { NeverExport = true, Gazetteer = "Hackney" }
            );
            var hackneyAddress = TestEfDataHelper.InsertAddressInDb(DatabaseContext,
                request: new NationalAddress { NeverExport = false, Gazetteer = "Hackney" }
            );
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Format = GlobalConstants.Format.Detailed,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                OutOfBoroughAddress = false,
            };
            var (addresses, _) = _classUnderTest.SearchAddresses(request);

            addresses.Count.Should().Be(1);
            addresses.First().Should().BeEquivalentTo(hackneyAddress.ToDomain());
        }

        [Test]
        public void WillFilterOutAllNationalAddressesWhenQueryingForNoOutOfBoroughAddresses()
        {
            var outOfBorough1 = TestEfDataHelper.InsertAddressInDb(DatabaseContext,
                request: new NationalAddress { NeverExport = true, Gazetteer = "National" }
            );
            var outOfBorough2 = TestEfDataHelper.InsertAddressInDb(DatabaseContext,
                request: new NationalAddress { NeverExport = false, Gazetteer = "National" }
            );
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Format = GlobalConstants.Format.Detailed,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                OutOfBoroughAddress = false,
            };
            var (addresses, _) = _classUnderTest.SearchAddresses(request);

            addresses.Count.Should().Be(0);
        }

        [Test]
        public void IfOutOfBoroughFlagIsTrueReturnsAllAddresses()
        {
            TestEfDataHelper.InsertAddressInDb(DatabaseContext,
                request: new NationalAddress { NeverExport = true, Gazetteer = "Hackney" }
            );
            TestEfDataHelper.InsertAddressInDb(DatabaseContext,
                request: new NationalAddress { NeverExport = false, Gazetteer = "Hackney" }
            );
            TestEfDataHelper.InsertAddressInDb(DatabaseContext,
                request: new NationalAddress { NeverExport = false, Gazetteer = "National" }
            );
            TestEfDataHelper.InsertAddressInDb(DatabaseContext,
                request: new NationalAddress { NeverExport = true, Gazetteer = "National" }
            );
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Format = GlobalConstants.Format.Detailed,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                OutOfBoroughAddress = true,
            };
            var (addresses, _) = _classUnderTest.SearchAddresses(request);

            addresses.Count.Should().Be(4);
        }

        [Test]
        public void WillSearchForAllAddressesUsingForACrossReference()
        {
            TestEfDataHelper.InsertAddressInDb(DatabaseContext);

            //--- Cross reference for address one
            var uprnOne = _faker.Random.Long(10000000, 99999999);

            var addressOne = TestEfDataHelper.InsertAddressInDb(DatabaseContext,
                request: new NationalAddress { UPRN = uprnOne }
            );

            TestEfDataHelper.InsertCrossReference(DatabaseContext, uprnOne,
                new CrossReference { Code = "TAXES", Value = "TestValue" }
            );

            //--- Cross reference for address two
            var uprnTwo = _faker.Random.Long(10000000, 99999999);

            var addressTwo = TestEfDataHelper.InsertAddressInDb(DatabaseContext,
                request: new NationalAddress { UPRN = uprnTwo }
            );

            TestEfDataHelper.InsertCrossReference(DatabaseContext, uprnTwo,
                new CrossReference { Code = "TAXES", Value = "TestValue" }
            );

            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Format = GlobalConstants.Format.Detailed,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                CrossRefCode = "TAXES",
                CrossRefValue = "TestValue"
            };

            var (addresses, _) = _classUnderTest.SearchAddresses(request);

            addresses.Count.Should().Be(2);
            addresses.Should().ContainEquivalentOf(addressOne.ToDomain());
            addresses.Should().ContainEquivalentOf(addressTwo.ToDomain());
        }
        #endregion

        #region parentShells

        [Test]
        public void IfSetToExcludeParentShellsWillOnlyReturnsResultsOfSearch()
        {
            var parentShell = TestEfDataHelper.InsertAddressInDb(DatabaseContext);
            var addressToMatch = TestEfDataHelper.InsertAddressInDb(DatabaseContext,
                request: new NationalAddress { ParentUPRN = parentShell.UPRN }
            );
            TestEfDataHelper.InsertAddressInDb(DatabaseContext);

            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Format = GlobalConstants.Format.Detailed,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                Postcode = addressToMatch.Postcode,
                IncludeParentShells = false
            };
            var (addresses, _) = _classUnderTest.SearchAddresses(request);

            addresses.Count.Should().Be(1);
            addresses.Should().ContainEquivalentOf(addressToMatch.ToDomain());
        }

        [Test]
        public void IfSetToIncludeParentShellsIncludeImmediateParentShells()
        {
            var parentShell = TestEfDataHelper.InsertAddressInDb(DatabaseContext);
            var addressToMatch = TestEfDataHelper.InsertAddressInDb(DatabaseContext,
                request: new NationalAddress { ParentUPRN = parentShell.UPRN }
            );
            TestEfDataHelper.InsertAddressInDb(DatabaseContext);

            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Format = GlobalConstants.Format.Detailed,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                Postcode = addressToMatch.Postcode,
                IncludeParentShells = true
            };
            var (addresses, _) = _classUnderTest.SearchAddresses(request);

            addresses.Count.Should().Be(2);
            addresses.Should().ContainEquivalentOf(addressToMatch.ToDomain());
            addresses.Should().ContainEquivalentOf(parentShell.ToDomain());
        }

        [Test]
        public void IfSetToIncludeParentShellsWillIncludeParentShellsInTotalCount()
        {
            var parentShell = TestEfDataHelper.InsertAddressInDb(DatabaseContext);
            var addressOneToMatch = TestEfDataHelper.InsertAddressInDb(DatabaseContext,
                request: new NationalAddress { ParentUPRN = parentShell.UPRN }
            );
            var parentShellTwo = TestEfDataHelper.InsertAddressInDb(DatabaseContext);
            var addressTwoToMatch = TestEfDataHelper.InsertAddressInDb(DatabaseContext,
                request: new NationalAddress { ParentUPRN = parentShellTwo.UPRN, Postcode = addressOneToMatch.Postcode }
            );

            TestEfDataHelper.InsertAddressInDb(DatabaseContext);

            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 2,
                Format = GlobalConstants.Format.Detailed,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                Postcode = addressOneToMatch.Postcode,
                IncludeParentShells = true
            };
            var (addresses, totalCount) = _classUnderTest.SearchAddresses(request);

            addresses.Count.Should().Be(2);
            totalCount.Should().Be(4);
        }

        [Test]
        public void IfSetToIncludeParentShellsIncludeParentShellsOfParentsShells()
        {
            var grandParentShell = TestEfDataHelper.InsertAddressInDb(DatabaseContext);
            var parentShell = TestEfDataHelper.InsertAddressInDb(DatabaseContext,
                request: new NationalAddress { ParentUPRN = grandParentShell.UPRN });
            var addressToMatch = TestEfDataHelper.InsertAddressInDb(DatabaseContext,
                request: new NationalAddress { ParentUPRN = parentShell.UPRN }
            );
            TestEfDataHelper.InsertAddressInDb(DatabaseContext);

            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Format = GlobalConstants.Format.Detailed,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                Postcode = addressToMatch.Postcode,
                IncludeParentShells = true
            };
            var (addresses, _) = _classUnderTest.SearchAddresses(request);

            addresses.Count.Should().Be(3);
            addresses.Should().ContainEquivalentOf(addressToMatch.ToDomain());
            addresses.Should().ContainEquivalentOf(parentShell.ToDomain());
            addresses.Should().ContainEquivalentOf(grandParentShell.ToDomain());
        }

        [TestCase("E8 7JH")]
        [TestCase("N1 5TH")]
        public void IfSetToExcludeParentShellsWillNotReturnAnyParentShells(string postcode)
        {
            var parentShell = TestEfDataHelper.InsertAddressInDb(DatabaseContext,
                request: new NationalAddress { PropertyShell = true, Postcode = postcode }
            );
            var notAParentShell = TestEfDataHelper.InsertAddressInDb(DatabaseContext,
                request: new NationalAddress { PropertyShell = false, Postcode = postcode }
            );

            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Format = GlobalConstants.Format.Detailed,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                Postcode = postcode,
                IncludeParentShells = false
            };
            var (addresses, _) = _classUnderTest.SearchAddresses(request);

            addresses.Count.Should().Be(1);
            addresses.Should().ContainEquivalentOf(notAParentShell.ToDomain());
        }

        #endregion

        #region ordering

        [Test]
        public void WillFirstlyOrderByTown()
        {
            var addressOne = TestEfDataHelper.InsertAddressInDb(DatabaseContext, request: new NationalAddress { Town = "town a" });
            var addressTwo = TestEfDataHelper.InsertAddressInDb(DatabaseContext, request: new NationalAddress { Town = "town b" });
            var addressThree = TestEfDataHelper.InsertAddressInDb(DatabaseContext, request: new NationalAddress { Town = "hackney" });

            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Format = GlobalConstants.Format.Detailed,
                Gazetteer = GlobalConstants.Gazetteer.Both,
            };
            var (addresses, _) = _classUnderTest.SearchAddresses(request);

            addresses.Count.Should().Be(3);
            addresses.ElementAt(0).Should().BeEquivalentTo(addressThree.ToDomain());
            addresses.ElementAt(1).Should().BeEquivalentTo(addressOne.ToDomain());
            addresses.ElementAt(2).Should().BeEquivalentTo(addressTwo.ToDomain());
        }

        [Test]
        public void WillSecondlyOrderByPostcodePresence()
        {
            var addressOne = TestEfDataHelper.InsertAddressInDb(DatabaseContext, request: new NationalAddress { Town = "town a", Postcode = "" });
            var addressTwo = TestEfDataHelper.InsertAddressInDb(DatabaseContext, request: new NationalAddress { Town = "town a", Postcode = "E3 4TT" });
            var addressThree = TestEfDataHelper.InsertAddressInDb(DatabaseContext, request: new NationalAddress { Town = "town b" });

            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Format = GlobalConstants.Format.Detailed,
                Gazetteer = GlobalConstants.Gazetteer.Both,
            };
            var (addresses, _) = _classUnderTest.SearchAddresses(request);

            addresses.Count.Should().Be(3);
            addresses.ElementAt(0).Should().BeEquivalentTo(addressTwo.ToDomain());
            addresses.ElementAt(1).Should().BeEquivalentTo(addressOne.ToDomain());
            addresses.ElementAt(2).Should().BeEquivalentTo(addressThree.ToDomain());
        }

        [Test]
        public void WillThirdlyOrderByStreet()
        {
            var addressOne = TestEfDataHelper.InsertAddressInDb(DatabaseContext, request: new NationalAddress { Town = "town b" });
            var addressTwo = TestEfDataHelper.InsertAddressInDb(DatabaseContext, request: new NationalAddress { Town = "town a", Postcode = "", Street = "B Street" });
            var addressThree = TestEfDataHelper.InsertAddressInDb(DatabaseContext, request: new NationalAddress { Town = "town a", Postcode = "", Street = "A Street" });
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Format = GlobalConstants.Format.Detailed,
                Gazetteer = GlobalConstants.Gazetteer.Both,
            };
            var (addresses, _) = _classUnderTest.SearchAddresses(request);

            addresses.Count.Should().Be(3);
            addresses.ElementAt(0).Should().BeEquivalentTo(addressThree.ToDomain());
            addresses.ElementAt(1).Should().BeEquivalentTo(addressTwo.ToDomain());
            addresses.ElementAt(2).Should().BeEquivalentTo(addressOne.ToDomain());
        }

        [Test]
        public void WillFourthlyOrderByPresenceAndOrderOfPaonStartNumber()
        {
            var addressOne = TestEfDataHelper.InsertAddressInDb(DatabaseContext, request: new NationalAddress
            {
                Town = "town a",
                Postcode = "",
                Street = "B Street",
                PaonStartNumber = 3
            });
            var addressTwo = TestEfDataHelper.InsertAddressInDb(DatabaseContext, request: new NationalAddress
            {
                Town = "town a",
                Postcode = "",
                Street = "B Street",
                PaonStartNumber = 0
            });
            var addressThree = TestEfDataHelper.InsertAddressInDb(DatabaseContext, request: new NationalAddress
            {
                Town = "town a",
                Postcode = "",
                Street = "B Street",
                PaonStartNumber = 5
            });
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Format = GlobalConstants.Format.Detailed,
                Gazetteer = GlobalConstants.Gazetteer.Both,
            };
            var (addresses, _) = _classUnderTest.SearchAddresses(request);

            addresses.Count.Should().Be(3);
            addresses.ElementAt(0).Should().BeEquivalentTo(addressOne.ToDomain());
            addresses.ElementAt(1).Should().BeEquivalentTo(addressThree.ToDomain());
            addresses.ElementAt(2).Should().BeEquivalentTo(addressTwo.ToDomain());
        }

        [Test]
        public void WillFifthlyOrderByPresenceAndOrderOfBuildingNumber()
        {
            var addressOne = TestEfDataHelper.InsertAddressInDb(DatabaseContext, request: new NationalAddress
            {
                Town = "town a",
                Postcode = "",
                Street = "B Street",
                PaonStartNumber = 1,
                BuildingNumber = ""
            });
            var addressTwo = TestEfDataHelper.InsertAddressInDb(DatabaseContext, request: new NationalAddress
            {
                Town = "town a",
                Postcode = "",
                Street = "B Street",
                PaonStartNumber = 1,
                BuildingNumber = "78"
            });
            var addressThree = TestEfDataHelper.InsertAddressInDb(DatabaseContext, request: new NationalAddress
            {
                Town = "town a",
                Postcode = "",
                Street = "B Street",
                PaonStartNumber = 1,
                BuildingNumber = "99"
            });
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Format = GlobalConstants.Format.Detailed,
                Gazetteer = GlobalConstants.Gazetteer.Both,
            };
            var (addresses, _) = _classUnderTest.SearchAddresses(request);

            addresses.Count.Should().Be(3);
            addresses.ElementAt(0).Should().BeEquivalentTo(addressTwo.ToDomain());
            addresses.ElementAt(1).Should().BeEquivalentTo(addressThree.ToDomain());
            addresses.ElementAt(2).Should().BeEquivalentTo(addressOne.ToDomain());
        }

        [Test]
        public void WillSixthOrderByPresenceAndOrderOfUnitNumber()
        {
            var addressOne = TestEfDataHelper.InsertAddressInDb(DatabaseContext, request: new NationalAddress
            {
                Town = "town a",
                Postcode = "",
                Street = "B Street",
                PaonStartNumber = 1,
                BuildingNumber = "78",
                UnitNumber = "43"
            });
            var addressTwo = TestEfDataHelper.InsertAddressInDb(DatabaseContext, request: new NationalAddress
            {
                Town = "town a",
                Postcode = "",
                Street = "B Street",
                PaonStartNumber = 1,
                BuildingNumber = "78",
                UnitNumber = ""
            });
            var addressThree = TestEfDataHelper.InsertAddressInDb(DatabaseContext, request: new NationalAddress
            {
                Town = "town a",
                Postcode = "",
                Street = "B Street",
                PaonStartNumber = 1,
                BuildingNumber = "78",
                UnitNumber = "23"
            });
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Format = GlobalConstants.Format.Detailed,
                Gazetteer = GlobalConstants.Gazetteer.Both,
            };
            var (addresses, _) = _classUnderTest.SearchAddresses(request);

            addresses.Count.Should().Be(3);
            addresses.ElementAt(0).Should().BeEquivalentTo(addressThree.ToDomain());
            addresses.ElementAt(1).Should().BeEquivalentTo(addressOne.ToDomain());
            addresses.ElementAt(2).Should().BeEquivalentTo(addressTwo.ToDomain());
        }

        [Test]
        public void WillInTheSeventhCaseOrderByPresenceAndOrderOfUnitName()
        {
            var addressOne = TestEfDataHelper.InsertAddressInDb(DatabaseContext, request: new NationalAddress
            {
                Town = "town a",
                Postcode = "",
                Street = "B Street",
                PaonStartNumber = 1,
                BuildingNumber = "78",
                UnitNumber = "43",
                UnitName = "J name"
            });
            var addressTwo = TestEfDataHelper.InsertAddressInDb(DatabaseContext, request: new NationalAddress
            {
                Town = "town a",
                Postcode = "",
                Street = "B Street",
                PaonStartNumber = 1,
                BuildingNumber = "78",
                UnitNumber = "43",
                UnitName = "A name"
            });
            var addressThree = TestEfDataHelper.InsertAddressInDb(DatabaseContext, request: new NationalAddress
            {
                Town = "town a",
                Postcode = "",
                Street = "B Street",
                PaonStartNumber = 1,
                BuildingNumber = "78",
                UnitNumber = "43",
                UnitName = ""
            });
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Format = GlobalConstants.Format.Detailed,
                Gazetteer = GlobalConstants.Gazetteer.Both,
            };
            var (addresses, _) = _classUnderTest.SearchAddresses(request);

            addresses.Count.Should().Be(3);
            addresses.ElementAt(0).Should().BeEquivalentTo(addressTwo.ToDomain());
            addresses.ElementAt(1).Should().BeEquivalentTo(addressOne.ToDomain());
            addresses.ElementAt(2).Should().BeEquivalentTo(addressThree.ToDomain());
        }
        #endregion

        #region pagination

        [Test]
        public void ItWillReturnTheNumberOfRecordsRequestedInPageSize()
        {
            AddOrderedRecordsToDatabase(3);

            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 2,
                Format = GlobalConstants.Format.Detailed,
                Gazetteer = GlobalConstants.Gazetteer.Both
            };

            var (addresses, _) = _classUnderTest.SearchAddresses(request);

            addresses.Count.Should().Be(2);
        }

        [Test]
        public void ItWillReturnASecondPageOfResults()
        {
            var records = AddOrderedRecordsToDatabase(10);

            var request = new SearchParameters
            {
                Page = 2,
                PageSize = 3,
                Format = GlobalConstants.Format.Detailed,
                Gazetteer = GlobalConstants.Gazetteer.Both
            };

            var (addresses, _) = _classUnderTest.SearchAddresses(request);

            addresses.Count.Should().Be(3);
            addresses.Should().ContainEquivalentOf(records.ElementAt(3).ToDomain());
            addresses.Should().ContainEquivalentOf(records.ElementAt(4).ToDomain());
            addresses.Should().ContainEquivalentOf(records.ElementAt(5).ToDomain());
        }

        [Test]
        public void ItWillReturnAThirdPageOfResults()
        {
            var records = AddOrderedRecordsToDatabase(13);

            var request = new SearchParameters
            {
                Page = 3,
                PageSize = 4,
                Format = GlobalConstants.Format.Detailed,
                Gazetteer = GlobalConstants.Gazetteer.Both
            };

            var (addresses, _) = _classUnderTest.SearchAddresses(request);

            addresses.Count.Should().Be(4);
            addresses.Should().ContainEquivalentOf(records.ElementAt(8).ToDomain());
            addresses.Should().ContainEquivalentOf(records.ElementAt(9).ToDomain());
            addresses.Should().ContainEquivalentOf(records.ElementAt(10).ToDomain());
            addresses.Should().ContainEquivalentOf(records.ElementAt(11).ToDomain());
        }

        [Test]
        public void ItWillAcceptZeroAsTheFirstPage()
        {
            var records = AddOrderedRecordsToDatabase(13);

            var request = new SearchParameters
            {
                Page = 0,
                PageSize = 4,
                Format = GlobalConstants.Format.Detailed,
                Gazetteer = GlobalConstants.Gazetteer.Both
            };

            var (addresses, _) = _classUnderTest.SearchAddresses(request);

            addresses.Count.Should().Be(4);
            addresses.Should().ContainEquivalentOf(records.ElementAt(0).ToDomain());
            addresses.Should().ContainEquivalentOf(records.ElementAt(1).ToDomain());
            addresses.Should().ContainEquivalentOf(records.ElementAt(2).ToDomain());
            addresses.Should().ContainEquivalentOf(records.ElementAt(3).ToDomain());
        }

        private List<NationalAddress> AddOrderedRecordsToDatabase(int count)
        {
            var towns = GenerateRandomOrderedListOfWordsOfMaxLength(count, 100);
            var postcodes = GenerateRandomOrderedListOfWordsOfMaxLength(count, 8);
            var streets = GenerateRandomOrderedListOfWordsOfMaxLength(count, 100);
            var buildingNumbers = GenerateRandomOrderedListOfWordsOfMaxLength(count, 17);
            var unitNumbers = GenerateRandomOrderedListOfWordsOfMaxLength(count, 17);
            var unitNames = GenerateRandomOrderedListOfWordsOfMaxLength(count, 90);
            var records = new List<NationalAddress>();
            for (var i = 0; i < count; i++)
            {
                records.Add(TestEfDataHelper.InsertAddressInDb(DatabaseContext, request: new NationalAddress
                {
                    Town = towns.ElementAt(i),
                    Postcode = postcodes.ElementAt(i),
                    Street = streets.ElementAt(i),
                    BuildingNumber = buildingNumbers.ElementAt(i),
                    UnitNumber = unitNumbers.ElementAt(i),
                    UnitName = unitNames.ElementAt(i),
                    PaonStartNumber = Convert.ToInt16($"{i}{_faker.Random.Int(10, 99)}")
                }));
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
        public void ItWillReturnTheTotalCountIfThereIsOnlyOnePageOfRecords(int count)
        {
            AddOrderedRecordsToDatabase(count);
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = count + 1,
                Format = GlobalConstants.Format.Detailed,
                Gazetteer = GlobalConstants.Gazetteer.Both
            };
            var (addresses, totalCount) = _classUnderTest.SearchAddresses(request);

            addresses.Count.Should().Be(count);
            totalCount.Should().Be(count);
        }

        [TestCase(20)]
        [TestCase(7)]
        public void ItWillReturnTheTotalCountIfThereAreMultiplePagesOfRecords(int count)
        {
            AddOrderedRecordsToDatabase(count);
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = count - 5,
                Format = GlobalConstants.Format.Detailed,
                Gazetteer = GlobalConstants.Gazetteer.Both
            };
            var (addresses, totalCount) = _classUnderTest.SearchAddresses(request);

            addresses.Count.Should().Be(count - 5);
            totalCount.Should().Be(count);
        }

        #endregion
        #endregion

        #region GetAddresses

        [Test]
        public void GivenAnAddressKey_GetAddresses_GetsTheAddressDetails()
        {
            var addressKey = _faker.Random.String2(14);
            var savedAddress = TestEfDataHelper.InsertAddressInDb(DatabaseContext, addressKey);

            var addresses = _classUnderTest.GetAddresses(new List<string>{addressKey}, GlobalConstants.Format.Detailed);

            addresses.Count.Should().Be(1);
            addresses.First().AddressKey.Should().Be(addressKey);
            addresses.First().Should().BeEquivalentTo(savedAddress.ToDomain());
        }

        [Test]
        public void GivenAnAddressKey_GetAddresses_OnlyGetsAddressesForTheKeys()
        {
            var addressKey = _faker.Random.String2(14);
            var savedAddress = TestEfDataHelper.InsertAddressInDb(DatabaseContext, addressKey);
            TestEfDataHelper.InsertAddressInDb(DatabaseContext);

            var addresses = _classUnderTest.GetAddresses(new List<string>{addressKey}, GlobalConstants.Format.Detailed);

            addresses.Count.Should().Be(1);
            addresses.First().AddressKey.Should().Be(addressKey);
            addresses.First().Should().BeEquivalentTo(savedAddress.ToDomain());
        }

        [Test]
        public void GivenSomeAddressKeys_GetAddresses_RetainsTheOrderOfAddresses()
        {
            var addressKeys = new List<string>
            {
                _faker.Random.String2(14), _faker.Random.String2(14), _faker.Random.String2(14)
            };
            var thirdAddress = TestEfDataHelper.InsertAddressInDb(DatabaseContext, addressKeys.ElementAt(2));
            var firstAddress = TestEfDataHelper.InsertAddressInDb(DatabaseContext, addressKeys.ElementAt(0));
            var secondAddress = TestEfDataHelper.InsertAddressInDb(DatabaseContext, addressKeys.ElementAt(1));

            var addresses = _classUnderTest.GetAddresses(addressKeys, GlobalConstants.Format.Detailed);

            addresses.ElementAt(0).Should().BeEquivalentTo(firstAddress.ToDomain());
            addresses.ElementAt(1).Should().BeEquivalentTo(secondAddress.ToDomain());
            addresses.ElementAt(2).Should().BeEquivalentTo(thirdAddress.ToDomain());
        }


        [Test]
        public void GetAddresses_WillGetADetailedAddressFromTheDatabase()
        {
            var addressKey = _faker.Random.String2(14);
            var savedAddress = TestEfDataHelper.InsertAddressInDb(DatabaseContext, addressKey);

            var addresses = _classUnderTest.GetAddresses(new List<string>{addressKey}, GlobalConstants.Format.Detailed);

            addresses.Count.Should().Be(1);
            addresses.First().AddressKey.Should().Be(addressKey);
            addresses.First().Should().BeEquivalentTo(savedAddress.ToDomain());
        }

        [Test]
        public void GetAddresses_WillGetASimpleAddressFromTheDatabase()
        {
            var addressKey = _faker.Random.String2(14);
            var savedAddress = TestEfDataHelper.InsertAddressInDb(DatabaseContext, addressKey);

            var addresses = _classUnderTest.GetAddresses(new List<string>{addressKey}, GlobalConstants.Format.Simple);

            var expectedAddress = new Address
            {
                Line1 = savedAddress.Line1,
                Line2 = savedAddress.Line2,
                Line3 = savedAddress.Line3,
                Line4 = savedAddress.Line4,
                Town = savedAddress.Town,
                UPRN = savedAddress.UPRN,
                Postcode = savedAddress.Postcode
            };

            addresses.Count.Should().Be(1);
            addresses.First().Should().BeEquivalentTo(expectedAddress);
        }

        #endregion
        #region GetSingleAddress

        [Test]
        public void ItWillReturnADetailedRecordForASingleAddressRetrievedUsingTheAddressKey()
        {
            var addressKey = _faker.Random.String2(14);
            var savedAddress = TestEfDataHelper.InsertAddressInDb(DatabaseContext, addressKey);

            var retrievedRecord = _classUnderTest.GetSingleAddress(addressKey);

            retrievedRecord.Should().NotBeNull();
            retrievedRecord.Should().BeEquivalentTo(savedAddress.ToDomain());
        }

        [Test]
        public void ItWillReturnNullIfAddressWithAMatchingKeyDoesNotExistInTheDatabase()
        {
            var addressKey = _faker.Random.String2(14);

            _classUnderTest.GetSingleAddress(addressKey).Should().BeNull();
        }

        #endregion
    }
}
