using QT.Common.Const;
using QT.Extras.DatabaseAccessor.SqlSugar;
using SqlSugar;

namespace QT.JXC.Entitys.Entity.ERP;

/// <summary>
/// 出库订单表实体.
/// </summary>
[SugarTable("erp_outorder")]
[Tenant(ClaimConst.TENANTID)]
public class ErpOutorderEntity: ICompanyEntity
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
    /// 出库订单号.
    /// </summary>
    [SugarColumn(ColumnName = "F_No")]
    public string No { get; set; }

    /// <summary>
    /// 出库类型.
    /// </summary>
    [SugarColumn(ColumnName = "F_InType")]
    public string InType { get; set; }

    /// <summary>
    /// 订单金额.
    /// </summary>
    [SugarColumn(ColumnName = "F_Amount")]
    public decimal Amount { get; set; }

    /// <summary>
    /// 销售订单编号.
    /// </summary>
    [SugarColumn(ColumnName = "F_XsNo")]
    public string XsNo { get; set; }

    /// <summary>
    /// 是否特殊出库.
    /// </summary>
    [SugarColumn(ColumnName = "F_IsSpecial")]
    public string IsSpecial { get; set; }

    /// <summary>
    /// 特殊处理完毕状态.
    /// </summary>
    [SugarColumn(ColumnName = "F_SpecialState")]
    public string SpecialState { get; set; }

    /// <summary>
    /// 特殊处理用户.
    /// </summary>
    [SugarColumn(ColumnName = "F_SpecialUserId")]
    public string SpecialUserId { get; set; }

    /// <summary>
    /// 出库人.
    /// </summary>
    [SugarColumn(ColumnName = "F_OutUserId")]
    public string OutUserId { get; set; }

    /// <summary>
    /// 出库审核人.
    /// </summary>
    [SugarColumn(ColumnName = "F_OutCheckUserId")]
    public string OutCheckUserId { get; set; }

    /// <summary>
    /// 出库时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_OutTime")]
    public DateTime? OutTime { get; set; }

    /// <summary>
    /// 订单状态（0：正常，1：调拨中，2：已调入，）.
    /// </summary>
    [SugarColumn(ColumnName = "F_State")]
    public string State { get; set; }

    /// <summary>
    /// 状态时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_StateTime")]
    public DateTime? StateTime { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    [SugarColumn(ColumnName = "F_Remark")]
    public string Remark { get; set; }

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
    /// 调入公司ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_InOid")]
    public string InOid { get; set; }
}