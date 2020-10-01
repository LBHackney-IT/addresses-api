using AddressesAPI.V1;
using AddressesAPI.V1.Boundary.Requests;
using FluentAssertions;
using NUnit.Framework;

namespace AddressesAPI.Tests.V1
{
    [TestFixture]
    public class SearchAddressRequestTests
    {
        #region Gazetteer
        [Test]
        public void GivenNoInputForGazetteer_WhenSearchAddressRequestObjectIsCreated_ItDefaultsToBoth()
        {
            var classUnderTest = new SearchAddressRequest();
            classUnderTest.Gazetteer.Should().Be(GlobalConstants.Gazetteer.Both);
        }

        [Test]
        public void GivenInputValueLocalForGazetteer_WhenSearchAddressRequestObjectIsCreated_ItIsSetToLocal()
        {
            var classUnderTest = new SearchAddressRequest { Gazetteer = GlobalConstants.Gazetteer.Local };
            classUnderTest.Gazetteer.Should().Be(GlobalConstants.Gazetteer.Local);
        }

        [Test]
        public void GivenInputValueBothForGazetteer_WhenSearchAddressRequestObjectIsCreated_ItIsSetToBoth()
        {
            var classUnderTest = new SearchAddressRequest { Gazetteer = GlobalConstants.Gazetteer.Both };
            classUnderTest.Gazetteer.Should().Be(GlobalConstants.Gazetteer.Both);
        }
        #endregion

        #region HackneyGazetteerOutOfBoroughAddress
        [Test] //Gazetteer -> no input, HGOBAddress -> no input
        public void GivenNoInputValueForGazetteerAndHackneyGazetteerOutOfBoroughAddressParameters_WhenSearchAddressRequestObjectIsCreated_GazetteerIsSetToItsDefaultAndHackneyGazetteerOutOfBoroughAddressIsSetToNullBasedOnThat()
        {
            var classUnderTest = new SearchAddressRequest();
            classUnderTest.HackneyGazetteerOutOfBoroughAddress.Should().BeNull();
        }

        [Test] //Gazetteer = Both, HGOBAddress -> no input
        public void GivenGazetteerValueBothAndNoInputForHackneyGazetteerOutOfBoroughAddress_WhenSearchAddressRequestObjectIsCreated_HackneyGazetteerOutOfBoroughAddressIsSetToNull()
        {
            var classUnderTest = new SearchAddressRequest { Gazetteer = GlobalConstants.Gazetteer.Both };
            classUnderTest.HackneyGazetteerOutOfBoroughAddress.Should().BeNull();
        }

        [Test] //Gazetteer = Local, HGOBAddress -> no input
        public void GivenGazetteerValueLocalAndNoInputForHackneyGazetteerOutOfBoroughAddress_WhenSearchAddressRequestObjectIsCreated_HackneyGazetteerOutOfBoroughAddressIsSetToFalse()
        {
            var classUnderTest = new SearchAddressRequest { Gazetteer = GlobalConstants.Gazetteer.Local };
            classUnderTest.HackneyGazetteerOutOfBoroughAddress.Should().BeFalse();
        }

        [TestCase(GlobalConstants.Gazetteer.Both, false)]
        [TestCase(GlobalConstants.Gazetteer.Both, true)]
        [TestCase(GlobalConstants.Gazetteer.Local, false)]
        [TestCase(GlobalConstants.Gazetteer.Local, true)]
        public void GivenAHackneyGazetteerOutOfBoroughAddressInputValueAndAnyGazetteerValue_WhenSearchAddressRequestObjectIsCreated_HackneyGazetteerOutOfBoroughAddressIsSetToWhateverItsInputWas(GlobalConstants.Gazetteer gazetteer, bool? hackneyGazetteerOutOfBoroughAddress)
        {
            var classUnderTest = new SearchAddressRequest { Gazetteer = gazetteer, HackneyGazetteerOutOfBoroughAddress = hackneyGazetteerOutOfBoroughAddress };
            classUnderTest.HackneyGazetteerOutOfBoroughAddress.Should().Be(hackneyGazetteerOutOfBoroughAddress);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GivenNoInputForGazetteerAndInputValueForHackneyGazetteerOutOfBoroughAddress_WhenSearchAddressRequestObjectIsCreated_HackneyGazetteerOutOfBoroughAddressIsSetToWhateverItsInputWas(bool? hackneyGazetteerOutOfBoroughAddress)
        {
            var classUnderTest = new SearchAddressRequest { HackneyGazetteerOutOfBoroughAddress = hackneyGazetteerOutOfBoroughAddress };
            classUnderTest.HackneyGazetteerOutOfBoroughAddress.Should().Be(hackneyGazetteerOutOfBoroughAddress);
        }
        #endregion
    }
}
