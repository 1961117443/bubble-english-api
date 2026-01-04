using QT.Common.Const;
using QT.Common.Contracts;
using QT.Extras.DatabaseAccessor.SqlSugar;
using SqlSugar;

namespace QT.Application.Entitys.FreshDelivery;

/// <summary>
/// 商品规格实体.
/// </summary>
[SugarTable("erp_productmodel")]
[Tenant(ClaimConst.TENANTID)]
public class ErpProductmodelEntity :EntityBase<string>
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true,ColumnDescription = "主键")]
    public override string Id { get; set; }


    /// <summary>
    /// 商品ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_Pid", ColumnDescription = "商品ID")]
    public string Pid { get; set; }

    /// <summary>
    /// 规格名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_Name", ColumnDescription = "规格名称")]
    public string Name { get; set; }

    /// <summary>
    /// 主单位数量比.
    /// </summary>
    [SugarColumn(ColumnName = "F_Ratio", ColumnDescription = "主单位数量比")]
    public decimal? Ratio { get; set; }

    /// <summary>
    /// 成本价.
    /// </summary>
    [SugarColumn(ColumnName = "F_CostPrice", ColumnDescription = "成本价")]
    public decimal CostPrice { get; set; }

    /// <summary>
    /// 销售价.
    /// </summary>
    [SugarColumn(ColumnName = "F_SalePrice", ColumnDescription = "销售价")]
    public decimal SalePrice { get; set; }

    /// <summary>
    /// 条形码.
    /// </summary>
    [SugarColumn(ColumnName = "F_BarCode", ColumnDescription = "条形码")]
    public string BarCode { get; set; }

    /// <summary>
    /// 起售数.
    /// </summary>
    [SugarColumn(ColumnName = "F_MinNum", ColumnDescription = "起售数")]
    public decimal MinNum { get; set; }

    /// <summary>
    /// 限购数.
    /// </summary>
    [SugarColumn(ColumnName = "F_MaxNum", ColumnDescription = "限购数")]
    public decimal MaxNum { get; set; }

    /// <summary>
    /// 毛利率.
    /// </summary>
    [SugarColumn(ColumnName = "F_GrossMargin", ColumnDescription = "毛利率")]
    public decimal GrossMargin { get; set; }

    /// <summary>
    /// 包装物.
    /// </summary>
    [SugarColumn(ColumnName = "F_Package", ColumnDescription = "包装物")]
    public string Package { get; set; }

    /// <summary>
    /// 库存.
    /// </summary>
    [SugarColumn(ColumnName = "F_Num", ColumnDescription = "库存")]
    public decimal Num { get; set; }

    /// <summary>
    /// 最后盘点时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_CheckTime", ColumnDescription = "最后盘点时间")]
    public DateTime? CheckTime { get; set; }

    /// <summary>
    /// 创建时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_CreatorTime", ColumnDescription = "创建时间")]
    public DateTime? CreatorTime { get; set; }

    /// <summary>
    /// 创建用户.
    /// </summary>
    [SugarColumn(ColumnName = "F_CreatorUserId", ColumnDescription = "创建用户")]
    public string CreatorUserId { get; set; }

    /// <summary>
    /// 修改时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_LastModifyTime", ColumnDescription = "修改时间")]
    public DateTime? LastModifyTime { get; set; }

    /// <summary>
    /// 修改用户.
    /// </summary>
    [SugarColumn(ColumnName = "F_LastModifyUserId", ColumnDescription = "修改用户")]
    public string LastModifyUserId { get; set; }
    /// <summary>
    /// 计量单位.
    /// </summary>
    [SugarColumn(ColumnName = "F_Unit", ColumnDescription = "计量单位")]
    public string Unit { get; set; }

    /// <summary>
    /// 关联产品id（规格）.
    /// </summary>
    [SugarColumn(ColumnName = "F_Rid", ColumnDescription = "关联产品id")]
    public string Rid { get; set; }

    /// <summary>
    /// 客户单位.
    /// </summary>
    [SugarColumn(ColumnName = "F_CustomerUnit", ColumnDescription = "客户单位")]
    public string CustomerUnit { get; set; }


}