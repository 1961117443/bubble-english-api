using QT.Common.Const;
using QT.Extras.DatabaseAccessor.SqlSugar;
using SqlSugar;

namespace QT.Application.Entitys.FreshDelivery;

/// <summary>
/// 商品信息实体.
/// </summary>
[SugarTable("erp_product")]
[Tenant(ClaimConst.TENANTID)]
public class ErpProductEntity
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public string Id { get; set; }


    /// <summary>
    /// 分类ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_Tid",ColumnDescription = "分类")]
    public string Tid { get; set; }

    /// <summary>
    /// 名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_Name", ColumnDescription = "名称")]
    public string Name { get; set; }

    /// <summary>
    /// 别名.
    /// </summary>
    [SugarColumn(ColumnName = "F_Nickname", ColumnDescription = "别名")]
    public string Nickname { get; set; }

    /// <summary>
    /// 拼音首字母.
    /// </summary>
    [SugarColumn(ColumnName = "F_FirstChar", ColumnDescription = "拼音首字母")]
    public string FirstChar { get; set; }

    /// <summary>
    /// 编号.
    /// </summary>
    [SugarColumn(ColumnName = "F_No", ColumnDescription = "编号")]
    public string No { get; set; }

    /// <summary>
    /// 计量单位.
    /// </summary>
    [SugarColumn(ColumnName = "F_Unit", ColumnDescription = "计量单位")]
    public string Unit { get; set; }

    /// <summary>
    /// 产地.
    /// </summary>
    [SugarColumn(ColumnName = "F_Producer", ColumnDescription = "产地")]
    public string Producer { get; set; }

    /// <summary>
    /// 主图.
    /// </summary>
    [SugarColumn(ColumnName = "F_Pic", ColumnDescription = "主图")]
    public string Pic { get; set; }

    /// <summary>
    /// 排序.
    /// </summary>
    [SugarColumn(ColumnName = "F_Sort", ColumnDescription = "排序")]
    public int? Sort { get; set; }

    /// <summary>
    /// 介绍.
    /// </summary>
    [SugarColumn(ColumnName = "F_Remark", ColumnDescription = "介绍")]
    public string Remark { get; set; }

    /// <summary>
    /// 存储条件.
    /// </summary>
    [SugarColumn(ColumnName = "F_Storage", ColumnDescription = "存储条件")]
    public string Storage { get; set; }

    /// <summary>
    /// 保质期.
    /// </summary>
    [SugarColumn(ColumnName = "F_Retention", ColumnDescription = "保质期")]
    public string Retention { get; set; }

    /// <summary>
    /// 供货商.
    /// </summary>
    [SugarColumn(ColumnName = "F_Supplier", ColumnDescription = "供货商")]
    public string Supplier { get; set; }

    /// <summary>
    /// 状态.
    /// </summary>
    [SugarColumn(ColumnName = "F_State", ColumnDescription = "状态")]
    public string State { get; set; }

    /// <summary>
    /// 销售类型.
    /// </summary>
    [SugarColumn(ColumnName = "F_Saletype", ColumnDescription = "销售类型")]
    public string Saletype { get; set; }

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

}