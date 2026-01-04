using QT.Common.Const;
using SqlSugar;

namespace QT.Logistics.Entitys;

/// <summary>
/// 配送点会员实体.
/// </summary>
[SugarTable("log_delivery_member")]
[Tenant(ClaimConst.TENANTID)]
public class LogDeliveryMemberEntity
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public string Id { get; set; }

    /// <summary>
    /// 配送点ID.
    /// </summary>
    [SugarColumn(ColumnName = "PointId")]
    public string PointId { get; set; }

    /// <summary>
    /// 线路ID.
    /// </summary>
    [SugarColumn(ColumnName = "MemberId")]
    public string MemberId { get; set; }

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

}