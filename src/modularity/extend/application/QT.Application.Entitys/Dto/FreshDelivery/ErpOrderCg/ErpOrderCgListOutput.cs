using QT.Application.Entitys.Enum.FreshDelivery;
using System.ComponentModel.DataAnnotations;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpOrderCg;

/// <summary>
/// 订单信息输入参数.
/// </summary>
public class ErpOrderCgListOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 订单编号.
    /// </summary>
    [Display(Name = "订单编号")]
    public string no { get; set; }

    /// <summary>
    /// 代下单人员.
    /// </summary>
    public string createUid { get; set; }

    /// <summary>
    /// 代下单时间.
    /// </summary>
    [Display(Name = "代下单时间")]
    public DateTime? createTime { get; set; }

    /// <summary>
    /// 客户ID.
    /// </summary>
    public string cid { get; set; }

    /// <summary>
    /// 约定送货时间.
    /// </summary>
    [Display(Name = "约定送货时间")]
    public DateTime? posttime { get; set; }

    /// <summary>
    /// 客户名称
    /// </summary>
    [Display(Name = "客户名称")]
    public string cidName { get; set; }


    /// <summary>
    /// 订单状态
    /// </summary>
    public OrderStateEnum state { get; set; }

    /// <summary>
    /// 星期几
    /// </summary>
    public string dayOfWeek { get; set; }

    /// <summary>
    /// 送货车辆
    /// </summary>
    public string deliveryCar { get; set; }

    /// <summary>
    /// 送货人
    /// </summary>
    public string deliveryManIdName { get; set; }

    /// <summary>
    /// 订单金额
    /// </summary>
    public decimal amount { get; set; }

    /// <summary>
    /// 餐别
    /// </summary>
    public string diningType { get; set; }

    /// <summary>
    /// 是否子单
    /// </summary>
    public bool isSub { get; set; }

    /// <summary>
    /// 订单备注
    /// </summary>
    public string remark { get; set; }
}

public class ErpOrderCgListExportOutput
{
    /// <summary>
    /// 订单编号.
    /// </summary>
    [Display(Name = "订单编号")]
    public string no { get; set; }

    /// <summary>
    /// 代下单人员.
    /// </summary>
    public string createUid { get; set; }

    /// <summary>
    /// 代下单时间.
    /// </summary>
    [Display(Name = "代下单时间")]
    public DateTime? createTime { get; set; }

    /// <summary>
    /// 客户ID.
    /// </summary>
    public string cid { get; set; }

    /// <summary>
    /// 约定送货时间.
    /// </summary>
    [Display(Name = "约定送货时间")]
    public DateTime? posttime { get; set; }

    /// <summary>
    /// 客户名称
    /// </summary>
    [Display(Name = "客户名称")]
    public string cidName { get; set; }


    /// <summary>
    /// 订单状态
    /// </summary>
    public OrderStateEnum state { get; set; }

    /// <summary>
    /// 星期几
    /// </summary>
    public string dayOfWeek { get; set; }

    /// <summary>
    /// 送货车辆
    /// </summary>
    public string deliveryCar { get; set; }

