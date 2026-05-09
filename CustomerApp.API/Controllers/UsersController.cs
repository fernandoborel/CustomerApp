using CustomerApp.API.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace CustomerApp.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly MongoDbService _mongoDbService;
    private readonly PasswordService _passwordService;
    private readonly JwtService _jwtService;

    public UsersController(MongoDbService mongoDbService, PasswordService passwordService, JwtService jwtService)
    {
        _mongoDbService = mongoDbService;
        _passwordService = passwordService;
        _jwtService = jwtService;
    }

    /// <summary>
    /// ENDPOINT para autenticação dos usuários.
    /// </summary>
    [HttpPost("auth")]
    public async Task<IActionResult> Auth([FromBody] AuthRequest request)
    {
        var existingUser = await _mongoDbService.Users
                            .Find(u => u.Email == request.email)
                            .FirstOrDefaultAsync();

        if (existingUser is null)
        {
            return Unauthorized(new { message = "Credenciais inválidas." });
        }

        var passwordOk = _passwordService.VerifyPasswords(request.password, existingUser.PasswordHash!);

        if (!passwordOk)
        {
            return Unauthorized(new { message = "Acesso negado." });
        }

        var token = _jwtService.GenerateToken(existingUser);

        return Ok(new
        {
            accessToken = token,
            tokenType = "Bearer",
            user = new
            {
                existingUser.Id,
                existingUser.Name,
                existingUser.Email
            }
        });
    }
}

public record AuthRequest(
        string email,
        string password
    );