using QT.Common.Filter;
using System.ComponentModel.DataAnnotations;

namespace QT.Extend.Entitys.Dto.QuoteOrder;

public class QuoteOrderListPageInput: PageInputBase
{
    /// <summary>
    /// 查看次数
    /// </summary>
    public int? minViewCount { get; set; }
}