    /// <summary>
    /// 送货人
    /// </summary>
    public string deliveryManIdName { get; set; }



    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }


    /// <summary>
    /// 订单主键.
    /// </summary>
    public string fid { get; set; }

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
    /// 商品规格.
    /// </summary>
    public string midName { get; set; }

    /// <summary>
    /// 商品单价.
    /// </summary>
    public decimal salePrice { get; set; }

    /// <summary>
    /// 订单备注
    /// </summary>
    [Display(Name = "订单备注")]
    public string remark { get; set; }

    /// <summary>
    /// 商品名称.
    /// </summary>
    public string productName { get; set; }

    /// <summary>
    /// 分拣时间.
    /// </summary>
    public DateTime? sorterTime { get; set; }

    /// <summary>
    /// 分拣人员.
    /// </summary>
    public string sorterUserId { get; set; }

    /// <summary>
    /// 分拣备注.
    /// </summary>
    public string sorterDes { get; set; }

    /// <summary>
    /// 分拣状态.
    /// </summary>
    public string sorterState { get; set; }

   

    /// <summary>
    /// 送达时间.
    /// </summary>
    public DateTime? receiveTime { get; set; }

    public string sorterUserName { get; set; }

    /// <summary>
    /// 规格单位
    /// </summary>
    [Display(Name = "单位")]
    public string midUnit { get; set; }

    /// <summary>
    /// 主单位数量比.
    /// </summary>
    public decimal? ratio { get; set; }

    /// <summary>
    /// 商品一级分类
    /// </summary>
    public string rootProducttype { get; set; }

    /// <summary>
    /// 图片集合
    /// </summary>
    public IEnumerable<string> imageList { get; set; }

    /// <summary>
    /// 商品id
    /// </summary>
    public string productId { get; set; }

    /// <summary>
    /// 订单编号
    /// </summary>
    public string orderNo { get; set; }


    /// <summary>
    /// 客户单位
    /// </summary>
    public string customerUnit { get; set; }

    /// <summary>
    /// 客户名称
    /// </summary>
    public string customerName { get; set; }

    /// <summary>
    /// 退回数量
    /// </summary>
    public decimal rejectNum { get; set; }

    /// <summary>
    /// 分拣数量.
    /// </summary>
    public decimal fjNum { get; set; }

    /// <summary>
    /// 子单实际分拣.
    /// </summary>
    public decimal cnum1 { get; set; }


    /// <summary>
    /// 子单分拣数量.
    /// </summary>
    public decimal cfjNum { get; set; }
     

    /// <summary>
    /// 分拣完成时间.
    /// </summary>
    public DateTime? sorterFinishTime { get; set; }

    /// <summary>
    /// 餐别
    /// </summary>
    public string diningType { get; set; }

    public int? order { get; set; }

    /// <summary>
    /// 商品金额.
    /// </summary>
    public decimal damount { get; set; }

    /// <summary>
    /// 打印别名
    /// </summary>
    public string printAlais { get; set; }

    /// <summary>
    /// 订单明细备注
    /// </summary>
    [Display(Name = "明细备注")]
    public string itRemark { get; set; }

    /// <summary>
    /// 客户类型
    /// </summary>
    public string cidType { get; set; }
}


public class ErpOrderCgListTaskExportOutput
{

    /// <summary>
    /// 客户名称
    /// </summary>
    [Display(Name = "客户名称")]
    public string cidName { get; set; }

    /// <summary>
    /// 订单编号.
    /// </summary>
    [Display(Name = "订单编号")]
    public string no { get; set; }


    /// <summary>
    /// 约定送货时间.
    /// </summary>
    [Display(Name = "约定送货时间")]
    public DateTime? posttime { get; set; }

    /// <summary>
    /// 代下单时间.
    /// </summary>
    [Display(Name = "下单时间")]
    public DateTime? createTime { get; set; }


    /// <summary>
    /// 订单状态
    /// </summary>
    [Display(Name = "订单状态")]
    public OrderStateEnum state { get; set; }

    /// <summary>
    /// 星期几
    /// </summary>
    public string dayOfWeek { get; set; }

    

    /// <summary>
    /// 送货人
    /// </summary>
    [Display(Name = "送货人")]
    public string deliveryManIdName { get; set; }

    /// <summary>
    /// 送货车辆
    /// </summary>
    [Display(Name = "送货车辆")]
    public string deliveryCar { get; set; }

    /////////从表

    /// <summary>
    /// 商品名称.
    /// </summary>
    [Display(Name = "商品名称")]
    public string productName { get; set; }

    /// <summary>
    /// 商品规格.
    /// </summary>
    [Display(Name = "商品规格")]
    public string midName { get; set; }
    /// <summary>
    /// 商品一级分类
    /// </summary>
    [Display(Name = "一级分类")]
    public string rootProducttype { get; set; }

