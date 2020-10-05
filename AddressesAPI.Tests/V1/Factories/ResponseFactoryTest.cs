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
            var response = domain.ToResponse();
            response.Line1.Should().Be(domain.Line1);
            response.Line2.Should().Be(domain.Line2);
            response.Line3.Should().Be(domain.Line3);
            response.Line4.Should().Be(domain.Line4);
            response.Town.Should().Be(domain.Town);
            response.Postcode.Should().Be(domain.Postcode);
            response.UPRN.Should().Be(domain.UPRN);
            response.AddressKey.Should().Be(domain.AddressKey);
            response.USRN.Should().Be(domain.USRN);
            response.ParentUPRN.Should().Be(domain.ParentUPRN);
            response.AddressStatus.Should().Be(domain.AddressStatus);
            response.UnitName.Should().Be(domain.UnitName);
            response.UnitNumber.Should().Be(domain.UnitNumber);
            response.BuildingName.Should().Be(domain.BuildingName);
            response.BuildingNumber.Should().Be(domain.BuildingNumber);
            response.Street.Should().Be(domain.Street);
            response.Locality.Should().Be(domain.Locality);
            response.Gazetteer.Should().Be(domain.Gazetteer);
            response.CommercialOccupier.Should().Be(domain.CommercialOccupier);
            response.Ward.Should().Be(domain.Ward);
            response.UsageDescription.Should().Be(domain.UsageDescription);
            response.UsagePrimary.Should().Be(domain.UsagePrimary);
            response.UsageCode.Should().Be(domain.UsageCode);
            response.PlanningUseClass.Should().Be(domain.PlanningUseClass);
            response.PropertyShell.Should().Be(domain.PropertyShell);
            response.HackneyGazetteerOutOfBoroughAddress.Should().Be(domain.HackneyGazetteerOutOfBoroughAddress);
            response.Easting.Should().Be(domain.Easting);
            response.Northing.Should().Be(domain.Northing);
            response.Longitude.Should().Be(domain.Longitude);
            response.Latitude.Should().Be(domain.Latitude);
            response.addressStartDate.Should().Be(domain.AddressStartDate);
            response.addressEndDate.Should().Be(domain.AddressEndDate);
            response.addressChangeDate.Should().Be(domain.AddressChangeDate);
            response.propertyStartDate.Should().Be(domain.PropertyStartDate);
            response.propertyEndDate.Should().Be(domain.PropertyEndDate);
            response.propertyChangeDate.Should().Be(domain.PropertyChangeDate);
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
