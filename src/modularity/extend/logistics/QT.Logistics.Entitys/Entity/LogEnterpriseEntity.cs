using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Logistics.Entitys;

/// <summary>
/// 入驻商家实体.
/// </summary>
[SugarTable("log_enterprise")]
[Tenant(ClaimConst.TENANTID)]
public class LogEnterpriseEntity : CUDEntityBase
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public override string Id { get; set; }

    /// <summary>
    /// 名称.
    /// </summary>
    [SugarColumn(ColumnName = "Name")]
    public string Name { get; set; }

    /// <summary>
    /// 电话.
    /// </summary>
    [SugarColumn(ColumnName = "Phone")]
    public string Phone { get; set; }

    /// <summary>
    /// 帐号.
    /// </summary>
    [SugarColumn(ColumnName = "Account")]
    public string Account { get; set; }

    /// <summary>
    /// 密码.
    /// </summary>
    [SugarColumn(ColumnName = "Password")]
    public string Password { get; set; }

    /// <summary>
    /// 负责人.
    /// </summary>
    [SugarColumn(ColumnName = "Leader")]
    public string Leader { get; set; }

    /// <summary>
    /// 负责人id.
    /// </summary>
    [SugarColumn(ColumnName = "AdminId")]
    public string AdminId { get; set; }

    /// <summary>
    /// 状态.
    /// </summary>
    [SugarColumn(ColumnName = "Status")]
    public int? Status { get; set; }


    /// <summary>
    /// 扩展属性.
    /// </summary>
    [SugarColumn(ColumnName = "PropertyJson")]
    public string PropertyJson { get; set; }


    /// <summary>
    /// 图标.
    /// </summary>
    [SugarColumn(ColumnName = "ImageUrl")]
    public string ImageUrl { get; set; }
}