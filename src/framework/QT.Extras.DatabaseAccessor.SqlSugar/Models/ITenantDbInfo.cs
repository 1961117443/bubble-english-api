using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Extras.DatabaseAccessor.SqlSugar;

/// <summary>
/// 租户数据库信息
/// </summary>
public interface ITenantDbInfo
{
    /// <summary>
    /// 开始时间(带毫秒的时间戳)
    /// </summary>
    long startTime { get; set; }

    /// <summary>
    /// 结束时间(带毫秒的时间戳)
    /// </summary>
    long endTime { get; set; }

    /// <summary>
    /// 租户编号
    /// </summary>
    string enCode { get; set; }

    /// <summary>
    /// 是否正常
    /// </summary>
    bool enable { get; }
}
