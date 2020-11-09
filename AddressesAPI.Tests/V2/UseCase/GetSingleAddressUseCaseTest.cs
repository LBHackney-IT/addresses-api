using System;
using AddressesAPI.Tests.V2.Helper;
using AddressesAPI.V2.Boundary.Requests;
using AddressesAPI.V2.Boundary.Responses;
using AddressesAPI.V2.Boundary.Responses.Metadata;
using AddressesAPI.V2.Domain;
using AddressesAPI.V2.Gateways;
using AddressesAPI.V2.UseCase;
using AddressesAPI.V2.UseCase.Interfaces;
using FluentAssertions;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;

namespace AddressesAPI.Tests.V2.UseCase
{
    public class GetSingleAddressUseCaseTest
    {
        private readonly IGetSingleAddressUseCase _classUnderTest;
        private readonly Mock<IAddressesGateway> _fakeGateway;
        private readonly Mock<IGetAddressRequestValidator> _fakeValidator;

        public GetSingleAddressUseCaseTest()
        {
            _fakeGateway = new Mock<IAddressesGateway>();
            _fakeValidator = new Mock<IGetAddressRequestValidator>();

            _classUnderTest = new GetSingleAddressUseCase(_fakeGateway.Object, _fakeValidator.Object);
        }

        [Test]
        public void GivenValidInput_WhenExecuteAsync_GatewayReceivesCorrectInputLength()
        {

            var lpi_key = "ABCDEFGHIJKLMN"; //14 characters
            _fakeGateway.Setup(s => s.GetSingleAddress("ABCDEFGHIJKLMN")).Returns(new Address()).Verifiable();
            SetupValidatorToReturnValid();
            var request = new GetAddressRequest
            {
                addressID = lpi_key
            };

            _classUnderTest.ExecuteAsync(request);

            _fakeGateway.Verify();
        }

        [Test]
        public void GivenInvalidInput_WhenExecuteAsync_ThrowsValidationError()
        {
            SetupValidatorToReturnValid(false);
            Func<GetAddressResponse> testDelegate = () => _classUnderTest.ExecuteAsync(new GetAddressRequest());
            testDelegate.Should().Throw<BadRequestException>();
        }

        [Test]
        public void GivenValidInput_WhenGatewayRespondsWithNull_ThenResponseShouldBeNull()
        {
            //arrange
            var lpi_key = "ABCDEFGHIJKLMN";
            SetupValidatorToReturnValid();
            _fakeGateway.Setup(s => s.GetSingleAddress("ABCDEFGHIJKLMN"))
                .Returns((Address) null);

            var request = new GetAddressRequest
            {
                addressID = lpi_key
            };
            //act
            var response = _classUnderTest.ExecuteAsync(request);
            //assert
            response.Address.Should().BeNull();
        }

        [Test]
        public void GivenValidLPIKey_WhenExecuteAsync_ThenAddressShouldBeReturned()
        {
            SetupValidatorToReturnValid();
            var address = new Address
            {
                AddressKey = "ABCDEFGHIJKLMN",
                UPRN = 10024389298,
                USRN = 21320239,
                ParentUPRN = 10024389282,
                AddressStatus = "Historical",
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

            response.Address.AddressShouldEqual(address);
        }

        private void SetupValidatorToReturnValid(bool valid = true)
        {
            var result = new Mock<ValidationResult>();

            result.Setup(x => x.IsValid).Returns(valid);
            _fakeValidator.Setup(x => x.Validate(It.IsAny<GetAddressRequest>()))
                .Returns(result.Object);
        }
    }
}
