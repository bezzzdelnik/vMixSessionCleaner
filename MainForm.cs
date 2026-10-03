using vMixSessionCleaner.Localization;
using vMixSessionCleaner.Models;
using vMixSessionCleaner.Services;

namespace vMixSessionCleaner;

public sealed class MainForm : Form
{
    private readonly ReplayProjectService _service = new();
    private readonly AppSettings _settings = AppSettings.Load();

    private readonly Label _pathLabel = new();
    private readonly TextBox _pathBox = new();
    private readonly Button _browseButton = new();
    private readonly Button _refreshButton = new();
    private readonly Label _languageLabel = new();
    private readonly ComboBox _languageBox = new();
    private readonly Button _deleteButton = new();
    private readonly Button _restoreButton = new();
    private readonly Button _selectAllButton = new();
    private readonly Button _selectNoneButton = new();
    private readonly ListView _list = new();
    private readonly StatusStrip _status = new();
    private readonly ToolStripStatusLabel _statusLabel = new();
    private readonly ToolStripStatusLabel _selectionLabel = new();
    private readonly Label _warningLabel = new();
    private bool _suppressLanguageEvent;

    public MainForm()
    {
        Width = 1000;
        Height = 640;
        MinimumSize = new Size(860, 480);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9.75f);

        var language = _settings.Language ?? Loc.DetectFromSystem();
        Loc.SetLanguage(language);

