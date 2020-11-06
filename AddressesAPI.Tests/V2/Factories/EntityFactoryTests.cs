using System.Collections.Generic;
using AddressesAPI.Infrastructure;
using AddressesAPI.Tests.V2.Helper;
using AddressesAPI.V2.Factories;
using AutoFixture;
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
                {"HackneyGazetteerOutOfBoroughAddress", addressEntity.NeverExport }
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
