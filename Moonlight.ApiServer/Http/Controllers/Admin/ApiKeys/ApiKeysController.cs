using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoonCore.Exceptions;
using MoonCore.Extended.Abstractions;
using MoonCore.Extended.PermFilter;
using MoonCore.Models;
using Moonlight.ApiServer.Database.Entities;
using Moonlight.ApiServer.Services;
using Moonlight.Shared.Http.Requests.Admin.ApiKeys;
using Moonlight.Shared.Http.Responses.Admin.ApiKeys;

namespace Moonlight.ApiServer.Http.Controllers.Admin.ApiKeys;

[ApiController]
[Route("api/admin/apikeys")]
public class ApiKeysController : Controller
{
    private readonly DatabaseRepository<ApiKey> ApiKeyRepository;
    private readonly ApiKeyService ApiKeyService;

    public ApiKeysController(DatabaseRepository<ApiKey> apiKeyRepository, ApiKeyService apiKeyService)
    {
        ApiKeyRepository = apiKeyRepository;
        ApiKeyService = apiKeyService;
    }

    [HttpGet]
    [RequirePermission("admin.apikeys.read")]
    public async Task<IPagedData<ApiKeyResponse>> Get(
        [FromQuery] int page,
        [FromQuery] [Range(1, 100)] int pageSize = 50
    )
    {
        var count = await ApiKeyRepository.Get().CountAsync();

        var apiKeys = await ApiKeyRepository
            .Get()
            .OrderBy(x => x.Id)
            .Skip(page * pageSize)
            .Take(pageSize)
            .ToArrayAsync();

        var mappedApiKey = apiKeys
            .Select(x => new ApiKeyResponse()
            {
                Id = x.Id,
                PermissionsJson = x.PermissionsJson,
                Description = x.Description,
                ExpiresAt = x.ExpiresAt
            })
            .ToArray();

        return new PagedData<ApiKeyResponse>()
        {
            CurrentPage = page,
            Items = mappedApiKey,
            PageSize = pageSize,
            TotalItems = count,
            TotalPages = count == 0 ? 0 : (count - 1) / pageSize
        };
    }

    [HttpGet("{id}")]
    [RequirePermission("admin.apikeys.read")]
    public async Task<ApiKeyResponse> GetSingle(int id)
    {
        var apiKey = await ApiKeyRepository
            .Get()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (apiKey == null)
            throw new HttpApiException("No api key with that id found", 404);

        return new ApiKeyResponse()
        {
            Id = apiKey.Id,
            PermissionsJson = apiKey.PermissionsJson,
            Description = apiKey.Description,
            ExpiresAt = apiKey.ExpiresAt
        };
    }

    [HttpPost]
    [RequirePermission("admin.apikeys.create")]
    public async Task<CreateApiKeyResponse> Create([FromBody] CreateApiKeyRequest request)
    {
        var apiKey = new ApiKey()
        {
            Description = request.Description,
            PermissionsJson = request.PermissionsJson,
            ExpiresAt = request.ExpiresAt
        };

        var finalApiKey = await ApiKeyRepository.Add(apiKey);

        var response = new CreateApiKeyResponse
        {
            Id = finalApiKey.Id,
            PermissionsJson = finalApiKey.PermissionsJson,
            Description = finalApiKey.Description,
            ExpiresAt = finalApiKey.ExpiresAt,
            Secret = ApiKeyService.GenerateJwt(finalApiKey)
        };

        return response;
    }

    [HttpPatch("{id}")]
    [RequirePermission("admin.apikeys.update")]
    public async Task<ApiKeyResponse> Update([FromRoute] int id, [FromBody] UpdateApiKeyRequest request)
    {
        var apiKey = await ApiKeyRepository
            .Get()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (apiKey == null)
            throw new HttpApiException("No api key with that id found", 404);

        apiKey.Description = request.Description;

        await ApiKeyRepository.Update(apiKey);

        return new ApiKeyResponse()
        {
            Id = apiKey.Id,
            Description = apiKey.Description,
            PermissionsJson = apiKey.PermissionsJson,
            ExpiresAt = apiKey.ExpiresAt
        };
    }

    [HttpDelete("{id}")]
    [RequirePermission("admin.apikeys.delete")]
    public async Task Delete([FromRoute] int id)
    {
        var apiKey = await ApiKeyRepository
            .Get()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (apiKey == null)
            throw new HttpApiException("No api key with that id found", 404);

        await ApiKeyRepository.Remove(apiKey);
    }
}