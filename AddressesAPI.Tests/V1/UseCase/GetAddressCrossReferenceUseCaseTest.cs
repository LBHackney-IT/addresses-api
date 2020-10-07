using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AddressesAPI.V1.Boundary.Requests;
using AddressesAPI.V1.Domain;
using AddressesAPI.V1.Gateways;
using AddressesAPI.V1.UseCase;
using AddressesAPI.V1.UseCase.Interfaces;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace AddressesAPI.Tests.V1.UseCase
{
    public class GetAddressCrossReferenceUseCaseTest
    {
        private IGetAddressCrossReferenceUseCase _classUnderTest;
        private Mock<IAddressesGatewayTSQL> _fakeGateway;

        [SetUp]
        public void SetUp()
        {
            _fakeGateway = new Mock<IAddressesGatewayTSQL>();
            _classUnderTest = new GetAddressCrossReferenceUseCase(_fakeGateway.Object);
        }

        [Test]
        public async Task GivenValidInput_WhenExecuteAsync_GatewayReceivesCorrectInputLength()
        {

            var uprn = 1234578912;
            _fakeGateway.Setup(s => s.GetAddressCrossReferenceAsync(1234578912)).ReturnsAsync(new List<AddressCrossReference>()).Verifiable();

            var request = new GetAddressCrossReferenceRequest
            {
                uprn = uprn
            };

            await _classUnderTest.ExecuteAsync(request).ConfigureAwait(true);

            _fakeGateway.Verify();
        }

        [Test]
        public async Task GivenValidInput_WhenGatewayRespondsWithEmptyList_ThenResponseShouldBeEmptyList()
        {
            //arrange
            var uprn = 1234578912;

            _fakeGateway.Setup(s => s.GetAddressCrossReferenceAsync(1234578912))
                .ReturnsAsync(new List<AddressCrossReference>());

            var request = new GetAddressCrossReferenceRequest
            {
                uprn = uprn
            };
            //act
            var response = await _classUnderTest.ExecuteAsync(request).ConfigureAwait(true);
            //assert
            response.AddressCrossReferences.Count.Should().Be(0);
        }

        [Test]
        public async Task GivenUPRN_WhenExecuteAsync_ThenMatchingCrossReferencesShouldBeReturned()
        {
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
            _fakeGateway.Setup(s => s.GetAddressCrossReferenceAsync(10024389298))
                .ReturnsAsync(crossReferences);

            var response = await _classUnderTest.ExecuteAsync(request).ConfigureAwait(true);
            response.Should().NotBeNull();
            response.AddressCrossReferences.Count.Should().Be(1);
            response.AddressCrossReferences.First().UPRN.Should().Be(uprn);
        }

        [Test]
        public async Task GivenUPRN_WhenExecuteAsync_ThenOnlyMatchingCrossReferencesShouldBeReturned()
        {
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
            _fakeGateway.Setup(s => s.GetAddressCrossReferenceAsync(10024389298))
                .ReturnsAsync(crossReferences);

            var response = await _classUnderTest.ExecuteAsync(request).ConfigureAwait(true);
            response.Should().NotBeNull();
            response.AddressCrossReferences.Count.Should().Be(3);
            response.AddressCrossReferences.First().UPRN.Should().Be(uprn);
        }
    }
}
