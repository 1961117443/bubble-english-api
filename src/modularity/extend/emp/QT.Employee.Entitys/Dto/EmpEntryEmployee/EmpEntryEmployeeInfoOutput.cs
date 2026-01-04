
namespace QT.Emp.Entitys.Dto.EmpEntryEmployee;

/// <summary>
/// 入职管理输出参数.
/// </summary>
public class EmpEntryEmployeeInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

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