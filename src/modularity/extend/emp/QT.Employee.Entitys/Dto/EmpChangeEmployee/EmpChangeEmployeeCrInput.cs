
using QT.Common.Models;

namespace QT.Emp.Entitys.Dto.EmpDismissionEmployee;

/// <summary>
/// 员工变更输入参数.
/// </summary>
public class EmpChangeEmployeeCrInput
{
    /// <summary>
    /// 员工id.
    /// </summary>
    public string employeeId { get; set; }

    /// <summary>
    /// 变更类型
    /// </summary>
    public int changeType { get; set; }

    /// <summary>
    /// 变更日期.
    /// </summary>
    public DateTime? operateTime { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string remark { get; set; }


    //public string organizeId { get; set; }


    //public string positionId { get; set; }

    /// <summary>
    /// 扩展属性.
    /// </summary>
    public string propertyJson { get; set; }


    /// <summary>
    /// 附件.
    /// </summary>
    public List<FileControlsModel> attachment { get; set; }
}