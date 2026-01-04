using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Extend.Entitys;

/// <summary>
/// 报价系统-客户管理
/// </summary>
[SugarTable("erp_customer")]
[Tenant(ClaimConst.TENANTID)]
public class QuoteCustomerEntity :EntityBase<string>
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public override string Id { get; set; }

    /// <summary>
    /// 公司ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_Oid")]
    public string Oid { get; set; }

    /// <summary>
    /// 客户名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_Name")]
    public string Name { get; set; }

    /// <summary>
    /// 客户编号.
    /// </summary>
    [SugarColumn(ColumnName = "F_No")]
    public string No { get; set; }

    /// <summary>
    /// 客户地址.
    /// </summary>
    [SugarColumn(ColumnName = "F_Address")]
    public string Address { get; set; }

    /// <summary>
    /// 负责人.
    /// </summary>
    [SugarColumn(ColumnName = "F_Admin")]
    public string Admin { get; set; }

    /// <summary>
    /// 负责人电话.
    /// </summary>
    [SugarColumn(ColumnName = "F_Admintel")]
    public string Admintel { get; set; }

    /// <summary>
    /// 客户简介.
    /// </summary>
    [SugarColumn(ColumnName = "F_Description")]
    public string Description { get; set; }

    /// <summary>
    /// 营业执照.
    /// </summary>
    [SugarColumn(ColumnName = "F_License")]
    public string License { get; set; }

    /// <summary>
    /// 登录帐号j,角色是“客户”.
    /// </summary>
    [SugarColumn(ColumnName = "F_LoginId")]
    public string LoginId { get; set; }

    /// <summary>
    /// 登录密码.
    /// </summary>
    [SugarColumn(ColumnName = "F_LoginPwd")]
    public string LoginPwd { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    [SugarColumn(ColumnName = "F_Remark")]
    public string Remark { get; set; }

    /// <summary>
    /// 创建时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_CreatorTime")]
    public DateTime? CreatorTime { get; set; }

    /// <summary>
    /// 创建用户.
    /// </summary>
    [SugarColumn(ColumnName = "F_CreatorUserId")]
    public string CreatorUserId { get; set; }

    /// <summary>
    /// 修改时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_LastModifyTime")]
    public DateTime? LastModifyTime { get; set; }

    /// <summary>
    /// 修改用户.
    /// </summary>
    [SugarColumn(ColumnName = "F_LastModifyUserId")]
    public string LastModifyUserId { get; set; }


    /// <summary>
    /// 客户类型（数据字典）.
    /// </summary>
    [SugarColumn(ColumnName = "F_Type")]
    public string Type { get; set; }

    /// <summary>
    /// 启用(0)和禁用(1).
    /// </summary>
    [SugarColumn(ColumnName = "F_Stop")]
    public int Stop { get; set; }

    /// <summary>
    /// 客户简称.
    /// </summary>
    [SugarColumn(ColumnName = "F_ShortName")]
    public string ShortName { get; set; }

    /// <summary>
    /// 餐别.
    /// </summary>
    [SugarColumn(ColumnName = "DiningType")]
    public string DiningType { get; set; }

    /// <summary>
    /// 送货人.
    /// </summary>
    [SugarColumn(ColumnName = "DeliveryManId")]
    public string DeliveryManId { get; set; }


    /// <summary>
    /// 折扣定价.
    /// </summary>
    [SugarColumn(ColumnName = "Discount")]
    public decimal? Discount { get; set; }
}
