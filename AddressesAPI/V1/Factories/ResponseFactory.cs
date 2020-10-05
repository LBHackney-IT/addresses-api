using System.Collections.Generic;
using System.Linq;
using AddressesAPI.V1.Boundary.Responses;
using AddressesAPI.V1.Domain;
using AddressCrossReference = AddressesAPI.V1.Boundary.Responses.Data.AddressCrossReference;
using AddressCrossReferenceDomain = AddressesAPI.V1.Domain.AddressCrossReference;

namespace AddressesAPI.V1.Factories
{
    public static class ResponseFactory
    {
        public static AddressCrossReference ToResponse(this AddressCrossReferenceDomain domain)
        {
            return new AddressCrossReference
            {
                code = domain.code,
                name = domain.name,
                value = domain.value,
                endDate = domain.endDate,
                crossRefKey = domain.crossRefKey,
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
                AddressStatus = domain.AddressStatus,
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
                HackneyGazetteerOutOfBoroughAddress = domain.HackneyGazetteerOutOfBoroughAddress,
                Easting = domain.Easting,
                Northing = domain.Northing,
                Longitude = domain.Longitude,
                Latitude = domain.Latitude,
                addressStartDate = domain.AddressStartDate,
                addressEndDate = domain.AddressEndDate,
                addressChangeDate = domain.AddressChangeDate,
                propertyStartDate = domain.PropertyStartDate,
                propertyEndDate = domain.PropertyEndDate,
                propertyChangeDate = domain.PropertyChangeDate,
            };
        }

        public static List<AddressCrossReference> ToResponse(this IEnumerable<AddressCrossReferenceDomain> domainList)
        {
            return domainList.Select(domain => domain.ToResponse()).ToList();
        }

        public static List<AddressResponse> ToResponse(this IEnumerable<Address> domainList)
        {
            return domainList.Select(domain => domain.ToResponse()).ToList();
        }
    }
}
