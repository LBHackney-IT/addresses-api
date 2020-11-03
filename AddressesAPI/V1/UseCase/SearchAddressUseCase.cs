using AddressesAPI.V1.Boundary.Requests;
using AddressesAPI.V1.Boundary.Responses;
using AddressesAPI.V1.Boundary.Responses.Metadata;
using AddressesAPI.V1.Domain;
using AddressesAPI.V1.Factories;
using AddressesAPI.V1.Gateways;
using AddressesAPI.V1.HelperMethods;
using AddressesAPI.V1.UseCase.Interfaces;

namespace AddressesAPI.V1.UseCase
{
    public class SearchAddressUseCase : ISearchAddressUseCase
    {
        private readonly IAddressesGateway _addressGateway;
        private ISearchAddressValidator _requestValidator;

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
                throw new BadRequestException(new RequestValidationResponse(validation));
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
            return new SearchParameters
            {
                Format = request.Format,
                Gazetteer = request.Gazetteer,
                Page = request.Page == 0 ? 1 : request.Page,
                Postcode = request.PostCode,
                Street = request.Street,
                Uprn = request.UPRN,
                Usrn = request.USRN,
                AddressStatus = request.AddressStatus,
                BuildingNumber = request.BuildingNumber,
                PageSize = request.PageSize,
                UsageCode = request.usageCode,
                UsagePrimary = request.usagePrimary,
                HackneyGazetteerOutOfBoroughAddress = request.HackneyGazetteerOutOfBoroughAddress
            };
        }
    }
}
