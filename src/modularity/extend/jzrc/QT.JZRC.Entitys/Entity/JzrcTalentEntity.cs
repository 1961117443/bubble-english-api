using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.JZRC.Entitys;

/// <summary>
/// 人才信息实体.
/// </summary>
[SugarTable("jzrc_talent")]
[Tenant(ClaimConst.TENANTID)]
public class JzrcTalentEntity : CUDEntityBase
{
    /// <summary>
    /// 姓名.
    /// </summary>
    [SugarColumn(ColumnName = "Name")]
    public string Name { get; set; }

    /// <summary>
    /// 身份证.
    /// </summary>
    [SugarColumn(ColumnName = "IDCard")]
    public string IdCard { get; set; }

    /// <summary>
    /// 性别.
    /// </summary>
    [SugarColumn(ColumnName = "Gender")]
    public string Gender { get; set; }

    /// <summary>
    /// 手机.
    /// </summary>
    [SugarColumn(ColumnName = "MobilePhone")]
    public string MobilePhone { get; set; }

    /// <summary>
    /// 区域.
    /// </summary>
    [SugarColumn(ColumnName = "Region")]
    public string Region { get; set; }

    /// <summary>
    /// 注册类别.
    /// </summary>
    [SugarColumn(ColumnName = "RegistrationCategory")]
    public string RegistrationCategory { get; set; }

    /// <summary>
    /// 专业1.
    /// </summary>
    [SugarColumn(ColumnName = "Major1")]
    public string Major1 { get; set; }

    /// <summary>
    /// 专业2.
    /// </summary>
    [SugarColumn(ColumnName = "Major2")]
    public string Major2 { get; set; }

    /// <summary>
    /// 专业3.
    /// </summary>
    [SugarColumn(ColumnName = "Major3")]
    public string Major3 { get; set; }

    /// <summary>
    /// 专业4.
    /// </summary>
    [SugarColumn(ColumnName = "Major4")]
    public string Major4 { get; set; }

    /// <summary>
    /// 专业5.
    /// </summary>
    [SugarColumn(ColumnName = "Major5")]
    public string Major5 { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    [SugarColumn(ColumnName = "Remark")]
    public string Remark { get; set; }

    /// <summary>
    /// 客户经理id.
    /// </summary>
    [SugarColumn(ColumnName = "ManagerId")]
    public string ManagerId { get; set; }

    /// <summary>
    /// 是否入驻.
    /// </summary>
    [SugarColumn(ColumnName = "WhetherSettled")]
    public int? WhetherSettled { get; set; }

    /// <summary>
    /// 入驻时间.
    /// </summary>
    [SugarColumn(ColumnName = "SettlementDate")]
    public DateTime? SettlementDate { get; set; }

    /// <summary>
    /// 社保情况.
    /// </summary>
    [SugarColumn(ColumnName = "SocialSecurityStatus")]
    public string SocialSecurityStatus { get; set; }

    /// <summary>
    /// 业绩情况.
    /// </summary>
    [SugarColumn(ColumnName = "PerformanceSituation")]
    public string PerformanceSituation { get; set; }

    /// <summary>
    /// 薪资要求.
    /// </summary>
    [SugarColumn(ColumnName = "SalaryRequirement")]
    public string SalaryRequirement { get; set; }

    /// <summary>
    /// 学历
    /// </summary>
    [SugarColumn(ColumnName = "Education")]
    public string Education { get; set; }

    /// <summary>
    /// 业绩数量
    /// </summary>
    [SugarColumn(ColumnName = "PerformanceNum")]
    public int? PerformanceNum { get; set; }
}