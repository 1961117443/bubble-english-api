namespace QT.JXC.Entitys.Dto;

/// <summary>
/// 盘点记录主表修改输入参数.
/// </summary>
public class ErpProductcheckMasterCrInput
{
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
    public List<ErpProductcheckCrInput> erpProductcheckList { get; set; }

}