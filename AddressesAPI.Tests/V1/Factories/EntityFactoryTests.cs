using System.Collections.Generic;
using AddressesAPI.Infrastructure;
using AddressesAPI.Tests.V1.Helper;
using AddressesAPI.V1.Factories;
using AutoFixture;
using NUnit.Framework;

namespace AddressesAPI.Tests.V1.Factories
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
                {"HackneyGazetteerOutOfBoroughAddress", addressEntity.OutOfBoroughAddress },
                {"UnitNumber", addressEntity.UnitNumber.ToString()}
            };
            address.ShouldBeEquivalentToExpectedObjectWithExceptions(addressEntity, exceptions);
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
