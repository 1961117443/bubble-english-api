using Mapster;
using QT.Common.Core.Manager.Files;
using QT.Common.Extension;
using QT.JXC.Entitys.Dto.ErpOrderCwdd;
using QT.Systems.Entitys.Permission;
using QT.Systems.Interfaces.Permission;
using QT.Systems.Interfaces.System;

namespace QT.JXC;

/// <summary>
/// 业务实现：订单信息--财务打单.
/// </summary>
[ApiDescriptionSettings(ModuleConst.ERP, Tag = "Erp", Name = "ErpOrderCwdd", Order = 200)]
[Route("api/Erp/[controller]")]
public class ErpOrderCwddService: IDynamicApiController
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<ErpOrderEntity> _repository;

    /// <summary>
    /// 单据规则服务.
    /// </summary>
    private readonly IBillRullService _billRullService;

    /// <summary>
    /// 多租户事务.
    /// </summary>
    private readonly ITenant _db;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;
    private readonly IDictionaryDataService _dictionaryDataService;
    private readonly IFileManager _fileManager;
    //private readonly ICache _cache;
    private readonly IUsersService _usersService;

    /// <summary>
    /// 初始化一个<see cref="ErpOrderService"/>类型的新实例.
    /// </summary>
    public ErpOrderCwddService(
        ISqlSugarRepository<ErpOrderEntity> erpOrderRepository,
        IBillRullService billRullService,
        ISqlSugarClient context,
        IUserManager userManager, IDictionaryDataService dictionaryDataService, IFileManager fileManager,
        IUsersService usersService, ICacheManager cacheManager)
    {
        _repository = erpOrderRepository;
        _billRullService = billRullService;
        _db = context.AsTenant();
        _userManager = userManager;
        _dictionaryDataService = dictionaryDataService;
        _fileManager = fileManager;
        //_cache = cache;
        _usersService = usersService;

        // 清楚全局过滤条件
        if (_repository.Context.QueryFilter.GeFilterList.Any())
        {
            _repository.Context.QueryFilter.Clear<ICompanyEntity>();
        }
    }

    /// <summary>
    /// 获取订单信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var output = (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<ErpOrderInfoOutput>();

        var erpOrderdetailList = await _repository.Context.Queryable<ErpOrderdetailEntity>()
            .InnerJoin<ErpProductmodelEntity>((w, x) => w.Mid == x.Id)
            .InnerJoin<ErpProductEntity>((w, x, b) => x.Pid == b.Id)
            //.ClearFilter<ICompanyEntity>()
            .Where(w => w.Fid == output.id)
            .Select((w, x, b) => new ErpOrderdetailCwddInfoOutput()
            {
                midName = x.Name,
                productName = b.Name,
                rootProducttype = SqlFunc.Subqueryable<ViewErpProducttypeEx>().Where(ddd => ddd.Id == b.Tid).Select(ddd => ddd.RootName)
            }, true)
            .ToListAsync();

        erpOrderdetailList.ForEach(x =>
        {
            if (!x.printNum.HasValue)
            {
                x.printNum = x.num1;
                x.printPrice = x.salePrice;
                x.printAmount = x.amount1;
            }
        });


        output.erpOrderdetailList = erpOrderdetailList.OrderBy(x => x.order ?? 99).Select(x=> (ErpOrderdetailInfoOutput)x).ToList(); //.Adapt<List<ErpOrderdetailInfoOutput>>();

        var erpOrderoperaterecordList = await _repository.Context.Queryable<ErpOrderoperaterecordEntity>().Where(w => w.Fid == output.id).ToListAsync();
        output.erpOrderoperaterecordList = erpOrderoperaterecordList.Adapt<List<ErpOrderoperaterecordInfoOutput>>();

        var erpOrderremarksList = await _repository.Context.Queryable<ErpOrderremarksEntity>().Where(w => w.OrderId == output.id).ToListAsync();
        output.erpOrderremarksList = erpOrderremarksList.Adapt<List<ErpOrderremarksInfoOutput>>();


        output.erpChildOrderList = await _repository.Context.Queryable<ErpOrderEntity>()
           .Where(it => SqlFunc.Subqueryable<ErpOrderRelationEntity>().Where(ddd => ddd.Oid == output.id && ddd.Cid == it.Id).Any()) // 过滤掉拆单记录
           .OrderBy(it => it.CreatorTime, OrderByType.Asc)
           .Select(it => new ErpSubOrderListOutput
           {
               id = it.Id,
               no = it.No,
               createUid = it.CreateUid,
               createTime = it.CreateTime,
               creatorTime = it.CreatorTime,
               cid = it.Cid,
               posttime = it.Posttime,
               cidName = SqlFunc.Subqueryable<ErpCustomerEntity>().Where(d => d.Id == it.Cid).Select(d => d.Name),
               createUidName = SqlFunc.Subqueryable<UserEntity>().Where(d => d.Id == it.CreateUid).Select(d => d.RealName),
               amount = it.Amount,
               state = it.State ?? 0,
               diningType = it.DiningType,
               oidName = SqlFunc.Subqueryable<OrganizeEntity>().Where(d => d.Id == it.Oid).Select(d => d.FullName),
               deliveryManIdName = SqlFunc.Subqueryable<UserEntity>().Where(d => d.Id == it.DeliveryManId).Select(d => d.RealName),
               deliveryCar = it.DeliveryCar
           }).ToListAsync();

        if (output.erpChildOrderList.IsAny())
        {
            var subIdList = output.erpChildOrderList.Select(x => x.id).ToList();

            var erpSubOrderdetailList = await _repository.Context.Queryable<ErpOrderdetailEntity>()
           .InnerJoin<ErpProductmodelEntity>((w, x) => w.Mid == x.Id)
           .InnerJoin<ErpProductEntity>((w, x, b) => x.Pid == b.Id)
           .Where(w => subIdList.Contains(w.Fid))
           .Select((w, x, b) => new ErpOrderdetailInfoOutput()
           {
               midName = x.Name,
               productName = b.Name,
               rootProducttype = SqlFunc.Subqueryable<ViewErpProducttypeEx>().Where(ddd => ddd.Id == b.Tid).Select(ddd => ddd.RootName)
           }, true)
           .ToListAsync();

            foreach (var item in output.erpChildOrderList)
            {
                item.erpOrderdetailList = erpSubOrderdetailList.Where(x => x.fid == item.id).ToList() ?? new List<ErpOrderdetailInfoOutput>();
            }
        }


        return output;
    }

    /// <summary>
    /// 获取订单信息列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] ErpOrderListQueryInput input)
    {
        List<string> oidList = new List<string>();
        //if (!_userManager.IsAdministrator)
        {
            //获取用户绑定的公司
            var olist = await _usersService.GetRelationOrganizeList(_userManager.UserId);
            oidList = olist?.Select(x => x.Id).ToList() ?? new List<string>();
            if (!oidList.IsAny())
            {
                oidList.Add($"NO_ORGANIZE_{_userManager.UserId}");
            }
        }

        List<string> gidList = new List<string>();
        if (input.productName.IsNotEmptyOrNull())
        {
            gidList = await _repository.Context.Queryable<ErpProductmodelEntity>()
                .Where(it => SqlFunc.Subqueryable<ErpProductEntity>().Where(d => d.Id == it.Pid && d.Name.Contains(input.productName)).Any())
                .Select(it => it.Id)
                .Take(100)
                .ToListAsync();
        }

        List<DateTime> posttimeRange = input.posttimeRange?.Split(',').ToObject<List<DateTime>>();
        DateTime? startPosttimeDate = posttimeRange?.First();
        DateTime? endPosttimeDate = posttimeRange?.Last();

        var data = await _repository.Context.Queryable<ErpOrderEntity>()
            //.ClearFilter<ICompanyEntity>()
            .WhereIF(input.beginDate.HasValue, it => it.CreateTime >= input.beginDate)
            .WhereIF(input.endDate.HasValue, it => it.CreateTime <= input.endDate)
            .WhereIF(input.state.IsNotEmptyOrNull(), it => it.State == input.state.Adapt<OrderStateEnum>())
            .WhereIF(input.createTime.HasValue, it => it.CreateTime >= input.createTime && it.CreateTime < input.createTime.Value.AddDays(1))
            .WhereIF(input.posttime.HasValue, it => it.Posttime >= input.posttime && it.Posttime < input.posttime.Value.AddDays(1))
            .WhereIF(!string.IsNullOrEmpty(input.cid), it => it.Cid == input.cid)
            .WhereIF(!string.IsNullOrEmpty(input.oid), it => it.Oid == input.oid)
            .WhereIF(!string.IsNullOrEmpty(input.no), it => it.No.Contains(input.no))
            .WhereIF(!string.IsNullOrEmpty(input.diningType), it => it.DiningType == input.diningType)
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.No.Contains(input.keyword)
                )
            .Where(it => oidList.Contains(it.Oid))
            .WhereIF(posttimeRange != null, it => SqlFunc.Between(it.Posttime, startPosttimeDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), endPosttimeDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .Where(it => SqlFunc.Subqueryable<ErpOrderRelationEntity>().Where(ddd => ddd.Cid == it.Id).NotAny()) // 过滤掉拆单记录
            .WhereIF(gidList.IsAny(), it => SqlFunc.Subqueryable<ErpOrderdetailEntity>().Where(d => d.Fid == it.Id && gidList.Contains(d.Mid)).Any())
            .OrderBy(it => it.CreatorTime, OrderByType.Desc)
            .Select(it => new ErpOrderCwddListOutput
            {
                id = it.Id,
                no = it.No,
                createUid = it.CreateUid,
                createTime = it.CreateTime,
                creatorTime = it.CreatorTime,
                cid = it.Cid,
                posttime = it.Posttime,
                cidName = SqlFunc.Subqueryable<ErpCustomerEntity>().Where(d => d.Id == it.Cid).Select(d => d.Name),
                createUidName = SqlFunc.Subqueryable<UserEntity>().Where(d => d.Id == it.CreateUid).Select(d => d.RealName),
                amount = it.Amount,
                state = it.State ?? 0,
                diningType = it.DiningType,
                oidName = SqlFunc.Subqueryable<OrganizeEntity>().Where(d => d.Id == it.Oid).Select(d => d.FullName),
                printAmount = SqlFunc.Subqueryable<ErpOrderdetailEntity>().Where(d=>d.Fid == it.Id).Sum(d => d.PrintAmount),
            }).ToPagedListAsync(input.currentPage, input.pageSize);
        if (data != null && data.list.IsAny())
        {
            foreach (var item in data.list)
            {
                if (item.posttime.HasValue)
                {
                    item.dayOfWeek = item.posttime.Value.ToString("dddd");
                }
                if (item.createTime.HasValue)
                {
                    item.createDayOfWeek = item.createTime.Value.ToString("dddd");
                }
            }
        }
        return PageResult<ErpOrderListOutput>.SqlSugarPageResult(data);
    }


    /// <summary>
    /// 更新订单信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] ErpOrderCwddUpInput input)
    {
        var ErpOrderdetailEntitys = await _repository.Context.Queryable<ErpOrderdetailEntity>().Where(x => x.Fid == id).ToListAsync();

        List<ErpOrderdetailEntity> updateList = new List<ErpOrderdetailEntity>();
        foreach (var item in ErpOrderdetailEntitys)
        {
            var entity = input.erpOrderdetailList?.Find(x => x.id == item.Id);
            if (entity!=null)
            {
                _repository.Context.Tracking(item);
                item.PrintNum = entity.printNum;
                item.PrintPrice = entity.printPrice;
                item.PrintAmount = entity.printAmount;

                updateList.Add(item);
            }
        }

        if (updateList.IsAny())
        {
            await _repository.Context.Updateable(updateList).ExecuteCommandAsync();
        }
    }
}
