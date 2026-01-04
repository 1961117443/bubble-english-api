using QT.Logistics.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Logistics;

/// <summary>
/// 业务实现：数据统计.
/// </summary>
[ApiDescriptionSettings(ModuleConst.Logistics, Tag = "数据统计服务", Name = "LogisticsGraph", Order = 200)]
[Route("api/Logistics/[controller]")]
public class LogisticsGraphService: IDynamicApiController
{
    private readonly ISqlSugarClient _sqlSugarClient;

    public LogisticsGraphService(ISqlSugarClient sqlSugarClient)
    {
        _sqlSugarClient = sqlSugarClient;
    }

    /// <summary>
    /// 近一年物流订单量
    /// </summary>
    /// <returns></returns>
    [HttpGet("wldd")]
    public async Task<dynamic> getWLDD()
    {
        DateTime dateTime = DateTime.Now;
        var start = DateTime.Parse(DateTime.Now.AddYears(-1).AddMonths(1).ToString("yyyy-MM-01"));

        var list = await _sqlSugarClient.Queryable<LogOrderEntity>()
             .Where(it => it.OrderDate > start)
             .GroupBy(it => it.OrderDate.Value.ToString("yyyy年MM月"))
             .Select(it => new
             {
                 month = it.OrderDate.Value.ToString("yyyy年MM月"),
                 count = SqlFunc.AggregateCount(it.Id)
             })
             .ToListAsync();

        string[] xAxis = new string[12];
        int[] data = new int[12];
        for (int i = 0; i < 12; i++)
        {
            var date = start.AddMonths(i).ToString("yyyy年MM月");
            xAxis[i] = date;

            var item = list.Find(it => it.month == date);

            data[i] = item?.count ?? 0;
        }

        return new
        {
            xAxis,
            data
        };
    }

    /// <summary>
    /// 配送点物流订单占比
    /// </summary>
    /// <returns></returns>
    [HttpGet("psddd")]
    public async Task<dynamic> getPSDDD()
    {
        var list = await _sqlSugarClient.Queryable<LogOrderEntity>()
             .GroupBy(it => it.SendPointId)
             .Select(it => new
             {
                 name = SqlFunc.Subqueryable<LogDeliveryPointEntity>().Where(x=>x.Id == it.SendPointId).Select(x=>x.Name),
                 value = SqlFunc.AggregateCount(it.Id)
             })
             .ToListAsync();

        string[] xAxis = new string[list.Count];
        //int[] data = new int[list.Count];
        for (int i = 0; i < list.Count; i++)
        {
            xAxis[i] = list[i].name;
            //data[i] = list[i].count;
        }
        
        return new
        {
            xAxis,
            data = list
        };
    }
}
