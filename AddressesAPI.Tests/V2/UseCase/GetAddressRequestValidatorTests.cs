using AddressesAPI.V2.Boundary.Requests;
using AddressesAPI.V2.UseCase;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace AddressesAPI.Tests.V2.UseCase
{
    public class GetAddressRequestValidatorTests
    {
        private GetAddressRequestValidator _classUnderTest;

        [SetUp]
        public void SetUp()
        {
            _classUnderTest = new GetAddressRequestValidator();
        }

        [Test]
        public void GivenString15Characters_WhenExecuteAsync_TheShouldThrowException()
        {
            var request = new GetAddressRequest { addressID = "ABCDEFGHIJKLMNO" };
            //act & assert
            _classUnderTest.ShouldHaveValidationErrorFor(x => x.addressID, request)
                .WithErrorMessage("addressID must be 14 characters");
        }

        [Test]
        public void GivenBlankStringInput_WhenExecuteAsync_ThenShouldThrowException()
        {
            //arrange
            var request = new GetAddressRequest { addressID = string.Empty };
            //act & assert
            _classUnderTest.ShouldHaveValidationErrorFor(x => x.addressID, request)
                .WithErrorMessage("addressID must be provided");
        }

        [Test]
        public void GivenString13Characters_WhenExecuteAsync_TheShouldThrowException()
        {
            var request = new GetAddressRequest { addressID = "ABCDEFGHIJKLM" };
            //act & assert
            _classUnderTest.ShouldHaveValidationErrorFor(x => x.addressID, request)
                .WithErrorMessage("addressID must be 14 characters");
        }
    }
}
