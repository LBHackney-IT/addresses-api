using System.Threading.Tasks;
using AddressesAPI.Infrastructure;
using AutoFixture;
using Nest;
using static Elasticsearch.Net.Refresh;

namespace AddressesAPI.Tests.V2.Helper
{
    public static class TestDataHelper
    {
        public static async Task<NationalAddress> InsertAddressInDbAndEs(AddressesContext context, ElasticClient elasticClient, string key = null,
            NationalAddress request = null)
        {
            var address = InsertAddressInDb(context, key, request);
            var queryableAddress = MapAddressToQueryableAddress(address);
            await InsertAddressInEs(elasticClient, address.AddressKey, queryableAddress).ConfigureAwait(true);
            return address;
        }

        public static async Task<QueryableAddress> InsertAddressInEs(ElasticClient elasticClient, string key = null, QueryableAddress addressConfig = null)
        {
            var randomQueryableAddress = CreateConfigurableQueryableAddressRecord(key, addressConfig);
            var response = await elasticClient.IndexAsync(randomQueryableAddress, i =>
                    i.Refresh(WaitFor).Index("hackney_addresses"))
                .ConfigureAwait(true);
            return randomQueryableAddress;
        }

        public static NationalAddress InsertAddressInDb(AddressesContext context, string key = null, NationalAddress request = null)
        {
            var randomAddressRecord = CreateConfigurableAddressRecord(key, request);
            context.NationalAddresses.Add(randomAddressRecord);
            context.SaveChanges();
            return randomAddressRecord;
        }

        private static QueryableAddress MapAddressToQueryableAddress(NationalAddress address)
        {
            return new QueryableAddress
            {
                Gazetteer = address.Gazetteer,
                Line1 = address.Line1,
                Line2 = address.Line2,
                Line3 = address.Line3,
                Line4 = address.Line4,
                Postcode = address.Postcode,
                Street = address.Street,
                Town = address.Town,
                AddressKey = address.AddressKey,
                AddressStatus = address.AddressStatus,
                BuildingNumber = address.BuildingNumber,
                PropertyShell = address.PropertyShell,
                UsageCode = address.UsageCode,
                UsagePrimary = address.UsagePrimary,
                AddressChangeDate = address.AddressChangeDate,
                OutOfBoroughAddress = address.NeverExport,
                UPRN = address.UPRN,
                USRN = address.USRN,
                ParentUPRN = address.ParentUPRN
            };
        }

