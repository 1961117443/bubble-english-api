using QT.DependencyInjection;
using SqlSugar;

namespace QT.Systems.Entitys.Dto.SysLog;

/// <summary>
/// 请求日记输出
/// </summary>
[SuppressSniffer]
public class LogRequestOutput
{
    /// <summary>
    /// id.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 请求时间.
    /// </summary>
    public DateTime? creatorTime { get; set; }

    /// <summary>
    /// 请求用户名.
    /// </summary>
    public string userName { get; set; }

    /// <summary>
    /// 请求IP.
    /// </summary>
    public string ipaddress { get; set; }

    /// <summary>
    /// 请求设备.
    /// </summary>
    public string platForm { get; set; }

    /// <summary>
    /// 请求地址.
    /// </summary>
    public string requestURL { get; set; }

    /// <summary>
    /// 请求类型.
    /// </summary>
    public string requestMethod { get; set; }

    /// <summary>
    /// 请求耗时.
    /// </summary>
    public int requestDuration { get; set; }
}

public class SysLogDiffOutput
{
    /// <summary>
    /// 雪花Id
    /// </summary>
    public virtual string id { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public virtual DateTime? createTime { get; set; }
     
    /// <summary>
    /// 创建者Id
    /// </summary>
    public virtual string? createUserId { get; set; }

    /// <summary>
    /// 创建者Id
    /// </summary>
    public virtual string? createUserIdName { get; set; }

    ///// <summary>
    ///// 软删除
    ///// </summary>
    //public virtual bool IsDelete { get; set; } = false;
    /// <summary>
    /// 操作前记录
    /// </summary>
    public string? beforeData { get; set; }

    /// <summary>
    /// 操作后记录
    /// </summary>
    public string? afterData { get; set; }

    /// <summary>
    /// Sql
    /// </summary>
    public string? sql { get; set; }

    /// <summary>
    /// 参数  手动传入的参数
    /// </summary>
    public string? parameters { get; set; }

    /// <summary>
    /// 业务对象
    /// </summary>
    public string? businessData { get; set; }

    /// <summary>
    /// 差异操作
    /// </summary>
    public string? diffType { get; set; }

    /// <summary>
    /// 耗时
    /// </summary>
    public long? elapsed { get; set; }
}