using QT.Common.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.TaskScheduler;

/// <summary>
/// 定时任务扩展类
/// </summary>
public static class SpareTimerExtensions
{
    /// <summary>
    /// 是否多租户定时任务
    /// </summary>
    /// <param name="timer"></param>
    /// <returns></returns>
    public static bool IsTenantTimer(this SpareTimer timer)
    {
        return KeyVariable.MultiTenancy;
    }

    /// <summary>
    /// 获取当前定时任务所在的租户
    /// </summary>
    /// <param name="timer"></param>
    /// <returns></returns>
    public static string CurrentTenant(this SpareTimer timer)
    {
        if (string.IsNullOrEmpty(timer.Description))
        {
            throw new ArgumentException();
        }
        return timer.Description.Split('/')[0];
    }
}
