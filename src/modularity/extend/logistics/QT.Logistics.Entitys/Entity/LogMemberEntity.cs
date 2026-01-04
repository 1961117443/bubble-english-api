using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Logistics.Entitys;

/// <summary>
/// 会员信息实体.
/// </summary>
[SugarTable("log_member")]
[Tenant(ClaimConst.TENANTID)]
public class LogMemberEntity : CUDEntityBase
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public override string Id { get; set; }

    /// <summary>
    /// 姓名.
    /// </summary>
    [SugarColumn(ColumnName = "Name")]
    public string Name { get; set; }

    /// <summary>
    /// 性别.
    /// </summary>
    [SugarColumn(ColumnName = "Gender")]
    public int? Gender { get; set; }

    /// <summary>
    /// 身份证号.
    /// </summary>
    [SugarColumn(ColumnName = "CardNumber")]
    public string CardNumber { get; set; }

    /// <summary>
    /// 出生年月.
    /// </summary>
    [SugarColumn(ColumnName = "BirthDate")]
    public DateTime? BirthDate { get; set; }

    /// <summary>
    /// 头像.
    /// </summary>
    [SugarColumn(ColumnName = "Avatar")]
    public string Avatar { get; set; }

    /// <summary>
    /// 手机号码.
    /// </summary>
    [SugarColumn(ColumnName = "PhoneNumber")]
    public string PhoneNumber { get; set; }

    /// <summary>
    /// 登录密码.
    /// </summary>
    [SugarColumn(ColumnName = "F_Password")]
    public string Password { get; set; }

    /// <summary>
    /// 邮箱.
    /// </summary>
    [SugarColumn(ColumnName = "Email")]
    public string Email { get; set; }

    /// <summary>
    /// 地址.
    /// </summary>
    [SugarColumn(ColumnName = "Address")]
    public string Address { get; set; }
}