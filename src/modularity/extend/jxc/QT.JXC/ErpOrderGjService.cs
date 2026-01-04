using QT.Common.Extension;
using QT.JXC.Entitys.Dto.Erp.OrderXs;
using QT.JXC.Entitys.Dto.ErpOrderGj;
using QT.JXC.Interfaces;

namespace QT.JXC;

/// <summary>
/// 业务实现：订单改价.
/// </summary>
[ApiDescriptionSettings(ModuleConst.ERP, Tag = "Erp", Name = "ErpOrderGj", Order = 200)]
[Route("api/Erp/[controller]")]
public class ErpOrderGjService: IDynamicApiController
{
    private ISqlSugarRepository<ErpOrderEntity> _repository;
    private IUserManager _userManager;
    private readonly IErpCustomerService _erpCustomerService;
    private ITenant _db;

    /// <summary>
    /// 初始化一个<see cref="ErpOrderXsService"/>类型的新实例.
    /// </summary>
    public ErpOrderGjService(
        ISqlSugarRepository<ErpOrderEntity> erpOrderRepository,
        ISqlSugarClient context,
        IUserManager userManager,
        IErpCustomerService erpCustomerService)
    {
        _repository = erpOrderRepository;
        _userManager = userManager;
        _erpCustomerService = erpCustomerService;
        _db = context.AsTenant();
    }

