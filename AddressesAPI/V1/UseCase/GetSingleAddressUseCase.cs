using System.Collections.Generic;
using System.Threading.Tasks;
using AddressesAPI.V1.Boundary.Requests;
using AddressesAPI.V1.Boundary.Responses;
using AddressesAPI.V1.Factories;
using AddressesAPI.V1.Gateways;
using AddressesAPI.V1.UseCase.Interfaces;

namespace AddressesAPI.V1.UseCase
{
    public class GetSingleAddressUseCase : IGetSingleAddressUseCase
    {
        private readonly IAddressesGatewayTSQL _addressGatewayTsql;

        public GetSingleAddressUseCase(IAddressesGatewayTSQL addressesGatewayTsql)
        {
            _addressGatewayTsql = addressesGatewayTsql;
        }

        public async Task<SearchAddressResponse> ExecuteAsync(GetAddressRequest request)
        {
            //validate
            if (request == null)
                throw new BadRequestException();


            var validationResponse = request.Validate(request);
            if (!validationResponse.IsValid)
                throw new BadRequestException(validationResponse);

            var response = await _addressGatewayTsql.GetSingleAddressAsync(request.addressID).ConfigureAwait(false);

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
