using Newtonsoft.Json;

namespace Moonlight.App.Http.Resources.Wings;

public class PaginationResult<T>
{
    [JsonProperty("data")]
    public List<T> Data { get; set; }
    
    [JsonProperty("meta")]
    public MetaData Meta { get; set; }

    public PaginationResult()
    {
        Data = new List<T>();
        Meta = new();
    }

    public static PaginationResult<T> CreatePagination(T[] data, int page, int perPage, int totalPages, int totalItems)
    {
        var res = new PaginationResult<T>();

        foreach (var i in data)
        {
            res.Data.Add(i);
        }

        res.Meta.Current_Page = page;
        res.Meta.Total_Pages = totalPages;
        res.Meta.Count = data.Length;
        res.Meta.Total = totalItems;
        res.Meta.Per_Page = perPage;
        res.Meta.Last_Page = totalPages;

        return res;
    }
    
    public class MetaData
    {
        public int Total { get; set; }
        public int Count { get; set; }
        public int Per_Page { get; set; }
        public int Current_Page { get; set; }
        public int Last_Page { get; set; }
        public int Total_Pages { get; set; }
    }
}