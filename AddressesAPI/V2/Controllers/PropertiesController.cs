using AddressesAPI.V2.Boundary.Requests;
using AddressesAPI.V2.Boundary.Responses;
using AddressesAPI.V2.Boundary.Responses.Metadata;
using AddressesAPI.V2.UseCase.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AddressesAPI.V2.Controllers
{
    [ApiVersion("2")]
    [Produces("application/json")]
    [Route("api/v2/properties")]
    [ProducesResponseType(typeof(APIResponse<object>), 400)]
    [ProducesResponseType(typeof(APIResponse<object>), 500)]
    public class GetPropertiesCrossReferenceController : BaseController
    {
        private readonly IGetPropertiesCrossReferenceUseCase _getPropertiesCrossReferenceUseCase;
        public GetPropertiesCrossReferenceController(IGetPropertiesCrossReferenceUseCase getPropertiesCrossReferenceUseCase)
        {
            _getPropertiesCrossReferenceUseCase = getPropertiesCrossReferenceUseCase;
        }

        /// <summary>
        /// Search Controller V1 to search for addresses
        /// </summary>
        /// <remarks>
        ///
        /// </remarks>
        /// <param name="uprn"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(APIResponse<GetPropertiesCrossReferenceResponse>), 200)]
        [HttpGet]
        [Route("{uprn}/crossreferences")]
        public IActionResult GetPropertiesCrossReference(long uprn)
        {
            var request = new GetPropertiesCrossReferenceRequest { uprn = uprn };
            try
            {
                var response = _getPropertiesCrossReferenceUseCase.ExecuteAsync(request);
                return new OkObjectResult(new APIResponse<GetPropertiesCrossReferenceResponse>(response));
            }
            catch (BadRequestException e)
            {
                return new BadRequestObjectResult(e.ValidationResponse);
            }

        }
    }
}
