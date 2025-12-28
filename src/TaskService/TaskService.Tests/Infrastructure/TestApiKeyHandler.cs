using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaskService.Api.Constants;

namespace TaskService.Tests.Infrastructure;

public class TestApiKeyHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private const string TestScheme = "TestApiKey";

    public TestApiKeyHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ApiKeyClaims.TasksRead, "true"),
            new Claim(ApiKeyClaims.TasksWrite, "true")
        };
        
        var identity = new ClaimsIdentity(claims, TestScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, TestScheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}