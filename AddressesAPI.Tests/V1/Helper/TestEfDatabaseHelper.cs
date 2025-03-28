using AddressesAPI.Infrastructure;
using AutoFixture;

namespace AddressesAPI.Tests.V1.Helper
{
    public static class TestEfDataHelper
    {
        public static NationalAddress InsertAddress(AddressesContext context, string key = null, NationalAddress request = null)
        {
            var fixture = new Fixture();
            var randomAddressRecord = fixture.Build<NationalAddress>()
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
            randomAddressRecord.ParentUPRN = request?.ParentUPRN; //we want to be able to pass null ParentUPRN in some cases and override the fixture value
            if (request?.UsagePrimary != null) randomAddressRecord.UsagePrimary = request.UsagePrimary;
            if (request?.UsageCode != null) randomAddressRecord.UsageCode = request.UsageCode;
            if (request?.Gazetteer != null) randomAddressRecord.Gazetteer = request.Gazetteer;
            if (request?.NeverExport != null) randomAddressRecord.NeverExport = request.NeverExport;
            if (request?.Town != null) randomAddressRecord.Town = request.Town;
            if (request?.Street != null) randomAddressRecord.Street = request.Street;
            if (request?.PaonStartNumber != null) randomAddressRecord.PaonStartNumber = request.PaonStartNumber;
            if (request?.BuildingNumber != null) randomAddressRecord.BuildingNumber = ReplaceEmptyStringWithNull(request.BuildingNumber);
            if (request?.UnitNumber != null) randomAddressRecord.UnitNumber = request.UnitNumber;
            if (request?.UnitName != null) randomAddressRecord.UnitName = ReplaceEmptyStringWithNull(request.UnitName);
            if (request?.Line1 != null) randomAddressRecord.Line1 = request.Line1;
            if (request?.Line2 != null) randomAddressRecord.Line2 = request.Line2;
            if (request?.Line3 != null) randomAddressRecord.Line3 = request.Line3;
            if (request?.Line4 != null) randomAddressRecord.Line4 = request.Line4;

            context.NationalAddresses.Add(randomAddressRecord);
            context.SaveChanges();
            return randomAddressRecord;
        }

        public static CrossReference InsertCrossReference(AddressesContext context, long uprn, CrossReference record = null)
        {
            if (record == null)
            {
                var fixture = new Fixture();
                record = fixture.Build<CrossReference>()
                    .With(cr => cr.UPRN, uprn)
                    .Create();
            }

            record.UPRN = uprn;
            context.AddressCrossReferences.Add(record);
            context.SaveChanges();

            return record;
        }

        private static string ReplaceEmptyStringWithNull(string request)
        {
            return request.Length == 0 ? null : request;
        }
    }
}
