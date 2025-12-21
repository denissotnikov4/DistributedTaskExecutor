using System.Security.Claims;
using System.Text.Encodings.Web;
using ApiKeys.Client.Models;
using Core.Results;
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
        var apiKey = GetApiKeyFromRequest();

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Logger.LogWarning("API key not found in request");
            return AuthenticateResult.Fail("API key is required");
        }

        var validationResult = await apiKeysClient.ValidateApiKeyAsync(new ValidateApiKeyRequest(apiKey));

        if (validationResult.IsFailure)
        {
            if (validationResult.Error is ClientError clientError)
            {
                Logger.LogWarning(
                    "API key validation failed: {StatusCode} - {Message}",
                    clientError.StatusCode,
                    clientError.Message);
            }
            else
            {
                Logger.LogWarning("API key validation failed: {Message}", validationResult.Error.Message);
            }

            return AuthenticateResult.Fail(validationResult.Error.Message);
        }

        var result = validationResult.Value;

        if (result.ApiKeyId is null)
        {
            Logger.LogWarning("API key validation failed: invalid key");
            return AuthenticateResult.Fail("Invalid API key");
        }

        var claims = new List<Claim>
        {
            new(Options.ApiKeyIdClaimType, result.ApiKeyId.Value.ToString())
        };

        if (result.Claims is { Count: > 0 })
        {
            foreach (var claim in result.Claims)
            {
                claims.Add(new Claim(Options.ApiKeyClaimType, claim));
            }
        }

        var identity = new ClaimsIdentity(claims, Options.SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Options.SchemeName);

        Logger.LogInformation("API key authenticated successfully: {ApiKeyId}", result.ApiKeyId);
        return AuthenticateResult.Success(ticket);
    }

    private string? GetApiKeyFromRequest()
    {
        if (Request.Headers.TryGetValue(Options.HeaderName, out var headerValue))
        {
            return headerValue.ToString();
        }

        if (!string.IsNullOrEmpty(Options.QueryParameterName) &&
            Request.Query.TryGetValue(Options.QueryParameterName, out var queryValue))
        {
            return queryValue.ToString();
        }

        return null;
    }
}