using System.Collections.Generic;
using AddressesAPI.V1.Boundary.Requests;
using AddressesAPI.V1.Boundary.Responses;
using AddressesAPI.V1.Boundary.Responses.Data;
using AddressesAPI.V1.Controllers;
using AddressesAPI.V1.UseCase.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace AddressesAPI.Tests.V1.Controllers
{
    public class GetAddressControllerTests
    {
        private GetAddressController _classUnderTest;
        private Mock<IGetSingleAddressUseCase> _mock;

        public GetAddressControllerTests()
        {
            _mock = new Mock<IGetSingleAddressUseCase>();
            _classUnderTest = new GetAddressController(_mock.Object);
        }

        [Test]
        public void GivenValidSearchAddressRequest_WhenCallingGet_ThenShouldReturn200()
        {
            _mock.Setup(s => s.ExecuteAsync(It.IsAny<GetAddressRequest>()))
                .Returns(new SearchAddressResponse
                {
                    Addresses = new List<AddressResponse>()
                });
            var lpi_key = "ABCDEFGHIJKLMN";

            var response = _classUnderTest.GetAddress(lpi_key);

            response.Should().NotBeNull();
            response.Should().BeOfType<ObjectResult>();
            var objectResult = response as ObjectResult;
            objectResult.StatusCode.Should().Be(200);
        }
    }
}
