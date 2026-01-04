using QT.Common.Const;
using SqlSugar;

namespace QT.JXC.Entitys.Entity;

/// <summary>
/// 付款单明细实体.
/// </summary>
[SugarTable("cw_payment_detail")]
[Tenant(ClaimConst.TENANTID)]
public class CwPaymentDetailEntity
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public string Id { get; set; }

    /// <summary>
    /// 主表id.
    /// </summary>
    [SugarColumn(ColumnName = "F_Fid")]
    public string Fid { get; set; }

    /// <summary>
    /// 转入id.
    /// </summary>
    [SugarColumn(ColumnName = "F_InId")]
    public string InId { get; set; }

    /// <summary>
    /// 转入类型.
    /// </summary>
    [SugarColumn(ColumnName = "F_InType")]
    public string InType { get; set; }

    /// <summary>
    /// 付款金额.
    /// </summary>
    [SugarColumn(ColumnName = "F_Amount")]
    public decimal Amount { get; set; }

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
    /// 删除标志.
    /// </summary>
    [SugarColumn(ColumnName = "F_DeleteMark")]
    public int? DeleteMark { get; set; }

    /// <summary>
    /// 删除时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_DeleteTime")]
    public DateTime? DeleteTime { get; set; }

    /// <summary>
    /// 删除用户.
    /// </summary>
    [SugarColumn(ColumnName = "F_DeleteUserId")]
    public string DeleteUserId { get; set; }

}