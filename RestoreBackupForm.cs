using vMixSessionCleaner.Localization;
using vMixSessionCleaner.Services;

namespace vMixSessionCleaner;

public sealed class RestoreBackupForm : Form
{
    private readonly ListBox _list = new();
    private readonly Label _hint = new();
    private readonly Button _okButton = new();
    private readonly Button _cancelButton = new();

    public BackupInfo? SelectedBackup { get; private set; }

    public RestoreBackupForm(IReadOnlyList<BackupInfo> backups)
    {
        Width = 620;
        Height = 420;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        Font = new Font("Segoe UI", 9.75f);

        Text = Loc.T("RestoreFormTitle");

        _hint.Left = 16;
        _hint.Top = 16;
        _hint.Width = 570;
        _hint.Height = 70;
        _hint.Text = Loc.T("RestoreFormHint");

        _list.Left = 16;
        _list.Top = 96;
        _list.Width = 570;
        _list.Height = 220;
        _list.DisplayMember = nameof(BackupDisplay.Text);
        foreach (var backup in backups)
            _list.Items.Add(new BackupDisplay(backup));

        if (_list.Items.Count > 0)
            _list.SelectedIndex = 0;

        _okButton.Text = Loc.T("Restore");
        _okButton.Left = 350;
        _okButton.Top = 330;
        _okButton.Width = 120;
        _okButton.Enabled = _list.Items.Count > 0;
        _okButton.Click += (_, _) => AcceptSelection();

        _cancelButton.Text = Loc.T("Cancel");
        _cancelButton.Left = 480;
        _cancelButton.Top = 330;
        _cancelButton.Width = 100;
        _cancelButton.DialogResult = DialogResult.Cancel;

        AcceptButton = _okButton;
        CancelButton = _cancelButton;

        Controls.Add(_hint);
        Controls.Add(_list);
        Controls.Add(_okButton);
        Controls.Add(_cancelButton);
    }

    private void AcceptSelection()
    {
        if (_list.SelectedItem is not BackupDisplay selected)
            return;

        SelectedBackup = selected.Info;
        DialogResult = DialogResult.OK;
        Close();
    }

    private sealed class BackupDisplay
    {
        public BackupDisplay(BackupInfo info) => Info = info;
        public BackupInfo Info { get; }
        public string Text =>
            $"{Info.CreatedAt:yyyy-MM-dd HH:mm:ss}  |  {FormatBytes(Info.SizeBytes)}  |  {Path.GetFileName(Info.Path)}";
    }

    private static string FormatBytes(long bytes)
    {
        double value = bytes;
        string[] units = ["B", "KB", "MB", "GB"];
        var unit = 0;
        while (value >= 1024 && unit < units.Length - 1)
        {
            value /= 1024;
            unit++;
        }

        return $"{value:0.##} {units[unit]}";
    }
}
