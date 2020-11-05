using AddressesAPI.V1.Boundary.Requests;
using AddressesAPI.V1.UseCase;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace AddressesAPI.Tests.V1.UseCase
{
    public class GetCrossReferenceRequestValidatorTests
    {
        private GetCrossReferenceRequestValidator _classUnderTest;

        [SetUp]
        public void SetUp()
        {
            _classUnderTest = new GetCrossReferenceRequestValidator();
        }

        [Test]
        public void GivenNullUprn_ShouldThrowValidationError()
        {
            //arrange
            var request = new GetAddressCrossReferenceRequest();
            //act & assert
            _classUnderTest.ShouldHaveValidationErrorFor(x => x.uprn, request)
                .WithErrorMessage("UPRN must be provided");
        }
    }
}
