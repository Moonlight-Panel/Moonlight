using Moonlight.Client.App.Models.Forms;
using Moonlight.Shared.Http.Resources;

namespace Moonlight.Client.App.Models.Crud;

public class CrudOptions<TItem, TCreateForm, TUpdateForm>
{
    public Func<int, int, Task<PagedResponse<TItem>>>? Loader { get; set; }
    public Func<string, int, int, Task<PagedResponse<TItem>>>? LoaderWithSearch { get; set; }
    public Func<TCreateForm, Task> CreateFunction { get; set; }
    public Func<TUpdateForm, TItem, Task> UpdateFunction { get; set; }
    public Func<TItem, Task> DeleteFunction { get; set; }

    public Action<SmartFormOption<TCreateForm>> OnConfigureCreate { get; set; }
    public Action<SmartFormOption<TUpdateForm>, TItem> OnConfigureUpdate { get; set; }

    public bool ShowCreateAsModal { get; set; } = true;
    public bool ShowUpdateAsModal { get; set; } = true;
}