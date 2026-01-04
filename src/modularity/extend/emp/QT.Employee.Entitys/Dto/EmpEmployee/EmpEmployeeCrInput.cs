using QT.Common.Models;
using QT.Emp.Entitys.Dto.EmpEmployeeEdu;
using QT.Emp.Entitys.Dto.EmpEmployeeFamily;
using QT.Emp.Entitys.Dto.EmpEmployeeUrgent;

namespace QT.Emp.Entitys.Dto.EmpEmployee;

/// <summary>
/// 员工档案修改输入参数.
/// </summary>
public class EmpEmployeeCrInput
{
    /// <summary>
    /// 姓名.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 邮箱.
    /// </summary>
    public string email { get; set; }

    /// <summary>
    /// 部门id.
    /// </summary>
    public string organizeId { get; set; }

    /// <summary>
    /// 组织id.
    /// </summary>
    public string positionId { get; set; }

    /// <summary>
    /// 手机号码.
    /// </summary>
    public string mobile { get; set; }

    /// <summary>
    /// 工号.
    /// </summary>
    public string jobNumber { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string remark { get; set; }

    /// <summary>
    /// 入职时间.
    /// </summary>
    public DateTime? confrimJoinTime { get; set; }

    /// <summary>
    /// 员工类型.
    /// </summary>
    public string employeeType { get; set; }

    /// <summary>
    /// 员工状态.
    /// </summary>
    public string employeeStatus { get; set; }

    /// <summary>
    /// 试用期.
    /// </summary>
    public string probationPeriodType { get; set; }

    /// <summary>
    /// 实际转正日期.
    /// </summary>
    public DateTime? regularTime { get; set; }

    /// <summary>
    /// 计划转正日期.
    /// </summary>
    public DateTime? planRegularTime { get; set; }

    /// <summary>
    /// 身份证姓名.
    /// </summary>
    public string realName { get; set; }

    /// <summary>
    /// 证件号码.
    /// </summary>
    public string certNo { get; set; }

    /// <summary>
    /// 出生日期.
    /// </summary>
    public DateTime? birthTime { get; set; }

    /// <summary>
    /// 性别.
    /// </summary>
    public string sexType { get; set; }

    /// <summary>
    /// 民族.
    /// </summary>
    public string nationType { get; set; }

    /// <summary>
    /// 身份证地址.
    /// </summary>
    public string certAddress { get; set; }

    /// <summary>
    /// 身份证有效期.
    /// </summary>
    public DateTime? certEndTime { get; set; }

    /// <summary>
    /// 婚姻状况.
    /// </summary>
    public string marriage { get; set; }

    /// <summary>
    /// 首次参加工作时间.
    /// </summary>
    public DateTime? joinWorkingTime { get; set; }

    /// <summary>
    /// 户籍类型.
    /// </summary>
    public string residenceType { get; set; }

    /// <summary>
    /// 住址.
    /// </summary>
    public string newAddress { get; set; }

    /// <summary>
    /// 政治面貌.
    /// </summary>
    public string politicalStatus { get; set; }

    /// <summary>
    /// 个人社保账号.
    /// </summary>
    public string personalSi { get; set; }

    /// <summary>
    /// 个人公积金账号.
    /// </summary>
    public string personalHf { get; set; }

    /// <summary>
    /// 银行卡号.
    /// </summary>
    public string bankAccountNo { get; set; }

    /// <summary>
    /// 开户行.
    /// </summary>
    public string accountBank { get; set; }

    /// <summary>
    /// 合同类型.
    /// </summary>
    public string contractType { get; set; }

    /// <summary>
    /// 首次合同起始日.
    /// </summary>
    public DateTime? firstContractStartTime { get; set; }

    /// <summary>
    /// 首次合同到期日.
    /// </summary>
    public DateTime? firstContractEndTime { get; set; }

    /// <summary>
    /// 现合同起始日.
    /// </summary>
    public DateTime? nowContractStartTime { get; set; }

    /// <summary>
    /// 现合同到期日.
    /// </summary>
    public DateTime? nowContractEndTime { get; set; }

    /// <summary>
    /// 合同期限.
    /// </summary>
    public string contractPeriodType { get; set; }

    /// <summary>
    /// 续签次数.
    /// </summary>
    public int? contractRenewCount { get; set; }

    /// <summary>
    /// 身份证.
    /// </summary>
    public List<FileControlsModel> iDcardCertificate { get; set; }

    /// <summary>
    /// 学历证书.
    /// </summary>
    public List<FileControlsModel> academicCertificate { get; set; }

    /// <summary>
    /// 学位证书.
    /// </summary>
    public List<FileControlsModel> diplomaCertificate { get; set; }

    /// <summary>
    /// 员工照片.
    /// </summary>
    public List<FileControlsModel> personalPhoto { get; set; }

    /// <summary>
    /// 其他附件.
    /// </summary>
    public List<FileControlsModel> attachment { get; set; }

    /// <summary>
    /// 扩展属性.
    /// </summary>
    public string propertyJson { get; set; }

    /// <summary>
    /// 员工学历.
    /// </summary>
    public List<EmpEmployeeEduCrInput> empEmployeeEduList { get; set; }

    /// <summary>
    /// 员工家庭信息.
    /// </summary>
    public List<EmpEmployeeFamilyCrInput> empEmployeeFamilyList { get; set; }

    /// <summary>
    /// 员工紧急联系人.
    /// </summary>
    public List<EmpEmployeeUrgentCrInput> empEmployeeUrgentList { get; set; }

    /// <summary>
    /// 工资
    /// </summary>
    public decimal salary { get; set; }

    /// <summary>
    /// 工资类型
    /// </summary>
    public string salaryType { get; set; }
    /// <summary>
    /// 工资说明
    /// </summary>
    public string salaryRemark { get; set; }

}