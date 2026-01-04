using SqlSugar;

namespace QT.JXC.Entitys.Dto.Erp;

/// <summary>
/// 出库订单表输出参数.
/// </summary>
public class ErpOutorderInfoOutput
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
    public List<ErpOutrecordInfoOutput> erpOutrecordList { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string remark { get; set; }


    /// <summary>
    /// 调入公司ID.
    /// </summary>
    public string inOid { get; set; }

    /// <summary>
    /// 订单状态（0：正常，1：调拨中，2：已调入，）.
    /// </summary>
    public string state { get; set; }
}


public class ErpOutorderDbAuditInfoOutput: ErpOutorderInfoOutput
{
    /// <summary>
    /// 商品出库记录.
    /// </summary>
    public new List<ErpOutrecordDbAuditInfoOutput> erpOutrecordList { get; set; }
}

public class ErpOutrecordDbAuditInfoOutput : ErpOutrecordInfoOutput
{
    /// <summary>
    /// 调出数量.
    /// </summary>
    public decimal outNum { get; set; }

    /// <summary>
    /// 特殊金额.
    /// </summary>
    public decimal tsAmount { get; set; }

    /// <summary>
    /// 关联特殊入库数量
    /// </summary>
    public decimal tsNum { get; set; }

    public string storeRomeId { get; set; }
    public string storeRomeAreaId { get; set; }
}