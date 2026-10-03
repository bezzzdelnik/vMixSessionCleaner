using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using vMixSessionCleaner.Localization;
using vMixSessionCleaner.Models;

namespace vMixSessionCleaner.Services;

public sealed class ReplayProjectService
{
    private static readonly Regex SessionFileRegex = new(
        @"^(?<id>stream-\d{8}-\d{6})-(?<angle>\d+)\.avi$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public string? ProjectPath { get; private set; }
    public string XmlPath => Path.Combine(ProjectPath ?? string.Empty, "replay2.xml");
    public IReadOnlyList<ReplaySession> Sessions { get; private set; } = [];

    public void Load(string projectPath)
    {
        if (!Directory.Exists(projectPath))
            throw new DirectoryNotFoundException(Loc.F("ErrFolderNotFound", projectPath));

        var xmlPath = Path.Combine(projectPath, "replay2.xml");
        if (!File.Exists(xmlPath))
            throw new FileNotFoundException(Loc.T("ErrNoReplayXml"), xmlPath);

        ProjectPath = projectPath;
        Sessions = BuildSessions(projectPath, xmlPath);
    }

    // Только основное приложение vMix, не сервис драйверов и не этот cleaner.
    private static readonly HashSet<string> VmixProcessNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "vMix",
        "vMix64"
    };

    public static bool IsVmixRunning()
    {
        return Process.GetProcesses()
            .Any(p =>
            {
                try
                {
                    return VmixProcessNames.Contains(p.ProcessName);
                }
                catch
                {
                    return false;
                }
            });
    }

    public DeleteResult DeleteSessions(IEnumerable<string> sessionIds)
    {
        if (string.IsNullOrWhiteSpace(ProjectPath))
            throw new InvalidOperationException(Loc.T("ErrProjectNotLoaded"));

        var ids = sessionIds.Distinct(StringComparer.OrdinalIgnoreCase).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (ids.Count == 0)
            return new DeleteResult(0, 0, 0, 0, []);

        var selected = Sessions.Where(s => ids.Contains(s.Id)).ToList();
        if (selected.Count == 0)
            return new DeleteResult(0, 0, 0, 0, []);

        var xmlPath = XmlPath;
        var backupPath = xmlPath + $".cleaner-backup-{DateTime.Now:yyyyMMdd-HHmmss}";
        File.Copy(xmlPath, backupPath, overwrite: false);

        var doc = XDocument.Load(xmlPath, LoadOptions.PreserveWhitespace);
        var root = doc.Root ?? throw new InvalidOperationException(Loc.T("ErrBadXml"));

        var filesToDelete = selected
            .SelectMany(s => s.RelatedFiles)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var filenames = selected
            .SelectMany(s => s.Filenames)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var removedSegments = 0;
        foreach (var stream in root.Elements("stream"))
        {
            var doomed = stream.Elements("segment")
                .Where(s => filenames.Contains((string?)s.Element("filename") ?? string.Empty))
                .ToList();

            foreach (var segment in doomed)
            {
                segment.Remove();
                removedSegments++;
            }
        }

        var removedEvents = 0;
        foreach (var list in root.Element("events")?.Elements("list") ?? [])
        {
            var doomedEvents = list.Elements("event")
                .Where(e => selected.Any(session => EventBelongsToSession(e, session)))
                .ToList();

            foreach (var evt in doomedEvents)
            {
                evt.Remove();
                removedEvents++;
            }
        }

        var tempXml = xmlPath + ".tmp";
        doc.Save(tempXml);
        File.Copy(tempXml, xmlPath, overwrite: true);
        File.Delete(tempXml);

        var xmlBackup = xmlPath + ".backup";
        File.Copy(xmlPath, xmlBackup, overwrite: true);

        var deletedFiles = 0;
        long freedBytes = 0;
        var errors = new List<string>();

        foreach (var file in filesToDelete)
        {
            try
            {
                if (!File.Exists(file))
                    continue;

                var info = new FileInfo(file);
                var size = info.Length;
                info.IsReadOnly = false;
                info.Delete();
                deletedFiles++;
                freedBytes += size;
            }
            catch (Exception ex)
            {
                errors.Add($"{Path.GetFileName(file)}: {ex.Message}");
            }
        }

        Load(ProjectPath);
        return new DeleteResult(selected.Count, deletedFiles, removedSegments, removedEvents, errors, freedBytes, backupPath);
    }

