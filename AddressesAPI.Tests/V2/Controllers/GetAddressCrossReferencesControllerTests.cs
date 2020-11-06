using System.Collections.Generic;
using AddressesAPI.V2.Boundary.Requests;
using AddressesAPI.V2.Boundary.Responses;
using AddressesAPI.V2.Boundary.Responses.Data;
using AddressesAPI.V2.Boundary.Responses.Metadata;
using AddressesAPI.V2.Controllers;
using AddressesAPI.V2.UseCase.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace AddressesAPI.Tests.V2.Controllers
{
    public class GetAddressCrossReferenceControllerTests
    {
        private GetAddressCrossReferenceController _classUnderTest;
        private Mock<IGetAddressCrossReferenceUseCase> _mock;

        public GetAddressCrossReferenceControllerTests()
        {
            _mock = new Mock<IGetAddressCrossReferenceUseCase>();
            _classUnderTest = new GetAddressCrossReferenceController(_mock.Object);
        }

        [Test]
        public void GivenValidAddressRequest_WhenCallingGet_ThenShouldReturnAPIResponseListOfAddresses()
        {
            //arrange
            _mock.Setup(s => s.ExecuteAsync(It.IsAny<GetAddressCrossReferenceRequest>()))
                .Returns(new GetAddressCrossReferenceResponse
                {
                    AddressCrossReferences = new List<AddressCrossReferenceResponse>()
                });
            long uprn = 12345;

            //act
            var response = _classUnderTest.GetAddressCrossReference(uprn);
            //assert
            response.Should().NotBeNull();
            response.Should().BeOfType<OkObjectResult>();
            var objectResult = response as OkObjectResult;
            var getAddresses = objectResult?.Value as APIResponse<GetAddressCrossReferenceResponse>;
            getAddresses.Should().NotBeNull();
        }

        [Test]
        public void IfPropertyCanNotBeFound_ReturnsA404StatusCode()
        {
            //arrange
            _mock.Setup(s => s.ExecuteAsync(It.IsAny<GetAddressCrossReferenceRequest>()))
                .Returns((GetAddressCrossReferenceResponse) null);
            long uprn = 12345;

            //act
            var response = _classUnderTest.GetAddressCrossReference(uprn);
            //assert
            response.Should().NotBeNull();
            response.Should().BeOfType<NotFoundResult>();
            var objectResult = response as NotFoundResult;
            objectResult.StatusCode.Should().Be(404);
        }
    }
}
