namespace QT.JXC.Entitys.Dto.Erp;

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

public class ErpInorderJgCrInput
{
    #region 入库数据
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
    public List<ErpInrecordJgCrInput> erpInrecordList { get; set; }

    /// <summary>
    /// 出库公司id.
    /// </summary>
    public string outOid { get; set; }

    #endregion

    /// <summary>
    /// 商品出库记录.
    /// </summary>
    public List<ErpOutrecordCrInput> erpOutrecordList { get; set; }
}

public class ErpInrecordJgCrInput : ErpInrecordCrInput
{
    #region 特殊入库数据
    public List<ErpInrecordTransferInputV2Item> erpTsInrecordList { get; set; }
    #endregion
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