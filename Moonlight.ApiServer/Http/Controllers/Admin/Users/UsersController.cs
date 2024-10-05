using Microsoft.AspNetCore.Mvc;
using MoonCore.Exceptions;
using MoonCore.Extended.Abstractions;
using MoonCore.Extended.Helpers;
using MoonCore.Models;
using Moonlight.ApiServer.Database.Entities;
using Moonlight.Shared.Http.Requests.Admin.Users;

namespace Moonlight.ApiServer.Http.Controllers.Admin.Users;

[ApiController]
[Route("api/admin/users")]
public class UsersController : Controller
{
    private readonly CrudHelper<User> CrudHelper;
    private readonly DatabaseRepository<User> UserRepository;

    public UsersController(CrudHelper<User> crudHelper, DatabaseRepository<User> userRepository)
    {
        CrudHelper = crudHelper;
        UserRepository = userRepository;
    }

    [HttpGet]
    public async Task<IPagedData<User>> Get([FromQuery] int page, [FromQuery] int pageSize = 50)
        => await CrudHelper.Get(page, pageSize);

    [HttpGet("{id}")]
    public async Task<User> GetSingle(int id)
        => await CrudHelper.GetSingle(id);

    [HttpPost]
    public async Task<User> Create([FromBody] CreateUserRequest request)
    {
        // Reformat values
        request.Username = request.Username.ToLower().Trim();
        request.Email = request.Email.ToLower().Trim();
        
        // Check for users with the same values
        if (UserRepository.Get().Any(x => x.Username == request.Username))
            throw new HttpApiException("A user with that username already exists", 400);
        
        if (UserRepository.Get().Any(x => x.Email == request.Email))
            throw new HttpApiException("A user with that email address already exists", 400);

        request.Password = HashHelper.Hash(request.Password);

        return await CrudHelper.Create(request);
    }

    [HttpPatch("{id}")]
    public async Task<User> Update([FromRoute] int id, [FromBody] UpdateUserRequest request)
    {
        var user = await CrudHelper.GetSingle(id);
        
        // Reformat values
        request.Username = request.Username.ToLower().Trim();
        request.Email = request.Email.ToLower().Trim();
        
        // Check for users with the same values
        if (UserRepository.Get().Any(x => x.Username == request.Username && x.Id != user.Id))
            throw new HttpApiException("A user with that username already exists", 400);
        
        if (UserRepository.Get().Any(x => x.Email == request.Email && x.Id != user.Id))
            throw new HttpApiException("A user with that email address already exists", 400);

        return await CrudHelper.Update(user, request);
    }

    [HttpDelete("{id}")]
    public async Task Delete([FromRoute] int id)
        => await CrudHelper.Delete(id);
}