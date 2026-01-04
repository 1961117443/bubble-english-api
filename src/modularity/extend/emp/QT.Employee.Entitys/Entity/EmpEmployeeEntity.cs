using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Emp.Entitys;

/// <summary>
/// 员工档案实体.
/// </summary>
[SugarTable("emp_employee")]
[Tenant(ClaimConst.TENANTID)]
public class EmpEmployeeEntity :CUDEntityBase
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
    /// 邮箱.
    /// </summary>
    [SugarColumn(ColumnName = "Email")]
    public string Email { get; set; }

    /// <summary>
    /// 部门id.
    /// </summary>
    [SugarColumn(ColumnName = "OrganizeId")]
    public string OrganizeId { get; set; }

    /// <summary>
    /// 组织id.
    /// </summary>
    [SugarColumn(ColumnName = "PositionId")]
    public string PositionId { get; set; }

    /// <summary>
    /// 手机号码.
    /// </summary>
    [SugarColumn(ColumnName = "Mobile")]
    public string Mobile { get; set; }

    /// <summary>
    /// 工号.
    /// </summary>
    [SugarColumn(ColumnName = "JobNumber")]
    public string JobNumber { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    [SugarColumn(ColumnName = "Remark")]
    public string Remark { get; set; }

    /// <summary>
    /// 入职时间.
    /// </summary>
    [SugarColumn(ColumnName = "ConfrimJoinTime")]
    public DateTime? ConfrimJoinTime { get; set; }

    /// <summary>
    /// 员工类型.
    /// </summary>
    [SugarColumn(ColumnName = "EmployeeType")]
    public string EmployeeType { get; set; }

    /// <summary>
    /// 员工状态.
    /// </summary>
    [SugarColumn(ColumnName = "EmployeeStatus")]
    public string EmployeeStatus { get; set; }

    /// <summary>
    /// 试用期.
    /// </summary>
    [SugarColumn(ColumnName = "ProbationPeriodType")]
    public string ProbationPeriodType { get; set; }

    /// <summary>
    /// 实际转正日期.
    /// </summary>
    [SugarColumn(ColumnName = "RegularTime")]
    public DateTime? RegularTime { get; set; }

    /// <summary>
    /// 计划转正日期.
    /// </summary>
    [SugarColumn(ColumnName = "PlanRegularTime")]
    public DateTime? PlanRegularTime { get; set; }

    /// <summary>
    /// 身份证姓名.
    /// </summary>
    [SugarColumn(ColumnName = "RealName")]
    public string RealName { get; set; }

    /// <summary>
    /// 证件号码.
    /// </summary>
    [SugarColumn(ColumnName = "CertNo")]
    public string CertNo { get; set; }

    /// <summary>
    /// 出生日期.
    /// </summary>
    [SugarColumn(ColumnName = "BirthTime")]
    public DateTime? BirthTime { get; set; }

    /// <summary>
    /// 性别.
    /// </summary>
    [SugarColumn(ColumnName = "SexType")]
    public string SexType { get; set; }

    /// <summary>
    /// 民族.
    /// </summary>
    [SugarColumn(ColumnName = "NationType")]
    public string NationType { get; set; }

    /// <summary>
    /// 身份证地址.
    /// </summary>
    [SugarColumn(ColumnName = "CertAddress")]
    public string CertAddress { get; set; }

    /// <summary>
    /// 身份证有效期.
    /// </summary>
    [SugarColumn(ColumnName = "CertEndTime")]
    public DateTime? CertEndTime { get; set; }

    /// <summary>
    /// 婚姻状况.
    /// </summary>
    [SugarColumn(ColumnName = "Marriage")]
    public string Marriage { get; set; }

    /// <summary>
    /// 首次参加工作时间.
    /// </summary>
    [SugarColumn(ColumnName = "JoinWorkingTime")]
    public DateTime? JoinWorkingTime { get; set; }

    /// <summary>
    /// 户籍类型.
    /// </summary>
    [SugarColumn(ColumnName = "ResidenceType")]
    public string ResidenceType { get; set; }

    /// <summary>
    /// 住址.
    /// </summary>
    [SugarColumn(ColumnName = "NewAddress")]
    public string NewAddress { get; set; }

    /// <summary>
    /// 政治面貌.
    /// </summary>
    [SugarColumn(ColumnName = "PoliticalStatus")]
    public string PoliticalStatus { get; set; }

    /// <summary>
    /// 个人社保账号.
    /// </summary>
    [SugarColumn(ColumnName = "PersonalSi")]
    public string PersonalSi { get; set; }

    /// <summary>
    /// 个人公积金账号.
    /// </summary>
    [SugarColumn(ColumnName = "PersonalHf")]
    public string PersonalHf { get; set; }

    /// <summary>
    /// 银行卡号.
    /// </summary>
    [SugarColumn(ColumnName = "BankAccountNo")]
    public string BankAccountNo { get; set; }

    /// <summary>
    /// 开户行.
    /// </summary>
    [SugarColumn(ColumnName = "AccountBank")]
    public string AccountBank { get; set; }

    /// <summary>
    /// 合同类型.
    /// </summary>
    [SugarColumn(ColumnName = "ContractType")]
    public string ContractType { get; set; }

    /// <summary>
    /// 首次合同起始日.
    /// </summary>
    [SugarColumn(ColumnName = "FirstContractStartTime")]
    public DateTime? FirstContractStartTime { get; set; }

    /// <summary>
    /// 首次合同到期日.
    /// </summary>
    [SugarColumn(ColumnName = "FirstContractEndTime")]
    public DateTime? FirstContractEndTime { get; set; }

    /// <summary>
    /// 现合同起始日.
    /// </summary>
    [SugarColumn(ColumnName = "NowContractStartTime")]
    public DateTime? NowContractStartTime { get; set; }

    /// <summary>
    /// 现合同到期日.
    /// </summary>
    [SugarColumn(ColumnName = "NowContractEndTime")]
    public DateTime? NowContractEndTime { get; set; }

    /// <summary>
    /// 合同期限.
    /// </summary>
    [SugarColumn(ColumnName = "ContractPeriodType")]
    public string ContractPeriodType { get; set; }

    /// <summary>
    /// 续签次数.
    /// </summary>
    [SugarColumn(ColumnName = "ContractRenewCount")]
    public int? ContractRenewCount { get; set; }

    /// <summary>
    /// 身份证.
    /// </summary>
    [SugarColumn(ColumnName = "IDcardCertificate")]
    public string IDcardCertificate { get; set; }

    /// <summary>
    /// 学历证书.
    /// </summary>
    [SugarColumn(ColumnName = "AcademicCertificate")]
    public string AcademicCertificate { get; set; }

    /// <summary>
    /// 学位证书.
    /// </summary>
    [SugarColumn(ColumnName = "DiplomaCertificate")]
    public string DiplomaCertificate { get; set; }

    /// <summary>
    /// 员工照片.
    /// </summary>
    [SugarColumn(ColumnName = "PersonalPhoto")]
    public string PersonalPhoto { get; set; }

    /// <summary>
    /// 其他附件.
    /// </summary>
    [SugarColumn(ColumnName = "Attachment")]
    public string Attachment { get; set; }

    /// <summary>
    /// 扩展属性.
    /// </summary>
    [SugarColumn(ColumnName = "PropertyJson")]
    public string PropertyJson { get; set; }

    /// <summary>
    /// 工资.
    /// </summary>
    [SugarColumn(ColumnName = "Salary")]
    public decimal Salary { get; set; }

    /// <summary>
    /// 工资类型.
    /// </summary>
    [SugarColumn(ColumnName = "SalaryType")]
    public string SalaryType { get; set; }

    /// <summary>
    /// 工资说明.
    /// </summary>
    [SugarColumn(ColumnName = "SalaryRemark")]
    public string SalaryRemark { get; set; }
}