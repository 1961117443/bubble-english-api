using QT.Common.Filter;

namespace QT.Logistics.Entitys.Dto.LogDeliverynote;

/// <summary>
/// 配送单列表查询输入
/// </summary>
public class LogDeliverynoteListQueryInput : PageInputBase
{
    /// <summary>
    /// 选择导出数据key.
    /// </summary>
    public string selectKey { get; set; }

    /// <summary>
    /// 导出类型.
    /// </summary>
    public int dataType { get; set; }

    /// <summary>
    /// 配送单号.
    /// </summary>
    public string orderNo { get; set; }

    /// <summary>
    /// 配送日期.
    /// </summary>
    public string orderDate { get; set; }

    /// <summary>
    /// 收件点
    /// </summary>
    public string pointId { get; set; }
}