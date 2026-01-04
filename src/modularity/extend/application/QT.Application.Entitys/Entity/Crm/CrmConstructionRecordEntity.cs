using QT.Common.Contracts;
using SqlSugar;

namespace QT.Iot.Application.Entity;

/// <summary>
/// 营销管理-实施记录
/// </summary>
[SugarTable("crm_construction_record")]
public class CrmConstructionRecordEntity : CUDEntityBase
{
    /// <summary>
    /// 项目id
    /// </summary>
    [SugarColumn(ColumnName = "Pid")]
    public string Pid { get; set; }
 

    /// <summary>
    /// 实施时间
    /// </summary>
    [SugarColumn(ColumnName = "ConstructionTime")]
    public DateTime? ConstructionTime { get; set; }

    /// <summary>
    /// 实施内容
    /// </summary>
    [SugarColumn(ColumnName = "Content")]
    public string Content { get; set; }


    /// <summary>
    /// 附件
    /// </summary>
    [SugarColumn(ColumnName = "Attachment")]
    public string Attachment { get; set; }
}
