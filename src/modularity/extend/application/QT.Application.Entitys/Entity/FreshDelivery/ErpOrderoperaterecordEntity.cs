using QT.Common.Const;
using SqlSugar;

namespace QT.Application.Entitys.FreshDelivery;

/// <summary>
/// 订单处理记录表实体.
/// </summary>
[SugarTable("erp_orderoperaterecord")]
[Tenant(ClaimConst.TENANTID)]
public class ErpOrderoperaterecordEntity
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
    /// 订单ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_Fid")]
    public string Fid { get; set; }

    /// <summary>
    /// 状态值.
    /// </summary>
    [SugarColumn(ColumnName = "F_State")]
    public string State { get; set; }

    /// <summary>
    /// 时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_Time")]
    public DateTime? Time { get; set; }

    /// <summary>
    /// 处理人.
    /// </summary>
    [SugarColumn(ColumnName = "F_UserId")]
    public string UserId { get; set; }

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

}