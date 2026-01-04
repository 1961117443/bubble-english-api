using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Logistics.Entitys;

/// <summary>
/// 配送单实体.
/// </summary>
[SugarTable("log_deliverynote")]
[Tenant(ClaimConst.TENANTID)]
public class LogDeliverynoteEntity : CUDEntityBase
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public override string Id { get; set; }

    /// <summary>
    /// 配送单号.
    /// </summary>
    [SugarColumn(ColumnName = "OrderNo")]
    public string OrderNo { get; set; }

    /// <summary>
    /// 配送日期.
    /// </summary>
    [SugarColumn(ColumnName = "OrderDate")]
    public DateTime? OrderDate { get; set; }

    /// <summary>
    /// 收件点.
    /// </summary>
    [SugarColumn(ColumnName = "PointId")]
    public string PointId { get; set; }
}