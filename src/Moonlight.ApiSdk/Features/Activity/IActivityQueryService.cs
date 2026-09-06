using FluentResults;

namespace Moonlight.ApiSdk.Features.Activity;

public interface IActivityQueryService
{
    public IQueryable Query();
}