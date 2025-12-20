using ApiKeysService.Client.Models;
using ApiKeysService.Logic.Services;
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
    public async Task<ActionResult<ApiKeyCreateResponse>> CreateApiKey([FromBody] ApiKeyCreateRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await apiKeysService.CreateApiKeyAsync(request, cancellationToken);
            logger.LogInformation("API key created with id: {ApiKeyId}", response.Id);
            return CreatedAtAction(nameof(GetApiKey), new { id = response.Id }, response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating API key");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Получить информацию об API-ключе по ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiKeyInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiKeyInfo>> GetApiKey(Guid id, CancellationToken cancellationToken)
    {
        var apiKey = await apiKeysService.GetApiKeyInfoAsync(id, cancellationToken);
        if (apiKey == null)
        {
            return NotFound(new { error = $"API key with id {id} not found" });
        }
        
        return Ok(apiKey);
    }

    /// <summary>
    /// Получить все API-ключи
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ICollection<ApiKeyInfo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ICollection<ApiKeyInfo>>> GetAllApiKeys(CancellationToken cancellationToken)
    {
        var apiKeys = await apiKeysService.GetAllApiKeysAsync(cancellationToken);
        return Ok(apiKeys);
    }

    /// <summary>
    /// Обновить API-ключ
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateApiKey(Guid id, [FromBody] ApiKeyUpdateRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await apiKeysService.UpdateApiKeyAsync(id, request, cancellationToken);
            logger.LogInformation("API key updated with id: {ApiKeyId}", id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = $"API key with id {id} not found" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating API key {ApiKeyId}", id);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Удалить API-ключ
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteApiKey(Guid id, CancellationToken cancellationToken)
    {
        await apiKeysService.DeleteApiKeyAsync(id, cancellationToken);
        logger.LogInformation("API key deleted with id: {ApiKeyId}", id);
        return NoContent();
    }

    /// <summary>
    /// Валидировать API-ключ
    /// </summary>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(ApiKeyValidationResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiKeyValidationResult>> ValidateApiKey([FromBody] ValidateApiKeyRequest request, CancellationToken cancellationToken)
    {
        var result = await apiKeysService.ValidateApiKeyAsync(request.ApiKey, cancellationToken);
        return Ok(result);
    }
}

public class ValidateApiKeyRequest
{
    public string ApiKey { get; set; } = string.Empty;
}

