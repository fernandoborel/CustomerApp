using CustomerApp.API.Models;
using CustomerApp.API.Services;
using MongoDB.Driver;

namespace CustomerApp.API.Runners;

/// <summary>
/// Classe para executar operações no banco de dados no momento em que o projeto for executado.
/// </summary>
public class DatabaseRunner
{
    private readonly MongoDbService _mongoDbService;
    private readonly PasswordService _passwordService;

    public DatabaseRunner(MongoDbService mongoDbService, PasswordService passwordService)
    {
        _mongoDbService = mongoDbService;
        _passwordService = passwordService;
    }

    public async Task SendAsync()
    {
        var email = "administrador@cotiinformatica.com.br";
        var senha = "Coti@2026";

        //Verificando se o usuário já está cadastrado no mongodb
        var existingUser = await _mongoDbService.Users
                            .Find(u => u.Email == email)
                            .FirstOrDefaultAsync();

        if (existingUser is null)
        {
            //Cadastrando o usuário
            var user = new User
            {
                Name = "Administrador COTI",
                Email = email,
                PasswordHash = _passwordService.HashPassword(senha)
            };

            await _mongoDbService.Users.InsertOneAsync(user);
        }
    }
}