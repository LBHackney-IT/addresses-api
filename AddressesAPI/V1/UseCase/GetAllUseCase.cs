using AddressesAPI.V1.Boundary.Response;
using AddressesAPI.V1.Factories;
using AddressesAPI.V1.Gateways;
using AddressesAPI.V1.UseCase.Interfaces;

namespace AddressesAPI.V1.UseCase
{
    //TODO: Rename class name and interface name to reflect the entity they are representing eg. GetAllClaimantsUseCase
    public class GetAllUseCase : IGetAllUseCase
    {
        private readonly IExampleGateway _gateway;
        public GetAllUseCase(IExampleGateway gateway)
        {
            _gateway = gateway;
        }

        public ResponseObjectList Execute()
        {
            return new ResponseObjectList { ResponseObjects = _gateway.GetAll().ToResponse() };
        }
    }
}
