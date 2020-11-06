using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AddressesAPI.V1.Boundary.Requests;
using AddressesAPI.V1.Boundary.Responses;
using AddressesAPI.V1.Boundary.Responses.Metadata;
using AddressesAPI.V1.Domain;
using AddressesAPI.V1.Gateways;
using AddressesAPI.V1.UseCase;
using AddressesAPI.V1.UseCase.Interfaces;
using FluentAssertions;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;

namespace AddressesAPI.Tests.V1.UseCase
{
    public class GetAddressCrossReferenceUseCaseTest
    {
        private IGetAddressCrossReferenceUseCase _classUnderTest;
        private Mock<ICrossReferencesGateway> _fakeGateway;
        private Mock<IGetCrossReferenceRequestValidator> _validator;

        [SetUp]
        public void SetUp()
        {
            _fakeGateway = new Mock<ICrossReferencesGateway>();
            _validator = new Mock<IGetCrossReferenceRequestValidator>();
            _classUnderTest = new GetAddressCrossReferenceUseCase(_fakeGateway.Object, _validator.Object);
        }

        [Test]
        public void GivenValidInput_WhenExecuteAsync_GatewayReceivesCorrectInputLength()
        {
            SetupValidatorToReturnValid();
            var uprn = 1234578912;
            _fakeGateway.Setup(s => s.GetAddressCrossReference(1234578912)).Returns(new List<AddressCrossReference>()).Verifiable();

            var request = new GetAddressCrossReferenceRequest
            {
                uprn = uprn
            };

            _classUnderTest.ExecuteAsync(request);

            _fakeGateway.Verify();
        }

        [Test]
        public void GivenValidInput_WhenGatewayRespondsWithEmptyList_ThenResponseShouldBeEmptyList()
        {
            SetupValidatorToReturnValid();
            //arrange
            var uprn = 1234578912;

            _fakeGateway.Setup(s => s.GetAddressCrossReference(1234578912))
                .Returns(new List<AddressCrossReference>());

            var request = new GetAddressCrossReferenceRequest
            {
                uprn = uprn
            };
            //act
            var response = _classUnderTest.ExecuteAsync(request);
            //assert
            response.AddressCrossReferences.Count.Should().Be(0);
        }

        [Test]
        public void GivenUPRN_WhenExecuteAsync_ThenMatchingCrossReferencesShouldBeReturned()
        {
            SetupValidatorToReturnValid();
            var crossReferences = new List<AddressCrossReference>
            {
                new AddressCrossReference
                {
                    UPRN = 10024389298,
                    Code = "", CrossRefKey ="" ,
                    Name ="" , Value ="",
                    EndDate = DateTime.Today
                }
            };

            var uprn = 10024389298;
            var request = new GetAddressCrossReferenceRequest
            {
                uprn = uprn
            };
            _fakeGateway.Setup(s => s.GetAddressCrossReference(10024389298))
                .Returns(crossReferences);

            var response = _classUnderTest.ExecuteAsync(request);
            response.Should().NotBeNull();
            response.AddressCrossReferences.Count.Should().Be(1);
            response.AddressCrossReferences.First().UPRN.Should().Be(uprn);
        }

        [Test]
        public void GivenUPRN_WhenExecuteAsync_ThenOnlyMatchingCrossReferencesShouldBeReturned()
        {
            SetupValidatorToReturnValid();
            var crossReferences = new List<AddressCrossReference>
            {
                new AddressCrossReference
                {
                    UPRN = 10024389298, Code = "", CrossRefKey ="" , Name ="" , Value ="", EndDate = DateTime.Today
                },
                new AddressCrossReference
                {
                    UPRN = 10024389298, Code = "", CrossRefKey ="" , Name ="" , Value ="", EndDate = DateTime.Today
                },
                new AddressCrossReference
                {
                    UPRN = 10024389291, Code = "", CrossRefKey ="" , Name ="" , Value ="", EndDate = DateTime.Today
                }
            };

            var uprn = 10024389298;
            var request = new GetAddressCrossReferenceRequest
            {
                uprn = uprn
            };
            _fakeGateway.Setup(s => s.GetAddressCrossReference(10024389298))
                .Returns(crossReferences);

            var response = _classUnderTest.ExecuteAsync(request);
            response.Should().NotBeNull();
            response.AddressCrossReferences.Count.Should().Be(3);
            response.AddressCrossReferences.First().UPRN.Should().Be(uprn);
        }

        [Test]
        public void GivenInvalidRequest_ExecuteAsyncShouldThrowAValidationError()
        {
            SetupValidatorToReturnValid(false);
            Func<GetAddressCrossReferenceResponse> testDelegate = () => _classUnderTest.ExecuteAsync(null);
            testDelegate.Should().Throw<BadRequestException>();
        }

        private void SetupValidatorToReturnValid(bool valid = true)
        {
            var result = new Mock<ValidationResult>();

            result.Setup(x => x.IsValid).Returns(valid);
            _validator.Setup(x => x.Validate(It.IsAny<GetAddressCrossReferenceRequest>()))
                .Returns(result.Object);
        }
    }
}
