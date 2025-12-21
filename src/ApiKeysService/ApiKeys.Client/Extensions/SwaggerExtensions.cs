using ApiKeys.Client.Auth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ApiKeys.Client.Extensions;

public static class SwaggerExtensions
{
    public static void AddApiKeySecurity(
        this SwaggerGenOptions options,
        string? headerName = null,
        string? description = null)
    {
        headerName ??= ApiKeyAuthConstants.HeaderName;
        description ??= "Введите ApiKey.";

        options.AddSecurityDefinition(ApiKeyAuthConstants.SchemeName, new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Header,
            Name = headerName,
            Description = description
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = ApiKeyAuthConstants.SchemeName
                    }
                },
                Array.Empty<string>()
            }
        });
    }
}

