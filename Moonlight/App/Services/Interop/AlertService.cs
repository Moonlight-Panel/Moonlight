using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.JSInterop;

namespace Moonlight.App.Services.Interop;

public class AlertService
{
    private readonly SmartTranslateService SmartTranslateService;
    private readonly IJSRuntime JsRuntime;
    private SweetAlertService? SweetAlertService;

    public AlertService(SmartTranslateService smartTranslateService, IJSRuntime jsRuntime)
    {
        SmartTranslateService = smartTranslateService;
        JsRuntime = jsRuntime;
    }

    // We create the swal service here and not using the dependency injection
    // because it initializes when instantiated which leads to js invoke errors 
    private Task EnsureService()
    {
        if (SweetAlertService == null)
        {
            SweetAlertService = new(JsRuntime, new()
            {
                Theme = SweetAlertTheme.Dark
            });
        }
        
        return Task.CompletedTask;
    }

    public async Task Info(string title, string desciption)
    {
        await EnsureService();
        
        await SweetAlertService!.FireAsync(new SweetAlertOptions()
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
        await EnsureService();
        
        await SweetAlertService!.FireAsync(new SweetAlertOptions()
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
        await EnsureService();
        
        await SweetAlertService!.FireAsync(new SweetAlertOptions()
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
        await EnsureService();
        
        await SweetAlertService!.FireAsync(new SweetAlertOptions()
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
        await EnsureService();
        
        var result = await SweetAlertService!.FireAsync(new SweetAlertOptions()
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
        await EnsureService();
        
        var result = await SweetAlertService!.FireAsync(new SweetAlertOptions()
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
            $"{i1} + {i2} =",
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