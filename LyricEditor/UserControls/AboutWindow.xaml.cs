#nullable enable

using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using LyricEditor.Utils;
using Velopack;

namespace LyricEditor.UserControls;

public partial class AboutWindow : Window
{
    private enum UpdateStage
    {
        Initial,
        Checking,
        UpToDate,
        Available,
        Downloading,
        ReadyToRestart,
    }

    private UpdateStage _stage = UpdateStage.Initial;
    private UpdateInfo? _pendingUpdate;

    public AboutWindow() : this(null)
    {
    }

    /// <param name="knownUpdate">如果调用方已经检查过更新（例如启动时的静默检查），可以直接传入，避免重复检查。</param>
    public AboutWindow(UpdateInfo? knownUpdate)
    {
        InitializeComponent();

        InfoText.Text = string.Format(Properties.Resources.Info, AppVersion.DisplayVersion);
        VersionText.Text = $"版本 {AppVersion.DisplayVersion}";

        if (!UpdateHelper.IsInstalled)
        {
            UpdateStatusText.Text = "当前非安装版本，无法检查更新";
            UpdateButton.IsEnabled = false;
            return;
        }

        if (knownUpdate != null)
        {
            _pendingUpdate = knownUpdate;
            SetStage(UpdateStage.Available);
        }
    }

    private void SetStage(UpdateStage stage)
    {
        _stage = stage;
        switch (stage)
        {
            case UpdateStage.Initial:
                UpdateButton.Content = "检查更新";
                UpdateButton.IsEnabled = true;
                UpdateStatusText.Text = string.Empty;
                break;
            case UpdateStage.Checking:
                UpdateButton.IsEnabled = false;
                UpdateStatusText.Text = "正在检查更新...";
                break;
            case UpdateStage.UpToDate:
                UpdateButton.Content = "检查更新";
                UpdateButton.IsEnabled = true;
                UpdateStatusText.Text = "已是最新版本";
                break;
            case UpdateStage.Available:
                UpdateButton.Content = $"下载更新 {_pendingUpdate!.TargetFullRelease.Version}";
                UpdateButton.IsEnabled = true;
                UpdateStatusText.Text = $"发现新版本 {_pendingUpdate.TargetFullRelease.Version}";
                break;
            case UpdateStage.Downloading:
                UpdateButton.IsEnabled = false;
                UpdateStatusText.Text = "正在下载更新...";
                break;
            case UpdateStage.ReadyToRestart:
                UpdateButton.Content = "重启并安装更新";
                UpdateButton.IsEnabled = true;
                UpdateStatusText.Text = "更新已下载完毕";
                break;
        }
    }

    private async void UpdateButton_Click(object sender, RoutedEventArgs e)
    {
        switch (_stage)
        {
            case UpdateStage.Initial:
            case UpdateStage.UpToDate:
                SetStage(UpdateStage.Checking);
                var info = await UpdateHelper.CheckForUpdatesAsync();
                if (info == null)
                    SetStage(UpdateStage.UpToDate);
                else
                {
                    _pendingUpdate = info;
                    SetStage(UpdateStage.Available);
                }
                break;

            case UpdateStage.Available:
                SetStage(UpdateStage.Downloading);
                try
                {
                    await UpdateHelper.DownloadUpdatesAsync(_pendingUpdate!);
                    SetStage(UpdateStage.ReadyToRestart);
                }
                catch
                {
                    SetStage(UpdateStage.Available);
                    UpdateStatusText.Text = "下载更新失败，请稍后重试";
                }
                break;

            case UpdateStage.ReadyToRestart:
                UpdateHelper.ApplyUpdatesAndRestart(_pendingUpdate!);
                break;
        }
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        e.Handled = true;
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
