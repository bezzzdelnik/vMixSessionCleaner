namespace vMixSessionCleaner.Models;

public sealed class ReplaySession
{
    public required string Id { get; init; }
    public required DateTime CreatedAt { get; init; }
    public long VideoStartTime { get; init; }
    public long VideoEndTime { get; init; } = long.MaxValue;
    public long TotalBytes { get; init; }
    public int EventCount { get; init; }
    public required List<string> Filenames { get; init; }
    public required List<string> RelatedFiles { get; init; }

    public string SizeText => FormatBytes(TotalBytes);
    public int AngleCount => Filenames.Count;

    public static string FormatBytes(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        double value = bytes;
        var unit = 0;
        while (value >= 1024 && unit < units.Length - 1)
        {
            value /= 1024;
            unit++;
        }

        return $"{value:0.##} {units[unit]}";
    }
}
