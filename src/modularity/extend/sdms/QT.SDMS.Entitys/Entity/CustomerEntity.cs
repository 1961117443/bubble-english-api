using QT.Common.Contracts;
using SqlSugar;
using System.ComponentModel;

namespace QT.SDMS.Entitys.Entity;

/// <summary>
/// 客户管理
/// </summary>
[SugarTable("sdms_customer")]
public class CustomerEntity : CDEntityBase
{
    /// <summary>
    /// 客户名称
    /// </summary>
    [SugarColumn(ColumnName = "Name")]
    public string Name { get; set; }


    /// <summary>
    /// 联系人
    /// </summary>
    [SugarColumn(ColumnName = "ContactName")]
    public string ContactName { get; set; }

    /// <summary>
    /// 联系人电话
    /// </summary>
    [SugarColumn(ColumnName = "ContactPhone")]
    public string ContactPhone { get; set; }

    /// <summary>
    /// 邮箱地址
    /// </summary>
    [SugarColumn(ColumnName = "Email")]
    public string Email { get; set; }

    /// <summary>
    /// 家庭地址
    /// </summary>
    [SugarColumn(ColumnName = "Address")]
    public string Address { get; set; }

    /// <summary>
    /// 客户经理
    /// </summary>
    [SugarColumn(ColumnName = "ManagerId")]
    public string ManagerId { get; set; }

    /// <summary>
    /// 计量点
    /// </summary>
    [SugarColumn(ColumnName = "MeterPoint")]
    public string MeterPoint { get; set; }

    /// <summary>
    /// 附件
    /// </summary>
    [SugarColumn(ColumnName = "Attachment")]
    public string Attachment { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnName = "Remark")]
    public string Remark { get; set; }


    /// <summary>
    /// 状态
    /// </summary>
    [SugarColumn(ColumnName = "Status")]
    public int Status { get; set; }

    /// <summary>
    /// 手机认证状态：0未验证 1已验证
    /// </summary>
    [SugarColumn(ColumnName = "PhoneVerifyStatus")]
    public int PhoneVerifyStatus { get; set; }

    /// <summary>
    /// 邮箱认证状态：0未验证 1已验证
    /// </summary>
    [SugarColumn(ColumnName = "EmailVerifyStatus")]
    public int EmailVerifyStatus { get; set; }


    /// <summary>
    /// 身份证集合
    /// </summary>
    [SugarColumn(ColumnName = "IdCardImgJson")]
    public string IdCardImgJson { get; set; }


    /// <summary>
    /// 户口簿图片集合
    /// </summary>
    [SugarColumn(ColumnName = "HouseholdImgJson")]
    public string HouseholdImgJson { get; set; }
}


public enum CertificationStatus: int
{
    /// <summary>
    /// 未认证
    /// </summary>
    [Description("未认证")]
    REGISTERED = 1,

    /// <summary>
    /// 已认证
    /// </summary>
    [Description("已认证")] 
    VERIFIED = 2,
    /// <summary>
    /// 认证失败
    /// </summary>
    [Description("认证失败")] 
    REJECTED = 3
}

public enum VerifyStatus : int
{
    /// <summary>
    /// 未验证
    /// </summary>
    [Description("未验证")]
    UNVERIFIED = 0,

    /// <summary>
    /// 已认证
    /// </summary>
    [Description("已认证")]
    VERIFIED = 1
}