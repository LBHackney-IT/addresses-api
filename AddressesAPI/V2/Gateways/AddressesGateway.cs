using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AddressesAPI.Infrastructure;
using AddressesAPI.V2.Domain;
using AddressesAPI.V2.Factories;
using Microsoft.EntityFrameworkCore;
using Address = AddressesAPI.Infrastructure.Address;

namespace AddressesAPI.V2.Gateways
{
    public class AddressesGateway : IAddressesGateway
    {
        private readonly AddressesContext _addressesContext;
        public AddressesGateway(AddressesContext addressesContext)
        {
            _addressesContext = addressesContext;
        }

        public V2.Domain.Address GetSingleAddress(string addressKey)
        {
            var addressRecord = _addressesContext.Addresses.FirstOrDefault(add => add.AddressKey.Equals(addressKey));
            return addressRecord?.ToDomain();
        }

        public List<Domain.Address> GetAddresses(List<string> addressKeys, GlobalConstants.Format format)
        {
            var addresses = _addressesContext.Addresses
                .Where(a => addressKeys.Contains(a.AddressKey));

            return addressKeys
                .Select(a => addresses.FirstOrDefault(ad => ad.AddressKey == a))
                .Where(a => a != null)
                .Select(a => format == GlobalConstants.Format.Simple ? a.ToSimpleDomain() : a.ToDomain())
                .ToList();
        }

        private IQueryable<Address> GetParentShells(IQueryable<Address> baseQuery, IQueryable<Address> baseAddresses)
        {
            var childShells = baseQuery;
            while (true)
            {
                var parentUprns = GetImmediateParentUprns(childShells);
                if (!parentUprns.Any()) break;
                var parentShells = GetImmediateParentShells(parentUprns, ref baseAddresses);
                childShells = parentShells;
            }

            return baseAddresses;
        }

        private static IQueryable<long?> GetImmediateParentUprns(IQueryable<Address> childShells)
        {
            return childShells.Select(a => a.ParentUPRN)
                .Where(pu => pu != null && pu != 0)
                .Distinct();
        }

        private IQueryable<Address> GetImmediateParentShells(IQueryable<long?> parentUprns, ref IQueryable<Address> baseAddresses)
        {
            var parentShells = _addressesContext.Addresses.Where(ps => parentUprns.Contains(ps.UPRN));
            baseAddresses = baseAddresses.Union(parentShells);
            return parentShells;
        }

        private static IQueryable<Address> PageAddresses(IQueryable<Address> query,
            int pageSize, int page)
        {
            var pageOffset = pageSize * (page == 0 ? 0 : page - 1);

            return query.Skip(pageOffset)
                .Take(pageSize);
        }

        private static IQueryable<Address> OrderAddresses(IQueryable<Address> query)
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
                .ThenBy(a => a.UnitNumber == null)
                .ThenBy(a => a.UnitNumber)
                .ThenBy(a => a.UnitName == null)
                .ThenBy(a => a.UnitName);
        }

        public List<long> GetMatchingCrossReferenceUprns(string code, string value)
        {
            return _addressesContext.AddressCrossReferences
                .Where(cr => cr.Code == code)
                .Where(cr => cr.Value == value)
                .Select(cr => cr.UPRN)
                .ToList();
        }


        private IQueryable<Address> CompileBaseSearchQuery(SearchParameters request)
        {
            var queryByCrossReference = string.IsNullOrWhiteSpace(request.CrossRefCode) && string.IsNullOrWhiteSpace(request.CrossRefValue);

            var addresses = queryByCrossReference
                ? _addressesContext.Addresses
                : _addressesContext.Addresses.Where(a => GetMatchingCrossReferenceUprns(request.CrossRefCode, request.CrossRefValue).Contains(a.UPRN));

            var postcodeSearchTerm = request.Postcode == null ? null : $"{request.Postcode.Replace(" ", "")}%";
            var buildingNumberSearchTerm = GenerateSearchTerm(request.BuildingNumber);
            var streetSearchTerm = GenerateSearchTerm(request.Street);
            var addressStatusQuery = (request.AddressStatus?.Select(a => a.ToLower()) ?? new[] { "approved" }).ToList();
            var addressStatusSearchTerms = addressStatusQuery.Contains("approved")
                ? addressStatusQuery.Append("approved preferred")
                : addressStatusQuery;
            var usageSearchTerms = request.UsagePrimary?.Split(',').ToList();
            var usageCodeSearchTerms = request.UsageCode?.Split(',').ToList();

            var queryResults = addresses
                .Where(a => string.IsNullOrWhiteSpace(request.Postcode)
                            || EF.Functions.ILike(a.Postcode.Replace(" ", ""), postcodeSearchTerm))
                .Where(a => string.IsNullOrWhiteSpace(request.BuildingNumber)
                            || EF.Functions.ILike(a.BuildingNumber, buildingNumberSearchTerm))
                .Where(a => string.IsNullOrWhiteSpace(request.Street) ||
                            EF.Functions.ILike(a.Street.Replace(" ", ""), streetSearchTerm))
                .Where(a => addressStatusSearchTerms == null ||
                            addressStatusSearchTerms.Contains(a.AddressStatus.ToLower()))
                .Where(a => request.Uprn == null || a.UPRN == request.Uprn)
                .Where(a => request.Usrn == null
                            || a.USRN == request.Usrn)
                .Where(a => (usageSearchTerms == null || !usageSearchTerms.Any())
                            || usageSearchTerms.Contains(a.UsagePrimary))
                .WhereAny(usageCodeSearchTerms?.Select(u =>
                        (Expression<Func<Address, bool>>) (x =>
                            EF.Functions.ILike(x.UsageCode, $"%{u}%")))
                    .ToArray())
                .Where(a => request.IncludeParentShells || !a.PropertyShell)
                .Where(a => request.Gazetteer == GlobalConstants.Gazetteer.Both
                            || EF.Functions.ILike(a.Gazetteer, request.Gazetteer.ToString())
                            || request.Gazetteer == GlobalConstants.Gazetteer.Hackney
                            && EF.Functions.ILike(a.Gazetteer, "local")
                )
                .Where(a => request.OutOfBoroughAddress
                            || !(EF.Functions.ILike(a.Gazetteer, "national") || a.OutOfBoroughAddress)
                );
            return queryResults;
        }

        private static string GenerateSearchTerm(string request)
        {
            if (request == null) return null;
            return $"%{request.Replace(" ", "")}%";
        }
    }
}
