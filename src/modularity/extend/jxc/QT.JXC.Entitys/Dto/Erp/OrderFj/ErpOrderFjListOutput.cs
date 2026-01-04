using QT.JXC.Entitys.Enums;
using System.ComponentModel.DataAnnotations;

namespace QT.JXC.Entitys.Dto.Erp.OrderFj;

/// <summary>
/// 订单信息输入参数.
/// </summary>
public class ErpOrderFjListOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 订单编号.
    /// </summary>
    public string no { get; set; }

    /// <summary>
    /// 代下单人员.
    /// </summary>
    public string createUid { get; set; }

    /// <summary>
    /// 代下单时间.
    /// </summary>
    public DateTime? createTime { get; set; }

    /// <summary>
    /// 客户ID.
    /// </summary>
    public string cid { get; set; }

    /// <summary>
    /// 约定送货时间.
    /// </summary>
    public DateTime? posttime { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? creatorTime { get; set; }

    /// <summary>
    /// 客户名称
    /// </summary>
    public string cidName { get; set; }

    /// <summary>
    /// 订单状态
    /// </summary>
    public OrderStateEnum state { get; set; }

    /// <summary>
    /// 代下单人员.
    /// </summary>
    public string createUidName { get; set; }

    /// <summary>
    /// 星期几
    /// </summary>
    public string dayOfWeek { get; set; }

    /// <summary>
    /// 品种数量
    /// </summary>
    public int detailCount { get; set; }

    /// <summary>
    /// 进度
    /// </summary>
    public double progress { get; set; }


    /// <summary>
    /// 餐别
    /// </summary>
    public string diningType { get; set; }
}


public class ErpOrderFjSumListOutput
{
    /// <summary>
    /// 规格id
    /// </summary>
    public string mid { get; set; }

    /// <summary>
    /// 商品id
    /// </summary>
    public string pid { get; set; }

    /// <summary>
    /// 商品名称
    /// </summary>
    [Display(Name = "商品名称")]
    public string productName { get; set; }

    /// <summary>
    /// 规格名称
    /// </summary>
    [Display(Name = "规格名称")]
    public string name { get; set; }


    /// <summary>
    /// 订单数量
    /// </summary>
    [Display(Name = "订单数量")]
    public decimal num { get; set; }
}


public class ErpOrderFjStoreSumListOutput: ErpOrderFjSumListOutput
{
    /// <summary>
    /// 库存数量
    /// </summary>
    [Display(Name = "库存数量")]
    public decimal storeNum { get; set; }
}