using QT.Common.Contracts;
using SqlSugar;

namespace QT.Iot.Application.Entity;

/// <summary>
/// 营销管理-项目管理-分配用户
/// </summary>
[SugarTable("crm_project_user")]
public class CrmProjectUserEntity : CUDEntityBase
{
    /// <summary>
    /// 项目id
    /// </summary>
    [SugarColumn(ColumnName = "Pid")]
    public string Pid { get; set; }
 

    /// <summary>
    /// 用户id
    /// </summary>
    [SugarColumn(ColumnName = "Uid")]
    public string Uid { get; set; }
}
