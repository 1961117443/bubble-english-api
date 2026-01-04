using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Extend.Entitys;

/// <summary>
/// 标签管理
/// </summary>
[SugarTable("EXT_LABEL")]
[Tenant(ClaimConst.TENANTID)]
public class LabelEntity : CLDEntityBase
{
    /// <summary>
    /// 标签分类.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_Category")]
    public string? Category { get; set; }

    /// <summary>
    /// 标签描述.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_FullName")]
    public string? FullName { get; set; }

    /// <summary>
    /// 标签集合，多个逗号相连.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_Label")]
    public string? Label { get; set; }

    /// <summary>
    /// 是否全局标签（系统标签）.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_IsGlobal")]
    public int IsGlobal { get; set; }
}
