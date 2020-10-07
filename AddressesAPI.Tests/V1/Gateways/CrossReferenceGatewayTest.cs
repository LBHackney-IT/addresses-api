using System.Linq;
using AddressesAPI.Tests.V1.Helper;
using AddressesAPI.V1.Factories;
using AddressesAPI.V1.Gateways;
using Bogus;
using FluentAssertions;
using NUnit.Framework;

namespace AddressesAPI.Tests.V1.Gateways
{
    public class CrossReferenceGatewayTest : DatabaseTests
    {
        private CrossReferencesGateway _classUnderTest { get; set; }
        private readonly Faker _faker = new Faker();

        [SetUp]
        public void SetUp()
        {
            _classUnderTest = new CrossReferencesGateway(DatabaseContext);
        }

        [Test]
        public void ItWillReturnAListOfCrossReferencesUsingAUprn()
        {
            var uprn = _faker.Random.Long();

            var savedCrossRef = TestEfDataHelper.insertCrossReference(DatabaseContext, uprn);

            var response = _classUnderTest.GetAddressCrossReference(uprn);

            response.Should().NotBeNull();

            response.First().UPRN.Should().Be(uprn);
            response.First().Should().BeEquivalentTo(savedCrossRef.ToDomain());
        }

        [Test]
        public void ItWillReturnAnEmptyListIfACrossRefCouldNotBeFound()
        {
            var uprn = _faker.Random.Long();

            var response = _classUnderTest.GetAddressCrossReference(uprn);

            response.Should().NotBeNull();
            response.Count.Should().Be(0);
        }
    }
}
