using QT.Systems.Entitys.Permission;

namespace QT.JXC;


/// <summary>
/// 业务实现：统计数据.
/// </summary>
[ApiDescriptionSettings(ModuleConst.ERP, Tag = "Erp", Name = "ErpGraph", Order = 200)]
[Route("api/Erp/[controller]")]
public class ErpGraphService: IDynamicApiController
{
    private readonly ISqlSugarClient _db;

    public ErpGraphService(ISqlSugarClient db)
    {
        _db = db;
    }

    /// <summary>
    /// 统计订单数据
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("Order")]
    public async Task<dynamic> GraphOrder([FromQuery] ErpOrderListQueryInput input)
    {
        List<DateTime> posttimeRange = input.posttimeRange?.Split(',').ToObject<List<DateTime>>();
        DateTime startPosttimeDate = posttimeRange?.First() ?? new DateTime(2024,9,1);
        DateTime endPosttimeDate = posttimeRange?.Last() ?? DateTime.Now;

        var list = await _db.Queryable<ErpOrderEntity>()
            .ClearFilter()
            .Where(x => SqlFunc.Between(x.Posttime, startPosttimeDate, endPosttimeDate))
            .Select(x => new
            {
                oid = x.Oid,
                mon = x.Posttime.Value.ToString("yyyy-MM"),
                amount = x.Amount
            })
            .MergeTable()
            .GroupBy(a => new { a.oid, a.mon })
            .Select(a => new
            {
                oidName = SqlFunc.Subqueryable<OrganizeEntity>().Where(ddd=>ddd.Id == a.oid).Select(ddd=>ddd.FullName),
                a.mon,
                amount = SqlFunc.AggregateSum(a.amount)
            })
            .ToListAsync();

        var output = new
        {
            legend = new List<string>(),
            xAxis = new[]
            {
                new { type = "category", data = new List<string>()}
            },
            series = new List<object>()
            //{
            //    new { type = "bar", data = new List<decimal>()}
            //}
        };

        var xAxis = new List<string>();
        for (int i = 0; i < 12; i++)
        {
            var dt = endPosttimeDate.AddMonths(i * -1);
            xAxis.Add(dt.ToString("yyyy-MM"));

            if (xAxis.Count == 12 || dt.ToString("yyyy-MM") == startPosttimeDate.ToString("yyyy-MM"))
            {
                break;
            }
        }
        foreach (var item in xAxis.OrderBy(x => x))
        {
            output.xAxis[0].data.Add(item);
        }


        // 只显示最近12个月的数据
        foreach (var g in list.GroupBy(x => x.oidName))
        {
            output.legend.Add(g.Key);

            var xdata = new List<decimal>();

            foreach (var item in output.xAxis[0].data)
            {
                xdata.Add(g.FirstOrDefault(x => x.mon == item)?.amount ?? 0);
            } 

            //g.OrderBy(x => x.mon);

            output.series.Add(new
            {
                type = "bar",
                data = new List<decimal>(xdata),
                name = g.Key
            });
        }

        return output;
    }
}
