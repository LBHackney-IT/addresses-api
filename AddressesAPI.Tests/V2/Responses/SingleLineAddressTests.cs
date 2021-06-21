using AddressesAPI.V2.Boundary.Responses.Data;
using FluentAssertions;
using NUnit.Framework;

namespace AddressesAPI.Tests.V2.Gateways
{
    public class SingleLineAddressTests
    {
        private const string Line1 = "Line 1";
        private const string Line2 = "Line 2";
        private const string Line3 = "Line 3";
        private const string Line4 = "Line 4";
        private const string Town = "Town";
        private const string Postcode = "Post Code";

        [TestCase(Line1, Line2, Line3, Line4, Town, Postcode, Line1 + ", " + Line2 + ", " + Line3 + ", " + Line4 + ", " + Town + ", " + Postcode)]
        [TestCase("", Line2, Line3, Line4, Town, Postcode, Line2 + ", " + Line3 + ", " + Line4 + ", " + Town + ", " + Postcode)]
        [TestCase(Line1, Line2, Line3, "", Town, Postcode, Line1 + ", " + Line2 + ", " + Line3 + ", " + Town + ", " + Postcode)]
        [TestCase(Line1, Line2, Line3, Line4, Town, "", Line1 + ", " + Line2 + ", " + Line3 + ", " + Line4 + ", " + Town )]
        [TestCase("", "", "", "", "", "", "")]
        public void SingleLineAddress_WithVariousData_ReturnsCorrectResult(
            string line1,
            string line2,
            string line3,
            string line4,
            string town,
            string postcode,
            string expectedSingleLine)
        {
            // Arrange
            var address = new AddressResponse()
            {
                Line1 = line1,
                Line2 = line2,
                Line3 = line3,
                Line4 = line4,
                Town = town,
                Postcode = postcode,
            };

            // Act
            var singleLineAddress = address.SingleLineAddress;

            // Assert
            singleLineAddress.Should().Be(expectedSingleLine);
        }
    }
}
