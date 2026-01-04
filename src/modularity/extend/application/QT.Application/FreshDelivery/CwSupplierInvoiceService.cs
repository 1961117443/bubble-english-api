using QT.Application.Entitys.Dto.FreshDelivery.CwCustomerInvoice;
using QT.Application.Entitys.Dto.FreshDelivery.CwSupplierInvoice;
using QT.Application.Entitys.Dto.FreshDelivery.CwSupplierInvoiceDetail;
using QT.Application.Entitys.FreshDelivery;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.Extras.DatabaseAccessor.SqlSugar;
using QT.Systems.Interfaces.System;

namespace QT.Application.FreshDelivery;

/// <summary>
/// 业务实现：供应商发票记录.
/// </summary>
[ApiDescriptionSettings("生鲜配送", Tag = "供应商发票记录", Name = "CwSupplierInvoice")]
[Route("api/apply/FreshDelivery/[controller]")]
public class CwSupplierInvoiceService : IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<CwSupplierInvoiceEntity> _repository;

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

    /// <summary>
    /// 初始化一个<see cref="CwSupplierInvoiceService"/>类型的新实例.
    /// </summary>
    public CwSupplierInvoiceService(
        ISqlSugarRepository<CwSupplierInvoiceEntity> cwSupplierInvoiceRepository,
        IBillRullService billRullService,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = cwSupplierInvoiceRepository;
        _billRullService = billRullService;
        _db = context.AsTenant();
        _userManager = userManager;

        // 取消公司过滤
        _repository.Context.QueryFilter.Clear<ICompanyEntity>();
    }

    /// <summary>
    /// 获取付款单.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var output = (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<CwSupplierInvoiceInfoOutput>();

        var cwSupplierInvoiceDetailList = await _repository.Context.Queryable<CwSupplierInvoiceDetailEntity>().Where(w => w.Fid == output.id).ToListAsync();
        output.cwSupplierInvoiceDetailList = cwSupplierInvoiceDetailList.Adapt<List<CwSupplierInvoiceDetailInfoOutput>>();

        var orderIdList = cwSupplierInvoiceDetailList.Where(it => !it.InId.IsNullOrEmpty() && it.InType == "cg").Select(it => it.InId).ToArray();

        if (output.sid.IsNotEmptyOrNull())
        {
            output.oid = await _repository.Context.Queryable<ErpSupplierEntity>().Where(x => x.Id == output.sid).Select(x => x.Oid).FirstAsync();
        }
        if (orderIdList.IsAny())
        {
            var orderDetails = await _repository.Context.Queryable<ErpBuyorderdetailEntity>()
                .InnerJoin<ErpBuyorderEntity>((it,a)=>it.Fid == a.Id)
                .Where(it => orderIdList.Contains(it.Id))
                .Select((it,a)=>new
                {
                    id = it.Id,
                    no = a.No,
                    amount = it.Amount
                })
                .ToListAsync();
            //var orderDetails = await _repository.Context.Queryable<ErpBuyorderEntity>()
            //    .Where(it => orderIdList.Contains(it.Fid)) //.GroupBy(it=>it.Fid)
            //    //.Select(it=>new
            //    //{
            //    //    id= it.Id,
            //    //    fid = it.Fid,
            //    //    amount = SqlFunc.AggregateSum(it.Amount)
            //    //})
            //    .ToListAsync();
            output.cwSupplierInvoiceDetailList.ForEach(x =>
            {
                var item = orderDetails.Find(it => it.id == x.inId);
                if (item != null)
                {
                    x.inNo = item.no;
                    x.inAmount = item.amount;
                }
                
            });
        }

        return output;
    }

    /// <summary>
    /// 获取付款单列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] CwSupplierInvoiceListQueryInput input)
    {
        List<DateTime> querySupplierInvoiceDate = input.paymentDate?.Split(',').ToObject<List<DateTime>>();
        DateTime? startSupplierInvoiceDate = querySupplierInvoiceDate?.First();
        DateTime? endSupplierInvoiceDate = querySupplierInvoiceDate?.Last();
        var data = await _repository.Context.Queryable<CwSupplierInvoiceEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.no), it => it.No.Contains(input.no))
            .WhereIF(!string.IsNullOrEmpty(input.sid), it => it.Sid.Equals(input.sid))
            .WhereIF(querySupplierInvoiceDate != null, it => SqlFunc.Between(it.PaymentDate, startSupplierInvoiceDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), endSupplierInvoiceDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.No.Contains(input.keyword)
                || it.Sid.Contains(input.keyword)
                )
            .WhereIF(input.oid.IsNotEmptyOrNull(), it => SqlFunc.Subqueryable<ErpSupplierEntity>().Where(ddd => ddd.Id == it.Sid && ddd.Oid == input.oid).Any())
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new CwSupplierInvoiceListOutput
            {
                id = it.Id,
                no = it.No,
                sid = it.Sid,
                paymentDate = it.PaymentDate,
                paymentMethod = it.PaymentMethod,
                remark = it.Remark,
                amount=it.Amount,
                sidName = SqlFunc.Subqueryable<ErpSupplierEntity>().Where(ddd=>ddd.Id == it.Sid).Select(ddd=>ddd.Name),
                oid = SqlFunc.Subqueryable<ErpSupplierEntity>().Where(ddd => ddd.Id == it.Sid).Select(ddd => ddd.Oid)
            }).OrderByPropertyNameIF(!string.IsNullOrEmpty(input.sidx), input.sidx, input.orderBy)
            .ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<CwSupplierInvoiceListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建付款单.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    [SqlSugarUnitOfWork]
    public async Task Create([FromBody] CwSupplierInvoiceCrInput input)
    {
        var entity = input.Adapt<CwSupplierInvoiceEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        //entity.No = await _billRullService.GetBillNumber("QTCWSupplierInvoice");
        entity.Amount = input.cwSupplierInvoiceDetailList?.Sum(x => x.amount) ?? 0;
        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

            var newEntity = await _repository.Context.Insertable<CwSupplierInvoiceEntity>(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteReturnEntityAsync();

            var cwSupplierInvoiceDetailEntityList = input.cwSupplierInvoiceDetailList.Adapt<List<CwSupplierInvoiceDetailEntity>>();
            if(cwSupplierInvoiceDetailEntityList != null)
            {
                foreach (var item in cwSupplierInvoiceDetailEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.Fid = newEntity.Id;
                }

                await _repository.Context.Insertable<CwSupplierInvoiceDetailEntity>(cwSupplierInvoiceDetailEntityList).ExecuteCommandAsync();
            }

        //    // 关闭事务
        //    _db.CommitTran();
        //}
        //catch (Exception)
        //{
        //    // 回滚事务
        //    _db.RollbackTran();

        //    throw Oops.Oh(ErrorCode.COM1000);
        //}
    }

    /// <summary>
    /// 更新付款单.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [SqlSugarUnitOfWork]
    public async Task Update(string id, [FromBody] CwSupplierInvoiceUpInput input)
    {
        var entity = input.Adapt<CwSupplierInvoiceEntity>();
        entity.Amount = input.cwSupplierInvoiceDetailList?.Sum(x => x.amount) ?? 0;
        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

            await _repository.Context.Updateable<CwSupplierInvoiceEntity>(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();

            // 清空付款单明细原有数据
            await _repository.Context.Deleteable<CwSupplierInvoiceDetailEntity>().Where(it => it.Fid == entity.Id).ExecuteCommandAsync();

            // 新增付款单明细新数据
            var cwSupplierInvoiceDetailEntityList = input.cwSupplierInvoiceDetailList.Adapt<List<CwSupplierInvoiceDetailEntity>>();
            if(cwSupplierInvoiceDetailEntityList != null)
            {
                foreach (var item in cwSupplierInvoiceDetailEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.Fid = entity.Id;
                }

                await _repository.Context.Insertable<CwSupplierInvoiceDetailEntity>(cwSupplierInvoiceDetailEntityList).ExecuteCommandAsync();
            }

        //    // 关闭事务
        //    _db.CommitTran();
        //}
        //catch (Exception)
        //{
        //    // 回滚事务
        //    _db.RollbackTran();
        //    throw Oops.Oh(ErrorCode.COM1001);
        //}
    }

    /// <summary>
    /// 删除付款单.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        if(!await _repository.Context.Queryable<CwSupplierInvoiceEntity>().AnyAsync(it => it.Id == id))
        {
            throw Oops.Oh(ErrorCode.COM1005);
        }       

        try
        {
            // 开启事务
            _db.BeginTran();

            var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(id));
            await _repository.Context.Deleteable<CwSupplierInvoiceEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();

                // 清空付款单明细表数据
            await _repository.Context.Deleteable<CwSupplierInvoiceDetailEntity>().Where(it => it.Fid.Equals(entity.Id)).ExecuteCommandAsync();

            // 关闭事务
            _db.CommitTran();
        }
        catch (Exception)
        {
            // 回滚事务
            _db.RollbackTran();

            throw Oops.Oh(ErrorCode.COM1002);
        }
    }

    [HttpGet("Actions/QueryBuyorder")]
    public async Task<dynamic> QueryBuyorderList([FromQuery] CwSupplierInvoiceListQueryInput input)
    {
        List<DateTime> queryPaymentDate = input.taskBuyTimeRange?.Split(',').ToObject<List<DateTime>>();
        DateTime? startDate = queryPaymentDate?.First();
        DateTime? endDate = queryPaymentDate?.Last();
        var data = await _repository.Context.Queryable<ErpBuyorderdetailEntity>()
             .LeftJoin<ErpSupplierEntity>((it, a) => it.Supplier == a.Id)
             .InnerJoin<ErpBuyorderEntity>((it,a,b)=>it.Fid == b.Id)
             .WhereIF(!string.IsNullOrEmpty(input.keyword), (it,a,b) => b.No.Contains(input.keyword) || a.Name.Contains(input.keyword))
             .WhereIF(!string.IsNullOrEmpty(input.sid), (it, a,b) => it.Supplier == input.sid)
             .Where(it=> SqlFunc.Subqueryable<ErpProductmodelEntity>().Where(ddd=>ddd.Id == it.Gid).Any())
             .WhereIF(queryPaymentDate != null, (it, a, b) => SqlFunc.Between(b.TaskBuyTime, startDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), endDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
             .Select((it, a,b) => new CwCustomerInvoiceQueryOrderListOutput
             {
                 id = it.Id,
                 no = b.No,
                 createTime = b.TaskBuyTime,
                 cidName = a.Name,
                 amount = it.Amount,
                 amount2 = SqlFunc.Subqueryable<CwSupplierInvoiceDetailEntity>().Where(xx => xx.InId == it.Id && xx.InType == "cg").Sum(xx => xx.Amount),
                 inNum = it.InNum ?? 0,
                 tsNum = it.TsNum,
                 price = it.Price
             })
             .ToPagedListAsync(input.currentPage, input.pageSize);

        // 计算退回
        // 统计退回数量
        var thList = await _repository.Context.Queryable<ErpOutorderEntity>()
            .InnerJoin<ErpOutrecordEntity>((a, b) => a.Id == b.OutId)
            .InnerJoin<ErpOutdetailRecordEntity>((a, b, c) => b.Id == c.OutId)
            .InnerJoin<ErpInrecordEntity>((a, b, c, d) => c.InId == d.Id)
            .Where((a, b, c, d) => a.InType == "5" && data.list.Any(x => x.id == d.Bid))
            .GroupBy((a, b, c, d) => d.Bid)
            .Select((a, b, c, d) => new
            {
                bid = d.Bid,
                num = SqlFunc.AggregateSum(c.Num)
            })
            .ToListAsync();

        foreach (var item in thList)
        {
            var row = data.list.FirstOrDefault(x => x.id == item.bid);
            if (row != null)
            {
                //退回金额
                var thAmount = item.num  == (row.inNum + row.tsNum) ? (row.amount) : item.num * (row.price ?? 0);

                row.amount -= thAmount;
            }
        }

        foreach (var item in data.list)
        {
            item.amount3 = item.amount - item.amount2;
        }
        return PageResult<CwCustomerInvoiceQueryOrderListOutput>.SqlSugarPageResult(data);
    }
}