        BuildUi();
        ApplyLanguage();
        _list.SelectedIndexChanged += (_, _) => UpdateSelectionStatus();
        Shown += (_, _) => TryLoadSavedPath();
        Loc.LanguageChanged += ApplyLanguage;
        FormClosed += (_, _) => Loc.LanguageChanged -= ApplyLanguage;
    }

    private void BuildUi()
    {
        _pathLabel.AutoSize = true;
        _pathLabel.Left = 16;
        _pathLabel.Top = 18;

        _languageLabel.AutoSize = true;
        _languageLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _languageLabel.Top = 18;

        _languageBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _languageBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _languageBox.Width = 120;
        _languageBox.Top = 14;
        _languageBox.Left = ClientSize.Width - 136;
        foreach (var option in Loc.LanguageOptions)
            _languageBox.Items.Add(option.Title);
        _languageBox.SelectedIndexChanged += LanguageBoxOnSelectedIndexChanged;

        _pathBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _pathBox.Left = 16;
        _pathBox.Top = 42;
        _pathBox.Width = ClientSize.Width - 220;
        _pathBox.PlaceholderText = @"C:\";
        _pathBox.Text = string.IsNullOrWhiteSpace(_settings.ProjectPath) ? @"C:\" : _settings.ProjectPath;

        _browseButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _browseButton.Width = 90;
        _browseButton.Left = ClientSize.Width - 198;
        _browseButton.Top = 40;
        _browseButton.Click += (_, _) => BrowseFolder();

        _refreshButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _refreshButton.Width = 90;
        _refreshButton.Left = ClientSize.Width - 100;
        _refreshButton.Top = 40;
        _refreshButton.Click += (_, _) => Reload();

        _warningLabel.AutoSize = false;
        _warningLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _warningLabel.Left = 16;
        _warningLabel.Top = 78;
        _warningLabel.Width = ClientSize.Width - 32;
        _warningLabel.Height = 42;
        _warningLabel.ForeColor = Color.DarkRed;

        _list.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        _list.Left = 16;
        _list.Top = 128;
        _list.Width = ClientSize.Width - 32;
        _list.Height = ClientSize.Height - 220;
        _list.View = View.Details;
        _list.FullRowSelect = true;
        _list.MultiSelect = true;
        _list.HideSelection = false;
        _list.GridLines = true;
        for (var i = 0; i < 6; i++)
            _list.Columns.Add("", 80);

        _selectAllButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        _selectAllButton.Width = 110;
        _selectAllButton.Left = 16;
        _selectAllButton.Top = ClientSize.Height - 78;
        _selectAllButton.Click += (_, _) =>
        {
            foreach (ListViewItem item in _list.Items)
                item.Selected = true;
        };

        _selectNoneButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        _selectNoneButton.Width = 110;
        _selectNoneButton.Left = 132;
        _selectNoneButton.Top = ClientSize.Height - 78;
        _selectNoneButton.Click += (_, _) => _list.SelectedItems.Clear();

        _restoreButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        _restoreButton.Width = 160;
        _restoreButton.Height = 34;
        _restoreButton.Left = ClientSize.Width - 376;
        _restoreButton.Top = ClientSize.Height - 82;
        _restoreButton.Click += (_, _) => RestoreBackup();

        _deleteButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        _deleteButton.Width = 180;
        _deleteButton.Height = 34;
        _deleteButton.Left = ClientSize.Width - 196;
        _deleteButton.Top = ClientSize.Height - 82;
        _deleteButton.BackColor = Color.FromArgb(192, 57, 43);
        _deleteButton.ForeColor = Color.White;
        _deleteButton.FlatStyle = FlatStyle.Flat;
        _deleteButton.Click += (_, _) => DeleteSelected();

        _statusLabel.Spring = true;
        _statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        _status.Items.Add(_statusLabel);
        _status.Items.Add(_selectionLabel);

        Controls.Add(_pathLabel);
        Controls.Add(_languageLabel);
        Controls.Add(_languageBox);
        Controls.Add(_pathBox);
        Controls.Add(_browseButton);
        Controls.Add(_refreshButton);
        Controls.Add(_warningLabel);
        Controls.Add(_list);
        Controls.Add(_selectAllButton);
        Controls.Add(_selectNoneButton);
        Controls.Add(_restoreButton);
        Controls.Add(_deleteButton);
        Controls.Add(_status);

        Resize += (_, _) =>
        {
            _languageBox.Left = ClientSize.Width - 136;
            _languageLabel.Left = _languageBox.Left - _languageLabel.PreferredWidth - 8;
        };
    }

    private void LanguageBoxOnSelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_suppressLanguageEvent || _languageBox.SelectedIndex < 0)
            return;

        var language = Loc.LanguageOptions[_languageBox.SelectedIndex].Language;
        Loc.SetLanguage(language);
        _settings.SaveLanguage(language);
    }

    private void ApplyLanguage()
    {
        if (InvokeRequired)
        {
            BeginInvoke(ApplyLanguage);
            return;
        }

        Text = Loc.T("AppTitle");
        _pathLabel.Text = Loc.T("PathLabel");
        _languageLabel.Text = Loc.T("Language");
        _browseButton.Text = Loc.T("Browse");
        _refreshButton.Text = Loc.T("Refresh");
        _selectAllButton.Text = Loc.T("SelectAll");
        _selectNoneButton.Text = Loc.T("SelectNone");
        _restoreButton.Text = Loc.T("RestoreXml");
        _deleteButton.Text = Loc.T("DeleteSelected");

        _list.Columns[0].Text = Loc.T("ColCreated");
        _list.Columns[0].Width = 160;
        _list.Columns[1].Text = Loc.T("ColSession");
        _list.Columns[1].Width = 220;
        _list.Columns[2].Text = Loc.T("ColSize");
        _list.Columns[2].Width = 100;
        _list.Columns[3].Text = Loc.T("ColCameras");
        _list.Columns[3].Width = 70;
        _list.Columns[4].Text = Loc.T("ColEvents");
        _list.Columns[4].Width = 130;
        _list.Columns[5].Text = Loc.T("ColFiles");
        _list.Columns[5].Width = 70;

        _suppressLanguageEvent = true;
        var idx = Loc.LanguageOptions.ToList().FindIndex(x => x.Language == Loc.Current);
        _languageBox.SelectedIndex = idx >= 0 ? idx : 0;
        _suppressLanguageEvent = false;

        _languageLabel.Left = _languageBox.Left - _languageLabel.PreferredWidth - 8;

        if (_service.Sessions.Count > 0)
        {
            var total = _service.Sessions.Sum(s => s.TotalBytes);
            var events = _service.Sessions.Sum(s => s.EventCount);
            _statusLabel.Text = Loc.F("StatusSummary", _service.Sessions.Count, events, ReplaySession.FormatBytes(total));
            UpdateWarningText();
        }
        else
        {
            _statusLabel.Text = Loc.T("StatusChooseFolder");
            _warningLabel.ForeColor = Color.DarkRed;
            _warningLabel.Text = Loc.T("WarningDefault");
        }

        UpdateSelectionStatus();
    }

    private void UpdateWarningText()
    {
        if (ReplayProjectService.IsVmixRunning())
        {
            _warningLabel.ForeColor = Color.DarkRed;
            _warningLabel.Text = Loc.T("WarningVmixRunning");
        }
        else
        {
            _warningLabel.ForeColor = Color.FromArgb(60, 60, 60);
            _warningLabel.Text = Loc.T("WarningSafe");
        }
    }

    private void TryLoadSavedPath()
    {
        var path = _pathBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(path))
            path = @"C:\";

        _pathBox.Text = path;

        if (Directory.Exists(path) && File.Exists(Path.Combine(path, "replay2.xml")))
            Reload();
    }

    private void BrowseFolder()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = Loc.T("BrowseDescription"),
            UseDescriptionForTitle = true,
            SelectedPath = Directory.Exists(_pathBox.Text) ? _pathBox.Text : @"C:\"
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _pathBox.Text = dialog.SelectedPath;
            Reload();
        }
    }

    private void Reload()
    {
        try
        {
            var path = _pathBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(path))
            {
                MessageBox.Show(this, Loc.T("PathEmpty"), Loc.T("PathEmptyTitle"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _service.Load(path);
            _settings.SaveProjectPath(path);
            PopulateList(_service.Sessions);

            var total = _service.Sessions.Sum(s => s.TotalBytes);
            var events = _service.Sessions.Sum(s => s.EventCount);
            _statusLabel.Text = Loc.F("StatusSummary", _service.Sessions.Count, events, ReplaySession.FormatBytes(total));
            UpdateSelectionStatus();
            UpdateWarningText();
        }
        catch (Exception ex)
        {
            _list.Items.Clear();
            _statusLabel.Text = Loc.T("StatusLoadError");
            MessageBox.Show(this, ex.Message, Loc.T("OpenFailedTitle"),
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void PopulateList(IReadOnlyList<ReplaySession> sessions)
    {
        _list.BeginUpdate();
        _list.Items.Clear();

        foreach (var session in sessions)
        {
            var item = new ListViewItem(session.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"))
            {
                Tag = session.Id
            };
            item.SubItems.Add(session.Id);
            item.SubItems.Add(session.SizeText);
            item.SubItems.Add(session.AngleCount.ToString());
            item.SubItems.Add(session.EventCount.ToString());
            item.SubItems.Add(session.RelatedFiles.Count.ToString());
            _list.Items.Add(item);
        }

        _list.EndUpdate();
    }

    private void UpdateSelectionStatus()
    {
        long selectedBytes = 0;
        var selectedEvents = 0;

        foreach (ListViewItem item in _list.SelectedItems)
        {
            var id = item.Tag as string;
            var session = _service.Sessions.FirstOrDefault(s => s.Id == id);
            if (session is null)
                continue;

            selectedBytes += session.TotalBytes;
            selectedEvents += session.EventCount;
        }

        _selectionLabel.Text = Loc.F(
            "StatusSelected",
            _list.SelectedItems.Count,
            ReplaySession.FormatBytes(selectedBytes),
            selectedEvents);
        _deleteButton.Enabled = _list.SelectedItems.Count > 0;
    }

    private void DeleteSelected()
    {
        if (_list.SelectedItems.Count == 0)
            return;

        var ids = _list.SelectedItems
            .Cast<ListViewItem>()
            .Select(i => i.Tag as string)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Cast<string>()
            .ToList();

        var selected = _service.Sessions.Where(s => ids.Contains(s.Id)).ToList();
        var totalBytes = selected.Sum(s => s.TotalBytes);
        var totalEvents = selected.Sum(s => s.EventCount);

        if (ReplayProjectService.IsVmixRunning())
        {
            var proceed = MessageBox.Show(this,
                Loc.T("VmixRunningBody"),
                Loc.T("VmixRunningTitle"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (proceed != DialogResult.Yes)
                return;
        }

        var confirm = MessageBox.Show(this,
            Loc.F("DeleteConfirmBody", selected.Count, ReplaySession.FormatBytes(totalBytes), totalEvents),
            Loc.T("DeleteConfirmTitle"),
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);

        if (confirm != DialogResult.Yes)
            return;

        try
        {
            UseWaitCursor = true;
            var result = _service.DeleteSessions(ids);
            PopulateList(_service.Sessions);
            UpdateSelectionStatus();

            var total = _service.Sessions.Sum(s => s.TotalBytes);
            _statusLabel.Text = Loc.F(
                "StatusAfterDelete",
                _service.Sessions.Count,
                ReplaySession.FormatBytes(total),
                ReplaySession.FormatBytes(result.FreedBytes));

            var message = Loc.F(
                "DeleteResultBody",
                result.SessionsDeleted,
                result.FilesDeleted,
                result.SegmentsRemoved,
                result.EventsRemoved,
                ReplaySession.FormatBytes(result.FreedBytes),
                Path.GetFileName(result.BackupPath ?? string.Empty));

            if (result.Errors.Count > 0)
            {
                message += Loc.F("DeleteErrors", string.Join("\n", result.Errors.Take(10)));
                MessageBox.Show(this, message, Loc.T("DeleteDoneWithErrors"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                MessageBox.Show(this, message, Loc.T("Done"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, Loc.T("DeleteErrorTitle"),
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            Reload();
        }
        finally
        {
            UseWaitCursor = false;
        }
    }

    private void RestoreBackup()
    {
        if (string.IsNullOrWhiteSpace(_service.ProjectPath))
        {
            MessageBox.Show(this, Loc.T("ProjectNotLoaded"), Loc.T("ProjectNotLoadedTitle"),
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var backups = _service.ListBackups();
        if (backups.Count == 0)
        {
            MessageBox.Show(this, Loc.T("NoBackupsBody"), Loc.T("NoBackupsTitle"),
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = new RestoreBackupForm(backups);
        if (dialog.ShowDialog(this) != DialogResult.OK || dialog.SelectedBackup is null)
            return;

        var confirm = MessageBox.Show(this,
            Loc.T("RestoreConfirmBody"),
            Loc.T("RestoreConfirmTitle"),
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);

        if (confirm != DialogResult.Yes)
            return;

        try
        {
            UseWaitCursor = true;
            var result = _service.RestoreBackup(dialog.SelectedBackup.Path);
            PopulateList(_service.Sessions);
            UpdateSelectionStatus();

            var total = _service.Sessions.Sum(s => s.TotalBytes);
            _statusLabel.Text = Loc.F(
                "StatusAfterRestore",
                _service.Sessions.Count,
                ReplaySession.FormatBytes(total));

            var message = Loc.F(
                "RestoreResultBody",
                Path.GetFileName(result.RestoredFrom),
                Path.GetFileName(result.SafetyCopyPath),
                result.MissingMediaFiles);

            MessageBox.Show(this, message,
                result.MissingMediaFiles > 0 ? Loc.T("RestoreWarnTitle") : Loc.T("RestoreOkTitle"),
                MessageBoxButtons.OK,
                result.MissingMediaFiles > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, Loc.T("RestoreErrorTitle"),
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            UseWaitCursor = false;
        }
    }
}
