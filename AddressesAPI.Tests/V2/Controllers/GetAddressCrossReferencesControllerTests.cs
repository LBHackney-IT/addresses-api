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
    public class GetPropertiesCrossReferenceControllerTests
    {
        private GetPropertiesCrossReferenceController _classUnderTest;
        private Mock<IGetPropertiesCrossReferenceUseCase> _mock;

        public GetPropertiesCrossReferenceControllerTests()
        {
            _mock = new Mock<IGetPropertiesCrossReferenceUseCase>();
            _classUnderTest = new GetPropertiesCrossReferenceController(_mock.Object);
        }

        [Test]
        public void GivenValidAddressRequest_WhenCallingGet_ThenShouldReturnAPIResponseListOfAddresses()
        {
            //arrange
            _mock.Setup(s => s.ExecuteAsync(It.IsAny<GetPropertiesCrossReferenceRequest>()))
                .Returns(new GetPropertiesCrossReferenceResponse
                {
                    AddressCrossReferences = new List<AddressCrossReferenceResponse>()
                });
            long uprn = 12345;

            //act
            var response = _classUnderTest.GetPropertiesCrossReference(uprn);
            //assert
            response.Should().NotBeNull();
            response.Should().BeOfType<OkObjectResult>();
            var objectResult = response as OkObjectResult;
            var getAddresses = objectResult?.Value as APIResponse<GetPropertiesCrossReferenceResponse>;
            getAddresses.Should().NotBeNull();
        }
    }
}
