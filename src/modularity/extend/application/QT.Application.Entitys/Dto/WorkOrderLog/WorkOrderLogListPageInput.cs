using QT.Common.Filter;
using System.ComponentModel.DataAnnotations;

namespace QT.Extend.Entitys.Dto.WorkOrderLog;

public class WorkOrderLogListPageInput: PageInputBase
{
    /// <summary>
    /// 工单id
    /// </summary>
    public string wid { get; set; }

    ///// <summary>
    ///// 日志日期
    ///// </summary>
    //public string date { get; set; }
}