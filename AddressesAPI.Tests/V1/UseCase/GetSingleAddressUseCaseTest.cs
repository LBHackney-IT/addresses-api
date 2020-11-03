using System.Linq;
using System.Threading.Tasks;
using AddressesAPI.Tests.V1.Helper;
using AddressesAPI.V1.Boundary.Requests;
using AddressesAPI.V1.Boundary.Responses;
using AddressesAPI.V1.Boundary.Responses.Metadata;
using AddressesAPI.V1.Domain;
using AddressesAPI.V1.Gateways;
using AddressesAPI.V1.UseCase;
using AddressesAPI.V1.UseCase.Interfaces;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace AddressesAPI.Tests.V1.UseCase
{
    public class GetSingleAddressUseCaseTest
    {
        private readonly IGetSingleAddressUseCase _classUnderTest;
        private readonly Mock<IAddressesGateway> _fakeGateway;


        public GetSingleAddressUseCaseTest()
        {
            _fakeGateway = new Mock<IAddressesGateway>();

            _classUnderTest = new GetSingleAddressUseCase(_fakeGateway.Object);
        }

        [Test]
        public void GivenValidInput_WhenExecuteAsync_GatewayReceivesCorrectInputLength()
        {

            var lpi_key = "ABCDEFGHIJKLMN"; //14 characters
            _fakeGateway.Setup(s => s.GetSingleAddress("ABCDEFGHIJKLMN")).Returns(new Address()).Verifiable();

            var request = new GetAddressRequest
            {
                addressID = lpi_key
            };

            _classUnderTest.ExecuteAsync(request);

            _fakeGateway.Verify();
        }

        [Test]
        public void GivenBlankStringInput_WhenExecuteAsync_ThenShouldThrowException()
        {
            //arrange
            var request = new GetAddressRequest { addressID = string.Empty };
            //act
            _classUnderTest.Invoking(y => y.ExecuteAsync(request))
                .Should().Throw<BadRequestException>()
                .Where(ex => ex.ValidationResponse.ValidationErrors.First().Message == "addressID must be provided");
        }

        [Test]
        public void GivenString13Characters_WhenExecuteAsync_TheShouldThrowException()
        {
            var request = new GetAddressRequest { addressID = "ABCDEFGHIJKLM" };
            //act
            //assert
            _classUnderTest.Invoking(y => y.ExecuteAsync(request))
                .Should().Throw<BadRequestException>()
                .Where(ex => ex.ValidationResponse.ValidationErrors.First().Message == "addressID must be 14 characters"); ;
        }

        [Test]
        public void GivenString15Characters_WhenExecuteAsync_TheShouldThrowException()
        {
            var request = new GetAddressRequest { addressID = "ABCDEFGHIJKLMNO" };
            //act
            //assert
            _classUnderTest.Invoking(y => y.ExecuteAsync(request))
                .Should().Throw<BadRequestException>()
                .Where(ex => ex.ValidationResponse.ValidationErrors.First().Message == "addressID must be 14 characters"); ;
        }

        [Test]
        public void GivenValidInput_WhenGatewayRespondsWithNull_ThenResponseShouldBeNull()
        {
            //arrange
            var lpi_key = "ABCDEFGHIJKLMN";

            _fakeGateway.Setup(s => s.GetSingleAddress("ABCDEFGHIJKLMN"))
                .Returns((Address) null);

            var request = new GetAddressRequest
            {
                addressID = lpi_key
            };
            //act
            var response = _classUnderTest.ExecuteAsync(request);
            //assert
            response.Addresses.Should().BeNull();
        }

        [Test]
        public void GivenValidLPIKey_WhenExecuteAsync_ThenAddressShouldBeReturned()
        {
            var address = new Address
            {
                AddressKey = "ABCDEFGHIJKLMN",
                UPRN = 10024389298,
                USRN = 21320239,
                ParentUPRN = 10024389282,
                AddressStatus = "Approved Preferred",
                UnitName = "FLAT 16",
                UnitNumber = "",
                BuildingName = "HAZELNUT COURT",
                BuildingNumber = "1",
                Street = "FIRWOOD LANE",
                Postcode = "RM3 0FS",
                Locality = "",
                Gazetteer = "NATIONAL",
                CommercialOccupier = "",
                UsageDescription = "Unclassified, Awaiting Classification",
                UsagePrimary = "Unclassified",
                UsageCode = "UC",
                PropertyShell = false,
                HackneyGazetteerOutOfBoroughAddress = false,
                Easting = 554189.4500,
                Northing = 190281.1000,
                Longitude = 0.2244347,
                Latitude = 51.590289
            };

            var lpi_key = "ABCDEFGHIJKLMN";
            var request = new GetAddressRequest
            {
                addressID = lpi_key
            };
            _fakeGateway.Setup(s => s.GetSingleAddress(lpi_key))
                .Returns(address);

            var response = _classUnderTest.ExecuteAsync(request);

            response.Should().NotBeNull();

            response.Addresses[0].AddressShouldEqual(address);
        }
    }
}
