using System.Collections.Generic;
using AddressesAPI.V2;
using AddressesAPI.V2.Boundary.Requests;
using AddressesAPI.V2.Boundary.Responses;
using AddressesAPI.V2.Boundary.Responses.Data;
using AddressesAPI.V2.Boundary.Responses.Metadata;
using AddressesAPI.V2.Controllers;
using AddressesAPI.V2.UseCase;
using AddressesAPI.V2.UseCase.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace AddressesAPI.Tests.V2.Controllers
{
    public class SearchAddressControllerTests
    {
        private SearchAddressController _classUnderTest;
        private Mock<ISearchAddressUseCase> _mock;

        [SetUp]
        public void SetUp()
        {
            _mock = new Mock<ISearchAddressUseCase>();
            var validator = new SearchAddressValidator();
            _classUnderTest = new SearchAddressController(_mock.Object);

            _classUnderTest.ControllerContext = new ControllerContext();
            _classUnderTest.ControllerContext.HttpContext = new DefaultHttpContext();
            _classUnderTest.ControllerContext.HttpContext.Request.QueryString = new QueryString("");

        }


        [TestCase("RM3 0FS", GlobalConstants.Gazetteer.Hackney)]
        [TestCase("IG11 7QD", GlobalConstants.Gazetteer.Both)]
        public void GivenValidSearchAddressRequest_WhenCallingGet_ThenShouldReturnAPIResponseListOfAddresses(string postcode, GlobalConstants.Gazetteer gazetteer)
        {
            //arrange
            _mock.Setup(s => s.ExecuteAsync(It.IsAny<SearchAddressRequest>()))
                .Returns(new SearchAddressResponse
                {
                    Addresses = new List<AddressResponse>()
                });

            var request = new SearchAddressRequest
            {
                Postcode = postcode,
                Gazetteer = gazetteer.ToString()
            };
            //act
            var response = _classUnderTest.GetAddresses(request);
            //assert
            response.Should().NotBeNull();
            response.Should().BeOfType<OkObjectResult>();
            var objectResult = response as OkObjectResult;
            var getAddresses = objectResult?.Value as APIResponse<SearchAddressResponse>;
            getAddresses.Should().NotBeNull();
        }

    }
}
