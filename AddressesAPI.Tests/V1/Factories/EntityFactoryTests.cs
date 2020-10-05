using AddressesAPI.V1.Factories;
using AddressesAPI.V1.Infrastructure;
using AutoFixture;
using FluentAssertions;
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

            address.AddressKey.Should().Be(addressEntity.AddressKey);
            address.USRN.Should().Be(addressEntity.USRN);
            address.ParentUPRN.Should().Be(addressEntity.ParentUPRN);
            address.AddressStatus.Should().Be(addressEntity.AddressStatus);
            address.UnitName.Should().Be(addressEntity.UnitName);
            address.UnitNumber.Should().Be(addressEntity.UnitNumber);
            address.BuildingName.Should().Be(addressEntity.BuildingName);
            address.BuildingNumber.Should().Be(addressEntity.BuildingNumber);
            address.Street.Should().Be(addressEntity.Street);
            address.Locality.Should().Be(addressEntity.Locality);
            address.Gazetteer.Should().Be(addressEntity.Gazetteer);
            address.CommercialOccupier.Should().Be(addressEntity.Organisation);
            address.Ward.Should().Be(addressEntity.Ward);
            address.UsageDescription.Should().Be(addressEntity.UsageDescription);
            address.UsagePrimary.Should().Be(addressEntity.UsagePrimary);
            address.UsageCode.Should().Be(addressEntity.UsageCode);
            address.PlanningUseClass.Should().Be(addressEntity.PlanningUseClass);
            address.PropertyShell.Should().Be(addressEntity.PropertyShell);
            address.HackneyGazetteerOutOfBoroughAddress.Should().Be(addressEntity.NeverExport);
            address.Easting.Should().Be(addressEntity.Easting);
            address.Northing.Should().Be(addressEntity.Northing);
            address.Longitude.Should().Be(addressEntity.Longitude);
            address.Latitude.Should().Be(addressEntity.Latitude);
            address.AddressStartDate.Should().Be(addressEntity.AddressStartDate);
            address.AddressEndDate.Should().Be(addressEntity.AddressEndDate);
            address.AddressChangeDate.Should().Be(addressEntity.AddressChangeDate);
            address.PropertyStartDate.Should().Be(addressEntity.PropertyStartDate);
            address.PropertyEndDate.Should().Be(addressEntity.PropertyEndDate);
            address.PropertyChangeDate.Should().Be(addressEntity.PropertyChangeDate);
            address.Line1.Should().Be(addressEntity.Line1);
            address.Line2.Should().Be(addressEntity.Line2);
            address.Line3.Should().Be(addressEntity.Line3);
            address.Line4.Should().Be(addressEntity.Line4);
            address.Town.Should().Be(addressEntity.Town);
            address.UPRN.Should().Be(addressEntity.UPRN);
            address.Postcode.Should().Be(addressEntity.Postcode);
        }
    }
}
