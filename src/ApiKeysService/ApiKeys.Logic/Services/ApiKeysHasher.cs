using System.Security.Cryptography;
using System.Text;

namespace ApiKeys.Logic.Services;

public static class ApiKeysHasher
{
    public static string ComputeHash(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToBase64String(hashBytes);
    }
}