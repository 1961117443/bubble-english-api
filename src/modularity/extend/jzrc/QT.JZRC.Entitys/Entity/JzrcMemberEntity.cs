using QT.Common.Const;
using QT.Common.Contracts;
using QT.JZRC.Entitys.Dto.AppService;
using SqlSugar;

namespace QT.JZRC.Entitys;

/// <summary>
/// 建筑平台会员信息实体.
/// </summary>
[SugarTable("jzrc_member")]
[Tenant(ClaimConst.TENANTID)]
public class JzrcMemberEntity :CUDEntityBase
{
    /// <summary>
    /// 账户（手机号码）.
    /// </summary>
    [SugarColumn(ColumnName = "F_Account")]
    public string Account { get; set; }

    /// <summary>
    /// 呢称.
    /// </summary>
    [SugarColumn(ColumnName = "F_NickName")]
    public string NickName { get; set; }

    /// <summary>
    /// 头像.
    /// </summary>
    [SugarColumn(ColumnName = "F_HeadIcon")]
    public string HeadIcon { get; set; }

    /// <summary>
    /// 密码.
    /// </summary>
    [SugarColumn(ColumnName = "F_Password")]
    public string Password { get; set; }

    /// <summary>
    /// 秘钥.
    /// </summary>
    [SugarColumn(ColumnName = "F_Secretkey")]
    public string Secretkey { get; set; }

    /// <summary>
    /// 首次登录时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_FirstLogTime")]
    public DateTime? FirstLogTime { get; set; }

    /// <summary>
    /// 首次登录IP.
    /// </summary>
    [SugarColumn(ColumnName = "F_FirstLogIP")]
    public string FirstLogIp { get; set; }

    /// <summary>
    /// 前次登录时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_PrevLogTime")]
    public DateTime? PrevLogTime { get; set; }

    /// <summary>
    /// 前次登录IP.
    /// </summary>
    [SugarColumn(ColumnName = "F_PrevLogIP")]
    public string PrevLogIp { get; set; }

    /// <summary>
    /// 最后登录时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_LastLogTime")]
    public DateTime? LastLogTime { get; set; }

    /// <summary>
    /// 最后登录IP.
    /// </summary>
    [SugarColumn(ColumnName = "F_LastLogIP")]
    public string LastLogIp { get; set; }

    /// <summary>
    /// 登录成功次数.
    /// </summary>
    [SugarColumn(ColumnName = "F_LogSuccessCount")]
    public int? LogSuccessCount { get; set; }

    /// <summary>
    /// 登录错误次数.
    /// </summary>
    [SugarColumn(ColumnName = "F_LogErrorCount")]
    public int? LogErrorCount { get; set; }

    /// <summary>
    /// 最后修改密码时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_ChangePasswordDate")]
    public DateTime? ChangePasswordDate { get; set; }

    /// <summary>
    /// 扩展属性.
    /// </summary>
    [SugarColumn(ColumnName = "F_PropertyJson")]
    public object PropertyJson { get; set; }

    /// <summary>
    /// 是否锁定.
    /// </summary>
    [SugarColumn(ColumnName = "F_LockMark")]
    public int? LockMark { get; set; }

    /// <summary>
    /// 解锁时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_UnlockTime")]
    public DateTime? UnlockTime { get; set; }

    /// <summary>
    /// 账号类型（0：人才，1：企业）.
    /// </summary>
    [SugarColumn(ColumnName = "F_Role")]
    public AppLoginUserRole Role { get; set; }

    /// <summary>
    /// 账户余额.
    /// </summary>
    [SugarColumn(ColumnName = "Amount")]
    public decimal Amount { get; set; }

    /// <summary>
    /// 保证金.
    /// </summary>
    [SugarColumn(ColumnName = "Margin")]
    public decimal Margin { get; set; }

    /// <summary>
    /// 关联的id（人才或者企业）.
    /// </summary>
    [SugarColumn(ColumnName = "RelationId")]
    public string RelationId { get; set; }
}