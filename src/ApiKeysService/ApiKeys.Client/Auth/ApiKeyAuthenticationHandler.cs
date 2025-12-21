using System.Security.Claims;
using System.Text.Encodings.Web;
using ApiKeys.Client.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ApiKeys.Client.Auth;

public class ApiKeyAuthenticationHandler(
    IApiKeysClient apiKeysClient,
    IOptionsMonitor<ApiKeyAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    ISystemClock clock)
    : AuthenticationHandler<ApiKeyAuthenticationOptions>(options, logger, encoder, clock)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var apiKey = GetApiKeyFromRequest();

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Logger.LogWarning("API key not found in request");
            return AuthenticateResult.Fail("API key is required");
        }

        try
        {
            var validationResult = await apiKeysClient.ValidateApiKeyAsync(new ValidateApiKeyRequest(apiKey));

            if (validationResult.ApiKeyId is null)
            {
                Logger.LogWarning("API key validation failed: invalid key");
                return AuthenticateResult.Fail("Invalid API key");
            }

            var claims = new List<Claim>
            {
                new(Options.ApiKeyIdClaimType, validationResult.ApiKeyId.Value.ToString())
            };

            if (validationResult.Claims is { Count: > 0 })
            {
                foreach (var claim in validationResult.Claims)
                {
                    claims.Add(new Claim(Options.ApiKeyClaimType, claim));
                }
            }

            var identity = new ClaimsIdentity(claims, Options.SchemeName);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Options.SchemeName);

            Logger.LogInformation("API key authenticated successfully: {ApiKeyId}", validationResult.ApiKeyId);
            return AuthenticateResult.Success(ticket);
        }
        catch (HttpRequestException ex)
        {
            Logger.LogError(ex, "Error validating API key");
            return AuthenticateResult.Fail($"API key validation error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error during API key authentication");
            return AuthenticateResult.Fail("Authentication failed");
        }
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