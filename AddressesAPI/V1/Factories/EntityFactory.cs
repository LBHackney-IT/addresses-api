using AddressesAPI.Infrastructure;
using AddressesAPI.V1.Domain;
using Address = AddressesAPI.V1.Domain.Address;

namespace AddressesAPI.V1.Factories
{
    public static class EntityFactory
    {
        //TODO: Refactor the next two methods to not be repeated.
        public static Address ToDomain(this AddressesAPI.Infrastructure.Address addressEntity)
        {
            return new Address
            {
                AddressKey = addressEntity.AddressKey,
                USRN = addressEntity.USRN,
                ParentUPRN = addressEntity.ParentUPRN,
                AddressStatus = addressEntity.AddressStatus,
                UnitName = addressEntity.UnitName,
                UnitNumber = addressEntity.UnitNumber,
                BuildingName = addressEntity.BuildingName,
                BuildingNumber = addressEntity.BuildingNumber,
                Street = addressEntity.Street,
                Locality = addressEntity.Locality,
                Gazetteer = addressEntity.Gazetteer,
                CommercialOccupier = addressEntity.Organisation,
                Ward = addressEntity.Ward,
                UsageDescription = addressEntity.UsageDescription,
                UsagePrimary = addressEntity.UsagePrimary,
                UsageCode = addressEntity.UsageCode,
                PlanningUseClass = addressEntity.PlanningUseClass,
                PropertyShell = addressEntity.PropertyShell,
                HackneyGazetteerOutOfBoroughAddress = addressEntity.NeverExport,
                Easting = addressEntity.Easting,
                Northing = addressEntity.Northing,
                Longitude = addressEntity.Longitude,
                Latitude = addressEntity.Latitude,
                AddressStartDate = addressEntity.AddressStartDate,
                AddressEndDate = addressEntity.AddressEndDate,
                AddressChangeDate = addressEntity.AddressChangeDate,
                PropertyStartDate = addressEntity.PropertyStartDate,
                PropertyEndDate = addressEntity.PropertyEndDate,
                PropertyChangeDate = addressEntity.PropertyChangeDate,
                Line1 = addressEntity.Line1,
                Line2 = addressEntity.Line2,
                Line3 = addressEntity.Line3,
                Line4 = addressEntity.Line4,
                Town = addressEntity.Town,
                UPRN = addressEntity.UPRN,
                Postcode = addressEntity.Postcode,
            };
        }

        public static Address ToDomain(this NationalAddress addressEntity)
        {
            return new Address
            {
                AddressKey = addressEntity.AddressKey,
                USRN = addressEntity.USRN,
                ParentUPRN = addressEntity.ParentUPRN,
                AddressStatus = addressEntity.AddressStatus,
                UnitName = addressEntity.UnitName,
                UnitNumber = addressEntity.UnitNumber,
                BuildingName = addressEntity.BuildingName,
                BuildingNumber = addressEntity.BuildingNumber,
                Street = addressEntity.Street,
                Locality = addressEntity.Locality,
                Gazetteer = addressEntity.Gazetteer,
                CommercialOccupier = addressEntity.Organisation,
                Ward = addressEntity.Ward,
                UsageDescription = addressEntity.UsageDescription,
                UsagePrimary = addressEntity.UsagePrimary,
                UsageCode = addressEntity.UsageCode,
                PlanningUseClass = addressEntity.PlanningUseClass,
                PropertyShell = addressEntity.PropertyShell,
                HackneyGazetteerOutOfBoroughAddress = addressEntity.NeverExport,
                Easting = addressEntity.Easting,
                Northing = addressEntity.Northing,
                Longitude = addressEntity.Longitude,
                Latitude = addressEntity.Latitude,
                AddressStartDate = addressEntity.AddressStartDate,
                AddressEndDate = addressEntity.AddressEndDate,
                AddressChangeDate = addressEntity.AddressChangeDate,
                PropertyStartDate = addressEntity.PropertyStartDate,
                PropertyEndDate = addressEntity.PropertyEndDate,
                PropertyChangeDate = addressEntity.PropertyChangeDate,
                Line1 = addressEntity.Line1,
                Line2 = addressEntity.Line2,
                Line3 = addressEntity.Line3,
                Line4 = addressEntity.Line4,
                Town = addressEntity.Town,
                UPRN = addressEntity.UPRN,
                Postcode = addressEntity.Postcode,
            };
        }

        public static Address ToSimpleDomain(this AddressesAPI.Infrastructure.Address addressEntity)
        {
            return new Address
            {
                Line1 = addressEntity.Line1,
                Line2 = addressEntity.Line2,
                Line3 = addressEntity.Line3,
                Line4 = addressEntity.Line4,
                Town = addressEntity.Town,
                UPRN = addressEntity.UPRN,
                Postcode = addressEntity.Postcode,
            };
        }

        public static AddressCrossReference ToDomain(this CrossReference crossReference)
        {
            return new AddressCrossReference
            {
                CrossRefKey = crossReference.CrossRefKey,
                UPRN = crossReference.UPRN,
                Code = crossReference.Code,
                Name = crossReference.Name,
                Value = crossReference.Value,
                EndDate = crossReference?.EndDate
            };
        }
    }
}