    /// <summary>
    /// 订单数量.
    /// </summary>
    [Display(Name = "订单数量")]
    public decimal num { get; set; }

    /// <summary>
    /// 商品单价.
    /// </summary>
    [Display(Name = "单价")]
    public decimal salePrice { get; set; }

    /// <summary>
    /// 订单总价.
    /// </summary>
    [Display(Name = "订单总价")]
    public decimal amount { get; set; }

    /// <summary>
    /// 配送数量.
    /// </summary>
    [Display(Name = "分拣数量")]
    public decimal num1 { get; set; }


    /// <summary>
    /// 订单备注
    /// </summary>
    [Display(Name = "订单备注")]
    public string remark { get; set; }

    /// <summary>
    /// 打印别名
    /// </summary>
    [Display(Name = "打印别名")]
    public string printAlais { get; set; }
}

public class ErpOrderCgListHisExportOutput
{

    /// <summary>
    /// 客户名称
    /// </summary>
    [Display(Name = "客户名称")]
    public string cidName { get; set; }

    /// <summary>
    /// 订单编号.
    /// </summary>
    [Display(Name = "订单编号")]
    public string no { get; set; }


    /// <summary>
    /// 约定送货时间.
    /// </summary>
    [Display(Name = "约定送货时间")]
    public DateTime? posttime { get; set; }

    /// <summary>
    /// 代下单时间.
    /// </summary>
    [Display(Name = "下单时间")]
    public DateTime? createTime { get; set; }


    /// <summary>
    /// 订单状态
    /// </summary>
    [Display(Name = "订单状态")]
    public string state { get; set; }

    /// <summary>
    /// 星期几
    /// </summary>
    public string dayOfWeek { get; set; }



    /// <summary>
    /// 送货人
    /// </summary>
    [Display(Name = "送货人")]
    public string deliveryManIdName { get; set; }

    /// <summary>
    /// 送货车辆
    /// </summary>
    [Display(Name = "送货车辆")]
    public string deliveryCar { get; set; }

    /////////从表

    /// <summary>
    /// 商品名称.
    /// </summary>
    [Display(Name = "商品名称")]
    public string productName { get; set; }

    /// <summary>
    /// 商品规格.
    /// </summary>
    [Display(Name = "商品规格")]
    public string midName { get; set; }

    /// <summary>
    /// 单位
    /// </summary>
    [Display(Name = "单位")]
    public string midUnit { get; set; }

    /// <summary>
    /// 商品一级分类
    /// </summary>
    [Display(Name = "一级分类")]
    public string rootProducttype { get; set; }


    /// <summary>
    /// 配送数量.
    /// </summary>
    [Display(Name = "配送数量")]
    public decimal num1 { get; set; }

    /// <summary>
    /// 配送总价.
    /// </summary>
    [Display(Name = "配送总价")]
    public decimal amount1 { get; set; }

    /// <summary>
    /// 复核数量.
    /// </summary>
    [Display(Name = "复核数量")]
    public decimal num2 { get; set; }

    /// <summary>
    /// 复核总价.
    /// </summary>
    [Display(Name = "复核总价")]
    public decimal amount2 { get; set; }

    /// <summary>
    /// 复核时间.
    /// </summary>
    [Display(Name = "复核时间")]
    public DateTime? checkTime { get; set; }

    /// <summary>
    /// 订单数量.
    /// </summary>
    [Display(Name = "订单数量")]
    public decimal num { get; set; }

    /// <summary>
    /// 商品单价.
    /// </summary>
    [Display(Name = "单价")]
    public decimal salePrice { get; set; }

    /// <summary>
    /// 金额.
    /// </summary>
    [Display(Name = "金额")]
    public decimal damount { get; set; }

    /// <summary>
    /// 订单总价.
    /// </summary>
    [Display(Name = "订单总价")]
    public decimal amount { get; set; }

