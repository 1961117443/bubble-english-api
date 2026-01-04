using Mapster;
using Microsoft.AspNetCore.Authorization;
using MiniExcelLibs;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using QT.ClayObject.Extensions;
using QT.Common;
using QT.Common.Cache;
using QT.Common.Core.Manager.Files;
using QT.Common.Extension;
using QT.DependencyInjection;
using QT.JXC.Entitys.Dto;
using QT.JXC.Entitys.Dto.Erp;
using QT.JXC.Entitys.Dto.Erp.BuyOrder;
using QT.JXC.Entitys.Dto.Erp.ErpProducttype;
using QT.JXC.Entitys.Dto.Erp.OrderXs;
using QT.JXC.Entitys.Dto.Order;
using QT.JXC.Entitys.Entity;
using QT.JXC.Entitys.Entity.ERP;
using QT.JXC.Entitys.Enums;
using QT.JXC.Entitys.Views;
using QT.JsonSerialization;
using QT.JXC.Interfaces;
using QT.Reflection.Extensions;
using QT.Systems.Entitys.Model.Organize;
using QT.Systems.Entitys.Permission;
using QT.Systems.Entitys.System;
using QT.Systems.Interfaces.Permission;
using QT.Systems.Interfaces.System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Reflection;

namespace QT.JXC;

