namespace QT.JXC.Entitys.Dto.ErpOrderdetailReject;

/// <summary>
/// 订单退货输入参数.
/// </summary>
public class ErpOrderdetailRejectListOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 订单明细id.
    /// </summary>
    public string orderDetailId { get; set; }

    /// <summary>
    /// 退货数量.
    /// </summary>
    public decimal num { get; set; }

    /// <summary>
    /// 退货金额.
    /// </summary>
    public decimal amount { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string remark { get; set; }


    /// <summary>
    /// 商品规格.
    /// </summary>
    public string midName { get; set; }

    /// <summary>
    /// 商品单价.
    /// </summary>
    public decimal salePrice { get; set; }

    /// <summary>
    /// 商品名称.
    /// </summary>
    public string productName { get; set; }
     

    /// <summary>
    /// 规格单位
    /// </summary>
    public string midUnit { get; set; }
      

    /// <summary>
    /// 订单编号
    /// </summary>
    public string orderNo { get; set; }
     

    /// <summary>
    /// 客户名称
    /// </summary>
    public string customerName { get; set; }

    /// <summary>
    /// 配送时间
    /// </summary>
    public DateTime? posttime { get; set; }

    /// <summary>
    /// 配送数量
    /// </summary>
    public decimal num1 { get; set; }

    /// <summary>
    /// 审核时间
    /// </summary>
    public DateTime? auditTime { get; set; }
}


/// <summary>
/// 订单商品表输出参数.
/// </summary>
public class ErpOrderdetailRejectQueryOrderListOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
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

    ///// <summary>
    ///// 复核数量.
    ///// </summary>
    //public decimal num2 { get; set; }

    ///// <summary>
    ///// 复核总价.
    ///// </summary>
    //public decimal amount2 { get; set; }

    ///// <summary>
    ///// 复核时间.
    ///// </summary>
    //public DateTime? checkTime { get; set; }

    /// <summary>
    /// 订单数量.
    /// </summary>
    public decimal num { get; set; }

    ///// <summary>
    ///// 订单总价.
    ///// </summary>
    //public decimal amount { get; set; }


    /// <summary>
    /// 商品规格.
    /// </summary>
    public string midName { get; set; }

    /// <summary>
    /// 商品单价.
    /// </summary>
    public decimal salePrice { get; set; }

    ///// <summary>
    ///// 订单备注
    ///// </summary>
    //public string remark { get; set; }

    /// <summary>
    /// 商品名称.
    /// </summary>
    public string productName { get; set; }

    ///// <summary>
    ///// 分拣时间.
    ///// </summary>
    //public DateTime? sorterTime { get; set; }

    ///// <summary>
    ///// 分拣人员.
    ///// </summary>
    //public string sorterUserId { get; set; }

    ///// <summary>
    ///// 分拣备注.
    ///// </summary>
    //public string sorterDes { get; set; }

    ///// <summary>
    ///// 分拣状态.
    ///// </summary>
    //public string sorterState { get; set; }

    ///// <summary>
    ///// 送达状态.
    ///// </summary>
    //public int receiveState { get; set; }

    ///// <summary>
    ///// 送达时间.
    ///// </summary>
    //public DateTime? receiveTime { get; set; }

    //public string sorterUserName { get; set; }

    /// <summary>
    /// 规格单位
    /// </summary>
    public string midUnit { get; set; }

    ///// <summary>
    ///// 主单位数量比.
    ///// </summary>
    //public decimal? ratio { get; set; }

    ///// <summary>
    ///// 商品一级分类
    ///// </summary>
    //public string rootProducttype { get; set; }

    ///// <summary>
    ///// 图片集合
    ///// </summary>
    //public IEnumerable<string> imageList { get; set; }

    /// <summary>
    /// 商品id
    /// </summary>
    public string productId { get; set; }

    /// <summary>
    /// 订单编号
    /// </summary>
    public string orderNo { get; set; }


    ///// <summary>
    ///// 客户单位
    ///// </summary>
    //public string customerUnit { get; set; }

    /// <summary>
    /// 客户名称
    /// </summary>
    public string customerName { get; set; }

    /// <summary>
    /// 配送时间
    /// </summary>
    public DateTime? posttime { get; set; }
}