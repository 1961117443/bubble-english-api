namespace QT.Application.Entitys.Dto.FreshDelivery.ErpProductcheck;

/// <summary>
/// 盘点记录主表输出参数.
/// </summary>
public class ErpProductcheckMasterInfoOutput
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
    /// 盘点记录.
    /// </summary>
    public List<ErpProductcheckInfoOutput> erpProductcheckList { get; set; }

}