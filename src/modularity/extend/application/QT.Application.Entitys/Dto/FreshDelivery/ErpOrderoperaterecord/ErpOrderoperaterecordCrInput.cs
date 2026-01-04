namespace QT.Application.Entitys.Dto.FreshDelivery.ErpOrderoperaterecord;

/// <summary>
/// 订单处理记录表修改输入参数.
/// </summary>
public class ErpOrderoperaterecordCrInput
{
    public string id { get; set; }
    /// <summary>
    /// 状态值.
    /// </summary>
    public string state { get; set; }

    /// <summary>
    /// 时间.
    /// </summary>
    public DateTime? time { get; set; }

    /// <summary>
    /// 处理人.
    /// </summary>
    public string userId { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string remark { get; set; }

}