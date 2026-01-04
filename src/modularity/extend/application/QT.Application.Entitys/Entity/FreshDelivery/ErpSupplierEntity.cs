using QT.Common.Const;
using QT.Extras.DatabaseAccessor.SqlSugar;
using SqlSugar;

namespace QT.Application.Entitys.FreshDelivery;

/// <summary>
/// 供货商信息实体.
/// </summary>
[SugarTable("erp_supplier")]
[Tenant(ClaimConst.TENANTID)]
public class ErpSupplierEntity: ICompanyEntity
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public string Id { get; set; }

    /// <summary>
    /// 公司ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_Oid")]
    public string Oid { get; set; }

    /// <summary>
    /// 名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_Name")]
    public string Name { get; set; }

    /// <summary>
    /// 地址.
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
    [SugarColumn(ColumnName = "F_AdminTel")]
    public string AdminTel { get; set; }

    /// <summary>
    /// 入驻时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_JoinTime")]
    public DateTime? JoinTime { get; set; }

    /// <summary>
    /// LOGO.
    /// </summary>
    [SugarColumn(ColumnName = "F_Logo")]
    public string Logo { get; set; }

    /// <summary>
    /// 经营周期.
    /// </summary>
    [SugarColumn(ColumnName = "F_WorkCycle")]
    public string WorkCycle { get; set; }

    /// <summary>
    /// 经营时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_WorkTime")]
    public string WorkTime { get; set; }

    /// <summary>
    /// 登录帐号，角色为“供货商”.
    /// </summary>
    [SugarColumn(ColumnName = "F_LoginId")]
    public string LoginId { get; set; }

    /// <summary>
    /// 登录密码.
    /// </summary>
    [SugarColumn(ColumnName = "F_LoginPwd")]
    public string LoginPwd { get; set; }

    /// <summary>
    /// 业务人员.
    /// </summary>
    [SugarColumn(ColumnName = "F_Salesman")]
    public string Salesman { get; set; }

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
    /// 名称首字母.
    /// </summary>
    [SugarColumn(ColumnName = "F_FirstChar")]
    public string FirstChar { get; set; }

     

    /// <summary>
    /// 营业执照.
    /// </summary>
    [SugarColumn(ColumnName = "BusinessLicense")]
    public string BusinessLicense { get; set; }

    /// <summary>
    /// 座机号码.
    /// </summary>
    [SugarColumn(ColumnName = "LandlineNumber")]
    public string LandlineNumber { get; set; }


    /// <summary>
    /// 生产许可证.
    /// </summary>
    [SugarColumn(ColumnName = "ProductionLicense")]
    public string ProductionLicense { get; set; }

    /// <summary>
    /// 食品经营许可证.
    /// </summary>
    [SugarColumn(ColumnName = "FoodBusinessLicense")]
    public string FoodBusinessLicense { get; set; }
}