using QT.Common.Filter;
using System.ComponentModel.DataAnnotations;

namespace QT.Extend.Entitys.Dto.QuoteOrderLog;

public class QuoteOrderLogListPageInput: PageInputBase
{
    /// <summary>
    /// 报价单id
    /// </summary>
    public string fid { get; set; }

    ///// <summary>
    ///// 日志日期
    ///// </summary>
    //public string date { get; set; }
}