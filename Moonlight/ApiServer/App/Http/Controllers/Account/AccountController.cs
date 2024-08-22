using Microsoft.AspNetCore.Mvc;
using MoonCore.Extended.Abstractions;
using MoonCore.Helpers;
using Moonlight.ApiServer.App.Attributes;
using Moonlight.ApiServer.App.Database.Entities;
using Moonlight.ApiServer.App.Exceptions;
using Moonlight.ApiServer.App.Extensions;
using Moonlight.Shared.Http.Requests.Account;
using Moonlight.Shared.Http.Resources.Account;

namespace Moonlight.ApiServer.App.Http.Controllers.Account;

[ApiController]
[Route("account")]
public class AccountController : Controller
{
    private readonly DatabaseRepository<User> UserRepository;

    public AccountController(DatabaseRepository<User> userRepository)
    {
        UserRepository = userRepository;
    }

    [HttpPatch("details")]
    [RequirePermission("meta.authenticated")]
    public async Task<ActionResult<UpdateDetailsResponse>> UpdateDetails([FromBody] UpdateDetailsRequest request)
    {
        var user = HttpContext.GetCurrentUser();
        
        if (UserRepository.Get().Any(x => x.Email == request.Email && x.Id != user.Id))
            throw new ApiException("A user with that email address already exists", statusCode: 400);

        if (UserRepository.Get().Any(x => x.Username == request.Username && x.Id != user.Id))
            throw new ApiException("A user with that username already exists", statusCode: 400);

        var finalUser = Mapper.Map(user, request);
        
        UserRepository.Update(finalUser);

        return Ok(Mapper.Map<UpdateDetailsResponse>(finalUser));
    }
}