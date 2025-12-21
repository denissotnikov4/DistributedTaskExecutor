using System.ComponentModel.DataAnnotations;
using ApiKeys.Api.Configuration;
using ApiKeys.Client.Models;
using ApiKeys.Logic.Services.ApiKeys;
using Core.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiKeys.Api.Controllers;

[ApiController]
[Route("api/apikeys")]
[Authorize(Policy = AuthPolicies.ManageApiKey)]
public class ApiKeysController(IApiKeysService apiKeysService) : ControllerBase
{
    /// <summary>
    /// Создать новый API-ключ
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiKeyCreateResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateApiKey([FromBody] ApiKeyCreateRequest request)
    {
        var result = await apiKeysService.CreateApiKeyAsync(request);

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
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetApiKey(Guid id)
    {
        var result = await apiKeysService.GetApiKeyInfoAsync(id);
        return result.ToActionResult(this);
    }

    /// <summary>
    /// Получить все API-ключи
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ICollection<ApiKeyInfo>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllApiKeys()
    {
        var result = await apiKeysService.GetAllApiKeysAsync();
        return result.ToActionResult(this);
    }

    /// <summary>
    /// Обновить API-ключ
    /// Требует claim "ManageApiKey"
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateApiKey(Guid id, [FromBody] ApiKeyUpdateRequest request)
    {
        var result = await apiKeysService.UpdateApiKeyAsync(id, request);
        return result.ToActionResult(this);
    }

    /// <summary>
    /// Удалить API-ключ
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteApiKey(Guid id)
    {
        var result = await apiKeysService.DeleteApiKeyAsync(id);
        return result.ToActionResult(this);
    }

    /// <summary>
    /// Валидировать API-ключ
    /// </summary>
    [HttpPost("validate")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiKeyValidationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ValidateApiKey([FromBody] [Required] ValidateApiKeyRequest request)
    {
        var result = await apiKeysService.ValidateApiKeyAsync(request.ApiKey);
        return result.ToActionResult(this);
    }
}