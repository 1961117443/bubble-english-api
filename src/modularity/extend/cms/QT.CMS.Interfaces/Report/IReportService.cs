using QT.CMS.Entitys;
using QT.CMS.Entitys.Dto.Report;
using QT.Common.Filter;
using SqlSugar;
using System.Linq.Expressions;

namespace QT.CMS.Interfaces;

/// <summary>
/// 数据统计接口
/// </summary>
public interface IReportService
{
    /// <summary>
    /// 统计会员注册数量
    /// </summary>
    Task<IEnumerable<ReportTotalDto>> GetMemberRegisterCountAsync(int top, Expression<Func<Members, bool>> funcWhere);

    /// <summary>
    /// 统计会员注册分页列表
    /// </summary>
    Task<SqlSugarPagedList<Members>> GetMemberRegisterPageAsync(int pageSize, int pageIndex, Expression<Func<Members, bool>> funcWhere, string orderBy);

    /// <summary>
    /// 统计会员消费金额
    /// </summary>
    Task<IEnumerable<ReportTotalDto>> GetMemberAmountCountAsync(int top, Expression<Func<MemberAmountLog, bool>> funcWhere);

    /// <summary>
    /// 统计会员消费分页列表
    /// </summary>
    Task<SqlSugarPagedList<MemberAmountLog>> GetMemberAmountPageAsync(int pageSize, int pageIndex,
        Expression<Func<MemberAmountLog, bool>> funcWhere, string orderBy);

    /// <summary>
    /// 统计商品销售数量
    /// </summary>
    Task<IEnumerable<ReportTotalDto>> GetGoodsSaleCountAsync(int top, Expression<Func<Orders, bool>> funcWhere);

    /// <summary>
    /// 统计商品销售分页列表
    /// </summary>
    Task<SqlSugarPagedList<OrderGoods>> GetGoodsSalePageAsync(int pageSize, int pageIndex, Expression<Func<OrderGoods, bool>> funcWhere,string orderBy);

    /// <summary>
    /// 统计订单金额
    /// </summary>
    Task<IEnumerable<ReportTotalDto>> GetOrderAmountCountAsync(int top, Expression<Func<Orders, bool>> funcWhere);

    /// <summary>
    /// 统计订单金额分页列表
    /// </summary>
    Task<SqlSugarPagedList<Orders>> GetOrderAmountPageAsync(int pageSize, int pageIndex, Expression<Func<Orders, bool>> funcWhere, string orderBy);
}
