using QT.Common.Filter;

namespace QT.JZRC.Entitys.Dto.JzrcOrder;

/// <summary>
/// 建筑人才平台订单管理列表查询输入
/// </summary>
public class JzrcOrderListQueryInput : PageInputBase
{
    /// <summary>
    /// 选择导出数据key.
    /// </summary>
    public string selectKey { get; set; }

    /// <summary>
    /// 导出类型.
    /// </summary>
    public int dataType { get; set; }

    /// <summary>
    /// 订单编号.
    /// </summary>
    public string orderNo { get; set; }

    /// <summary>
    /// 客户经理id.
    /// </summary>
    public string managerId { get; set; }

    /// <summary>
    /// 人才姓名
    /// </summary>
    public string talentIdName { get; set; }


    /// <summary>
    /// 企业姓名
    /// </summary>
    public string companyIdName { get; set; }

}