using AddressesAPI.Tests.V2.Helper;
using AddressesAPI.V2.Boundary.Responses.Data;
using AddressesAPI.V2.Domain;
using AddressesAPI.V2.Factories;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;

namespace AddressesAPI.Tests.V2.Factories
{
    public class ResponseFactoryTest
    {
        private readonly IFixture _fixture = new Fixture();
        [Test]
        public void MapsAnAddressDomainDirectlyToAnAddressResponse()
        {
            // Arrange
            var domain = _fixture.Create<Address>();
            var response = domain.ToResponse();
            // make an exception for SingleLineAddress
            var address = new AddressResponse()
            {
                Line1 = domain.Line1,
                Line2 = domain.Line2,
                Line3 = domain.Line3,
                Line4 = domain.Line4,
                Town = domain.Town,
                Postcode = domain.Postcode,
            };

            // Act
            var exceptions = new Dictionary<string, object>() { { "SingleLineAddress", address.SingleLineAddress } };

            // Assert
            response.ShouldBeEquivalentToExpectedObjectWithExceptions(domain, exceptions);
        }

        [Test]
        public void MapsTheStatusApprovedPreferredToApproved()
        {
            var domain = _fixture.Build<Address>()
                .With(a => a.AddressStatus, "Approved Preferred")
                .Create();
            var response = domain.ToResponse();
            response.AddressStatus.Should().Be("Approved");
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
