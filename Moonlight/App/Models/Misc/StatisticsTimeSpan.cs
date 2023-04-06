namespace Moonlight.App.Models.Misc;

public enum StatisticsTimeSpan
{
    Hour = 1,
    Day = 24,
    Month = Day * 31,
    Year = 365 * Day,
    AllTime = Year * 99
}