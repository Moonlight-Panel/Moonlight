using Microsoft.AspNetCore.Mvc;
using MoonCore.Exceptions;
using MoonCore.Extended.Abstractions;
using MoonCore.Extended.Helpers;
using MoonCore.Models;
using Moonlight.ApiServer.Attributes;
using Moonlight.ApiServer.Database.Entities;
using Moonlight.Shared.Http.Requests.Admin.Users;
using Moonlight.Shared.Http.Responses.Admin.Users;

namespace Moonlight.ApiServer.Http.Controllers.Admin.Users;

[ApiController]
[Route("api/admin/users")]
public class UsersController : Controller
{
    private readonly CrudHelper<User, UserDetailResponse> CrudHelper;
    private readonly DatabaseRepository<User> UserRepository;

    public UsersController(CrudHelper<User, UserDetailResponse> crudHelper, DatabaseRepository<User> userRepository)
    {
        CrudHelper = crudHelper;
        UserRepository = userRepository;
    }

    [HttpGet]
    [RequirePermission("admin.users.read")]
    public async Task<IPagedData<UserDetailResponse>> Get([FromQuery] int page, [FromQuery] int pageSize = 50)
        => await CrudHelper.Get(page, pageSize);

    [HttpGet("{id}")]
    [RequirePermission("admin.users.read")]
    public async Task<UserDetailResponse> GetSingle(int id)
        => await CrudHelper.GetSingle(id);

    [HttpPost]
    [RequirePermission("admin.users.create")]
    public async Task<UserDetailResponse> Create([FromBody] CreateUserRequest request)
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
    [RequirePermission("admin.users.update")]
    public async Task<UserDetailResponse> Update([FromRoute] int id, [FromBody] UpdateUserRequest request)
    {
        var user = await CrudHelper.GetSingleModel(id);
        
        // Reformat values
        request.Username = request.Username.ToLower().Trim();
        request.Email = request.Email.ToLower().Trim();
        
        // Check for users with the same values
        if (UserRepository.Get().Any(x => x.Username == request.Username && x.Id != user.Id))
            throw new HttpApiException("A user with that username already exists", 400);
        
        if (UserRepository.Get().Any(x => x.Email == request.Email && x.Id != user.Id))
            throw new HttpApiException("A user with that email address already exists", 400);
        
        // Perform hashing the password if required
        if (!string.IsNullOrEmpty(request.Password))
        {
            request.Password = HashHelper.Hash(request.Password);
            user.TokenValidTimestamp = DateTime.UtcNow; // This change will get applied by the crud helper
        }
        
        return await CrudHelper.Update(user, request);
    }

    [HttpDelete("{id}")]
    [RequirePermission("admin.users.delete")]
    public async Task Delete([FromRoute] int id)
        => await CrudHelper.Delete(id);
}