using Moonlight.App.Database.Entities;

namespace Moonlight.App.Helpers;

public static class AvgHelper
{
    public static StatisticsData[] Calculate(StatisticsData[] data, int splitSize = 40)
    {
        if (data.Length <= splitSize)
            return data;

        var result = new List<StatisticsData>();

        var i = data.Length / (float)splitSize;
        var pc = (int)Math.Round(i);

        foreach (var part in data.Chunk(pc))
        {
            double d = 0;
            var res = new StatisticsData();

            foreach (var entry in part)
            {
                d += entry.Value;
            }

            res.Chart = part.First().Chart;
            res.Date = part.First().Date;

            if (d == 0)
                res.Value = 0;

            res.Value = d / part.Length;
            result.Add(res);
        }

        return result.ToArray();
    }
}