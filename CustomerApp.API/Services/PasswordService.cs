using System.Security.Cryptography;
using System.Text;

namespace CustomerApp.API.Services;

/// <summary>
/// Classe de serviço para rotinas de criptografia de senha
/// </summary>
public class PasswordService
{
    /// <summary>
    /// Criptografar a senha utilizando o algoritmo SHA256 e retornando o hash em Base64
    /// </summary>
    public string HashPassword(string password)
    {
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = SHA256.HashData(bytes);

        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// Compara a senha com e sem hash, retornando true se forem iguais ou false caso contrário
    /// </summary>
    public bool VerifyPasswords(string password, string passwordHash)
    {
        return HashPassword(password) == passwordHash;
    }
}