using System;
using System.Collections.Generic;
using AddressesAPI.Tests.V1.Helper;
using AddressesAPI.V1;
using AddressesAPI.V1.Boundary.Requests;
using AddressesAPI.V1.Boundary.Responses;
using AddressesAPI.V1.Boundary.Responses.Metadata;
using AddressesAPI.V1.Domain;
using AddressesAPI.V1.Factories;
using AddressesAPI.V1.Gateways;
using AddressesAPI.V1.UseCase;
using AddressesAPI.V1.UseCase.Interfaces;
using FluentAssertions;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;

namespace AddressesAPI.Tests.V1.UseCase
{
    public class SearchAddressUseCaseTest
    {
        private ISearchAddressUseCase _classUnderTest;
        private Mock<IAddressesGateway> _fakeGateway;
        private Mock<ISearchAddressValidator> _fakeValidator;

        [SetUp]
        public void SetUp()
        {
            _fakeGateway = new Mock<IAddressesGateway>();
            _fakeValidator = new Mock<ISearchAddressValidator>();
            _classUnderTest = new SearchAddressUseCase(_fakeGateway.Object, _fakeValidator.Object);
        }

        [Test]
        public void GivenLocalGazetteer_WhenExecuteAsync_ThenOnlyLocalAddressesShouldBeReturned()
        {
            SetupValidatorToReturnValid();
            var addresses = new List<Address>
            {
                new Address
                {
                    AddressKey = "ABCDEFGHIJKLMN", UPRN = 10024389298,USRN = 21320239,ParentUPRN = 10024389282,AddressStatus = "Approved Preferred",UnitName = "FLAT 16",UnitNumber = "",BuildingName = "HAZELNUT COURT",BuildingNumber = "1",Street = "FIRWOOD LANE",Postcode = "RM3 0FS",Locality = "",Gazetteer = "NATIONAL",CommercialOccupier = "",UsageDescription = "Unclassified, Awaiting Classification",UsagePrimary = "Unclassified", UsageCode = "UC",PropertyShell = false,HackneyGazetteerOutOfBoroughAddress = false,Easting = 554189.4500,Northing = 190281.1000,Longitude = 0.2244347,Latitude = 51.590289
                },
                new Address
                {
                    AddressKey = "ABCDEFGHIJKLM2", UPRN = 10024389298,USRN = 21320239,ParentUPRN = 10024389282,AddressStatus = "Approved Preferred",UnitName = "FLAT 16",UnitNumber = "",BuildingName = "HAZELNUT COURT",BuildingNumber = "1",Street = "FIRWOOD LANE",Postcode = "RM3 0FS",Locality = "",Gazetteer = "LOCAL",CommercialOccupier  = "",UsageDescription = "Unclassified, Awaiting Classification",UsagePrimary = "Unclassified", UsageCode = "UC",PropertyShell = false,HackneyGazetteerOutOfBoroughAddress = false,Easting = 554189.4500,Northing = 190281.1000,Longitude = 0.2244347,Latitude = 51.590289
                }
            };

            var postcode = "RM3 0FS";
            var request = new SearchAddressRequest
            {
                PostCode = postcode,
                Gazetteer = GlobalConstants.Gazetteer.Hackney.ToString()
            };
            _fakeGateway.Setup(s => s.SearchAddresses(It.Is<SearchParameters>(i =>
                    i.Postcode.Equals(postcode) && i.Gazetteer == GlobalConstants.Gazetteer.Hackney)))
                .Returns((addresses, 1));

            var response = _classUnderTest.ExecuteAsync(request);
            response.Should().NotBeNull();
            response.Addresses.Count.Should().Equals(1);
            response.TotalCount.Should().Equals(1);
            response.Addresses.Should().BeEquivalentTo(addresses.ToResponse());
        }

        [Test]
        public void GivenLocalGazetteer_WhenExecuteAsync_InterpretsThisAsHackneyGazetteer()
        {
            SetupValidatorToReturnValid();
            var request = new SearchAddressRequest
            {
                PostCode = "RM3 0FS",
                Gazetteer = "Local"
            };
            _fakeGateway.Setup(s => s.SearchAddresses(It.Is<SearchParameters>(i =>
                    i.Gazetteer == GlobalConstants.Gazetteer.Hackney)))
                .Returns((new List<Address>(), 1)).Verifiable();

            _classUnderTest.ExecuteAsync(request);
            _fakeGateway.Verify();
        }

