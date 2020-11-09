using System.Collections.Generic;
using AddressesAPI.V2.Boundary.Requests;
using AddressesAPI.V2.Boundary.Responses;
using AddressesAPI.V2.Boundary.Responses.Metadata;
using AddressesAPI.V2.Factories;
using AddressesAPI.V2.Gateways;
using AddressesAPI.V2.UseCase.Interfaces;

namespace AddressesAPI.V2.UseCase
{
    public class GetSingleAddressUseCase : IGetSingleAddressUseCase
    {
        private readonly IAddressesGateway _addressGateway;
        private readonly IGetAddressRequestValidator _getAddressValidator;

        public GetSingleAddressUseCase(IAddressesGateway addressesGateway, IGetAddressRequestValidator getAddressValidator)
        {
            _addressGateway = addressesGateway;
            _getAddressValidator = getAddressValidator;
        }

        public GetAddressResponse ExecuteAsync(GetAddressRequest request)
        {
            var validationResponse = _getAddressValidator.Validate(request);
            if (!validationResponse.IsValid)
                throw new BadRequestException(validationResponse);

            var response = _addressGateway.GetSingleAddress(request.addressID);

            if (response == null)
                return new GetAddressResponse();
            var useCaseResponse = new GetAddressResponse
            {
                Address = response.ToResponse()
            };
            return useCaseResponse;
        }
    }
}
