using AddressesAPI.V1.Boundary.Responses.Data;
using AddressesAPI.V1.Domain;
using AddressesAPI.V1.Factories;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;
using AddressCrossReferenceResponse = AddressesAPI.V1.Boundary.Responses.Data.AddressCrossReferenceResponse;

namespace AddressesAPI.Tests.V1.Factories
{
    public class ResponseFactoryTest
    {
        private readonly IFixture _fixture = new Fixture();
        [Test]
        public void MapsAnAddressDomainDirectlyToAnAddressResponse()
        {
            var domain = _fixture.Build<Address>().Without(a => a.ChildAddresses).Create();

            var childAddress = _fixture.Build<Address>().Without(a => a.ChildAddresses).Create();
            domain.ChildAddresses = new List<Address>() { childAddress };

            var result = domain.ToResponse();

            result.Should().BeEquivalentTo(domain);
            result.Should().BeOfType<AddressResponse>();
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