        [Test]
        public void GivenValidInput_WhenGatewayRespondsWithNull_ThenResponseShouldBeNull()
        {
            SetupValidatorToReturnValid();
            //arrange
            var postcode = "RM3 0FS";

            _fakeGateway.Setup(s => s.SearchAddresses(
                    It.Is<SearchParameters>(i => i.Postcode.Equals(postcode))))
                .Returns((null, 0));

            var request = new SearchAddressRequest
            {
                PostCode = postcode
            };
            //act
            var response = _classUnderTest.ExecuteAsync(request);
            //assert
            response.Addresses.Should().BeNull();
        }

        [Test]
        public void GivenValidPostCode_WhenExecuteAsync_ThenMultipleAddressesShouldBeReturned()
        {
            SetupValidatorToReturnValid();
            var addresses = new List<Address>
            {
                new Address
                {
                    AddressKey = "ABCDEFGHIJKLMN", UPRN = 10024389298,USRN = 21320239,ParentUPRN = 10024389282,AddressStatus = "Approved Preferred",UnitName = "FLAT 16",UnitNumber = "",BuildingName = "HAZELNUT COURT",BuildingNumber = "1",Street = "FIRWOOD LANE",Postcode = "RM3 0FS",Locality = "",Gazetteer = "NATIONAL",CommercialOccupier = "",UsageDescription = "Unclassified, Awaiting Classification",UsagePrimary = "Unclassified",                UsageCode = "UC",PropertyShell = false,HackneyGazetteerOutOfBoroughAddress = false,Easting = 554189.4500,Northing = 190281.1000,Longitude = 0.2244347,Latitude = 51.590289
                },
                new Address
                {
                    AddressKey = "ABCDEFGHIJKLM2", UPRN = 10024389298,USRN = 21320239,ParentUPRN = 10024389282,AddressStatus = "Approved Preferred",UnitName = "FLAT 16",UnitNumber = "",BuildingName = "HAZELNUT COURT",BuildingNumber = "1",Street = "FIRWOOD LANE",Postcode = "RM3 0FS",Locality = "",Gazetteer = "NATIONAL",CommercialOccupier = "",UsageDescription = "Unclassified, Awaiting Classification",UsagePrimary = "Unclassified",                UsageCode = "UC",PropertyShell = false,HackneyGazetteerOutOfBoroughAddress = false,Easting = 554189.4500,Northing = 190281.1000,Longitude = 0.2244347,Latitude = 51.590289
                }
            };

            var postcode = "RM3 0FS";
            var request = new SearchAddressRequest
            {
                PostCode = postcode
            };
            _fakeGateway.Setup(s =>
                    s.SearchAddresses(It.Is<SearchParameters>(i => i.Postcode.Equals("RM3 0FS"))))
                .Returns((addresses, 2));

            var response = _classUnderTest.ExecuteAsync(request);
            response.Should().NotBeNull();
            response.Addresses.Count.Should().Equals(2);
            response.TotalCount.Should().Equals(2);
        }

        [Test]
        public void GivenValidPostCode_WhenExecuteAsync_ThenAddressShouldBeReturned()
        {
            SetupValidatorToReturnValid();
            var addresses = new List<Address>();
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
            addresses.Add(address);
            var postcode = "RM3 0FS";
            var request = new SearchAddressRequest
            {
                PostCode = postcode
            };
            _fakeGateway.Setup(s =>
                    s.SearchAddresses(It.Is<SearchParameters>(i => i.Postcode.Equals("RM3 0FS"))))
                .Returns((addresses, 1));

            var response = _classUnderTest.ExecuteAsync(request);

            response.Should().NotBeNull();
            response.Addresses[0].AddressShouldEqual(address);
        }

        [Test]
        public void GivenPageZero_WhenExecuteAsync_ReturnsPageOne()
        {
            SetupValidatorToReturnValid();
            //arrange
            var request = new SearchAddressRequest
            {
                Page = 0
            };
            //act
            _classUnderTest.ExecuteAsync(request);

            //assert
            _fakeGateway.Verify(s => s.SearchAddresses(
                It.Is<SearchParameters>(i => i.Page.Equals(1))));
        }

        [Test]
        public void GivenInvalidInput_WhenExecuteAsync_ThrowsValidationError()
        {
            SetupValidatorToReturnValid(false);
            Func<SearchAddressResponse> testDelegate = () => _classUnderTest.ExecuteAsync(new SearchAddressRequest());
            testDelegate.Should().Throw<BadRequestException>();
        }

        private void SetupValidatorToReturnValid(bool valid = true)
        {
            var result = new Mock<ValidationResult>();
            result.Setup(x => x.IsValid).Returns(valid);
            _fakeValidator.Setup(x => x.Validate(It.IsAny<SearchAddressRequest>()))
                .Returns(result.Object);
        }
    }
}
