
namespace QT.Emp.Entitys.Dto.EmpInterviewEmployee;

/// <summary>
/// 入职管理输出参数.
/// </summary>
public class EmpInterviewEmployeeInfoOutput
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
    public DateTime? interviewTime { get; set; }


    /// <summary>
    /// 计划入职日期.
    /// </summary>
    public DateTime? confrimJoinTime { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string remark { get; set; }

    /// <summary>
    /// 面试结果
    /// </summary>
    public string result { get; set; }



    /// <summary>
    /// 审核时间.
    /// </summary>
    public DateTime? auditTime { get; set; }

}