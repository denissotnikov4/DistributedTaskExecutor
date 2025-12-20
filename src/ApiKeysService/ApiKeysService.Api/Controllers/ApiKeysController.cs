using ApiKeysService.Client.Models;
using ApiKeysService.Logic.Services;
using Core.Results;
using Microsoft.AspNetCore.Mvc;

namespace ApiKeysService.Api.Controllers;

[ApiController]
[Route("api/apikeys")]
public class ApiKeysController(IApiKeysService apiKeysService, ILogger<ApiKeysController> logger) : ControllerBase
{
    /// <summary>
    /// Создать новый API-ключ
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiKeyCreateResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateApiKey([FromBody] ApiKeyCreateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await apiKeysService.CreateApiKeyAsync(request, cancellationToken);

        return result.ToActionResult(
            this,
            response => CreatedAtAction(nameof(CreateApiKey), new { id = response.Id }, response)
        );
    }

    /// <summary>
    /// Получить информацию об API-ключе по ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiKeyInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetApiKey(Guid id, CancellationToken cancellationToken)
    {
        var result = await apiKeysService.GetApiKeyInfoAsync(id, cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>
    /// Получить все API-ключи
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ICollection<ApiKeyInfo>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllApiKeys(CancellationToken cancellationToken)
    {
        var result = await apiKeysService.GetAllApiKeysAsync(cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>
    /// Обновить API-ключ
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateApiKey(Guid id, [FromBody] ApiKeyUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await apiKeysService.UpdateApiKeyAsync(id, request, cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>
    /// Удалить API-ключ
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteApiKey(Guid id, CancellationToken cancellationToken)
    {
        var result = await apiKeysService.DeleteApiKeyAsync(id, cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>
    /// Валидировать API-ключ
    /// </summary>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(ApiKeyValidationResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidateApiKey([FromBody] ValidateApiKeyRequest request,
        CancellationToken cancellationToken)
    {
        var result = await apiKeysService.ValidateApiKeyAsync(request.ApiKey, cancellationToken);
        return Ok(result);
    }
}

public class ValidateApiKeyRequest
{
    public string ApiKey { get; set; } = string.Empty;
}