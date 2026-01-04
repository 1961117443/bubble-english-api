using QT.DependencyInjection;
using SqlSugar;

namespace QT.Common.Filter;

/// <summary>
/// 通用分页输入参数.
/// </summary>
[SuppressSniffer]
public class PageInputBase : KeywordInput
{
    /// <summary>
    /// 查询条件.
    /// </summary>
    public virtual string queryJson { get; set; } = string.Empty;

    /// <summary>
    /// 当前页码:pageIndex.
    /// </summary>
    public virtual int currentPage { get; set; } = 1;

    /// <summary>
    /// 每页行数.
    /// </summary>
    public virtual int pageSize { get; set; } = 200;

    /// <summary>
    /// 排序字段:sortField.
    /// </summary>
    public virtual string sidx { get; set; } = string.Empty;

    /// <summary>
    /// 排序类型:sortType.
    /// </summary>
    public virtual string sort { get; set; } = "desc";

    /// <summary>
    /// 菜单ID.
    /// </summary>
    public virtual string? menuId { get; set; }

    /// <summary>
    /// sqlsugar的排序方式
    /// </summary>
    public virtual OrderByType orderBy => sort == "desc" ? OrderByType.Desc : OrderByType.Asc;
}