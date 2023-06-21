using Moonlight.App.Helpers;
using Octokit;
using Repository = LibGit2Sharp.Repository;

namespace Moonlight.App.Services;

public class MoonlightService
{
    private readonly ConfigService ConfigService;
    public readonly DateTime StartTimestamp;
    public readonly string AppVersion;
    public readonly List<string[]> ChangeLog = new();

    public MoonlightService(ConfigService configService)
    {
        ConfigService = configService;
        StartTimestamp = DateTime.UtcNow;
        
        if (File.Exists("version") && !ConfigService.DebugMode)
            AppVersion = File.ReadAllText("version");
        else if (ConfigService.DebugMode)
        {
            string repositoryPath = Path.GetFullPath("..");
            using var repo = new Repository(repositoryPath);
            var commit = repo.Head.Tip;
            AppVersion = commit.Sha;
        }
        else
            AppVersion = "unknown";

        Task.Run(FetchChangeLog);
    }

    private async Task FetchChangeLog()
    {
        if (ConfigService.DebugMode)
        {
            ChangeLog.Add(new[]
            {
                "Disabled",
                "Fetching changelog from github is disabled in debug mode"
            });
            
            return;
        }

        try
        {
            var client = new GitHubClient(new ProductHeaderValue("Moonlight"));
            
            var pullRequests = await client.PullRequest.GetAllForRepository("Moonlight-Panel", "Moonlight", new PullRequestRequest
            {
                State = ItemStateFilter.Closed,
                SortDirection = SortDirection.Ascending,
                SortProperty = PullRequestSort.Created
            });

            var groupedPullRequests = new Dictionary<DateTime, List<string>>();

            foreach (var pullRequest in pullRequests)
            {
                if (pullRequest.MergedAt != null)
                {
                    var date = pullRequest.MergedAt.Value.Date;

                    if (!groupedPullRequests.ContainsKey(date))
                    {
                        groupedPullRequests[date] = new List<string>();
                    }

                    groupedPullRequests[date].Add(pullRequest.Title);
                }
            }

            int i = 1;
            foreach (var group in groupedPullRequests)
            {
                var pullRequestsList = new List<string>();
                var date = group.Key.ToString("dd.MM.yyyy");

                pullRequestsList.Add($"Patch {i}, {date}");
        
                foreach (var pullRequest in group.Value)
                {
                    pullRequestsList.Add(pullRequest);
                }

                ChangeLog.Add(pullRequestsList.ToArray());
                i++;
            }
        }
        catch (Exception e)
        {
            Logger.Warn("Error fetching changelog");
            Logger.Warn(e);
        }
    }
}