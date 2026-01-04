using QT.Application.Entitys.Dto.FreshDelivery.ErpInrecord;
using QT.Application.Entitys.Dto.FreshDelivery.ErpOutrecord;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpInorder;

/// <summary>
/// 入库订单表修改输入参数.
/// </summary>
public class ErpInorderCrInput
{
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
    public List<ErpInrecordCrInput> erpInrecordList { get; set; }

    /// <summary>
    /// 出库公司id.
    /// </summary>
    public string outOid { get; set; }

}

public class ErpInorderJgCrInput : ErpInorderCrInput
{
    /// <summary>
    /// 商品出库记录.
    /// </summary>
    public List<ErpOutrecordCrInput> erpOutrecordList { get; set; }
}

public class ErpInorderCbCrInput : ErpInorderCrInput
{
    /// <summary>
    /// 商品出库记录.
    /// </summary>
    public List<ErpOutrecordCrInput> erpOutrecordList { get; set; }
}

public class ErpInorderExtUpAuditTime
{
    public string id { get; set; }

    public DateTime? auditTime { get; set; }
}

public class ErpInorderExtUpExpenseTime
{
    public string id { get; set; }

    public DateTime? expenseTime { get; set; }
}

public class UpdateStoreInfoInput
{
    public string id { get; set; }

    public DateTime? productionDate { get; set; }

    public string retention { get; set; }
}