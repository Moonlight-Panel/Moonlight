using Microsoft.AspNetCore.Mvc;
using MoonCore.Extended.Abstractions;
using MoonCore.Extended.Helpers;
using MoonCore.Helpers;
using Moonlight.ApiServer.App.Database.Entities;
using Moonlight.ApiServer.App.Exceptions;
using Moonlight.ApiServer.App.Helpers;
using Moonlight.Shared.Http.Requests.Admin.Users;
using Moonlight.Shared.Http.Resources.Admin.Users;

namespace Moonlight.ApiServer.App.Http.Controllers.Admin.Users;

[Route("admin/users")]
[ApiController]
public class UsersController : BaseCrudController<User, DetailUserResponse, CreateUserRequest, DetailUserResponse, UpdateUserRequest, DetailUserResponse>
{
    private readonly DatabaseRepository<User> UserRepository;
    
    public UsersController(DatabaseRepository<User> itemRepository) : base(itemRepository)
    {
        UserRepository = itemRepository;
    }

    [HttpPost]
    public override async Task<ActionResult<DetailUserResponse>> Create(CreateUserRequest request)
    {
        request.Email = request.Email.ToLower();
        
        if (UserRepository.Get().Any(x => x.Email == request.Email))
            throw new ApiException("A user with that email address already exists", statusCode: 400);
        
        if (UserRepository.Get().Any(x => x.Username == request.Username))
            throw new ApiException("A user with that username already exists", statusCode: 400);

        request.Password = HashHelper.Hash(request.Password);
        
        var item = Mapper.Map<User>(request!);

        var finalItem = UserRepository.Add(item);

        var response = Mapper.Map<DetailUserResponse>(finalItem);

        return Ok(response);
    }

    [HttpPatch("{id}")]
    public override async Task<ActionResult<DetailUserResponse>> Update([FromRoute] int id, UpdateUserRequest request)
    {
        var item = LoadItemById(id);
        
        var oldPassword = (string)item.Password.Clone();
        
        var mappedItem = Mapper.Map(item, request!);

        if (string.IsNullOrEmpty(request.Password))
            mappedItem.Password = oldPassword;
        
        UserRepository.Update(mappedItem);

        return Mapper.Map<DetailUserResponse>(mappedItem);
    }
}