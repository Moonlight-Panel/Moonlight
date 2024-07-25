using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using MoonCore.Extended.Abstractions;
using MoonCore.Helpers;
using Moonlight.ApiServer.App.Exceptions;
using Moonlight.ApiServer.App.Http.Responses;

namespace Moonlight.ApiServer.App.Helpers;

public abstract class BaseCrudController<TItem, TDetailResponse, TCreateRequest, TCreateResponse, TUpdateRequest, TUpdateResponse> : Controller where TItem : class
{
    private readonly DatabaseRepository<TItem> ItemRepository;

    protected BaseCrudController(DatabaseRepository<TItem> itemRepository)
    {
        ItemRepository = itemRepository;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<TDetailResponse>>> GetAll([FromQuery] int pageSize = 50, [FromQuery] int page = 0)
    {
        if (pageSize > 100)
            throw new ApiException("The page size cannot be greater than 100", statusCode: 400);

        var itemSource = IncludeRelations(ItemRepository.Get());
        
        var items = itemSource
            .Skip(page * pageSize)
            .Take(pageSize)
            .ToArray()
            .Select(x => Mapper.Map<TDetailResponse>(x))
            .ToArray();

        var totalPages = ItemRepository.Get().Count() / pageSize;

        return Ok(new PagedResponse<TDetailResponse>()
        {
            Items = items,
            CurrentPage = page,
            PageSize = pageSize,
            TotalPages = totalPages
        });
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<TDetailResponse>> Get([FromRoute] int id)
    {
        var itemSource = IncludeRelations(ItemRepository.Get());
        
        var item = itemSource.FirstOrDefault(
            GetIdExpression(id)
        );

        if (item == null)
            throw new ApiException($"The item with the id {id} does not exist", statusCode: 404);

        return Ok(Mapper.Map<TDetailResponse>(item));
    }

    [HttpPost]
    public async Task<ActionResult<TCreateResponse>> Post([FromBody] TCreateRequest request)
    {
        var response = await CreateItem(request);

        if (response == null)
            return NoContent();

        return Ok();
    }
    
    [HttpPatch("{id}")]
    public async Task<ActionResult<TUpdateResponse>> Patch([FromRoute] int id, [FromBody] TUpdateRequest request)
    {
        var itemSource = IncludeRelations(ItemRepository.Get());
        
        var item = itemSource.FirstOrDefault(
            GetIdExpression(id)
        );

        if (item == null)
            throw new ApiException($"The item with the id {id} does not exist", statusCode: 404);
        
        var response = await UpdateItem(item, request);

        if (response == null)
            return NoContent();

        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete([FromRoute] int id)
    {
        var itemSource = IncludeRelations(ItemRepository.Get());
        
        var item = itemSource.FirstOrDefault(
            GetIdExpression(id)
        );

        if (item == null)
            throw new ApiException($"The item with the id {id} does not exist", statusCode: 404);

        await DeleteItem(item);

        return NoContent();
    }

    // Overrideables
    
    protected virtual IEnumerable<TItem> IncludeRelations(IQueryable<TItem> items) => items;

    protected virtual async Task<TCreateResponse?> CreateItem(TCreateRequest request)
    {
        var item = Mapper.Map<TItem>(request!);

        var finalItem = ItemRepository.Add(item);

        return Mapper.Map<TCreateResponse>(finalItem);
    }
    
    protected virtual async Task<TUpdateResponse?> UpdateItem(TItem item, TUpdateRequest request)
    {
        var mappedItem = Mapper.Map(item, request!);

        ItemRepository.Update(mappedItem);

        return Mapper.Map<TUpdateResponse>(mappedItem);
    }
    
    protected virtual async Task DeleteItem(TItem item)
    {
        ItemRepository.Delete(item);
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