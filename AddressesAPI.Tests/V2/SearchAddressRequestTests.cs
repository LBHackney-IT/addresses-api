using AddressesAPI.V2;
using AddressesAPI.V2.Boundary.Requests;
using FluentAssertions;
using NUnit.Framework;

namespace AddressesAPI.Tests.V2
{
    [TestFixture]
    public class SearchAddressRequestTests
    {
        private static string _bothGazetteers;
        private static string _localGazetteer;

        [SetUp]
        public void SetUp()
        {
            _bothGazetteers = GlobalConstants.Gazetteer.Both.ToString();
            _localGazetteer = GlobalConstants.Gazetteer.Hackney.ToString();
        }
        #region Gazetteer
        [Test]
        public void GivenNoInputForGazetteer_WhenSearchAddressRequestObjectIsCreated_ItDefaultsToBoth()
        {
            var classUnderTest = new SearchAddressRequest();
            classUnderTest.Gazetteer.Should().Be(_bothGazetteers);
        }

        [Test]
        public void GivenInputValueLocalForGazetteer_WhenSearchAddressRequestObjectIsCreated_ItIsSetToLocal()
        {
            var classUnderTest = new SearchAddressRequest { Gazetteer = _localGazetteer };
            classUnderTest.Gazetteer.Should().Be(_localGazetteer);
        }

        [Test]
        public void GivenInputValueBothForGazetteer_WhenSearchAddressRequestObjectIsCreated_ItIsSetToBoth()
        {
            var classUnderTest = new SearchAddressRequest { Gazetteer = _bothGazetteers };
            classUnderTest.Gazetteer.Should().Be(_bothGazetteers);
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
            var classUnderTest = new SearchAddressRequest { Gazetteer = _bothGazetteers };
            classUnderTest.HackneyGazetteerOutOfBoroughAddress.Should().BeNull();
        }

        [Test] //Gazetteer = Local, HGOBAddress -> no input
        public void GivenGazetteerValueLocalAndNoInputForHackneyGazetteerOutOfBoroughAddress_WhenSearchAddressRequestObjectIsCreated_HackneyGazetteerOutOfBoroughAddressIsSetToFalse()
        {
            var classUnderTest = new SearchAddressRequest { Gazetteer = _localGazetteer };
            classUnderTest.HackneyGazetteerOutOfBoroughAddress.Should().BeFalse();
        }

        [TestCase("Both", false)]
        [TestCase("Both", true)]
        [TestCase("Local", false)]
        [TestCase("Local", true)]
        public void GivenAHackneyGazetteerOutOfBoroughAddressInputValueAndAnyGazetteerValue_WhenSearchAddressRequestObjectIsCreated_HackneyGazetteerOutOfBoroughAddressIsSetToWhateverItsInputWas(string gazetteer, bool? hackneyGazetteerOutOfBoroughAddress)
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
