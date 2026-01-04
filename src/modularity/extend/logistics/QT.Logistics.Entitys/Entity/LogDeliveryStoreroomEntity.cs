using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Logistics.Entitys;

/// <summary>
/// 配送点仓库实体.
/// </summary>
[SugarTable("log_delivery_storeroom")]
[Tenant(ClaimConst.TENANTID)]
public class LogDeliveryStoreroomEntity : CUDEntityBase
{
    /// <summary>
    /// 自然主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public override string Id { get; set; }

    /// <summary>
    /// 名称.
    /// </summary>
    [SugarColumn(ColumnName = "Name")]
    public string Name { get; set; }

    /// <summary>
    /// 编号.
    /// </summary>
    [SugarColumn(ColumnName = "Code")]
    public string Code { get; set; }

    /// <summary>
    /// 上级id.
    /// </summary>
    [SugarColumn(ColumnName = "PId")]
    public string PId { get; set; }

    /// <summary>
    /// 简介.
    /// </summary>
    [SugarColumn(ColumnName = "Description")]
    public string Description { get; set; }

    /// <summary>
    /// 类型（仓库0，库区1，货柜2，柜层3）.
    /// </summary>
    [SugarColumn(ColumnName = "Category")]
    public int? Category { get; set; }

    /// <summary>
    /// 配送点id.
    /// </summary>
    [SugarColumn(ColumnName = "PointId")]
    public string PointId { get; set; }

}