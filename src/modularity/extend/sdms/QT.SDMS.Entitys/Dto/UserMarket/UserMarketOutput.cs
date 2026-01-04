using QT.SDMS.Entitys.Dto.Order;

namespace QT.SDMS.Entitys.Dto.UserMarket;

/// <summary>
/// 我的部门
/// </summary>
public class UserMarketOutput
{
    ///// <summary>
    ///// 个人累计业绩
    ///// </summary>
    //public decimal totalAmount { get; set; }


    ///// <summary>
    ///// 个人本月业绩
    ///// </summary>
    //public decimal monAmount { get; set; }

    ///// <summary>
    ///// 部门累计业绩
    ///// </summary>
    //public decimal deptTotalAmount { get; set; }


    ///// <summary>
    ///// 部门本月业绩
    ///// </summary>
    //public decimal deptMonAmount { get; set; }

    /// <summary>
    /// 下级业务员
    /// </summary>
    public List<UserMarketerOutput> crmUserMarketers { get; set; }

    /// <summary>
    /// 我的订单列表
    /// </summary>
    public List<UserMarketerOrderOutput> myOrderList { get; set; }


    /// <summary>
    /// panel统计
    /// </summary>
    public List<object> panels { get; set; }

}

/// <summary>
/// 我的下级业务员
/// </summary>

public class UserMarketerOutput
{
    public  string id { get; set; }

    /// <summary>
    /// 营销人员
    /// </summary>
    public string userName { get; set; }

    /// <summary>
    /// 累计业绩
    /// </summary>
    public decimal totalAmount { get; set; }

    /// <summary>
    /// 本月业绩
    /// </summary>
    public decimal monAmount { get; set; }

    /// <summary>
    /// 下级部门累计业绩
    /// </summary>
    public decimal childTotalAmount { get; set; }

    /// <summary>
    /// 下级部门本月业绩
    /// </summary>
    public decimal childMonAmount { get; set; }

    /// <summary>
    /// 本月提成（给部门主管创造的提成）
    /// </summary>
    public decimal commission { get; set; }

    /// <summary>
    /// 累计提成（给部门主管创造的提成）
    /// </summary>
    public decimal monCommission { get; set; }


    ///// <summary>
    ///// 部门本月提成
    ///// </summary>
    //public decimal deptCommission { get; set; }

    ///// <summary>
    ///// 部门累计提成
    ///// </summary>
    //public decimal monDeptCommission { get; set; }

    /// <summary>
    /// 孙级业绩
    /// </summary>
    public decimal grandAmount { get; set; }

    /// <summary>
    /// 孙级当月业绩
    /// </summary>
    public decimal monGrandAmount { get; set; }
}


public class UserMarketerOrderOutput: OrderListOutput
{
    public string no { get; set; }

    public DateTime? orderDate { get; set; }

    /// <summary>
    /// 订单业务经理
    /// </summary>
    public string orderUserId { get; set; }

    public decimal amount { get; set; }

    public string remark { get; set; }

    public decimal commission { get; set; }

    /// <summary>
    /// 订单归属（业务经理）
    /// </summary>
    public string owner { get; set; }
}