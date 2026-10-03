namespace vMixSessionCleaner.Localization;

public static class Loc
{
    public static AppLanguage Current { get; private set; } = AppLanguage.English;

    public static event Action? LanguageChanged;

    public static void SetLanguage(AppLanguage language)
    {
        if (Current == language)
            return;

        Current = language;
        LanguageChanged?.Invoke();
    }

    public static AppLanguage DetectFromSystem()
    {
        var name = Thread.CurrentThread.CurrentUICulture.Name;
        if (name.StartsWith("ru", StringComparison.OrdinalIgnoreCase))
            return AppLanguage.Russian;
        if (name.StartsWith("zh", StringComparison.OrdinalIgnoreCase))
            return AppLanguage.ChineseSimplified;
        return AppLanguage.English;
    }

    public static string T(string key)
    {
        if (Tables[Current].TryGetValue(key, out var value))
            return value;
        if (Tables[AppLanguage.English].TryGetValue(key, out var fallback))
            return fallback;
        return key;
    }

    public static string F(string key, params object[] args) => string.Format(T(key), args);

    public static IReadOnlyList<(AppLanguage Language, string Title)> LanguageOptions { get; } =
    [
        (AppLanguage.English, "English"),
        (AppLanguage.Russian, "Русский"),
        (AppLanguage.ChineseSimplified, "简体中文")
    ];

    private static readonly Dictionary<AppLanguage, Dictionary<string, string>> Tables = new()
    {
        [AppLanguage.English] = En(),
        [AppLanguage.Russian] = Ru(),
        [AppLanguage.ChineseSimplified] = Zh()
    };

    private static Dictionary<string, string> En() => new()
    {
        ["AppTitle"] = "vMix Session Cleaner",
        ["PathLabel"] = "Instant Replay folder:",
        ["Browse"] = "Browse...",
        ["Refresh"] = "Refresh",
        ["Language"] = "Language",
        ["SelectAll"] = "Select all",
        ["SelectNone"] = "Clear selection",
        ["RestoreXml"] = "Restore XML...",
        ["DeleteSelected"] = "Delete selected",
        ["ColCreated"] = "Created",
        ["ColSession"] = "Session",
        ["ColSize"] = "Size",
        ["ColCameras"] = "Cameras",
        ["ColEvents"] = "Markers (In/Out)",
        ["ColFiles"] = "Files",
        ["StatusChooseFolder"] = "Select an Instant Replay project folder.",
        ["StatusSelected"] = "Selected: {0} | {1} | markers: {2}",
        ["StatusSummary"] = "Sessions: {0} | Markers In/Out: {1} | Used: {2}",
        ["StatusAfterDelete"] = "Sessions: {0} | Used: {1} | Freed: {2}",
        ["StatusAfterRestore"] = "Sessions: {0} | Used: {1} | XML restored",
        ["StatusLoadError"] = "Load error.",
        ["WarningDefault"] = "Close vMix (or at least Instant Replay) before deleting. Otherwise files may be locked. A backup XML is created before changes.",
        ["WarningVmixRunning"] = "vMix is running. Close it before deleting, otherwise files may be locked and deletion may be partial.",
        ["WarningSafe"] = "A backup XML is created before changes. Session files and markers whose In and/or Out fall into this session are deleted.",
        ["BrowseDescription"] = "Select Instant Replay folder (contains replay2.xml)",
        ["PathEmpty"] = "Please specify the Instant Replay folder path.",
        ["PathEmptyTitle"] = "Path not set",
        ["OpenFailedTitle"] = "Failed to open project",
        ["VmixRunningBody"] = "vMix is currently running. Deletion may fail due to file locks.\n\nContinue anyway?",
        ["VmixRunningTitle"] = "vMix is running",
        ["DeleteConfirmBody"] = "Delete sessions: {0}\nApproximate space to free: {1}\nMarkers In/Out to remove: {2}\n\nOther sessions and project settings will be kept.\nContinue?",
        ["DeleteConfirmTitle"] = "Confirm deletion",
        ["DeleteResultBody"] = "Sessions deleted: {0}\nFiles deleted: {1}\nXML segments removed: {2}\nMarkers In/Out removed: {3}\nFreed: {4}\nBackup XML: {5}",
        ["DeleteErrors"] = "\n\nErrors:\n{0}",
        ["DeleteDoneWithErrors"] = "Completed with errors",
        ["Done"] = "Done",
        ["DeleteErrorTitle"] = "Delete error",
        ["ProjectNotLoaded"] = "Open an Instant Replay folder first.",
        ["ProjectNotLoadedTitle"] = "Project not loaded",
        ["NoBackupsBody"] = "No backups named replay2.xml.cleaner-backup-... were found in the project folder.\nThey are created automatically before each deletion.",
        ["NoBackupsTitle"] = "No backups found",
        ["RestoreConfirmBody"] = "Only replay2.xml will be restored from the selected backup.\nAVI and data\\ files are not restored.\n\nIf media was already deleted, sessions will reappear in XML but files will be missing on disk.\n\nContinue?",
        ["RestoreConfirmTitle"] = "Confirm restore",
        ["RestoreResultBody"] = "XML restored from:\n{0}\n\nCurrent XML saved as:\n{1}\n\nMissing AVI files on disk: {2}",
        ["RestoreWarnTitle"] = "Restored with warning",
        ["RestoreOkTitle"] = "Restored",
        ["RestoreErrorTitle"] = "Restore error",
        ["RestoreFormTitle"] = "Restore from backup",
        ["RestoreFormHint"] = "Only replay2.xml is restored (segment links and In/Out markers).\nDeleted AVI and data\\ files are not restored — disk space was already freed.\nBefore rollback, the current XML is saved as replay2.xml.pre-restore-...",
        ["Restore"] = "Restore",
        ["Cancel"] = "Cancel",
        ["ErrFolderNotFound"] = "Folder not found: {0}",
        ["ErrNoReplayXml"] = "No replay2.xml in this folder — not an Instant Replay project.",
        ["ErrProjectNotLoaded"] = "Project is not loaded.",
        ["ErrBadXml"] = "Invalid replay2.xml",
        ["ErrBackupNotFound"] = "Backup file not found.",
        ["ErrCurrentXmlNotFound"] = "Current replay2.xml not found."
    };

