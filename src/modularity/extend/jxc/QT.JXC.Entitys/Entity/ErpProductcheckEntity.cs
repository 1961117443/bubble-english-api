using QT.Common.Const;
using SqlSugar;

namespace QT.JXC.Entitys.Entity;

/// <summary>
/// 盘点记录实体.
/// </summary>
[SugarTable("erp_productcheck")]
[Tenant(ClaimConst.TENANTID)]
public class ErpProductcheckEntity
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public string Id { get; set; }

    /// <summary>
    /// 公司ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_Oid")]
    public string Oid { get; set; }

    /// <summary>
    /// 规格ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_Gid")]
    public string Gid { get; set; }

    /// <summary>
    /// 盘点日期.
    /// </summary>
    [SugarColumn(ColumnName = "F_CheckTime")]
    public DateTime? CheckTime { get; set; }

    /// <summary>
    /// 系统数量.
    /// </summary>
    [SugarColumn(ColumnName = "F_SystemNum")]
    public decimal SystemNum { get; set; }

    /// <summary>
    /// 实际数量.
    /// </summary>
    [SugarColumn(ColumnName = "F_RealNum")]
    public decimal RealNum { get; set; }

    /// <summary>
    /// 拆损数量.
    /// </summary>
    [SugarColumn(ColumnName = "F_LoseNum")]
    public decimal LoseNum { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    [SugarColumn(ColumnName = "F_Remark")]
    public string Remark { get; set; }

    /// <summary>
    /// 盘点人.
    /// </summary>
    [SugarColumn(ColumnName = "F_CheckUsers")]
    public string CheckUsers { get; set; }

    /// <summary>
    /// 操作人.
    /// </summary>
    [SugarColumn(ColumnName = "F_CheckUserId")]
    public string CheckUserId { get; set; }

    /// <summary>
    /// 创建时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_CreatorTime")]
    public DateTime? CreatorTime { get; set; }

    /// <summary>
    /// 创建用户.
    /// </summary>
    [SugarColumn(ColumnName = "F_CreatorUserId")]
    public string CreatorUserId { get; set; }

    /// <summary>
    /// 修改时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_LastModifyTime")]
    public DateTime? LastModifyTime { get; set; }

    /// <summary>
    /// 修改用户.
    /// </summary>
    [SugarColumn(ColumnName = "F_LastModifyUserId")]
    public string LastModifyUserId { get; set; }

    /// <summary>
    /// 主表ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_Fid")]
    public string Fid { get; set; }

    /// <summary>
    /// 仓库.
    /// </summary>
    [SugarColumn(ColumnName = "F_StoreRomeId")]
    public string StoreRomeId { get; set; }

    /// <summary>
    /// 仓库库区.
    /// </summary>
    [SugarColumn(ColumnName = "F_StoreRomeAreaId")]
    public string StoreRomeAreaId { get; set; }

    /// <summary>
    /// 入库单价.
    /// </summary>
    [SugarColumn(ColumnName = "F_Price")]
    public decimal Price { get; set; }

    /// <summary>
    /// 入库金额.
    /// </summary>
    [SugarColumn(ColumnName = "F_Amount")]
    public decimal Amount { get; set; }

    /// <summary>
    /// 生产日期.
    /// </summary>
    [SugarColumn(ColumnName = "ProductionDate")]
    public DateTime? ProductionDate { get; set; }

    /// <summary>
    /// 批次号.
    /// </summary>
    [SugarColumn(ColumnName = "BatchNumber")]
    public string BatchNumber { get; set; }


    /// <summary>
    /// 保质期.
    /// </summary>
    [SugarColumn(ColumnName = "Retention")]
    public string Retention { get; set; }
}