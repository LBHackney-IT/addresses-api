using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private Mock<IAddressesGateway> _addressGateway;
        private Mock<ISearchAddressValidator> _fakeValidator;
        private Faker _faker = new Faker();
        private Fixture _fixture = new Fixture();
        private Mock<ISearchAddressesGateway> _searchAddressGateway;

        [SetUp]
        public void SetUp()
        {
            _addressGateway = new Mock<IAddressesGateway>();
            _searchAddressGateway = new Mock<ISearchAddressesGateway>();
            _fakeValidator = new Mock<ISearchAddressValidator>();
            _classUnderTest = new SearchAddressUseCase(
                _addressGateway.Object, _fakeValidator.Object, _searchAddressGateway.Object);
        }

        [Test]
        public void GivenValidInput_WhenExecute_WillPassAllSearchParametersToTheSearchGateway()
        {
            SetupValidatorToReturnValid();

            var request = new SearchAddressRequest
            {
                Query = _faker.Address.FullAddress(),
                Postcode = "RM3 0FS",
                AddressScope = "HackneyGazetteer",
                Page = _faker.Random.Int(),
                Street = _faker.Address.StreetAddress(),
                UsageCode = _faker.Random.String2(4),
                UsagePrimary = _faker.Random.Word(),
                AddressStatus = "approved",
                BuildingNumber = _faker.Address.BuildingNumber(),
                PageSize = _faker.Random.Int(10, 40),
                UPRN = _faker.Random.Long(0, 9999999999),
                USRN = _faker.Random.Int(0, 9999999),
                IncludePropertyShells = _faker.Random.Bool(),
                CrossRefCode = "123DEF",
                CrossRefValue = "20000",
                ModifiedSince = "2019-03-05"
            };
            _searchAddressGateway.Setup(s => s.SearchAddresses(It.Is<SearchParameters>(
                    x =>
                        x.Gazetteer == GlobalConstants.Gazetteer.Hackney
                        && x.OutOfBoroughAddress
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
                        && x.IncludePropertyShells == request.IncludePropertyShells
                        && x.CrossRefCode == request.CrossRefCode
                        && x.CrossRefValue == request.CrossRefValue
                        && x.AddressQuery == request.Query
                        && x.ModifiedSince.Value.ToString("yyyy-MM-dd") == request.ModifiedSince)))
                .ReturnsAsync((new List<string>(), 1)).Verifiable();

            _classUnderTest.ExecuteAsync(request);
            _addressGateway.Verify();
        }

        [TestCase("HackneyBorough", GlobalConstants.Gazetteer.Hackney, false)]
        [TestCase("hackney borough", GlobalConstants.Gazetteer.Hackney, false)]
        [TestCase("HackneyGazetteer", GlobalConstants.Gazetteer.Hackney, true)]
        [TestCase("hackney gazetteer", GlobalConstants.Gazetteer.Hackney, true)]
        [TestCase("National", GlobalConstants.Gazetteer.Both, true)]
        public void ExecuteAsync_CorrectlyMapsAddressScopeToGazetteerAndOutOfBorough(string addressScope,
            GlobalConstants.Gazetteer expectedGazetteer, bool expectedOutOfBorough)
        {
            SetupValidatorToReturnValid();
            var request = new SearchAddressRequest
            {
                Postcode = "E8",
                AddressScope = addressScope
            };
            _searchAddressGateway.Setup(s => s.SearchAddresses(It.Is<SearchParameters>(i =>
                    i.Gazetteer.Equals(expectedGazetteer) && i.OutOfBoroughAddress.Equals(expectedOutOfBorough))))
                .ReturnsAsync((null, 0)).Verifiable();
            _classUnderTest.ExecuteAsync(request);
            _addressGateway.Verify();
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
            _searchAddressGateway.Setup(s => s.SearchAddresses(It.Is<SearchParameters>(i =>
                    i.AddressStatus.SequenceEqual(expectedList))))
                .ReturnsAsync((null, 0)).Verifiable();
            _classUnderTest.ExecuteAsync(request);
            _addressGateway.Verify();
        }

        [Test]
        public void IfCrossReferenceCodeAndValueAreSupplied_GetsTheMatchingUprnsFromAddressGateway()
        {
            var code = _faker.Random.String2(6);
            var value = _faker.Random.String2(12);
            SetupValidatorToReturnValid();
            var request = new SearchAddressRequest
            {
                CrossRefCode = code,
                CrossRefValue = value
            };
            var uprns = _fixture.CreateMany<long>().ToList();
            _addressGateway.Setup(s => s.GetMatchingCrossReferenceUprns(code, value))
                .Returns(uprns).Verifiable();
            _searchAddressGateway.Setup(s => s.SearchAddresses(It.Is<SearchParameters>(i =>
                    i.CrossReferencedUprns.SequenceEqual(uprns))))
                .ReturnsAsync((null, 0)).Verifiable();
            _classUnderTest.ExecuteAsync(request);

            _addressGateway.Verify();
            _searchAddressGateway.Verify();
        }

        [Test]
        public async Task GivenValidInput_WhenSearchGatewayRespondsWithNull_ThenResponseShouldBeNull()
        {
            SetupValidatorToReturnValid();
            //arrange
            _searchAddressGateway.Setup(s => s.SearchAddresses(It.IsAny<SearchParameters>()))
                .ReturnsAsync((null, 0));

            //act
            var response = await _classUnderTest.ExecuteAsync(new SearchAddressRequest()).ConfigureAwait(true);
            //assert
            response.Addresses.Should().BeNull();
        }

        [TestCase(GlobalConstants.Format.Simple)]
        [TestCase(GlobalConstants.Format.Detailed)]
        public async Task GivenValidInput_Execute_ShouldRetrieveAddressDetailsFromTheGateway(GlobalConstants.Format format)
        {
            SetupValidatorToReturnValid();

            var addresses = _fixture.CreateMany<Address>().ToList();
            var addressKeyList = _fixture.CreateMany<string>().ToList();
            var postcode = "RM3 0FS";
            var request = new SearchAddressRequest
            {
                Postcode = postcode,
                Format = format.ToString()
            };
            _searchAddressGateway.Setup(s =>
                    s.SearchAddresses(It.Is<SearchParameters>(i => i.Postcode.Equals(postcode))))
                .ReturnsAsync((addressKeyList, 1));
            _addressGateway.Setup(s => s.GetAddresses(addressKeyList, format))
                .Returns(addresses);

            var response = await _classUnderTest.ExecuteAsync(request).ConfigureAwait(true);

            response.Should().NotBeNull();
            response.Addresses.Should().BeEquivalentTo(addresses.ToResponse());
        }

        [Test]
        public async Task GivenValidInput_WhenExecute_ThenAddressesShouldBeReturnedWithCount()
        {
            SetupValidatorToReturnValid();

            var totalCount = _faker.Random.Int(30, 200);
            var numberOfAddressesInPage = _faker.Random.Int(3, 30);
            var addresses = _fixture.CreateMany<Address>(numberOfAddressesInPage).ToList();
            var addressKeyList = _fixture.CreateMany<string>().ToList();

            _searchAddressGateway.Setup(s =>
                    s.SearchAddresses(It.IsAny<SearchParameters>()))
                .ReturnsAsync((addressKeyList, totalCount));
            _addressGateway.Setup(s => s.GetAddresses(addressKeyList, GlobalConstants.Format.Simple))
                .Returns(addresses);

            var response = await _classUnderTest.ExecuteAsync(new SearchAddressRequest()).ConfigureAwait(false);

            response.Addresses.Count.Should().Be(numberOfAddressesInPage);
            response.TotalCount.Should().Be(totalCount);
        }

        [Test]
        public async Task GivenPageZero_WhenExecute_ReturnsPageOneAsync()
        {
            SetupValidatorToReturnValid();
            //arrange
            var request = new SearchAddressRequest
            {
                Page = 0
            };
            //act
            await _classUnderTest.ExecuteAsync(request).ConfigureAwait(true);

            //assert
            _searchAddressGateway.Verify(s => s.SearchAddresses(
                It.Is<SearchParameters>(i => i.Page.Equals(1))));
        }

        [Test]
        public void GivenInvalidInput_WhenExecute_ThrowsValidationError()
        {
            SetupValidatorToReturnValid(false);
            Func<Task<SearchAddressResponse>> testDelegate = async () => await _classUnderTest.ExecuteAsync(new SearchAddressRequest()).ConfigureAwait(true);
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
