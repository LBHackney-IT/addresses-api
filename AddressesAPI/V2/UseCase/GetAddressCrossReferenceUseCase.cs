using AddressesAPI.V2.Boundary.Requests;
using AddressesAPI.V2.Boundary.Responses;
using AddressesAPI.V2.Boundary.Responses.Metadata;
using AddressesAPI.V2.Factories;
using AddressesAPI.V2.Gateways;
using AddressesAPI.V2.UseCase.Interfaces;

namespace AddressesAPI.V2.UseCase
{
    public class GetAddressCrossReferenceUseCase : IGetAddressCrossReferenceUseCase
    {
        private readonly ICrossReferencesGateway _crossReferenceGateway;
        private readonly IGetCrossReferenceRequestValidator _getAddressCrossReferenceValidator;

        public GetAddressCrossReferenceUseCase(ICrossReferencesGateway crossReferencesGateway,
            IGetCrossReferenceRequestValidator getAddressCrossReferenceValidator)
        {
            _crossReferenceGateway = crossReferencesGateway;
            _getAddressCrossReferenceValidator = getAddressCrossReferenceValidator;
        }

        public GetAddressCrossReferenceResponse ExecuteAsync(GetAddressCrossReferenceRequest request)
        {
            var validationResponse = _getAddressCrossReferenceValidator.Validate(request);
            if (!validationResponse.IsValid)
                throw new BadRequestException(validationResponse);

            var response = _crossReferenceGateway.GetAddressCrossReference(request.uprn);

            var useCaseResponse = new GetAddressCrossReferenceResponse
            {
                AddressCrossReferences = response.ToResponse()
            };

            return useCaseResponse;
        }
    }
}
