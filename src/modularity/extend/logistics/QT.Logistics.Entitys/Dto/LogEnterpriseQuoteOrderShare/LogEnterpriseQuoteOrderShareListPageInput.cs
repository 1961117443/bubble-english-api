using QT.Common.Filter;
using System.ComponentModel.DataAnnotations;

namespace QT.Logistics.Entitys.Dto.LogEnterpriseQuoteOrderShare;

public class LogEnterpriseQuoteOrderShareListPageInput: PageInputBase
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