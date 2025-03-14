using Microsoft.AspNetCore.Mvc;
using MoonCore.Extended.Abstractions;
using MoonCore.Extended.Helpers;
using MoonCore.Extended.PermFilter;
using MoonCore.Helpers;
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
    private readonly CrudHelper<ApiKey, ApiKeyDetailResponse> CrudHelper;
    private readonly DatabaseRepository<ApiKey> ApiKeyRepository;
    private readonly ApiKeyService ApiKeyService;

    public ApiKeysController(CrudHelper<ApiKey, ApiKeyDetailResponse> crudHelper, DatabaseRepository<ApiKey> apiKeyRepository, ApiKeyService apiKeyService)
    {
        CrudHelper = crudHelper;
        ApiKeyRepository = apiKeyRepository;
        ApiKeyService = apiKeyService;
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
        var apiKey = new ApiKey()
        {
            Description = request.Description,
            PermissionsJson = request.PermissionsJson,
            ExpiresAt = request.ExpiresAt
        };

        var finalApiKey = await ApiKeyRepository.Add(apiKey);

        var response = Mapper.Map<CreateApiKeyResponse>(finalApiKey);

        response.Secret = ApiKeyService.GenerateJwt(finalApiKey);
        
        return response;
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