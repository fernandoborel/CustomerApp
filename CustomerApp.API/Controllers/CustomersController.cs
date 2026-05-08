using Microsoft.AspNetCore.Mvc;

namespace CustomerApp.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CustomersController : ControllerBase
{
    /// <summary>
    /// ENDPOINT para cadastro de cliente
    /// </summary>
    [HttpPost("create")]
    public async Task<IActionResult> Create()
    {
        return Ok();
    }

    /// <summary>
    /// ENDPOINT para atualização de cliente
    /// </summary>
    [HttpPut("update/{id}")]
    public async Task<IActionResult> Update(string id)
    {
        return Ok();
    }

    /// <summary>
    /// ENDPOINT para exclusão de cliente
    /// </summary>
    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        return Ok();
    }

    /// <summary>
    /// ENDPOINT para consulta de clientes
    /// </summary>
    [HttpGet("list")]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        return Ok();
    }

    /// <summary>
    /// ENDPOINT para consultar 1 cliente através do ID
    /// </summary>
    [HttpGet("find/{id}")]
    public async Task<IActionResult> Find(string id)
    {
        return Ok();
    }
}