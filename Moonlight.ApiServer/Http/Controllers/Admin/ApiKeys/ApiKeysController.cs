using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoonCore.Extended.Abstractions;
using MoonCore.Models;
using Moonlight.ApiServer.Database.Entities;
using Moonlight.ApiServer.Mappers;
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
    public async Task<ActionResult<ICountedData<ApiKeyResponse>>> Get(
        [FromQuery] int startIndex,
        [FromQuery] int count,
        [FromQuery] string? orderBy,
        [FromQuery] string? filter,
        [FromQuery] string orderByDir = "asc"
    )
    {
        if (count > 100)
            return Problem("You cannot fetch more items than 100 at a time", statusCode: 400);
        
        IQueryable<ApiKey> query = ApiKeyRepository.Get();

        query = orderBy switch
        {
            nameof(ApiKey.Id) => orderByDir == "desc"
                ? query.OrderByDescending(x => x.Id)
                : query.OrderBy(x => x.Id),
                
            nameof(ApiKey.ExpiresAt) => orderByDir == "desc"
                ? query.OrderByDescending(x => x.ExpiresAt)
                : query.OrderBy(x => x.ExpiresAt),
            
            nameof(ApiKey.CreatedAt) => orderByDir == "desc"
                ? query.OrderByDescending(x => x.CreatedAt)
                : query.OrderBy(x => x.CreatedAt),
            
            _ => query.OrderBy(x => x.Id)
        };

        if (!string.IsNullOrEmpty(filter))
        {
            query = query.Where(x =>
                EF.Functions.ILike(x.Description, $"%{filter}%")
            );
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip(startIndex)
            .Take(count)
            .AsNoTracking()
            .ProjectToResponse()
            .ToArrayAsync();

        return new CountedData<ApiKeyResponse>()
        {
            Items = items,
            TotalCount = totalCount
        };
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "permissions:admin.apikeys.get")]
    public async Task<ActionResult<ApiKeyResponse>> GetSingle(int id)
    {
        var apiKey = await ApiKeyRepository
            .Get()
            .AsNoTracking()
            .ProjectToResponse()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (apiKey == null)
            return Problem("No api key with that id found", statusCode: 404);

        return apiKey;
    }

    [HttpPost]
    [Authorize(Policy = "permissions:admin.apikeys.create")]
    public async Task<CreateApiKeyResponse> Create([FromBody] CreateApiKeyRequest request)
    {
        var apiKey = ApiKeyMapper.ToApiKey(request);
        
        var finalApiKey = await ApiKeyRepository.AddAsync(apiKey);

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

    [HttpPatch("{id:int}")]
    [Authorize(Policy = "permissions:admin.apikeys.update")]
    public async Task<ActionResult<ApiKeyResponse>> Update([FromRoute] int id, [FromBody] UpdateApiKeyRequest request)
    {
        var apiKey = await ApiKeyRepository
            .Get()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (apiKey == null)
            return Problem("No api key with that id found", statusCode: 404);

        ApiKeyMapper.Merge(apiKey, request);

        await ApiKeyRepository.UpdateAsync(apiKey);

        return ApiKeyMapper.ToResponse(apiKey);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "permissions:admin.apikeys.delete")]
    public async Task<ActionResult> Delete([FromRoute] int id)
    {
        var apiKey = await ApiKeyRepository
            .Get()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (apiKey == null)
            return Problem("No api key with that id found", statusCode: 404);

        await ApiKeyRepository.RemoveAsync(apiKey);
        return NoContent();
    }
}