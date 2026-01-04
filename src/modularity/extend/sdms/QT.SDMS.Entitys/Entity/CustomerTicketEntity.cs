using QT.Common.Contracts;
using SqlSugar;
using System.ComponentModel;

namespace QT.SDMS.Entitys.Entity;

/// <summary>
/// 客户咨询
/// </summary>
[SugarTable("sdms_customer_ticket")]
public class CustomerTicketEntity : CUDEntityBase
{
    /// <summary>
    /// 咨询编号
    /// </summary>
    [SugarColumn(ColumnName = "No")]
    public string No { get; set; }

    /// <summary>
    /// 客户ID
    /// </summary>
    [SugarColumn(ColumnName = "CustomerId")]
    public string CustomerId { get; set; }

    /// <summary>
    /// 问题标题
    /// </summary>
    [SugarColumn(ColumnName = "Title")]
    public string Title { get; set; }

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
    /// 工单状态
    /// </summary>
    [SugarColumn(ColumnName = "Status")]
    public TicketStatus Status { get; set; }

    /// <summary>
    /// 图片
    /// </summary>
    [SugarColumn(ColumnName = "ImageJson")]
    public string? ImageJson { get; set; }

    /// <summary>
    /// 客户经理
    /// </summary>
    [SugarColumn(ColumnName = "ManagerId")]
    public string? ManagerId { get; set; }
}

 

public enum TicketStatus:int
{
    [Description("待处理")] Pending = 0,     // 待处理
    [Description("处理中")] Processing = 1,  // 处理中
    [Description("已解决")] Resolved = 2,    // 已解决
    [Description("已关闭")] Closed = 3,      // 已关闭
    [Description("重新打开")] Reopened = 4     // 重新打开
}

