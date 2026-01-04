using QT.Logistics.Entitys.Dto.LogEnterpriseOutrecord;

namespace QT.Logistics.Entitys.Dto.LogEnterpriseOutorder;

/// <summary>
/// 商家商品出库表修改输入参数.
/// </summary>
public class LogEnterpriseOutorderCrInput
{
    /// <summary>
    /// 出库订单号.
    /// </summary>
    public string no { get; set; }

    /// <summary>
    /// 出库类型.
    /// </summary>
    public string outType { get; set; }

    /// <summary>
    /// 出库日期.
    /// </summary>
    public DateTime? outTime { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string remark { get; set; }

    /// <summary>
    /// 商家商品出库明细.
    /// </summary>
    public List<LogEnterpriseOutrecordCrInput> logEnterpriseOutrecordList { get; set; }

}