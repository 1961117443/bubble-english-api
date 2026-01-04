namespace QT.Application.Entitys.Dto.FreshDelivery.ErpInorder;

/// <summary>
/// 入库订单表输入参数.
/// </summary>
public class ErpInorderListOutput
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


    public string oidName { get; set; }

    public string specialState { get; set; }


    /// <summary>
    /// 调出公司
    /// </summary>
    public string outOidName { get; set; }


    /// <summary>
    /// 订单编号（特殊入库才显示）
    /// </summary>
    public string orderNo { get; set; }

    /// <summary>
    /// 商品数量
    /// </summary>
    public int num { get; set; }

    /// <summary>
    /// 商品名称集合
    /// </summary>
    public string productNames { get; set; }

    /// <summary>
    /// 商品数量汇总
    /// </summary>
    public decimal totalNum { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? creatorTime { get; set; }


    /// <summary>
    /// 供应商名称
    /// </summary>
    public string supplierName { get; set; }

    /// <summary>
    /// 客户名称
    /// </summary>
    public string cidName { get; set; }
}