    private static Dictionary<string, string> Ru() => new()
    {
        ["AppTitle"] = "vMix Session Cleaner",
        ["PathLabel"] = "Папка Instant Replay:",
        ["Browse"] = "Обзор...",
        ["Refresh"] = "Обновить",
        ["Language"] = "Язык",
        ["SelectAll"] = "Выбрать все",
        ["SelectNone"] = "Снять выбор",
        ["RestoreXml"] = "Восстановить XML...",
        ["DeleteSelected"] = "Удалить выбранные",
        ["ColCreated"] = "Дата создания",
        ["ColSession"] = "Сессия",
        ["ColSize"] = "Размер",
        ["ColCameras"] = "Камеры",
        ["ColEvents"] = "Разметки (In/Out)",
        ["ColFiles"] = "Файлы",
        ["StatusChooseFolder"] = "Укажите папку проекта Instant Replay.",
        ["StatusSelected"] = "Выбрано: {0} | {1} | событий: {2}",
        ["StatusSummary"] = "Сессий: {0} | Событий In/Out: {1} | Занято: {2}",
        ["StatusAfterDelete"] = "Сессий: {0} | Занято: {1} | Освобождено: {2}",
        ["StatusAfterRestore"] = "Сессий: {0} | Занято: {1} | XML восстановлен",
        ["StatusLoadError"] = "Ошибка загрузки.",
        ["WarningDefault"] = "Перед удалением закройте vMix (или хотя бы Instant Replay). Иначе файлы могут быть заблокированы. Перед правкой XML создаётся резервная копия.",
        ["WarningVmixRunning"] = "Обнаружен запущенный vMix. Закройте его перед удалением, иначе файлы могут быть заняты и удаление пройдёт частично.",
        ["WarningSafe"] = "Перед правкой XML создаётся резервная копия. Удаляются файлы сессии и разметки, у которых In и/или Out попадают в эту сессию.",
        ["BrowseDescription"] = "Выберите папку Instant Replay (где лежит replay2.xml)",
        ["PathEmpty"] = "Укажите путь к папке Instant Replay.",
        ["PathEmptyTitle"] = "Путь не задан",
        ["OpenFailedTitle"] = "Не удалось открыть проект",
        ["VmixRunningBody"] = "vMix сейчас запущен. Удаление может завершиться с ошибками блокировки файлов.\n\nВсё равно продолжить?",
        ["VmixRunningTitle"] = "vMix запущен",
        ["DeleteConfirmBody"] = "Удалить сессий: {0}\nОсвободится примерно: {1}\nБудет удалено событий In/Out: {2}\n\nОстальные сессии и настройки проекта сохранятся.\nПродолжить?",
        ["DeleteConfirmTitle"] = "Подтверждение удаления",
        ["DeleteResultBody"] = "Удалено сессий: {0}\nУдалено файлов: {1}\nУдалено сегментов из XML: {2}\nУдалено событий In/Out: {3}\nОсвобождено: {4}\nBackup XML: {5}",
        ["DeleteErrors"] = "\n\nОшибки:\n{0}",
        ["DeleteDoneWithErrors"] = "Удаление завершено с ошибками",
        ["Done"] = "Готово",
        ["DeleteErrorTitle"] = "Ошибка удаления",
        ["ProjectNotLoaded"] = "Сначала откройте папку Instant Replay.",
        ["ProjectNotLoadedTitle"] = "Проект не загружен",
        ["NoBackupsBody"] = "В папке проекта нет бэкапов вида replay2.xml.cleaner-backup-...\nОни создаются автоматически перед каждым удалением.",
        ["NoBackupsTitle"] = "Бэкапы не найдены",
        ["RestoreConfirmBody"] = "Будет восстановлен только replay2.xml из выбранного бэкапа.\nAVI и файлы data\\ не восстанавливаются.\n\nЕсли медиа уже удалены, сессии снова появятся в XML, но файлов на диске не будет.\n\nПродолжить?",
        ["RestoreConfirmTitle"] = "Подтверждение восстановления",
        ["RestoreResultBody"] = "XML восстановлен из:\n{0}\n\nТекущий XML сохранён как:\n{1}\n\nAVI, отсутствующих на диске: {2}",
        ["RestoreWarnTitle"] = "Восстановлено с предупреждением",
        ["RestoreOkTitle"] = "Восстановлено",
        ["RestoreErrorTitle"] = "Ошибка восстановления",
        ["RestoreFormTitle"] = "Восстановление из бэкапа",
        ["RestoreFormHint"] = "Восстанавливается только replay2.xml (ссылки на сегменты и разметки In/Out).\nУдалённые AVI и файлы из data\\ из бэкапа не возвращаются — место на диске уже освобождено.\nПеред откатом текущий XML будет сохранён как replay2.xml.pre-restore-...",
        ["Restore"] = "Восстановить",
        ["Cancel"] = "Отмена",
        ["ErrFolderNotFound"] = "Папка не найдена: {0}",
        ["ErrNoReplayXml"] = "В папке нет replay2.xml — это не проект Instant Replay.",
        ["ErrProjectNotLoaded"] = "Проект не загружен.",
        ["ErrBadXml"] = "Некорректный replay2.xml",
        ["ErrBackupNotFound"] = "Файл бэкапа не найден.",
        ["ErrCurrentXmlNotFound"] = "Текущий replay2.xml не найден."
    };

