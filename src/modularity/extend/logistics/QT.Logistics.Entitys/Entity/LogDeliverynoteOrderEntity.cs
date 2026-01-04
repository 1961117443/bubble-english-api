using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Logistics.Entitys;

/// <summary>
/// 配送单订单明细实体.
/// </summary>
[SugarTable("log_deliverynote_order")]
[Tenant(ClaimConst.TENANTID)]
public class LogDeliverynoteOrderEntity : CUDEntityBase
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public override string Id { get; set; }

    /// <summary>
    /// 配送单ID.
    /// </summary>
    [SugarColumn(ColumnName = "NoteId")]
    public string NoteId { get; set; }

    /// <summary>
    /// 订单ID.
    /// </summary>
    [SugarColumn(ColumnName = "OrderId")]
    public string OrderId { get; set; }

}