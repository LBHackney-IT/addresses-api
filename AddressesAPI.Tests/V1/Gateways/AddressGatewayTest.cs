using System.Threading.Tasks;
using AddressesAPI.Tests.V1.Helper;
using AddressesAPI.V1.Gateways;
using FluentAssertions;
using NUnit.Framework;

namespace AddressesAPI.Tests.V1.Gateways
{
    public class AddressGatewayTest : TestDatabaseFixture
    {

        private IAddressesGateway _classUnderTest;

        [SetUp]
        public void SetUp()
        {
            _classUnderTest = new AddressesGateway(ConnectionString);
        }

        [Test]
        public async Task can_retrieve_using_address_id()
        {
            var key = "xxxxxxxxxxxxxx";
            //var expectedAddress = Fake.GenerateAddressProvidingKey(key);
            TestDataHelper.InsertAddress(key, Db);

            var response = await _classUnderTest.GetSingleAddressAsync(key).ConfigureAwait(true);

            response.Should().NotBeNull();
            response.AddressKey.Should().BeEquivalentTo(key);

            TestDataHelper.DeleteAddress(key, Db);
        }

        [Test]
        public async Task can_retrieve_crossref_using_uprn()
        {
            var uprn = 1234578912;
            TestDataHelper.InsertCrossRef(uprn, Db);

            var response = await _classUnderTest.GetAddressCrossReferenceAsync(uprn);
            response.Should().NotBeNull();
            response[0].UPRN.Should().Equals(uprn);

            TestDataHelper.DeleteCrossRef(uprn, Db);

        }
    }
}
