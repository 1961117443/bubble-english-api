using QT.Application.Entitys.Dto.FreshDelivery.ErpOutrecord;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpOutorder;

/// <summary>
/// 出库订单表修改输入参数.
/// </summary>
public class ErpOutorderCrInput
{
    /// <summary>
    /// 公司ID.
    /// </summary>
    public string oid { get; set; }

    /// <summary>
    /// 出库订单号.
    /// </summary>
    public string no { get; set; }

    /// <summary>
    /// 出库类型.
    /// </summary>
    public string inType { get; set; }

    /// <summary>
    /// 订单金额.
    /// </summary>
    public decimal amount { get; set; }

    /// <summary>
    /// 销售订单编号.
    /// </summary>
    public string xsNo { get; set; }

    /// <summary>
    /// 商品出库记录.
    /// </summary>
    public List<ErpOutrecordCrInput> erpOutrecordList { get; set; }

    /// <summary>
    /// 调入公司ID.
    /// </summary>
    public string inOid { get; set; }

    /// <summary>
    /// 订单状态（0：正常，1：调拨中，2：已调入，）.
    /// </summary>
    public string state { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string remark { get; set; }
}