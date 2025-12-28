using System.Security.Claims;
using System.Text.Encodings.Web;
using ApiKeys.Client.Models;
using DistributedTaskExecutor.Core.Results;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ApiKeys.Client.Auth;

public class ApiKeyAuthenticationHandler(
    IApiKeysClient apiKeysClient,
    IOptionsMonitor<ApiKeyAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<ApiKeyAuthenticationOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var apiKey = this.GetApiKeyFromRequest();

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            this.Logger.LogWarning("API key not found in request");
            return AuthenticateResult.Fail("API key is required");
        }

        var validationResult = await apiKeysClient.ValidateApiKeyAsync(new ValidateApiKeyRequest(apiKey));

        if (validationResult.IsFailure)
        {
            if (validationResult.Error is ClientError clientError)
            {
                this.Logger.LogWarning(
                    "API key validation failed: {StatusCode} - {Message}",
                    clientError.StatusCode,
                    clientError.Message);
            }
            else
            {
                this.Logger.LogWarning("API key validation failed: {Message}", validationResult.Error.Message);
            }

            return AuthenticateResult.Fail(validationResult.Error.Message);
        }

        var result = validationResult.Value;

        if (result.ApiKeyId is null)
        {
            this.Logger.LogWarning("API key validation failed: invalid key");
            return AuthenticateResult.Fail("Invalid API key");
        }

        var claims = new List<Claim>
        {
            new(this.Options.ApiKeyIdClaimType, result.ApiKeyId.Value.ToString())
        };

        if (result.Claims is { Count: > 0 })
        {
            foreach (var claim in result.Claims)
            {
                claims.Add(new Claim(this.Options.ApiKeyClaimType, claim));
            }
        }

        var identity = new ClaimsIdentity(claims, this.Options.SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, this.Options.SchemeName);

        this.Logger.LogInformation("API key authenticated successfully: {ApiKeyId}", result.ApiKeyId);
        return AuthenticateResult.Success(ticket);
    }

    private string? GetApiKeyFromRequest()
    {
        if (this.Request.Headers.TryGetValue(this.Options.HeaderName, out var headerValue))
        {
            return headerValue.ToString();
        }

        if (!string.IsNullOrEmpty(this.Options.QueryParameterName) && this.Request.Query.TryGetValue(this.Options.QueryParameterName, out var queryValue))
        {
            return queryValue.ToString();
        }

        return null;
    }
}