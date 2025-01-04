using Microsoft.AspNetCore.Mvc;
using MoonCore.Attributes;
using MoonCore.Extended.Abstractions;
using MoonCore.Extended.Helpers;
using MoonCore.Helpers;
using MoonCore.Models;
using Moonlight.ApiServer.Database.Entities;
using Moonlight.Shared.Http.Requests.Admin.ApiKeys;
using Moonlight.Shared.Http.Responses.Admin.ApiKeys;

namespace Moonlight.ApiServer.Http.Controllers.Admin.ApiKeys;

[ApiController]
[Route("api/admin/apikeys")]
public class ApiKeysController : Controller
{
    private readonly CrudHelper<ApiKey, ApiKeyDetailResponse> CrudHelper;
    private readonly DatabaseRepository<ApiKey> ApiKeyRepository;

    public ApiKeysController(CrudHelper<ApiKey, ApiKeyDetailResponse> crudHelper, DatabaseRepository<ApiKey> apiKeyRepository)
    {
        CrudHelper = crudHelper;
        ApiKeyRepository = apiKeyRepository;
    }
    
    [HttpGet]
    [RequirePermission("admin.apikeys.read")]
    public async Task<IPagedData<ApiKeyDetailResponse>> Get([FromQuery] int page, [FromQuery] int pageSize = 50)
        => await CrudHelper.Get(page, pageSize);

    [HttpGet("{id}")]
    [RequirePermission("admin.apikeys.read")]
    public async Task<ApiKeyDetailResponse> GetSingle(int id)
        => await CrudHelper.GetSingle(id);

    [HttpPost]
    [RequirePermission("admin.apikeys.create")]
    public async Task<CreateApiKeyResponse> Create([FromBody] CreateApiKeyRequest request)
    {
        var secret = "api_" + Formatter.GenerateString(32);
        
        var apiKey = new ApiKey()
        {
            Description = request.Description,
            PermissionsJson = request.PermissionsJson,
            ExpiresAt = request.ExpiresAt,
            Secret = secret
        };

        var finalApiKey = await ApiKeyRepository.Add(apiKey);
        
        return Mapper.Map<CreateApiKeyResponse>(finalApiKey);
    }

    [HttpPatch("{id}")]
    [RequirePermission("admin.apikeys.update")]
    public async Task<ApiKeyDetailResponse> Update([FromRoute] int id, [FromBody] UpdateApiKeyRequest request)
        => await CrudHelper.Update(id, request);

    [HttpDelete("{id}")]
    [RequirePermission("admin.apikeys.delete")]
    public async Task Delete([FromRoute] int id)
        => await CrudHelper.Delete(id);
}