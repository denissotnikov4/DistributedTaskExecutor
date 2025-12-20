using System.Security.Cryptography;

namespace ApiKeysService.Logic.Services;

public interface IApiKeyGenerator
{
    string GenerateApiKey();
}

public class ApiKeyGenerator : IApiKeyGenerator
{
    private const string Prefix = "ak_";
    private const int NumberOfSecureBytesToGenerate = 32;
    private const int LengthOfKey = 32;
    
    public string GenerateApiKey()
    {
        var bytes = RandomNumberGenerator.GetBytes(NumberOfSecureBytesToGenerate);

        var base64String = Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_");
        
        var keyLength = LengthOfKey - Prefix.Length; 

        return Prefix + base64String[..keyLength];
    }
}