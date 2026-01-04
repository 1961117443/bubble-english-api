using QT.CMS.Entitys.Dto.Base;

namespace QT.CMS.Interfaces;

/// <summary>
/// 文件上传接口
/// </summary>
public interface IFileService
{
    /// <summary>
    /// 通过文件流上传文件方法
    /// </summary>
    /// <param name="byteData">文件字节数组</param>
    /// <param name="fileName">文件名</param>
    /// <param name="isThumbnail">是否生成缩略图</param>
    /// <param name="isWater">是否打水印</param>
    Task<FileDto> SaveAsync(byte[] byteData, string fileName, bool isThumbnail, bool isWater);

    /// <summary>
    /// 裁剪图片并保存
    /// </summary>
    Task<FileDto> CropAsync(string fileUri, int maxWidth, int maxHeight, int cropWidth, int cropHeight, int X, int Y);

    /// <summary>
    /// 保存远程文件到本地
    /// </summary>
    /// <param name="webRoot">站点的物理根目录</param>
    /// <param name="sourceUri">URI地址</param>
    /// <returns>上传后的路径</returns>
    Task<FileDto> RemoteAsync(string sourceUri);
}
