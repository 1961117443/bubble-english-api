namespace QT.Application.Entitys.Dto.FreshDelivery.ErpProductcheck;

/// <summary>
/// 盘点记录主表输入参数.
/// </summary>
public class ErpProductcheckMasterListOutput
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
    /// 盘点日期.
    /// </summary>
    public DateTime? checkTime { get; set; }

    /// <summary>
    /// 订单编号.
    /// </summary>
    public string no { get; set; }

    /// <summary>
    /// 公司名称.
    /// </summary>
    public string oidName { get; set; }

    /// <summary>
    /// 审核时间.
    /// </summary>
    public DateTime? auditTime { get; set; }
}