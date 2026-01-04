using QT.DependencyInjection;

namespace QT.Common.Extension;

/// <summary>
/// 时间<see cref="DateTime"/>类型的扩展辅助操作类.
/// </summary>
[SuppressSniffer]
public static partial class Extensions
{
    #region 时间判断

    /// <summary>
    /// 判断时间是否在某个时间段内.
    /// </summary>
    /// <param name="nowTime">数据时间.</param>
    /// <param name="beginHm">查询开始时间.</param>
    /// <param name="endHm">查询结束时间.</param>
    /// <param name="type">0-yyyy-MM-dd,1-yyyy-MM,2-yyyy.</param>
    /// <returns></returns>
    public static bool IsInTimeRange(DateTime nowTime, string beginHm, string endHm, int type = 0)
    {
        DateTime start = new DateTime();
        DateTime end = new DateTime();
        switch (type)
        {
            case 1:
                {
                    DateTime beginTime = beginHm.ParseToDateTime();
                    DateTime endTime = endHm.ParseToDateTime();
                    start = new DateTime(beginTime.Year, beginTime.Month, 1, 0, 0, 0, 0);
                    end = new DateTime(endTime.Year, endTime.Month, DateTime.DaysInMonth(endTime.Year, endTime.Month), 23, 59, 59, 999);
                }

                break;
            case 2:
                {
                    DateTime beginTime = beginHm.ParseToDateTime();
                    DateTime endTime = endHm.ParseToDateTime();
                    start = new DateTime(beginTime.Year, 1, 1, 0, 0, 0, 0);
                    end = new DateTime(endTime.Year, 12, 31, 23, 59, 59, 999);
                }

                break;
            case 0:
                {
                    DateTime beginTime = beginHm.ParseToDateTime();
                    DateTime endTime = endHm.ParseToDateTime();
                    start = new DateTime(beginTime.Year, beginTime.Month, beginTime.Day, 0, 0, 0, 0);
                    end = new DateTime(endTime.Year, endTime.Month, endTime.Day, 23, 59, 59, 999);
                }

                break;
            case 3:
                {
                    DateTime beginTime = beginHm.ParseToDateTime();
                    DateTime endTime = endHm.ParseToDateTime();
                    start = new DateTime(beginTime.Year, beginTime.Month, beginTime.Day, beginTime.Hour, beginTime.Minute, beginTime.Second, 0);
                    end = new DateTime(endTime.Year, endTime.Month, endTime.Day, endTime.Hour, endTime.Minute, endTime.Second, 999);
                }

                break;
        }

        if (ParseToUnixTime(nowTime) >= ParseToUnixTime(start) && ParseToUnixTime(nowTime) <= ParseToUnixTime(end))
            return true;

        return false;
    }

    /// <summary>
    /// 时间判断.
    /// </summary>
    /// <param name="nowTime">数据时间.</param>
    /// <param name="dayTimeStart">查询开始时间.</param>
    /// <param name="dayTimeEnd">查询结束时间.</param>
    /// <returns></returns>
    public static bool timeCalendar(string nowTime, string dayTimeStart, string dayTimeEnd)
    {
        // 设置当前时间
        DateTime date = nowTime.ParseToDateTime();

        // 设置开始时间
        DateTime timeStart = dayTimeStart.ParseToDateTime();
        timeStart = new DateTime(timeStart.Year, timeStart.Month, timeStart.Day, 0, 0, 0, 0);

        // 设置结束时间
        DateTime timeEnd = dayTimeEnd.ParseToDateTime();
        timeEnd = new DateTime(timeEnd.Year, timeEnd.Month, timeEnd.Day, 23, 59, 59, 999);

        // 当date > timeStart时，date.CompareTo(timeStart)返回 1
        // 当date = timeStart时，date.CompareTo(timeStart)返回 0
        // 当date < timeStart时，date.CompareTo(timeStart)返回 -1
        if (DateTime.Compare(date, timeStart) >= 0 && DateTime.Compare(date, timeEnd) <= 0)
            return true;

        return false;
    }

    #endregion
}