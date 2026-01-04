using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Systems.Entitys.System;

/// <summary>
/// 日记.
/// </summary>
//[SugarTable("BASE_SYSLOG")]
[SugarTable("BASE_SYSLOG_{year}{month}{day}")]
[Tenant(ClaimConst.TENANTID)]
[SplitTable(SplitType.Month)]
[SugarIndex("creatortime", nameof(SysLogEntity.CreatorTime), OrderByType.Desc)]
public class SysLogEntity : EntityBase<string>, ICreatorTime
{
    /// <summary>
    /// 日记.
    /// </summary>
    public SysLogEntity()
    {
        CreatorTime = DateTime.Now;
    }

    [SugarColumn(ColumnName = "F_Id", ColumnDescription = "主键", IsPrimaryKey = true, Length = 50,ColumnDataType = "bigint")]
    public override string Id { get; set; }

    /// <summary>
    /// 用户主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_USERID", IsNullable = true, Length = 50)]
    public string UserId { get; set; }

    /// <summary>
    /// 用户名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_USERNAME", IsNullable = true, Length = 100)]
    public string UserName { get; set; }

    /// <summary>
    /// 日志分类
    /// 1.登录日记,2-访问日志,3-操作日志,4-异常日志,5-请求日志.
    /// </summary>
    [SugarColumn(ColumnName = "F_CATEGORY", IsNullable = true)]
    public int? Category { get; set; }

    /// <summary>
    /// 日志类型.
    /// </summary>
    [SugarColumn(ColumnName = "F_TYPE", IsNullable = true)]
    public int? Type { get; set; }

    /// <summary>
    /// 日志级别.
    /// </summary>
    [SugarColumn(ColumnName = "F_LEVEL", IsNullable = true)]
    public int? Level { get; set; }

    /// <summary>
    /// IP地址.
    /// </summary>
    [SugarColumn(ColumnName = "F_IPADDRESS", IsNullable = true, Length = 50)]
    public string IPAddress { get; set; }

    /// <summary>
    /// IP所在城市.
    /// </summary>
    [SugarColumn(ColumnName = "F_IPADDRESSNAME", IsNullable = true, Length = 50)]
    public string IPAddressName { get; set; }

    /// <summary>
    /// 请求地址.
    /// </summary>
    [SugarColumn(ColumnName = "F_REQUESTURL", IsNullable = true, ColumnDataType = "text")]
    public string RequestURL { get; set; }

    /// <summary>
    /// 请求方法.
    /// </summary>
    [SugarColumn(ColumnName = "F_REQUESTMETHOD", IsNullable = true, Length = 50)]
    public string RequestMethod { get; set; }

    /// <summary>
    /// 请求耗时.
    /// </summary>
    [SugarColumn(ColumnName = "F_REQUESTDURATION", IsNullable = true)]
    public int? RequestDuration { get; set; }

    /// <summary>
    /// 日志摘要.
    /// </summary>
    [SugarColumn(ColumnName = "F_ABSTRACTS", IsNullable = true, ColumnDataType = "text")]
    public string Abstracts { get; set; }

    /// <summary>
    /// 日志内容.
    /// </summary>
    [SugarColumn(ColumnName = "F_JSON", IsNullable = true, ColumnDataType = "longtext")]
    public string Json { get; set; }

    /// <summary>
    /// 平台设备.
    /// </summary>
    [SugarColumn(ColumnName = "F_PLATFORM", IsNullable = true, ColumnDataType = "text")]
    public string PlatForm { get; set; }

    /// <summary>
    /// 操作日期.
    /// </summary>
    [SugarColumn(ColumnName = "F_CREATORTIME", IsNullable = true)]
    [SplitField]
    public DateTime? CreatorTime { get; set; }

    /// <summary>
    /// 功能主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_MODULEID", IsNullable = true, Length = 50)]
    public string ModuleId { get; set; }

    /// <summary>
    /// 功能名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_MODULENAME", IsNullable = true, Length = 50)]
    public string ModuleName { get; set; }

    /// <summary>
    /// 对象主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_OBJECTID", IsNullable = true, Length = 50)]
    public string ObjectId { get; set; }
}

/// <summary>
/// 登录日志
/// </summary>
[SugarTable("base_loginlog")]
[Tenant(ClaimConst.TENANTID)]
public class SysLoginLog : SysLogEntity
{

}


/// <summary>
/// 字段变更日记.
/// </summary>
[SugarTable("BASE_SYSLOGFIELD")]
[Tenant(ClaimConst.TENANTID)]
public class SysLogFieldEntity
{
    [SugarColumn(ColumnName = "F_Id", ColumnDescription = "主键", IsPrimaryKey = true,IsIdentity = true, Length = 50, ColumnDataType = "bigint")]
    public long Id { get; set; }

    [SugarColumn(ColumnName = "F_OBJECTID", IsNullable = false, Length = 50)]
    public string ObjectId { get; set; }

    [SugarColumn(ColumnName = "F_TableName", IsNullable = false)]
    public string TableName { get; set; }

    [SugarColumn(ColumnName = "F_FieldName", IsNullable = false)]
    public string FieldName { get; set; }

    [SugarColumn(ColumnName = "F_Description", IsNullable = false)]
    public string Description { get; set; }

    [SugarColumn(ColumnName = "F_OldValue")]
    public string OldValue { get; set; }

    [SugarColumn(ColumnName = "F_NewValue")]
    public string NewValue { get; set; }

    /// <summary>
    /// 操作日期.
    /// </summary>
    [SugarColumn(ColumnName = "F_CREATORTIME", IsNullable = true)]
    public DateTime? CreatorTime { get; set; }

    /// <summary>
    /// 用户主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_UserId", IsNullable = true, Length = 50)]
    public string UserId { get; set; }
}