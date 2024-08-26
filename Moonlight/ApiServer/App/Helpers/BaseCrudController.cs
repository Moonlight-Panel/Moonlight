using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using MoonCore.Extended.Abstractions;
using MoonCore.Helpers;
using Moonlight.ApiServer.App.Exceptions;
using Moonlight.ApiServer.App.Extensions;
using Moonlight.Shared.Http.Resources;

namespace Moonlight.ApiServer.App.Helpers;

public abstract class BaseCrudController<TItem, TDetailResponse, TCreateRequest, TCreateResponse, TUpdateRequest, TUpdateResponse> : Controller where TItem : class
{
    private readonly DatabaseRepository<TItem> ItemRepository;
    public string PermissionPrefix { get; set; } = "";

    protected BaseCrudController(DatabaseRepository<TItem> itemRepository)
    {
        ItemRepository = itemRepository;
    }

    [HttpGet]
    public virtual async Task<ActionResult<PagedResponse<TDetailResponse>>> GetAll([FromQuery] int pageSize = 50, [FromQuery] int page = 0)
    {
        if (!string.IsNullOrEmpty(PermissionPrefix) && !HttpContext.HasPermission(PermissionPrefix + ".get"))
            throw new MissingPermissionException([PermissionPrefix + ".get"]);
        
        if (pageSize > 100)
            throw new ApiException("The page size cannot be greater than 100", statusCode: 400);

        var itemSource = IncludeRelations(ItemRepository.Get());
        
        var items = itemSource
            .Skip(page * pageSize)
            .Take(pageSize)
            .ToArray()
            .Select(x => Mapper.Map<TDetailResponse>(x))
            .ToArray();

        var totalCount = ItemRepository.Get().Count();
        var totalPages = pageSize == 0 ? pageSize : totalCount / pageSize;

        return Ok(new PagedResponse<TDetailResponse>()
        {
            Items = items,
            CurrentPage = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            TotalItems = totalCount
        });
    }
    
    [HttpGet("{id}")]
    public virtual async Task<ActionResult<TDetailResponse>> GetById([FromRoute] int id)
    {
        if (!string.IsNullOrEmpty(PermissionPrefix) && !HttpContext.HasPermission(PermissionPrefix + ".get"))
            throw new MissingPermissionException([PermissionPrefix + ".get"]);
        
        var item = LoadItemById(id);

        return Ok(Mapper.Map<TDetailResponse>(item));
    }

    [HttpPost]
    public virtual async Task<ActionResult<TCreateResponse>> Create([FromBody] TCreateRequest request)
    {
        if (!string.IsNullOrEmpty(PermissionPrefix) && !HttpContext.HasPermission(PermissionPrefix + ".create"))
            throw new MissingPermissionException([PermissionPrefix + ".create"]);
        
        var item = Mapper.Map<TItem>(request!);

        var finalItem = ItemRepository.Add(item);

        var response = Mapper.Map<TCreateResponse>(finalItem);

        return Ok(response);
    }
    
    [HttpPatch("{id}")]
    public virtual async Task<ActionResult<TUpdateResponse>> Update([FromRoute] int id, [FromBody] TUpdateRequest request)
    {
        if (!string.IsNullOrEmpty(PermissionPrefix) && !HttpContext.HasPermission(PermissionPrefix + ".update"))
            throw new MissingPermissionException([PermissionPrefix + ".update"]);
        
        var item = LoadItemById(id);
        
        var mappedItem = Mapper.Map(item, request!, ignoreNullValues: true);

        ItemRepository.Update(mappedItem);

        var response = Mapper.Map<TUpdateResponse>(mappedItem);

        return Ok(response);
    }

    [HttpDelete("{id}")]
    public virtual async Task<ActionResult> Delete([FromRoute] int id)
    {
        if (!string.IsNullOrEmpty(PermissionPrefix) && !HttpContext.HasPermission(PermissionPrefix + ".delete"))
            throw new MissingPermissionException([PermissionPrefix + ".delete"]);
        
        var item = LoadItemById(id);

        ItemRepository.Delete(item);

        return NoContent();
    }

    // Overrideables
    
    protected virtual IEnumerable<TItem> IncludeRelations(IQueryable<TItem> items) => items;

    protected TItem LoadItemById(int id)
    {
        var itemSource = IncludeRelations(ItemRepository.Get());
        
        var item = itemSource.FirstOrDefault(
            GetIdExpression(id)
        );

        if (item == null)
            throw new ApiException($"The item with the id {id} does not exist", statusCode: 404);

        return item;
    }

    private static Func<TItem, bool> GetIdExpression(int id)
    {
        var parameter = Expression.Parameter(typeof(TItem), "x");
        var property = Expression.Property(parameter, "Id");
        var constant = Expression.Constant(id);
        var equalityExpression = Expression.Equal(property, constant);
        return Expression.Lambda<Func<TItem, bool>>(equalityExpression, parameter).Compile();
    }
}