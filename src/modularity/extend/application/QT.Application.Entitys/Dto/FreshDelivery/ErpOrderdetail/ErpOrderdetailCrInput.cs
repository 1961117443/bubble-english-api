namespace QT.Application.Entitys.Dto.FreshDelivery.ErpOrderdetail;

/// <summary>
/// 订单商品表修改输入参数.
/// </summary>
public class ErpOrderdetailCrInput
{
    public string id { get; set; }

    /// <summary>
    /// 商品规格ID.
    /// </summary>
    public string mid { get; set; }

    /// <summary>
    /// 配送数量.
    /// </summary>
    public decimal num1 { get; set; }

    /// <summary>
    /// 配送总价.
    /// </summary>
    public decimal amount1 { get; set; }

    /// <summary>
    /// 复核数量.
    /// </summary>
    public decimal num2 { get; set; }

    /// <summary>
    /// 复核总价.
    /// </summary>
    public decimal amount2 { get; set; }

    /// <summary>
    /// 复核时间.
    /// </summary>
    public DateTime? checkTime { get; set; }

    /// <summary>
    /// 订单数量.
    /// </summary>
    public decimal num { get; set; }

    /// <summary>
    /// 订单总价.
    /// </summary>
    public decimal amount { get; set; }

    /// <summary>
    /// 销售单价
    /// </summary>
    public decimal salePrice { get; set; }

    /// <summary>
    /// 订单备注
    /// </summary>
    public string remark { get; set; }

    /// <summary>
    /// 退回数量.
    /// </summary>
    public decimal rejectNum { get; set; }

    /// <summary>
    /// 序号
    /// </summary>
    public int? order { get; set; }


    /// <summary>
    /// 打印别名
    /// </summary>
    public string printAlais { get; set; }

    /// <summary>
    /// 生产日期.
    /// </summary>
    //public string productDate { get; set; }
    public DateTime? productionDate { get; set; }
    /// <summary>
    /// 保质期.
    /// </summary>
    public string retention { get; set; }
}