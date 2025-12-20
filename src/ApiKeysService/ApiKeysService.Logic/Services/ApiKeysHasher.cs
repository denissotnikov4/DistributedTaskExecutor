using System.Security.Cryptography;
using System.Text;

namespace ApiKeysService.Logic.Services;

public class ApiKeysHasher
{
    public static string ComputeHash(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToBase64String(hashBytes);
    }
}