
namespace QT.Logistics.Entitys.Dto.LogOrderDetail;

/// <summary>
/// 订单物品明细输出参数.
/// </summary>
public class LogOrderDetailInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 物品名称.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 计量单位.
    /// </summary>
    public string unit { get; set; }

    /// <summary>
    /// 数量.
    /// </summary>
    public decimal num { get; set; }

    /// <summary>
    /// 运费.
    /// </summary>
    public decimal freight { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string remark { get; set; }

}