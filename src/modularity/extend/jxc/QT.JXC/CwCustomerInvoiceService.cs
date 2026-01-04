using Mapster;
using QT.Common.Extension;
using QT.DependencyInjection;
using QT.JXC.Entitys.Dto.CwCustomerInvoice;
using QT.Systems.Interfaces.System;

namespace QT.JXC;

/// <summary>
/// 业务实现：收款单.
/// </summary>
[ApiDescriptionSettings(ModuleConst.ERP, Tag = "Erp", Name = "CwCustomerInvoice", Order = 200)]
[Route("api/Erp/[controller]")]
public class CwCustomerInvoiceService : IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<CwCustomerInvoiceEntity> _repository;

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
    /// 初始化一个<see cref="CwCustomerInvoiceService"/>类型的新实例.
    /// </summary>
    public CwCustomerInvoiceService(
        ISqlSugarRepository<CwCustomerInvoiceEntity> cwCustomerInvoiceRepository,
        IBillRullService billRullService,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = cwCustomerInvoiceRepository;
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
        var output = (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<CwCustomerInvoiceInfoOutput>();

        var cwCustomerInvoiceDetailList = await _repository.Context.Queryable<CwCustomerInvoiceDetailEntity>().Where(w => w.Fid == output.id).ToListAsync();
        output.cwCustomerInvoiceDetailList = cwCustomerInvoiceDetailList.Adapt<List<CwCustomerInvoiceDetailInfoOutput>>();

        var orderIdList = cwCustomerInvoiceDetailList.Where(it => !it.InId.IsNullOrEmpty() && it.InType == "order").Select(it => it.InId).ToArray();

        if (output.cid.IsNotEmptyOrNull())
        {
            output.oid = await _repository.Context.Queryable<ErpCustomerEntity>().Where(x => x.Id == output.cid).Select(x => x.Oid).FirstAsync();
        }


        if (orderIdList.IsAny())
        {
            var orders = await _repository.Context.Queryable<ErpOrderEntity>().ClearFilter<ICompanyEntity>().Where(it => orderIdList.Contains(it.Id)).ToListAsync();
            output.cwCustomerInvoiceDetailList.ForEach(x =>
            {
                var order = orders.Find(it => it.Id == x.inId);
                if (order != null)
                {
                    x.inNo = order.No;
                    x.inAmount = order.Amount;
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
    public async Task<dynamic> GetList([FromQuery] CwCustomerInvoiceListQueryInput input)
    {
        List<DateTime> queryCustomerInvoiceDate = input.receiptDate?.Split(',').ToObject<List<DateTime>>();
        DateTime? startCustomerInvoiceDate = queryCustomerInvoiceDate?.First();
        DateTime? endCustomerInvoiceDate = queryCustomerInvoiceDate?.Last();
        var data = await _repository.Context.Queryable<CwCustomerInvoiceEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.no), it => it.No.Contains(input.no))
            .WhereIF(!string.IsNullOrEmpty(input.cid), it => it.Cid.Equals(input.cid))
            .WhereIF(queryCustomerInvoiceDate != null, it => SqlFunc.Between(it.ReceiptDate, startCustomerInvoiceDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), endCustomerInvoiceDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .WhereIF(!string.IsNullOrEmpty(input.paymentMethod), it => it.PaymentMethod.Equals(input.paymentMethod))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.No.Contains(input.keyword)
                || it.Cid.Contains(input.keyword)
                || it.PaymentMethod.Contains(input.keyword)
                )
            .WhereIF(input.oid.IsNotEmptyOrNull(), it=> SqlFunc.Subqueryable<ErpCustomerEntity>().Where(ddd=>ddd.Id == it.Cid && ddd.Oid == input.oid).Any())
            .WhereIF(input.amountMin.HasValue, it => it.Amount >= input.amountMin)
            .WhereIF(input.amountMax.HasValue, it => it.Amount <= input.amountMax)
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new CwCustomerInvoiceListOutput
            {
                id = it.Id,
                no = it.No,
                cid = it.Cid,
                receiptDate = it.ReceiptDate,
                paymentMethod = it.PaymentMethod,
                remark = it.Remark,
                amount = it.Amount,
                cidName = SqlFunc.Subqueryable<ErpCustomerEntity>().Where(ddd => ddd.Id == it.Cid).Select(ddd => ddd.Name),
                oid = SqlFunc.Subqueryable<ErpCustomerEntity>().Where(ddd => ddd.Id == it.Cid).Select(ddd => ddd.Oid)
            }).OrderByPropertyNameIF(!string.IsNullOrEmpty(input.sidx), input.sidx, input.orderBy)
            .ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<CwCustomerInvoiceListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建收款单.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    [SqlSugarUnitOfWork]
    public async Task Create([FromBody] CwCustomerInvoiceCrInput input)
    {
        var entity = input.Adapt<CwCustomerInvoiceEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        //entity.No = await _billRullService.GetBillNumber("QTCWCustomerInvoice");
        entity.Amount = input.cwCustomerInvoiceDetailList?.Sum(x => x.amount) ?? 0;
        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

            var newEntity = await _repository.Context.Insertable<CwCustomerInvoiceEntity>(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteReturnEntityAsync();

            var cwCustomerInvoiceDetailEntityList = input.cwCustomerInvoiceDetailList.Adapt<List<CwCustomerInvoiceDetailEntity>>();
            if (cwCustomerInvoiceDetailEntityList != null)
            {
                foreach (var item in cwCustomerInvoiceDetailEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.Fid = newEntity.Id;
                }

                await _repository.Context.Insertable(cwCustomerInvoiceDetailEntityList).ExecuteCommandAsync();
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
    public async Task Update(string id, [FromBody] CwCustomerInvoiceUpInput input)
    {
        var entity = input.Adapt<CwCustomerInvoiceEntity>();
        entity.Amount = input.cwCustomerInvoiceDetailList?.Sum(x => x.amount) ?? 0;
        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

            await _repository.Context.Updateable<CwCustomerInvoiceEntity>(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();

            // 清空收款单明细原有数据
            await _repository.Context.Deleteable<CwCustomerInvoiceDetailEntity>().Where(it => it.Fid == entity.Id).ExecuteCommandAsync();

            // 新增收款单明细新数据
            var cwCustomerInvoiceDetailEntityList = input.cwCustomerInvoiceDetailList.Adapt<List<CwCustomerInvoiceDetailEntity>>();
            if (cwCustomerInvoiceDetailEntityList != null)
            {
                foreach (var item in cwCustomerInvoiceDetailEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.Fid = entity.Id;
                }

                await _repository.Context.Insertable(cwCustomerInvoiceDetailEntityList).ExecuteCommandAsync();
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
        if (!await _repository.Context.Queryable<CwCustomerInvoiceEntity>().AnyAsync(it => it.Id == id))
        {
            throw Oops.Oh(ErrorCode.COM1005);
        }

        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

            var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(id));
            await _repository.Context.Deleteable<CwCustomerInvoiceEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();

            // 清空收款单明细表数据
            await _repository.Context.Deleteable<CwCustomerInvoiceDetailEntity>().Where(it => it.Fid.Equals(entity.Id)).ExecuteCommandAsync();

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
    public async Task<dynamic> QueryOrderList([FromQuery] CwCustomerInvoiceListQueryInput input)
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
            .Select((it, a) => new CwCustomerInvoiceQueryOrderListOutput
            {
                id = it.Id,
                no = it.No,
                createTime = it.CreateTime,
                cidName = a.Name,
                amount = it.Amount,
                amount2 = SqlFunc.Subqueryable<CwCustomerInvoiceDetailEntity>().Where(xx => xx.InId == it.Id && xx.InType == "order").Sum(xx => xx.Amount),
                posttime = it.Posttime
            })
            .ToPagedListAsync(input.currentPage, input.pageSize);

        foreach (var item in data.list)
        {
            item.amount3 = item.amount - item.amount2;
        }
        return PageResult<CwCustomerInvoiceQueryOrderListOutput>.SqlSugarPageResult(data);
    }
}