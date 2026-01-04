using QT.Common.Const;
using QT.Extras.DatabaseAccessor.SqlSugar;
using SqlSugar;

namespace QT.JXC.Entitys.Entity;

/// <summary>
/// 盘点记录主表实体.
/// </summary>
[SugarTable("erp_productcheck_master")]
[Tenant(ClaimConst.TENANTID)]
public class ErpProductcheckMasterEntity :ICompanyEntity
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
    /// 盘点日期.
    /// </summary>
    [SugarColumn(ColumnName = "F_CheckTime")]
    public DateTime? CheckTime { get; set; }

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
    /// 订单编号.
    /// </summary>
    [SugarColumn(ColumnName = "F_No")]
    public string No { get; set; }

    /// <summary>
    /// 审核时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_AuditTime")]
    public DateTime? AuditTime { get; set; }

    /// <summary>
    /// 审核用户.
    /// </summary>
    [SugarColumn(ColumnName = "F_AuditUserId")]
    public string AuditUserId { get; set; }
}