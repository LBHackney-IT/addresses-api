using AddressesAPI.Infrastructure;
using AddressesAPI.Tests.V1.Helper;
using AddressesAPI.V1;
using AddressesAPI.V1.Domain;
using AddressesAPI.V1.Factories;
using AddressesAPI.V1.Gateways;
using Bogus;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Address = AddressesAPI.V1.Domain.Address;

namespace AddressesAPI.Tests.V1.Gateways
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
            var savedAddress = TestEfDataHelper.InsertAddress(DatabaseContext, addressKey);
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
            var savedAddress = TestEfDataHelper.InsertAddress(DatabaseContext, addressKey);
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
            var savedAddress = TestEfDataHelper.InsertAddress(DatabaseContext,
                request: new NationalAddress { Postcode = savedPostcode }
                );
            var randomPostcode = $"NW{_faker.Random.Int(1, 9)} {_faker.Random.Int(1, 9)}TY";
            TestEfDataHelper.InsertAddress(DatabaseContext, request: new NationalAddress
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
            var savedAddress = TestEfDataHelper.InsertAddress(DatabaseContext,
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
            var savedAddress = TestEfDataHelper.InsertAddress(DatabaseContext,
                request: new NationalAddress { BuildingNumber = savedBuildingNumber }
            );
            TestEfDataHelper.InsertAddress(DatabaseContext);
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
            var savedAddress = TestEfDataHelper.InsertAddress(DatabaseContext,
                request: new NationalAddress { Street = savedStreet }
            );
            TestEfDataHelper.InsertAddress(DatabaseContext);
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

        [TestCase("Alternative", "Alternative")]
        [TestCase("Approved Preferred", "Approved Preferred")]
        [TestCase("Historical", "Historical")]
        [TestCase("Provisional", "Provisional")]
        [TestCase("Alternative", "Alternative,Approved Preferred")]
        [TestCase("alternative", "Alternative,Approved Preferred")]
        [TestCase("Historical", "Alternative,Approved Preferred,Historical")]
        [TestCase("Provisional", "Historical,Provisional")]
        [TestCase("Provisional", "Historical,provisional")]
        public void WillSearchForAddressesWithStatus(string savedStatus, string statusSearchTerm)
        {
            var savedAddress = TestEfDataHelper.InsertAddress(DatabaseContext,
                request: new NationalAddress { AddressStatus = savedStatus }
            );

            TestEfDataHelper.InsertAddress(DatabaseContext, request: new NationalAddress
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
            var savedAddress = TestEfDataHelper.InsertAddress(DatabaseContext);

            TestEfDataHelper.InsertAddress(DatabaseContext, request: new NationalAddress { AddressStatus = "Historical" });
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
            var savedAddress = TestEfDataHelper.InsertAddress(DatabaseContext,
                request: new NationalAddress { UPRN = uprn }
            );
            TestEfDataHelper.InsertAddress(DatabaseContext);
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
        [Ignore("Disabled until we have optimised tables for parent UPRN queries")]
        public void WillSearchParentUPRNsForAMatch()
        {
            var matchingAddress = TestEfDataHelper.InsertAddress(DatabaseContext,
                request: new NationalAddress { ParentUPRN = 11111111 }
            );

            var otherAddress = TestEfDataHelper.InsertAddress(DatabaseContext,
               request: new NationalAddress { ParentUPRN = 22222222 }
           );

            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Format = GlobalConstants.Format.Detailed,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                ParentUprn = matchingAddress.ParentUPRN
            };

            var (addresses, _) = _classUnderTest.SearchAddresses(request);
            addresses.Count.Should().Be(1);
            addresses.First().Should().BeEquivalentTo(matchingAddress.ToDomain());
        }

        [Test]
        public void WillSearchUsrnsForAMatch()
        {
            var uprn = _faker.Random.Number(10000000, 99999999);
            var savedAddress = TestEfDataHelper.InsertAddress(DatabaseContext,
                request: new NationalAddress { USRN = uprn }
            );
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
            var savedAddress = TestEfDataHelper.InsertAddress(DatabaseContext,
                request: new NationalAddress { UsagePrimary = savedUsage }
            );
            TestEfDataHelper.InsertAddress(DatabaseContext, request: new NationalAddress
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

        [Test]
        public void WontFilterByParentShellWhenSearchingByPrimaryUsage()
        {
            TestEfDataHelper.InsertAddress(DatabaseContext,
                request: new NationalAddress { UsagePrimary = "Parent Shell" }
            );
            TestEfDataHelper.InsertAddress(DatabaseContext, request: new NationalAddress
            {
                UsagePrimary = "Commercial"
            });
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Format = GlobalConstants.Format.Detailed,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                UsagePrimary = "Parent Shell"
            };
            var (addresses, _) = _classUnderTest.SearchAddresses(request);

            addresses.Count.Should().Be(2);
        }

        [TestCase("A1", "A1")]
        [TestCase("A3", "A")]
        [TestCase("C5", "B6,C5,C8")]
        [TestCase("A2", "a2,C5")]
        public void WillSearchForAnAddressesUsageCodeFromAList(string savedUsage, string usageSearchTerm)
        {
            var savedAddress = TestEfDataHelper.InsertAddress(DatabaseContext,
                request: new NationalAddress { UsageCode = savedUsage }
            );

            TestEfDataHelper.InsertAddress(DatabaseContext, request: new NationalAddress
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


        [TestCase("Hackney")]
        [TestCase("hackney")]
        public void CanSearchOnlyLocalHackneyAddressesCaseInsensitively(string gazetteerDatabaseValue)
        {
            var savedAddress = TestEfDataHelper.InsertAddress(DatabaseContext,
                request: new NationalAddress { Gazetteer = gazetteerDatabaseValue }
            );
            TestEfDataHelper.InsertAddress(DatabaseContext,
                request: new NationalAddress { Gazetteer = "National" }
            );
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Format = GlobalConstants.Format.Detailed,
                Gazetteer = GlobalConstants.Gazetteer.Local,
            };
            var (addresses, _) = _classUnderTest.SearchAddresses(request);

            addresses.Count.Should().Be(1);
            addresses.First().Should().BeEquivalentTo(savedAddress.ToDomain());
        }

        [TestCase(true)]
        [TestCase(false)]
        public void WillFilterByOutOfBoroughAddresses(bool outOfBoroughAddressFlag)
        {
            var savedAddress = TestEfDataHelper.InsertAddress(DatabaseContext,
                request: new NationalAddress { NeverExport = outOfBoroughAddressFlag }
            );
            TestEfDataHelper.InsertAddress(DatabaseContext,
                request: new NationalAddress { NeverExport = !outOfBoroughAddressFlag }
            );
            var request = new SearchParameters
            {
                Page = 1,
                PageSize = 50,
                Format = GlobalConstants.Format.Detailed,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                HackneyGazetteerOutOfBoroughAddress = outOfBoroughAddressFlag,
            };
            var (addresses, _) = _classUnderTest.SearchAddresses(request);

            addresses.Count.Should().Be(1);
            addresses.First().Should().BeEquivalentTo(savedAddress.ToDomain());
        }
        #endregion

        #region ordering

        [Test]
        public void WillFirstlyOrderByTown()
        {
            var addressOne = TestEfDataHelper.InsertAddress(DatabaseContext, request: new NationalAddress { Town = "town a" });
            var addressTwo = TestEfDataHelper.InsertAddress(DatabaseContext, request: new NationalAddress { Town = "town b" });
            var addressThree = TestEfDataHelper.InsertAddress(DatabaseContext, request: new NationalAddress { Town = "hackney" });

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
            var addressOne = TestEfDataHelper.InsertAddress(DatabaseContext, request: new NationalAddress { Town = "town a", Postcode = "" });
            var addressTwo = TestEfDataHelper.InsertAddress(DatabaseContext, request: new NationalAddress { Town = "town a", Postcode = "E3 4TT" });
            var addressThree = TestEfDataHelper.InsertAddress(DatabaseContext, request: new NationalAddress { Town = "town b" });

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
            var addressOne = TestEfDataHelper.InsertAddress(DatabaseContext, request: new NationalAddress { Town = "town b" });
            var addressTwo = TestEfDataHelper.InsertAddress(DatabaseContext, request: new NationalAddress { Town = "town a", Postcode = "", Street = "B Street" });
            var addressThree = TestEfDataHelper.InsertAddress(DatabaseContext, request: new NationalAddress { Town = "town a", Postcode = "", Street = "A Street" });
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
            var addressOne = TestEfDataHelper.InsertAddress(DatabaseContext, request: new NationalAddress
            {
                Town = "town a",
                Postcode = "",
                Street = "B Street",
                PaonStartNumber = 3
            });
            var addressTwo = TestEfDataHelper.InsertAddress(DatabaseContext, request: new NationalAddress
            {
                Town = "town a",
                Postcode = "",
                Street = "B Street",
                PaonStartNumber = 0
            });
            var addressThree = TestEfDataHelper.InsertAddress(DatabaseContext, request: new NationalAddress
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
            var addressOne = TestEfDataHelper.InsertAddress(DatabaseContext, request: new NationalAddress
            {
                Town = "town a",
                Postcode = "",
                Street = "B Street",
                PaonStartNumber = 1,
                BuildingNumber = ""
            });
            var addressTwo = TestEfDataHelper.InsertAddress(DatabaseContext, request: new NationalAddress
            {
                Town = "town a",
                Postcode = "",
                Street = "B Street",
                PaonStartNumber = 1,
                BuildingNumber = "78"
            });
            var addressThree = TestEfDataHelper.InsertAddress(DatabaseContext, request: new NationalAddress
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
            var addressOne = TestEfDataHelper.InsertAddress(DatabaseContext, request: new NationalAddress
            {
                Town = "town a",
                Postcode = "",
                Street = "B Street",
                PaonStartNumber = 1,
                BuildingNumber = "78",
                UnitNumber = 43
            });
            var addressTwo = TestEfDataHelper.InsertAddress(DatabaseContext, request: new NationalAddress
            {
                Town = "town a",
                Postcode = "",
                Street = "B Street",
                PaonStartNumber = 1,
                BuildingNumber = "78",
                UnitNumber = 0
            });
            var addressThree = TestEfDataHelper.InsertAddress(DatabaseContext, request: new NationalAddress
            {
                Town = "town a",
                Postcode = "",
                Street = "B Street",
                PaonStartNumber = 1,
                BuildingNumber = "78",
                UnitNumber = 23
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
            var addressOne = TestEfDataHelper.InsertAddress(DatabaseContext, request: new NationalAddress
            {
                Town = "town a",
                Postcode = "",
                Street = "B Street",
                PaonStartNumber = 1,
                BuildingNumber = "78",
                UnitNumber = 43,
                UnitName = "J name"
            });
            var addressTwo = TestEfDataHelper.InsertAddress(DatabaseContext, request: new NationalAddress
            {
                Town = "town a",
                Postcode = "",
                Street = "B Street",
                PaonStartNumber = 1,
                BuildingNumber = "78",
                UnitNumber = 43,
                UnitName = "A name"
            });
            var addressThree = TestEfDataHelper.InsertAddress(DatabaseContext, request: new NationalAddress
            {
                Town = "town a",
                Postcode = "",
                Street = "B Street",
                PaonStartNumber = 1,
                BuildingNumber = "78",
                UnitNumber = 43,
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
                records.Add(TestEfDataHelper.InsertAddress(DatabaseContext, request: new NationalAddress
                {
                    Town = towns.ElementAt(i),
                    Postcode = postcodes.ElementAt(i),
                    Street = streets.ElementAt(i),
                    BuildingNumber = buildingNumbers.ElementAt(i),
                    UnitNumber = Convert.ToInt16($"{i}{_faker.Random.Int(10, 99)}"),
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

        #region GetSingleAddress

        [Test]
        public void ItWillReturnADetailedRecordForASingleAddressRetrievedUsingTheAddressKey()
        {
            var addressKey = _faker.Random.String2(14);
            var savedAddress = TestEfDataHelper.InsertAddress(DatabaseContext, addressKey);

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

        #region PropertyHierarchy
        [Test]
        public void ItWillReturnPropertyHierarchyWhenBuildHierarchyFlagIsTrue()
        {
            var parentUprn = _faker.Random.Number(10000000, 99999999);

            var parentAddress = new NationalAddress()
            {
                UPRN = parentUprn,
                ParentUPRN = null
            };

            var childAddressOne = new NationalAddress()
            {
                ParentUPRN = parentAddress.UPRN
            };

            var childAddressTwo = new NationalAddress()
            {
                ParentUPRN = parentAddress.UPRN
            };

            TestEfDataHelper.InsertAddress(DatabaseContext, request: parentAddress);
            TestEfDataHelper.InsertAddress(DatabaseContext, request: childAddressOne);
            TestEfDataHelper.InsertAddress(DatabaseContext, request: childAddressTwo);

            var request = new SearchParameters()
            {
                Gazetteer = GlobalConstants.Gazetteer.Both,
                Structure = GlobalConstants.Structure.Hierarchy
            };

            var (addresses, totalCount) = _classUnderTest.SearchAddresses(request);

            addresses.Count.Should().Be(1);
            totalCount.Should().Be(3);
            addresses.First().UPRN.Should().Be(parentAddress.UPRN);
            addresses.First().ParentUPRN.Should().BeNull();
            addresses.First().ChildAddresses.Count.Should().Be(2);
            _ = addresses.First().ChildAddresses.All(x => x.ChildAddresses == null);
            _ = addresses.First().ChildAddresses.All(x => x.ParentUPRN == parentAddress.UPRN);
        }
        //TODO: test multiple levels

        [Test]
        public void ItWillIncludeParentRecordsToTheHierarchyEvenIfTheyAreNotIncludedInTheOriginalResultsSet()
        {
            //Missing parents when searching by postcode
            //for some addresses the parent address may not have a postcode specified,
            //leading to a situation where an address has a parentUPRN attribute that points to a UPRN that is not is the result set.
            //To build the hierarchy these missing parents must be identified and added to the result set by making additional database calls to fetch them by UPRN.
            var parentUprn = _faker.Random.Number(10000000, 99999999);
            var postCode = "E8 4TT";

            var parentAddressOutsideResultSet = new NationalAddress()
            {
                UPRN = parentUprn,
                ParentUPRN = null,
                Postcode = null
            };

            var childAddressOne = new NationalAddress()
            {
                ParentUPRN = parentAddressOutsideResultSet.UPRN,
                UPRN = _faker.Random.Number(10000000, 99999999),
                Postcode = postCode
            };

            var childAddressTwo = new NationalAddress()
            {
                ParentUPRN = parentAddressOutsideResultSet.UPRN,
                UPRN = _faker.Random.Number(10000000, 99999999),
                Postcode = postCode
            };

            TestEfDataHelper.InsertAddress(DatabaseContext, request: parentAddressOutsideResultSet);
            TestEfDataHelper.InsertAddress(DatabaseContext, request: childAddressOne);
            TestEfDataHelper.InsertAddress(DatabaseContext, request: childAddressTwo);

            var request = new SearchParameters()
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                Structure = GlobalConstants.Structure.Hierarchy,
                Postcode = postCode
            };

            var (addresses, totalCount) = _classUnderTest.SearchAddresses(request);

            addresses.Count.Should().Be(1);
            totalCount.Should().Be(3);
            addresses.First().UPRN.Should().Be(parentAddressOutsideResultSet.UPRN);
            addresses.First().ParentUPRN.Should().BeNull();
            addresses.First().ChildAddresses.Count.Should().Be(2);
            _ = addresses.First().ChildAddresses.All(x => x.ChildAddresses == null);
            _ = addresses.First().ChildAddresses.All(x => x.ParentUPRN == parentAddressOutsideResultSet.UPRN);
        }

        //Parents with children in another postcode.
        //Sometimes the children of a parent can be split across 2 postcodes.
        //In order to get all the children for the parent an additional search must be made
        // to find all the addresses that have a parentUPRN matching any of the parentUPRNs in the original search
        // and combining the results before building the hierarchy.

        [Test]
        [Ignore("Disabled until we have optimised tables for parent UPRN queries")]
        public void ItWillIncludeChildRecordsToTheHierarchyEvenIfTheyAreNotIncludedInTheOriginalResultsSet()
        {
            var parentUprn = _faker.Random.Number(10000000, 99999999);
            var matchingPostcode = "E8 4TT";
            var nonMatchingPostCode = "A1 B23";

            var parentAddress = new NationalAddress()
            {
                UPRN = parentUprn,
                ParentUPRN = null,
                Postcode = matchingPostcode
            };

            var childAddressWithMatchingPostcode = new NationalAddress()
            {
                UPRN = _faker.Random.Number(10000000, 99999999),
                ParentUPRN = parentUprn,
                Postcode = matchingPostcode
            };

            var childAddressWithoutMatchingPostCode = new NationalAddress()
            {
                UPRN = _faker.Random.Number(10000000, 99999999),
                ParentUPRN = parentUprn,
                Postcode = nonMatchingPostCode
            };

            TestEfDataHelper.InsertAddress(DatabaseContext, request: parentAddress);
            TestEfDataHelper.InsertAddress(DatabaseContext, request: childAddressWithMatchingPostcode);
            TestEfDataHelper.InsertAddress(DatabaseContext, request: childAddressWithoutMatchingPostCode);

            var request = new SearchParameters()
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                Structure = GlobalConstants.Structure.Hierarchy,
                Postcode = matchingPostcode
            };

            var (addresses, totalCount) = _classUnderTest.SearchAddresses(request);

            addresses.Count.Should().Be(1);
            totalCount.Should().Be(3);
            addresses.First().UPRN.Should().Be(parentAddress.UPRN);
            addresses.First().ParentUPRN.Should().BeNull();
            addresses.First().ChildAddresses.Count.Should().Be(2);
            _ = addresses.First().ChildAddresses.All(x => x.ChildAddresses == null);
            _ = addresses.First().ChildAddresses.All(x => x.ParentUPRN == parentAddress.UPRN);
        }

        [Test]
        public void ItWillKeepTheCorrectOrderOfParentRecordsWithinTheHierarchy()
        {
            var parentUprnOne = _faker.Random.Number(10000000, 99999999);
            var parentUprnTwo = _faker.Random.Number(10000000, 99999999);

            var town = "Test town";

            //non matching postcode ensures this gets addedd to the results set as a missing parent after initial ordering
            //adding records later will break the order in most cases and not handling it would make the final order of parents unpredictable
            var parentAddressOne = new NationalAddress()
            {
                UPRN = parentUprnOne,
                ParentUPRN = null,
                Postcode = "AB 1CD",
                Town = town,
            };

            var childAddressOne = new NationalAddress()
            {
                UPRN = _faker.Random.Number(10000000, 99999999),
                ParentUPRN = parentUprnOne,
                Postcode = "E8 4TX9",
                Town = town,
            };

            var parentAddressTwo = new NationalAddress()
            {
                UPRN = parentUprnTwo,
                ParentUPRN = null,
                Postcode = "E8 4TX3",
                Town = town,
            };

            var childAddressTwo = new NationalAddress()
            {
                UPRN = _faker.Random.Number(10000000, 99999999),
                ParentUPRN = parentUprnTwo,
                Postcode = "E8 4TX3",
                Town = town,
            };

            TestEfDataHelper.InsertAddress(DatabaseContext, request: parentAddressOne);
            TestEfDataHelper.InsertAddress(DatabaseContext, request: parentAddressTwo);
            TestEfDataHelper.InsertAddress(DatabaseContext, request: childAddressOne);
            TestEfDataHelper.InsertAddress(DatabaseContext, request: childAddressTwo);

            var request = new SearchParameters()
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                Structure = GlobalConstants.Structure.Hierarchy,
                Postcode = "E8 4T"
            };

            var (addresses, totalCount) = _classUnderTest.SearchAddresses(request);

            addresses.Count.Should().Be(2);
            totalCount.Should().Be(4);
            _ = addresses.
                First().Postcode.Should().Be(parentAddressOne.Postcode);
            _ = addresses.
                Last().Postcode.Should().Be(parentAddressTwo.Postcode);
        }

        [Test]
        [Ignore("Disabled until we have optimised tables for parent UPRN queries")]
        public void ItWillKeepTheCorrectOrderOfChildRecordsWithinTheHierarchy()
        {
            var parentUprn = _faker.Random.Number(10000000, 99999999);

            var town = "Test town";

            var parentAddress = new NationalAddress()
            {
                UPRN = parentUprn,
                ParentUPRN = null,
                Postcode = "E8 4TX9",
                Town = town,
            };

            var childAddressOne = new NationalAddress()
            {
                UPRN = _faker.Random.Number(10000000, 99999999),
                ParentUPRN = parentAddress.UPRN,
                Postcode = parentAddress.Postcode,
                Town = town,
            };

            //child address with the same parent uprn, but with different postode.
            //This ensures this child gets added to the result set after initial sorting 
            //post code difference to other child record is not realistic, but highlights ordering issues if not handled
            var childAddressTwo = new NationalAddress()
            {
                UPRN = _faker.Random.Number(10000000, 99999999),
                ParentUPRN = parentAddress.UPRN,
                Postcode = "AB 1CD",
                Town = town,
            };

            TestEfDataHelper.InsertAddress(DatabaseContext, request: parentAddress);
            TestEfDataHelper.InsertAddress(DatabaseContext, request: childAddressOne);
            TestEfDataHelper.InsertAddress(DatabaseContext, request: childAddressTwo);

            var request = new SearchParameters()
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                Structure = GlobalConstants.Structure.Hierarchy,
                Postcode = "E8 4TX"
            };

            var (addresses, totalCount) = _classUnderTest.SearchAddresses(request);

            addresses.Count.Should().Be(1);
            totalCount.Should().Be(3);
            _ = addresses.
                First().ChildAddresses.First().Postcode.Should().Be(childAddressTwo.Postcode);
        }

        [Test]
        public void ItWillReturnAsManylayersOfHierarchyAsRequiredByTheDataStructure()
        {
            var levelOneParent = new NationalAddress()
            {
                UPRN = 1,
                ParentUPRN = null,
                Postcode = "E8 4TA"
            };

            var levelOneChild = new NationalAddress()
            {
                UPRN = 2,
                ParentUPRN = levelOneParent.UPRN,
                Postcode = "E8 4TA"
            };

            var levelTwoParent = new NationalAddress()
            {
                UPRN = 3,
                ParentUPRN = levelOneChild.UPRN,
                Postcode = "E8 4TB"
            };

            var levelTwoChild = new NationalAddress()
            {
                UPRN = 4,
                ParentUPRN = levelTwoParent.UPRN,
                Postcode = "E8 4TB"
            };

            TestEfDataHelper.InsertAddress(DatabaseContext, request: levelOneParent);
            TestEfDataHelper.InsertAddress(DatabaseContext, request: levelOneChild);
            TestEfDataHelper.InsertAddress(DatabaseContext, request: levelTwoParent);
            TestEfDataHelper.InsertAddress(DatabaseContext, request: levelTwoChild);

            var request = new SearchParameters()
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                Structure = GlobalConstants.Structure.Hierarchy,
                Postcode = "E8 4T"
            };

            var (addresses, totalCount) = _classUnderTest.SearchAddresses(request);

            addresses.Count.Should().Be(1);
            addresses.First().ChildAddresses.Count.Should().Be(1);
            addresses.First().ChildAddresses.First().ChildAddresses.Count.Should().Be(1);
        }

        [Test]
        public void ItWillReturnRecordsWithoutParentUPRNWhenHierarchyIsRequestedButMatchingRecordsDontHaveAnyChildren()
        {
            var postcode = "E8 4TB";

            // this could be standalone unit or a parent
            var parentOne = new NationalAddress()
            {
                UPRN = 3,
                ParentUPRN = null,
                Postcode = postcode
            };

            var parentTwo = new NationalAddress()
            {
                UPRN = 4,
                ParentUPRN = null,
                Postcode = postcode
            };

            TestEfDataHelper.InsertAddress(DatabaseContext, request: parentOne);
            TestEfDataHelper.InsertAddress(DatabaseContext, request: parentTwo);

            var request = new SearchParameters()
            {
                Page = 1,
                PageSize = 50,
                Gazetteer = GlobalConstants.Gazetteer.Both,
                Structure = GlobalConstants.Structure.Hierarchy,
                Postcode = postcode
            };

            var (addresses, totalCount) = (_classUnderTest.SearchAddresses(request));
            totalCount.Should().Be(2);
            addresses.Any(x => x.ChildAddresses == null).Should().BeTrue();
        }

        #region OrderDomainAddresses

        [Test]
        public void OrderDomainAddressesWillFirstlyOrderByTown()
        {
            var addressOne = new Address() { Town = "town c" };
            var addressTwo = new Address() { Town = "town b" };
            var addressThree = new Address() { Town = "town a" };

            var addresses = new List<Address>()
            {
                addressOne,
                addressTwo,
                addressThree
            };

            var results = _classUnderTest.OrderDomainAddresses(addresses);
            results.ElementAt(0).Should().Be(addressThree);
            results.ElementAt(1).Should().Be(addressTwo);
            results.ElementAt(2).Should().Be(addressOne);

        }

        [Test]
        public void OrderDomainAddressesWillSecondlyOrderByPostCodePresense()
        {
            var addressOne = new Address() { Town = "town a" };
            var addressTwo = new Address() { Town = "town a", Postcode = "A1 2B" };
            var addressThree = new Address() { Town = "town a" };

            var addresses = new List<Address>()
            {
                addressOne,
                addressTwo,
                addressThree
            };

            var results = _classUnderTest.OrderDomainAddresses(addresses);
            results.ElementAt(0).Should().Be(addressTwo);
            results.ElementAt(1).Should().Be(addressOne);
            results.ElementAt(2).Should().Be(addressThree);
        }

        [Test]
        public void OrderDomainAddressesWillThirdlyOrderBypostcode()
        {
            var addressOne = new Address() { Town = "town a", Street = "street a", Postcode = "C3 1A" };
            var addressTwo = new Address() { Town = "town a", Street = "street a", Postcode = "B2 3C" };
            var addressThree = new Address() { Town = "town a", Street = "street a", Postcode = "A1 2B" };

            var addresses = new List<Address>()
            {
                addressOne,
                addressTwo,
                addressThree
            };

            var results = _classUnderTest.OrderDomainAddresses(addresses);
            results.ElementAt(0).Should().Be(addressThree);
            results.ElementAt(1).Should().Be(addressTwo);
            results.ElementAt(2).Should().Be(addressOne);
        }

        [Test]
        public void OrderDomainAddressesWillFourthlyOrderByStreet()
        {
            var addressOne = new Address() { Town = "town a", Street = "street c" };
            var addressTwo = new Address() { Town = "town a", Street = "street b" };
            var addressThree = new Address() { Town = "town a", Street = "street a" };

            var addresses = new List<Address>()
            {
                addressOne,
                addressTwo,
                addressThree
            };

            var results = _classUnderTest.OrderDomainAddresses(addresses);
            results.ElementAt(0).Should().Be(addressThree);
            results.ElementAt(1).Should().Be(addressTwo);
            results.ElementAt(2).Should().Be(addressOne);
        }

        [Test]
        public void OrderDomainAddressesWillFifthlyOrderByPresenceOfBuildingNumber()
        {
            var addressOne = new Address() { Town = "town a", Postcode = "A1", Street = "street a", BuildingNumber = "1" };
            var addressTwo = new Address() { Town = "town a", Postcode = "A1", Street = "street a", };
            var addressThree = new Address() { Town = "town a", Postcode = "A1", Street = "street a", BuildingNumber = "2" };

            var addresses = new List<Address>()
            {
                addressOne,
                addressTwo,
                addressThree
            };

            var results = _classUnderTest.OrderDomainAddresses(addresses);
            results.ElementAt(0).Should().Be(addressOne);
            results.ElementAt(1).Should().Be(addressThree);
            results.ElementAt(2).Should().Be(addressTwo);
        }

        [Test]
        public void OrderDomainAddressesWillSixthlyOrderByBuildingNumber()
        {
            var addressOne = new Address() { Town = "town a", Street = "street a", BuildingNumber = "2" };
            var addressTwo = new Address() { Town = "town a", Street = "street a", BuildingNumber = "3" };
            var addressThree = new Address() { Town = "town a", Street = "street a", BuildingNumber = "1" };

            var addresses = new List<Address>()
            {
                addressOne,
                addressTwo,
                addressThree
            };

            var results = _classUnderTest.OrderDomainAddresses(addresses);
            results.ElementAt(0).Should().Be(addressThree);
            results.ElementAt(1).Should().Be(addressOne);
            results.ElementAt(2).Should().Be(addressTwo);
        }

        [Test]
        public void OrderDomainAddressesWillSeventhlyOrderByZeroUnitNumber()
        {
            var addressOne = new Address() { Town = "town a", Street = "street a", UnitNumber = "0" };
            var addressTwo = new Address() { Town = "town a", Street = "street a", UnitNumber = "3" };
            var addressThree = new Address() { Town = "town a", Street = "street a", UnitNumber = "1" };

            var addresses = new List<Address>()
            {
                addressOne,
                addressTwo,
                addressThree
            };

            var results = _classUnderTest.OrderDomainAddresses(addresses);
            results.ElementAt(0).Should().Be(addressThree);
            results.ElementAt(1).Should().Be(addressTwo);
            results.ElementAt(2).Should().Be(addressOne);
        }

        [Test]
        public void OrderDomainAddressesWillEightlyOrderByUnitNumber()
        {
            var addressOne = new Address() { Town = "town a", Street = "street a", UnitNumber = "0" };
            var addressTwo = new Address() { Town = "town a", Street = "street a", UnitNumber = "3" };
            var addressThree = new Address() { Town = "town a", Street = "street a", UnitNumber = "1" };

            var addresses = new List<Address>()
            {
                addressOne,
                addressTwo,
                addressThree
            };

            var results = _classUnderTest.OrderDomainAddresses(addresses);
            results.ElementAt(0).Should().Be(addressThree);
            results.ElementAt(1).Should().Be(addressTwo);
            results.ElementAt(2).Should().Be(addressOne);
        }

        [Test]
        public void OrderDomainAddressesWillNinthlyOrderByPresenseOfUnitName()
        {
            var addressOne = new Address() { Town = "town a", Street = "street a", };
            var addressTwo = new Address() { Town = "town a", Street = "street a", UnitName = "name a" };
            var addressThree = new Address() { Town = "town a", Street = "street a", UnitName = "name a" };

            var addresses = new List<Address>()
            {
                addressOne,
                addressTwo,
                addressThree
            };

            var results = _classUnderTest.OrderDomainAddresses(addresses);
            results.ElementAt(0).Should().Be(addressTwo);
            results.ElementAt(1).Should().Be(addressThree);
            results.ElementAt(2).Should().Be(addressOne);
        }

        [Test]
        public void OrderDomainAddressesWillTenthlylyOrderByUnitName()
        {
            var addressOne = new Address() { Town = "town a", Street = "street a", };
            var addressTwo = new Address() { Town = "town a", Street = "street a", UnitName = "name b" };
            var addressThree = new Address() { Town = "town a", Street = "street a", UnitName = "name a" };

            var addresses = new List<Address>()
            {
                addressOne,
                addressTwo,
                addressThree
            };

            var results = _classUnderTest.OrderDomainAddresses(addresses);
            results.ElementAt(0).Should().Be(addressThree);
            results.ElementAt(1).Should().Be(addressTwo);
            results.ElementAt(2).Should().Be(addressOne);
        }
        #endregion
        #endregion
    }
}
