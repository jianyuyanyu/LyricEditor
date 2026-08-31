using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LyricEditor.Utils;

/// <summary>
/// 应用配置，序列化为 <c>%AppData%\LyricEditor\settings.json</c>，
/// 而非写在程序运行目录，避免安装在受保护目录（如 Program Files）时无法保存。
/// </summary>
public sealed class AppSettings
{
    /// <summary>退出时自动缓存</summary>
    public bool AutoSaveTemp { get; set; } = true;

    /// <summary>导出为 UTF-8 编码</summary>
    public bool ExportUTF8 { get; set; } = true;

    /// <summary>时间取近似值</summary>
    public bool ApproxTime { get; set; } = false;

    /// <summary>界面字体</summary>
    public string UIFont { get; set; } = "Microsoft YaHei UI";

    /// <summary>歌词字体</summary>
    public string LyricFont { get; set; } = "Microsoft YaHei UI";

    /// <summary>时间偏差（毫秒）</summary>
    public double TimeOffset { get; set; } = 150;

    /// <summary>短快进快退（秒）</summary>
    public double ShortTimeShift { get; set; } = 2;

    /// <summary>长快进快退（秒）</summary>
    public double LongTimeShift { get; set; } = 5;

    [JsonIgnore]
    private static string SettingsPath
    {
        get
        {
            string dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "LyricEditor"
            );
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, "settings.json");
        }
    }

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
    };

    /// <summary>
    /// 从配置文件读取；文件不存在或损坏时返回默认设置。
    /// </summary>
    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                string json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch
        {
            // 配置损坏时回退到默认值，避免影响启动
        }
        return new AppSettings();
    }

    /// <summary>
    /// 将当前设置写入配置文件。
    /// </summary>
    public void Save()
    {
        try
        {
            string json = JsonSerializer.Serialize(this, SerializerOptions);
            File.WriteAllText(SettingsPath, json);
        }
        catch
        {
            // 保存失败不应阻断退出流程
        }
    }
}
