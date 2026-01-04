using QT.DependencyInjection;

namespace QT.Common.Models
{
    /// <summary>
    /// 附件模型
    /// </summary>
    [SuppressSniffer]
    public class AnnexModel
    {
        /// <summary>
        /// 文件ID.
        /// </summary>
        public string? FileId { get; set; }

        /// <summary>
        /// 文件名称.
        /// </summary>
        public string? FileName { get; set; }

        /// <summary>
        /// 文件大小.
        /// </summary>
        public string? FileSize { get; set; }

        /// <summary>
        /// 文件上传时间.
        /// </summary>
        public DateTime FileTime { get; set; }

        /// <summary>
        /// 文件状态.
        /// </summary>
        public string? FileState { get; set; }

        /// <summary>
        /// 文件类型.
        /// </summary>
        public string? FileType { get; set; }

        /// <summary>
        /// 文件地址.
        /// </summary>
        public string? FileUrl { get; set; }
    }
}
