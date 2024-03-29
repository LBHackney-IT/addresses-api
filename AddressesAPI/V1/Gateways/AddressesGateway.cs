using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AddressesAPI.Infrastructure;
using AddressesAPI.V1.Domain;
using AddressesAPI.V1.Factories;
using Microsoft.EntityFrameworkCore;
using Address = AddressesAPI.V1.Domain.Address;

namespace AddressesAPI.V1.Gateways
{
    public class AddressesGateway : IAddressesGateway
    {
        private readonly AddressesContext _addressesContext;
        public AddressesGateway(AddressesContext addressesContext)
        {
            _addressesContext = addressesContext;
        }

        public Address GetSingleAddress(string addressKey)
        {
            var addressRecord = _addressesContext.Addresses.FirstOrDefault(add => add.AddressKey.Equals(addressKey));
            return addressRecord?.ToDomain();
        }

        public (List<Address>, int) SearchAddresses(SearchParameters request)
        {
            var baseQuery = CompileBaseSearchQuery(request);
            var totalCount = baseQuery.Count();

            var addresses = PageAddresses(OrderAddresses(baseQuery), request.PageSize, request.Page)
                .Select(
                    a => request.Format == GlobalConstants.Format.Simple ? a.ToSimpleDomain() : a.ToDomain()
                    )
                .ToList();

            return (addresses, totalCount);
        }

        private static IQueryable<AddressesAPI.Infrastructure.Address> PageAddresses(IQueryable<AddressesAPI.Infrastructure.Address> query,
            int pageSize, int page)
        {
            var pageOffset = pageSize * (page == 0 ? 0 : page - 1);

            return query.Skip(pageOffset)
                .Take(pageSize);
        }

        private static IQueryable<AddressesAPI.Infrastructure.Address> OrderAddresses(IQueryable<AddressesAPI.Infrastructure.Address> query)
        {
            return query.OrderBy(a => a.Town)
                .ThenBy(a => a.Postcode == null)
                .ThenBy(a => a.Postcode)
                .ThenBy(a => a.Street)
                .ThenBy(a => a.PaonStartNumber == null)
                .ThenBy(a => a.PaonStartNumber == 0)
                .ThenBy(a => a.PaonStartNumber)
                .ThenBy(a => a.BuildingNumber == null)
                .ThenBy(a => a.BuildingNumber)
                .ThenBy(a => a.UnitNumber == 0)
                .ThenBy(a => a.UnitNumber)
                .ThenBy(a => a.UnitName == null)
                .ThenBy(a => a.UnitName);
        }

        private IQueryable<AddressesAPI.Infrastructure.Address> CompileBaseSearchQuery(SearchParameters request)
        {
            var postcodeSearchTerm = request.Postcode == null ? null : $"{request.Postcode.Replace(" ", "")}%";
            var buildingNumberSearchTerm = GenerateSearchTerm(request.BuildingNumber);
            var streetSearchTerm = GenerateSearchTerm(request.Street);
            var addressStatusSearchTerms = request.AddressStatus?.Split(',').Select(a => a.ToLower())
                                           ?? new[] { "approved preferred" };
            var usageSearchTerms = request.UsagePrimary?.Split(',').Where(u => u != "Parent Shell").ToList();
            var usageCodeSearchTerms = request.UsageCode?.Split(',').ToList();
            var queryBase = _addressesContext.Addresses
                .Where(a => string.IsNullOrWhiteSpace(request.Postcode)
                            || EF.Functions.ILike(a.Postcode.Replace(" ", ""), postcodeSearchTerm))
                .Where(a => string.IsNullOrWhiteSpace(request.BuildingNumber)
                            || EF.Functions.ILike(a.BuildingNumber, buildingNumberSearchTerm))
                .Where(a => string.IsNullOrWhiteSpace(request.Street) ||
                            EF.Functions.ILike(a.Street.Replace(" ", ""), streetSearchTerm))
                .Where(a => addressStatusSearchTerms == null || addressStatusSearchTerms.Contains(a.AddressStatus.ToLower()))
                .Where(a => request.Uprn == null || a.UPRN == request.Uprn)
                .Where(a => request.Usrn == null
                            || a.USRN == request.Usrn)
                .Where(a => (usageSearchTerms == null || !usageSearchTerms.Any())
                            || usageSearchTerms.Contains(a.UsagePrimary))
                .WhereAny(usageCodeSearchTerms?.Select(u =>
                        (Expression<Func<AddressesAPI.Infrastructure.Address, bool>>)(x =>
                            EF.Functions.ILike(x.UsageCode, $"%{u}%")))
                    .ToArray())
                .Where(a => request.Gazetteer == GlobalConstants.Gazetteer.Both ||
                            a.Gazetteer.ToLower() == GlobalConstants.GazetteerDatabaseValueForLocal.ToLower())
                .Where(a => request.HackneyGazetteerOutOfBoroughAddress == null ||
                            request.HackneyGazetteerOutOfBoroughAddress == a.OutOfBoroughAddress);
            return queryBase;
        }

        private static string GenerateSearchTerm(string request)
        {
            if (request == null) return null;
            return $"%{request.Replace(" ", "")}%";
        }
    }
}
