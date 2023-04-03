using CurrieTechnologies.Razor.SweetAlert2;

namespace Moonlight.App.Services.Interop;

public class AlertService
{
    private readonly SweetAlertService SweetAlertService;
    private readonly SmartTranslateService SmartTranslateService;

    public AlertService(SweetAlertService service, SmartTranslateService smartTranslateService)
    {
        SweetAlertService = service;
        SmartTranslateService = smartTranslateService;
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
    
    public async Task Info(string desciption)
    {
        await Info("", desciption);
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
    
    public async Task Success(string desciption)
    {
        await Success("", desciption);
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
    
    public async Task Warning(string desciption)
    {
        await Warning("", desciption);
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
    
    public async Task Error(string desciption)
    {
        await Error("", desciption);
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
    
    public async Task<string?> Text(string title, string desciption, string setValue)
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

    public async Task<bool> ConfirmMath()
    {
        var r = new Random();
        var i1 = r.Next(5, 15);
        var i2 = r.Next(5, 15);
        
        var input = await Text(
            SmartTranslateService.Translate("Confirm"),
            SmartTranslateService.Translate($"{i1} + {i2} ="),
            ""
        );

        if (int.TryParse(input, out int i))
        {
            if (i == i1 + i2)
            {
                return true;
            }
        }

        return false;
    }
}