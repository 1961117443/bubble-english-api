namespace AI.BubbleEnglish.Infrastructure.Storage;

using AI.BubbleEnglish.Infrastructure.Options;
using Microsoft.Extensions.Options;

public interface IBubbleStorageService
{
    string GetVideoWorkDir(string videoId, DateTime? createTime = null);
    string GetVideoOriginalPath(string videoId, string extension, DateTime? createTime = null);
    string GetDerivedDir(string videoId, DateTime? createTime = null);
    string GetSubtitleDir(string videoId, DateTime? createTime = null);
    string GetUnitAudioDir(string videoId, DateTime? createTime = null);
    string ToPublicUrl(string absPath);
}

/// <summary>
/// 统一生成本地文件路径与对外URL。
/// 目录结构：{Root}/bubble/videos/{yyyy}/{MM}/{videoId}/
/// </summary>
public class BubbleStorageService : IBubbleStorageService
{
    private readonly BubbleStorageOptions _opt;

    public BubbleStorageService(IOptions<BubbleStorageOptions> opt)
    {
        _opt = opt.Value;
    }

    public string GetVideoWorkDir(string videoId, DateTime? createTime = null)
    {
        var dt = createTime ?? DateTime.Now;
        var yyyy = dt.ToString("yyyy");
        var mm = dt.ToString("MM");
        return Path.Combine(_opt.Root, "bubble", "videos", yyyy, mm, videoId);
    }

    public string GetVideoOriginalPath(string videoId, string extension, DateTime? createTime = null)
    {
        extension = extension.StartsWith('.') ? extension : "." + extension;
        return Path.Combine(GetVideoWorkDir(videoId, createTime), $"original{extension}");
    }

    public string GetDerivedDir(string videoId, DateTime? createTime = null)
        => Path.Combine(GetVideoWorkDir(videoId, createTime), "derived");

    public string GetSubtitleDir(string videoId, DateTime? createTime = null)
        => Path.Combine(GetDerivedDir(videoId, createTime), "subtitle");

    public string GetUnitAudioDir(string videoId, DateTime? createTime = null)
        => Path.Combine(GetDerivedDir(videoId, createTime), "unit-audio");

    public string ToPublicUrl(string absPath)
    {
        // 将 {Root}/bubble/videos/... 映射为 {PublicPrefix}/...
        var rootVideos = Path.Combine(_opt.Root, "bubble", "videos");
        var normRoot = Path.GetFullPath(rootVideos).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var normPath = Path.GetFullPath(absPath);
        if (!normPath.StartsWith(normRoot, StringComparison.OrdinalIgnoreCase))
            return absPath;

        var rel = normPath.Substring(normRoot.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return _opt.PublicPrefix.TrimEnd('/') + "/" + rel.Replace('\\', '/');
    }
}