/// <summary>
/// 业务实现：订单信息.
/// </summary>
[ApiDescriptionSettings(ModuleConst.ERP, Tag = "Erp", Name = "ErpOrder", Order = 200)]
[Route("api/Erp/[controller]")]
public class ErpOrderService : IErpOrderService, IDynamicApiController, ITransient
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
    private readonly ICacheManager _cache;

    /// <summary>
    /// 初始化一个<see cref="ErpOrderService"/>类型的新实例.
    /// </summary>
    public ErpOrderService(
        ISqlSugarRepository<ErpOrderEntity> erpOrderRepository,
        IBillRullService billRullService,
        ISqlSugarClient context,
        IUserManager userManager, IDictionaryDataService dictionaryDataService, IFileManager fileManager,
        ICache cache, IUsersService usersService, ICacheManager cacheManager)
    {
        _repository = erpOrderRepository;
        _billRullService = billRullService;
        _db = context.AsTenant();
        _userManager = userManager;
        _dictionaryDataService = dictionaryDataService;
        _fileManager = fileManager;
        //_cache = cache;
        _usersService = usersService;
        _cache = cacheManager;

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
            .Select((w, x, b) => new ErpOrderdetailInfoOutput()
            {
                midName = x.Name,
                productName = b.Name,
                rootProducttype = SqlFunc.Subqueryable<ViewErpProducttypeEx>().Where(ddd => ddd.Id == b.Tid).Select(ddd => ddd.RootName)
            }, true)
            .ToListAsync();
        output.erpOrderdetailList = erpOrderdetailList.OrderBy(x => x.order ?? 99).ToList(); //.Adapt<List<ErpOrderdetailInfoOutput>>();

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

            gidList.Add(input.productName);
        }

        List<string> cidList = new List<string>();
        if (input.customerType.IsNotEmptyOrNull())
        {
            cidList = await _repository.Context.Queryable<ErpCustomerEntity>()
                .Where(x => x.Type == input.customerType)
                .Select(it => it.Id)
                .Take(100)
                .ToListAsync();

            cidList.Add(input.customerType);
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
            .WhereIF(gidList.IsAny(), it => SqlFunc.Subqueryable<ErpOrderdetailEntity>().Where(d => d.Fid == it.Id && gidList.Contains(d.Mid) && d.DeleteTime == null).Any())
            .WhereIF(cidList.IsAny(), it => cidList.Contains(it.Cid))
            .OrderBy(it => it.CreatorTime, OrderByType.Desc)
            .Select(it => new ErpOrderListOutput
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
    /// 新建订单信息.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    [SqlSugarUnitOfWork]
    public async Task<string> Create([FromBody] ErpOrderCrInput input, [FromServices] IErpConfigService erpConfigService)
    {
        await erpConfigService.ValidCustomerOrder();

        if (string.IsNullOrEmpty(input.cid))
        {
            throw Oops.Oh("请输入客户");
        }
        if (!input.posttime.HasValue)
        {
            throw Oops.Oh("请输入约定送货时间");
        }
        var entity = input.Adapt<ErpOrderEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        entity.No = await _billRullService.GetBillNumber("QTErpOrder");
        entity.State = OrderStateEnum.Draft;
        entity.Amount = input.erpOrderdetailList?.Sum(a => a.amount) ?? 0;
        if (!entity.CreateTime.HasValue)
        {
            entity.CreateTime = DateTime.Now;
        }
        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

            var newEntity = await _repository.Context.Insertable<ErpOrderEntity>(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteReturnEntityAsync();

            var erpOrderdetailEntityList = input.erpOrderdetailList.Adapt<List<ErpOrderdetailEntity>>();
            if (erpOrderdetailEntityList != null)
            {
                foreach (var item in erpOrderdetailEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.Fid = newEntity.Id;
                }

                await _repository.Context.Insertable(erpOrderdetailEntityList).ExecuteCommandAsync();
            }

            var erpOrderoperaterecordEntityList = input.erpOrderoperaterecordList.Adapt<List<ErpOrderoperaterecordEntity>>();
            if (erpOrderoperaterecordEntityList != null)
            {
                foreach (var item in erpOrderoperaterecordEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.Fid = newEntity.Id;
                }

                await _repository.Context.Insertable(erpOrderoperaterecordEntityList).ExecuteCommandAsync();
            }

            var erpOrderremarksEntityList = input.erpOrderremarksList.Adapt<List<ErpOrderremarksEntity>>();
            if (erpOrderremarksEntityList != null)
            {
                foreach (var item in erpOrderremarksEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.OrderId = newEntity.Id;
                }

                await _repository.Context.Insertable(erpOrderremarksEntityList).ExecuteCommandAsync();
            }

            // 关闭事务
            //_db.CommitTran();

            return entity.Id;
        //}
        //catch (Exception)
        //{
        //    // 回滚事务
        //    _db.RollbackTran();

        //    throw Oops.Oh(ErrorCode.COM1000);
        //}
    }

    /// <summary>
    /// 更新订单备注
    /// </summary>
    /// <returns></returns>
    [HttpPut("memo/{id}")]
    public async Task<ErpOrderremarksInfoOutput> UpdateMemo(string id, [FromBody] ErpOrderremarksCrInput input)
    {
        var entity = new ErpOrderremarksEntity
        {
            Id = SnowflakeIdHelper.NextId(),
            OrderId = input.id,
            Remark = input.remark
        };

        await _repository.Context.Insertable(entity).ExecuteCommandAsync();

        return entity.Adapt<ErpOrderremarksInfoOutput>();
    }

    /// <summary>
    /// 更新订单明细备注
    /// </summary>
    /// <returns></returns>
    [HttpPut("Detail/{id}/memo")]
    public async Task UpdateDetailMemo(string id, [FromBody] ErpOrderremarksCrInput input)
    {
        ErpOrderdetailEntity erpOrderdetailEntity = input.Adapt<ErpOrderdetailEntity>();

        await _repository.Context.Updateable<ErpOrderdetailEntity>(erpOrderdetailEntity).UpdateColumns(x => x.Remark).ExecuteCommandAsync();
        //await _repository.Context.Insertable(entity).ExecuteCommandAsync();

        //return entity.Adapt<ErpOrderremarksInfoOutput>();
    }

    /// <summary>
    /// 更新订单信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [SqlSugarUnitOfWork]
    public async Task Update(string id, [FromBody] ErpOrderUpInput input)
    {
        if (string.IsNullOrEmpty(input.cid))
        {
            throw Oops.Oh("请输入客户");
        }
        if (!input.posttime.HasValue)
        {
            throw Oops.Oh("请输入约定送货时间");
        }
        var oldEntity = await _repository.Context.Queryable<ErpOrderEntity>().SingleAsync(x => x.Id == id) ?? throw Oops.Oh(ErrorCode.COM1005);
        var entity = input.Adapt<ErpOrderEntity>();

        var tempList = await _repository.Context.Queryable<ErpOrderdetailEntity>().Where(x => x.Fid == entity.Id).Select(x => new ErpOrderdetailEntity
        {
            Id = x.Id,
            Num1 = x.Num1
        }).ToListAsync();

        foreach (var item in tempList)
        {
            var dbEntity = input.erpOrderdetailList?.Find(x => x.id == item.Id);
            if (dbEntity != null)
            {
                if (dbEntity.num1 != item.Num1)
                {
                    throw Oops.Oh("订单分拣数据已修改，请重新打开订单！");
                }
            }
            else
            {
                if (item.Num1 > 0)
                {
                    throw Oops.Oh("商品已分拣，不允许删除！");
                }
            }
        }


        input.erpOrderdetailList?.ForEach(x =>
        {
            if (x.rejectNum > 0)
            {
                x.amount = (x.num1 - x.rejectNum) * x.salePrice;
            }
        });
        entity.Amount = input.erpOrderdetailList?.Sum(a => a.amount) ?? 0;
        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

            await _repository.Context.Updateable<ErpOrderEntity>(entity).IgnoreColumns(ignoreAllNullColumns: true).EnableDiffLogEvent().ExecuteCommandAsync();

            entity = await _repository.Context.Queryable<ErpOrderEntity>().InSingleAsync(entity.Id);
            List<string> addItems = new List<string>();
            List<string> updateItems = new List<string>();
            List<string> delItems = new List<string>();

            // 订单明细
            await _repository.Context.CUDSaveAsnyc<ErpOrderdetailEntity>(it => it.Fid == entity.Id, input.erpOrderdetailList, it =>
            {
                it.Fid = entity.Id;
                addItems.Add(it.Mid);
            }
            , it =>
            {
                it.Amount2 = it.Num2 * it.SalePrice;
                updateItems.Add(it.Mid);
            }, it => delItems.Add(it.Mid));

            List<ErpOrderoperaterecordEntity> erpOrderoperaterecordEntities = new List<ErpOrderoperaterecordEntity>();
            if (addItems.IsAny())
            {
                var addlist = await _repository.Context.Queryable<ErpProductEntity>().InnerJoin<ErpProductmodelEntity>((a, b) => a.Id == b.Pid)
                    .Where((a, b) => addItems.Contains(b.Id)).Select(a => a.Name).ToListAsync();

                erpOrderoperaterecordEntities.Add(new ErpOrderoperaterecordEntity
                {
                    Id = SnowflakeIdHelper.NextId(),
                    State = ((int)entity.State).ToString(),
                    Fid = entity.Id,
                    Time = DateTime.Now,
                    UserId = _userManager.RealName,
                    Remark = $"订单添加商品明细：{string.Join(",", addlist.Distinct().ToArray())}"

                });
            }

            if (updateItems.IsAny())
            {
                var addlist = await _repository.Context.Queryable<ErpProductEntity>().InnerJoin<ErpProductmodelEntity>((a, b) => a.Id == b.Pid)
                    .Where((a, b) => updateItems.Contains(b.Id)).Select(a => a.Name).ToListAsync();

                erpOrderoperaterecordEntities.Add(new ErpOrderoperaterecordEntity
                {
                    Id = SnowflakeIdHelper.NextId(),
                    State = ((int)entity.State).ToString(),
                    Fid = entity.Id,
                    Time = DateTime.Now,
                    UserId = _userManager.RealName,
                    Remark = $"订单修改商品明细：{string.Join(",", addlist.Distinct().ToArray())}"

                });
            }


            if (delItems.IsAny())
            {
                //// 判断是否已经分拣
                //if (await _repository.Context.Queryable<ErpOrderdetailEntity>().Where(x=> delItems.Contains(x.Id) && x.SorterTime.HasValue).AnyAsync())
                //{
                //    throw Oops.Oh("订单明细已分拣不允许删除！");
                //}
                

                var addlist = await _repository.Context.Queryable<ErpProductEntity>().InnerJoin<ErpProductmodelEntity>((a, b) => a.Id == b.Pid)
                    .Where((a, b) => delItems.Contains(b.Id)).Select(a => a.Name).ToListAsync();

                erpOrderoperaterecordEntities.Add(new ErpOrderoperaterecordEntity
                {
                    Id = SnowflakeIdHelper.NextId(),
                    State = ((int)entity.State).ToString(),
                    Fid = entity.Id,
                    Time = DateTime.Now,
                    UserId = _userManager.RealName,
                    Remark = $"订单删除商品明细：{string.Join(",", addlist.Distinct().ToArray())}"

                });
            }

            // 判断主表变更记录
            if (oldEntity.Posttime != entity.Posttime)
            {
                erpOrderoperaterecordEntities.Add(new ErpOrderoperaterecordEntity
                {
                    Id = SnowflakeIdHelper.NextId(),
                    State = ((int)entity.State).ToString(),
                    Fid = entity.Id,
                    Time = DateTime.Now,
                    UserId = _userManager.RealName,
                    Remark = $"约定配送时间由[{oldEntity.Posttime?.ToString("yyyy-MM-dd")}]变更为[{entity.Posttime?.ToString("yyyy-MM-dd")}]"
                });
            }
            if (oldEntity.DiningType != entity.DiningType)
            {
                //餐别字典
                var diningTypeOptions = await _dictionaryDataService.GetList("ErpOrderDiningType");
                erpOrderoperaterecordEntities.Add(new ErpOrderoperaterecordEntity
                {
                    Id = SnowflakeIdHelper.NextId(),
                    State = ((int)entity.State).ToString(),
                    Fid = entity.Id,
                    Time = DateTime.Now,
                    UserId = _userManager.RealName,
                    Remark = $"餐别由[{diningTypeOptions?.Find(x => x.EnCode == oldEntity.DiningType)?.FullName}]变更为[{diningTypeOptions?.Find(x => x.EnCode == entity.DiningType)?.FullName}]"
                });
            }
            if (oldEntity.Remark != entity.Remark)
            {
                erpOrderoperaterecordEntities.Add(new ErpOrderoperaterecordEntity
                {
                    Id = SnowflakeIdHelper.NextId(),
                    State = ((int)entity.State).ToString(),
                    Fid = entity.Id,
                    Time = DateTime.Now,
                    UserId = _userManager.RealName,
                    Remark = $"备注由[{oldEntity.Remark}]变更为[{entity.Remark}]"
                });
            }
            if (oldEntity.Cid != entity.Cid)
            {
                var cidList = await _repository.Context.Queryable<ErpCustomerEntity>().Where(x => x.Id == oldEntity.Cid || x.Id == entity.Cid)
                     .Select(x => new ErpCustomerEntity
                     {
                         Id = x.Id,
                         Name = x.Name
                     })
                     .ToListAsync();
                erpOrderoperaterecordEntities.Add(new ErpOrderoperaterecordEntity
                {
                    Id = SnowflakeIdHelper.NextId(),
                    State = ((int)entity.State).ToString(),
                    Fid = entity.Id,
                    Time = DateTime.Now,
                    UserId = _userManager.RealName,
                    Remark = $"客户由[{cidList.Find(x => x.Id == oldEntity.Cid)?.Name}]变更为[{cidList.Find(x => x.Id == entity.Cid)?.Name}]"
                });
            }

            if (erpOrderoperaterecordEntities.IsAny())
            {
                await _repository.Context.Insertable(erpOrderoperaterecordEntities).ExecuteCommandAsync();
            }


        // 更新订单金额
        var amount = await _repository.Context.Queryable<ErpOrderdetailEntity>().Where(x => x.Fid == id).SumAsync(x => x.Amount);

        if (entity.Amount!= amount)
        {
            entity.Amount = amount;
            await _repository.Context.Updateable<ErpOrderEntity>(entity).UpdateColumns(x => x.Amount).ExecuteCommandAsync();
        }

            //// 清空订单商品表原有数据
            //await _repository.Context.Deleteable<ErpOrderdetailEntity>().Where(it => it.Fid == entity.Id).EnableDiffLogEvent().ExecuteCommandAsync();

            //// 新增订单商品表新数据
            //var erpOrderdetailEntityList = input.erpOrderdetailList.Adapt<List<ErpOrderdetailEntity>>();
            //if (erpOrderdetailEntityList != null)
            //{
            //    foreach (var item in erpOrderdetailEntityList)
            //    {
            //        item.Id ??= SnowflakeIdHelper.NextId();
            //        item.Fid = entity.Id;
            //    }

            //    await _repository.Context.Insertable<ErpOrderdetailEntity>(erpOrderdetailEntityList).EnableDiffLogEvent().ExecuteCommandAsync();
            //}

            //// 清空订单处理记录表原有数据
            //await _repository.Context.Deleteable<ErpOrderoperaterecordEntity>().Where(it => it.Fid == entity.Id).EnableDiffLogEvent().ExecuteCommandAsync();

            //// 新增订单处理记录表新数据
            //var erpOrderoperaterecordEntityList = input.erpOrderoperaterecordList.Adapt<List<ErpOrderoperaterecordEntity>>();
            //if (erpOrderoperaterecordEntityList != null)
            //{
            //    foreach (var item in erpOrderoperaterecordEntityList)
            //    {
            //        item.Id ??= SnowflakeIdHelper.NextId();
            //        item.Fid = entity.Id;
            //    }

            //    await _repository.Context.Insertable<ErpOrderoperaterecordEntity>(erpOrderoperaterecordEntityList).EnableDiffLogEvent().ExecuteCommandAsync();
            //}

            //// 清空订单备注记录原有数据
            //await _repository.Context.Deleteable<ErpOrderremarksEntity>().Where(it => it.OrderId == entity.Id).ExecuteCommandAsync();

            //// 新增订单备注记录新数据
            //var erpOrderremarksEntityList = input.erpOrderremarksList.Adapt<List<ErpOrderremarksEntity>>();
            //if (erpOrderremarksEntityList != null)
            //{
            //    foreach (var item in erpOrderremarksEntityList)
            //    {
            //        item.Id = SnowflakeIdHelper.NextId();
            //        item.OrderId = entity.Id;
            //    }

            //    await _repository.Context.Insertable<ErpOrderremarksEntity>(erpOrderremarksEntityList).ExecuteCommandAsync();
            //}

        //    // 关闭事务
        //    _db.CommitTran();
        //}
        //catch (Exception ex)
        //{
        //    // 回滚事务
        //    _db.RollbackTran();
        //    throw Oops.Oh(ErrorCode.COM1001);
        //}
    }

    /// <summary>
    /// 删除订单信息.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [SqlSugarUnitOfWork]
    public async Task Delete(string id)
    {
        // 软删除
        var entity = await _repository.Context.Queryable<ErpOrderEntity>().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);


        // 分拣状态，判断是否有已分拣的数据，不允许删除
        if (entity.State == OrderStateEnum.PendingApproval)
        {
            if (await _repository.Context.Queryable<ErpOrderdetailEntity>().Where(x => x.Fid == id && x.SorterTime.HasValue).AnyAsync())
            {
                throw Oops.Oh("订单已分拣不允许删除！");
            }
        }
        else if (entity.State != OrderStateEnum.Draft)
        {
            throw Oops.Oh("当前订单状态不允许删除！");
        }

        _repository.Context.Tracking(entity);
        entity.DeleteTime = DateTime.Now;
        entity.DeleteUserId = _userManager.UserId;
        await _repository.Context.Updateable(entity).EnableDiffLogEvent().ExecuteCommandAsync();

        // 判断是否有子单，删除子单
        var subList = await _repository.Context.Queryable<ErpOrderRelationEntity>().Where(x => x.Oid == entity.Id).ToListAsync();

        if (subList.IsAny())
        {
            var subOrders = await _repository.Context.Queryable<ErpOrderEntity>().Where(it => subList.Any(x => x.Cid == it.Id)).ToListAsync();

            if (subOrders.IsAny())
            {
                // 如果子单已分拣不允许删除
                if (await _repository.Context.Queryable<ErpOrderdetailEntity>().Where(it => subOrders.Any(x => x.Id == it.Fid) && it.SorterState == "1").AnyAsync())
                {
                    throw Oops.Oh("子单已分拣，请退回主单后再删除！");
                }

                foreach (var item in subOrders)
                {
                    _repository.Context.Tracking(item);
                    item.DeleteTime = DateTime.Now;
                    item.DeleteUserId = _userManager.UserId;
                }
                await _repository.Context.Updateable(subOrders).EnableDiffLogEvent().ExecuteCommandAsync();
            }
        }


        //if (!await _repository.Context.Queryable<ErpOrderEntity>().AnyAsync(it => it.Id == id))
        //{
        //    throw Oops.Oh(ErrorCode.COM1005);
        //}
        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

        //    var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(id));
        //    await _repository.Context.Deleteable<ErpOrderEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();

        //    // 清空订单商品表表数据
        //    await _repository.Context.Deleteable<ErpOrderdetailEntity>().Where(it => it.Fid.Equals(entity.Id)).ExecuteCommandAsync();

        //    // 清空订单处理记录表表数据
        //    await _repository.Context.Deleteable<ErpOrderoperaterecordEntity>().Where(it => it.Fid.Equals(entity.Id)).ExecuteCommandAsync();

        //    // 清空订单备注记录表数据
        //    await _repository.Context.Deleteable<ErpOrderremarksEntity>().Where(it => it.OrderId.Equals(entity.Id)).ExecuteCommandAsync();

        //    // 关闭事务
        //    _db.CommitTran();
        //}
        //catch (Exception)
        //{
        //    // 回滚事务
        //    _db.RollbackTran();

        //    throw Oops.Oh(ErrorCode.COM1002);
        //}
    }


    /// <summary>
    /// 更新订单状态
    /// </summary>
    /// <param name="id">订单id</param>
    /// <param name="state">新的状态</param>
    [HttpPost("Process/{id}/{state}")]
    [SqlSugarUnitOfWork]
    public async Task ProcessOrder(string id, OrderStateEnum state)
    {
        using (var _lock = RedisHelper.Lock($"ProcessOrder:{id}", 5))
        {
            var order = await _repository.Entities.InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);
            if (order.State == OrderStateEnum.Cancelled)
            {
                throw Oops.Oh("订单已取消");
            }
            if (!order.State.HasValue || (int)state <= (int)order.State)
            {
                throw Oops.Oh("订单状态异常");
            }
            _repository.Context.Tracking(order);

            //修改订单状态
            order.State = state;
            switch (state)
            {
                case OrderStateEnum.Draft:
                    Console.WriteLine("草稿，待生效");
                    break;
                case OrderStateEnum.PendingApproval:
                    Console.WriteLine("生效，待分拣");
                    break;
                case OrderStateEnum.Picked:
                    //Console.WriteLine("已分拣，待出库");
                    // 判断明细是否全都待检完毕
                    var detailList = await _repository.Context.Queryable<ErpOrderdetailEntity>()
                        .Where(x => x.Fid == id)
                        .Select(x => new ErpOrderdetailEntity
                        {
                            Id = x.Id,
                            Amount = x.Amount,
                            Amount1 = x.Amount1,
                            SorterTime = x.SorterTime
                        })
                        .ToListAsync();
                    if (detailList.IsAny() && detailList.Any(x => !x.SorterTime.HasValue))
                    {
                        throw Oops.Oh("该订单还有明细未进行分拣，不能提交！");
                    }
                    var totalAmount = detailList.Sum(x => x.Amount);
                    if (order.Amount !=totalAmount)
                    {
                        order.Amount = totalAmount;
                        order.Amount1 = detailList.Sum(x => x.Amount1);
                    }
                    //if (await _repository.Context.Queryable<ErpOrderdetailEntity>().AnyAsync(x => x.Fid == id && !x.SorterTime.HasValue))
                    //{
                    //    throw Oops.Oh("该订单还有明细未进行分拣，不能提交！");
                    //}
                    // 更新订单金额
                    
                    break;
                case OrderStateEnum.Outbound:
                    //Console.WriteLine("已出库，待配送");
                    if (string.IsNullOrEmpty(order.DeliveryManId))
                    {
                        throw Oops.Oh("送货人为空，不能提交！");
                    }
                    if (string.IsNullOrEmpty(order.DeliveryCar))
                    {
                        throw Oops.Oh("送货车辆为空，不能提交！");
                    }
                    break;
                case OrderStateEnum.Delivery:
                    //Console.WriteLine("已配送，待收货");
                    if (string.IsNullOrEmpty(order.DeliveryProof))
                    {
                        throw Oops.Oh("配送凭证为空，不能提交！");
                    }
                    order.DeliveryTime = DateTime.Now;
                    break;
                case OrderStateEnum.Receiving:
                    //Console.WriteLine("已收货，待结算");
                    if (string.IsNullOrEmpty(order.DeliveryProof))
                    {
                        throw Oops.Oh("送货凭证为空，不能提交！");
                    }
                    order.ReceiveState = "1";
                    order.ReceiveTime = DateTime.Now;
                    order.DeliveryToTime = DateTime.Now;
                    await ProcessReceivingSubOrder(order);
                    break;
                case OrderStateEnum.Checkout:
                    Console.WriteLine("已结算，待收款");
                    break;
                case OrderStateEnum.Paid:
                    Console.WriteLine("已收款");
                    break;
                case OrderStateEnum.Cancelled:
                    Console.WriteLine("已取消");
                    break;
            }

            var erpOrderoperaterecordEntity = new ErpOrderoperaterecordEntity
            {
                Id = SnowflakeIdHelper.NextId(),
                State = ((int)state).ToString(),
                Fid = order.Id,
                Time = DateTime.Now,
                UserId = _userManager.RealName,
                Remark = $"[{_userManager.RealName}]将订单状态改为[{(int)state}-{state.ToDescription()}]"
            };

            await _repository.Context.Updateable<ErpOrderEntity>(order).ExecuteCommandAsync();

            //写入订单日志
            await _repository.Context.Insertable<ErpOrderoperaterecordEntity>(erpOrderoperaterecordEntity).ExecuteCommandAsync();

            //try
            //{
            //    // 开启事务
            //    _db.BeginTran();
            //    await _repository.Context.Updateable<ErpOrderEntity>(order).ExecuteCommandAsync();

            //    //写入订单日志
            //    await _repository.Context.Insertable<ErpOrderoperaterecordEntity>(erpOrderoperaterecordEntity).ExecuteCommandAsync();

            //    // 关闭事务
            //    _db.CommitTran();
            //}
            //catch (Exception)
            //{
            //    // 回滚事务
            //    _db.RollbackTran();

            //    throw Oops.Oh(ErrorCode.COM1003);
            //}


        }
    }

    /// <summary>
    /// 更新订单状态为草稿状态
    /// </summary>
    /// <param name="id">订单id</param>
    /// <param name="state">新的状态</param>
    [HttpPost("Actions/{id}/Draft")]
    [SqlSugarUnitOfWork]
    public async Task PutDraft(string id)
    {
        var order = await _repository.Entities.InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);
        if (order.State != OrderStateEnum.PendingApproval)
        {
            throw Oops.Oh("生效状态才能退回！");
        }

        // 判断明细是否有分拣
        if (await _repository.Context.Queryable<ErpOrderdetailEntity>().Where(it => it.Fid == order.Id && it.SorterState == "1").AnyAsync())
        {
            throw Oops.Oh("明细已分拣，不能退回！");
        }

        _repository.Context.Tracking(order);
        order.State = OrderStateEnum.Draft;
        var erpOrderoperaterecordEntity = new ErpOrderoperaterecordEntity
        {
            Id = SnowflakeIdHelper.NextId(),
            State = ((int)order.State).ToString(),
            Fid = order.Id,
            Time = DateTime.Now,
            UserId = _userManager.RealName,
            Remark = $"[{_userManager.RealName}]将订单状态改为[{(int)order.State}-{order.State.ToDescription()}]，订单退回"
        };

        // 更新状态
        await _repository.Context.Updateable<ErpOrderEntity>(order).UpdateColumns(nameof(ErpOrderEntity.State)).ExecuteCommandAsync();

        //写入订单日志
        await _repository.Context.Insertable<ErpOrderoperaterecordEntity>(erpOrderoperaterecordEntity).ExecuteCommandAsync();
    }


    /// <summary>
    /// 仓管前台更新订单状态为分拣状态
    /// </summary>
    /// <param name="id">订单id</param>
    /// <param name="state">新的状态</param>
    [HttpPost("Actions/{id}/Picked")]
    [SqlSugarUnitOfWork]
    public async Task PutBackToPicked(string id)
    {
        var order = await _repository.Entities.InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);
        if (order.State != OrderStateEnum.Picked)
        {
            throw Oops.Oh("待出库状态才能退回！");
        }

        _repository.Context.Tracking(order);
        order.State = OrderStateEnum.PendingApproval;
        var erpOrderoperaterecordEntity = new ErpOrderoperaterecordEntity
        {
            Id = SnowflakeIdHelper.NextId(),
            State = ((int)order.State).ToString(),
            Fid = order.Id,
            Time = DateTime.Now,
            UserId = _userManager.RealName,
            Remark = $"[{_userManager.RealName}]将订单状态改为[{(int)order.State}-{order.State.ToDescription()}]，订单退回"
        };

        // 更新状态
        await _repository.Context.Updateable<ErpOrderEntity>(order).UpdateColumns(nameof(ErpOrderEntity.State)).ExecuteCommandAsync();

        //写入订单日志
        await _repository.Context.Insertable<ErpOrderoperaterecordEntity>(erpOrderoperaterecordEntity).ExecuteCommandAsync();
    }

    /// <summary>
    /// 【配送】更新订单状态为【已分拣，待出库】（仓管前台）
    /// </summary>
    /// <param name="id">订单id</param>
    /// <param name="state">新的状态</param>
    [HttpPost("Actions/{id}/BackToPicked")]
    [SqlSugarUnitOfWork]
    public async Task PutBackToPickedFromDelivery(string id)
    {
        var order = await _repository.Entities.InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);
        if (order.State != OrderStateEnum.Outbound && order.State != OrderStateEnum.Delivery)
        {
            throw Oops.Oh("订单状态异常！");
        }

        _repository.Context.Tracking(order);
        order.State = OrderStateEnum.Picked;
        var erpOrderoperaterecordEntity = new ErpOrderoperaterecordEntity
        {
            Id = SnowflakeIdHelper.NextId(),
            State = ((int)order.State).ToString(),
            Fid = order.Id,
            Time = DateTime.Now,
            UserId = _userManager.RealName,
            Remark = $"[{_userManager.RealName}]将订单状态改为[{(int)order.State}-{order.State.ToDescription()}]，订单退回"
        };

        // 更新状态
        await _repository.Context.Updateable<ErpOrderEntity>(order).UpdateColumns(nameof(ErpOrderEntity.State)).ExecuteCommandAsync();

        //写入订单日志
        await _repository.Context.Insertable<ErpOrderoperaterecordEntity>(erpOrderoperaterecordEntity).ExecuteCommandAsync();
    }

    /// <summary>
    /// 【收货】更新订单状态为【已出库，待配送】（配送任务）
    /// </summary>
    /// <param name="id">订单id</param>
    /// <param name="state">新的状态</param>
    [HttpPost("Actions/{id}/BackToDelivery")]
    [SqlSugarUnitOfWork]
    public async Task PutBackToDeliveryFromReceiving(string id)
    {
        var order = await _repository.Entities.InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);
        if (order.State != OrderStateEnum.Receiving)
        {
            throw Oops.Oh("订单状态异常！");
        }

        _repository.Context.Tracking(order);
        order.State = OrderStateEnum.Delivery;
        var erpOrderoperaterecordEntity = new ErpOrderoperaterecordEntity
        {
            Id = SnowflakeIdHelper.NextId(),
            State = ((int)order.State).ToString(),
            Fid = order.Id,
            Time = DateTime.Now,
            UserId = _userManager.RealName,
            Remark = $"[{_userManager.RealName}]将订单状态改为[{(int)order.State}-{order.State.ToDescription()}]，订单退回"
        };

        // 更新状态
        await _repository.Context.Updateable<ErpOrderEntity>(order).UpdateColumns(nameof(ErpOrderEntity.State)).ExecuteCommandAsync();

        //写入订单日志
        await _repository.Context.Insertable<ErpOrderoperaterecordEntity>(erpOrderoperaterecordEntity).ExecuteCommandAsync();
    }

    /// <summary>
    /// 判断order是否为子单，并且判断所有子单是否为已收货
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    [NonAction]
    public async Task ProcessReceivingSubOrder(ErpOrderEntity order)
    {
        //判断是否为子单
        var relation = await _repository.Context.Queryable<ErpOrderRelationEntity>().SingleAsync(it => it.Cid == order.Id && it.Type == nameof(ErpOrderEntity));
        if (relation != null)
        {
            // 先判断订单是否完成，整单完成才更新
            var result = await App.GetService<IErpOrderFjService>().HandleSubmitCheck(relation.Oid);
            if (!result.done)
            {
                return;
            }
            // 判断其他的子单是否 "已收货，待结算"(排除当前订单，状态未更新)
            var list = await _repository.Context.Queryable<ErpOrderEntity>()
                .Where(it => SqlFunc.Subqueryable<ErpOrderRelationEntity>().Where(x => x.Cid == it.Id && x.Oid == relation.Oid).Any())
                .ToListAsync();

            bool done = true;
            foreach (var item in list)
            {
                var state = item.Id == order.Id ? order.State : item.State;
                if (state != OrderStateEnum.Receiving)
                {
                    done = false;
                    return;
                }
            }

            if (done)
            {
                var subOrderIdList = list.Select(x => x.Id).ToList();
                // 所有子单已收货，汇总子单的 分拣数，复合数，分拣金额，复合金额，退货数量到主单
                var childrenList = await _repository.Context.Queryable<ErpOrderdetailEntity>()
                    .InnerJoin<ErpOrderRelationEntity>((a, b) => a.Id == b.Cid && b.Type == nameof(ErpOrderdetailEntity))
                    .Where((a, b) => subOrderIdList.Contains(a.Fid))
                    .GroupBy((a, b) => b.Oid)
                    .Select((a, b) => new ErpOrderdetailEntity
                    {
                        Id = b.Oid,
                        Num1 = SqlFunc.AggregateSum(a.Num1),
                        Num2 = SqlFunc.AggregateSum(a.Num2),
                        Amount = SqlFunc.AggregateSum(a.Amount),
                        Amount1 = SqlFunc.AggregateSum(a.Amount1),
                        Amount2 = SqlFunc.AggregateSum(a.Amount2),
                        RejectNum = SqlFunc.AggregateSum(a.RejectNum)
                    })
                    .ToListAsync();

                var erpOrderEntity = await _repository.Context.Queryable<ErpOrderEntity>().InSingleAsync(relation.Oid) ?? throw Oops.Oh(ErrorCode.COM1005);
                var erpOrderdetailEntities = await _repository.Context.Queryable<ErpOrderdetailEntity>().Where(it => it.Fid == relation.Oid).ToListAsync();

                _repository.Context.Tracking(erpOrderEntity);
                _repository.Context.Tracking(erpOrderdetailEntities);

                // 订单主表
                erpOrderEntity.State = OrderStateEnum.Receiving;
                erpOrderEntity.Amount = childrenList.Sum(x => x.Amount);
                erpOrderEntity.Amount1 = childrenList.Sum(x => x.Amount1);
                erpOrderEntity.Amount2 = childrenList.Sum(x => x.Amount2);
                erpOrderEntity.DeliveryToTime = order.DeliveryToTime;
                //erpOrderEntity
                erpOrderEntity.ReceiveState = order.ReceiveState;
                erpOrderEntity.ReceiveTime = order.ReceiveTime;

                //订单从表
                foreach (var entity in erpOrderdetailEntities)
                {
                    var item = childrenList.Find(x => x.Id == entity.Id);
                    if (item != null)
                    {
                        entity.Num1 = item.Num1;
                        entity.Num2 = item.Num2;
                        entity.Amount = item.Amount;
                        entity.Amount1 = item.Amount1;
                        entity.Amount2 = item.Amount2;
                        entity.RejectNum = item.RejectNum;
                    }
                }

                await _repository.Context.Updateable<ErpOrderdetailEntity>(erpOrderdetailEntities).ExecuteCommandAsync();
                await _repository.Context.Updateable<ErpOrderEntity>(erpOrderEntity).ExecuteCommandAsync();
            }
        }
    }

    /// <summary>
    /// 获取订单状态的字典值
    /// </summary>
    /// <returns></returns>
    [HttpGet("state/dict")]
    public Dictionary<int, string> dynamicOrderStateDict()
    {
        var typeEnum = EnumExtensions.GetEnumDescDictionary(typeof(OrderStateEnum));
        //return typeEnum.Select(x=> new {code=x.Key,name=x.Value}).ToList();
        return typeEnum;
    }

    /// <summary>
    /// 获取订单信息列表，根据用户id过滤.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("History")]
    public async Task<dynamic> GetHistoryList([FromQuery] ErpOrderListQueryInput input)
    {
        List<DateTime> posttimeRange = input.posttimeRange?.Split(',').ToObject<List<DateTime>>();
        DateTime? startPosttimeDate = posttimeRange?.First();
        DateTime? endPosttimeDate = posttimeRange?.Last();

        List<DateTime> createTimeRange = input.createTimeRange?.Split(',').ToObject<List<DateTime>>();
        DateTime? startCreateTimeDate = createTimeRange?.First();
        DateTime? endCreateTimeDate = createTimeRange?.Last();

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
        if (input.tid.IsNotEmptyOrNull())
        {
            cidList = await _repository.Context.Queryable<ErpCustomerEntity>().Where(it => it.Type == input.tid).Select(it => it.Id)
                .Take(100).ToListAsync();

            cidList.Add(input.tid);
        }

        var data = await _repository.Context.Queryable<ErpOrderEntity>()
            .Where(it => it.Oid == _userManager.CompanyId)
            //.Where(it => SqlFunc.Subqueryable<ErpOrderoperaterecordEntity>().Where(o => o.Fid == it.Id && o.CreatorUserId == _userManager.UserId).Any()) // 筛选当前用户参与的订单
            .WhereIF(input.beginDate.HasValue, it => it.CreateTime >= input.beginDate)
            .WhereIF(input.endDate.HasValue, it => it.CreateTime <= input.endDate)
            .WhereIF(!string.IsNullOrEmpty(input.cid), it => it.Cid == input.cid)
            .WhereIF(!string.IsNullOrEmpty(input.oid), it => it.Oid == input.oid)
            .WhereIF(!string.IsNullOrEmpty(input.no), it => it.No.Contains(input.no))
            .WhereIF(posttimeRange != null, it => SqlFunc.Between(it.Posttime, startPosttimeDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), endPosttimeDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.No.Contains(input.keyword)
                )
            .WhereIF(cidList.IsAny(), it => cidList.Contains(it.Cid))
            .WhereIF(input.diningType.IsNotEmptyOrNull(), it => it.DiningType == input.diningType)
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.No.Contains(input.keyword)
                || it.Cid.Contains(input.keyword)
                )
            .WhereIF(createTimeRange != null, it => SqlFunc.Between(it.CreateTime, startCreateTimeDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), endCreateTimeDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .WhereIF(posttimeRange != null, it => SqlFunc.Between(it.Posttime, startPosttimeDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), endPosttimeDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .WhereIF(gidList.IsAny(), it => SqlFunc.Subqueryable<ErpOrderdetailEntity>().Where(d => d.Fid == it.Id && gidList.Contains(d.Mid) && d.DeleteMark == null).Any())
            .WhereIF(input.posttime.HasValue, it => it.Posttime >= input.posttime && it.Posttime < input.posttime.Value.AddDays(1))
            .OrderBy(it => it.CreatorTime, OrderByType.Desc)
            .Select(it => new ErpOrderListOutput
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
                state = it.State ?? 0
            }).ToPagedListAsync(input.currentPage, input.pageSize);

        if (data != null && data.list.IsAny())
        {
            var ids = data.list.Select(x => x.id).ToList();
            var rejectList = await _repository.Context.Queryable<ErpOrderdetailEntity>().Where(x => ids.Contains(x.Fid))
                .Where(x => x.RejectNum > 0)
                .Select(x => new ErpOrderdetailEntity
                {
                    Fid = x.Fid,
                    RejectNum = x.RejectNum,
                    SalePrice = x.SalePrice
                })
                .ToListAsync();

            foreach (var item in data.list)
            {
                if (item.posttime.HasValue)
                {
                    item.dayOfWeek = item.posttime.Value.ToString("dddd");
                }

                // 计算退货金额
                item.rejectAmount = rejectList.Where(x => x.Fid == item.id).Select(x => Math.Round((x.RejectNum ?? 0) * x.SalePrice, 2)).Sum();
            }
        }
        return PageResult<ErpOrderListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 更新订单从表信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}/itemPart")]
    public async Task ItemUpdatePart(string id, [FromBody] ErpOrderDetailUpPartInput input)
    {
        //var entity = new ErpOrderdetailEntity
        //{
        //    Id = input.id
        //};
        var entity = await _repository.Context.Queryable<ErpOrderdetailEntity>().InSingleAsync(id) ?? throw Oops.Oh("更新失败!");

        var props = entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var p = props.FirstOrDefault(x => x.Name.ToLower() == input.field.ToLower());

        if (p == null)
        {
            throw Oops.Oh($"更新的字段[{input.field}]不存在！");
        }
        var update = _repository.Context.Updateable<ErpOrderdetailEntity>()
            .Where(it => it.Id == id)
            .SetColumns(p.Name, input.value);
        //if (input.value != null)
        //{
        //    //p.SetValue(entity, Convert.ChangeType(input.value, p.PropertyType));
        //    //var exp = ExpressionBuilderHelper.CreateExpressionSelectFieldObject(typeof(ErpOrderdetailEntity), p.Name);
        //    update = update.SetColumns(p.Name, input.value);
        //}
        //update = update.SetColumns(p.Name, input.value);

        await update.EnableDiffLogEvent().ExecuteCommandAsync();
    }


    /// <summary>
    /// 用户操作过的订单
    /// </summary>
    /// <param name="stateEnum">订单状态</param>
    /// <returns></returns>
    [HttpGet("user/{stateEnum}")]
    public async Task<dynamic> UserHandleList([FromRoute] OrderStateEnum stateEnum, [FromQuery] PageInputBase input)
    {
        var state = ((int)stateEnum).ToString();
        var data = await _repository.Context.Queryable<ErpOrderEntity>()
            .Where(it => SqlFunc.Subqueryable<ErpOrderoperaterecordEntity>()
            .Where(s => s.Fid == it.Id && s.CreatorUserId == _userManager.UserId)
            .WhereIF(!string.IsNullOrEmpty(state), s => s.State == state).Any())
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.No.Contains(input.keyword)
                || it.Cid.Contains(input.keyword)
                || SqlFunc.Subqueryable<ErpCustomerEntity>().Where(d => d.Id == it.Cid && d.Name.Contains(input.keyword)).Any()
                )
            .OrderBy(it => it.CreatorTime, OrderByType.Desc)
            .Select(it => new ErpOrderListOutput
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
                deliveryCar = it.DeliveryCar,
            }).ToPagedListAsync(input.currentPage, input.pageSize);

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
        return PageResult<ErpOrderListOutput>.SqlSugarPageResult(data);
    }

    #region 导出
    /// <summary>
    /// 导出Excel.
    /// 有固定的模板.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("ExportExcel")]
    public async Task<dynamic> ExportExcel([FromQuery] ErpOrderListQueryInput input)
    {
        if (string.IsNullOrEmpty(input.oid))
        {
            input.oid = _userManager.CompanyId;
        }
        List<string> uoidList = new List<string>();
        {
            //获取用户绑定的公司
            var olist = await _usersService.GetRelationOrganizeList(_userManager.UserId);
            uoidList = olist?.Select(x => x.Id).ToList() ?? new List<string>();
            if (!uoidList.IsAny())
            {
                uoidList.Add($"NO_ORGANIZE_{_userManager.UserId}");
            }
        }
        List<string> ids = new List<string>();
        if (input.allOid == 1)
        {
            _repository.Context.QueryFilter.Clear<ICompanyEntity>();
        }

        List<DateTime> posttimeRange = input.posttimeRange?.Split(',').ToObject<List<DateTime>>();
        DateTime? startPosttimeDate = posttimeRange?.First();
        DateTime? endPosttimeDate = posttimeRange?.Last();

        //var q1 = input.allOid == 1 ? _repository.Context.Queryable<ErpOrderEntity>()
        //    .ClearFilter<ICompanyEntity>() : _repository.Context.Queryable<ErpOrderEntity>()
        //     .WhereIF(!string.IsNullOrEmpty(input.oid), it => it.Oid == input.oid);
        var qur = _repository.Context.Queryable<ErpOrderEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.oid) && input.allOid != 1, it => it.Oid == input.oid)
           .WhereIF(input.beginDate.HasValue, it => it.CreateTime >= input.beginDate)
           .WhereIF(input.endDate.HasValue, it => it.CreateTime <= input.endDate)
            .WhereIF(input.createTime.HasValue, it => it.CreateTime >= input.createTime && it.CreateTime < input.createTime.Value.AddDays(1))
            .WhereIF(input.posttime.HasValue, it => it.Posttime >= input.posttime && it.Posttime < input.posttime.Value.AddDays(1))
           .WhereIF(!string.IsNullOrEmpty(input.cid), it => it.Cid == input.cid)
           //.WhereIF(!string.IsNullOrEmpty(input.oid), it => it.Oid == input.oid)
           .WhereIF(!string.IsNullOrEmpty(input.no), it => it.No.Contains(input.no))
           .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
               it.No.Contains(input.keyword)
               )
            .Where(it => uoidList.Contains(it.Oid))
            .WhereIF(posttimeRange != null, it => SqlFunc.Between(it.Posttime, startPosttimeDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), endPosttimeDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .Where(it => SqlFunc.Subqueryable<ErpOrderRelationEntity>().Where(ddd => ddd.Cid == it.Id).NotAny()) // 过滤掉拆单记录
           .OrderBy(it => it.CreatorTime, OrderByType.Desc)
           .Select(it => new ErpOrderListOutput
           {
               id = it.Id
           });
        if (input.dataType == 2)
        {
            ids = input.items.Split(",", true).ToList();
        }
        else if (input.dataType == 0)
        {
            var pageIdList = await qur.ToPagedListAsync(input.currentPage, input.pageSize);

            ids = pageIdList.list.Select(x => x.id).ToList();
        }
        else
        {
            var pageIdList = await qur.Take(1000).ToListAsync();

            ids = pageIdList.Select(x => x.id).ToList();
        }



        var erpOrderList = await _repository.AsQueryable()
             // .WhereIF(!string.IsNullOrEmpty(input.oid), it => it.Oid == input.oid)
             //.WhereIF(ids.Any(), x => ids.Contains(x.Id))
             .Where(x => ids.Contains(x.Id))
             .LeftJoin<ErpCustomerEntity>((x, c) => x.Cid == c.Id)
            .Select((x, c) => new ErpOrderExportMasterOutput
            {
                id = x.Id,
                cidAddress = c.Address,
                cidAdmin = c.Admin,
                cidAdminTel = c.Admintel,
                cidName = c.Name,
                orderNo = x.No,
                orderDate = x.CreatorTime.HasValue ? x.CreatorTime.Value.ToString("yyyy-MM-dd HH:mm") : "",
                totalCount = "",
                totalAmount = "",
                oidName = x.Oid,
                CreateTime = x.CreateTime,
                Posttime = x.Posttime,
                DiningType = x.DiningType,
                CreateUidName = SqlFunc.Subqueryable<UserEntity>().Where(ddd => ddd.Id == x.CreateUid).Select(ddd => ddd.RealName),
                deliveryManIdName = SqlFunc.Subqueryable<ErpDeliverymanEntity>().Where(ddd => ddd.Id == x.DeliveryManId).Select(ddd => ddd.Name)
            }).ToListAsync();

        if (!erpOrderList.IsAny())
        {
            return string.Empty;
        }

        var erpOrderdetailList = await _repository.Context.Queryable<ErpOrderdetailEntity>()
            .InnerJoin<ErpProductmodelEntity>((x, a) => x.Mid == a.Id)
            .InnerJoin<ErpProductEntity>((x, a, b) => a.Pid == b.Id)
            .WhereIF(ids.Any(), x => ids.Contains(x.Fid))
            .Where(x => SqlFunc.Subqueryable<ErpOrderEntity>().Where(it => it.Id == x.Fid)
            .WhereIF(!string.IsNullOrEmpty(input.oid) && input.allOid != 1, it => it.Oid == input.oid).Any())
            .Select((x, a, b) => new ErpOrderExportDetailOutput
            {
                fid = x.Fid,
                amount = x.Amount.ToString(),
                num = x.Num.ToString(),
                price = x.SalePrice.ToString(),
                productCode = b.No,
                productName = b.Name,
                productSpec = a.Name,
                remark = x.Remark,
                productUnit = a.Unit,
                num1 = x.Num1.ToString(),
                productType = SqlFunc.Subqueryable<ViewErpProducttypeEx>().Where(ddd => ddd.Id == b.Tid).Select(ddd => ddd.RootName), // 导出的时候，类别取一级分类
                //productType = SqlFunc.Subqueryable<ErpProducttypeEntity>().Where(ddd => ddd.Id == b.Tid).Select(ddd => ddd.Name)
            })
            .ToListAsync();

        // 公司集合
        var oidList = erpOrderList.Where(x => !string.IsNullOrEmpty(x.oidName)).Select(x => x.oidName).ToArray();
        var oids = await _repository.Context.Queryable<OrganizeEntity>().Where(x => oidList.Contains(x.Id)).ToListAsync();

        var unitOptions = await _dictionaryDataService.GetList("JLDW");

        // 餐别
        var diningTypeOptions = await _dictionaryDataService.GetList("ErpOrderDiningType");

        //List<Task> exportTasks = new List<Task>();
        var tempDir = Guid.NewGuid().ToString();
        var rootPath = Path.Combine(Path.GetTempPath(), tempDir);
        if (!Directory.Exists(rootPath))
        {
            Directory.CreateDirectory(rootPath);
        }
        var templatePath = Path.Combine(App.WebHostEnvironment.WebRootPath, "Template", "Erp", "客户订单导出模板.xlsx");
        List<string> excelFilePaths = new List<string>();
        int no = 1;
        List<ErpOrderListExportDataOutput> sumList = new List<ErpOrderListExportDataOutput>(); //汇总列表
        foreach (var item in erpOrderList)
        {
            #region 设置公司名称
            var org = oids.Find(x => x.Id == item.oidName);
            if (org != null)
            {
                item.oidName = org.FullName;
                if (!string.IsNullOrEmpty(org.PropertyJson))
                {
                    var extend = org.PropertyJson.ToObject<OrganizePropertyModel>();
                    if (extend != null && !string.IsNullOrEmpty(extend.shortName))
                    {
                        item.oidName = extend.shortName;
                    }
                }
            }
            #endregion

            var details = erpOrderdetailList.Where(x => x.fid == item.id).ToList();

            if (details.Any())
            {
                item.totalCount = details.Count.ToString();
                item.totalAmount = details.Sum(x => decimal.Parse(x.amount)).ToString();

                details.ForEach(dx =>
                {
                    dx.productUnit = unitOptions.Find(x => x.EnCode == dx.productUnit)?.FullName ?? "";

                    // 转成汇总列表
                    var sumDto = new ErpOrderListExportDataOutput
                    {
                        Amount = decimal.TryParse(dx.amount, out var amount) ? amount : 0,
                        CidName = item.cidName,
                        CreateTime = item.CreateTime.HasValue ? item.CreateTime.Value.ToString("yyyy-MM-dd") : "",
                        CreateUidName = item.CreateUidName,
                        DiningType = diningTypeOptions.Find(x => x.EnCode == item.DiningType)?.FullName ?? item.DiningType,
                        Posttime = item.Posttime.HasValue ? item.Posttime.Value.ToString("yyyy-MM-dd") : "",
                        ProductType = dx.productType,
                        MidName = dx.productSpec,
                        Num = decimal.TryParse(dx.num, out var num) ? num : 0,
                        ProductName = dx.productName,
                        ProductUnit = dx.productUnit,
                        Remark = dx.remark,
                        SalePrice = decimal.TryParse(dx.price, out var price) ? price : 0,
                        OrderNo = item.orderNo,
                        Num1 = decimal.TryParse(dx.num1, out var num1) ? num1 : 0,
                        deliveryManIdName = item.deliveryManIdName
                    };
                    sumList.Add(sumDto);
                });
            }

            #region 设置导出数据源
            var path = Path.Combine(rootPath, $"{no++}-{DateTime.Parse(item.orderDate).ToString("MMdd")}-{item.cidName}.xlsx");
            excelFilePaths.Add(path);

            var param = item.ToDictionary();

            param.Add("detail", details);
            #endregion

            //exportTasks.Add(MiniExcel.SaveAsByTemplateAsync(path, templatePath, param));
            await MiniExcel.SaveAsByTemplateAsync(path, templatePath, param);
        }

        //Task.WaitAll(exportTasks.ToArray());

        var fileName = string.Format("{0:yyyyMMddHHmmss}_客户订单.xls", DateTime.Now);
        var destFile = Path.Combine(rootPath, fileName);
        ExcelExportHelper.MergeExcels(excelFilePaths.ToArray(), destFile);

        // 添加汇总列表
        if (sumList.IsAny())
        {
            XSSFWorkbook workbook;
            using (FileStream stream = File.OpenRead(destFile))
                workbook = new XSSFWorkbook(stream);

            ISheet sheet = workbook.CreateSheet("汇总明细");
            workbook.SetSheetOrder("汇总明细", 0);
            workbook.SetActiveSheet(0);
            IRow row = sheet.CreateRow(0);
            Dictionary<string, string>? FileEncode = Common.Extension.Extensions.GetPropertityToMaps<ErpOrderListExportDataOutput>();

            //var FileEncode = Common.Extension.Extensions.GetPropertityToMapsWithProperty<ErpOrderListExportDataOutput>();

            List<PropertyInfo> propertyMap = new List<PropertyInfo>();
            for (int i = 0; i < FileEncode.Count; i++)
            {
                var item = FileEncode.ElementAt(i);
                row.CreateCell(i).SetCellValue(item.Value);
                propertyMap.Add(EntityHelper<ErpOrderListExportDataOutput>.InstanceProperties.FirstOrDefault(x => x.Name == item.Key)!);
            }
            int rowNum = 0;

            var props = EntityHelper<ErpOrderListExportDataOutput>.InstanceProperties;

            foreach (var item in sumList)
            {
                rowNum++;
                IRow dataRow = sheet.CreateRow(rowNum);

                for (int i = 0; i < propertyMap.Count; i++)
                {
                    var cell = dataRow.CreateCell(i);
                    var value = propertyMap[i].GetValue(item, null);

                    if (propertyMap[i].PropertyType.IsNumericType() && double.TryParse(value?.ToString(), out double v))
                    {
                        cell.SetCellValue(v);
                        //cell.SetCellType(CellType.Numeric);
                    }
                    else
                    {
                        cell.SetCellValue(value?.ToString());
                    }


                }
            }

            using (FileStream fs = new FileStream(destFile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                workbook.Write(fs);

        }

        string url = string.Empty;
        using (var fs = new FileStream(destFile, FileMode.Open, FileAccess.Read))
        {
            var flag = await _fileManager.UploadFileByType(fs, FileVariable.TemporaryFilePath, fileName);
            if (flag.Item1)
            {
                url = flag.Item2;
                fs.Flush();
                fs.Close();
            }
        }

        //删掉临时文件夹
        if (Directory.Exists(rootPath))
        {
            Directory.Delete(rootPath, true);
        }

        return new { name = fileName, url = url ?? "/api/file/Download?encryption=" + DESCEncryption.Encrypt(_userManager.UserId + "|" + fileName + "|" + destFile, "QT") };
    }
    #endregion

    /// <summary>
    /// 获取当前账号绑定的客户id
    /// </summary>
    /// <returns></returns>
    [HttpGet("User/Customer")]
    public async Task<dynamic> GetCustomer()
    {
        var id = await _repository.Context.Queryable<ErpCustomerEntity>()
            .Where(x => x.Oid == _userManager.CompanyId && x.LoginId == _userManager.Account).Select(x => x.Id).FirstAsync();

        if (string.IsNullOrEmpty(id))
        {
            throw Oops.Oh("用户在当前公司没有绑定客户信息！");
        }

        return new { cid = id };
    }

    #region 批量上传客户订单

    /// <summary>
    /// 模板下载.
    /// </summary>
    /// <returns></returns>
    [HttpGet("TemplateDownload")]
    public async Task<dynamic> TemplateDownload()
    {
        // 初始化 一条空数据 
        List<ErpOrderListImportDataInput>? dataList = new List<ErpOrderListImportDataInput>() { new ErpOrderListImportDataInput() { } };

        ExcelConfig excelconfig = ExcelConfig.Default("批量上传客户订单.xls");
        foreach (KeyValuePair<string, string> item in Common.Extension.Extensions.GetPropertityToMaps<ErpOrderListImportDataInput>())
        {
            if (item.Key == nameof(BaseImportDataInput.ErrorMessage))
            {
                continue;
            }
            string? column = item.Key;
            string? excelColumn = item.Value;
            excelconfig.ColumnModel.Add(new ExcelColumnModel() { Column = column, ExcelColumn = excelColumn });
        }

        string? addPath = Path.Combine(FileVariable.TemporaryFilePath, excelconfig.FileName);
        //ExcelExportHelper<ErpOrderListImportDataInput>.Export(dataList, excelconfig, addPath);

        var flag = await _fileManager.UploadFileByType(ExcelExportHelper<ErpOrderListImportDataInput>.ExportMemoryStream(dataList, excelconfig), FileVariable.TemporaryFilePath, excelconfig.FileName);


        return new { name = excelconfig.FileName, url = flag.Item2 ?? "/api/file/Download?encryption=" + DESCEncryption.Encrypt(_userManager.UserId + "|" + excelconfig.FileName + "|" + addPath + "|" + _userManager.TenantId, "QT") };
    }

    ///// <summary>
    ///// 01.上传文件.
    ///// </summary>
    ///// <param name="file"></param>
    ///// <returns></returns>
    //[HttpPost("Uploader")]
    //public async Task<dynamic> Uploader(IFormFile file)
    //{
    //    var _filePath = _fileManager.GetPathByType(string.Empty);
    //    var _fileName = DateTime.Now.ToString("yyyyMMdd") + "_" + SnowflakeIdHelper.NextId() + Path.GetExtension(file.FileName);
    //    var stream = file.OpenReadStream();
    //    await _fileManager.UploadFileByType(stream, _filePath, _fileName);
    //    return new { name = _fileName, url = string.Format("/api/File/Image/{0}/{1}", string.Empty, _fileName) };
    //}

    /// <summary>
    /// 01.上传文件.（api/File/Uploader/template）
    /// 02.导入预览.
    /// </summary>
    /// <returns></returns>
    [HttpGet("ImportPreview")]
    public async Task<dynamic> ImportPreview(string fileName)
    {
        try
        {
            Dictionary<string, string>? FileEncode = Common.Extension.Extensions.GetPropertityToMaps<ErpOrderListImportDataInput>();

            //string? filePath = FileVariable.TemporaryFilePath;
            //string? savePath = Path.Combine(filePath, fileName);

            //string? filePath = Path.Combine(GetPathByType(type), fileName.Replace("@", "."));
            //await _fileManager.DownloadFileByType(filePath, fileName);

            string? filePath = Path.Combine(FileVariable.TemporaryFilePath, fileName.Replace("@", "."));
            using (var stream = (await _fileManager.DownloadFileByType(filePath, fileName))?.FileStream)
            {
                //var excelData1 = ExcelImportHelper.ToDataTable(stream,true);
                // 得到数据
                var excelData = ExcelImportHelper.ToDataTable(stream, ExcelImportHelper.IsXls(fileName));
                foreach (DataColumn item in excelData.Columns)
                {
                    //
                    if (FileEncode.ContainsValue(item.ToString()))
                    {
                        excelData.Columns[item.ToString()].ColumnName = FileEncode.Where(x => x.Value == item.ToString()).FirstOrDefault().Key;
                    }

                }

                // 返回结果
                return new { dataRow = excelData };
            }


        }
        catch (Exception e)
        {
            //UnifyContext.Fill(e.Message);
            throw Oops.Oh(ErrorCode.D1801);
        }
    }

    /// <summary>
    /// 03.导入数据.
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    [HttpPost("ImportData")]
    public async Task<dynamic> ImportData([FromBody] ErpOrderImportDataInput list)
    {
        object[]? res = await ImportOrderData(list.list);
        List<ErpOrderEntity>? addlist = res.First() as List<ErpOrderEntity>;
        List<ErpOrderListImportDataInput>? errorlist = res.Last() as List<ErpOrderListImportDataInput>;
        var output = new ErpOrderImportResultOutput()
        {
            snum = addlist.Count,
            fnum = errorlist.Count,
            //failResult = errorlist,
            resultType = errorlist.Count < 1 ? 0 : 1
        };
        output.failResult = errorlist;
        return output;
    }

    /// <summary>
    /// 04.导出错误报告.
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    [HttpPost("ExportExceptionData")]
    public async Task<dynamic> ExportExceptionData([FromBody] ErpOrderImportDataInput list)
    {
        object[]? res = await ImportOrderData(list.list);

        // 错误数据
        List<ErpOrderListImportDataInput>? errorlist = res.Last() as List<ErpOrderListImportDataInput>;

        ExcelConfig excelconfig = ExcelConfig.Default($"订单明细导入错误报告.xls");
        foreach (KeyValuePair<string, string> item in Common.Extension.Extensions.GetPropertityToMaps<ErpOrderListImportDataInput>())
        {
            string? column = item.Key;
            string? excelColumn = item.Value;
            excelconfig.ColumnModel.Add(new ExcelColumnModel() { Column = column, ExcelColumn = excelColumn });
        }

        string? addPath = Path.Combine(FileVariable.TemporaryFilePath, excelconfig.FileName);
        //ExcelExportHelper<ErpOrderListImportDataInput>.Export(errorlist, excelconfig, addPath);

        var flag = await _fileManager.UploadFileByType(ExcelExportHelper<ErpOrderListImportDataInput>.ExportMemoryStream(errorlist, excelconfig), FileVariable.TemporaryFilePath, excelconfig.FileName);
        return new { name = excelconfig.FileName, url = flag.Item2 ?? "/api/file/Download?encryption=" + DESCEncryption.Encrypt(_userManager.UserId + "|" + excelconfig.FileName + "|" + addPath + "|" + _userManager.TenantId, "QT") };
    }

    /// <summary>
    /// 导入用户数据函数.
    /// </summary>
    /// <param name="list">list.</param>
    /// <returns>[成功列表,失败列表].</returns>
    private async Task<object[]> ImportOrderData(List<ErpOrderListImportDataInput> list)
    {
        List<ErpOrderListImportDataInput> userInputList = list;
        var oid = _userManager.CompanyId;
        var oidName = "当前公司";
        if (oid.IsNotEmptyOrNull())
        {
            oidName = await _repository.Context.Queryable<OrganizeEntity>().Where(it => it.Id == oid).Select(it => it.FullName).FirstAsync();
        }
        #region 初步排除错误数据

        if (userInputList == null || userInputList.Count() < 1)
            throw Oops.Oh(ErrorCode.D5019);

        List<ErpOrderListImportDataInput>? errorList = new List<ErpOrderListImportDataInput>();
        // 必填字段验证 (配送日期、客户名称、商品名称、单位、数量、餐别)
        var requiredList = EntityHelper<ErpOrderListImportDataInput>.InstanceProperties.Where(p => p.HasAttribute<RequiredAttribute>());
        if (requiredList.Any())
        {
            errorList = userInputList.Where(x =>
            {
                var error = requiredList.Any(p => !p.GetValue(x, null).IsNotEmptyOrNull());

                if (!error)
                {
                    // 判断类型是否转换成功
                    if (
                       /*!DateTime.TryParse(x.CreateTime, out var t1)  ||*/
                       !DateTime.TryParse(x.Posttime, out var t2)
                    || !decimal.TryParse(x.Num, out var n1)
                    || !string.IsNullOrEmpty(x.Amount) && !decimal.TryParse(x.Amount, out var n2)
                    || !string.IsNullOrEmpty(x.SalePrice) && !decimal.TryParse(x.SalePrice, out var n3) // 价格填了值才去判断
                    )
                    {
                        error = true;
                        x.ErrorMessage = $"日期或者金额字段类型转换失败！";
                    }
                }
                else
                {
                    var str = string.Join(",", requiredList.Select(x => x.GetDescription()));
                    if (!string.IsNullOrEmpty(str))
                    {
                        x.ErrorMessage = $"{str}，字段不能为空！";
                    }
                }

                return error;
            }).ToList();
        }

        #endregion

        //单位字典
        var unitOptions = await _dictionaryDataService.GetList("JLDW");

        //餐别字典
        var diningTypeOptions = await _dictionaryDataService.GetList("ErpOrderDiningType");

        // 带出关联表的数据
        // 客户集合
        var customerList = await _repository.Context.Queryable<ErpCustomerEntity>().Where(it => it.Oid == oid).Select(it => new ErpCustomerEntity { Id = it.Id, Name = it.Name }).ToListAsync();
        // 用户集合
        var userList = await _repository.Context.Queryable<UserEntity>().Select(it => new UserEntity { Id = it.Id, RealName = it.RealName }).ToListAsync();
        // 商品集合
        var productList = await _repository.Context.Queryable<ErpProductmodelEntity>()
            .InnerJoin<ErpProductEntity>((a, b) => a.Pid == b.Id)
            .LeftJoin<ErpProducttypeEntity>((a, b, c) => b.Tid == c.Id)
            .Select((a, b, c) => new
            {
                productName = b.Name,
                productType = c.Name,
                midName = a.Name,
                id = a.Id,
                unit = a.Unit,
                pid = b.Id
            })
            .ToListAsync();

        // 当前公司关联的商品
        var relationCompanys = await _repository.Context.Queryable<ErpProductcompanyEntity>().Select(it => new ErpProductcompanyEntity
        {
            Pid = it.Pid,
            Oid = it.Oid
        }).ToListAsync();


        userInputList = userInputList.Except(errorList).ToList();
        List<ErpOrderListImportDataInput> newList = new List<ErpOrderListImportDataInput>();
        foreach (var item in userInputList)
        {
            // 转客户id
            var cid = customerList.Find(x => x.Name == item.CidName)?.Id ?? "";
            // 转下单员
            var createUid = userList.Find(x => x.RealName == item.CreateUidName)?.Id ?? _userManager.UserId;
            // 转单位
            var unit = unitOptions.Find(x => x.FullName == item.ProductUnit)?.EnCode ?? "";

            // 转餐别
            var diningType = diningTypeOptions.Find(x => x.FullName == item.DiningType)?.EnCode ?? "";

            // 转规格
            var qur = productList.Where(x => x.productName == item.ProductName && x.midName == item.MidName);
            if (!string.IsNullOrEmpty(item.ProductType))
            {
                qur = qur.Where(x => x.productType == item.ProductType);
            }
            if (!string.IsNullOrEmpty(unit))
            {
                qur = qur.Where(x => x.unit == unit);
            }
            var midEntity = qur.FirstOrDefault();
            var mid = midEntity?.id ?? "";

            //var mid = productList.Find(x => x.productName == item.ProductName && x.productType == item.ProductType && x.midName == item.MidName && x.unit == unit)?.id ?? "";

            // 转换后判断客户id，下单员、规格是否有空，如果是则加入错误列表
            if (!cid.IsNotEmptyOrNull() || !createUid.IsNotEmptyOrNull() || !mid.IsNotEmptyOrNull() || item.DiningType.IsNotEmptyOrNull() && !diningType.IsNotEmptyOrNull())
            {
                var errors = new List<string>();
                if (!cid.IsNotEmptyOrNull())
                {
                    errors.Add("客户不存在！");
                }
                if (!createUid.IsNotEmptyOrNull())
                {
                    errors.Add("下单员不存在！");
                }
                if (!mid.IsNotEmptyOrNull())
                {
                    errors.Add("商品规格不存在！");
                }
                if (!diningType.IsNotEmptyOrNull())
                {
                    errors.Add("餐别不存在！");
                }
                item.ErrorMessage = string.Join(", ", errors);
                errorList.Add(item);
            }
            else
            {
                // 判断规格是否绑定了当前公司
                if (relationCompanys.Any(x => x.Oid == oid && (x.Pid == midEntity!.id || x.Pid == midEntity.pid))
                    || !relationCompanys.Any(x => x.Pid == midEntity!.id || x.Pid == midEntity.pid))
                {
                    // 给记录重新赋值
                    item.CidName = cid;
                    item.CreateUidName = createUid;
                    item.MidName = mid;
                    item.DiningType = diningType;
                    newList.Add(item);
                }
                else
                {
                    item.ErrorMessage = $"商品没有关联{oidName}";
                    errorList.Add(item);
                }
            }
        }
        // 再重新排除一次异常记录
        userInputList = newList;

        List<ErpOrderEntity> addList = new List<ErpOrderEntity>();
        List<ErpOrderdetailEntity> addDetailList = new List<ErpOrderdetailEntity>();
        // 根据餐别、客户、下单日期、配送日期、下单员 分组生成订单
        ////自动按客户名称及餐别、配送日期分别生成销售订单
        foreach (var g in userInputList.GroupBy(it => new { it.DiningType, it.CidName, it.CreateTime, it.CreateUidName, it.Posttime }))
        {
            if (!DateTime.TryParse(g.Key.CreateTime, out DateTime createTime))
            {
                createTime = DateTime.Now;
            };
            DateTime.TryParse(g.Key.Posttime, out DateTime posttime);
            ErpOrderXsCrInput erpOrderCrInput = new ErpOrderXsCrInput()
            {
                cid = g.Key.CidName,
                createTime = createTime,
                posttime = posttime,
                diningType = g.Key.DiningType,
                //createUid = g.Key.CreateUidName,
                erpOrderdetailList = new List<ErpOrderdetailCrInput>()
            };
            var customerType = await _repository.Context.Queryable<ErpCustomerEntity>().Where(x => x.Id == erpOrderCrInput.cid).Select(x => x.Type).FirstAsync();

            var q1 = _repository.Context.Queryable<ErpProductcustomertypepriceEntity>().Where(xxx => xxx.Oid == _userManager.CompanyId);
            var q2 = _repository.Context.Queryable<ErpProductcustomertypepriceEntity>()
                .Where(yyy => (yyy.Oid ?? "") == "" && !SqlFunc.Subqueryable<ErpProductcustomertypepriceEntity>().Where(xxx => xxx.Oid == _userManager.CompanyId && xxx.Tid == yyy.Tid && xxx.Gid == yyy.Gid).Any());
            var qur = _repository.Context.Union(q1, q2);

            //没有单价的自动带出单价
            var customerPriceList = await _repository.Context.Queryable<ErpProductmodelEntity>()
                .LeftJoin<ErpProductEntity>((a, b) => a.Pid == b.Id)
                .LeftJoin<ErpProductpriceEntity>((a, b, c) => a.Id == c.Gid && c.Cid == erpOrderCrInput.cid)
                .LeftJoin(qur, (a, b, c, d) => a.Id == d.Gid && d.Tid == customerType)
                //.LeftJoin<ErpProductcustomertypepriceEntity>((a, b, c, d) => a.Id == d.Gid && d.Tid == customerType)
                .Where((a, b) => b.State == "1")
                .Select((a, b, c, d) => new ErpProductSelectorOutput
                {
                    id = a.Id,
                    salePrice = c.Price > 0 ? c.Price : d.PricingType == 1 ? a.SalePrice * d.Discount * 0.01m : d.PricingType == 2 ? d.Price : a.SalePrice, //优先使用客户定价，然后是客户类型折扣，再是客户类型价格，最后是原价
                }).ToListAsync();
            foreach (var item in g)
            {
                decimal salePrice = 0;
                decimal amount = 0;
                if (item.SalePrice.IsNullOrEmpty())
                {
                    salePrice = customerPriceList.FirstOrDefault(x => x.id == item.MidName)?.salePrice ?? 0;
                }
                else
                {
                    salePrice = decimal.Parse(item.SalePrice);
                }

                // 计算金额
                amount = salePrice * decimal.Parse(item.Num);
                //if (item.Amount.IsNullOrEmpty())
                //{
                //    amount = salePrice * decimal.Parse(item.Num);
                //}
                //else
                //{
                //    amount = decimal.Parse(item.Amount);
                //}
                erpOrderCrInput.erpOrderdetailList.Add(new ErpOrderdetailCrInput
                {
                    mid = item.MidName,
                    num = item.Num.IsNullOrEmpty() ? 0 : decimal.Parse(item.Num),
                    amount = amount,// item.Amount.IsNullOrEmpty() ? 0 : decimal.Parse(item.Amount),
                    salePrice = salePrice, // item.SalePrice.IsNullOrEmpty() ? 0 : decimal.Parse(item.SalePrice),
                    remark = item.Remark
                });
            }

            // 处理主表信息
            var entity = erpOrderCrInput.Adapt<ErpOrderEntity>();
            entity.Id = SnowflakeIdHelper.NextId();
            entity.CreateUid = g.Key.CreateUidName;
            entity.No = await _billRullService.GetBillNumber("QTErpOrder");
            entity.State = OrderStateEnum.Draft;
            entity.Amount = erpOrderCrInput.erpOrderdetailList?.Sum(a => a.amount) ?? 0;
            addList.Add(entity);


            // 处理明细表信息
            var erpOrderdetailEntityList = erpOrderCrInput.erpOrderdetailList.Adapt<List<ErpOrderdetailEntity>>();
            if (erpOrderdetailEntityList != null)
            {
                foreach (var item in erpOrderdetailEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.Fid = entity.Id;
                }
                addDetailList.AddRange(erpOrderdetailEntityList);
            }
        }


        if (addList.Any())
        {
            try
            {
                // 开启事务
                _db.BeginTran();

                // 新增订单记录
                await _repository.Context.Insertable(addList).ExecuteCommandAsync();

                //新增订单明细
                await _repository.Context.Insertable(addDetailList).ExecuteCommandAsync();

                _db.CommitTran();
            }
            catch (Exception)
            {
                _db.RollbackTran();
                errorList.AddRange(userInputList);
                userInputList = new List<ErpOrderListImportDataInput>();
            }
        }

        return new object[] { addList, errorList };
    }

    #endregion

    /// <summary>
    /// 记录订单打印记录
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("Actions/{id}/Print/{printId}")]
    public async Task PrintLog(string id, string printId)
    {
        var order = await _repository.AsQueryable().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);
        var printDev = await _repository.Context.Queryable<PrintDevEntity>().InSingleAsync(printId) ?? throw Oops.Oh(ErrorCode.COM1005);
        var erpOrderoperaterecordEntity = new ErpOrderoperaterecordEntity
        {
            Id = SnowflakeIdHelper.NextId(),
            State = order.State.HasValue ? ((int)order.State).ToString() : "0",
            Fid = id,
            Time = DateTime.Now,
            UserId = _userManager.RealName,
            Remark = $"[{_userManager.RealName}]打印订单[{printDev?.FullName}]"
        };
        await _repository.Context.Insertable<ErpOrderoperaterecordEntity>(erpOrderoperaterecordEntity).ExecuteCommandAsync();
    }

    /// <summary>
    /// 批量提交订单，只提交草稿状态的订单
    /// </summary>
    /// <returns></returns>
    [HttpPost("Actions/BatchProcess")]
    public async Task<int> BatchProcessOrder(ErpOrderBatchProcessInput input)
    {
        if (!input.items.IsAny())
        {
            throw Oops.Oh("请选择订单！");
        }

        // 找出草稿状态并且存在订单明细的数据
        var orderList = await _repository.Where(it => input.items.Contains(it.Id) && it.State == OrderStateEnum.Draft)
            .Where(it => SqlFunc.Subqueryable<ErpOrderdetailEntity>().Where(x => x.Fid == it.Id).Any())
            .Select(it => it.Id).ToListAsync();

        int count = 0;
        foreach (var item in orderList)
        {
            try
            {
                await ProcessOrder(item, OrderStateEnum.PendingApproval);
                count++;
            }
            catch
            {

            }
        }
        return count;
    }

    /// <summary>
    /// 批量删除订单，只删除草稿状态的订单
    /// </summary>
    /// <returns></returns>
    [HttpPost("Actions/BatchDel")]
    public async Task<int> BatchDelOrder(ErpOrderBatchProcessInput input)
    {
        if (!input.items.IsAny())
        {
            throw Oops.Oh("请选择订单！");
        }

        // 找出草稿状态并且存在订单明细的数据
        var orderList = await _repository.Where(it => input.items.Contains(it.Id) && it.State == OrderStateEnum.Draft)
            .Select(it => new ErpOrderEntity
            {
                Id = it.Id,
                DeleteTime = it.DeleteTime,
                DeleteUserId = it.DeleteUserId
            }).ToListAsync();

        int count = 0;
        if (orderList.IsAny())
        {
            var userId = _userManager.UserId;
            _repository.Context.Tracking(orderList);
            foreach (var item in orderList)
            {
                item.DeleteTime = DateTime.Now;
                item.DeleteUserId = userId;
            }
            count = await _repository.Context.Updateable<ErpOrderEntity>(orderList).EnableDiffLogEvent().ExecuteCommandAsync();
        }
        return count;
    }

    /// <summary>
    /// 按订单数量计价，
    /// </summary>
    /// <returns></returns>
    [HttpPost("Actions/{id}/Zero")]
    [SqlSugarUnitOfWork]
    public async Task OrderZero(string id)
    {
        var list = await _repository.Context.Queryable<ErpOrderdetailEntity>().Where(x => x.Fid == id && x.Num1 > x.Num).Select(x => x.Id).ToListAsync();

        if (list.IsAny())
        {
            foreach (var item in list)
            {
                await OrderDetailZero(item);
            }
        }
    }


    /// <summary>
    /// 按订单数量计算金额
    /// </summary>
    /// <returns></returns>
    [HttpPost("Actions/Detail/{id}/Zero")]
    [SqlSugarUnitOfWork]
    public async Task OrderDetailZero(string id)
    {
        //1、分拣数量大于订单数量
        //1.1、找出出库记录，和出库关联记录
        //2、更新分拣数量=订单数量，更新订单金额
        //3、多余的分拣数量归到盘亏单，注明是订单生成
        //4、盘亏记录关联出库记录

        var entity = await _repository.Context.Queryable<ErpOrderdetailEntity>().SingleAsync(x => x.Id == id) ?? throw Oops.Oh(ErrorCode.COM1005);
        if (entity.Num1 <= entity.Num)
        {
            throw Oops.Oh("分拣数量小于订单数量，无需操作！");
        }
        if (entity.Num1 <= 0)
        {
            throw Oops.Oh("没有分拣数量，无需操作！");
        }

        var mainEntity = await _repository.Context.Queryable<ErpOrderEntity>().SingleAsync(x => x.Id == entity.Fid);
        var erpOutorderEntity = await _repository.Context.Queryable<ErpOutorderEntity>().SingleAsync(x => x.Id == entity.Id);
        var erpOutrecordEntitys = await _repository.Context.Queryable<ErpOutrecordEntity>().Where(x => x.OutId == erpOutorderEntity.Id).ToListAsync();
        var erpOutdetailRecordEntitys = await _repository.Context.Queryable<ErpOutdetailRecordEntity>().Where(x => erpOutrecordEntitys.Any(e => e.Id == x.OutId)).ToListAsync();
        var erpOutrecordEntity = erpOutrecordEntitys.FirstOrDefault(x => x.Id == entity.Id);
        if (erpOutrecordEntity == null)
        {
            throw Oops.Oh("数据异常，请联系管理员！");
        }


        _repository.Context.Tracking(entity);
        _repository.Context.Tracking(mainEntity);
        _repository.Context.Tracking(erpOutrecordEntity);
        _repository.Context.Tracking(erpOutorderEntity);
        //2、更新分拣数量=订单数量，更新订单金额
        var diffAmount = entity.Amount1;
        var diffNum = entity.Num1 - entity.Num;
        entity.Num1 = entity.Num;
        entity.Amount1 = Math.Round(entity.Num1 * entity.SalePrice, 2);
        entity.Amount = entity.Amount1;

        diffAmount = diffAmount - entity.Amount1;
        mainEntity.Amount = mainEntity.Amount - diffAmount;

        //await _repository.Context.Updateable<ErpOrderdetailEntity>(entity).ExecuteCommandAsync();
        //await _repository.Context.Updateable<ErpOrderEntity>(mainEntity).ExecuteCommandAsync();

        //3、多余的分拣数量归到盘亏单，注明是订单生成，扣减原来的出库数量和金额
        var newErpOutorderEntity = new ErpOutorderEntity();
        var newErpOutrecordEntity = new ErpOutrecordEntity();
        erpOutorderEntity.Adapt(newErpOutorderEntity);
        erpOutrecordEntity.Adapt(newErpOutrecordEntity);

        newErpOutorderEntity.Id = SnowflakeIdHelper.NextId();
        newErpOutorderEntity.Amount = diffAmount;
        newErpOutorderEntity.InType = "4"; //报损出库
        newErpOutorderEntity.Remark = $"由销售订单【{mainEntity.No}】生成！";
        //明细数据
        newErpOutrecordEntity.Id = SnowflakeIdHelper.NextId();
        newErpOutrecordEntity.OutId = newErpOutorderEntity.Id;
        newErpOutrecordEntity.Num = diffNum;
        newErpOutrecordEntity.Amount = diffAmount;

        // 原来的出库记录
        erpOutrecordEntity.Num = entity.Num;
        erpOutrecordEntity.Amount = entity.Amount;
        erpOutorderEntity.Amount -= diffAmount;

        //4、盘亏记录关联出库记录
        var erpOutdetailRecordEntity = erpOutdetailRecordEntitys.FirstOrDefault(x => x.Num == diffNum);
        if (erpOutdetailRecordEntity == null)
        {
            erpOutdetailRecordEntity = erpOutdetailRecordEntitys.FirstOrDefault(x => x.Num > diffNum);
        }

        if (erpOutdetailRecordEntity == null)
        {
            throw Oops.Oh("数据异常，请联系管理员！");
        }
        var newErpOutdetailRecordEntity = new ErpOutdetailRecordEntity()
        {
            Id = SnowflakeIdHelper.NextId(),
            Gid = erpOutdetailRecordEntity.Gid,
            InId = erpOutdetailRecordEntity.InId,
            Num = diffNum,
            OutId = newErpOutrecordEntity.Id
        };
        if (erpOutdetailRecordEntity.Num == diffNum)
        {
            // 数量相等，直接删除
            await _repository.Context.Deleteable<ErpOutdetailRecordEntity>(erpOutdetailRecordEntity.Id).ExecuteCommandAsync();
        }
        else
        {
            _repository.Context.Tracking(erpOutdetailRecordEntity);
            erpOutdetailRecordEntity.Num -= diffNum;
            await _repository.Context.Updateable<ErpOutdetailRecordEntity>(erpOutdetailRecordEntity).ExecuteCommandAsync();
        }

        //
        await _repository.Context.Updateable<ErpOutrecordEntity>(erpOutrecordEntity).ExecuteCommandAsync();
        await _repository.Context.Updateable<ErpOutorderEntity>(erpOutorderEntity).ExecuteCommandAsync();

        await _repository.Context.Insertable<ErpOutdetailRecordEntity>(newErpOutdetailRecordEntity).ExecuteCommandAsync();
        await _repository.Context.Insertable<ErpOutorderEntity>(newErpOutorderEntity).ExecuteCommandAsync();
        await _repository.Context.Insertable<ErpOutrecordEntity>(newErpOutrecordEntity).ExecuteCommandAsync();

        await _repository.Context.Updateable<ErpOrderdetailEntity>(entity).ExecuteCommandAsync();
        await _repository.Context.Updateable<ErpOrderEntity>(mainEntity).ExecuteCommandAsync();
    }

    #region 移动端客户下单相关接口
    /// <summary>
    /// 获取客户分配的商品分类.
    /// </summary>
    /// <returns></returns>
    [HttpGet("Actions/Customer/{cid}/ErpProducttype")]
    public async Task<dynamic> GetCustomerSelector(string cid)
    {
        var customer = await _repository.Context.Queryable<ErpCustomerEntity>().ClearFilter().InSingleAsync(cid) ?? throw Oops.Oh("客户不存在！");

        // 获取分配的商品
        var tidList = await _repository.Context.Queryable<ErpProductEntity>().ClearFilter()
            .InnerJoin<ErpProductmodelEntity>((xxx, yyy) => xxx.Id == yyy.Pid)
            .Where((xxx, yyy) => SqlFunc.Subqueryable<ErpProductcompanyEntity>().Where(ddd => ddd.Pid == xxx.Id && ddd.Oid == customer.Oid).Any()
            || SqlFunc.Subqueryable<ErpProductcompanyEntity>().Where(ddd => ddd.Pid == yyy.Id && ddd.Oid == customer.Oid).Any())
            .Where((xxx, yyy) => SqlFunc.Subqueryable<ErpProductcustomertypepriceEntity>().Where(ddd => ddd.Gid == yyy.Id && ddd.Tid == customer.Type).Any()
            || SqlFunc.Subqueryable<ErpProductpriceEntity>().Where(ddd => ddd.Gid == yyy.Id && ddd.Cid == customer.Id).Any())
            .GroupBy(xxx => xxx.Tid)
            .Select(xxx => xxx.Tid)
            .ToListAsync();

        if (!tidList.IsAny())
        {
            throw Oops.Oh("当前客户没有分配商品！");
        }

        // 只要一级目录
        var data = await _repository.Context.Queryable<ErpProducttypeEntity>().ClearFilter().ToListAsync();

        //List<ErpProducttypeEntity>? data = await _repository.Context.Queryable<ErpProducttypeEntity>()
        //    .ClearFilter()
        //    .Where(it=> string.IsNullOrEmpty(it.Fid) || it.Fid == "0")
        //    .Where(it=> SqlFunc.Subqueryable<ErpProductEntity>().Where(xxx=>xxx.Tid == it.Id 
        //    && SqlFunc.Subqueryable<ErpProductcompanyEntity>().Where(ddd=>ddd.Pid == xxx.Id && ddd.Oid == customer.Oid).Any())
        //    .Any())
        //    .OrderBy(o => o.Code)
        //    .OrderBy(a => a.CreatorTime, OrderByType.Desc).ToListAsync();

        List<ProducttypeSelectorOutput>? treeList = data.Select(item =>
        {
            var dto = item.Adapt<ProducttypeSelectorOutput>();
            dto.parentId = item.Fid;
            dto.fullName = item.Name;
            dto.data = item;
            return dto;
        }).ToList();
        treeList = treeList.OrderBy(x => x.code).ToList() // .ToTree("-1")
            .TreeWhere(x => tidList.Contains(x.id) || tidList.Contains(x.parentId), x => x.id, x => x.parentId)
            .ToList();
        //return new { list = treeList.OrderBy(x => x.code).ToList().ToTree("-1") };

        return new
        {
            list = treeList.OrderBy(x => x.code).ToList().ToTree("-1")
        };
    }
    /// <summary>
    /// 加入购物车
    /// </summary>
    /// <returns></returns>
    [HttpPost("Actions/Cart/{cid}")]
    public async Task AddCart(string cid, [FromBody] ErpProductSelectorOutput input)
    {
        //string cid = string.Empty;
        if (string.IsNullOrEmpty(cid))
        {
            throw Oops.Oh("请选择客户");
        }
        ErpOrderShoppingCartModel cartModel = new ErpOrderShoppingCartModel()
        {
            gid = input.id,
            num = 1,
            info = input,
            cid = cid
        };
        var key = $"shoppingcart:u:{_userManager.UserId}";
        var list = await _cache.GetAsync<List<ErpOrderShoppingCartModel>>(key) ?? new List<ErpOrderShoppingCartModel>();
        var item = list.Find(x => x.id == cartModel.id);
        if (item != null)
        {
            item.num += cartModel.num;
            item.info.salePrice = cartModel.info.salePrice;
        }
        else
        {
            list.Add(cartModel);
        }

        await _cache.SetAsync(key, list);
    }

    /// <summary>
    /// 获取购物车列表
    /// </summary>
    /// <param name="cid"></param>
    /// <returns></returns>
    [HttpGet("Actions/Cart")]
    public async Task<List<ErpOrderShoppingCartByCustomerModel>> CartList([FromServices] IErpProductService erpProductService)
    {
        string cid = string.Empty;
        var key = $"shoppingcart:u:{_userManager.UserId}";
        var list = await _cache.GetAsync<List<ErpOrderShoppingCartModel>>(key) ?? new List<ErpOrderShoppingCartModel>();


        var cidList = list.Select(x => x.cid).Distinct().ToList();
        if (cidList.Any())
        {
            var cids = await _repository.Context.Queryable<ErpCustomerEntity>().Where(x => cidList.Contains(x.Id)).Select(x => new { x.Id, x.Name }).ToListAsync();
            // 根据客户分组
            var data = list.GroupBy(x => x.cid).Select(x => new ErpOrderShoppingCartByCustomerModel
            {
                cid = x.Key,
                cidName = cids.FirstOrDefault(it => it.Id == x.Key)?.Name ?? x.Key,
                list = x.ToList(),
                @checked = x.Count() > 0 && x.Count(it => !it.@checked) == 0
            }).ToList();

            var updateList = false;
            foreach (var item in data)
            {
                if (item.list.IsAny())
                {
                    var priceList = await erpProductService.QueryProductSalePrice(new ErpProductSelectorQueryInput
                    {
                        cid = item.cid,
                        filterPrice = true,
                        gidList = item.list?.Select(x => x.gid).ToList() ?? new List<string>()
                    });
                    for (int i = item.list.Count-1; i >-1; i--)
                    {
                        var xitem = item.list[i];
                        var p = priceList.Find(x => x.id == xitem.gid);
                        if (p == null)
                        {
                            // 没有定价，不显示商品
                            list.Remove(xitem);
                            item.list.RemoveAt(i);
                            updateList = true;
                            continue;
                        }
                        if (p != null && p.salePrice != xitem.info.salePrice)
                        {
                            xitem.info.salePrice = p.salePrice;
                        }
                    }
                    //foreach (var xitem in item.list)
                    //{
                    //    var p = priceList.Find(x => x.id == xitem.gid);
                    //    if (p==null)
                    //    {
                    //        // 没有定价，不显示商品
                    //        list.Remove(xitem);
                    //        continue;
                    //    }
                    //    if (p!=null && p.salePrice!=xitem.info.salePrice)
                    //    {
                    //        xitem.info.salePrice = p.salePrice;
                    //    }
                    //}
                }

            }
            if (updateList)
            {
                for (int i = data.Count-1; i>-1 ; i--)
                {
                    if (!data[i].list.IsAny())
                    {
                        data.RemoveAt(i);
                    }
                }
                await _cache.SetAsync(key, list);
            }
            return data;
        }

        return new List<ErpOrderShoppingCartByCustomerModel>();
    }

    /// <summary>
    /// 清空购物车
    /// </summary>
    /// <param name="cid"></param>
    /// <returns></returns>
    [HttpDelete("Actions/Cart")]
    public async Task DelCartList([FromBody] List<ErpOrderShoppingCartModel> input)
    {
        string cid = string.Empty;
        var key = $"shoppingcart:u:{_userManager.UserId}";
        var list = await _cache.GetAsync<List<ErpOrderShoppingCartModel>>(key) ?? new List<ErpOrderShoppingCartModel>();
        if (input == null || !input.IsAny())
        {
            list = new List<ErpOrderShoppingCartModel>();
        }
        else
        {
            var list1 = list.Where(x => input.Any(w => w.id == x.id)).ToList();
            if (list1.Any())
            {
                list = list.Except(list1).ToList();
            }
        }
        await _cache.SetAsync(key, list);
    }

    /// <summary>
    /// 更新购物车
    /// </summary>
    /// <param name="cid"></param>
    /// <returns></returns>
    [HttpPut("Actions/Cart/{id}")]
    public async Task<List<ErpOrderShoppingCartModel>> UpdateCart(string id, Dictionary<string, object> input)
    {
        string cid = string.Empty;
        var key = $"shoppingcart:u:{_userManager.UserId}";
        var list = await _cache.GetAsync<List<ErpOrderShoppingCartModel>>(key) ?? new List<ErpOrderShoppingCartModel>();

        var item = list.Find(x => x.id == id);
        if (item != null)
        {
            foreach (var p in EntityHelper<ErpOrderShoppingCartModel>.InstanceProperties.Where(x => input.Keys.Contains(x.Name)))
            {
                p.SetValue(item, Convert.ChangeType(input[p.Name], p.PropertyType));
            }
            await _cache.SetAsync(key, list);
        }
        return list;
    }


    /// <summary>
    /// 批量更新购物车选中状态
    /// </summary>
    /// <param name="cid"></param>
    /// <returns></returns>
    [HttpPost("Actions/Cart/Checked/{state}")]
    public async Task CartChecked(int state)
    {
        string cid = string.Empty;
        var key = $"shoppingcart:u:{_userManager.UserId}";
        var list = await _cache.GetAsync<List<ErpOrderShoppingCartModel>>(key) ?? new List<ErpOrderShoppingCartModel>();

        foreach (var item in list)
        {
            item.@checked = state == 1;
        }
        await _cache.SetAsync(key, list);
    }

    /// <summary>
    /// 批量更新购物车某个客户的选中状态
    /// </summary>
    /// <param name="cid"></param>
    /// <returns></returns>
    [HttpPost("Actions/Cart/{cid}/Checked/{state}")]
    public async Task CartChecked(string cid, int state)
    {
        var key = $"shoppingcart:u:{_userManager.UserId}";
        var list = await _cache.GetAsync<List<ErpOrderShoppingCartModel>>(key) ?? new List<ErpOrderShoppingCartModel>();

        foreach (var item in list)
        {
            if (item.cid == cid)
            {
                item.@checked = state == 1;
            }
        }
        await _cache.SetAsync(key, list);
    }
    #endregion


    /// <summary>
    /// 按订单数量计价，
    /// </summary>
    /// <returns></returns>
    [HttpGet("Actions/Zero")]
    [SqlSugarUnitOfWork]
    public async Task OrderZero()
    {
        var list = await _repository.Context.Queryable<ErpOrderdetailEntity>().Where(x => x.Num1 > x.Num).Select(x => x.Id).ToListAsync();

        if (list.IsAny())
        {
            foreach (var item in list)
            {
                await OrderDetailZero(item);
            }
        }
    }

    /// <summary>
    /// 按订单数量计算金额，翻转回退
    /// </summary>
    /// <returns></returns>
    [HttpPost("Actions/Detail/{id}/Zero/Reverse")]
    [SqlSugarUnitOfWork]
    public async Task OrderDetailZeroReverse(string id)
    {
        //1、查出报损出库记录
        //2、删除报损出库记录
        //3、报损数量和金额增加到销售出库单
        //4、更新中间关联表关系，报损的数量添加到出库记录中，
        //5、更新销售订单的分拣数量和金额

        var entity = await _repository.Context.Queryable<ErpOrderdetailEntity>().SingleAsync(x => x.Id == id) ?? throw Oops.Oh(ErrorCode.COM1005);
        var mainEntity = await _repository.Context.Queryable<ErpOrderEntity>().SingleAsync(x => x.Id == entity.Fid);
        var bsErpOutrecordEntity = await _repository.Context.Queryable<ErpOutrecordEntity>().Where(x => x.OrderId == entity.Id && SqlFunc.Subqueryable<ErpOutorderEntity>().Where(d => d.Id == x.OutId && d.InType == "4").Any()).FirstAsync();

        // 不存在报损数据，退出
        if (bsErpOutrecordEntity == null)
        {
            return;
        }

        var bsErpOutorderEntity = await _repository.Context.Queryable<ErpOutorderEntity>().SingleAsync(x => x.Id == bsErpOutrecordEntity.OutId);

        if (bsErpOutrecordEntity.Amount != bsErpOutorderEntity.Amount)
        {
            throw Oops.Oh("数据异常，请联系管理员！");
        }

        var erpOutorderEntity = await _repository.Context.Queryable<ErpOutorderEntity>().SingleAsync(x => x.Id == entity.Id) ?? throw Oops.Oh("销售出库记录不存在！");
        var erpOutrecordEntity = await _repository.Context.Queryable<ErpOutrecordEntity>().SingleAsync(x => x.Id == entity.Id) ?? throw Oops.Oh("销售出库记录不存在！");

        _repository.Context.Tracking(erpOutorderEntity);
        _repository.Context.Tracking(erpOutrecordEntity);

        //3、报损数量和金额增加到销售出库单
        erpOutorderEntity.Amount += bsErpOutorderEntity.Amount;
        erpOutrecordEntity.Num += bsErpOutrecordEntity.Num;
        erpOutrecordEntity.Amount += bsErpOutrecordEntity.Amount;

        //4、更新中间关联表关系
        var bsErpOutdetailRecordEntitys = await _repository.Context.Queryable<ErpOutdetailRecordEntity>().Where(x => x.OutId == bsErpOutrecordEntity.Id).ToListAsync();
        var erpOutdetailRecordEntitys = await _repository.Context.Queryable<ErpOutdetailRecordEntity>().Where(x => x.OutId == erpOutrecordEntity.Id).ToListAsync();


        List<ErpOutdetailRecordEntity> updateList = new List<ErpOutdetailRecordEntity>();
        List<ErpOutdetailRecordEntity> insertList = new List<ErpOutdetailRecordEntity>();

        foreach (var item in bsErpOutdetailRecordEntitys)
        {
            var erpOutdetailRecordEntity = erpOutdetailRecordEntitys.FirstOrDefault(x => x.InId == item.InId);
            if (erpOutdetailRecordEntity != null)
            {
                _repository.Context.Tracking(erpOutdetailRecordEntity);
                erpOutdetailRecordEntity.Num += item.Num;
                if (!updateList.Contains(erpOutdetailRecordEntity))
                {
                    updateList.Add(erpOutdetailRecordEntity);
                }
            }
            else
            {
                insertList.Add(new ErpOutdetailRecordEntity()
                {
                    Id = SnowflakeIdHelper.NextId(),
                    Gid = item.Gid,
                    InId = item.InId,
                    Num = item.Num,
                    OutId = erpOutrecordEntity.Id
                });
            }
        }

        if (updateList.IsAny())
        {
            //修改
            await _repository.Context.Updateable(updateList).ExecuteCommandAsync();
        }
        if (insertList.IsAny())
        {
            //新增
            await _repository.Context.Insertable(insertList).ExecuteCommandAsync();
        }
        if (bsErpOutdetailRecordEntitys.IsAny())
        {
            // 删除
            await _repository.Context.Deleteable<ErpOutdetailRecordEntity>(bsErpOutdetailRecordEntitys).ExecuteCommandAsync();
        }

        await _repository.Context.Updateable<ErpOutorderEntity>(erpOutorderEntity).ExecuteCommandAsync();
        await _repository.Context.Updateable<ErpOutrecordEntity>(erpOutrecordEntity).ExecuteCommandAsync();

        await _repository.Context.Deleteable<ErpOutorderEntity>(bsErpOutorderEntity).ExecuteCommandAsync();
        await _repository.Context.Deleteable<ErpOutrecordEntity>(bsErpOutrecordEntity).ExecuteCommandAsync();


        //5、更新销售订单的分拣数量和金额
        _repository.Context.Tracking(entity);
        var diffAmount = erpOutrecordEntity.Amount - entity.Amount1;
        entity.Num1 = erpOutrecordEntity.Num;
        entity.Amount1 = erpOutrecordEntity.Amount;


        _repository.Context.Tracking(mainEntity);
        mainEntity.Amount += diffAmount;

        await _repository.Context.Updateable<ErpOrderdetailEntity>(entity).ExecuteCommandAsync();
        await _repository.Context.Updateable<ErpOrderEntity>(mainEntity).ExecuteCommandAsync();
    }

    /// <summary>
    /// status = off 停用下单
    /// status = on  启用下单
    /// </summary>
    /// <param name="enCode"></param>
    /// <param name="flag"></param>
    /// <returns></returns>
    [HttpPost("Actions/CompanyStatusChange/{enCode}/{status}")]
    [AllowAnonymous]
    public async Task CompanyStatusChange(string enCode, string status)
    {
        var org = await _repository.Context.Queryable<OrganizeEntity>().Where(it => it.EnCode == enCode).FirstAsync();
        if (org != null)
        {
            var flag = status.ToLower() == "on";
            if (!string.IsNullOrEmpty(org.PropertyJson))
            {
                var extend = org.PropertyJson.ToObject<OrganizePropertyModel>();
                if (extend.enableOrder != flag)
                {
                    _repository.Context.Tracking(org);
                    extend.enableOrder = flag;
                    org.PropertyJson = JSON.Serialize(extend);
                    await _repository.Context.Updateable(org).ExecuteCommandAsync();
                }
            }
        }
    }


    /// <summary>
    /// 更新采购任务订单质检报告和自检报告
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpGet("DetailProof/{id}")]
    public async Task<ErpBuyorderCkUpProofInput> GetDetailProof(string id)
    {
        var entity = await _repository.Context.Queryable<ErpOrderdetailEntity>().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);

        return entity.Adapt<ErpBuyorderCkUpProofInput>();
    }

    /// <summary>
    /// 更新采购任务订单质检报告和自检报告
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("DetailProof/{id}")]
    public async Task DetailProof(string id, [FromBody] ErpBuyorderCkUpProofInput input)
    {
        var entity = input.Adapt<ErpOrderdetailEntity>();

        await _repository.Context.Updateable<ErpOrderdetailEntity>(entity).UpdateColumns(x => new { x.QualityReportProof }).ExecuteCommandAsync();
    }
}