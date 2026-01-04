using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Logistics.Entitys;

/// <summary>
/// 商家客户实体.
/// </summary>
[SugarTable("log_enterprise_customer")]
[Tenant(ClaimConst.TENANTID)]
public class LogEnterpriseCustomerEntity : CUDEntityBase, ILogEnterpriseEntity
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public override string Id { get; set; }

    /// <summary>
    /// 商家ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_EId")]
    public string EId { get; set; }

    /// <summary>
    /// 客户名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_Name")]
    public string Name { get; set; }

    /// <summary>
    /// 客户地址.
    /// </summary>
    [SugarColumn(ColumnName = "F_Address")]
    public string Address { get; set; }

    /// <summary>
    /// 联系人.
    /// </summary>
    [SugarColumn(ColumnName = "F_Admin")]
    public string Admin { get; set; }

    /// <summary>
    /// 联系人电话.
    /// </summary>
    [SugarColumn(ColumnName = "F_Admintel")]
    public string Admintel { get; set; }
}