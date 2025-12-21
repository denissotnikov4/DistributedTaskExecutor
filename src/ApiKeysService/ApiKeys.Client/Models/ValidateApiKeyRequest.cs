using System.ComponentModel.DataAnnotations;

namespace ApiKeys.Client.Models;

public record ValidateApiKeyRequest([Required] string ApiKey);