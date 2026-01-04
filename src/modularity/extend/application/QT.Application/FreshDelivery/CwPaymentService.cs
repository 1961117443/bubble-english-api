using QT.Application.Entitys.Dto.FreshDelivery.CwPayment;
using QT.Application.Entitys.Dto.FreshDelivery.CwPaymentDetail;
using QT.Application.Entitys.Dto.FreshDelivery.CwReceipt;
using QT.Application.Entitys.FreshDelivery;
using QT.Common.Configuration;
using QT.Common.Core.Manager.Files;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Models.NPOI;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.Extras.DatabaseAccessor.SqlSugar;
using QT.Systems.Entitys.Permission;
using QT.Systems.Interfaces.System;

namespace QT.Application.FreshDelivery;

/// <summary>
/// 业务实现：付款单.
/// </summary>
[ApiDescriptionSettings("生鲜配送", Tag = "付款单", Name = "CwPayment", Order = 200)]
[Route("api/apply/FreshDelivery/[controller]")]
public class CwPaymentService : IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<CwPaymentEntity> _repository;

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
    /// 初始化一个<see cref="CwPaymentService"/>类型的新实例.
    /// </summary>
    public CwPaymentService(
        ISqlSugarRepository<CwPaymentEntity> cwPaymentRepository,
        IBillRullService billRullService,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = cwPaymentRepository;
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
        var output = (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<CwPaymentInfoOutput>();

        var cwPaymentDetailList = await _repository.Context.Queryable<CwPaymentDetailEntity>().Where(w => w.Fid == output.id).ToListAsync();
        output.cwPaymentDetailList = cwPaymentDetailList.Adapt<List<CwPaymentDetailInfoOutput>>();

       

        var orderIdList = cwPaymentDetailList.Where(it => !it.InId.IsNullOrEmpty() && it.InType == "cg").Select(it => it.InId).ToArray();

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

            var cwPaymentDetailEntitys = await _repository.Context.Queryable<CwPaymentDetailEntity>().Where(xx => xx.InType == "cg")
                           .Where(xx => xx.Fid != output.id && orderIdList.Contains(xx.InId))
                         .GroupBy(xx => xx.InId)
                         .Select(xx => new
                         {
                             InId = xx.InId,
                             Amount = SqlFunc.AggregateSum(xx.Amount)
                         }).ToListAsync();

            //var orderDetails = await _repository.Context.Queryable<ErpBuyorderEntity>()
            //    .Where(it => orderIdList.Contains(it.Fid)) //.GroupBy(it=>it.Fid)
            //    //.Select(it=>new
            //    //{
            //    //    id= it.Id,
            //    //    fid = it.Fid,
            //    //    amount = SqlFunc.AggregateSum(it.Amount)
            //    //})
            //    .ToListAsync();
            output.cwPaymentDetailList.ForEach(x =>
            {
                var item = orderDetails.Find(it => it.id == x.inId);
                if (item != null)
                {
                    x.inNo = item.no;
                    x.inAmount = item.amount;
                }

                x.amount2 = cwPaymentDetailEntitys.Where(e => e.InId == x.inId).Sum(e => e.Amount);
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
    public async Task<dynamic> GetList([FromQuery] CwPaymentListQueryInput input)
    {
        List<DateTime> queryPaymentDate = input.paymentDate?.Split(',').ToObject<List<DateTime>>();
        DateTime? startPaymentDate = queryPaymentDate?.First();
        DateTime? endPaymentDate = queryPaymentDate?.Last();

        List<string> cgNos = new();
        if (input.cgNo.IsNotEmptyOrNull())
        {
            cgNos = await _repository.Context.Queryable<ErpBuyorderdetailEntity>().InnerJoin<ErpBuyorderEntity>((it, a) => it.Fid == a.Id)
                .Where((it, a) => a.No.Contains(input.cgNo))
                .Select((it, a) => it.Id)
                .Take(200)
                .ToListAsync();

            cgNos.Add(input.cgNo);
        }
        var data = await _repository.Context.Queryable<CwPaymentEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.no), it => it.No.Contains(input.no))
            .WhereIF(!string.IsNullOrEmpty(input.sid), it => it.Sid.Equals(input.sid))
            .WhereIF(queryPaymentDate != null, it => SqlFunc.Between(it.PaymentDate, startPaymentDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), endPaymentDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.No.Contains(input.keyword)
                || it.Sid.Contains(input.keyword)
                )
            .WhereIF(cgNos.IsAny(), it=> SqlFunc.Subqueryable<CwPaymentDetailEntity>().Where(ddd=>cgNos.Contains(ddd.InId) && ddd.Fid == it.Id).Any())
            .WhereIF(input.amountMin.HasValue, it=> it.Amount>= input.amountMin)
            .WhereIF(input.amountMax.HasValue, it => it.Amount <= input.amountMax)
            .WhereIF(input.oid.IsNotEmptyOrNull(), it => SqlFunc.Subqueryable<ErpSupplierEntity>().Where(ddd => ddd.Id == it.Sid && ddd.Oid == input.oid).Any())
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new CwPaymentListOutput
            {
                id = it.Id,
                no = it.No,
                sid = it.Sid,
                paymentDate = it.PaymentDate,
                paymentMethod = it.PaymentMethod,
                remark = it.Remark,
                amount=it.Amount,
                sidName = SqlFunc.Subqueryable<ErpSupplierEntity>().Where(ddd=>ddd.Id==it.Sid).Select(ddd=>ddd.Name),
                oid = SqlFunc.Subqueryable<ErpSupplierEntity>().Where(ddd => ddd.Id == it.Sid).Select(ddd => ddd.Oid),
            }).OrderByPropertyNameIF(!string.IsNullOrEmpty(input.sidx), input.sidx, input.orderBy)
            .ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<CwPaymentListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建付款单.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    [SqlSugarUnitOfWork]
    public async Task Create([FromBody] CwPaymentCrInput input)
    {
        var entity = input.Adapt<CwPaymentEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        entity.No = await _billRullService.GetBillNumber("QTCWPayment");
        entity.Amount = input.cwPaymentDetailList?.Sum(x => x.amount) ?? 0;
        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

            var newEntity = await _repository.Context.Insertable<CwPaymentEntity>(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteReturnEntityAsync();

            var cwPaymentDetailEntityList = input.cwPaymentDetailList.Adapt<List<CwPaymentDetailEntity>>();
            if(cwPaymentDetailEntityList != null)
            {
                foreach (var item in cwPaymentDetailEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.Fid = newEntity.Id;
                }

                await _repository.Context.Insertable<CwPaymentDetailEntity>(cwPaymentDetailEntityList).ExecuteCommandAsync();
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
    public async Task Update(string id, [FromBody] CwPaymentUpInput input)
    {
        var entity = input.Adapt<CwPaymentEntity>();
        entity.Amount = input.cwPaymentDetailList?.Sum(x => x.amount) ?? 0;
        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

            await _repository.Context.Updateable<CwPaymentEntity>(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();

            // 清空付款单明细原有数据
            await _repository.Context.Deleteable<CwPaymentDetailEntity>().Where(it => it.Fid == entity.Id).ExecuteCommandAsync();

            // 新增付款单明细新数据
            var cwPaymentDetailEntityList = input.cwPaymentDetailList.Adapt<List<CwPaymentDetailEntity>>();
            if(cwPaymentDetailEntityList != null)
            {
                foreach (var item in cwPaymentDetailEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.Fid = entity.Id;
                }

                await _repository.Context.Insertable<CwPaymentDetailEntity>(cwPaymentDetailEntityList).ExecuteCommandAsync();
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
    [SqlSugarUnitOfWork]
    public async Task Delete(string id)
    {
        if(!await _repository.Context.Queryable<CwPaymentEntity>().AnyAsync(it => it.Id == id))
        {
            throw Oops.Oh(ErrorCode.COM1005);
        }       

        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

            var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(id));
            await _repository.Context.Deleteable<CwPaymentEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();

                // 清空付款单明细表数据
            await _repository.Context.Deleteable<CwPaymentDetailEntity>().Where(it => it.Fid.Equals(entity.Id)).ExecuteCommandAsync();

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

    [HttpGet("Actions/QueryBuyorder")]
    public async Task<dynamic> QueryBuyorderList([FromQuery] CwPaymentListQueryInput input)
    {
        List<DateTime> queryPaymentDate = input.taskBuyTimeRange?.Split(',').ToObject<List<DateTime>>();
        DateTime? startDate = queryPaymentDate?.First();
        DateTime? endDate = queryPaymentDate?.Last();

        var qur = _repository.Context.Queryable<CwPaymentDetailEntity>().Where(xx => xx.InType == "cg")
            .GroupBy(xx => xx.InId)
            .Select(xx => new
            {
                Id = xx.InId,
                Amount = SqlFunc.AggregateSum(xx.Amount)
            });

        var thqur = _repository.Context.Queryable<ErpOutorderEntity>()
            .InnerJoin<ErpOutrecordEntity>((a, b) => a.Id == b.OutId)
            .InnerJoin<ErpOutdetailRecordEntity>((a, b, c) => b.Id == c.OutId)
            .InnerJoin<ErpInrecordEntity>((a, b, c, d) => c.InId == d.Id)
            .Where((a, b, c, d) => a.InType == "5")
            .GroupBy((a, b, c, d) => d.Bid)
            .Select((a, b, c, d) => new
            {
                bid = d.Bid,
                num = SqlFunc.AggregateSum(c.Num)
            });



        var data = await _repository.Context.Queryable<ErpBuyorderdetailEntity>()
             .LeftJoin<ErpSupplierEntity>((it, a) => it.Supplier == a.Id)
             .InnerJoin<ErpBuyorderEntity>((it,a,b)=>it.Fid == b.Id)
             .LeftJoin(qur, (it,a,b,c)=> it.Id == c.Id)
             .LeftJoin(thqur, (it,a,b,c,d)=> it.Id == d.bid)
             .WhereIF(!string.IsNullOrEmpty(input.keyword), (it,a,b) => b.No.Contains(input.keyword) || a.Name.Contains(input.keyword) || b.RkNo.Contains(input.keyword))
             .WhereIF(!string.IsNullOrEmpty(input.sid), (it, a,b) => it.Supplier == input.sid)
             .WhereIF(queryPaymentDate != null, (it,a,b) => SqlFunc.Between(b.TaskBuyTime, startDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), endDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
             .Select((it, a,b,c,d) => new CwReceiptQueryOrderListOutput
             {
                 id = it.Id,
                 no = b.No,
                 createTime = b.TaskBuyTime,
                 cidName = a.Name,
                 amount = it.Amount- SqlFunc.IsNull((d.num == ((it.InNum ?? 0) + (it.TsNum)) ? it.Amount : d.num * (it.Price)), 0),
                 //amount2 = SqlFunc.Subqueryable<CwPaymentDetailEntity>().Where(xx => xx.InId == it.Id && xx.InType == "cg").Sum(xx => xx.Amount),
                 amount2 = c.Amount,
                 amount3 = it.Amount - SqlFunc.IsNull(c.Amount,0) - SqlFunc.IsNull((d.num == ((it.InNum ?? 0) + (it.TsNum)) ? it.Amount : d.num * (it.Price)), 0),
                 rkNo = b.RkNo,
                 readNum = (it.InNum ?? 0)+ (it.TsNum),
                 price = it.Price
             })
             .MergeTable()
             .Where(ddd=> ddd.amount3>0)
             .ToPagedListAsync(input.currentPage, input.pageSize);

        //foreach (var item in data.list)
        //{
        //    item.amount3 = item.amount - item.amount2;
        //}
        return PageResult<CwReceiptQueryOrderListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 导出.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("ExportExcel")]
    public async Task<dynamic> ExportExcel([FromQuery] CwPaymentListQueryInput input, [FromServices] IFileManager fileManager, [FromServices] IDictionaryDataService dictionaryDataService)
    {
        input.pageSize = 10000;
        List<string> items = new List<string>();
        if (input.exportIds.IsNotEmptyOrNull())
        {
            items = input.exportIds.Split(",", true).ToList();
        }
        if (input.dataType == 1)
        {
            items.Clear();
        }
        else
        {
            items.Add(SnowflakeIdHelper.NextId()); //防止查询全部
        }


        var qur = _repository.Context.Queryable<ErpBuyorderdetailEntity>()
                .InnerJoin<ErpBuyorderEntity>((it, a) => it.Fid == a.Id)
                .Select((it, a) => new
                {
                    id = it.Id,
                    no = a.No,
                    amount = it.Amount
                });

        List<DateTime> queryPaymentDate = input.paymentDate?.Split(',').ToObject<List<DateTime>>();
        DateTime? startPaymentDate = queryPaymentDate?.First();
        DateTime? endPaymentDate = queryPaymentDate?.Last();
        var data = await _repository.Context.Queryable<CwPaymentEntity>()
            .LeftJoin<CwPaymentDetailEntity>((it,itd)=>it.Id == itd.Fid)
            .LeftJoin<ErpSupplierEntity>((it, itd,s) => it.Sid == s.Id)
            .LeftJoin(qur, (it,itd,s,q)=> itd.InId == q.id)
            .WhereIF(!string.IsNullOrEmpty(input.no), it => it.No.Contains(input.no))
            .WhereIF(!string.IsNullOrEmpty(input.sid), it => it.Sid.Equals(input.sid))
            .WhereIF(queryPaymentDate != null, it => SqlFunc.Between(it.PaymentDate, startPaymentDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), endPaymentDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.No.Contains(input.keyword)
                || it.Sid.Contains(input.keyword)
                )
            .WhereIF(input.oid.IsNotEmptyOrNull(), it => SqlFunc.Subqueryable<ErpSupplierEntity>().Where(ddd => ddd.Id == it.Sid && ddd.Oid == input.oid).Any())
            .WhereIF(items.IsAny(), it=> items.Contains(it.Id))
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            //.OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select((it,itd,s,q) => new CwPaymentExportOutput
            {
                id = itd.Id,
                no = it.No,
                sid = it.Sid,
                paymentDate = it.PaymentDate.Value.ToString("yyyy-MM-dd"),
                paymentMethod = it.PaymentMethod,
                remark = it.Remark,
                itremark = itd.Remark,
                //amount = it.Amount,
                sidName = s.Name,
                oidName = SqlFunc.Subqueryable<OrganizeEntity>().Where(ddd => ddd.Id == s.Oid).Select(ddd => ddd.FullName),
                 amount = itd.Amount,
                amount2 = SqlFunc.Subqueryable<CwPaymentDetailEntity>().Where(xx => xx.InType == "cg" && xx.InId == itd.InId && xx.Fid != itd.Fid).Sum(xx=>xx.Amount),
                inAmount =  q.amount,
                inNo = q.no
            })
            .Take(input.pageSize)
            .ToListAsync();

         

        ExcelConfig excelconfig = ExcelConfig.Default(string.Format("{0:yyyy-MM-dd}_{1}.xls", DateTime.Now, "供应商付款记录"));
        Dictionary<string, string>? FileEncode = Common.Extension.Extensions.GetPropertityToMaps<CwPaymentExportOutput>();
        foreach (KeyValuePair<string, string> item in FileEncode)
        {
            string? column = item.Key;
            string? excelColumn = item.Value;
            excelconfig.ColumnModel!.Add(new ExcelColumnModel() { Column = column, ExcelColumn = excelColumn });
        }

        string? addPath = Path.Combine(FileVariable.TemporaryFilePath, excelconfig.FileName);
        var fs = ExcelExportHelper<CwPaymentExportOutput>.ExportMemoryStream(data, excelconfig);
        var flag = await fileManager.UploadFileByType(fs, FileVariable.TemporaryFilePath, excelconfig.FileName);
        if (flag.Item1)
        {
            fs.Flush();
            fs.Close();
        }


        return new { name = excelconfig.FileName, url = flag.Item2 };
    }
}