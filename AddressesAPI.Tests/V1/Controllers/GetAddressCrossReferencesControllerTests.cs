using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AddressesAPI.V1.Boundary.Requests;
using AddressesAPI.V1.Boundary.Responses;
using AddressesAPI.V1.Boundary.Responses.Data;
using AddressesAPI.V1.Boundary.Responses.Metadata;
using AddressesAPI.V1.Controllers;
using AddressesAPI.V1.UseCase.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace AddressesAPI.Tests.V1.Controllers
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
                .ReturnsAsync(new GetAddressCrossReferenceResponse
                {
                    AddressCrossReferences = new List<AddressCrossReference>()
                });
            long uprn = 12345;

            //act
            var response = _classUnderTest.GetAddressCrossReference(uprn) as OkObjectResult;
            //assert
            response.Should().NotBeNull();
            response.Should().BeOfType<OkObjectResult>();
        }
    }
}
