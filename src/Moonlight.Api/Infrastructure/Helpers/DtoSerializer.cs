using System.Text.Json;
using System.Text.Json.Serialization;
using Moonlight.ApiSdk.Features.Misc;
using Moonlight.Shared.Features.Auth;
using Moonlight.Shared.Features.Users;
using Moonlight.Shared.Shared;

namespace Moonlight.Api.Infrastructure.Helpers;

// Auth
[JsonSerializable(typeof(InfoDto))]

// Users
[JsonSerializable(typeof(UserDto))]
[JsonSerializable(typeof(CreateUserDto))]
[JsonSerializable(typeof(UpdateUserDto))]
[JsonSerializable(typeof(RangedData<UserDto>))]

// Misc
[JsonSerializable(typeof(ProblemDetailsDto))]

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
public partial class DtoSerializer : JsonSerializerContext
{
    
}