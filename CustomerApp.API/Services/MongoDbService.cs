using CustomerApp.API.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace CustomerApp.API.Services;

/// <summary>
/// Classe para acesso ao banco de dados do MongoDB
/// </summary>
public class MongoDbService
{
    private readonly IMongoDatabase _mongoDatabase;

    public MongoDbService(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _mongoDatabase = client.GetDatabase(settings.Value.DatabaseName);
    }

    #region Mapeamento das collections do banco de dados

    public IMongoCollection<User> Users => _mongoDatabase.GetCollection<User>("users");
    public IMongoCollection<Customer> Customers => _mongoDatabase.GetCollection<Customer>("customers");

    #endregion
}

/// <summary>
/// Classe para capturar as configurações do appsettings.json para o MongoDB
/// </summary>
public class MongoDbSettings
{
    public string? ConnectionString { get; set; }
    public string? DatabaseName { get; set; }
}