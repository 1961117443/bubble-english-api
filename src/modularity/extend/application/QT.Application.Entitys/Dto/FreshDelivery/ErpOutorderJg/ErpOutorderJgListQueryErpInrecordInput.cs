using QT.Common.Filter;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpOrderXs;

/// <summary>
/// 订单信息列表查询输入
/// </summary>
public class ErpOutorderJgListQueryErpInrecordInput : PageInputBase
{
    /// <summary>
    /// 所属公司
    /// </summary>
    public string oid { get; set; }

    /// <summary>
    /// 关联的商品
    /// </summary>
    public string rid { get; set; }

    /// <summary>
    /// 创建时间 范围查询
    /// </summary>
    public string creatorTimeRange { get; set; }
}