using System.Threading.Tasks;
using AddressesAPI.V1.Boundary.Requests;
using AddressesAPI.V1.Boundary.Responses;
using AddressesAPI.V1.Boundary.Responses.Metadata;
using AddressesAPI.V1.Domain;
using AddressesAPI.V1.UseCase.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AddressesAPI.V1.Controllers
{
    [ApiVersion("1")]
    [Produces("application/json")]
    [Route("api/v1/properties")]
    [ProducesResponseType(typeof(APIResponse<object>), 400)]
    [ProducesResponseType(typeof(APIResponse<object>), 500)]
    public class GetAddressCrossReferenceController : BaseController
    {
        private readonly IGetAddressCrossReferenceUseCase _getAddressCrossReferenceUseCase;
        public GetAddressCrossReferenceController(IGetAddressCrossReferenceUseCase getAddressCrossReferenceUseCase)
        {
            _getAddressCrossReferenceUseCase = getAddressCrossReferenceUseCase;
        }


        /// <summary>
        /// Search Controller V1 to search for addresses
        /// </summary>
        /// <remarks>
        ///
        /// </remarks>
        /// <param name="uprn"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(APIResponse<GetAddressCrossReferenceResponse>), 200)]
        [HttpGet]
        [Route("{uprn}/crossreferences")]
        public IActionResult GetAddressCrossReference(long uprn)
        {
            var request = new GetAddressCrossReferenceRequest { uprn = uprn };
            var response = _getAddressCrossReferenceUseCase.ExecuteAsync(request);
            //We convert the result to an APIResponse via extensions on BaseController
            return HandleResponse(response);
        }
    }
}
