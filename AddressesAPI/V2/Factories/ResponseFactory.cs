using System;
using System.Collections.Generic;
using System.Linq;
using AddressesAPI.V2.Boundary.Responses.Data;
using AddressesAPI.V2.Domain;
using AddressCrossReferenceDomain = AddressesAPI.V2.Domain.AddressCrossReference;
namespace AddressesAPI.V2.Factories
{
    public static class ResponseFactory
    {
        public static AddressCrossReferenceResponse ToResponse(this AddressCrossReferenceDomain domain)
        {
            return new AddressCrossReferenceResponse
            {
                Code = domain.Code,
                Name = domain.Name,
                Value = domain.Value,
                EndDate = domain.EndDate,
                CrossRefKey = domain.CrossRefKey,
                UPRN = domain.UPRN
            };
        }
        public static AddressResponse ToResponse(this Address domain)
        {
            return new AddressResponse
            {
                Line1 = domain.Line1,
                Line2 = domain.Line2,
                Line3 = domain.Line3,
                Line4 = domain.Line4,
                Town = domain.Town,
                Postcode = domain.Postcode,
                UPRN = domain.UPRN,
                AddressKey = domain.AddressKey,
                USRN = domain.USRN,
                ParentUPRN = domain.ParentUPRN,
                AddressStatus = MapAddressStatus(domain.AddressStatus),
                UnitName = domain.UnitName,
                UnitNumber = domain.UnitNumber,
                BuildingName = domain.BuildingName,
                BuildingNumber = domain.BuildingNumber,
                Street = domain.Street,
                Locality = domain.Locality,
                Gazetteer = domain.Gazetteer,
                CommercialOccupier = domain.CommercialOccupier,
                Ward = domain.Ward,
                UsageDescription = domain.UsageDescription,
                UsagePrimary = domain.UsagePrimary,
                UsageCode = domain.UsageCode,
                PlanningUseClass = domain.PlanningUseClass,
                PropertyShell = domain.PropertyShell,
                OutOfBoroughAddress = domain.OutOfBoroughAddress,
                Easting = domain.Easting,
                Northing = domain.Northing,
                Longitude = domain.Longitude,
                Latitude = domain.Latitude,
                AddressStartDate = domain.AddressStartDate,
                AddressEndDate = domain.AddressEndDate,
                AddressChangeDate = domain.AddressChangeDate,
                PropertyStartDate = domain.PropertyStartDate,
                PropertyEndDate = domain.PropertyEndDate,
                PropertyChangeDate = domain.PropertyChangeDate,
                PaoStartNum = domain.PaoStartNum
            };
        }

        private static string MapAddressStatus(string domainStatus)
        {
            if (domainStatus == null) return null;
            return domainStatus.Equals("Approved Preferred", StringComparison.InvariantCultureIgnoreCase)
                ? "Approved"
                : domainStatus;
        }

        public static List<AddressCrossReferenceResponse> ToResponse(this IEnumerable<AddressCrossReferenceDomain> domainList)
        {
            return domainList.Select(domain => domain.ToResponse()).ToList();
        }

        public static List<AddressResponse> ToResponse(this IEnumerable<Address> domainList)
        {
            return domainList.Select(domain => domain.ToResponse()).ToList();
        }
    }
}
