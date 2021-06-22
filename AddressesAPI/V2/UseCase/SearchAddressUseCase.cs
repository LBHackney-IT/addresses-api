using System;
using System.Globalization;
using System.Threading.Tasks;
using AddressesAPI.V2.Boundary.Requests;
using AddressesAPI.V2.Boundary.Responses;
using AddressesAPI.V2.Boundary.Responses.Metadata;
using AddressesAPI.V2.Domain;
using AddressesAPI.V2.Factories;
using AddressesAPI.V2.Gateways;
using AddressesAPI.V2.HelperMethods;
using AddressesAPI.V2.UseCase.Interfaces;

namespace AddressesAPI.V2.UseCase
{
    public class SearchAddressUseCase : ISearchAddressUseCase
    {
        private readonly IAddressesGateway _addressGateway;
        private readonly ISearchAddressValidator _requestValidator;
        private readonly ISearchAddressesGateway _searchAddressesGateway;

        public SearchAddressUseCase(IAddressesGateway addressesGateway, ISearchAddressValidator requestValidator,
            ISearchAddressesGateway searchAddressesGateway)
        {
            _addressGateway = addressesGateway;
            _searchAddressesGateway = searchAddressesGateway;
            _requestValidator = requestValidator;
        }

        public async Task<SearchAddressResponse> ExecuteAsync(SearchAddressRequest request)
        {
            var validation = _requestValidator.Validate(request);
            if (!validation.IsValid)
            {
                throw new BadRequestException(validation);
            }
            var searchParameters = MapRequestToSearchParameters(request);

            if (request.CrossRefCode != null && request.CrossRefValue != null)
            {
                searchParameters.CrossReferencedUprns =
                    _addressGateway.GetMatchingCrossReferenceUprns(request.CrossRefCode, request.CrossRefValue);
            }
            var (addressKeys, totalCount) = await _searchAddressesGateway.SearchAddresses(searchParameters);

            var format = Enum.Parse<GlobalConstants.Format>(request.Format, true);
            var results = _addressGateway.GetAddresses(addressKeys, format);

            if (results == null)
                return new SearchAddressResponse();
            var useCaseResponse = new SearchAddressResponse
            {
                Addresses = results.ToResponse(),
                TotalCount = totalCount,
                PageCount = totalCount.CalculatePageCount(request.PageSize)
            };

            return useCaseResponse;
        }

        private static SearchParameters MapRequestToSearchParameters(SearchAddressRequest request)
        {
            var addressScope = Enum.Parse<GlobalConstants.AddressScope>(request.AddressScope.Replace(" ", ""), true);

            return new SearchParameters
            {
                Gazetteer = addressScope != GlobalConstants.AddressScope.National
                    ? GlobalConstants.Gazetteer.Hackney
                    : GlobalConstants.Gazetteer.Both,
                Page = request.Page == 0 ? 1 : request.Page,
                Postcode = request.Postcode,
                Street = request.Street,
                Uprn = request.UPRN,
                Usrn = request.USRN,
                AddressStatus = request.AddressStatus?.Split(','),
                BuildingNumber = request.BuildingNumber,
                PageSize = request.PageSize,
                UsageCode = request.UsageCode,
                UsagePrimary = request.UsagePrimary,
                OutOfBoroughAddress = addressScope != GlobalConstants.AddressScope.HackneyBorough,
                IncludePropertyShells = request.IncludePropertyShells,
                AddressQuery = request.Query,
                ModifiedSince = request.ModifiedSince == null
                    ? (DateTime?) null
                    : DateTime.ParseExact(request.ModifiedSince, "yyyy-MM-dd", CultureInfo.InvariantCulture)
            };
        }
    }
}
