using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AddressesAPI.V1.Domain;
using AddressesAPI.V1.Factories;
using AddressesAPI.V1.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
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
                .Select(a => a.ToDomain())
                .ToList();

            return (addresses, totalCount);
        }

        public (List<SimpleAddress>, int) SearchSimpleAddresses(SearchParameters request)
        {
            var baseQuery = CompileBaseSearchQuery(request);
            var totalCount = baseQuery.Count();

            var addresses = PageAddresses(OrderAddresses(baseQuery), request.PageSize, request.Page)
                .Select(a => (SimpleAddress) a.ToDomain())
                .ToList();

            return (addresses, totalCount);
        }

        private static IQueryable<Infrastructure.Address> PageAddresses(IQueryable<Infrastructure.Address> query,
            int pageSize, int page)
        {
            var pageOffset = pageSize * (page == 0 ? 0 : page - 1);

            return query.Skip(pageOffset)
                .Take(pageSize);
        }

        private static IQueryable<Infrastructure.Address> OrderAddresses(IQueryable<Infrastructure.Address> query)
        {
            return query.OrderBy(a => a.Town)
                .ThenBy(a => a.Postcode == null ? 1 : 0)
                .ThenBy(a => a.Postcode)
                .ThenBy(a => a.Street)
                .ThenBy(a => a.PaonStartNumber == null || a.PaonStartNumber == 0 ? 1 : 0)
                .ThenBy(a => a.PaonStartNumber)
                .ThenBy(a => a.BuildingNumber == null ? 1 : 0)
                .ThenBy(a => a.BuildingNumber)
                .ThenBy(a => a.UnitNumber == null ? 1 : 0)
                .ThenBy(a => a.UnitNumber)
                .ThenBy(a => a.UnitName == null ? 1 : 0)
                .ThenBy(a => a.UnitName);
        }

        private IQueryable<Infrastructure.Address> CompileBaseSearchQuery(SearchParameters request)
        {
            var postcodeSearchTerm = GenerateSearchTerm(request.Postcode);
            var buildingNumberSearchTerm = GenerateSearchTerm(request.BuildingNumber);
            var streetSearchTerm = GenerateSearchTerm(request.Street);
            var addressStatusSearchTerms = request.AddressStatus?.Split(',') ?? new[] { "Approved Preferred" };
            var usageSearchTerms = request.UsagePrimary?.Split(',').Where(u => u != "Parent Shell").ToList();
            var usageCodeSearchTerms = request.UsageCode?.Split(',').ToList();
            var queryBase = _addressesContext.Addresses
                .Where(a => string.IsNullOrWhiteSpace(request.Postcode)
                            || EF.Functions.ILike(a.PostcodeNoSpace, postcodeSearchTerm))
                .Where(a => string.IsNullOrWhiteSpace(request.BuildingNumber)
                            || EF.Functions.ILike(a.BuildingNumber, buildingNumberSearchTerm))
                .Where(a => string.IsNullOrWhiteSpace(request.Street) ||
                            EF.Functions.ILike(a.Street.Replace(" ", ""), streetSearchTerm))
                .Where(a => addressStatusSearchTerms == null || addressStatusSearchTerms.Contains(a.AddressStatus))
                .Where(a => request.Uprn == null || a.UPRN == request.Uprn)
                .Where(a => request.Usrn == null
                            || a.USRN == request.Usrn)
                .Where(a => (usageSearchTerms == null || !usageSearchTerms.Any())
                            || usageSearchTerms.Contains(a.UsagePrimary))
                .WhereAny(usageCodeSearchTerms?.Select(u =>
                        (Expression<Func<Infrastructure.Address, bool>>) (x =>
                            EF.Functions.ILike(x.UsageCode, $"%{u}%")))
                    .ToArray())
                .Where(a => request.Gazetteer == GlobalConstants.Gazetteer.Both ||
                            a.Gazetteer == GlobalConstants.Gazetteer.Local.ToString())
                .Where(a => request.HackneyGazetteerOutOfBoroughAddress == null ||
                            request.HackneyGazetteerOutOfBoroughAddress == a.NeverExport);
            return queryBase;
        }

        private static string GenerateSearchTerm(string request)
        {
            if (request == null) return null;
            return $"%{request.Replace(" ", "")}%";
        }
    }
}
