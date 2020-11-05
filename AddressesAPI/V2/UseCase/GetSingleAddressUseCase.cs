using System.Collections.Generic;
using System.Threading.Tasks;
using AddressesAPI.V1.Boundary.Requests;
using AddressesAPI.V1.Boundary.Responses;
using AddressesAPI.V1.Boundary.Responses.Data;
using AddressesAPI.V1.Boundary.Responses.Metadata;
using AddressesAPI.V1.Factories;
using AddressesAPI.V1.Gateways;
using AddressesAPI.V1.UseCase.Interfaces;

namespace AddressesAPI.V1.UseCase
{
    public class GetSingleAddressUseCase : IGetSingleAddressUseCase
    {
        private readonly IAddressesGateway _addressGateway;
        private IGetAddressRequestValidator _getAddressValidator;

        public GetSingleAddressUseCase(IAddressesGateway addressesGateway, IGetAddressRequestValidator getAddressValidator)
        {
            _addressGateway = addressesGateway;
            _getAddressValidator = getAddressValidator;
        }

        public SearchAddressResponse ExecuteAsync(GetAddressRequest request)
        {
            var validationResponse = _getAddressValidator.Validate(request);
            if (!validationResponse.IsValid)
                throw new BadRequestException(validationResponse);

            var response = _addressGateway.GetSingleAddress(request.addressID);

            if (response == null)
                return new SearchAddressResponse();
            var useCaseResponse = new SearchAddressResponse
            {
                Addresses = new List<AddressResponse> { response.ToResponse() }
            };
            return useCaseResponse;
        }
    }
}
