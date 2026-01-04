using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.JZRC.Entitys.Dto.JzrcTalent;


public class JzrcTalentImportDataTemplate
{
    /// <summary>
    /// 姓名.
    /// </summary>
    [Display(Name = "姓名")]
    public string name { get; set; }

    /// <summary>
    /// 身份证.
    /// </summary>
    [Display(Name = "身份证")]
    public string idCard { get; set; }

    /// <summary>
    /// 性别.
    /// </summary>
    [Display(Name = "性别")]
    public string gender { get; set; }

    /// <summary>
    /// 手机.
    /// </summary>
    [Display(Name = "手机")]
    public string mobilePhone { get; set; }

    /// <summary>
    /// 区域.
    /// </summary>
    [Display(Name = "区域")]
    public string region { get; set; }

    /// <summary>
    /// 注册类别.
    /// </summary>
    [Display(Name = "注册类别")]
    public string registrationCategory { get; set; }

    /// <summary>
    /// 专业1.
    /// </summary>
    [Display(Name = "专业1")]
    public string major1 { get; set; }

    /// <summary>
    /// 专业2.
    /// </summary>
    [Display(Name = "专业2")]
    public string major2 { get; set; }

    /// <summary>
    /// 专业3.
    /// </summary>
    [Display(Name = "专业3")]
    public string major3 { get; set; }

    /// <summary>
    /// 专业4.
    /// </summary>
    [Display(Name = "专业4")]
    public string major4 { get; set; }

    /// <summary>
    /// 专业5.
    /// </summary>
    [Display(Name = "专业5")]
    public string major5 { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    [Display(Name = "备注")]
    public string remark { get; set; }


    /// <summary>
    /// 社保情况.
    /// </summary>
    [Display(Name = "社保情况")]
    public string socialSecurityStatus { get; set; }

    /// <summary>
    /// 业绩情况.
    /// </summary>
    [Display(Name = "业绩情况")]
    public string performanceSituation { get; set; }

    /// <summary>
    /// 薪资要求.
    /// </summary>
    [Display(Name = "薪资要求")]
    public string salaryRequirement { get; set; }
}
public class JzrcTalentImportDataInput : JzrcTalentImportDataTemplate
{
    /// <summary>
    /// 主键.
    /// </summary>
    [Display(Name = "主键")]
    public string id { get; set; }
}
