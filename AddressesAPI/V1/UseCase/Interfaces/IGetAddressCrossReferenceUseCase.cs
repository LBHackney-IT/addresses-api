using System.Threading.Tasks;
using AddressesAPI.V1.Boundary.Requests;
using AddressesAPI.V1.Boundary.Responses;

namespace AddressesAPI.V1.UseCase.Interfaces
{
    public interface IGetAddressCrossReferenceUseCase
    {
        Task<GetAddressCrossReferenceResponse> ExecuteAsync(GetAddressCrossReferenceRequest request);
    }
}
