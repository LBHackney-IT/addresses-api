using AddressesAPI.V1.Boundary.Response;

namespace AddressesAPI.V1.UseCase.Interfaces
{
    public interface IGetAllUseCase
    {
        ResponseObjectList Execute();
    }
}
