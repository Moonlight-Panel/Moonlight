using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using MoonCore.Extended.Abstractions;
using MoonCore.Extended.Helpers;
using MoonCore.Helpers;
using Moonlight.ApiServer.App.Attributes;
using Moonlight.ApiServer.App.Database.Entities;
using Moonlight.ApiServer.App.Exceptions;
using Moonlight.ApiServer.App.Helpers;
using Moonlight.Shared.Http.Requests.Admin.Users;
using Moonlight.Shared.Http.Resources.Admin.Users;

namespace Moonlight.ApiServer.App.Http.Controllers.Admin.Users;

[ApiController]
[Route("admin/users")]
public class UsersController : BaseCrudController<User, DetailUserResponse, CreateUserRequest, DetailUserResponse, UpdateUserRequest, DetailUserResponse>
{
    public UsersController(DatabaseRepository<User> itemRepository) : base(itemRepository)
    {
        PermissionPrefix = "admin.users";
    }

    [HttpPost]
    [RequirePermission("admin.users.create")]
    public override async Task<ActionResult<DetailUserResponse>> Create(CreateUserRequest request)
    {
        request.Email = request.Email.ToLower();
        
        if (ItemRepository.Get().Any(x => x.Email == request.Email))
            throw new ApiException("A user with that email address already exists", statusCode: 400);
        
        if (ItemRepository.Get().Any(x => x.Username == request.Username))
            throw new ApiException("A user with that username already exists", statusCode: 400);

        request.Password = HashHelper.Hash(request.Password);
        
        var item = Mapper.Map<User>(request!);

        var finalItem = ItemRepository.Add(item);

        var response = Mapper.Map<DetailUserResponse>(finalItem);

        return Ok(response);
    }

    [HttpPatch("{id}")]
    [RequirePermission("admin.users.update")]
    public override async Task<ActionResult<DetailUserResponse>> Update([FromRoute] int id, UpdateUserRequest request)
    {
        var item = LoadItemById(id);

        if (ItemRepository.Get().Any(x => x.Email == request.Email && x.Id != item.Id))
            throw new ApiException("A user with that email address already exists", statusCode: 400);

        if (ItemRepository.Get().Any(x => x.Username == request.Username && x.Id != item.Id))
            throw new ApiException("A user with that username already exists", statusCode: 400);

        if (!string.IsNullOrEmpty(request.Password))
        {
            if (request.Password.Length < 7 || request.Password.Length > 256)
                throw new ApiException("The password needs to be longer than 7 characters and shorter than 256 characters", statusCode: 400);

            request.Password = HashHelper.Hash(request.Password);
        }

        try
        {
            var perms = JsonSerializer.Deserialize<string[]>(request.PermissionsJson);
            ArgumentNullException.ThrowIfNull(perms);
        }
        catch (Exception)
        {
            throw new ApiException("The permissions need to be provided as a valid json string array", statusCode: 400);
        }
        
        var mappedItem = Mapper.Map(item, request!, ignoreNullValues: true);

        if(!string.IsNullOrEmpty(request.Password))
            mappedItem.TokenValidTime = DateTime.UtcNow;
        
        ItemRepository.Update(mappedItem);

        return Mapper.Map<DetailUserResponse>(mappedItem);
    }
}