    /// <summary>
    /// 订单备注
    /// </summary>
    [Display(Name = "订单备注")]
    public string remark { get; set; }


    /// <summary>
    /// 订单明细备注
    /// </summary>
    [Display(Name = "明细备注")]
    public string itRemark { get; set; }
}

public class ErpOrderCgListExportData
{
    /// <summary>
    /// 约定送货时间.
    /// </summary>
    [Display(Name = "约定送货时间")]
    public string posttime { get; set; }


    /// <summary>
    /// 订单编号.
    /// </summary>
    [Display(Name = "订单编号")]
    public string no { get; set; }

    /// <summary>
    /// 客户名称
    /// </summary>
    [Display(Name = "客户名称")]
    public string cidName { get; set; }


    /// <summary>
    /// 商品名称.
    /// </summary>
    [Display(Name = "商品名称")]
    public string productName { get; set; }


    /// <summary>
    /// 商品一级分类
    /// </summary>
    [Display(Name = "一级分类")]
    public string rootProducttype { get; set; }



    /// <summary>
    /// 商品规格.
    /// </summary>
    [Display(Name = "商品规格")]
    public string midName { get; set; }


    /// <summary>
    /// 单位
    /// </summary>
    [Display(Name = "单位")]
    public string midUnit { get; set; }

    /// <summary>
    /// 商品单价.
    /// </summary>
    [Display(Name = "单价")]
    public decimal salePrice { get; set; }

    ///// <summary>
    ///// 订单数量.
    ///// </summary>
    //[Display(Name = "订单数量")]
    //public decimal num { get; set; }

    /// <summary>
    /// 分拣数量.
    /// </summary>
    [Display(Name = "数量")]
    public decimal num1 { get; set; }

    /// <summary>
    /// 金额.
    /// </summary>
    [Display(Name = "金额")]
    public decimal damount { get; set; }

    /// <summary>
    /// 代下单时间.
    /// </summary>
    //[Display(Name = "下单时间")]
    public DateTime? createTime { get; set; }


    /// <summary>
    /// 订单状态
    /// </summary>
    //[Display(Name = "订单状态")]
    public string state { get; set; }

    /// <summary>
    /// 星期几
    /// </summary>
    public string dayOfWeek { get; set; }



    /// <summary>
    /// 送货人
    /// </summary>
    //[Display(Name = "送货人")]
    public string deliveryManIdName { get; set; }

    /// <summary>
    /// 送货车辆
    /// </summary>
    //[Display(Name = "送货车辆")]
    public string deliveryCar { get; set; }

    /////////从表


    ///// <summary>
    ///// 配送数量.
    ///// </summary>
    ////[Display(Name = "配送数量")]
    //public decimal num1 { get; set; }

    /// <summary>
    /// 配送总价.
    /// </summary>
    //[Display(Name = "配送总价")]
    public decimal amount1 { get; set; }

    /// <summary>
    /// 复核数量.
    /// </summary>
    //[Display(Name = "复核数量")]
    public decimal num2 { get; set; }

    /// <summary>
    /// 复核总价.
    /// </summary>
    //[Display(Name = "复核总价")]
    public decimal amount2 { get; set; }

    /// <summary>
    /// 复核时间.
    /// </summary>
    //[Display(Name = "复核时间")]
    public DateTime? checkTime { get; set; }



  

    /// <summary>
    /// 订单总价.
    /// </summary>
    //[Display(Name = "订单总价")]
    public decimal amount { get; set; }

    /// <summary>
    /// 订单备注
    /// </summary>
    [Display(Name = "订单备注")]
    public string remark { get; set; }


    /// <summary>
    /// 订单明细备注
    /// </summary>
    [Display(Name = "明细备注")]
    public string itRemark { get; set; }

    /// <summary>
    /// 餐别
    /// </summary>
    [Display(Name = "餐别")]
    public string diningType { get; set; }


    /// <summary>
    /// 客户类型
    /// </summary>
    [Display(Name = "客户类型")]
    public string cidType { get; set; }
}