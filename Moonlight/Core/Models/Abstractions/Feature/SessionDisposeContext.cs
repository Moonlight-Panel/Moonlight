using Moonlight.Core.Services;

namespace Moonlight.Core.Models.Abstractions.Feature;

public class SessionDisposeContext
{
    public ScopedStorageService ScopedStorageService { get; set; }
}