    /// <summary>
    /// 获取订单信息列表.
    /// 下单时间限制 近三个月
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] ErpOrderGjListQueryInput input)
    {
        //设置约定送货时间是上个月后10天、当月和下个月前10天可以修改价格
        var start = DateTime.Now.ToString("yyyy-MM-01");
        var end = DateTime.Now.AddMonths(1).ToString("yyyy-MM-10 23:59:59");

        // GD2025090003 开始时间改成上个月最后10天
        start = DateTime.Parse(start).AddDays(-10).ToString("yyyy-MM-dd");


        //var start = DateTime.Now.AddMonths(-3).ToString("yyyy-MM-01");
        //var end = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")).AddMonths(1).AddDays(-1).ToString("yyyy-MM-dd 23:59:59");

        List<DateTime> queryOrderDate = input.createTime?.Split(',').ToObject<List<DateTime>>();
        DateTime? startOrderDate = queryOrderDate?.First();
        DateTime? endOrderDate = queryOrderDate?.Last();

        List<DateTime> queryPostDate = input.posttime?.Split(',').ToObject<List<DateTime>>();
        DateTime? startPostDate = queryPostDate?.First();
        DateTime? endPostDate = queryPostDate?.Last();

        List<string> gidList = new List<string>();
        if (input.productName.IsNotEmptyOrNull())
        {
            gidList = await _repository.Context.Queryable<ErpProductmodelEntity>()
                .Where(it => SqlFunc.Subqueryable<ErpProductEntity>().Where(d => d.Id == it.Pid && d.Name.Contains(input.productName)).Any())
                .Select(it => it.Id)
                .Take(100)
                .ToListAsync();

            gidList.Add(input.productName);
        }
        List<string> cidList = new List<string>();
        if (input.customerType.IsNotEmptyOrNull())
        {
            cidList = await _repository.Context.Queryable<ErpCustomerEntity>()
                .Where(x=>x.Type == input.customerType)
                .Select(it => it.Id)
                .Take(100)
                .ToListAsync();

            cidList.Add(input.customerType);
        }

        var data = await _repository.Context.Queryable<ErpOrderEntity>()
            .Where(it => SqlFunc.Between(it.Posttime, start, end))
            .WhereIF(queryOrderDate != null, it => SqlFunc.Between(it.CreateTime, startOrderDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), endOrderDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .WhereIF(input.beginDate.HasValue, it => it.CreateTime >= input.beginDate)
            .WhereIF(input.endDate.HasValue, it => it.CreateTime <= input.endDate)
            .WhereIF(queryPostDate != null, it => SqlFunc.Between(it.Posttime, startPostDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), endPostDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .WhereIF(!string.IsNullOrEmpty(input.no), it => it.No.Contains(input.no))
            .WhereIF(!string.IsNullOrEmpty(input.cid), it => it.Cid == input.cid)
            .WhereIF(!string.IsNullOrEmpty(input.diningType), it => it.DiningType == input.diningType)
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.No.Contains(input.keyword)
                || it.Cid.Contains(input.keyword)
                )
            .WhereIF(gidList.IsAny(), it => SqlFunc.Subqueryable<ErpOrderdetailEntity>().Where(d => d.Fid == it.Id && gidList.Contains(d.Mid)).Any())
            .WhereIF(cidList.IsAny(), it=> cidList.Contains(it.Cid))
            .Where(it => SqlFunc.Subqueryable<ErpOrderRelationEntity>().Where(ddd => ddd.Cid == it.Id).NotAny()) // 过滤掉拆单记录
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.CreatorTime, OrderByType.Desc)
            .Select(it => new ErpOrderXsListOutput
            {
                id = it.Id,
                no = it.No,
                cid = it.Cid,
                posttime = it.Posttime,
                creatorTime = it.CreatorTime,
                state = it.State ?? 0,
                createTime = it.CreateTime,
                cidName = SqlFunc.Subqueryable<ErpCustomerEntity>().Where(d => d.Id == it.Cid).Select(d => d.Name),
                diningType = it.DiningType
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);

        if (data != null && data.list.IsAny())
        {
            foreach (var item in data.list)
            {
                if (item.posttime.HasValue)
                {
                    item.dayOfWeek = item.posttime.Value.ToString("dddd");
                }
            }
        }
        return PageResult<ErpOrderXsListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 批量修改订单价格，
    /// </summary>
    /// <returns></returns>
    [HttpPost("Actions/Batch/EditPrice")]
    //[SqlSugarUnitOfWork]
    public async Task<int> BatchEditPrice(ErpOrderBatchProcessInput input)
    {
        if (!input.items.IsAny())
        {
            throw Oops.Oh("请选择订单！");
        }

        Dictionary<string, string> cacheCustomer = new Dictionary<string, string>();


        int count = 0;
        foreach (var item in input.items)
        {
            var order = await _repository.Context.Queryable<ErpOrderEntity>().InSingleAsync(item);
            if (order == null)
            {
                continue;
            }
            var list = await _repository.Context.Queryable<ErpOrderdetailEntity>().Where(it => it.Fid == order.Id).ToListAsync();
            if (!list.IsAny())
            {
                continue;
            }



            #region 获取订单价格
            string customerType = "";
            if (cacheCustomer.ContainsKey(order.Cid))
            {
                customerType = cacheCustomer[order.Cid];
            }
            else
            {
                customerType = await _repository.Context.Queryable<ErpCustomerEntity>().Where(x => x.Id == order.Cid).Select(x => x.Type).FirstAsync();
                cacheCustomer.Add(order.Cid, customerType);
            }
            var q1 = _repository.Context.Queryable<ErpProductcustomertypepriceEntity>().Where(xxx => xxx.Oid == _userManager.CompanyId);
            var q2 = _repository.Context.Queryable<ErpProductcustomertypepriceEntity>()
                .Where(yyy => (yyy.Oid ?? "") == "" && !SqlFunc.Subqueryable<ErpProductcustomertypepriceEntity>().Where(xxx => xxx.Oid == _userManager.CompanyId && xxx.Tid == yyy.Tid && xxx.Gid == yyy.Gid).Any());
            var qur = _repository.Context.Union(q1, q2);

            var customerPriceList = await _repository.Context.Queryable<ErpProductmodelEntity>()
            .LeftJoin<ErpProductEntity>((a, b) => a.Pid == b.Id)
               .LeftJoin<ErpProductpriceEntity>((a, b, c) => a.Id == c.Gid && c.Cid == order.Cid)
               .LeftJoin(qur, (a, b, c, d) => a.Id == d.Gid && d.Tid == customerType)
               .Where((a, b) => b.State == "1")
               .Select((a, b, c, d) => new ErpProductSelectorOutput
               {
                   id = a.Id,
                   salePrice = c.Price > 0 ? c.Price : d.PricingType == 1 ? a.SalePrice * d.Discount * 0.01m : d.PricingType == 2 ? d.Price : a.SalePrice, //优先使用客户定价，然后是客户类型折扣，再是客户类型价格，最后是原价
               }).ToListAsync();
            #endregion

            var toTwoDecimalsNoRound = await _erpCustomerService.Check(order.Cid);

            _repository.Context.TempItems.Clear();
            //_repository.Context.Tracking(list);
            // 更新订单价格

            List<ErpOrderdetailEntity> updateList = new List<ErpOrderdetailEntity>();
            foreach (var entity in list)
            {
                var cp = customerPriceList.FirstOrDefault(x => x.id == entity.Mid);
                if (cp != null && cp.salePrice>0)
                {
                    _repository.Context.Tracking(entity);

                    entity.SalePrice = cp.salePrice;

                    entity.Amount1 = entity.Num1 * entity.SalePrice;
                    entity.Amount2 = entity.Num2 * entity.SalePrice;

                    if (entity.RejectNum > 0)
                    {
                        entity.Amount = (entity.Num1 - entity.RejectNum.Value) * entity.SalePrice;
                    }
                    else if (entity.Num1 > 0)
                    {
                        entity.Amount = entity.Amount1;
                    }
                    else
                    {
                        entity.Amount = entity.SalePrice * entity.Num;
                    }

                    // 判断客户是否用特殊的方式计算金额
                    if (toTwoDecimalsNoRound)
                    {
                        entity.Amount = Math.Floor(entity.Amount * 100) * 0.01m;
                        entity.Amount1 = Math.Floor(entity.Amount1 * 100) * 0.01m;
                        entity.Amount2 = Math.Floor(entity.Amount2 * 100) * 0.01m;
                    }

                    // 判断是否有变化
                    var changes = _repository.Context.GetChanges(entity);

                    if (changes.Count>0)
                    {
                        updateList.Add(entity);
                    }
                }
            }

            if (updateList.Count>0)
            {
                _repository.Context.Tracking(order);
                // 更新主表金额
                order.Amount = list.Sum(a => a.Amount);
            }
            else
            {
                continue;
            }


            StringBuilder stringBuilder = new StringBuilder();
            //try
            //{
            //    _db.BeginTran();

            //await _repository.Context.Updateable(order).EnableDiffLogEvent().ExecuteCommandAsync();
            //await _repository.Context.Updateable(list).EnableDiffLogEvent().ExecuteCommandAsync();

            stringBuilder.Append(_repository.Context.Updateable(order).EnableDiffLogEvent().ToSqlString()).Append(";").AppendLine();
            stringBuilder.Append(_repository.Context.Updateable(updateList).EnableDiffLogEvent().ToSqlString()).Append(";").AppendLine();

            // 插入处理日志
            var erpOrderoperaterecordEntity = new ErpOrderoperaterecordEntity
                {
                    Id = SnowflakeIdHelper.NextId(),
                    State = ((int)order.State).ToString(),
                    Fid = order.Id,
                    Time = DateTime.Now,
                    UserId = _userManager.RealName,
                    Remark = $"[{_userManager.RealName}]执行订单改价"
                };

                //写入订单日志
                //await _repository.Context.Insertable<ErpOrderoperaterecordEntity>(erpOrderoperaterecordEntity).ExecuteCommandAsync();
             //stringBuilder.Append(_repository.Context.Insertable<ErpOrderoperaterecordEntity>(erpOrderoperaterecordEntity).ToSqlString()).Append(";").AppendLine();

            if (stringBuilder.Length > 0)
            {
                await _repository.Context.UseTranAsync(async () =>
                 {
                     try
                     {
                         await _repository.Ado.ExecuteCommandAsync(stringBuilder.ToString());
                         await _repository.Context.Insertable<ErpOrderoperaterecordEntity>(erpOrderoperaterecordEntity).ExecuteCommandAsync();
                         count++;
                     }
                     catch (Exception ex)
                     {

                         throw;
                     }
                 });
            }
            //    _db.CommitTran();
            //}
            //catch (Exception ex)
            //{
            //    // 回滚事务
            //    _db.RollbackTran();
            //}            
        }

        
        return count;
    }

    /// <summary>
    /// 批量修改订单价格，
    /// </summary>
    /// <returns></returns>
    [HttpPost("Actions/{id}/EditPrice")]
    [SqlSugarUnitOfWork]
    public async Task<int> EditPrice(string id)
    {
        // 2024.10.22停用功能
        return 0;
        var order = await _repository.Context.Queryable<ErpOrderEntity>().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);

        //约定配送时间内才更新
        if (!order.Posttime.HasValue || DateTime.Now.Date > order.Posttime.Value.Date)
        {
            return 0;
        }

        
        var list = await _repository.Context.Queryable<ErpOrderdetailEntity>().Where(it => it.Fid == order.Id).ToListAsync();
        if (!list.IsAny())
        {
            return 0;
        }



        #region 获取订单价格
        string customerType = await _repository.Context.Queryable<ErpCustomerEntity>().Where(x => x.Id == order.Cid).Select(x => x.Type).FirstAsync(); 

        var q1 = _repository.Context.Queryable<ErpProductcustomertypepriceEntity>().Where(xxx => xxx.Oid == _userManager.CompanyId);
        var q2 = _repository.Context.Queryable<ErpProductcustomertypepriceEntity>()
            .Where(yyy => (yyy.Oid ?? "") == "" && !SqlFunc.Subqueryable<ErpProductcustomertypepriceEntity>().Where(xxx => xxx.Oid == _userManager.CompanyId && xxx.Tid == yyy.Tid && xxx.Gid == yyy.Gid).Any());
        var qur = _repository.Context.Union(q1, q2);

        var customerPriceList = await _repository.Context.Queryable<ErpProductmodelEntity>()
        .LeftJoin<ErpProductEntity>((a, b) => a.Pid == b.Id)
           .LeftJoin<ErpProductpriceEntity>((a, b, c) => a.Id == c.Gid && c.Cid == order.Cid)
           .LeftJoin(qur, (a, b, c, d) => a.Id == d.Gid && d.Tid == customerType)
           .Where((a, b) => b.State == "1")
           .Select((a, b, c, d) => new ErpProductSelectorOutput
           {
               id = a.Id,
               salePrice = c.Price > 0 ? c.Price : d.PricingType == 1 ? a.SalePrice * d.Discount * 0.01m : d.PricingType == 2 ? d.Price : a.SalePrice, //优先使用客户定价，然后是客户类型折扣，再是客户类型价格，最后是原价
           }).ToListAsync();
        #endregion

        _repository.Context.TempItems.Clear();
        _repository.Context.Tracking(order);
        //_repository.Context.Tracking(list);
        List<ErpOrderdetailEntity> updateList = new List<ErpOrderdetailEntity>();
        // 更新订单价格

        foreach (var entity in list)
        {
            var cp = customerPriceList.FirstOrDefault(x => x.id == entity.Mid);
            if (cp != null)
            {
                _repository.Context.Tracking(entity);
                updateList.Add(entity);

                entity.SalePrice = cp.salePrice;

                entity.Amount1 = entity.Num1 * entity.SalePrice;
                entity.Amount2 = entity.Num2 * entity.SalePrice;

                if (entity.RejectNum > 0)
                {
                    entity.Amount = (entity.Num1 - entity.RejectNum.Value) * entity.SalePrice;
                }
                else if (entity.Num1 > 0)
                {
                    entity.Amount = entity.Amount1;
                }
                else
                {
                    entity.Amount = entity.SalePrice * entity.Num;
                }
            }
        }

        // 更新主表金额
        order.Amount = list.Sum(a => a.Amount);

        await _repository.Context.Updateable<ErpOrderEntity>(order).EnableDiffLogEvent().ExecuteCommandAsync();
        if (updateList.IsAny())
        {
            await _repository.Context.Updateable(updateList).EnableDiffLogEvent().ExecuteCommandAsync();
        }
        

        // 插入处理日志
        var erpOrderoperaterecordEntity = new ErpOrderoperaterecordEntity
        {
            Id = SnowflakeIdHelper.NextId(),
            State = ((int)order.State).ToString(),
            Fid = order.Id,
            Time = DateTime.Now,
            UserId = _userManager.RealName,
            Remark = $"[{_userManager.RealName}]执行订单改价"
        };

        //写入订单日志
        await _repository.Context.Insertable<ErpOrderoperaterecordEntity>(erpOrderoperaterecordEntity).ExecuteCommandAsync();

        return 1;
    }
}
