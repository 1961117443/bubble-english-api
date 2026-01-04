using QT.Common.Filter;

namespace QT.CMS.Entitys.Dto.Parameter;

/// <summary>
/// 基本参数
/// </summary>
public class BaseParameter : PageInputBase
{
    /// <summary>
    /// 所属站点
    /// </summary>
    public virtual int SiteId { get; set; }
    /// <summary>
    /// 查询关健字
    /// </summary>
    public virtual string Keyword { get; set; } = string.Empty;
    /// <summary>
    /// 状态
    /// </summary>
    public virtual int? Status { get; set; } = -1;
    /// <summary>
    /// 排序
    /// </summary>
    public virtual string? OrderBy { get; set; }
    /// <summary>
    /// 显示字段,以逗号分隔
    /// </summary>
    public virtual string? Fields { get; set; }
}
