using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AddressesAPI.V1;
using AddressesAPI.V1.Boundary.Requests;
using AddressesAPI.V1.Boundary.Responses;
using AddressesAPI.V1.Boundary.Responses.Data;
using AddressesAPI.V1.Boundary.Responses.Metadata;
using AddressesAPI.V1.Controllers;
using AddressesAPI.V1.UseCase;
using AddressesAPI.V1.UseCase.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace AddressesAPI.Tests.V1.Controllers
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
            _classUnderTest = new SearchAddressController(_mock.Object, validator);

            _classUnderTest.ControllerContext = new ControllerContext();
            _classUnderTest.ControllerContext.HttpContext = new DefaultHttpContext();
            _classUnderTest.ControllerContext.HttpContext.Request.QueryString = new QueryString("");

        }


        [TestCase("RM3 0FS", GlobalConstants.Gazetteer.Local)]
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
                PostCode = postcode,
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
