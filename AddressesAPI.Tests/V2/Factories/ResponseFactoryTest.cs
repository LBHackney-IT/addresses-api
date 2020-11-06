using AddressesAPI.Tests.V2.Helper;
using AddressesAPI.V2.Boundary.Responses.Data;
using AddressesAPI.V2.Domain;
using AddressesAPI.V2.Factories;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;

namespace AddressesAPI.Tests.V2.Factories
{
    public class ResponseFactoryTest
    {
        private readonly IFixture _fixture = new Fixture();
        [Test]
        public void MapsAnAddressDomainDirectlyToAnAddressResponse()
        {
            var domain = _fixture.Create<Address>();
            var response = domain.ToResponse();
            response.ShouldBeEquivalentToExpectedObjectWithExceptions(domain);
        }

        [Test]
        public void MapsAnAddressCrossReferenceDomainDirectlyToAResponse()
        {
            var domain = _fixture.Create<AddressCrossReference>();
            domain.ToResponse().Should().BeEquivalentTo(domain);
            domain.ToResponse().Should().BeOfType<AddressCrossReferenceResponse>();
        }
    }
}
