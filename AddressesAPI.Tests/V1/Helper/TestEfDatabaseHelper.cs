using AddressesAPI.V1.Infrastructure;
using AutoFixture;

namespace AddressesAPI.Tests.V1.Helper
{
    public static class TestEfDataHelper
    {
        public static Address InsertAddress(AddressesContext context, string key = null, Address request = null)
        {
            var fixture = new Fixture();
            var randomAddressRecord = fixture.Build<Address>()
                .With(a => a.AddressStatus, request?.AddressStatus ?? "Approved Preferred")
                .Create();
            if (key != null) randomAddressRecord.AddressKey = key;
            if (request?.Postcode != null)
            {
                randomAddressRecord.Postcode = ReplaceEmptyStringWithNull(request.Postcode);
                randomAddressRecord.PostcodeNoSpace = request.Postcode?.Replace(" ", "");
            }
            if (request?.BuildingNumber != null) randomAddressRecord.BuildingNumber = request.BuildingNumber;
            if (request?.Street != null) randomAddressRecord.Street = request.Street;
            if (request?.UPRN != null) randomAddressRecord.UPRN = request.UPRN;
            if (request?.USRN != null) randomAddressRecord.USRN = request.USRN;
            if (request?.UsagePrimary != null) randomAddressRecord.UsagePrimary = request.UsagePrimary;
            if (request?.UsageCode != null) randomAddressRecord.UsageCode = request.UsageCode;
            if (request?.Gazetteer != null) randomAddressRecord.Gazetteer = request.Gazetteer;
            if (request?.NeverExport != null) randomAddressRecord.NeverExport = request.NeverExport;
            if (request?.Town != null) randomAddressRecord.Town = request.Town;
            if (request?.Street != null) randomAddressRecord.Street = request.Street;
            if (request?.PaonStartNumber != null) randomAddressRecord.PaonStartNumber = request.PaonStartNumber;
            if (request?.BuildingNumber != null) randomAddressRecord.BuildingNumber = ReplaceEmptyStringWithNull(request.BuildingNumber);
            if (request?.UnitNumber != null) randomAddressRecord.UnitNumber = ReplaceEmptyStringWithNull(request.UnitNumber);
            if (request?.UnitName != null) randomAddressRecord.UnitName = ReplaceEmptyStringWithNull(request.UnitName);

            context.Addresses.Add(randomAddressRecord);
            context.SaveChanges();
            return randomAddressRecord;
        }

        private static string ReplaceEmptyStringWithNull(string request)
        {
            return request.Length == 0 ? null : request;
        }
    }
}
