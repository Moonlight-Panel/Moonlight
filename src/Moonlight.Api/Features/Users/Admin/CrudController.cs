using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moonlight.Api.Infrastructure.Database;
using Moonlight.ApiSdk.Features.Misc;
using Moonlight.Shared.Features.Users;

namespace Moonlight.Api.Features.Users.Admin;

[Authorize]
[ApiController]
[Route("api/admin/users")]
public class CrudController : Controller
{
    private readonly DataContext _dataContext;

    public CrudController(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    [HttpGet]
    public async Task<ActionResult<RangedData<UserDto>>> GetAsync([FromQuery] int limit, [FromQuery] int offset)
    {
        if (limit > 100)
            return Problem("You cannot retrieve more than 100 items", statusCode: 400);

        var totalLength = await _dataContext.Users.CountAsync();

        var users = await _dataContext
            .Users
            .Skip(offset)
            .Take(limit)
            .ProjectToAdminDto()
            .ToArrayAsync();

        return new RangedData<UserDto>(users, totalLength);
    }
}