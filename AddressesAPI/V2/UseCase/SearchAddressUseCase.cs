using System;
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

        public SearchAddressUseCase(IAddressesGateway addressesGateway, ISearchAddressValidator requestValidator)
        {
            _addressGateway = addressesGateway;
            _requestValidator = requestValidator;
        }

        public SearchAddressResponse ExecuteAsync(SearchAddressRequest request)
        {
            var validation = _requestValidator.Validate(request);
            if (!validation.IsValid)
            {
                throw new BadRequestException(validation);
            }
            var searchParameters = MapRequestToSearchParameters(request);
            var (results, totalCount) = _addressGateway.SearchAddresses(searchParameters);

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
            var gazetteer = request.Gazetteer.ToLower() == "local"
                ? GlobalConstants.Gazetteer.Hackney
                : Enum.Parse<GlobalConstants.Gazetteer>(request.Gazetteer, true);
            return new SearchParameters
            {
                Format = Enum.Parse<GlobalConstants.Format>(request.Format, true),
                Gazetteer = gazetteer,
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
                OutOfBoroughAddress = request.OutOfBoroughAddress,
                IncludeParentShells = request.IncludeParentShells,
                CrossRefCode = request.CrossRefCode,
                CrossRefValue = request.CrossRefValue
            };
        }
    }
}
