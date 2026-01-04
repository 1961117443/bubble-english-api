using System;
using System.Collections.Generic;
using System.Text;

namespace QT.CMS.Entitys.Dto.Parameter;

/// <summary>
/// 友情链接查询参数
/// </summary>
public class LinkParameter : BaseParameter
{
    /// <summary>
    /// 是否推荐
    /// </summary>
    public int IsRecom { get; set; } = -1;

    /// <summary>
    /// 是否图片链接
    /// </summary>
    public int IsImage { get; set; } = -1;
}
