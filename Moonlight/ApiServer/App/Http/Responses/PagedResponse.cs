namespace Moonlight.ApiServer.App.Http.Responses;

public class PagedResponse<TItem>
{
    public TItem[] Items { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
}