using QT.Common.Filter;
using System.ComponentModel.DataAnnotations;

namespace QT.JXC.Entitys.Dto.Erp.OrderKh;

/// <summary>
/// 订单信息列表查询输入
/// </summary>
public class ErpOrderKhListQueryInput : PageInputBase
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
    /// 订单编号.
    /// </summary>
    public string no { get; set; }

    /// <summary>
    /// 开始日期
    /// </summary>
    public DateTime? beginDate { get; set; }


    /// <summary>
    /// 结束日期
    /// </summary>
    public DateTime? endDate { get; set; }

    /// <summary>
    /// 订单编号.
    /// </summary>
    [Required(ErrorMessage ="客户不存在！")]
    public string cid { get; set; }
}