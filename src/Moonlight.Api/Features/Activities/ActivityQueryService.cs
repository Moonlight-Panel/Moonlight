using Moonlight.Api.Infrastructure.Database;
using Moonlight.ApiSdk.Features.Activity;

namespace Moonlight.Api.Features.Activities;

public class ActivityQueryService : IActivityQueryService
{
    private readonly DataContext _dataContext;

    public ActivityQueryService(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public IQueryable<Activity> Query()
    {
        return _dataContext.Activities;
    }
}