using AddressesAPI.V1.Boundary.Requests;
using AddressesAPI.V1.Boundary.Responses;
using AddressesAPI.V1.Boundary.Responses.Metadata;
using AddressesAPI.V1.Factories;
using AddressesAPI.V1.Gateways;
using AddressesAPI.V1.UseCase.Interfaces;

namespace AddressesAPI.V1.UseCase
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
