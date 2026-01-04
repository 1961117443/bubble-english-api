using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Logistics.Entitys;

/// <summary>
/// 商家商品信息实体.
/// </summary>
[SugarTable("log_product")]
[Tenant(ClaimConst.TENANTID)]
public class LogProductEntity : CUDEntityBase
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public override string Id { get; set; }

    /// <summary>
    /// 分类ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_Tid")]
    public string Tid { get; set; }

    /// <summary>
    /// 名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_Name")]
    public string Name { get; set; }

    /// <summary>
    /// 拼音首字母.
    /// </summary>
    [SugarColumn(ColumnName = "F_FirstChar")]
    public string FirstChar { get; set; }

    /// <summary>
    /// 产地.
    /// </summary>
    [SugarColumn(ColumnName = "F_Producer")]
    public string Producer { get; set; }

    /// <summary>
    /// 排序.
    /// </summary>
    [SugarColumn(ColumnName = "F_Sort")]
    public int? Sort { get; set; }

    /// <summary>
    /// 介绍.
    /// </summary>
    [SugarColumn(ColumnName = "F_Remark")]
    public string Remark { get; set; }

    /// <summary>
    /// 存储条件.
    /// </summary>
    [SugarColumn(ColumnName = "F_Storage")]
    public string Storage { get; set; }

    /// <summary>
    /// 保质期.
    /// </summary>
    [SugarColumn(ColumnName = "F_Retention")]
    public string Retention { get; set; }

    /// <summary>
    /// 状态.
    /// </summary>
    [SugarColumn(ColumnName = "F_State")]
    public int? State { get; set; }

    /// <summary>
    /// 简图.
    /// </summary>
    [SugarColumn(ColumnName = "ImageUrl")]
    public string ImageUrl { get; set; }
}