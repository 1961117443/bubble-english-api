using QT.Common.Contracts;
using SqlSugar;
using System.ComponentModel;

namespace QT.SDMS.Entitys.Entity;

/// <summary>
/// 客户咨询回复
/// </summary>
[SugarTable("sdms_customer_ticket_reply")]
public class CustomerTicketReplyEntity : CUDEntityBase
{
    /// <summary>
    /// 工单ID
    /// </summary>
    [SugarColumn(ColumnName = "TicketId")]
    public string TicketId { get; set; }

    /// <summary>
    /// 回复人ID
    /// </summary>
    [SugarColumn(ColumnName = "ReplyById")]
    public string ReplyById { get; set; }

    /// <summary>
    /// 回复人
    /// </summary>
    [SugarColumn(ColumnName = "ReplyBy")]
    public string ReplyBy { get; set; }

    /// <summary>
    /// 回复角色：客户 / 销售
    /// </summary>
    public ReplyRole ReplyRole { get; set; }
     

    /// <summary>
    /// 问题内容
    /// </summary>
    [SugarColumn(ColumnName = "Content")]
    public string Content { get; set; }

    /// <summary>
    /// 附件URL
    /// </summary>
    [SugarColumn(ColumnName = "Attachment")]
    public string? Attachment { get; set; }
 
    /// <summary>
    /// 图片
    /// </summary>
    [SugarColumn(ColumnName = "ImageJson")]
    public string? ImageJson { get; set; }

}

public enum ReplyRole
{
    [Description("客户")] Customer = 1,    // 客户
    [Description("销售人员")] Sales = 2        // 销售人员
}
