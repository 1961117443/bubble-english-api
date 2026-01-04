using QT.Common.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Logistics.Entitys.Dto.LogArticle;

public class LogArticleNoticeInput : PageInputBase
{
    /// <summary>
    /// 类别：1-园区介绍，2- 招商公告.
    /// </summary>
    public int? type { get; set; }

}
