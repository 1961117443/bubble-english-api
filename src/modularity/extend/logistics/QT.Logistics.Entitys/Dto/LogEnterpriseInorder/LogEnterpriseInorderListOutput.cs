namespace QT.Logistics.Entitys.Dto.LogEnterpriseInorder;

/// <summary>
/// 商家入库订单表输入参数.
/// </summary>
public class LogEnterpriseInorderListOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 入库订单号.
    /// </summary>
    public string no { get; set; }

    /// <summary>
    /// 入库类型.
    /// </summary>
    public string inType { get; set; }

    /// <summary>
    /// 入库日期.
    /// </summary>
    public DateTime? inTime { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string remark { get; set; }

}