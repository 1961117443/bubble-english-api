namespace QT.JZRC.Entitys.Dto.JzrcOrder;

/// <summary>
/// 建筑人才平台订单管理输入参数.
/// </summary>
public class JzrcOrderListOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 订单编号.
    /// </summary>
    public string orderNo { get; set; }

    /// <summary>
    /// 人才id.
    /// </summary>
    public string talentId { get; set; }

    /// <summary>
    /// 企业id.
    /// </summary>
    public string companyId { get; set; }

    /// <summary>
    /// 客户经理id.
    /// </summary>
    public string managerId { get; set; }

    /// <summary>
    /// 订单金额.
    /// </summary>
    public decimal amount { get; set; }

    /// <summary>
    /// 人才应分.
    /// </summary>
    public decimal talentShare { get; set; }

    /// <summary>
    /// 企业应分.
    /// </summary>
    public decimal companyShare { get; set; }

    /// <summary>
    /// 平台应分.
    /// </summary>
    public decimal platformShare { get; set; }


    /// <summary>
    /// 人才id.
    /// </summary>
    public string talentIdName { get; set; }

    /// <summary>
    /// 企业id.
    /// </summary>
    public string companyIdName { get; set; }

    /// <summary>
    /// 客户经理id.
    /// </summary>
    public string managerIdName { get; set; }

}