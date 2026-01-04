using QT.Common.Models;

namespace QT.Logistics.Entitys.Dto.LogMember;

/// <summary>
/// 会员信息输出参数.
/// </summary>
public class LogMemberInfoOutput
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
    /// 身份证号.
    /// </summary>
    public string cardNumber { get; set; }

    /// <summary>
    /// 出生年月.
    /// </summary>
    public DateTime? birthDate { get; set; }

    /// <summary>
    /// 头像.
    /// </summary>
    public List<FileControlsModel> avatar { get; set; }

    /// <summary>
    /// 手机号码.
    /// </summary>
    public string phoneNumber { get; set; }

    /// <summary>
    /// 登录密码.
    /// </summary>
    public string password { get; set; }

    /// <summary>
    /// 邮箱.
    /// </summary>
    public string email { get; set; }

    /// <summary>
    /// 地址.
    /// </summary>
    public string address { get; set; }

}