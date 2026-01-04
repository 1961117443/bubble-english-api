using QT.Common.Filter;
using System.ComponentModel.DataAnnotations;

namespace QT.Extend.Entitys.Dto.WorkOrder;

public class WorkOrderListPageInput: PageInputBase
{
    ///// <summary>
    ///// 团队id
    ///// </summary>
    //[Required]
    //public string projectId { get; set; }

    /// <summary>
    /// 工单日期
    /// </summary>
    public string date { get; set; }

    /// <summary>
    /// 类型：inBox:我处理的，sent:我发送的
    /// </summary>
    public string activeTab { get; set; }

    /// <summary>
    /// 是否关闭
    /// </summary>
    public int? enabledMark { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    public string creatorUserId { get; set; }

    /// <summary>
    /// 处理人
    /// </summary>
    public string assignUserId { get; set; }
}