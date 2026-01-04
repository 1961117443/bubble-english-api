using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Asset.Entity;

/// <summary>
/// 仓库信息实体.
/// </summary>
[SugarTable("asset_storeroom")]
[Tenant(ClaimConst.TENANTID)]
public class AssetWarehouseEntity : CUDEntityBase
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
    /// 地址.
    /// </summary>
    [SugarColumn(ColumnName = "Address")]
    public string Address { get; set; }

    /// <summary>
    /// 简介.
    /// </summary>
    [SugarColumn(ColumnName = "Description")]
    public string Description { get; set; }

    /// <summary>
    /// 负责人.
    /// </summary>
    [SugarColumn(ColumnName = "AdminId")]
    public string AdminId { get; set; }

    /// <summary>
    /// 电话.
    /// </summary>
    [SugarColumn(ColumnName = "AdminTel")]
    public string AdminTel { get; set; }

    /// <summary>
    /// 面积.
    /// </summary>
    [SugarColumn(ColumnName = "Area")]
    public decimal Area { get; set; }

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
    /// 状态.
    /// </summary>
    [SugarColumn(ColumnName = "Status")]
    public int? Status { get; set; }

    /// <summary>
    /// 类型（仓库0，库区1，货柜2，柜层3）.
    /// </summary>
    [SugarColumn(ColumnName = "Category")]
    public int? Category { get; set; }

    /// <summary>
    /// 条码.
    /// </summary>
    [SugarColumn(ColumnName = "barcode")]
    public string Barcode { get; set; }

}