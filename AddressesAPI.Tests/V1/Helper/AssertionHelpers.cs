using System.Collections.Generic;
using AddressesAPI.V1.Boundary.Responses;
using AddressesAPI.V1.Boundary.Responses.Data;
using AddressesAPI.V1.Domain;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AddressesAPI.Tests.V1.Helper
{
    public static class AssertionHelpers
    {
        public static void AddressShouldEqual(this AddressResponse received, Address expected)
        {
            received.AddressKey.Should().BeEquivalentTo(expected.AddressKey);
            received.Postcode.Should().NotBeNullOrEmpty();

            received.UPRN.Should().Be(expected.UPRN);
            received.USRN.Should().Be(expected.USRN);

            received.AddressStatus.Should().NotBeNullOrEmpty();
            received.AddressStatus.Should().BeEquivalentTo(expected.AddressStatus);

            received.UnitName.Should().BeEquivalentTo(expected.UnitName);
            received.UnitNumber.Should().BeEquivalentTo(expected.UnitNumber);
            received.BuildingName.Should().BeEquivalentTo(expected.BuildingName);
            received.BuildingNumber.Should().BeEquivalentTo(expected.BuildingNumber);

            received.Street.Should().NotBeNullOrEmpty();
            received.Street.Should().BeEquivalentTo(expected.Street);

            received.Postcode.Should().BeEquivalentTo(expected.Postcode);

            received.Locality.Should().BeEquivalentTo(expected.Locality);

            received.Gazetteer.Should().NotBeNullOrEmpty();
            received.Gazetteer.Should().BeEquivalentTo(expected.Gazetteer);

            received.CommercialOccupier.Should().BeEquivalentTo(expected.CommercialOccupier);
            received.UsageDescription.Should().BeEquivalentTo(expected.UsageDescription);
            received.UsagePrimary.Should().BeEquivalentTo(expected.UsagePrimary);
            received.UsageCode.Should().BeEquivalentTo(expected.UsageCode);
            received.PropertyShell.Should().Be(expected.PropertyShell);
            received.HackneyGazetteerOutOfBoroughAddress.Should().Be(expected.HackneyGazetteerOutOfBoroughAddress);
            received.Easting.Should().Be(expected.Easting);
            received.Northing.Should().Be(expected.Northing);

            received.Longitude.Should().Be(expected.Longitude);
            received.Latitude.Should().Be(expected.Latitude);
        }
    }
}
