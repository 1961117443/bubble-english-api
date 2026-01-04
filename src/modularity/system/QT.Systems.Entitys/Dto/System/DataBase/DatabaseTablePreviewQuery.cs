using QT.Common.Filter;
using QT.DependencyInjection;

namespace QT.Systems.Entitys.Dto.Database;

/// <summary>
/// 数据库表预览查询.
/// </summary>
[SuppressSniffer]
public class DatabaseTablePreviewQuery : PageInputBase
{
    /// <summary>
    /// 字段.
    /// </summary>
    public string field { get; set; }
}