using AddressesAPI.V1.Boundary.Responses;
using AddressCrossReferenceResponse = AddressesAPI.V1.Boundary.Responses.Data.AddressCrossReference;
using AddressesAPI.V1.Domain;
using AddressesAPI.V1.Factories;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;

namespace AddressesAPI.Tests.V1.Factories
{
    public class ResponseFactoryTest
    {
        private readonly IFixture _fixture = new Fixture();
        [Test]
        public void MapsAnAddressDomainDirectlyToAnAddressResponse()
        {
            var domain = _fixture.Create<Address>();
            domain.ToResponse().Should().BeEquivalentTo(domain);
            domain.ToResponse().Should().BeOfType<AddressResponse>();
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
