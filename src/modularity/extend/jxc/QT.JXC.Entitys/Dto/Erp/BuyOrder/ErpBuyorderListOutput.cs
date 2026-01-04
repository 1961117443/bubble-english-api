using SqlSugar;

namespace QT.JXC.Entitys.Dto.Erp.BuyOrder;

/// <summary>
/// 采购任务订单输入参数.
/// </summary>
public class ErpBuyorderListOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    ///// <summary>
    ///// 公司ID.
    ///// </summary>
    //public string oid { get; set; }

    /// <summary>
    /// 订单编号.
    /// </summary>
    public string no { get; set; }

    /// <summary>
    /// 指派采购人员.
    /// </summary>
    public string taskToUserId { get; set; }

    /// <summary>
    /// 计划采购时间.
    /// </summary>
    public DateTime? taskBuyTime { get; set; }

    /// <summary>
    /// 任务备注.
    /// </summary>
    public string taskRemark { get; set; }

    /// <summary>
    /// 指派采购人员名称.
    /// </summary>
    public string taskToUserName { get; set; }

    /// <summary>
    /// 状态.
    /// </summary>
    public string state { get; set; }

    /// <summary>
    /// 公司.
    /// </summary>
    public string oidName { get; set; }

    public DateTime? creatorTime { get; set; }

    public DateTime? auditTime { get; set; }

    public string auditUserName { get; set; }

    /// <summary>
    /// 明细完成数量
    /// </summary>
    public int detailCount { get; set; }

    public string supplierName { get; set; }

    public string channel { get; set; }

    public string payment { get; set; }

    /// <summary>
    /// 是否有质检报告
    /// </summary>
    public int hasQualityReport { get; set; }

    /// <summary>
    /// 是否自检
    /// </summary>
    public int selfCheck { get; set; }

    /// <summary>
    /// 入库单号
    /// </summary>
    public string rkNo { get; set; }

    /// <summary>
    /// 采购金额
    /// </summary>
    public decimal amount { get; set; }

    /// <summary>
    /// 是否上报进货登记
    /// </summary>
    public bool hasCycs { get; set; }

    /// <summary>
    /// 退回金额
    /// </summary>
    public decimal thAmount { get; set; }
}