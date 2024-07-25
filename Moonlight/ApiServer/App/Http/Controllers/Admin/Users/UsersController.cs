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
    private readonly DatabaseRepository<User> DatabaseRepository;
    
    public UsersController(DatabaseRepository<User> itemRepository) : base(itemRepository)
    {
        DatabaseRepository = itemRepository;
    }

    protected override Task<DetailUserResponse?> CreateItem(CreateUserRequest request)
    {
        request.Email = request.Email.ToLower();
        
        if (DatabaseRepository.Get().Any(x => x.Email == request.Email))
            throw new ApiException("A user with that email address already exists", statusCode: 400);
        
        if (DatabaseRepository.Get().Any(x => x.Username == request.Username))
            throw new ApiException("A user with that username already exists", statusCode: 400);

        request.Password = HashHelper.Hash(request.Password);
        
        return base.CreateItem(request);
    }

    protected override async Task<DetailUserResponse?> UpdateItem(User item, UpdateUserRequest request)
    {
        var oldPassword = (string)item.Password.Clone();
        
        var mappedItem = Mapper.Map(item, request!);

        if (string.IsNullOrEmpty(request.Password))
            mappedItem.Password = oldPassword;
        
        DatabaseRepository.Update(mappedItem);

        return Mapper.Map<DetailUserResponse>(mappedItem);
    }
}