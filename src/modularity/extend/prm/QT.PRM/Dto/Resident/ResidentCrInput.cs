using QT.DependencyInjection;
using QT.PRM.Entitys;
using System.ComponentModel.DataAnnotations;

namespace QT.PRM.Dto.Resident;

/// <summary>
/// 住户创建输入
/// </summary>
[SuppressSniffer]
public class ResidentCrInput
{
    /// <summary>
    /// 姓名
    /// </summary>
    [Required]
    [StringLength(50, ErrorMessage = "姓名不能超过50字符")]
    public string name { get; set; }

    /// <summary>
    /// 身份证号
    /// </summary>
    [RegularExpression(@"^\d{17}[\dXx]$", ErrorMessage = "身份证号格式错误")]
    public string idCard { get; set; }

    /// <summary>
    /// 手机号
    /// </summary>
    [Required]
    [Phone(ErrorMessage = "手机号格式错误")]
    public string phone { get; set; }

    /// <summary>
    /// 住户类型
    /// </summary>
    [Required]
    public ResidentType residentType { get; set; }

    /// <summary>
    /// 房间ID
    /// </summary>
    [Required]
    public string roomId { get; set; }

    /// <summary>
    /// 紧急联系人信息
    /// </summary>
    public List<EmergencyContactDto> emergencyContact { get; set; }
}

/// <summary>
/// 紧急联系人信息
/// </summary>
public class EmergencyContactDto
{
    /// <summary>
    /// 紧急联系人姓名
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 紧急联系人电话
    /// </summary>
    public string phone { get; set; }
}
