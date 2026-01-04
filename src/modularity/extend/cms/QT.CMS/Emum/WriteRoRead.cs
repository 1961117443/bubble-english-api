using System;
using System.Collections.Generic;
using System.Text;

namespace QT.CMS.Emum;

/// <summary>
/// 数据库读写枚举
/// </summary>
public enum WriteRoRead
{
    /// <summary>
    /// 写数据库(主库)
    /// </summary>
    Write,
    /// <summary>
    /// 读数据库(从库)
    /// </summary>
    Read
}
