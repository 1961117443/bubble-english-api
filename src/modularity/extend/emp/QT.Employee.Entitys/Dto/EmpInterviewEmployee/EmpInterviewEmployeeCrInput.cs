
namespace QT.Emp.Entitys.Dto.EmpInterviewEmployee;

/// <summary>
/// 面试管理修改输入参数.
/// </summary>
public class EmpInterviewEmployeeCrInput
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
    /// 预计面试时间.
    /// </summary>
    public DateTime? interviewTime { get; set; }

}