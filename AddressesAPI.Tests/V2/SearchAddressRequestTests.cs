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

        #region OutOfBoroughAddress
        [Test]
        public void GivenNoInputValueForOutOfBoroughAddressParameter_WhenSearchAddressRequestObjectIsCreated_OutOfBoroughAddressIsSetToTrue()
        {
            var classUnderTest = new SearchAddressRequest();
            classUnderTest.OutOfBoroughAddress.Should().BeTrue();
        }

        [TestCase("Both", false)]
        [TestCase("Both", true)]
        [TestCase("Local", false)]
        [TestCase("Local", true)]
        public void GivenAHackneyGazetteerOutOfBoroughAddressInputValueAndAnyGazetteerValue_WhenSearchAddressRequestObjectIsCreated_HackneyGazetteerOutOfBoroughAddressIsSetToWhateverItsInputWas(string gazetteer, bool hackneyGazetteerOutOfBoroughAddress)
        {
            var classUnderTest = new SearchAddressRequest { Gazetteer = gazetteer, OutOfBoroughAddress = hackneyGazetteerOutOfBoroughAddress };
            classUnderTest.OutOfBoroughAddress.Should().Be(hackneyGazetteerOutOfBoroughAddress);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GivenNoInputForGazetteerAndInputValueForHackneyGazetteerOutOfBoroughAddress_WhenSearchAddressRequestObjectIsCreated_HackneyGazetteerOutOfBoroughAddressIsSetToWhateverItsInputWas(bool hackneyGazetteerOutOfBoroughAddress)
        {
            var classUnderTest = new SearchAddressRequest { OutOfBoroughAddress = hackneyGazetteerOutOfBoroughAddress };
            classUnderTest.OutOfBoroughAddress.Should().Be(hackneyGazetteerOutOfBoroughAddress);
        }
        #endregion
    }
}
