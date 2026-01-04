using QT.Application.Entitys.Dto.FreshDelivery.ErpInrecord;
using QT.Application.Entitys.Dto.FreshDelivery.ErpOutrecord;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpInorder;

/// <summary>
/// 入库订单表输出参数.
/// </summary>
public class ErpInorderInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 公司ID.
    /// </summary>
    public string oid { get; set; }

    /// <summary>
    /// 入库订单号.
    /// </summary>
    public string no { get; set; }

    /// <summary>
    /// 入库类型.
    /// </summary>
    public string inType { get; set; }

    /// <summary>
    /// 订单金额.
    /// </summary>
    public decimal amount { get; set; }

    /// <summary>
    /// 采购订单编号.
    /// </summary>
    public string cgNo { get; set; }

    /// <summary>
    /// 商品入库记录.
    /// </summary>
    public List<ErpInrecordInfoOutput> erpInrecordList { get; set; }


    /// <summary>
    /// 出库公司ID.
    /// </summary>
    public string outOid { get; set; }

}

public class ErpInorderJgInfoOutput : ErpInorderInfoOutput
{
    /// <summary>
    /// 商品出库记录.
    /// </summary>
    public List<ErpOutrecordInfoOutput> erpOutrecordList { get; set; }
}