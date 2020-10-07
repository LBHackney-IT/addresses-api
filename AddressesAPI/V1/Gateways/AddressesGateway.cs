using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AddressesAPI.V1.Domain;
using AddressesAPI.V1.Factories;
using AddressesAPI.V1.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Address = AddressesAPI.V1.Domain.Address;

#pragma warning disable 1998
// CS1998 Requires await operators with async functions.
// Remove pragma disable after setting up gateway methods to full functionality

namespace AddressesAPI.V1.Gateways
{
    public class AddressesGateway : IAddressesGateway
    {
        private readonly AddressesContext _addressesContext;
        public AddressesGateway(AddressesContext addressesContext)
        {
            _addressesContext = addressesContext;
        }

        public async Task<Address> GetSingleAddressAsync(string addressId)
        {
            return new Address();
        }

        public (List<Address>, int) SearchAddresses(SearchParameters request)
        {
            var postcodeSearchTerm = GenerateSearchTerm(request.Postcode);
            var buildingNumberSearchTerm = GenerateSearchTerm(request.BuildingNumber);
            var streetSearchTerm = GenerateSearchTerm(request.Street);
            var addressStatusSearchTerms = request.AddressStatus?.Split(',') ?? new[] { "Approved Preferred" };
            var usageSearchTerms = request.UsagePrimary?.Split(',').Where(u => u != "Parent Shell").ToList();
            var usageCodeSearchTerms = request.UsageCode?.Split(',').ToList();
            var pageOffset = request.PageSize * (request.Page == 0 ? 0 : request.Page - 1);

            var addresses = _addressesContext.Addresses
                .Where(a => string.IsNullOrWhiteSpace(request.Postcode)
                            || EF.Functions.ILike(a.PostcodeNoSpace, postcodeSearchTerm))
                .Where(a => string.IsNullOrWhiteSpace(request.BuildingNumber)
                            || EF.Functions.ILike(a.BuildingNumber, buildingNumberSearchTerm))
                .Where(a => string.IsNullOrWhiteSpace(request.Street)
                            || EF.Functions.ILike(a.Street.Replace(" ", ""), streetSearchTerm))
                .Where(a => addressStatusSearchTerms == null || addressStatusSearchTerms.Contains(a.AddressStatus))
                .Where(a => request.Uprn == null || a.UPRN == request.Uprn)
                .Where(a => request.Usrn == null || a.USRN == request.Usrn)
                .Where(a => (usageSearchTerms == null || !usageSearchTerms.Any())
                            || usageSearchTerms.Contains(a.UsagePrimary))
                .WhereAny(usageCodeSearchTerms?.Select(u =>
                    (Expression<Func<V1.Infrastructure.Address, bool>>) (x => EF.Functions.ILike(x.UsageCode, $"%{u}%")))
                    .ToArray())
                .Where(a => request.Gazetteer == GlobalConstants.Gazetteer.Both || a.Gazetteer == GlobalConstants.Gazetteer.Local.ToString())
                .Where(a => request.HackneyGazetteerOutOfBoroughAddress == null || request.HackneyGazetteerOutOfBoroughAddress == a.NeverExport)
                .OrderBy(a => a.Town)
                .ThenBy(a => a.Postcode == null ? 1 : 0)
                .ThenBy(a => a.Street)
                .ThenBy(a => a.PaonStartNumber == null || a.PaonStartNumber == 0 ? 1 : 0)
                .ThenBy(a => a.PaonStartNumber)
                .ThenBy(a => a.BuildingNumber == null ? 1 : 0)
                .ThenBy(a => a.BuildingNumber)
                .ThenBy(a => a.UnitNumber == null ? 1 : 0)
                .ThenBy(a => a.UnitNumber)
                .ThenBy(a => a.UnitName == null ? 1 : 0)
                .ThenBy(a => a.UnitName)
                .Skip(pageOffset)
                .Take(request.PageSize)
                .Select(a => a.ToDomain())
                .ToList();
            return (addresses, 0);
        }

        public async Task<(List<SimpleAddress>, int)> SearchSimpleAddressesAsync(SearchParameters request)
        {
            return (new List<SimpleAddress>(), 0);
        }

        private static string GenerateSearchTerm(string request)
        {
            if (request == null) return null;
            return $"%{request.Replace(" ", "")}%";
        }
    }
}
