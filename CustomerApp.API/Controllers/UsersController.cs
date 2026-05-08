using Microsoft.AspNetCore.Mvc;

namespace CustomerApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        /// <summary>
        /// ENDPOINT para autenticação dos usuários.
        /// </summary>
        [HttpPost("auth")]
        public async Task<IActionResult> Auth()
        {
            return Ok();
        }
    }
}