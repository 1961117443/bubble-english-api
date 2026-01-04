using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Systems.Entitys.System;

/// <summary>
/// 数据连接



/// 日 期：2021-06-01.
/// </summary>
[SugarTable("BASE_DBLINK")]
[Tenant(ClaimConst.TENANTID)]
public class DbLinkEntity : CLDEntityBase
{
    /// <summary>
    /// 连接名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_FULLNAME")]
    public string FullName { get; set; }

    /// <summary>
    /// 连接驱动.
    /// </summary>
    [SugarColumn(ColumnName = "F_DBTYPE")]
    public string DbType { get; set; }

    /// <summary>
    /// 主机名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_HOST")]
    public string Host { get; set; }

    /// <summary>
    /// 端口.
    /// </summary>
    [SugarColumn(ColumnName = "F_PORT")]
    public int? Port { get; set; }

    /// <summary>
    /// 用户.
    /// </summary>
    [SugarColumn(ColumnName = "F_USERNAME")]
    public string UserName { get; set; }

    /// <summary>
    /// 密码.
    /// </summary>
    [SugarColumn(ColumnName = "F_PASSWORD")]
    public string Password { get; set; }

    /// <summary>
    /// 服务名称（ORACLE 用的）.
    /// </summary>
    [SugarColumn(ColumnName = "F_SERVICENAME")]
    public string ServiceName { get; set; }

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

    /// <summary>
    /// 表模式.
    /// </summary>
    [SugarColumn(ColumnName = "F_DBSCHEMA")]
    public string DbSchema { get; set; }

    /// <summary>
    /// 表空间.
    /// </summary>
    [SugarColumn(ColumnName = "F_TABLESPACE")]
    public string TableSpace { get; set; }

    /// <summary>
    /// Oracle参数字段.
    /// </summary>
    [SugarColumn(ColumnName = "F_ORACLEPARAM")]
    public string OracleParam { get; set; }
}