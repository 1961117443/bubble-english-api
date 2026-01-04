using QT.Common.Const;
using SqlSugar;

namespace QT.Emp.Entitys;

/// <summary>
/// 模块提醒实体.
/// </summary>
[SugarTable("base_moduleremind")]
[Tenant(ClaimConst.TENANTID)]
public class BaseModuleremindEntity
{
    /// <summary>
    /// 主键_id.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public string Id { get; set; }

    /// <summary>
    /// 名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_FullName")]
    public string FullName { get; set; }

    /// <summary>
    /// 描述.
    /// </summary>
    [SugarColumn(ColumnName = "F_Description")]
    public string Description { get; set; }

    /// <summary>
    /// 排序码.
    /// </summary>
    [SugarColumn(ColumnName = "F_SortCode")]
    public long SortCode { get; set; }

    /// <summary>
    /// 有效标志.
    /// </summary>
    [SugarColumn(ColumnName = "F_EnabledMark")]
    public int? EnabledMark { get; set; }

    /// <summary>
    /// 创建时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_CreatorTime")]
    public DateTime? CreatorTime { get; set; }

    /// <summary>
    /// 创建用户_id.
    /// </summary>
    [SugarColumn(ColumnName = "F_CreatorUserId")]
    public string CreatorUserId { get; set; }

    /// <summary>
    /// 修改时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_LastModifyTime")]
    public DateTime? LastModifyTime { get; set; }

    /// <summary>
    /// 修改用户_id.
    /// </summary>
    [SugarColumn(ColumnName = "F_LastModifyUserId")]
    public string LastModifyUserId { get; set; }

    /// <summary>
    /// 删除标志.
    /// </summary>
    [SugarColumn(ColumnName = "F_DeleteMark")]
    public int? DeleteMark { get; set; }

    /// <summary>
    /// 删除时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_DeleteTime")]
    public DateTime? DeleteTime { get; set; }

    /// <summary>
    /// 删除用户_id.
    /// </summary>
    [SugarColumn(ColumnName = "F_DeleteUserId")]
    public string DeleteUserId { get; set; }

    /// <summary>
    /// sql语句.
    /// </summary>
    [SugarColumn(ColumnName = "F_SqlTemplate")]
    public string SqlTemplate { get; set; }

    /// <summary>
    /// 功能主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_ModuleId")]
    public string ModuleId { get; set; }

}