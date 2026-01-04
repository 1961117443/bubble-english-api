namespace QT.Logistics.Entitys.Dto.LogMember;

/// <summary>
/// 会员信息输入参数.
/// </summary>
public class LogMemberListOutput
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
    public int? gender { get; set; }

    /// <summary>
    /// 身份证号.
    /// </summary>
    public string cardNumber { get; set; }

    /// <summary>
    /// 出生年月.
    /// </summary>
    public DateTime? birthDate { get; set; }

    /// <summary>
    /// 手机号码.
    /// </summary>
    public string phoneNumber { get; set; }

    /// <summary>
    /// 邮箱.
    /// </summary>
    public string email { get; set; }

    /// <summary>
    /// 地址.
    /// </summary>
    public string address { get; set; }

}