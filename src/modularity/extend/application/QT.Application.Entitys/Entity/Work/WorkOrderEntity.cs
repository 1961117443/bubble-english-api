using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Extend.Entitys;

/// <summary>
/// 工单管理
/// </summary>
[SugarTable("EXT_WORKORDER")]
[Tenant(ClaimConst.TENANTID)]
public class WorkOrderEntity : CLDEntityBase
{
    /// <summary>
    /// 工单编号.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_No")]
    public string? No { get; set; }

    /// <summary>
    /// 问题描述.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_CONTENT")]
    public string? Content { get; set; }

    /// <summary>
    /// 工单状态.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_Status")]
    public int Status { get; set; }

    /// <summary>
    /// 图片.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_ImageJson")]
    public string? ImageJson { get; set; }

    /// <summary>
    /// 附件.
    /// </summary>
    /// <returns></returns>
    [SugarColumn(ColumnName = "F_AttachJson")]
    public string? AttachJson { get; set; }


    /// <summary>
    /// 指派时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_AssignTime", ColumnDescription = "指派时间")]
    public virtual DateTime? AssignTime { get; set; }

    /// <summary>
    /// 指派用户.
    /// </summary>
    [SugarColumn(ColumnName = "F_AssignUserId", ColumnDescription = "指派用户")]
    public virtual string AssignUserId { get; set; }

    /// <summary>
    /// 工单类型.
    /// </summary>
    [SugarColumn(ColumnName = "F_Category", ColumnDescription = "工单类型")]
    public virtual int Category { get; set; }

    /// <summary>
    /// 团队id.
    /// </summary>
    [SugarColumn(ColumnName = "F_Tid", ColumnDescription = "团队id")]
    public virtual string Tid { get; set; }

    /// <summary>
    /// 项目id.
    /// </summary>
    [SugarColumn(ColumnName = "F_Pid", ColumnDescription = "项目id")]
    public virtual string Pid { get; set; }
}
