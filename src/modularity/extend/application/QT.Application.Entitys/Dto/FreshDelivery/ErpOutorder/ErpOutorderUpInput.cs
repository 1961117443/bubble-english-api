namespace QT.Application.Entitys.Dto.FreshDelivery.ErpOutorder;

/// <summary>
/// 出库订单表更新输入.
/// </summary>
public class ErpOutorderUpInput : ErpOutorderCrInput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }
}

public class ErpDbInAduitCrInput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 调出公司ID.
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
    /// 调入公司ID.
    /// </summary>
    public string inOid { get; set; }

    /// <summary>
    /// 调出记录.
    /// </summary>
    public List<ErpDbrecordInAduitCrInput> erpOutrecordList { get; set; }
}

public class ErpDbrecordInAduitCrInput
{
    /// <summary>
    /// 调出id
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 商品规格ID.
    /// </summary>
    public string gid { get; set; }

    /// <summary>
    /// 数量.
    /// </summary>
    public decimal num { get; set; }

    /// <summary>
    /// 单价.
    /// </summary>
    public decimal price { get; set; }

    /// <summary>
    /// 总价.
    /// </summary>
    public decimal amount { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string remark { get; set; }

    public string storeRomeAreaId { get; set; }
    public string storeRomeId { get; set; }
}