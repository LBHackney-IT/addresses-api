using AddressesAPI.V1.Boundary.Response;

namespace AddressesAPI.V1.UseCase.Interfaces
{
    public interface IGetByIdUseCase
    {
        ResponseObject Execute(int id);
    }
}
