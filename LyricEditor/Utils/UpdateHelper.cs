#nullable enable

using System;
using System.Threading.Tasks;
using Velopack;
using Velopack.Sources;

namespace LyricEditor.Utils;

/// <summary>
/// 封装基于 Velopack 的自动更新检查/下载/安装逻辑。更新源固定为本项目在 GitHub 上的 Releases。
/// </summary>
public static class UpdateHelper
{
    private const string GithubRepoUrl = "https://github.com/BYJRK/LyricEditor";

    private static readonly Lazy<UpdateManager> LazyManager = new(() =>
        new UpdateManager(new GithubSource(GithubRepoUrl, null, prerelease: true)));

    private static UpdateManager Manager => LazyManager.Value;

    /// <summary>
    /// 当前是否运行在 Velopack 安装的环境中（例如开发环境下直接 F5 运行则为 false）。
    /// </summary>
    public static bool IsInstalled
    {
        get
        {
            try
            {
                return Manager.IsInstalled;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// 检查是否有新版本。开发环境（未安装）或网络异常时返回 null，不抛出异常。
    /// </summary>
    public static async Task<UpdateInfo?> CheckForUpdatesAsync()
    {
        if (!IsInstalled)
            return null;

        try
        {
            return await Manager.CheckForUpdatesAsync();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 下载指定的更新包。
    /// </summary>
    public static Task DownloadUpdatesAsync(UpdateInfo info) => Manager.DownloadUpdatesAsync(info);

    /// <summary>
    /// 应用更新并重启应用程序。此调用不会返回。
    /// </summary>
    public static void ApplyUpdatesAndRestart(UpdateInfo info) => Manager.ApplyUpdatesAndRestart(info);
}