    public IReadOnlyList<BackupInfo> ListBackups()
    {
        if (string.IsNullOrWhiteSpace(ProjectPath) || !Directory.Exists(ProjectPath))
            return [];

        return Directory.EnumerateFiles(ProjectPath, "replay2.xml.cleaner-backup-*")
            .Select(path =>
            {
                var info = new FileInfo(path);
                return new BackupInfo(path, info.LastWriteTime, info.Length);
            })
            .OrderByDescending(b => b.CreatedAt)
            .ToList();
    }

    public RestoreResult RestoreBackup(string backupPath)
    {
        if (string.IsNullOrWhiteSpace(ProjectPath))
            throw new InvalidOperationException(Loc.T("ErrProjectNotLoaded"));

        if (!File.Exists(backupPath))
            throw new FileNotFoundException(Loc.T("ErrBackupNotFound"), backupPath);

        var xmlPath = XmlPath;
        if (!File.Exists(xmlPath))
            throw new FileNotFoundException(Loc.T("ErrCurrentXmlNotFound"), xmlPath);

        // Страховка текущего XML перед откатом.
        var safetyCopyPath = xmlPath + $".pre-restore-{DateTime.Now:yyyyMMdd-HHmmss}";
        File.Copy(xmlPath, safetyCopyPath, overwrite: false);

        File.Copy(backupPath, xmlPath, overwrite: true);
        File.Copy(xmlPath, xmlPath + ".backup", overwrite: true);

        Load(ProjectPath);

        var missing = Sessions
            .SelectMany(s => s.Filenames)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count(name => !File.Exists(Path.Combine(ProjectPath, name)));

        return new RestoreResult(backupPath, safetyCopyPath, missing);
    }

    private static List<ReplaySession> BuildSessions(string projectPath, string xmlPath)
    {
        var doc = XDocument.Load(xmlPath);
        var root = doc.Root ?? throw new InvalidOperationException(Loc.T("ErrBadXml"));

        var segmentMap = new Dictionary<string, SessionAccumulator>(StringComparer.OrdinalIgnoreCase);

        foreach (var stream in root.Elements("stream"))
        {
            foreach (var segment in stream.Elements("segment"))
            {
                var filename = (string?)segment.Element("filename");
                if (string.IsNullOrWhiteSpace(filename))
                    continue;

                var match = SessionFileRegex.Match(filename);
                if (!match.Success)
                    continue;

                var id = match.Groups["id"].Value;
                var timestampText = (string?)segment.Element("timestamp");
                var videoStartText = (string?)segment.Element("videoStartTime") ?? "0";
                var videoStart = long.Parse(videoStartText, CultureInfo.InvariantCulture);

                if (!segmentMap.TryGetValue(id, out var acc))
                {
                    acc = new SessionAccumulator(id, ParseTimestamp(timestampText, id), videoStart);
                    segmentMap[id] = acc;
                }

                acc.Filenames.Add(filename);
                acc.VideoStartTime = Math.Min(acc.VideoStartTime, videoStart);
            }
        }

        foreach (var avi in Directory.EnumerateFiles(projectPath, "stream-*.avi"))
        {
            var name = Path.GetFileName(avi);
            var match = SessionFileRegex.Match(name);
            if (!match.Success)
                continue;

            var id = match.Groups["id"].Value;
            if (!segmentMap.TryGetValue(id, out var acc))
            {
                acc = new SessionAccumulator(id, File.GetCreationTime(avi), 0);
                segmentMap[id] = acc;
            }

            acc.Filenames.Add(name);
        }

        var ordered = segmentMap.Values
            .OrderBy(s => s.CreatedAt)
            .ThenBy(s => s.Id, StringComparer.OrdinalIgnoreCase)
            .ToList();

        for (var i = 0; i < ordered.Count; i++)
        {
            ordered[i].VideoEndTime = i + 1 < ordered.Count
                ? ordered[i + 1].VideoStartTime
                : long.MaxValue;
        }

        var events = root.Element("events")?
            .Elements("list")
            .SelectMany(l => l.Elements("event"))
            .ToList() ?? [];

        var dataDir = Path.Combine(projectPath, "data");
        var sessions = new List<ReplaySession>(ordered.Count);

        foreach (var acc in ordered)
        {
            var filenames = acc.Filenames
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var related = new List<string>();
            long totalBytes = 0;

            foreach (var filename in filenames)
            {
                AddFile(related, ref totalBytes, Path.Combine(projectPath, filename));
                AddFile(related, ref totalBytes, Path.Combine(dataDir, filename + ".adata"));
                AddFile(related, ref totalBytes, Path.Combine(dataDir, filename + ".vdata"));
            }

            sessions.Add(new ReplaySession
            {
                Id = acc.Id,
                CreatedAt = acc.CreatedAt,
                VideoStartTime = acc.VideoStartTime,
                VideoEndTime = acc.VideoEndTime,
                TotalBytes = totalBytes,
                EventCount = events.Count(e => EventBelongsToSession(e, acc.VideoStartTime, acc.VideoEndTime)),
                Filenames = filenames,
                RelatedFiles = related
            });
        }

        return sessions;
    }

