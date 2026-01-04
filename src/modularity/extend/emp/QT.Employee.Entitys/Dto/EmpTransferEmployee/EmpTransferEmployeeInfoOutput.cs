using QT.Common.Models;

namespace QT.Emp.Entitys.Dto.EmpTransferEmployee;

/// <summary>
/// 调岗管理输出参数.
/// </summary>
public class EmpTransferEmployeeInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 员工id.
    /// </summary>
    public string employeeId { get; set; }

    /// <summary>
    /// 调入部门id.
    /// </summary>
    public string transferOrganizeId { get; set; }

    /// <summary>
    /// 调入岗位id.
    /// </summary>
    public string transferPositionId { get; set; }

    /// <summary>
    /// 调岗原因.
    /// </summary>
    public string reason { get; set; }

    /// <summary>
    /// 附件.
    /// </summary>
    public List<FileControlsModel> attachment { get; set; }

    /// <summary>
    /// 调岗日期.
    /// </summary>
    public DateTime? transferTime { get; set; }

}