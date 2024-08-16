using MoonCore.Attributes;
using Moonlight.Client.App.UI.Components.Alerts;
using Moonlight.Client.App.UI.Components.Modals;

namespace Moonlight.Client.App.Services;

[Scoped]
public class AlertService
{
    private readonly ModalService ModalService;

    public AlertService(ModalService modalService)
    {
        ModalService = modalService;
    }

    public Task Information(string title, string text) => LaunchInternal<InformationAlert>(title, text);
    public Task Success(string title, string text) => LaunchInternal<SuccessAlert>(title, text);
    public Task Warning(string title, string text) => LaunchInternal<WarningAlert>(title, text);
    public Task Error(string title, string text) => LaunchInternal<ErrorAlert>(title, text);

    public async Task DangerConfirm(string text, Func<Task> onConfirm, string? icon = null)
    {
        await ModalService.Launch<DangerConfirmAlert>(buildAttributes: parameters =>
        {
            if(!string.IsNullOrEmpty(icon))
                parameters.Add("Icon", icon);
            
            parameters.Add("Text", text);
            parameters.Add("OnConfirm", onConfirm);
        });
    }

    private async Task LaunchInternal<T>(string title, string text) where T : BaseModal
    {
        await ModalService.Launch<T>(buildAttributes: parameters =>
        {
            parameters.Add("Title", title);
            parameters.Add("Text", text);
        });
    }
}