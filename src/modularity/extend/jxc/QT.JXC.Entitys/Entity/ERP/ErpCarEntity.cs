using QT.Common.Const;
using SqlSugar;

namespace QT.JXC.Entitys.Entity.ERP;

/// <summary>
/// 车辆管理实体.
/// </summary>
[SugarTable("erp_car")]
[Tenant(ClaimConst.TENANTID)]
public class ErpCarEntity
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
    /// 车牌号码.
    /// </summary>
    [SugarColumn(ColumnName = "F_CarNo")]
    public string CarNo { get; set; }

    /// <summary>
    /// 驾驶员.
    /// </summary>
    [SugarColumn(ColumnName = "F_Driver")]
    public string Driver { get; set; }

    /// <summary>
    /// 驾驶员手机.
    /// </summary>
    [SugarColumn(ColumnName = "F_Phone")]
    public string Phone { get; set; }

    /// <summary>
    /// 运输状态.
    /// </summary>
    [SugarColumn(ColumnName = "F_Status")]
    public string Status { get; set; }

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
    /// 车载监控.
    /// </summary>
    [SugarColumn(ColumnName = "F_DeviceId")]
    public string DeviceId { get; set; }
}