    private static Dictionary<string, string> Zh() => new()
    {
        ["AppTitle"] = "vMix Session Cleaner",
        ["PathLabel"] = "Instant Replay 文件夹：",
        ["Browse"] = "浏览...",
        ["Refresh"] = "刷新",
        ["Language"] = "语言",
        ["SelectAll"] = "全选",
        ["SelectNone"] = "取消选择",
        ["RestoreXml"] = "恢复 XML...",
        ["DeleteSelected"] = "删除所选",
        ["ColCreated"] = "创建时间",
        ["ColSession"] = "会话",
        ["ColSize"] = "大小",
        ["ColCameras"] = "机位",
        ["ColEvents"] = "标记 (In/Out)",
        ["ColFiles"] = "文件",
        ["StatusChooseFolder"] = "请选择 Instant Replay 项目文件夹。",
        ["StatusSelected"] = "已选：{0} | {1} | 标记：{2}",
        ["StatusSummary"] = "会话：{0} | In/Out 标记：{1} | 占用：{2}",
        ["StatusAfterDelete"] = "会话：{0} | 占用：{1} | 已释放：{2}",
        ["StatusAfterRestore"] = "会话：{0} | 占用：{1} | XML 已恢复",
        ["StatusLoadError"] = "加载失败。",
        ["WarningDefault"] = "删除前请关闭 vMix（或至少关闭 Instant Replay），否则文件可能被占用。修改 XML 前会创建备份。",
        ["WarningVmixRunning"] = "检测到 vMix 正在运行。删除前请关闭，否则文件可能被占用，删除可能不完整。",
        ["WarningSafe"] = "修改 XML 前会创建备份。将删除会话文件，以及 In 和/或 Out 落在该会话时间范围内的标记。",
        ["BrowseDescription"] = "选择 Instant Replay 文件夹（包含 replay2.xml）",
        ["PathEmpty"] = "请指定 Instant Replay 文件夹路径。",
        ["PathEmptyTitle"] = "未设置路径",
        ["OpenFailedTitle"] = "无法打开项目",
        ["VmixRunningBody"] = "vMix 正在运行。删除可能因文件锁定而失败。\n\n仍要继续吗？",
        ["VmixRunningTitle"] = "vMix 正在运行",
        ["DeleteConfirmBody"] = "将删除会话：{0}\n预计释放空间：{1}\n将删除 In/Out 标记：{2}\n\n其他会话和项目设置将保留。\n继续？",
        ["DeleteConfirmTitle"] = "确认删除",
        ["DeleteResultBody"] = "已删除会话：{0}\n已删除文件：{1}\n已删除 XML 片段：{2}\n已删除 In/Out 标记：{3}\n已释放：{4}\nBackup XML：{5}",
        ["DeleteErrors"] = "\n\n错误：\n{0}",
        ["DeleteDoneWithErrors"] = "完成但有错误",
        ["Done"] = "完成",
        ["DeleteErrorTitle"] = "删除错误",
        ["ProjectNotLoaded"] = "请先打开 Instant Replay 文件夹。",
        ["ProjectNotLoadedTitle"] = "项目未加载",
        ["NoBackupsBody"] = "项目文件夹中没有 replay2.xml.cleaner-backup-... 备份。\n每次删除前会自动创建这些备份。",
        ["NoBackupsTitle"] = "未找到备份",
        ["RestoreConfirmBody"] = "只会从所选备份恢复 replay2.xml。\n不会恢复 AVI 和 data\\ 文件。\n\n如果媒体已被删除，会话会重新出现在 XML 中，但磁盘上没有文件。\n\n继续？",
        ["RestoreConfirmTitle"] = "确认恢复",
        ["RestoreResultBody"] = "XML 已从以下文件恢复：\n{0}\n\n当前 XML 已保存为：\n{1}\n\n磁盘上缺失的 AVI：{2}",
        ["RestoreWarnTitle"] = "已恢复（有警告）",
        ["RestoreOkTitle"] = "已恢复",
        ["RestoreErrorTitle"] = "恢复错误",
        ["RestoreFormTitle"] = "从备份恢复",
        ["RestoreFormHint"] = "仅恢复 replay2.xml（片段引用和 In/Out 标记）。\n已删除的 AVI 和 data\\ 文件不会恢复——磁盘空间已释放。\n回滚前会将当前 XML 保存为 replay2.xml.pre-restore-...",
        ["Restore"] = "恢复",
        ["Cancel"] = "取消",
        ["ErrFolderNotFound"] = "未找到文件夹：{0}",
        ["ErrNoReplayXml"] = "该文件夹没有 replay2.xml — 不是 Instant Replay 项目。",
        ["ErrProjectNotLoaded"] = "项目未加载。",
        ["ErrBadXml"] = "无效的 replay2.xml",
        ["ErrBackupNotFound"] = "未找到备份文件。",
        ["ErrCurrentXmlNotFound"] = "未找到当前的 replay2.xml。"
    };
}
