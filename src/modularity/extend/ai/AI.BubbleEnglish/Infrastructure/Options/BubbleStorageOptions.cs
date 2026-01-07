namespace AI.BubbleEnglish.Infrastructure.Options;

public class BubbleStorageOptions
{
    /// <summary>根目录，例如 Linux: /data/bubble-storage  Windows: D:\\bubble-storage</summary>
    public string Root { get; set; } = "/data/bubble-storage";

    /// <summary>
    /// 公开访问前缀（用于生成 AudioUrl 等），例如 /bubble/videos
    /// 需要由你的静态文件/反向代理映射到 Root/bubble/videos
    /// </summary>
    public string PublicPrefix { get; set; } = "/bubble/videos";
}
