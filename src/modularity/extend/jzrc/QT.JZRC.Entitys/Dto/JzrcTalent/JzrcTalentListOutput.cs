namespace QT.JZRC.Entitys.Dto.JzrcTalent;

/// <summary>
/// 人才信息输入参数.
/// </summary>
public class JzrcTalentListOutput
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
    /// 性别.
    /// </summary>
    public string gender { get; set; }

    /// <summary>
    /// 区域.
    /// </summary>
    public string region { get; set; }

    /// <summary>
    /// 注册类别.
    /// </summary>
    public string registrationCategory { get; set; }

    /// <summary>
    /// 专业1.
    /// </summary>
    public string major1 { get; set; }

    /// <summary>
    /// 专业2.
    /// </summary>
    public string major2 { get; set; }

    /// <summary>
    /// 专业3.
    /// </summary>
    public string major3 { get; set; }

    /// <summary>
    /// 薪资要求.
    /// </summary>
    public string salaryRequirement { get; set; }

    /// <summary>
    /// 客户经理
    /// </summary>
    public string managerIdName { get; set; }

    /// <summary>
    /// 是否签约入驻
    /// </summary>
    public bool signed { get; set; }

}