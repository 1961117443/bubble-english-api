using QT.CMS.Entitys.Dto.Report;
using QT.DependencyInjection;
using System.Linq.Expressions;

namespace QT.CMS;

/// <summary>
/// 数据统计接口实现
/// </summary>
public class ReportService : IReportService,IScoped
{
    public ISqlSugarRepository<Members> _db { get; }

    public ReportService(ISqlSugarRepository<Members> db)
    {
        _db = db;
    }

    /// <summary>
    /// 统计会员注册数量
    /// </summary>
    public async Task<IEnumerable<ReportTotalDto>> GetMemberRegisterCountAsync(int top, Expression<Func<Members, bool>> funcWhere)
    {
        var result = _db.Context.Queryable<Members>()
            .Where(funcWhere)
            .GroupBy(x => new { x.RegTime.Month,x.RegTime.Day })
            .Select (g => new ReportTotalDto { 
                title = $"{g.RegTime.Month}月{g.RegTime.Day}日",
                total = SqlFunc.AggregateCount(g.Id),
                amount = SqlFunc.AggregateSum(g.Amount)
            });
        if (top > 0) result = result.Take(top);//等于0显示所有数据
        return await result.ToListAsync();
    }

    /// <summary>
    /// 统计会员注册分页列表
    /// </summary>
    public async Task<SqlSugarPagedList<Members>> GetMemberRegisterPageAsync(int pageSize, int pageIndex, Expression<Func<Members, bool>> funcWhere, string orderBy)
    {
        var result = _db.AsQueryable()
            .Includes(x => x.User).Where(funcWhere);//条件筛选
        result = result.OrderBy(orderBy);//调用Linq扩展类排序

        var list = await result.ToPagedListAsync(pageIndex, pageSize);

        return list;
    }

    /// <summary>
    /// 统计会员消费金额
    /// </summary>
    public async Task<IEnumerable<ReportTotalDto>> GetMemberAmountCountAsync(int top, Expression<Func<MemberAmountLog, bool>> funcWhere)
    {
        var result = _db.Context.Queryable<MemberAmountLog>()
            .Where(x => x.Value > 0)
            .Where(funcWhere)
            .GroupBy(x => new { x.AddTime.Month, x.AddTime.Day })
            .Select(g => new ReportTotalDto
            {
                title = $"{g.AddTime.Month.ToString()}月{g.AddTime.Day.ToString()}日",
                total = SqlFunc.AggregateCount(g.Id),
                amount = SqlFunc.AggregateSum(g.Value)
            });
        if (top < 0) result = result.Take(top);//等于0显示所有数据
        return await result.ToListAsync();
    }

    /// <summary>
    /// 统计会员消费分页列表
    /// </summary>
    public async Task<SqlSugarPagedList<MemberAmountLog>> GetMemberAmountPageAsync(int pageSize, int pageIndex,
        Expression<Func<MemberAmountLog, bool>> funcWhere, string orderBy)
    {
        //联合查询重新组合
        var result = _db.Context.Queryable<MemberAmountLog>()
            .Where(funcWhere)
            .OrderBy(orderBy);//调用Linq扩展类排序
        var list = await result.ToPagedListAsync(pageIndex, pageSize);

        return list;
    }

    /// <summary>
    /// 统计商品销售数量
    /// </summary>
    public async Task<IEnumerable<ReportTotalDto>> GetGoodsSaleCountAsync(int top, Expression<Func<Orders, bool>> funcWhere)
    {
        var query = _db.Context.Queryable<Orders>().Includes(x => x.OrderGoods).Where(funcWhere);
        if (top > 0) query = query.Take(top);//等于0显示所有数据
        var list = await query.ToListAsync();
        var result = list.GroupBy(x => new { x.AddTime.Month, x.AddTime.Day })
            .Select(g => new ReportTotalDto
            {
                title = $"{g.Key.Month.ToString()}月{g.Key.Day.ToString()}日",
                total = g.Sum(x => x.OrderGoods.Count)
            });
        return result.ToList();

    }

    /// <summary>
    /// 统计商品销售分页列表
    /// </summary>
    public async Task<SqlSugarPagedList<OrderGoods>> GetGoodsSalePageAsync(int pageSize, int pageIndex, Expression<Func<OrderGoods, bool>> funcWhere,
        string orderBy)
    {
        var result = _db.Context.Queryable<OrderGoods>()
            .Includes(x => x.Order)
            .Where(funcWhere)
            .OrderBy(orderBy);//调用Linq扩展类排序
        var list =  await result.ToPagedListAsync(pageIndex, pageSize);
        return list;
        //return PageResult<OrderGoods>.SqlSugarPageResult(list);
    }

    /// <summary>
    /// 统计订单金额
    /// </summary>
    public async Task<IEnumerable<ReportTotalDto>> GetOrderAmountCountAsync(int top, Expression<Func<Orders, bool>> funcWhere)
    {
        var query = _db.Context.Queryable<Orders>().Includes(x => x.OrderGoods).Where(funcWhere);
        if (top > 0) query = query.Take(top);//等于0显示所有数据
        var list = await query.ToListAsync();
        var result = list.GroupBy(x => new { x.AddTime.Month, x.AddTime.Day })
            .Select(g => new ReportTotalDto
            {
                title = $"{g.Key.Month.ToString()}月{g.Key.Day.ToString()}日",
                total = g.Count(),
                amount = g.Sum(x => x.OrderAmount)
            });
        return result.ToList();
    }

    /// <summary>
    /// 统计订单金额分页列表
    /// </summary>
    public async Task<SqlSugarPagedList<Orders>> GetOrderAmountPageAsync(int pageSize, int pageIndex, Expression<Func<Orders, bool>> funcWhere,
        string orderBy)
    {
        var result = _db.Context.Queryable<Orders>()
            .Where(funcWhere)
            .OrderBy(orderBy);//调用Linq扩展类排序
        return await result.ToPagedListAsync(pageIndex, pageSize);
        
    }
}
