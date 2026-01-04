namespace QT.CMS.Entitys.Dto.Parameter;

/// <summary>
/// 分页参数
/// </summary>
public class PageParamater
{
    const int maxPageSize = 100;
    private int _pageIndex = 1;
    private int _pageSize = 10;

    /// <summary>
    /// 当前页码
    /// </summary>
    public int PageIndex
    {
        get
        {
            return _pageIndex;
        }
        set
        {
            if (value >= 1)
            {
                _pageIndex = value;
            }
        }
    }
    /// <summary>
    /// 页面大小
    /// </summary>
    public int PageSize
    {
        get
        {
            return _pageSize;
        }
        set
        {
            if (value >= 1)
            {
                _pageSize = (value > maxPageSize) ? maxPageSize : value;
            }
        }
    }
}
