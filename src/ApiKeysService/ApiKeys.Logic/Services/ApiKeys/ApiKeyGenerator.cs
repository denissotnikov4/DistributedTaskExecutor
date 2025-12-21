using System.Security.Cryptography;

namespace ApiKeys.Logic.Services.ApiKeys;

public interface IApiKeyGenerator
{
    string Generate();
}

public class ApiKeyGenerator : IApiKeyGenerator
{
    private const string Prefix = "ak_";
    private const int NumberOfSecureBytesToGenerate = 32;
    private const int LengthOfKey = 32;
    
    public string Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(NumberOfSecureBytesToGenerate);

        var base64String = Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_");
        
        var keyLength = LengthOfKey - Prefix.Length; 

        return Prefix + base64String[..keyLength];
    }
}