using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Systems.Entitys.System;

/// <summary>
/// 数据备份



/// 日 期：2021-06-01.
/// </summary>
[SugarTable("BASE_DBBACKUP")]
[Tenant(ClaimConst.TENANTID)]
public class DbBackupEntity : CLDEntityBase
{
    /// <summary>
    /// 备份库名.
    /// </summary>
    [SugarColumn(ColumnName = "F_BACKUPDBNAME")]
    public string BackupDbName { get; set; }

    /// <summary>
    /// 备份时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_BACKUPTIME")]
    public DateTime? BackupTime { get; set; }

    /// <summary>
    /// 文件名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_FILENAME")]
    public string FileName { get; set; }

    /// <summary>
    /// 文件大小.
    /// </summary>
    [SugarColumn(ColumnName = "F_FILESIZE")]
    public string FileSize { get; set; }

    /// <summary>
    /// 文件路径.
    /// </summary>
    [SugarColumn(ColumnName = "F_FILEPATH")]
    public string FilePath { get; set; }

    /// <summary>
    /// 描述.
    /// </summary>
    [SugarColumn(ColumnName = "F_DESCRIPTION")]
    public string Description { get; set; }

    /// <summary>
    /// 排序码.
    /// </summary>
    [SugarColumn(ColumnName = "F_SORTCODE")]
    public long? SortCode { get; set; }
}