using System;
using System.Collections.Generic;
using System.Text;

namespace QT.CMS.Emum;

/// <summary>
/// 数据库集群策略枚举
/// </summary>
public enum Strategy
{
    /// <summary>
    /// 输循策略
    /// </summary>
    Polling,
    /// <summary>
    /// 随机策略
    /// </summary>
    Random
}
