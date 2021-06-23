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

        #region GetMatchingCrossReferenceUprns

        [TestCase("TAXES", "TestValue")]
        [TestCase("Parkin", "A72927GH")]
        public void WillReturnAllUprnsForACrossReference(string code, string value)
        {
            TestDataHelper.InsertAddressInDb(DatabaseContext);

            //--- First cross reference
            var uprnOne = _faker.Random.Long(10000000, 99999999);

            TestDataHelper.InsertCrossReference(DatabaseContext, uprnOne,
                new CrossReference { Code = code, Value = value }
            );

            //--- Second Cross reference
            var uprnTwo = _faker.Random.Long(10000000, 99999999);

            TestDataHelper.InsertCrossReference(DatabaseContext, uprnTwo,
                new CrossReference { Code = code, Value = value }
            );

            var uprns = _classUnderTest.GetMatchingCrossReferenceUprns(code, value);

            uprns.Count.Should().Be(2);
            uprns.Should().ContainEquivalentOf(uprnOne);
            uprns.Should().ContainEquivalentOf(uprnTwo);
        }
        #endregion

        #region GetAddresses

        [Test]
        public void GivenAnAddressKey_GetAddresses_GetsTheAddressDetails()
        {
            var addressKey = _faker.Random.String2(14);
            var savedAddress = TestDataHelper.InsertAddressInDb(DatabaseContext, addressKey);

            var addresses = _classUnderTest.GetAddresses(new List<string> { addressKey }, GlobalConstants.Format.Detailed);

            addresses.Count.Should().Be(1);
            addresses.First().AddressKey.Should().Be(addressKey);
            addresses.First().Should().BeEquivalentTo(savedAddress.ToDomain());
        }

        [Test]
        public void GivenAnAddressKey_GetAddresses_OnlyGetsAddressesForTheKeys()
        {
            var addressKey = _faker.Random.String2(14);
            var savedAddress = TestDataHelper.InsertAddressInDb(DatabaseContext, addressKey);
            TestDataHelper.InsertAddressInDb(DatabaseContext);

            var addresses = _classUnderTest.GetAddresses(new List<string> { addressKey }, GlobalConstants.Format.Detailed);

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
            var thirdAddress = TestDataHelper.InsertAddressInDb(DatabaseContext, addressKeys.ElementAt(2));
            var firstAddress = TestDataHelper.InsertAddressInDb(DatabaseContext, addressKeys.ElementAt(0));
            var secondAddress = TestDataHelper.InsertAddressInDb(DatabaseContext, addressKeys.ElementAt(1));

            var addresses = _classUnderTest.GetAddresses(addressKeys, GlobalConstants.Format.Detailed);

            addresses.ElementAt(0).Should().BeEquivalentTo(firstAddress.ToDomain());
            addresses.ElementAt(1).Should().BeEquivalentTo(secondAddress.ToDomain());
            addresses.ElementAt(2).Should().BeEquivalentTo(thirdAddress.ToDomain());
        }


        [Test]
        public void GetAddresses_WillGetADetailedAddressFromTheDatabase()
        {
            var addressKey = _faker.Random.String2(14);
            var savedAddress = TestDataHelper.InsertAddressInDb(DatabaseContext, addressKey);

            var addresses = _classUnderTest.GetAddresses(new List<string> { addressKey }, GlobalConstants.Format.Detailed);

            addresses.Count.Should().Be(1);
            addresses.First().AddressKey.Should().Be(addressKey);
            addresses.First().Should().BeEquivalentTo(savedAddress.ToDomain());
        }

        [Test]
        public void GetAddresses_WillGetASimpleAddressFromTheDatabase()
        {
            var addressKey = _faker.Random.String2(14);
            var savedAddress = TestDataHelper.InsertAddressInDb(DatabaseContext, addressKey);

            var addresses = _classUnderTest.GetAddresses(new List<string> { addressKey }, GlobalConstants.Format.Simple);

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
            var savedAddress = TestDataHelper.InsertAddressInDb(DatabaseContext, addressKey);

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
