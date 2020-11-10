using System.Collections.Generic;
using AddressesAPI.Infrastructure;
using AddressesAPI.Tests.V2.Helper;
using AddressesAPI.V2;
using AddressesAPI.V2.Factories;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;

namespace AddressesAPI.Tests.V2.Factories
{
    public class EntityFactoryTests
    {
        private readonly Fixture _fixture = new Fixture();

        [Test]
        public void CanMapAnAddressEntityToDomain()
        {
            var addressEntity = _fixture.Create<Address>();
            var address = addressEntity.ToDomain();

            //Fields named differently between infrastructure and domain objects
            var exceptions = new Dictionary<string, object>
            {
                {"CommercialOccupier", addressEntity.Organisation},
                {"OutOfBoroughAddress", false}
            };
            address.ShouldBeEquivalentToExpectedObjectWithExceptions(addressEntity, exceptions);
        }

        [TestCase("Hackney", false, false)]
        [TestCase("Hackney", true, true)]
        [TestCase("National", true, true)]
        [TestCase("National", false, true)]
        public void CorrectlySetsOutOfBoroughProperty(string gazetteer, bool dbOutOfBoroughValue, bool expectedOutOfBorough)
        {
            var domain = _fixture.Build<Address>()
                .With(a => a.Gazetteer, gazetteer)
                .With(a => a.OutOfBoroughAddress, dbOutOfBoroughValue)
                .Create();
            var response = domain.ToDomain();
            response.OutOfBoroughAddress.Should().Be(expectedOutOfBorough);
        }

        [Test]
        public void CanMapACrossReferenceEntityToDomain()
        {
            var crossReferenceEntity = _fixture.Create<CrossReference>();
            var crossReference = crossReferenceEntity.ToDomain();

            crossReference.ShouldBeEquivalentToExpectedObjectWithExceptions(crossReferenceEntity);
        }
    }
}
