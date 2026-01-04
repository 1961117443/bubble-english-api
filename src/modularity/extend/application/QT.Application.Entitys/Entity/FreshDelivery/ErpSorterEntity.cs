using QT.Common.Const;
using QT.Extras.DatabaseAccessor.SqlSugar;
using SqlSugar;

namespace QT.Application.Entitys.FreshDelivery;

/// <summary>
/// 分拣员实体.
/// </summary>
[SugarTable("erp_sorter")]
[Tenant(ClaimConst.TENANTID)]
public class ErpSorterEntity: ICompanyEntity
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
    /// 姓名.
    /// </summary>
    [SugarColumn(ColumnName = "F_Name")]
    public string Name { get; set; }

    /// <summary>
    /// 电话.
    /// </summary>
    [SugarColumn(ColumnName = "F_Mobile")]
    public string Mobile { get; set; }

    /// <summary>
    /// 登录帐号，角色叫分拣员.
    /// </summary>
    [SugarColumn(ColumnName = "F_LoginId")]
    public string LoginId { get; set; }

    /// <summary>
    /// 登录密码.
    /// </summary>
    [SugarColumn(ColumnName = "F_LoginPwd")]
    public string LoginPwd { get; set; }

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

}