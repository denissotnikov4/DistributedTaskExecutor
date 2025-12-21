using ApiKeys.Api.Configuration;
using Core.Auth;
using Core.DI;

namespace ApiKeys.Api.DI;

public class AuthDiModule : IDiModule
{
    public void RegisterIn(IServiceCollection services, IConfiguration configuration)
    {
        services.AddJwtAuth(configuration);
        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthPolicies.ManageApiKey, policy => 
                policy.RequireClaim("claim", Claims.ManageApiKey));
        });
    }
}