    private static void AddFile(List<string> related, ref long totalBytes, string path)
    {
        if (!File.Exists(path))
            return;

        related.Add(path);
        totalBytes += new FileInfo(path).Length;
    }

    private static bool EventBelongsToSession(XElement evt, ReplaySession session)
        => EventBelongsToSession(evt, session.VideoStartTime, session.VideoEndTime);

    private static bool EventBelongsToSession(XElement evt, long start, long end)
    {
        // Событие относится к сессии, если в её таймлайне лежит In и/или Out.
        // Разметка может начинаться в одной сессии и заканчиваться в другой.
        var inHit = false;
        var outHit = false;

        var inPointText = (string?)evt.Element("inPoint");
        if (long.TryParse(inPointText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var inPoint))
            inHit = inPoint >= start && inPoint < end;

        var outPointText = (string?)evt.Element("outPoint");
        if (long.TryParse(outPointText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var outPoint)
            && outPoint != long.MaxValue)
        {
            outHit = outPoint >= start && outPoint < end;
        }

        return inHit || outHit;
    }

    private static DateTime ParseTimestamp(string? timestampText, string sessionId)
    {
        if (!string.IsNullOrWhiteSpace(timestampText) &&
            DateTimeOffset.TryParse(timestampText, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto))
        {
            return dto.LocalDateTime;
        }

        var parts = sessionId.Split('-');
        if (parts.Length >= 3 &&
            DateTime.TryParseExact(
                parts[1] + parts[2],
                "yyyyMMddHHmmss",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeLocal,
                out var parsed))
        {
            return parsed;
        }

        return DateTime.MinValue;
    }

    private sealed class SessionAccumulator(string id, DateTime createdAt, long videoStartTime)
    {
        public string Id { get; } = id;
        public DateTime CreatedAt { get; } = createdAt;
        public long VideoStartTime { get; set; } = videoStartTime;
        public long VideoEndTime { get; set; } = long.MaxValue;
        public HashSet<string> Filenames { get; } = new(StringComparer.OrdinalIgnoreCase);
    }
}

public sealed record DeleteResult(
    int SessionsDeleted,
    int FilesDeleted,
    int SegmentsRemoved,
    int EventsRemoved,
    IReadOnlyList<string> Errors,
    long FreedBytes = 0,
    string? BackupPath = null);

public sealed record BackupInfo(string Path, DateTime CreatedAt, long SizeBytes);

public sealed record RestoreResult(
    string RestoredFrom,
    string SafetyCopyPath,
    int MissingMediaFiles);
