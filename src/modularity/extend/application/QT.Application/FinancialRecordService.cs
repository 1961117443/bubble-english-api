using QT.Application.Entitys;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.Extend.Entitys.Dto.FinancialRecord;
using QT.LinqBuilder;
using SqlSugar.Extensions;
using System.Linq.Expressions;

namespace QT.Application;

/// <summary>
/// 个人记账
/// </summary>
[ApiDescriptionSettings("乾通ERP.V2", Tag = "个人记账", Name = "FinancialRecord", Order = 600)]
[Route("api/extend/[controller]")]
public class FinancialRecordService : QTBaseService<FinancialRecordEntity, FinancialRecordCrInput, FinancialRecordUpInput, FinancialRecordInfoOutput, FinancialRecordListPageInput, FinancialRecordListOutput>, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<FinancialRecordEntity> _repository;
    private readonly IUserManager _userManager;

    public FinancialRecordService(ISqlSugarRepository<FinancialRecordEntity> repository, ISqlSugarClient context, IUserManager userManager) : base(repository, context, userManager)
    {
        _repository = repository;
        _userManager = userManager;
    }

    public override async Task<PageResult<FinancialRecordListOutput>> GetList([FromQuery] FinancialRecordListPageInput input)
    {
        List<DateTime> billDateRange = input.billDateRange?.Split(',').ToObject<List<DateTime>>();
        DateTime? startDate = billDateRange?.First();
        DateTime? endDate = billDateRange?.Last();

        Expression<Func<FinancialRecordEntity, bool>> labelWhere = null;
        if (input.label.IsNotEmptyOrNull())
        {
            foreach (var label in input.label.Split(",", true))
            {
                if (labelWhere == null)
                {
                    labelWhere = it => QTSqlFunc.FIND_IN_SET(label, it.Label);
                }
                else
                {
                    labelWhere = labelWhere.Or(it => QTSqlFunc.FIND_IN_SET(label, it.Label));
                }
            }
        }

        var data = await _repository.Context.Queryable<FinancialRecordEntity>()
            .Where(x=>x.CreatorUserId == _userManager.UserId)
            .WhereIF(input.category.HasValue,x=>x.Category == input.category)
            .WhereIF(billDateRange != null, it => SqlFunc.Between(it.BillDate, startDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), endDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .WhereIF(labelWhere!=null, labelWhere)
           .Select<FinancialRecordListOutput>()
           .ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<FinancialRecordListOutput>.SqlSugarPagedList(data);

    }

    #region 当前用户 current
    /// <summary>
    /// 当前用户的支出，未报销列表
    /// </summary>
    /// <returns></returns>
    [HttpGet("current/pay")]
    public async Task<List<FinancialRecordListOutput>> GetCurrentPayList([FromQuery] FinancialRecordListPageInput input)
    {
        var list = await _repository.Context.Queryable<FinancialRecordEntity>()
          .Where(x => x.CreatorUserId == _userManager.UserId && x.Category == 0)
          .Where(x => SqlFunc.Subqueryable<ExtExpenseDetailEntity>().Where(a => a.InId == x.Id).NotAny()) // 还没做报销的记录
         .Select<FinancialRecordListOutput>()
         .ToListAsync();

        return list;
    }

    /// <summary>
    /// 当前用户每月收支统计
    /// </summary>
    /// <returns></returns>
    [HttpGet("current/statistics/mon")]
    public async Task<EchartOutput> GetCurrentFinancialListByMon([FromQuery] FinancialRecordListPageInput input)
    {
        var list = await _repository.Context.Queryable<FinancialRecordEntity>()
          .Where(x => x.CreatorUserId == _userManager.UserId)
          .GroupBy(x => new { Category=x.Category, mon=x.BillDate.Value.ToString("yyyy-MM") })
          .Select(x=> new FinancialRecordMonthlyStatistics
          {
              category = x.Category,
              amount = SqlFunc.AggregateSum(x.Amount ?? 0),
              mon = x.BillDate.Value.ToString("yyyy-MM")
          })
         .ToListAsync();

        EchartOutput echartOutput = new EchartOutput()
        {
            xAxis = new XAxis
            {
                data = new List<string>()
            },
            series = new List<SerieItem>()
        };

        foreach (var g in list.GroupBy(x => x.mon).OrderBy(x => x.Key))
        {
            echartOutput.xAxis.data.Add(g.Key);
        }

        echartOutput.series.Add(new SerieItem
        {
            name = "支出",
            type = "line",
            data = echartOutput.xAxis.data.Select(x => (object)(list.FirstOrDefault(w => w.mon == x && w.category == 0)?.amount ?? 0m)).ToList()
        });
        echartOutput.series.Add(new SerieItem
        {
            name = "收入",
            type = "line",
            data = echartOutput.xAxis.data.Select(x => (object)(list.FirstOrDefault(w => w.mon == x && w.category == 1)?.amount ?? 0m)).ToList()
        });
        return echartOutput;
    }

    /// <summary>
    /// 当前用户每月收支统计
    /// </summary>
    /// <returns></returns>
    [HttpGet("current/statistics/label")]
    public async Task<EchartOutput> GetCurrentFinancialListByLabel([FromQuery] FinancialRecordListPageInput input)
    {
        var list = await _repository.Context.Queryable<FinancialRecordEntity>()
          .Where(x => x.CreatorUserId == _userManager.UserId)
          .Where(x=>x.Category == input.category)
          .Where(x=> !SqlFunc.IsNullOrEmpty(x.Label))
          .GroupBy(x => x.Label)
          .Select(x => new FinancialRecordEntity
          {
              Label = x.Label,
              Amount = SqlFunc.AggregateSum(x.Amount ?? 0)
          })
         .ToListAsync();

        // 发生最多的10个标签
        var sumListTop10  = list.SelectMany(x =>
        {
            if (x.Label.IsNotEmptyOrNull())
            {
                return x.Label.Split(",", true).Select(label => new FinancialRecordEntity
                {
                    Label = label,
                    Amount = x.Amount
                });
            }
            else
            {
                return new List<FinancialRecordEntity>() { x };
            }
        })
            .GroupBy(x => x.Label)
            .Select(x => new FinancialRecordInfoOutput
            {
                label = x.Key,
                amount = x.Sum(g => g.Amount ?? 0)
            })
            .OrderByDescending(x=>x.amount)
            .Take(10)
            .ToList();

        //return sumListTop10.Select(x => new
        //{
        //    label = x.label,
        //    amount = x.amount
        //}).ToList();

        EchartOutput echartOutput = new EchartOutput()
        {
            xAxis = new XAxis
            {
                data = new List<string>()
            },
            series = new List<SerieItem>()
            {
               
            }
        };
        var serie = new SerieItem()
        {
            data= new List<object>()
        };
        echartOutput.series.Add(serie);
        foreach (var g in sumListTop10)
        {
            echartOutput.xAxis.data.Add(g.label);

            serie.data.Add(new { value = g.amount, name = g.label });
        }

       
        return echartOutput;
    }
    #endregion
}
