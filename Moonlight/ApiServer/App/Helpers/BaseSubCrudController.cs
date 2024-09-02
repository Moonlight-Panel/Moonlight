using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using MoonCore.Extended.Abstractions;
using MoonCore.Helpers;
using Moonlight.ApiServer.App.Exceptions;
using Moonlight.ApiServer.App.Extensions;
using Moonlight.Shared.Http.Resources;

namespace Moonlight.ApiServer.App.Helpers;

public abstract class BaseSubCrudController<TRootItem, TItem, TDetailResponse, TCreateRequest, TCreateResponse, TUpdateRequest, TUpdateResponse> : Controller 
    where TItem : class 
    where TRootItem : class
{
    private readonly DatabaseRepository<TItem> ItemRepository;
    private readonly DatabaseRepository<TRootItem> RootItemRepository;
    private readonly TRootItem RootItem;
    
    public string PermissionPrefix { get; set; } = "";
    public abstract Func<TRootItem, List<TItem>> Property { get; set; }


    protected BaseSubCrudController(
        DatabaseRepository<TItem> itemRepository,
        DatabaseRepository<TRootItem> rootItemRepository,
        IHttpContextAccessor contextAccessor)
    {
        ItemRepository = itemRepository;
        RootItemRepository = rootItemRepository;

        var routeValues = contextAccessor.HttpContext!.Request.RouteValues;

        if (!routeValues.ContainsKey("rootItem"))
        {
            throw new ApiException("'rootItem' route data was missing. Is your controller route correct?",
                statusCode: 500);
        }

        var rootItemRouteValue = routeValues["rootItem"];

        if (rootItemRouteValue == null)
        {
            throw new ApiException("'rootItem' route data was missing. Is your controller route correct?",
                statusCode: 500);
        }

        if (rootItemRouteValue is not int rootItemId)
        {
            throw new ApiException("'rootItem' route data was an invalid type (expected: int). Is your controller route correct?",
                statusCode: 500);
        }
        
        RootItem = LoadRootItemById(rootItemId);
    }

    [HttpGet]
    public virtual async Task<ActionResult<PagedResponse<TDetailResponse>>> GetAll([FromQuery] int pageSize = 50, [FromQuery] int page = 0)
    {
        if (!string.IsNullOrEmpty(PermissionPrefix) && !HttpContext.HasPermission(PermissionPrefix + ".get"))
            throw new MissingPermissionException([PermissionPrefix + ".get"]);
        
        if (pageSize > 100)
            throw new ApiException("The page size cannot be greater than 100", statusCode: 400);

        var itemSource = Property.Invoke(RootItem);
        
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

        Property.Invoke(RootItem).Add(item);
        RootItemRepository.Update(RootItem);

        var response = Mapper.Map<TCreateResponse>(item);

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
        
        Property.Invoke(RootItem).Remove(item);
        RootItemRepository.Update(RootItem);

        return NoContent();
    }
    
    protected virtual IEnumerable<TRootItem> IncludeRelations(IQueryable<TRootItem> items) => items;

    protected TRootItem LoadRootItemById(int id)
    {
        var itemSource = IncludeRelations(RootItemRepository.Get());
        
        var item = itemSource.FirstOrDefault(
            GetIdExpression<TRootItem>(id)
        );

        if (item == null)
            throw new ApiException($"The root item with the id {id} does not exist", statusCode: 404);

        return item;
    }
    
    protected TItem LoadItemById(int id)
    {
        var item = ItemRepository.Get().FirstOrDefault(
            GetIdExpression<TItem>(id)
        );

        if (item == null)
            throw new ApiException($"The item with the id {id} does not exist", statusCode: 404);

        return item;
    }

    private static Func<T, bool> GetIdExpression<T>(int id)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, "Id");
        var constant = Expression.Constant(id);
        var equalityExpression = Expression.Equal(property, constant);
        return Expression.Lambda<Func<T, bool>>(equalityExpression, parameter).Compile();
    }
}