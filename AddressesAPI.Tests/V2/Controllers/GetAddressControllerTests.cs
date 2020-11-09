using AddressesAPI.V2.Boundary.Requests;
using AddressesAPI.V2.Boundary.Responses;
using AddressesAPI.V2.Boundary.Responses.Data;
using AddressesAPI.V2.Controllers;
using AddressesAPI.V2.UseCase.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace AddressesAPI.Tests.V2.Controllers
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
        public void GivenValidAddressKey_WhenCallingGet_ThenShouldReturn200()
        {
            _mock.Setup(s => s.ExecuteAsync(It.IsAny<GetAddressRequest>()))
                .Returns(new GetAddressResponse
                {
                    Address = new AddressResponse()
                });
            var lpi_key = "ABCDEFGHIJKLMN";

            var response = _classUnderTest.GetAddress(lpi_key);

            response.Should().NotBeNull();
            response.Should().BeOfType<OkObjectResult>();
            (response as OkObjectResult).StatusCode.Should().Be(200);
        }

        [Test]
        public void WhenNoAddressIsProvidedByTheUsecase_ThenShouldReturn404()
        {
            _mock.Setup(s => s.ExecuteAsync(It.IsAny<GetAddressRequest>()))
                .Returns(new GetAddressResponse());
            var lpi_key = "ABCDEFGHIJKLMN";

            var response = _classUnderTest.GetAddress(lpi_key);

            response.Should().NotBeNull();
            response.Should().BeOfType<NotFoundResult>();
            (response as NotFoundResult).StatusCode.Should().Be(404);
        }
    }
}
