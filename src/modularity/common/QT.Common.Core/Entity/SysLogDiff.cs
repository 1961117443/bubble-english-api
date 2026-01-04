using QT.Common.Const;
using QT.Systems.Entitys.System;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Common.Core.Entity;

/// <summary>
/// 日记.
/// </summary>
//[SugarTable("base_syslogdiff")]
[SugarTable("base_syslogdiff_{year}{month}{day}")]
[Tenant(ClaimConst.TENANTID)]
[SplitTable(SplitType.Month)]
[SugarIndex("createtime", nameof(SysLogDiff.CreateTime), OrderByType.Desc)]
public class SysLogDiff
{
    /// <summary>
    /// 雪花Id
    /// </summary>
    [SugarColumn(ColumnDescription = "Id", IsPrimaryKey = true, IsIdentity = false, ColumnDataType = "bigint")]
    public virtual string Id { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [SugarColumn(ColumnDescription = "创建时间", IsOnlyIgnoreUpdate = true,IsNullable = true)]
    [SplitField]
    public virtual DateTime? CreateTime { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    [SugarColumn(ColumnDescription = "更新时间", IsOnlyIgnoreInsert = true, IsNullable = true)]
    public virtual DateTime? UpdateTime { get; set; }

    /// <summary>
    /// 创建者Id
    /// </summary>
    [SugarColumn(ColumnDescription = "创建者Id", IsOnlyIgnoreUpdate = true, IsNullable = true, Length = 50)]
    public virtual string? CreateUserId { get; set; }

    /// <summary>
    /// 修改者Id
    /// </summary>
    [SugarColumn(ColumnDescription = "修改者Id", IsOnlyIgnoreInsert = true, IsNullable = true, Length = 50)]
    public virtual string? UpdateUserId { get; set; }

    /// <summary>
    /// 软删除
    /// </summary>
    [SugarColumn(ColumnDescription = "软删除")]
    public virtual bool IsDelete { get; set; } = false;
    /// <summary>
    /// 操作前记录
    /// </summary>
    [SugarColumn(ColumnDescription = "操作前记录", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    public string? BeforeData { get; set; }

    /// <summary>
    /// 操作后记录
    /// </summary>
    [SugarColumn(ColumnDescription = "操作后记录", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    public string? AfterData { get; set; }

    /// <summary>
    /// Sql
    /// </summary>
    [SugarColumn(ColumnDescription = "Sql", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    public string? Sql { get; set; }

    /// <summary>
    /// 参数  手动传入的参数
    /// </summary>
    [SugarColumn(ColumnDescription = "参数", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    public string? Parameters { get; set; }

    /// <summary>
    /// 业务对象
    /// </summary>
    [SugarColumn(ColumnDescription = "业务对象", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    public string? BusinessData { get; set; }

    /// <summary>
    /// 差异操作
    /// </summary>
    [SugarColumn(ColumnDescription = "差异操作", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    public string? DiffType { get; set; }

    /// <summary>
    /// 耗时
    /// </summary>
    [SugarColumn(ColumnDescription = "耗时", IsNullable = true)]
    public long? Elapsed { get; set; }
}
