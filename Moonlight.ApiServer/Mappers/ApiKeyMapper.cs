using Moonlight.ApiServer.Database.Entities;
using Moonlight.Shared.Http.Requests.Admin.ApiKeys;
using Moonlight.Shared.Http.Responses.Admin.ApiKeys;
using Riok.Mapperly.Abstractions;

namespace Moonlight.ApiServer.Mappers;

[Mapper]
public static partial class ApiKeyMapper
{
    // Mappers
    public static partial ApiKeyResponse ToResponse(ApiKey apiKey);
    public static partial ApiKey ToApiKey(CreateApiKeyRequest request);
    public static partial void Merge([MappingTarget] ApiKey apiKey, UpdateApiKeyRequest request);
    
    // EF Relations

    public static partial IQueryable<ApiKeyResponse> ProjectToResponse(this IQueryable<ApiKey> apiKeys);
}