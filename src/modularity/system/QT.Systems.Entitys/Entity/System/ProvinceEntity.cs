using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Systems.Entitys.System;

/// <summary>
/// 行政区划
/// </summary>
[SugarTable("BASE_PROVINCE")]
[Tenant(ClaimConst.TENANTID)]
public class ProvinceEntity : CLDEntityBase
{
    /// <summary>
    /// 区域上级.
    /// </summary>
    [SugarColumn(ColumnName = "F_PARENTID")]
    public string ParentId { get; set; }

    /// <summary>
    /// 区域编码.
    /// </summary>
    [SugarColumn(ColumnName = "F_ENCODE")]
    public string EnCode { get; set; }

    /// <summary>
    /// 区域名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_FULLNAME")]
    public string FullName { get; set; }

    /// <summary>
    /// 快速查询.
    /// </summary>
    [SugarColumn(ColumnName = "F_QUICKQUERY")]
    public string QuickQuery { get; set; }

    /// <summary>
    /// 区域类型：1-省份、2-城市、3-县区、4-街道.
    /// </summary>
    [SugarColumn(ColumnName = "F_TYPE")]
    public string Type { get; set; }

    /// <summary>
    /// 描述.
    /// </summary>
    [SugarColumn(ColumnName = "F_DESCRIPTION")]
    public string Description { get; set; }

    /// <summary>
    /// 排序码.
    /// </summary>
    [SugarColumn(ColumnName = "F_SORTCODE")]
    public long? SortCode { get; set; }
}