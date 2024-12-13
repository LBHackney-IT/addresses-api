using AddressesAPI.Infrastructure;
using AddressesAPI.V1.Domain;
using AddressesAPI.V1.Factories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

            //paging not currently supported when building property hierarchies
            //no point ordering records yet if we need to build a hierarchy and potentially add more records to the results set
            var addresses = request.Structure == GlobalConstants.Structure.Hierarchy ? baseQuery : PageAddresses(OrderAddresses(baseQuery), request.PageSize, request.Page);

            var formattedAddresses = addresses.Select(
                a => request.Format == GlobalConstants.Format.Simple ? a.ToSimpleDomain() : a.ToDomain()
                ).ToList();

            if (request.Structure == GlobalConstants.Structure.Hierarchy)
            {
                (formattedAddresses, totalCount) = BuildHierarchy(formattedAddresses, request, totalCount);
            }

            return (formattedAddresses, totalCount);
        }

        private (List<Address>, int) BuildHierarchy(List<Address> addresses, SearchParameters originalRequest, int totalCount)
        {
            //ensure we are not missing parents from the result set
            //this is typically when parent doesn't have a postcode and is therefore not included in the results set
            var allUprns = addresses.Select(a => a.UPRN);

            var missingParents = addresses
                .Where(a => a.ParentUPRN != null)
                .Select(a => (long)a.ParentUPRN).Distinct()
                .Except(allUprns).ToList();

            //fetch all missing parents
            foreach (var parentUprn in missingParents)
            {
                var missingParentQuery = new SearchParameters()
                {
                    Format = originalRequest.Format,
                    Uprn = parentUprn,
                    Gazetteer = originalRequest.Gazetteer,
                };

                //should only ever have one
                var matchingMissingParents = CompileBaseSearchQuery(missingParentQuery);

                var formattedAddresses = matchingMissingParents.Select(
                    a => missingParentQuery.Format == GlobalConstants.Format.Simple
                    ? a.ToSimpleDomain() : a.ToDomain())
                    .ToList();

                addresses.AddRange(formattedAddresses);

                //increase the total count by matching missing parents count
                totalCount += formattedAddresses.Count;
            }

            //build initial hierarchy based on parents
            var hierarchyWithAllParents = BuildHierarchyForParent(null, addresses);

            //// DISABLED UNTIL WE HAVE OPTIMISED THE TABLES FOR PARENT UPRN QUERIES ////
            //ensure we are not missing children from the results set
            //this is typically when a child address is outside the parent's post code

            //var distinctParents = hierarchyWithAllParents
            //    .Where(x => x.ParentUPRN == null)
            //    .Select(a => a).Distinct();

            //foreach (var parent in distinctParents)
            //{
            //    var originalChildCount = parent.ChildAddresses?.Count;


            //    var getAllChildrenByParentUPRNQuery = new SearchParameters()
            //    {
            //        Format = originalRequest.Format,
            //        Gazetteer = originalRequest.Gazetteer,
            //        ParentUprn = parent.UPRN,
            //    };

            //    var baseQuery = CompileBaseSearchQuery(getAllChildrenByParentUPRNQuery).ToList();

            //    var formattedChildAddress = baseQuery.Select(
            //        a => getAllChildrenByParentUPRNQuery.Format == GlobalConstants.Format.Simple
            //        ? a.ToSimpleDomain() : a.ToDomain())
            //        .ToList();

            //    if (formattedChildAddress.Count > 0)
            //    {
            //        parent.ChildAddresses
            //            .AddRange(formattedChildAddress
            //                .Where(child => !parent.ChildAddresses
            //                    .Any(x => x.UPRN == child.UPRN)));

            //        //increase total count by new child accounts count
            //        totalCount += (int)(parent.ChildAddresses.Count - originalChildCount);
            //    }
            //}

            //order parents again to ensure parents added to the initial result set appear in the correct position
            var orderedByparentsHierarchy = OrderDomainAddresses(hierarchyWithAllParents);

            //order child records to ensure they are in the correct order within the parent
            foreach (var parentAddress in hierarchyWithAllParents)
            {
                if (parentAddress.ChildAddresses != null)
                {
                    parentAddress.ChildAddresses = OrderDomainAddresses(parentAddress.ChildAddresses);
                }
            }

            return (orderedByparentsHierarchy, totalCount);
        }

        private static List<Address> BuildHierarchyForParent(long? parentUprn, List<Address> addresses)
        {
            //This will throw if we have multiple addresses with the same UPRN (should never happen)
            //Not much we can do about it here if it ever happens, so let it bubble up
            var results = addresses.Where(a => a.ParentUPRN == parentUprn).Select(a =>
            {
                var childAddresses = BuildHierarchyForParent(a.UPRN, addresses);
                a.ChildAddresses = childAddresses.Count > 0 ? childAddresses : null;
                return a;
            }).ToList();

            return results;
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

        //infrastructure properties not availabe in domain object removed
        public List<Address> OrderDomainAddresses(List<Address> addresses)
        {
            return addresses
                .OrderBy(a => a.Town)
                .ThenBy(a => a.Postcode == null)
                .ThenBy(a => a.Postcode)
                .ThenBy(a => a.Street)
                .ThenBy(a => a.BuildingNumber == null)
                .ThenBy(a => a.BuildingNumber)
                .ThenBy(a => a.UnitNumber == "0")
                .ThenBy(a => a.UnitNumber)
                .ThenBy(a => a.UnitName == null)
                .ThenBy(a => a.UnitName)
                .ToList();
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
                //// DISABLED UNTIL WE HAVE OPTIMISED THE TABLES FOR PARENT UPRN QUERIES ////
                //.Where(a => request.ParentUprn == null || a.ParentUPRN == request.ParentUprn)
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
