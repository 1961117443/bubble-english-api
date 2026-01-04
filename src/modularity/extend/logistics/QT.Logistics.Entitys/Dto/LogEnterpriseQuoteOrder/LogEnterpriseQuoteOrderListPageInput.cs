using QT.Common.Filter;
using System.ComponentModel.DataAnnotations;

namespace QT.Logistics.Entitys.Dto.LogEnterpriseQuoteOrder;

public class LogEnterpriseQuoteOrderListPageInput: PageInputBase
{
    /// <summary>
    /// 查看次数
    /// </summary>
    public int? minViewCount { get; set; }
}