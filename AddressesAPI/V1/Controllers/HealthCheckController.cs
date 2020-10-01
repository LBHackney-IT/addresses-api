using System.Collections.Generic;
using AddressesAPI.V1.UseCase;
using Microsoft.AspNetCore.Mvc;

namespace AddressesAPI.V1.Controllers
{
    [ApiVersion("1")]
    [Produces("application/json")]
    [Route("api/v1/healthcheck")]
    [ApiController]
    public class HealthCheckController : BaseController
    {
        [HttpGet]
        [Route("ping")]
        [ProducesResponseType(typeof(Dictionary<string, bool>), 200)]
        public IActionResult HealthCheck()
        {
            var result = new Dictionary<string, bool> { { "success", true } };

            return Ok(result);
        }

        [HttpGet]
        [Route("error")]
        public void ThrowError()
        {
            ThrowOpsErrorUsecase.Execute();
        }
    }
}
