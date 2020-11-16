using System;
using System.Collections.Generic;
using System.Linq;
using AddressesAPI.V2;
using AddressesAPI.V2.Boundary.Requests;
using AddressesAPI.V2.Boundary.Responses;
using AddressesAPI.V2.Boundary.Responses.Metadata;
using AddressesAPI.V2.Domain;
using AddressesAPI.V2.Factories;
using AddressesAPI.V2.Gateways;
using AddressesAPI.V2.UseCase;
using AddressesAPI.V2.UseCase.Interfaces;
using AutoFixture;
using Bogus;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace AddressesAPI.Tests.V2.UseCase
{
    public class SearchAddressUseCaseTest
    {
        private ISearchAddressUseCase _classUnderTest;
        private Mock<IAddressesGateway> _fakeGateway;
        private Mock<ISearchAddressValidator> _fakeValidator;
        private Faker _faker = new Faker();
        private Fixture _fixture = new Fixture();

        [SetUp]
        public void SetUp()
        {
            _fakeGateway = new Mock<IAddressesGateway>();
            _fakeValidator = new Mock<ISearchAddressValidator>();
            _classUnderTest = new SearchAddressUseCase(_fakeGateway.Object, _fakeValidator.Object);
        }

        [Test]
        public void GivenValidInput_WhenExecute_WillPassAllSearchParametersToTheGateway()
        {
            SetupValidatorToReturnValid();

            var request = new SearchAddressRequest
            {
                Postcode = "RM3 0FS",
                Gazetteer = "Local",
                Format = "Detailed",
                Page = _faker.Random.Int(),
                Street = _faker.Address.StreetAddress(),
                UsageCode = _faker.Random.String2(4),
                UsagePrimary = _faker.Random.Word(),
                AddressStatus = "approved",
                BuildingNumber = _faker.Address.BuildingNumber(),
                PageSize = _faker.Random.Int(10, 40),
                UPRN = _faker.Random.Long(0, 9999999999),
                USRN = _faker.Random.Int(0, 9999999),
                IncludeParentShells = _faker.Random.Bool(),
                OutOfBoroughAddress = _faker.Random.Bool(),
                CrossRefCode = "123DEF",
                CrossRefValue = "20000"
            };
            _fakeGateway.Setup(s => s.SearchAddresses(It.Is<SearchParameters>(
                    x => x.Format == GlobalConstants.Format.Detailed
                         && x.Gazetteer == GlobalConstants.Gazetteer.Hackney
                         && x.Page == request.Page
                         && x.Postcode == request.Postcode
                         && x.Street == request.Street
                         && x.Uprn == request.UPRN
                         && x.Usrn == request.USRN
                         && x.AddressStatus.SequenceEqual(new[] { "approved" })
                         && x.BuildingNumber == request.BuildingNumber
                         && x.PageSize == request.PageSize
                         && x.UsageCode == request.UsageCode
                         && x.UsagePrimary == request.UsagePrimary
                         && x.IncludeParentShells == request.IncludeParentShells
                         && x.OutOfBoroughAddress == request.OutOfBoroughAddress
                         && x.CrossRefCode == request.CrossRefCode
                         && x.CrossRefValue == request.CrossRefValue)))
                .Returns((new List<Address>(), 1)).Verifiable();

            _classUnderTest.ExecuteAsync(request);
            _fakeGateway.Verify();
        }

        [TestCase("approved,historical", new[] { "approved", "historical" })]
        [TestCase("provisional", new[] { "provisional" })]
        public void ExecuteAsync_CorrectlyConvertsAddressStatusIntoAList(string addressQuery, IEnumerable<string> expectedList)
        {
            SetupValidatorToReturnValid();
            var request = new SearchAddressRequest
            {
                Postcode = "E8",
                AddressStatus = addressQuery
            };
            _fakeGateway.Setup(s => s.SearchAddresses(It.Is<SearchParameters>(i =>
                    i.AddressStatus.SequenceEqual(expectedList))))
                .Returns((null, 0)).Verifiable();
            _classUnderTest.ExecuteAsync(request);
            _fakeGateway.Verify();
        }

        [Test]
        public void GivenLocalGazetteer_WhenExecute_InterpretsThisAsHackneyGazetteer()
        {
            SetupValidatorToReturnValid();
            var request = new SearchAddressRequest
            {
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
            _fakeGateway.Setup(s => s.SearchAddresses(It.IsAny<SearchParameters>()))
                .Returns((null, 0));

            //act
            var response = _classUnderTest.ExecuteAsync(new SearchAddressRequest());
            //assert
            response.Addresses.Should().BeNull();
        }

        [Test]
        public void GivenValidInput_WhenExecute_ThenAddressDetailsShouldBeReturned()
        {
            SetupValidatorToReturnValid();

            var addresses = _fixture.CreateMany<Address>().ToList();

            var postcode = "RM3 0FS";
            var request = new SearchAddressRequest
            {
                Postcode = postcode
            };
            _fakeGateway.Setup(s =>
                    s.SearchAddresses(It.Is<SearchParameters>(i => i.Postcode.Equals(postcode))))
                .Returns((addresses, 1));

            var response = _classUnderTest.ExecuteAsync(request);

            response.Should().NotBeNull();
            response.Addresses.Should().BeEquivalentTo(addresses.ToResponse());
        }

        [Test]
        public void GivenValidInput_WhenExecute_ThenAddressesShouldBeReturnedWithCount()
        {
            SetupValidatorToReturnValid();

            var totalCount = _faker.Random.Int(30, 200);
            var numberOfAddressesInPage = _faker.Random.Int(3, 30);
            var addresses = _fixture.CreateMany<Address>(numberOfAddressesInPage).ToList();

            _fakeGateway.Setup(s =>
                    s.SearchAddresses(It.IsAny<SearchParameters>()))
                .Returns((addresses, totalCount));

            var response = _classUnderTest.ExecuteAsync(new SearchAddressRequest());

            response.Addresses.Count.Should().Be(numberOfAddressesInPage);
            response.TotalCount.Should().Be(totalCount);
        }

        [Test]
        public void GivenPageZero_WhenExecute_ReturnsPageOne()
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
        public void GivenInvalidInput_WhenExecute_ThrowsValidationError()
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
