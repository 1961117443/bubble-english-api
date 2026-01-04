using QT.DependencyInjection;
using Mapster;
using SqlSugar;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QT.Common.Filter;

/// <summary>
/// 分页结果.
/// </summary>
[SuppressSniffer]
public class PageResult<T>
{
    /// <summary>
    /// 分页实体.
    /// </summary>
    public PageResult pagination { get; set; }

    /// <summary>
    /// 数据.
    /// </summary>
    public List<T> list { get; set; }

    /// <summary>
    /// 替换sqlsugar分页.
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    public static dynamic SqlSugarPageResult(SqlSugarPagedList<T> page)
    {
        return new
        {
            pagination = page.pagination.Adapt<PageResult>(),
            list = page.list
        };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TS"></typeparam>
    /// <param name="page"></param>
    /// <returns></returns>
    public static dynamic SqlSugarPageResult<TS>(SqlSugarPagedList<TS> page)
    {
        return new
        {
            pagination = page.pagination.Adapt<PageResult>(),
            list = page.list.Adapt<List<T>>()
        };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TS"></typeparam>
    /// <param name="page"></param>
    /// <returns></returns>
    public static PageResult<T> SqlSugarPagedList<TS>(SqlSugarPagedList<TS> page)
    {
        return new PageResult<T>
        {
            pagination = page.pagination.Adapt<PageResult>(),
            list = page.list.Adapt<List<T>>()
        };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    public static PageResult<T> SqlSugarPagedList(SqlSugarPagedList<T> page)
    {
        return new PageResult<T>
        {
            pagination = page.pagination.Adapt<PageResult>(),
            list = page.list.ToList()
        };
    }
}

/// <summary>
/// 分页结果.
/// </summary>
[SuppressSniffer]
public class PageResult
{
    /// <summary>
    /// 页码.
    /// </summary>
    public int pageIndex { get; set; }

    /// <summary>
    /// 页容量.
    /// </summary>
    public int pageSize { get; set; }

    /// <summary>
    /// 总条数.
    /// </summary>
    public int total { get; set; }
}