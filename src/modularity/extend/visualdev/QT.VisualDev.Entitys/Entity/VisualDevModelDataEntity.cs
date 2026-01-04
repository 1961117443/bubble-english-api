using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.VisualDev.Entitys;

/// <summary>
/// 可视化开发功能实体




/// </summary>
[SugarTable("BASE_VISUALDEV_MODELDATA")]
[Tenant(ClaimConst.TENANTID)]
public class VisualDevModelDataEntity : CLDEntityBase
{
    /// <summary>
    /// 功能ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_VISUALDEVID")]
    public string VisualDevId { get; set; }

    /// <summary>
    /// 排序码.
    /// </summary>
    [SugarColumn(ColumnName = "F_SORTCODE")]
    public long? SortCode { get; set; }

    /// <summary>
    /// 区分主子表.
    /// </summary>
    [SugarColumn(ColumnName = "F_PARENTID")]
    public string ParentId { get; set; }

    /// <summary>
    /// 数据包.
    /// </summary>
    [SugarColumn(ColumnName = "F_DATA")]
    public string Data { get; set; }
}