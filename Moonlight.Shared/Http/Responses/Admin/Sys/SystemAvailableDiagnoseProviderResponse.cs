using Moonlight.Shared.Misc;

namespace Moonlight.Shared.Http.Responses.Admin.Sys;

public class SystemAvailableDiagnoseProviderResponse
{
    public DiagnoseProvider[] AvailableProviders { get; set; }= [];
}
