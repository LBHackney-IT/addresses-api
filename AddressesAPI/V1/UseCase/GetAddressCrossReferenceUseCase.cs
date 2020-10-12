using System.Threading.Tasks;
using AddressesAPI.V1.Boundary.Requests;
using AddressesAPI.V1.Boundary.Responses;
using AddressesAPI.V1.Factories;
using AddressesAPI.V1.Gateways;
using AddressesAPI.V1.UseCase.Interfaces;

namespace AddressesAPI.V1.UseCase
{
    public class GetAddressCrossReferenceUseCase : IGetAddressCrossReferenceUseCase
    {
        private readonly ICrossReferencesGateway _crossReferenceGateway;

        public GetAddressCrossReferenceUseCase(ICrossReferencesGateway crossReferencesGateway)
        {
            _crossReferenceGateway = crossReferencesGateway;
        }

        public GetAddressCrossReferenceResponse ExecuteAsync(GetAddressCrossReferenceRequest request)
        {
            if (request == null)
                throw new BadRequestException();

            var validationResponse = request.Validate(request);
            if (!validationResponse.IsValid)
                throw new BadRequestException(validationResponse);

            var response = _crossReferenceGateway.GetAddressCrossReference(request.uprn);

            if (response == null)
                return new GetAddressCrossReferenceResponse();
            var useCaseResponse = new GetAddressCrossReferenceResponse
            {
                AddressCrossReferences = response.ToResponse()
            };

            return useCaseResponse;
        }
    }
}
