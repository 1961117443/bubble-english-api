using QT.Logistics.Entitys.Dto.LogProductcheckRecord;

namespace QT.Logistics.Entitys.Dto.LogProductcheckMaster;

/// <summary>
/// 盘点记录主表输出参数.
/// </summary>
public class LogProductcheckMasterInfoOutput
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
    /// 盘点记录.
    /// </summary>
    public List<LogProductcheckRecordInfoOutput> logProductcheckRecordList { get; set; }


    /// <summary>
    /// 审核时间.
    /// </summary>
    public DateTime? auditTime { get; set; }

    /// <summary>
    /// 审核人.
    /// </summary>
    public string auditUserIdName { get; set; }

}