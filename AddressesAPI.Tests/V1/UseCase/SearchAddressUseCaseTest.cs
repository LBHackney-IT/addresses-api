using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AddressesAPI.Tests.V1.Helper;
using AddressesAPI.V1;
using AddressesAPI.V1.Boundary.Requests;
using AddressesAPI.V1.Domain;
using AddressesAPI.V1.Gateways;
using AddressesAPI.V1.UseCase;
using AddressesAPI.V1.UseCase.Interfaces;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace AddressesAPI.Tests.V1.UseCase
{
    public class SearchAddressUseCaseTest
    {
        private readonly ISearchAddressUseCase _classUnderTest;
        private readonly Mock<IAddressesGateway> _fakeGateway;


        public SearchAddressUseCaseTest()
        {
            _fakeGateway = new Mock<IAddressesGateway>();

            _classUnderTest = new SearchAddressUseCase(_fakeGateway.Object);
        }

        [Test]
        public async Task GivenLocalGazetteer_WhenExecuteAsync_ThenOnlyLocalAddressesShouldBeReturned()
        {
            var addresses = new List<Address>
            {
                new Address
                {
                    AddressKey = "ABCDEFGHIJKLMN", UPRN = 10024389298,USRN = 21320239,ParentUPRN = 10024389282,AddressStatus = "Approved Preferred",UnitName = "FLAT 16",UnitNumber = "",BuildingName = "HAZELNUT COURT",BuildingNumber = "1",Street = "FIRWOOD LANE",Postcode = "RM3 0FS",Locality = "",Gazetteer = "NATIONAL",CommercialOccupier = "",UsageDescription = "Unclassified, Awaiting Classification",UsagePrimary = "Unclassified", UsageCode = "UC",PropertyShell = false,HackneyGazetteerOutOfBoroughAddress = false,Easting = 554189.4500,Northing = 190281.1000,Longitude = 0.2244347,Latitude = 51.590289
                },
                new Address
                {
                    AddressKey = "ABCDEFGHIJKLM2", UPRN = 10024389298,USRN = 21320239,ParentUPRN = 10024389282,AddressStatus = "Approved Preferred",UnitName = "FLAT 16",UnitNumber = "",BuildingName = "HAZELNUT COURT",BuildingNumber = "1",Street = "FIRWOOD LANE",Postcode = "RM3 0FS",Locality = "",Gazetteer = "LOCAL",CommercialOccupier  = "",UsageDescription = "Unclassified, Awaiting Classification",UsagePrimary = "Unclassified", UsageCode = "UC",PropertyShell = false,HackneyGazetteerOutOfBoroughAddress = false,Easting = 554189.4500,Northing = 190281.1000,Longitude = 0.2244347,Latitude = 51.590289
                }
            };

            var postcode = "RM3 0FS";
            var gazetteer = "LOCAL";
            var request = new SearchAddressRequest
            {
                PostCode = postcode,
                Gazetteer = GlobalConstants.Gazetteer.Local
            };
            _fakeGateway.Setup(s => s.SearchAddressesAsync(It.Is<SearchParameters>(i =>
                    i.Postcode.Equals("RM3 0FS") && i.Gazetteer == GlobalConstants.Gazetteer.Local)))
                .ReturnsAsync((addresses, 1));

            var response = await _classUnderTest.ExecuteAsync(request);
            response.Should().NotBeNull();
            response.Addresses.Count().Should().Equals(1);
            response.TotalCount.Should().Equals(1);
            response.Addresses[0].Gazetteer.Should().Equals(gazetteer);
        }

        [Test]
        public async Task GivenValidInput_WhenGatewayRespondsWithNull_ThenResponseShouldBeNull()
        {
            //arrange
            var postcode = "RM3 0FS";

            _fakeGateway.Setup(s => s.SearchAddressesAsync(It.Is<SearchParameters>(i => i.Postcode.Equals("ABCDEFGHIJKLMN"))))
                .ReturnsAsync((null, 0));

            var request = new SearchAddressRequest
            {
                PostCode = postcode
            };
            //act
            var response = await _classUnderTest.ExecuteAsync(request);
            //assert
            response.Addresses.Should().BeNull();
        }

        [Test]
        public async Task GivenValidPostCode_WhenExecuteAsync_ThenMultipleAddressesShouldBeReturned()
        {
            var addresses = new List<Address>
            {
                new Address
                {
                    AddressKey = "ABCDEFGHIJKLMN", UPRN = 10024389298,USRN = 21320239,ParentUPRN = 10024389282,AddressStatus = "Approved Preferred",UnitName = "FLAT 16",UnitNumber = "",BuildingName = "HAZELNUT COURT",BuildingNumber = "1",Street = "FIRWOOD LANE",Postcode = "RM3 0FS",Locality = "",Gazetteer = "NATIONAL",CommercialOccupier = "",UsageDescription = "Unclassified, Awaiting Classification",UsagePrimary = "Unclassified",                UsageCode = "UC",PropertyShell = false,HackneyGazetteerOutOfBoroughAddress = false,Easting = 554189.4500,Northing = 190281.1000,Longitude = 0.2244347,Latitude = 51.590289
                },
                new Address
                {
                    AddressKey = "ABCDEFGHIJKLM2", UPRN = 10024389298,USRN = 21320239,ParentUPRN = 10024389282,AddressStatus = "Approved Preferred",UnitName = "FLAT 16",UnitNumber = "",BuildingName = "HAZELNUT COURT",BuildingNumber = "1",Street = "FIRWOOD LANE",Postcode = "RM3 0FS",Locality = "",Gazetteer = "NATIONAL",CommercialOccupier = "",UsageDescription = "Unclassified, Awaiting Classification",UsagePrimary = "Unclassified",                UsageCode = "UC",PropertyShell = false,HackneyGazetteerOutOfBoroughAddress = false,Easting = 554189.4500,Northing = 190281.1000,Longitude = 0.2244347,Latitude = 51.590289
                }
            };

            var postcode = "RM3 0FS";
            var request = new SearchAddressRequest
            {
                PostCode = postcode
            };
            _fakeGateway.Setup(s =>
                    s.SearchAddressesAsync(It.Is<SearchParameters>(i => i.Postcode.Equals("RM3 0FS"))))
                .ReturnsAsync((addresses, 2));

            var response = await _classUnderTest.ExecuteAsync(request);
            response.Should().NotBeNull();
            response.Addresses.Count().Should().Equals(2);
            response.TotalCount.Should().Equals(2);
        }

        [Test]
        public async Task GivenValidPostCode_WhenExecuteAsync_ThenAddressShouldBeReturned()
        {
            var addresses = new List<Address>();
            var address = new Address
            {
                AddressKey = "ABCDEFGHIJKLMN",
                UPRN = 10024389298,
                USRN = 21320239,
                ParentUPRN = 10024389282,
                AddressStatus = "Approved Preferred",
                UnitName = "FLAT 16",
                UnitNumber = "",
                BuildingName = "HAZELNUT COURT",
                BuildingNumber = "1",
                Street = "FIRWOOD LANE",
                Postcode = "RM3 0FS",
                Locality = "",
                Gazetteer = "NATIONAL",
                CommercialOccupier = "",
                UsageDescription = "Unclassified, Awaiting Classification",
                UsagePrimary = "Unclassified",
                UsageCode = "UC",
                PropertyShell = false,
                HackneyGazetteerOutOfBoroughAddress = false,
                Easting = 554189.4500,
                Northing = 190281.1000,
                Longitude = 0.2244347,
                Latitude = 51.590289

            };
            addresses.Add(address);
            var postcode = "RM3 0FS";
            var request = new SearchAddressRequest
            {
                PostCode = postcode
            };
            _fakeGateway.Setup(s =>
                    s.SearchAddressesAsync(It.Is<SearchParameters>(i => i.Postcode.Equals("RM3 0FS"))))
                .ReturnsAsync((addresses, 1));

            var response = await _classUnderTest.ExecuteAsync(request);

            response.Should().NotBeNull();
            response.Addresses[0].ShouldEqual(address);
        }
    }
}
