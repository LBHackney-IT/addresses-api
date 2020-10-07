using System.Collections.Generic;
using AddressesAPI.Tests.V1.Helper;
using AddressesAPI.V1.Factories;
using AddressesAPI.V1.Infrastructure;
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
                {"HackneyGazetteerOutOfBoroughAddress", addressEntity.NeverExport }
            };
            address.ShouldBeEquivalentToExpectedObjectWithExceptions(addressEntity, exceptions);
        }
    }
}