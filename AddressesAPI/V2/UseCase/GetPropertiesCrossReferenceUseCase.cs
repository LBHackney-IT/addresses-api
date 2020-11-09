using AddressesAPI.V2.Boundary.Requests;
using AddressesAPI.V2.Boundary.Responses;
using AddressesAPI.V2.Boundary.Responses.Metadata;
using AddressesAPI.V2.Factories;
using AddressesAPI.V2.Gateways;
using AddressesAPI.V2.UseCase.Interfaces;

namespace AddressesAPI.V2.UseCase
{
    public class GetPropertiesCrossReferenceUseCase : IGetPropertiesCrossReferenceUseCase
    {
        private readonly ICrossReferencesGateway _crossReferenceGateway;
        private readonly IGetCrossReferenceRequestValidator _getAddressCrossReferenceValidator;

        public GetPropertiesCrossReferenceUseCase(ICrossReferencesGateway crossReferencesGateway,
            IGetCrossReferenceRequestValidator getAddressCrossReferenceValidator)
        {
            _crossReferenceGateway = crossReferencesGateway;
            _getAddressCrossReferenceValidator = getAddressCrossReferenceValidator;
        }

        public GetPropertiesCrossReferenceResponse ExecuteAsync(GetPropertiesCrossReferenceRequest request)
        {
            var validationResponse = _getAddressCrossReferenceValidator.Validate(request);
            if (!validationResponse.IsValid)
                throw new BadRequestException(validationResponse);

            var response = _crossReferenceGateway.GetAddressCrossReference(request.uprn);

            var useCaseResponse = new GetPropertiesCrossReferenceResponse
            {
                AddressCrossReferences = response.ToResponse()
            };

            return useCaseResponse;
        }
    }
}
