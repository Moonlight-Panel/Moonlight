using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoonCore.Exceptions;
using MoonCore.Extended.Abstractions;
using MoonCore.Extended.Models;
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
    [Authorize(Policy = "permissions:admin.apikeys.get")]
    public async Task<IPagedData<ApiKeyResponse>> Get([FromQuery] PagedOptions options)
    {
        var count = await ApiKeyRepository.Get().CountAsync();

        var apiKeys = await ApiKeyRepository
            .Get()
            .OrderBy(x => x.Id)
            .Skip(options.Page * options.PageSize)
            .Take(options.PageSize)
            .ToArrayAsync();

        var mappedApiKey = apiKeys
            .Select(x => new ApiKeyResponse()
            {
                Id = x.Id,
                Permissions = x.Permissions,
                Description = x.Description,
                ExpiresAt = x.ExpiresAt
            })
            .ToArray();

        return new PagedData<ApiKeyResponse>()
        {
            CurrentPage = options.Page,
            Items = mappedApiKey,
            PageSize = options.PageSize,
            TotalItems = count,
            TotalPages = count == 0 ? 0 : (count - 1) / options.PageSize
        };
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "permissions:admin.apikeys.get")]
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
            Permissions = apiKey.Permissions,
            Description = apiKey.Description,
            ExpiresAt = apiKey.ExpiresAt
        };
    }

    [HttpPost]
    [Authorize(Policy = "permissions:admin.apikeys.create")]
    public async Task<CreateApiKeyResponse> Create([FromBody] CreateApiKeyRequest request)
    {
        var apiKey = new ApiKey()
        {
            Description = request.Description,
            Permissions = request.Permissions,
            ExpiresAt = request.ExpiresAt
        };

        var finalApiKey = await ApiKeyRepository.Add(apiKey);

        var response = new CreateApiKeyResponse
        {
            Id = finalApiKey.Id,
            Permissions = finalApiKey.Permissions,
            Description = finalApiKey.Description,
            ExpiresAt = finalApiKey.ExpiresAt,
            Secret = ApiKeyService.GenerateJwt(finalApiKey)
        };

        return response;
    }

    [HttpPatch("{id}")]
    [Authorize(Policy = "permissions:admin.apikeys.update")]
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
            Permissions = apiKey.Permissions,
            ExpiresAt = apiKey.ExpiresAt
        };
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "permissions:admin.apikeys.delete")]
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