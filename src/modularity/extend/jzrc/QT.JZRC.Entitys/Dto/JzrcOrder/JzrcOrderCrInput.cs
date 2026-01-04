
namespace QT.JZRC.Entitys.Dto.JzrcOrder;

/// <summary>
/// 建筑人才平台订单管理修改输入参数.
/// </summary>
public class JzrcOrderCrInput
{
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

}