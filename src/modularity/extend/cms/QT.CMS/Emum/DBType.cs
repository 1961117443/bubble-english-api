using System;
using System.Collections.Generic;
using System.Text;

namespace QT.CMS.Emum;

/// <summary>
/// 数据库类型
/// </summary>
public enum DBType
{
    /// <summary>
    /// MySql
    /// </summary>
    MySql,
    /// <summary>
    /// SqlServer2012及以上
    /// </summary>
    SqlServer,
    /// <summary>
    /// Sqlite
    /// </summary>
    Sqlite,
    /// <summary>
    /// Oracle
    /// </summary>
    Oracle,
    /// <summary>
    /// PostgreSQL
    /// </summary>
    PostgreSQL
}
