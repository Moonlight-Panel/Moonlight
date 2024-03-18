namespace Moonlight.Core.Models.Abstractions.Feature;

public abstract class MoonlightFeature
{
    public string Name { get; set; } = "";
    public string Author { get; set; } = "";
    public string IssueTracker { get; set; } = "";

    public virtual Task OnPreInitialized(PreInitContext context) => Task.CompletedTask;
    public virtual Task OnInitialized(InitContext context) => Task.CompletedTask;
    public virtual Task OnUiInitialized(UiInitContext context) => Task.CompletedTask;

    public virtual Task OnSessionInitialized(SessionInitContext context) => Task.CompletedTask;
    public virtual Task OnSessionDisposed(SessionDisposeContext context) => Task.CompletedTask;
    
    // A little explanation:
    // We cannot use IServiceProvider to pass it to the feature in order for it to resolve and dispose
    // as blazor disposes the IServiceProvider before we can detect it
    // thats why you should save the services you need to dispose (which should be done in the page anyways)
    // in the scoped storage
}