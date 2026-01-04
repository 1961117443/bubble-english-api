
namespace QT.Emp.Entitys.Dto.EmpEntryEmployee;

/// <summary>
/// 入职管理修改输入参数.
/// </summary>
public class EmpEntryEmployeeCrInput
{
    /// <summary>
    /// 姓名.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 手机号码.
    /// </summary>
    public string mobile { get; set; }

    /// <summary>
    /// 部门id.
    /// </summary>
    public string organizeId { get; set; }

    /// <summary>
    /// 岗位id.
    /// </summary>
    public string positionId { get; set; }

    /// <summary>
    /// 预计入职时间.
    /// </summary>
    public DateTime? confrimJoinTime { get; set; }

}