using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MoonCore.Extended.Abstractions;
using MoonCore.Helpers;
using Moonlight.ApiServer.App.Attributes;
using Moonlight.ApiServer.App.Database.Entities;
using Moonlight.ApiServer.App.Exceptions;
using Moonlight.ApiServer.App.Helpers;
using Moonlight.Shared.Http.Requests.Admin.ApiKeys;
using Moonlight.Shared.Http.Resources.Admin.ApiKeys;

namespace Moonlight.ApiServer.App.Http.Controllers.Admin;

[ApiController]
[Route("admin/apikeys")]
public class ApiKeysController : BaseCrudController<ApiKey, DetailApiKeyResponse, CreateApiKeyRequest, CreateApiKeyResponse, UpdateApiKeyRequest, DetailApiKeyResponse>
{
    private DatabaseRepository<ApiKey> ApiKeyRepository;
    
    public ApiKeysController(DatabaseRepository<ApiKey> itemRepository) : base(itemRepository)
    {
        ApiKeyRepository = itemRepository;
        
        PermissionPrefix = "admin.apikeys";
    }

    [HttpPost]
    [RequirePermission("admin.apikeys.create")]
    public override async Task<ActionResult<CreateApiKeyResponse>> Create(CreateApiKeyRequest request)
    {
        // Validate expire date
        if (request.ExpireDate < DateTime.UtcNow)
            throw new ApiException("The expire date cannot be in the past", statusCode: 400);
        
        // Validate json
        try
        {
            var permissions = JsonSerializer.Deserialize<string[]>(request.PermissionsJson);
            
            if(permissions == null)
                throw new ApiException("Invalid permission json string provided", statusCode: 400);
        }
        catch (Exception)
        {
            throw new ApiException("Invalid permission json string provided", statusCode: 400);
        }

        // Build model
        var apiKey = Mapper.Map<ApiKey>(request);

        // Generate api key
        apiKey.Key = "api_" + Guid
            .NewGuid()
            .ToString()
            .Replace("-", "");

        var finalApiKey = ApiKeyRepository.Add(apiKey);

        var result = Mapper.Map<CreateApiKeyResponse>(finalApiKey);
        return Ok(result);
    }

    [HttpPatch("{id}")]
    [RequirePermission("admin.apikeys.update")]
    public override Task<ActionResult<DetailApiKeyResponse>> Update(int id, UpdateApiKeyRequest request)
    {
        // Validate expire date
        if (request.ExpireDate < DateTime.UtcNow)
            throw new ApiException("The expire date cannot be in the past", statusCode: 400);
        
        // Validate json
        try
        {
            var permissions = JsonSerializer.Deserialize<string[]>(request.PermissionsJson);
            
            if(permissions == null)
                throw new ApiException("Invalid permission json string provided", statusCode: 400);
        }
        catch (Exception)
        {
            throw new ApiException("Invalid permission json string provided", statusCode: 400);
        }
        
        return base.Update(id, request);
    }
}