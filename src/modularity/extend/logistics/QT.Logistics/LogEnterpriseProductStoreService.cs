using NPOI.POIFS.Crypt.Dsig;
using QT.Common.Extension;
using QT.Logistics.Entitys;
using QT.Logistics.Entitys.Dto.LogEnterpriseProductStore;
using QT.Logistics.Interfaces;

namespace QT.Logistics;

/// <summary>
/// 业务实现：商家商品库存.
/// </summary>
[ApiDescriptionSettings(ModuleConst.Logistics, Tag = "商家商品库存服务", Name = "LogEnterpriseProductStore", Order = 200)]
[Route("api/Logistics/[controller]")]
public class LogEnterpriseProductStoreService : ILogEnterpriseProductStoreService, IDynamicApiController,ITransient
{
    private readonly ISqlSugarRepository<LogEnterpriseProductEntity> _repository;

    public LogEnterpriseProductStoreService(ISqlSugarRepository<LogEnterpriseProductEntity> repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// 获取商家商品库存列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] LogEnterpriseProductStoreListQueryInput input)
    { 
        var data = await _repository.Context.Queryable<LogEnterpriseProductEntity>()
            .InnerJoin<LogEnterpriseProductmodelEntity>((a, b) => a.Id == b.Pid)
            .WhereIF(!string.IsNullOrEmpty(input.tid), a => a.Tid.Equals(input.tid))
            .WhereIF(!string.IsNullOrEmpty(input.name), (a, b) => a.Name.Contains(input.name))
            .WhereIF(!string.IsNullOrEmpty(input.firstChar), a => a.FirstChar.Contains(input.firstChar))
            //.WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
            //    it.Tid.Contains(input.keyword)
            //    || it.Name.Contains(input.keyword)
            //    || it.FirstChar.Contains(input.keyword)
            //    )
            //.OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select((a, b) => new LogEnterpriseProductStoreListOutput
            {
                id = b.Id,
                tid = a.Tid,
                productName = a.Name,
                name = b.Name,
                firstChar = a.FirstChar,
                producer = a.Producer,
                barCode = b.BarCode,
                unit = b.Unit,
                //remark = it.Remark,
                storage = a.Storage,
                retention = a.Retention,
                state = a.State ?? 0,
                tidName = SqlFunc.Subqueryable<LogEnterpriseProducttypeEntity>().Where(x => x.Id == a.Tid).Select(x => x.Name),
                eIdName = SqlFunc.Subqueryable<LogEnterpriseEntity>().Where(x => x.Id == a.EId).Select(x => x.Name)
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);

        if (data.list.IsAny())
        {
            //var gidList = data.list.Select(it => it.id).ToList();
            //// 统计库存数
            //var inList = _repository.Context.Queryable<LogEnterpriseInrecordEntity>()
            //    .Where(it => gidList.Contains(it.Gid))
            //    .Select(it => new LogEnterpriseProductStoreListOutput { id = it.Gid, storeNum = it.InNum });
            //var outList = _repository.Context.Queryable<LogEnterpriseOutrecordEntity>()
            //    .Where(it => gidList.Contains(it.Gid))
            //    .Select(it => new LogEnterpriseProductStoreListOutput { id = it.Gid, storeNum = it.Num * (-1) });

            //var storeList = await _repository.Context.UnionAll(inList, outList).MergeTable()
            //    .GroupBy(it => it.id)
            //    .Select(it => new
            //    {
            //        Gid = it.id,
            //        Num = SqlFunc.AggregateSum(it.storeNum)
            //    })
            //    .ToListAsync();

            var storeList = await GetLogEnterpriseProductStoreSumAsync(new LogEnterpriseProductStoreDetailQueryInput
            {
                gids = data.list.Select(it => it.id).ToList()
            });

            if (storeList.IsAny())
            {
                foreach (var item in data.list)
                {
                    item.storeNum = storeList.Find(x => x.id == item.id)?.storeNum ?? 0;
                }
            }
        }

        return PageResult<LogEnterpriseProductStoreListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 获取库存查询语句
    /// </summary>
    /// <param name="logEnterpriseProductStoreDetailQueryInput"></param>
    /// <returns></returns>
    private ISugarQueryable<LogEnterpriseProductStoreDetailListOutput> CreateQueryable(LogEnterpriseProductStoreDetailQueryInput logEnterpriseProductStoreDetailQueryInput)
    {
        // 统计库存数
        var inList = _repository.Context.Queryable<LogEnterpriseInrecordEntity>()
            .InnerJoin<LogEnterpriseInorderEntity>((a, b) => a.InId == b.Id)
            .Where((a, b) => logEnterpriseProductStoreDetailQueryInput.gids.Contains(a.Gid))
            .WhereIF(!string.IsNullOrEmpty(logEnterpriseProductStoreDetailQueryInput.storeRoomId), (a, b) => a.StoreRomeId == logEnterpriseProductStoreDetailQueryInput.storeRoomId)
            .Select((a, b) => new LogEnterpriseProductStoreDetailListOutput
            {
                id = a.Gid,
                inNum = a.InNum,
                billDate = b.InTime,
                billType = b.InType,
                outNum = 0,
                storeRoomId = a.StoreRomeId,
                category = 1
            });

        var outList = _repository.Context.Queryable<LogEnterpriseOutrecordEntity>()
            .InnerJoin<LogEnterpriseOutorderEntity>((a, b) => a.OutId == b.Id)
            .Where((a, b) => logEnterpriseProductStoreDetailQueryInput.gids.Contains(a.Gid))
            .WhereIF(!string.IsNullOrEmpty(logEnterpriseProductStoreDetailQueryInput.storeRoomId), (a, b) => a.StoreRomeId == logEnterpriseProductStoreDetailQueryInput.storeRoomId)
            .Select((a, b) => new LogEnterpriseProductStoreDetailListOutput
            {
                id = a.Gid,
                inNum = 0,
                billDate = b.OutTime,
                billType = b.OutType,
                outNum = a.Num,
                storeRoomId = a.StoreRomeId,
                category = -1
            });

        return _repository.Context.UnionAll(inList, outList).MergeTable();
    }
    /// <summary>
    /// 获取库存明细
    /// </summary>
    /// <param name="logEnterpriseProductStoreDetailQueryInput"></param>
    /// <returns></returns>
    public async Task<List<LogEnterpriseProductStoreDetailListOutput>> GetLogEnterpriseProductStoreDetailListAsync(LogEnterpriseProductStoreDetailQueryInput logEnterpriseProductStoreDetailQueryInput)
    {
        var qur = CreateQueryable(logEnterpriseProductStoreDetailQueryInput);

        var storeList = await qur.OrderBy(it => it.billDate).ToListAsync();

        return storeList;
    }

    /// <summary>
    /// 统计库存数量
    /// </summary>
    /// <param name="logEnterpriseProductStoreDetailQueryInput"></param>
    /// <returns></returns>
    [NonAction]
    public Task<List<LogEnterpriseProductStoreSumOutput>> GetLogEnterpriseProductStoreSumAsync(LogEnterpriseProductStoreDetailQueryInput logEnterpriseProductStoreDetailQueryInput)
    {
        return CreateQueryable(logEnterpriseProductStoreDetailQueryInput)
            .GroupBy(it => it.id)
            .Select(it => new LogEnterpriseProductStoreSumOutput
            {
                id = it.id,
                storeNum = SqlFunc.AggregateSum(it.inNum - it.outNum)
            })
            .ToListAsync();
    }
}
