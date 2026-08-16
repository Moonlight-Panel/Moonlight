using System.Text.Json;
using FluentResults;
using Moonlight.Shared.Shared;

namespace Moonlight.Frontend.Infrastructure.Helpers;

public static class HttpExtensions
{
    public static async Task<Result> GetStatusResultAsync(this HttpResponseMessage message)
    {
        if (message.IsSuccessStatusCode)
            return Result.Ok();

        try
        {
            var problemDetails = await message.Content.ReadFromJsonAsync<ProblemDetailsDto>(
                DtoSerializer.Default.Options
            );

            if (problemDetails is null) throw new JsonException("JSON deserialization returned null");

            Result result;

            if (string.IsNullOrWhiteSpace(problemDetails.Detail))
                result = Result.Fail(problemDetails.Title);
            else
                result = Result.Fail($"{problemDetails.Title}: {problemDetails.Detail}");

            if (problemDetails.Errors is not null)
            {
                foreach (var error in problemDetails.Errors)
                    result = result.WithError($"{error.Key}: {string.Join(", ", error.Value)}");
            }

            return result;
        }
        catch (JsonException)
        {
            // When we are unable to parse, we just return all we know at the moment

            return Result.Fail($"Request failed with status code {message.StatusCode}");
        }
    }
}