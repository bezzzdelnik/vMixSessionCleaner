# vMix Session Cleaner

A small Windows utility for safely deleting selected **vMix Instant Replay** recording sessions and reclaiming disk space.

vMix Instant Replay can create many session segments, but it does not provide a convenient way to remove specific older sessions while keeping the rest of the project intact. This tool fills that gap.

## Features

- Browse an Instant Replay project folder (`replay2.xml`)
- List sessions sorted by creation date
- Multi-select sessions with `Ctrl` / `Shift`
- Show session size, camera count, related In/Out markers, and file count
- Delete selected sessions without breaking remaining sessions or project settings
- Remove related In/Out markers (including markers that span two sessions)
- Create an automatic XML backup before every deletion
- Restore `replay2.xml` from a previous cleaner backup
- Multilingual UI: **English**, **Russian**, **Simplified Chinese**
- Portable self-contained build — no .NET, Node.js, Python, or VS Redistributable required for end users

## What gets deleted

For each selected session (`stream-YYYYMMDD-HHMMSS`):

| Location | Files |
|---|---|
| Project root | `stream-...-N.avi` for all cameras |
| `data\` | matching `.adata` / `.vdata` sidecar files |

In `replay2.xml`:

- matching `<segment>` entries under all `<stream>` nodes
- `<event>` markers whose **In** and/or **Out** fall into the session timeline

Not deleted: other sessions, music, logs, tags, and general replay settings.

## Backup & restore

Before each deletion the app creates:

```text
replay2.xml.cleaner-backup-YYYYMMDD-HHMMSS
```

**Restore XML...** can roll `replay2.xml` back to one of those backups.

Important:

- Restore recovers **XML only** (session references and markers)
- Deleted AVI / `data\` media files are **not** restored
- Before restore, the current XML is saved as `replay2.xml.pre-restore-...`

## Requirements

### End users

- Windows x64
- No additional runtimes needed when using the published single-file build

### Building from source

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- Windows

## Build / publish

```powershell
powershell -ExecutionPolicy Bypass -File .\publish.ps1
```

Output:

```text
dist\vMixSessionCleaner.exe
```

Or manually:

```powershell
dotnet publish .\vMixSessionCleaner.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:EnableCompressionInSingleFile=true `
  -o .\dist
```

## Usage

1. Close vMix (or at least Instant Replay) before deleting sessions
2. Run `vMixSessionCleaner.exe`
3. Select the Instant Replay folder that contains `replay2.xml`
4. Review sessions, select the ones to remove
5. Click **Delete selected**
6. Optionally use **Restore XML...** if you need to roll back the project XML

The last selected folder path and UI language are saved under:

```text
%LocalAppData%\vMixSessionCleaner\settings.json
```

## Safety notes

- Prefer closing vMix before deletion to avoid locked media files
- Always keep/verify cleaner XML backups if you may need to undo project metadata changes
- Restoring XML after media deletion will bring sessions back into the project list, but missing AVI files will still be absent on disk

## License

Use and modify freely for your production workflow. If you publish changes, credit is appreciated.
