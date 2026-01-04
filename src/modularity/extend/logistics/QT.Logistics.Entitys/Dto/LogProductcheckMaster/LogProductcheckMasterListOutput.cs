namespace QT.Logistics.Entitys.Dto.LogProductcheckMaster;

/// <summary>
/// 盘点记录主表输入参数.
/// </summary>
public class LogProductcheckMasterListOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 盘点日期.
    /// </summary>
    public DateTime? checkTime { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string remark { get; set; }

    /// <summary>
    /// 盘点仓库ID.
    /// </summary>
    public string storeRomeId { get; set; }

    /// <summary>
    /// 盘点单号.
    /// </summary>
    public string no { get; set; }


    /// <summary>
    /// 盘点仓库.
    /// </summary>
    public string storeRomeIdName { get; set; }

    /// <summary>
    /// 审核时间.
    /// </summary>
    public DateTime? auditTime { get; set; }

    /// <summary>
    /// 审核人.
    /// </summary>
    public string auditUserIdName { get; set; }
}