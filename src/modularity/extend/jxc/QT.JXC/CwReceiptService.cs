using Mapster;
using QT.Common.Extension;
using QT.DependencyInjection;
using QT.JXC.Interfaces;
using QT.Systems.Interfaces.System;

namespace QT.JXC;

/// <summary>
/// 业务实现：收款单.
/// </summary>
[ApiDescriptionSettings(ModuleConst.ERP, Tag = "Erp", Name = "CwReceipt", Order = 200)]
[Route("api/Erp/[controller]")]
public class CwReceiptService : ICwReceiptService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<CwReceiptEntity> _repository;

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
    /// 初始化一个<see cref="CwReceiptService"/>类型的新实例.
    /// </summary>
    public CwReceiptService(
        ISqlSugarRepository<CwReceiptEntity> cwReceiptRepository,
        IBillRullService billRullService,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = cwReceiptRepository;
        _billRullService = billRullService;
        _db = context.AsTenant();
        _userManager = userManager;


        // 取消公司过滤
        _repository.Context.QueryFilter.Clear<ICompanyEntity>();
    }

    #region 增删改查
    /// <summary>
    /// 获取收款单.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var output = (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<CwReceiptInfoOutput>();

        if (output.cid.IsNotEmptyOrNull())
        {
            output.oid = await _repository.Context.Queryable<ErpCustomerEntity>().Where(x => x.Id == output.cid).Select(x => x.Oid).FirstAsync();
        }

        var cwReceiptDetailList = await _repository.Context.Queryable<CwReceiptDetailEntity>().Where(w => w.Fid == output.id).ToListAsync();
        output.cwReceiptDetailList = cwReceiptDetailList.Adapt<List<CwReceiptDetailInfoOutput>>();

        var orderIdList = cwReceiptDetailList.Where(it => !it.InId.IsNullOrEmpty() && it.InType == "order").Select(it => it.InId).ToArray();

        if (orderIdList.IsAny())
        {
            var orders = await _repository.Context.Queryable<ErpOrderEntity>().Where(it => orderIdList.Contains(it.Id)).ToListAsync();
            output.cwReceiptDetailList.ForEach(x =>
            {
                if (x.inType == "order")
                {
                    var order = orders.Find(it => it.Id == x.inId);
                    if (order != null)
                    {
                        x.inNo = order.No;
                        x.inAmount = order.Amount;
                    }
                }                
            });
        }

        var orderDetailIdList = cwReceiptDetailList.Where(it => !it.InId.IsNullOrEmpty() && it.InType == "orderdetail").Select(it => it.InId).ToArray();

        if (orderDetailIdList.IsAny())
        {
            var orders = await _repository.Context.Queryable<ErpOrderEntity>()
                .InnerJoin<ErpOrderdetailEntity>((it, a) => it.Id == a.Fid)
                .LeftJoin<ErpProductmodelEntity>((it,a,b)=>a.Mid == b.Id)
                .Where((it, a) => orderDetailIdList.Contains(a.Id))
                .Select((it, a,b) => new
                {
                    a.Id,
                    it.No,
                    a.Amount,
                    midName = b.Name,
                    productName = SqlFunc.Subqueryable<ErpProductEntity>().Where(ddd=>ddd.Id == b.Pid).Select(ddd=>ddd.Name)
                })
                .ToListAsync();
            output.cwReceiptDetailList.ForEach(x =>
            {
                if (x.inType == "orderdetail")
                {
                    var order = orders.Find(it => it.Id == x.inId);
                    if (order != null)
                    {
                        x.inNo = order.No;
                        x.inAmount = order.Amount;
                        x.productName = order.productName;
                        x.midName = order.midName;
                    }
                }                
            });
        }


        return output;
    }

    /// <summary>
    /// 获取收款单列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] CwReceiptListQueryInput input)
    {
        List<DateTime> queryReceiptDate = input.receiptDate?.Split(',').ToObject<List<DateTime>>();
        DateTime? startReceiptDate = queryReceiptDate?.First();
        DateTime? endReceiptDate = queryReceiptDate?.Last();
        var data = await _repository.Context.Queryable<CwReceiptEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.no), it => it.No.Contains(input.no))
            .WhereIF(!string.IsNullOrEmpty(input.cid), it => it.Cid.Equals(input.cid))
            .WhereIF(queryReceiptDate != null, it => SqlFunc.Between(it.ReceiptDate, startReceiptDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), endReceiptDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .WhereIF(!string.IsNullOrEmpty(input.paymentMethod), it => it.PaymentMethod.Equals(input.paymentMethod))
            .WhereIF(input.amount.HasValue, it=>it.Amount == input.amount)
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.No.Contains(input.keyword)
                || it.Cid.Contains(input.keyword)
                || it.PaymentMethod.Contains(input.keyword)
                )
            .WhereIF(input.oid.IsNotEmptyOrNull(), it => SqlFunc.Subqueryable<ErpCustomerEntity>().Where(ddd => ddd.Id == it.Cid && ddd.Oid == input.oid).Any())
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new CwReceiptListOutput
            {
                id = it.Id,
                no = it.No,
                cid = it.Cid,
                receiptDate = it.ReceiptDate,
                paymentMethod = it.PaymentMethod,
                remark = it.Remark,
                amount = it.Amount,
                cidName = SqlFunc.Subqueryable<ErpCustomerEntity>().Where(ddd=>ddd.Id == it.Cid).Select(ddd=>ddd.Name),
                oid = SqlFunc.Subqueryable<ErpCustomerEntity>().Where(ddd => ddd.Id == it.Cid).Select(ddd => ddd.Oid)
            }).OrderByPropertyNameIF(!string.IsNullOrEmpty(input.sidx), input.sidx, input.orderBy)
            .ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<CwReceiptListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建收款单.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    [SqlSugarUnitOfWork]
    public async Task Create([FromBody] CwReceiptCrInput input)
    {
        var entity = input.Adapt<CwReceiptEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        entity.No = await _billRullService.GetBillNumber("QTCWReceipt");
        entity.Amount = input.cwReceiptDetailList?.Sum(x => x.amount) ?? 0;
        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

            var newEntity = await _repository.Context.Insertable<CwReceiptEntity>(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteReturnEntityAsync();

            var cwReceiptDetailEntityList = input.cwReceiptDetailList.Adapt<List<CwReceiptDetailEntity>>();
            if (cwReceiptDetailEntityList != null)
            {
                foreach (var item in cwReceiptDetailEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.Fid = newEntity.Id;
                }

                await _repository.Context.Insertable(cwReceiptDetailEntityList).ExecuteCommandAsync();
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
    /// 更新收款单.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [SqlSugarUnitOfWork]
    public async Task Update(string id, [FromBody] CwReceiptUpInput input)
    {
        var entity = input.Adapt<CwReceiptEntity>();
        entity.Amount = input.cwReceiptDetailList?.Sum(x => x.amount) ?? 0;
        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

            await _repository.Context.Updateable<CwReceiptEntity>(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();

            // 清空收款单明细原有数据
            await _repository.Context.Deleteable<CwReceiptDetailEntity>().Where(it => it.Fid == entity.Id).ExecuteCommandAsync();

            // 新增收款单明细新数据
            var cwReceiptDetailEntityList = input.cwReceiptDetailList.Adapt<List<CwReceiptDetailEntity>>();
            if (cwReceiptDetailEntityList != null)
            {
                foreach (var item in cwReceiptDetailEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.Fid = entity.Id;
                }

                await _repository.Context.Insertable(cwReceiptDetailEntityList).ExecuteCommandAsync();
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
    /// 删除收款单.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [SqlSugarUnitOfWork]
    public async Task Delete(string id)
    {
        if (!await _repository.Context.Queryable<CwReceiptEntity>().AnyAsync(it => it.Id == id))
        {
            throw Oops.Oh(ErrorCode.COM1005);
        }

        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

            var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(id));
            await _repository.Context.Deleteable<CwReceiptEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();

            // 清空收款单明细表数据
            await _repository.Context.Deleteable<CwReceiptDetailEntity>().Where(it => it.Fid.Equals(entity.Id)).ExecuteCommandAsync();

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
    #endregion

    [HttpGet("Actions/QueryOrder")]
    public async Task<dynamic> QueryOrderList([FromQuery] CwReceiptListQueryInput input)
    {
        List<DateTime> queryCreateTime = input.createTime?.Split(',').ToObject<List<DateTime>>();
        DateTime? startCreateDate = queryCreateTime?.First();
        DateTime? endCreateDate = queryCreateTime?.Last();

        List<DateTime> queryPostTime = input.posttime?.Split(',').ToObject<List<DateTime>>();
        DateTime? startPostDate = queryPostTime?.First();
        DateTime? endPostDate = queryPostTime?.Last();


        var data = await  _repository.Context.Queryable<ErpOrderEntity>()
            .LeftJoin<ErpCustomerEntity>((it, a) => it.Cid == a.Id)
            .WhereIF(!string.IsNullOrEmpty(input.keyword), (it,a) => it.No.Contains(input.keyword) || a.Name.Contains(input.keyword) )
            .WhereIF(!string.IsNullOrEmpty(input.cid),(it,a)=>it.Cid == input.cid)
            .WhereIF(!string.IsNullOrEmpty(input.diningType), it => it.DiningType == input.diningType)
            .WhereIF(queryCreateTime != null, it => SqlFunc.Between(it.CreateTime, startCreateDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), endCreateDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .WhereIF(queryPostTime != null, it => SqlFunc.Between(it.Posttime, startPostDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), endPostDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .WhereIF(input.outNo.IsNotEmptyOrNull(), it=>SqlFunc.Subqueryable<ErpOutorderEntity>().Where(ddd=>ddd.No.Contains(input.outNo) && ddd.XsNo == it.No).Any())
            .Select((it, a) => new CwReceiptQueryOrderListOutput
            {
                id = it.Id,
                no = it.No,
                createTime = it.CreateTime,
                cidName = a.Name,
                amount = it.Amount,
                amount2 = SqlFunc.Subqueryable<CwReceiptDetailEntity>().Where(xx => xx.InId == it.Id && xx.InType == "order").Sum(xx => xx.Amount),
                posttime = it.Posttime,
                outNo = SqlFunc.Subqueryable<ErpOutorderEntity>().Where(ddd => ddd.XsNo == it.No).Select(ddd => ddd.No)
            })
            .ToPagedListAsync(input.currentPage, input.pageSize);

        foreach (var item in data.list)
        {
            item.amount3 = item.amount - item.amount2;
        }
        return PageResult<CwReceiptQueryOrderListOutput>.SqlSugarPageResult(data);
    }

    [HttpGet("Actions/QueryOrderDetail")]
    public async Task<dynamic> QueryOrderDetailList([FromQuery] CwReceiptListQueryInput input)
    {
        List<DateTime> queryCreateTime = input.createTime?.Split(',').ToObject<List<DateTime>>();
        DateTime? startCreateDate = queryCreateTime?.First();
        DateTime? endCreateDate = queryCreateTime?.Last();

        List<DateTime> queryPostTime = input.posttime?.Split(',').ToObject<List<DateTime>>();
        DateTime? startPostDate = queryPostTime?.First();
        DateTime? endPostDate = queryPostTime?.Last();

        List<string> fpOrders = new List<string>();
        if (input.fpDate.HasValue || input.fpNo.IsNotEmptyOrNull())
        {
            DateTime? ReceiptDate = null;
            if (input.fpDate.HasValue)
            {
                ReceiptDate = input.fpDate.Value.TimeStampToDateTime();
            }
            fpOrders = await _repository.Context.Queryable<CwCustomerInvoiceEntity>()
                .InnerJoin<CwCustomerInvoiceDetailEntity>((a, b) => a.Id == b.Fid)
                .WhereIF(input.fpNo.IsNotEmptyOrNull(), (a, b) => a.No == input.fpNo)
                .WhereIF(ReceiptDate.HasValue, (a, b) => SqlFunc.Between(a.ReceiptDate, ReceiptDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), ReceiptDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
                .Where((a, b) => b.InType == "order")
                .Select((a, b) => b.InId)
                .ToListAsync();

            if (!fpOrders.IsAny())
            {
                fpOrders.Add("*");
            }
        }


        var data = await _repository.Context.Queryable<ErpOrderEntity>()
            .LeftJoin<ErpCustomerEntity>((it, a) => it.Cid == a.Id)
            .LeftJoin<ErpOrderdetailEntity>((it,a,b)=>it.Id == b.Fid)
            .LeftJoin<ErpProductmodelEntity>((it,a,b,c)=>b.Mid == c.Id)
            .WhereIF(!string.IsNullOrEmpty(input.keyword), (it, a,b) => it.No.Contains(input.keyword) || a.Name.Contains(input.keyword))
            .WhereIF(!string.IsNullOrEmpty(input.cid), (it, a,b) => it.Cid == input.cid)
            .WhereIF(!string.IsNullOrEmpty(input.diningType), (it,a,b) => it.DiningType == input.diningType)
            .WhereIF(queryCreateTime != null, (it, a, b) => SqlFunc.Between(it.CreateTime, startCreateDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), endCreateDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .WhereIF(queryPostTime != null, (it, a, b) => SqlFunc.Between(it.Posttime, startPostDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), endPostDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            //.WhereIF(input.outNo.IsNotEmptyOrNull(), it => SqlFunc.Subqueryable<ErpOutorderEntity>().Where(ddd => ddd.No.Contains(input.outNo) && ddd.XsNo == it.No).Any())
            .WhereIF(input.outNo.IsNotEmptyOrNull(), (it, a, b) => SqlFunc.Subqueryable<ErpOutrecordEntity>().Where(ddd => ddd.OrderId == b.Id && SqlFunc.Subqueryable<ErpOutorderEntity>().Where(zzz=>zzz.Id == ddd.OutId && zzz.No == input.outNo).Any() ).Any())
            .WhereIF(fpOrders.IsAny(),(it,a,b)=> fpOrders.Contains(it.Id))
            .Select((it, a,b,c) => new CwReceiptQueryOrderDetailListOutput
            {
                id = b.Id, // it.Id,
                no = it.No,
                createTime = it.CreateTime,
                cidName = a.Name,
                amount = SqlFunc.IsNull(b.Amount,0), // it.Amount,
                amount2 = SqlFunc.IsNull(SqlFunc.Subqueryable<CwReceiptDetailEntity>().Where(xx => xx.InId == b.Id && xx.InType == "orderdetail").Sum(xx => xx.Amount),0),
                posttime = it.Posttime,
                outNo = SqlFunc.Subqueryable<ErpOutorderEntity>().LeftJoin<ErpOutrecordEntity>((ddd,zzz)=>ddd.Id == zzz.OutId).Where((ddd,zzz) => zzz.OutId == b.Id).Select((ddd,zzz) => ddd.No),
                midName = c.Name,
                productName = SqlFunc.Subqueryable<ErpProductEntity>().Where(xx => xx.Id == c.Pid).Select(xx => xx.Name)
            })
            .MergeTable()
            .WhereIF(!input.containZero.HasValue || !input.containZero.Value, x=> x.amount > x.amount2)
            .ToPagedListAsync(input.currentPage, input.pageSize);

        foreach (var item in data.list)
        {
            item.amount3 = item.amount - item.amount2;
        }
        return PageResult<CwReceiptQueryOrderDetailListOutput>.SqlSugarPageResult(data);
    }
}