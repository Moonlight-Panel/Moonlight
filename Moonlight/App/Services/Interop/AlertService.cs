using CurrieTechnologies.Razor.SweetAlert2;

namespace Moonlight.App.Services.Interop;

public class AlertService
{
    private readonly SweetAlertService SweetAlertService;

    public AlertService(SweetAlertService service)
    {
        SweetAlertService = service;
    }

    public async Task Info(string title, string desciption)
    {
        await SweetAlertService.FireAsync(new SweetAlertOptions()
        {
            Title = title,
            Text = desciption,
            Icon = SweetAlertIcon.Info
        });
    }
    
    public async Task Success(string title, string desciption)
    {
        await SweetAlertService.FireAsync(new SweetAlertOptions()
        {
            Title = title,
            Text = desciption,
            Icon = SweetAlertIcon.Success
        });
    }
    
    public async Task Warning(string title, string desciption)
    {
        await SweetAlertService.FireAsync(new SweetAlertOptions()
        {
            Title = title,
            Text = desciption,
            Icon = SweetAlertIcon.Warning
        });
    }
    
    public async Task Error(string title, string desciption)
    {
        await SweetAlertService.FireAsync(new SweetAlertOptions()
        {
            Title = title,
            Text = desciption,
            Icon = SweetAlertIcon.Error
        });
    }
    
    public async Task<bool> YesNo(string title, string desciption, string yesText, string noText)
    {
        var result = await SweetAlertService.FireAsync(new SweetAlertOptions()
        {
            Title = title,
            Text = desciption,
            ShowCancelButton = false,
            ShowDenyButton = true,
            ShowConfirmButton = true,
            ConfirmButtonText = yesText,
            DenyButtonText = noText
        });

        return result.IsConfirmed;
    }
    
    public async Task<string> Text(string title, string desciption, string setValue)
    {
        var result = await SweetAlertService.FireAsync(new SweetAlertOptions()
        {
            Title = title,
            Text = desciption,
            Input = SweetAlertInputType.Text,
            InputValue = setValue
        });

        return result.Value;
    }
}