        public static CrossReference InsertCrossReference(AddressesContext context, long uprn, CrossReference record = null)
        {
            var fixture = new Fixture();
            var randomCrossReference = fixture.Build<CrossReference>()
                .With(cr => cr.UPRN, uprn)
                .Create();

            if (record?.UPRN != null && record.UPRN != 0) randomCrossReference.UPRN = record.UPRN;
            if (record?.CrossRefKey != null) randomCrossReference.CrossRefKey = record.CrossRefKey;
            if (record?.Code != null) randomCrossReference.Code = record.Code;
            if (record?.Name != null) randomCrossReference.Name = record.Name;
            if (record?.Value != null) randomCrossReference.Value = record.Value;
            if (record?.EndDate != null) randomCrossReference.EndDate = record.EndDate;

            context.AddressCrossReferences.Add(randomCrossReference);
            context.SaveChanges();
            return randomCrossReference;
        }
        private static QueryableAddress CreateConfigurableQueryableAddressRecord(string key, QueryableAddress request)
        {
            var fixture = new Fixture();
            var randomAddressRecord = fixture.Build<QueryableAddress>()
                .With(a => a.AddressStatus, request?.AddressStatus ?? "Approved")
                .Create();
            if (key != null) randomAddressRecord.AddressKey = key;
            if (request?.Postcode != null) randomAddressRecord.Postcode = ReplaceEmptyStringWithNull(request.Postcode);
            if (request?.BuildingNumber != null) randomAddressRecord.BuildingNumber = request.BuildingNumber;
            if (request?.Street != null) randomAddressRecord.Street = request.Street;
            if (request?.UPRN != null && request.UPRN != 0) randomAddressRecord.UPRN = request.UPRN;
            if (request?.USRN != null && request.USRN != 0) randomAddressRecord.USRN = request.USRN;
            if (request?.UsagePrimary != null) randomAddressRecord.UsagePrimary = request.UsagePrimary;
            if (request?.UsageCode != null) randomAddressRecord.UsageCode = request.UsageCode;
            if (request?.Gazetteer != null) randomAddressRecord.Gazetteer = request.Gazetteer;
            if (request?.OutOfBoroughAddress != null) randomAddressRecord.OutOfBoroughAddress = request.OutOfBoroughAddress;
            if (request?.Town != null) randomAddressRecord.Town = request.Town;
            if (request?.Street != null) randomAddressRecord.Street = request.Street;
            if (request?.PaonStartNumber != null) randomAddressRecord.PaonStartNumber = request.PaonStartNumber;
            if (request?.PaonStartNumber == 0) randomAddressRecord.PaonStartNumber = null;
            if (request?.BuildingNumber != null)
                randomAddressRecord.BuildingNumber = ReplaceEmptyStringWithNull(request.BuildingNumber);
            if (request?.UnitNumber != null) randomAddressRecord.UnitNumber = ReplaceEmptyStringWithNull(request.UnitNumber);
            if (request?.UnitName != null) randomAddressRecord.UnitName = ReplaceEmptyStringWithNull(request.UnitName);
            if (request?.Line1 != null) randomAddressRecord.Line1 = request.Line1;
            if (request?.Line2 != null) randomAddressRecord.Line2 = request.Line2;
            if (request?.Line3 != null) randomAddressRecord.Line3 = request.Line3;
            if (request?.Line4 != null) randomAddressRecord.Line4 = request.Line4;
            randomAddressRecord.ParentUPRN = (request?.ParentUPRN).GetValueOrDefault() == 0 ? null : request.ParentUPRN;
            randomAddressRecord.PropertyShell = request?.PropertyShell ?? false;
            return randomAddressRecord;
        }
        private static NationalAddress CreateConfigurableAddressRecord(string key, NationalAddress request)
        {
            var fixture = new Fixture();
            var randomAddressRecord = fixture.Build<NationalAddress>()
                .With(a => a.AddressStatus, request?.AddressStatus ?? "Approved")
                .Create();
            if (key != null) randomAddressRecord.AddressKey = key;
            if (request?.Postcode != null)
            {
                randomAddressRecord.Postcode = ReplaceEmptyStringWithNull(request.Postcode);
                randomAddressRecord.PostcodeNoSpace = request.Postcode?.Replace(" ", "");
            }

            if (request?.BuildingNumber != null) randomAddressRecord.BuildingNumber = request.BuildingNumber;
            if (request?.Street != null) randomAddressRecord.Street = request.Street;
            if (request?.UPRN != null && request.UPRN != 0) randomAddressRecord.UPRN = request.UPRN;
            if (request?.USRN != null && request.USRN != 0) randomAddressRecord.USRN = request.USRN;
            if (request?.UsagePrimary != null) randomAddressRecord.UsagePrimary = request.UsagePrimary;
            if (request?.UsageCode != null) randomAddressRecord.UsageCode = request.UsageCode;
            if (request?.Gazetteer != null) randomAddressRecord.Gazetteer = request.Gazetteer;
            if (request?.NeverExport != null) randomAddressRecord.NeverExport = request.NeverExport;
            if (request?.Town != null) randomAddressRecord.Town = request.Town;
            if (request?.Street != null) randomAddressRecord.Street = request.Street;
            if (request?.PaonStartNumber != null) randomAddressRecord.PaonStartNumber = request.PaonStartNumber;
            if (request?.BuildingNumber != null)
                randomAddressRecord.BuildingNumber = ReplaceEmptyStringWithNull(request.BuildingNumber);
            if (request?.UnitNumber != null) randomAddressRecord.UnitNumber = ReplaceEmptyStringWithNull(request.UnitNumber);
            if (request?.UnitName != null) randomAddressRecord.UnitName = ReplaceEmptyStringWithNull(request.UnitName);
            if (request?.Line1 != null) randomAddressRecord.Line1 = request.Line1;
            if (request?.Line2 != null) randomAddressRecord.Line2 = request.Line2;
            if (request?.Line3 != null) randomAddressRecord.Line3 = request.Line3;
            if (request?.Line4 != null) randomAddressRecord.Line4 = request.Line4;
            randomAddressRecord.ParentUPRN = (request?.ParentUPRN).GetValueOrDefault() == 0 ? null : request.ParentUPRN;
            randomAddressRecord.PropertyShell = request?.PropertyShell ?? false;
            return randomAddressRecord;
        }

        private static string ReplaceEmptyStringWithNull(string request)
        {
            return request.Length == 0 ? null : request;
        }
    }
}
