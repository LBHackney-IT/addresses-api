using AddressesAPI.V2;
using AddressesAPI.V2.Boundary.Requests;
using FluentAssertions;
using NUnit.Framework;

namespace AddressesAPI.Tests.V2
{
    [TestFixture]
    public class SearchAddressRequestTests
    {
        [Test]
        public void GivenNoInputValueForAddressScope_WhenSearchAddressRequestObjectIsCreated_AddressScopeIsSetToHackneyBorough()
        {
            var classUnderTest = new SearchAddressRequest();
            classUnderTest.AddressScope.Should().Be(GlobalConstants.AddressScope.HackneyBorough.ToString());
        }

        [Test]
        public void GivenNoInputValueForAddressStatus_WhenSearchAddressRequestObjectIsCreated_AddressStatusIsSetToApproved()
        {
            var classUnderTest = new SearchAddressRequest();
            classUnderTest.AddressStatus.Should().Be("approved");
        }
    }
}
