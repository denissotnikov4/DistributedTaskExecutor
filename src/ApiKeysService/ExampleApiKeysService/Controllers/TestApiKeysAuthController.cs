using ApiKeys.Client.Auth;
using ApiKeys.Client.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace ExampleApiKeysService.Controllers;

[ApiController]
[Route("api/test-apikeys-auth")]
public class TestApiKeysAuthController : ControllerBase
{
    /// <summary>
    /// Публичный эндпоинт без аутентификации
    /// </summary>
    [HttpGet("public")]
    public IActionResult GetPublic()
    {
        return NoContent();
    }

    /// <summary>
    /// Защищенный эндпоинт, требует валидный API-ключ
    /// </summary>
    [HttpGet("protected")]
    [ApiKeyRequired]
    public IActionResult GetProtected()
    {
        var apiKeyId = User.GetApiKeyId();
        var claims = User.GetApiKeyClaims();

        return Ok(new
        {
            apiKeyId, 
            claims
        });
    }

    /// <summary>
    /// Эндпоинт с проверкой claim "secure-claim"
    /// </summary>
    [HttpGet("secure-claim")]
    [ApiKeyRequired(RequiredClaims = ["secure-claim"])]
    public IActionResult GetAdmin()
    {
        var apiKeyId = User.GetApiKeyId();
        var claims = User.GetApiKeyClaims();

        return Ok(new
        {
            apiKeyId,
            claims
        });
    }

    /// <summary>
    /// Эндпоинт с проверкой одного из нескольких claims
    /// </summary>
    [HttpGet("read-write")]
    [ApiKeyRequired(RequiredClaims = new[] { "read", "write", "admin" })]
    public IActionResult GetReadWrite()
    {
        var apiKeyId = User.GetApiKeyId();
        var claims = User.GetApiKeyClaims();

        return Ok(new
        {
            apiKeyId,
            claims
        });
    }

    /// <summary>
    /// Эндпоинт с проверкой claims в коде
    /// </summary>
    [HttpGet("custom-check")]
    [ApiKeyRequired]
    public IActionResult GetCustomCheck()
    {
        var apiKeyId = User.GetApiKeyId();
        var claims = User.GetApiKeyClaims().ToHashSet();

        if (!claims.Contains("some-claim"))
        {
            return Forbid();
        }

        return Ok(new
        {
            apiKeyId,
            claims
        });
    }
}