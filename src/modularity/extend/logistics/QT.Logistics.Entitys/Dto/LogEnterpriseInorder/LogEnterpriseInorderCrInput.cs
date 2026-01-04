using QT.Logistics.Entitys.Dto.LogEnterpriseInrecord;

namespace QT.Logistics.Entitys.Dto.LogEnterpriseInorder;

/// <summary>
/// 商家入库订单表修改输入参数.
/// </summary>
public class LogEnterpriseInorderCrInput
{
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

    /// <summary>
    /// 商家商品入库明细.
    /// </summary>
    public List<LogEnterpriseInrecordCrInput> logEnterpriseInrecordList { get; set; }

}