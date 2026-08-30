using System.Text.Json;
using System.Text.Json.Serialization;
using Moonlight.ApiSdk.Features.Misc;
using Moonlight.Shared.Features.Auth;
using Moonlight.Shared.Features.Roles;
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

// Roles
[JsonSerializable(typeof(RoleDto))]
[JsonSerializable(typeof(CreateRoleDto))]
[JsonSerializable(typeof(UpdateRoleDto))]
[JsonSerializable(typeof(RangedData<RoleDto>))]
[JsonSerializable(typeof(AddMembershipDto))]
[JsonSerializable(typeof(RangedData<MembershipDto>))]

// Misc
[JsonSerializable(typeof(ProblemDetailsDto))]

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
public partial class DtoSerializer : JsonSerializerContext
{
    
}