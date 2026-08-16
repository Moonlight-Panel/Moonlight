using System.Text.Json;
using System.Text.Json.Serialization;
using Moonlight.Shared.Features.Auth;

namespace Moonlight.Shared.Shared;

// Auth
[JsonSerializable(typeof(InfoDto))]

// Misc
[JsonSerializable(typeof(ProblemDetailsDto))]

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
public partial class DtoSerializer : JsonSerializerContext
{
    
}