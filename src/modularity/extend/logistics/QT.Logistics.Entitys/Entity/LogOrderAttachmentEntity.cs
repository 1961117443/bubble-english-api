using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Logistics.Entitys;

/// <summary>
/// 物流订单实体.
/// </summary>
[SugarTable("log_order_attachment")]
[Tenant(ClaimConst.TENANTID)]
public class LogOrderAttachmentEntity : CUDEntityBase, IDeleteTime
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public override string Id { get; set; }

    /// <summary>
    /// 订单id.
    /// </summary>
    [SugarColumn(ColumnName = "OrderId")]
    public string OrderId { get; set; }

    /// <summary>
    /// 附件名称.
    /// </summary>
    [SugarColumn(ColumnName = "AttachmentName")]
    public string AttachmentName { get; set; }

    /// <summary>
    /// 附件路径.
    /// </summary>
    [SugarColumn(ColumnName = "AttachmentPath")]
    public string AttachmentPath { get; set; }

    /// <summary>
    /// 文件大小.
    /// </summary>
    [SugarColumn(ColumnName = "FileSize")]
    public string FileSize { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    [SugarColumn(ColumnName = "Remark")]
    public string Remark { get; set; }
}