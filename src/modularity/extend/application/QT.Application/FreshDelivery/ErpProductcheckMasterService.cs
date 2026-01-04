using QT.Application.Entitys.Dto.FreshDelivery.Base;
using QT.Application.Entitys.Dto.FreshDelivery.ErpInrecord;
using QT.Application.Entitys.Dto.FreshDelivery.ErpOutrecord;
using QT.Application.Entitys.Dto.FreshDelivery.ErpProductcheck;
using QT.Application.Entitys.FreshDelivery;
using QT.Application.Interfaces.FreshDelivery;
using QT.Common;
using QT.Common.Configuration;
using QT.Common.Core.Manager.Files;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Helper;
using QT.Common.Models.NPOI;
using QT.Common.Security;
using QT.DataEncryption;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.Extras.DatabaseAccessor.SqlSugar;
using QT.Reflection.Extensions;
using QT.Systems.Entitys.Permission;
using QT.Systems.Interfaces.System;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace QT.Application.FreshDelivery;

/// <summary>
/// 业务实现：盘点记录主表.
/// </summary>
[ApiDescriptionSettings("生鲜配送", Tag = "盘点记录", Name = "ErpProductcheckMaster", Order = 200)]
[Route("api/apply/FreshDelivery/[controller]")]
public class ErpProductcheckMasterService : IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<ErpProductcheckMasterEntity> _repository;

    /// <summary>
    /// 多租户事务.
    /// </summary>
    private readonly ITenant _db;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;
    private readonly IBillRullService _billRullService;
    private readonly IErpStoreService _erpStoreService;
    private readonly IFileManager _fileManager;
    private readonly IDictionaryDataService _dictionaryDataService;
    private readonly IErpInorderService _erpInorderService;

    /// <summary>
    /// 初始化一个<see cref="ErpProductcheckMasterService"/>类型的新实例.
    /// </summary>
    public ErpProductcheckMasterService(
        ISqlSugarRepository<ErpProductcheckMasterEntity> erpProductcheckMasterRepository,
        ISqlSugarClient context,
        IUserManager userManager,
        IBillRullService billRullService,
        IErpStoreService erpStoreService,
        IFileManager fileManager,
        IDictionaryDataService dictionaryDataService,
        IErpInorderService erpInorderService)
    {
        _repository = erpProductcheckMasterRepository;
        _db = context.AsTenant();
        _userManager = userManager;
        _billRullService = billRullService;
        _erpStoreService = erpStoreService;
        _fileManager = fileManager;
        _dictionaryDataService = dictionaryDataService;
        _erpInorderService = erpInorderService;
    }

    /// <summary>
    /// 获取盘点记录主表.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var output = (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<ErpProductcheckMasterInfoOutput>();

        var erpProductcheckList = await _repository.Context.Queryable<ErpProductcheckEntity>()
            .InnerJoin<ErpProductmodelEntity>((w, a) => w.Gid == a.Id)
            .InnerJoin<ErpProductEntity>((w, a, b) => a.Pid == b.Id)
            .Where(w => w.Fid == output.id)
            .Select((w, a, b) => new ErpProductcheckInfoOutput
            {
                gidName = a.Name,
                productName = b.Name,
                ratio = a.Ratio,
                price = w.Price,
                amount = w.Amount,
                productUnit = a.Unit,
                storeRomeIdName = SqlFunc.Subqueryable<ErpStoreroomEntity>().Where(ddd => ddd.Id == w.StoreRomeId).Select(ddd => ddd.Name),
                storeRomeAreaIdName = SqlFunc.Subqueryable<ErpStoreareaEntity>().Where(ddd => ddd.Id == w.StoreRomeAreaId).Select(ddd => ddd.Name)
            }, true)
            .ToListAsync();
        output.erpProductcheckList = erpProductcheckList; //.Adapt<List<ErpProductcheckInfoOutput>>();

        return output;
    }

    /// <summary>
    /// 获取盘点记录主表列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] ErpProductcheckMasterListQueryInput input)
    {
        List<DateTime> creatorTimeRange = input.checkTimeRange?.Split(',').ToObject<List<DateTime>>();
        DateTime? startCreatorTimeDate = creatorTimeRange?.First();
        DateTime? endCreatorTimeDate = creatorTimeRange?.Last();
        var data = await _repository.Context.Queryable<ErpProductcheckMasterEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.oid), it => it.Oid.Equals(input.oid))
            .WhereIF(!string.IsNullOrEmpty(input.no), it => it.No.Contains(input.no))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.Oid.Contains(input.keyword)
                )
            .WhereIF(input.productName.IsNotEmptyOrNull(), it => SqlFunc.Subqueryable<ErpProductcheckEntity>().Where(d1 => d1.Fid == it.Id
                    && SqlFunc.Subqueryable<ErpProductmodelEntity>().Where(d2 => d2.Id == d1.Gid
                    && SqlFunc.Subqueryable<ErpProductEntity>().Where(d3 => d3.Id == d2.Pid && d3.Name.Contains(input.productName)).Any()).Any()).Any())
            .WhereIF(creatorTimeRange != null, it => SqlFunc.Between(it.CheckTime, startCreatorTimeDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), endCreatorTimeDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new ErpProductcheckMasterListOutput
            {
                id = it.Id,
                oid = it.Oid,
                checkTime = it.CheckTime,
                no = it.No,
                auditTime = it.AuditTime,
                oidName = SqlFunc.Subqueryable<OrganizeEntity>().Where(d => d.Id == it.Oid).Select(d => d.FullName)
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<ErpProductcheckMasterListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建盘点记录主表.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    [SqlSugarUnitOfWork]
    public async Task Create([FromBody] ErpProductcheckMasterCrInput input)
    {
        var entity = input.Adapt<ErpProductcheckMasterEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        entity.No = await _billRullService.GetBillNumber("QTErpProductcheck");
        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

        var newEntity = await _repository.Context.Insertable<ErpProductcheckMasterEntity>(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteReturnEntityAsync();

        var erpProductcheckEntityList = input.erpProductcheckList.Adapt<List<ErpProductcheckEntity>>();
        if (erpProductcheckEntityList != null)
        {
            foreach (var item in erpProductcheckEntityList)
            {
                item.Id = SnowflakeIdHelper.NextId();
                item.Fid = newEntity.Id;
            }

            await _repository.Context.Insertable<ErpProductcheckEntity>(erpProductcheckEntityList).ExecuteCommandAsync();
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
    /// 更新盘点记录主表.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [SqlSugarUnitOfWork]
    public async Task Update(string id, [FromBody] ErpProductcheckMasterUpInput input)
    {
        var entity = input.Adapt<ErpProductcheckMasterEntity>();
        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

        await _repository.Context.Updateable<ErpProductcheckMasterEntity>(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();

        // 清空盘点记录原有数据
        await _repository.Context.Deleteable<ErpProductcheckEntity>().Where(it => it.Fid == entity.Id).ExecuteCommandAsync();

        // 新增盘点记录新数据
        var erpProductcheckEntityList = input.erpProductcheckList.Adapt<List<ErpProductcheckEntity>>();
        if (erpProductcheckEntityList != null)
        {
            foreach (var item in erpProductcheckEntityList)
            {
                item.Id = SnowflakeIdHelper.NextId();
                item.Fid = entity.Id;
            }

            await _repository.Context.Insertable<ErpProductcheckEntity>(erpProductcheckEntityList).ExecuteCommandAsync();
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
    /// 删除盘点记录主表.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [SqlSugarUnitOfWork]
    public async Task Delete(string id, [FromServices] IErpInorderService erpInorderService, [FromServices] IErpOutorderService erpOutorderService)
    {
        if (!await _repository.Context.Queryable<ErpProductcheckMasterEntity>().AnyAsync(it => it.Id == id))
        {
            throw Oops.Oh(ErrorCode.COM1005);
        }

        // 删除入库和出库记录
        if (await _repository.Context.Queryable<ErpInorderEntity>().AnyAsync(x => x.Id == id))
        {
            // 入库删除
            await erpInorderService.Delete(id);
        }
        if (await _repository.Context.Queryable<ErpOutorderEntity>().AnyAsync(x => x.Id == id))
        {
            // 出库删除
            await erpOutorderService.Delete(id);
        }


        //var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(id));
        await _repository.Context.Deleteable<ErpProductcheckMasterEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();

        // 清空盘点记录表数据
        await _repository.Context.Deleteable<ErpProductcheckEntity>().Where(it => it.Fid.Equals(id)).ExecuteCommandAsync();

        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

        //    var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(id));
        //    await _repository.Context.Deleteable<ErpProductcheckMasterEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();

        //    // 清空盘点记录表数据
        //    await _repository.Context.Deleteable<ErpProductcheckEntity>().Where(it => it.Fid.Equals(entity.Id)).ExecuteCommandAsync();

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
    /// 审核盘点记录主表.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpPut("Audit/{id}")]
    [SqlSugarUnitOfWork]
    public async Task Audit(string id)
    {
        var entity = await _repository.Context.Queryable<ErpProductcheckMasterEntity>().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);

        if (entity.AuditTime.HasValue)
        {
            throw Oops.Oh("单据已审核！");
        }
        // 从表记录
        var slaves = await _repository.Context.Queryable<ErpProductcheckEntity>().Where(x => x.Fid == entity.Id).ToListAsync();
        #region 【1、生成报损出库和报溢入库单】
        ErpInorderEntity erpInorder = new ErpInorderEntity()
        {
            Id = entity.Id,
            Oid = entity.Oid,
            No = entity.No,
            InType = "4",
            Remark = $"由盘点单【{entity.No}】审核生成"
        };
        ErpOutorderEntity erpOutorder = new ErpOutorderEntity()
        {
            Id = entity.Id,
            Oid = entity.Oid,
            No = entity.No,
            InType = "4",
            Remark = $"由盘点单【{entity.No}】审核生成"
        };
        // 1、生成报损出库和报溢入库单
        List<ErpInrecordEntity> erpInrecords = new List<ErpInrecordEntity>();
        List<ErpOutrecordEntity> erpOutrecords = new List<ErpOutrecordEntity>();
        foreach (var item in slaves)
        {
            //报溢入库
            if (item.LoseNum > 0)
            {
                erpInrecords.Add(new ErpInrecordEntity
                {
                    Id = item.Id,
                    Gid = item.Gid,
                    InNum = item.LoseNum,
                    Num = item.LoseNum,
                    StoreRomeAreaId = item.StoreRomeAreaId,
                    StoreRomeId = item.StoreRomeId,
                    InId = entity.Id,
                    Price = item.Price,
                    Amount = item.Amount,
                    ProductionDate = item.ProductionDate,
                    BatchNumber = item.BatchNumber,
                    Retention = item.Retention,
                });
            }
            else if (item.LoseNum < 0)
            {
                erpOutrecords.Add(new ErpOutrecordEntity
                {
                    Id = item.Id,
                    Gid = item.Gid,
                    Num = Math.Abs(item.LoseNum),
                    OutId = entity.Id,
                });
            }
        }
        #endregion

        // 如果有报溢入库的数据，取当前最后一次采购数据进行关联，方便溯源
        if (erpInrecords.IsAny())
        {
            var gidList = erpInrecords.Where(x => x.Bid.IsNullOrEmpty() && x.Gid.IsNotEmptyOrNull()).Select(x => x.Gid).Distinct().ToArray();

            var list = _repository.Context.Queryable<ErpBuyorderdetailEntity>()
                .Where(x => gidList.Contains(x.Gid))
                .Select(it => new
                {
                    index2 = SqlFunc.RowNumber($"{it.Id} desc ", it.Gid),//order by id partition by name
                                                                         //多字段排序  order by id asc ,name desc
                                                                         //SqlFunc.RowNumber($"{it.Id} asc ,{it.Name} desc ",$"{it.Name}")
                    Id = it.Id,
                    Gid = it.Gid
                })
            .MergeTable()//将结果合并成一个表
            .Where(it => it.index2 == 1) //相同的name只取一条记录
            //前20条用Where(it=>it.index2=<=20) 
           .ToList();

            //var list = await _repository.Context.Queryable<ErpBuyorderdetailEntity>()
            //     .Where(x => gidList.Contains(x.Gid))
            //     .Select(x => new ErpBuyorderdetailEntity
            //     {
            //         Id = x.Id,
            //         Gid = x.Gid
            //     }).ToListAsync();

            foreach (var x in erpInrecords)
            {
                if (x.Bid.IsNullOrEmpty() && x.Gid.IsNotEmptyOrNull())
                {
                    var item = list.Where(w => w.Gid == x.Gid).OrderByDescending(x => x.Id).FirstOrDefault();
                    if (item != null)
                    {
                        x.Bid = item.Id;
                    }
                }
            }
        }

        // 必须先出库，再入库
        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();
        #region 【2、更新数据库】
        // 报损出库，找出入库记录
        if (erpOutrecords.Any())
        {
            var gidList = erpOutrecords.Select(x => x.Gid).ToArray();

            var inList = await _repository.Context.Queryable<ErpInrecordEntity>()
                .Where(it => it.Oid == entity.Oid && it.Num > 0 && gidList.Contains(it.Gid))
                .Select(it => new { it.Id, it.Gid, it.Num })
                .ToListAsync();

            //更新库存

            foreach (var item in erpOutrecords)
            {
                var cost = await _erpStoreService.Reduce(new ErpOutdetailRecordUpInput
                {
                    id = item.Id,
                    num = item.Num,
                    records = inList.Where(x => x.Gid == item.Gid).Adapt<List<ErpOutdetailRecordInInput>>()
                });

                item.CostAmount = cost.CostAmount;
            }

            //主表
            await _repository.Context.Insertable(erpOutorder).ExecuteCommandAsync();
            //插入报损记录
            await _repository.Context.Insertable(erpOutrecords).ExecuteCommandAsync();
        }

        //插入报损记录
        if (erpInrecords.Any())
        {
            //主表
            await _repository.Context.Insertable(erpInorder).ExecuteCommandAsync();
            //从表
            await _repository.Context.Insertable(erpInrecords).ExecuteCommandAsync();
        }



        // 2、更新单据状态
        entity.AuditTime = DateTime.Now;
        entity.AuditUserId = _userManager.UserId;

        await _repository.Context.Updateable(entity).UpdateColumns(x => new { x.AuditUserId, x.AuditTime }).ExecuteCommandAsync();

        #endregion
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

    #region 批量上传盘点单

    /// <summary>
    /// 模板下载.
    /// </summary>
    /// <returns></returns>
    [HttpGet("TemplateDownload")]
    public async Task<dynamic> TemplateDownload()
    {
        // 初始化 一条空数据 
        List<ErpProductcheckMasterImportData>? dataList = new List<ErpProductcheckMasterImportData>() { new ErpProductcheckMasterImportData() { } };

        ExcelConfig excelconfig = ExcelConfig.Default("盘点单导入模板.xls");
        foreach (KeyValuePair<string, string> item in Common.Extension.Extensions.GetPropertityToMaps<ErpProductcheckMasterImportData>())
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

        var flag = await _fileManager.UploadFileByType(ExcelExportHelper<ErpProductcheckMasterImportData>.ExportMemoryStream(dataList, excelconfig), FileVariable.TemporaryFilePath, excelconfig.FileName);


        return new { name = excelconfig.FileName, url = flag.Item2 ?? "/api/file/Download?encryption=" + DESCEncryption.Encrypt(_userManager.UserId + "|" + excelconfig.FileName + "|" + addPath + "|" + _userManager.TenantId, "QT") };
    }

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
            Dictionary<string, string>? FileEncode = Common.Extension.Extensions.GetPropertityToMaps<ErpProductcheckMasterImportData>();

            string? filePath = Path.Combine(FileVariable.TemporaryFilePath, fileName.Replace("@", "."));
            using (var stream = (await _fileManager.DownloadFileByType(filePath, fileName))?.FileStream)
            {
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
    [SqlSugarUnitOfWork]
    public async Task<dynamic> ImportData([FromBody] ErpProductcheckMasterImportDataInput list)
    {
        object[]? res = await ImportOrderData(list.list);
        List<ErpProductcheckMasterCrInput>? addlist = res.First() as List<ErpProductcheckMasterCrInput>;
        List<ErpProductcheckMasterImportData>? errorlist = res.Last() as List<ErpProductcheckMasterImportData>;
        var output = new ErpProductcheckMasterImportResultOutput()
        {
            snum = addlist.Count,
            fnum = errorlist.Count,
            //failResult = errorlist,
            resultType = errorlist.Count < 1 ? 0 : 1
        };
        output.failResult = errorlist;

        if (errorlist.IsAny())
        {
            ExcelConfig excelconfig = ExcelConfig.Default($"{DateTime.Now.ToString("yyyyMMddHHmmss")}_盘点单导入错误报告.xls");
            foreach (KeyValuePair<string, string> item in Common.Extension.Extensions.GetPropertityToMaps<ErpProductcheckMasterImportErrorData>())
            {
                string? column = item.Key;
                string? excelColumn = item.Value;
                excelconfig.ColumnModel.Add(new ExcelColumnModel() { Column = column, ExcelColumn = excelColumn });
            }

            string? addPath = Path.Combine(FileVariable.TemporaryFilePath, excelconfig.FileName);

            var flag = await _fileManager.UploadFileByType(ExcelExportHelper<ErpProductcheckMasterImportData>.ExportMemoryStream(errorlist, excelconfig), FileVariable.TemporaryFilePath, excelconfig.FileName);


            output.failFileUrl = flag.Item2 ?? "/api/file/Download?encryption=" + DESCEncryption.Encrypt(_userManager.UserId + "|" + excelconfig.FileName + "|" + addPath + "|" + _userManager.TenantId, "QT");
        }
        return output;
    }

    /// <summary>
    /// 导入用户数据函数.
    /// </summary>
    /// <param name="list">list.</param>
    /// <returns>[成功列表,失败列表].</returns>
    private async Task<object[]> ImportOrderData(List<ErpProductcheckMasterImportData> list)
    {
        List<ErpProductcheckMasterImportData> userInputList = list;
        //var oid = _userManager.CompanyId;
        //var oidName = "当前公司";
        //if (oid.IsNotEmptyOrNull())
        //{
        //    oidName = await _repository.Context.Queryable<OrganizeEntity>().Where(it => it.Id == oid).Select(it => it.FullName).FirstAsync();
        //}
        #region 初步排除错误数据

        if (userInputList == null || userInputList.Count() < 1)
            throw Oops.Oh(ErrorCode.D5019);

        List<ErpProductcheckMasterImportData>? errorList = new List<ErpProductcheckMasterImportData>();
        // 必填字段验证 (配送日期、客户名称、商品名称、单位、数量、餐别)
        var requiredList = EntityHelper<ErpProductcheckMasterImportData>.InstanceProperties.Where(p => p.HasAttribute<RequiredAttribute>());
        if (requiredList.Any())
        {
            errorList = userInputList.Where(x =>
            {
                var error = requiredList.Any(p => !Common.Extension.Extensions.IsNotEmptyOrNull(p.GetValue(x, null)));

                if (!error)
                {
                    // 判断类型是否转换成功
                    if (
                       //(!DateTime.TryParse(x.CreateTime, out var t1)) ||
                       !decimal.TryParse(x.realNum, out var n1)
                    || (!string.IsNullOrEmpty(x.amount) && !decimal.TryParse(x.amount, out var n2))
                    || (!string.IsNullOrEmpty(x.price) && !decimal.TryParse(x.price, out var n3)) // 价格填了值才去判断
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

        // 公司集合
        var organizeList = await _repository.Context.Queryable<OrganizeEntity>().Select(it => new OrganizeEntity { Id = it.Id, FullName = it.FullName }).ToListAsync();

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

        var erpStoreroomList = await _repository.Context.Queryable<ErpStoreroomEntity>().ClearFilter<ICompanyEntity>().ToListAsync();
        var erpStoreareaList = await _repository.Context.Queryable<ErpStoreareaEntity>().ClearFilter<ICompanyEntity>().ToListAsync();




        userInputList = userInputList.Except(errorList).ToList();
        List<ErpProductcheckMasterImportData> newList = new List<ErpProductcheckMasterImportData>();
        foreach (var item in userInputList)
        {
            // 盘点公司id
            item.oid = organizeList.Find(x => x.FullName == item.OidName)?.Id ?? "";


            // 转规格
            var qur = productList.Where(x => x.productName == item.ProductName && x.midName == item.MidName);


            var midEntity = qur.FirstOrDefault();
            item.gid = midEntity?.id ?? "";

            //var mid = productList.Find(x => x.productName == item.ProductName && x.productType == item.ProductType && x.midName == item.MidName && x.unit == unit)?.id ?? "";

            var errors = new List<string>();
            // 转换后判断客户id，下单员、规格是否有空，如果是则加入错误列表
            if (!item.oid.IsNotEmptyOrNull() || !item.gid.IsNotEmptyOrNull())
            {
                if (!item.oid.IsNotEmptyOrNull())
                {
                    errors.Add("盘点公司不存在！");
                }
                else
                {

                }
                if (!item.gid.IsNotEmptyOrNull())
                {
                    errors.Add("商品规格不存在！");
                }

                //item.ErrorMessage = string.Join(", ", errors);
                //errorList.Add(item);
            }
            else
            {
                // 判断规格是否绑定了当前公司
                if (relationCompanys.Any(x => x.Oid == item.oid && (x.Pid == midEntity!.id || x.Pid == midEntity.pid))
                    || !(relationCompanys.Any(x => x.Pid == midEntity!.id || x.Pid == midEntity.pid)))
                {
                    // 给记录重新赋值
                    //item.Oid1Name = oid1;
                    //item.Oid2Name = oid2;
                    //item.CreateUidName = createUid;
                    //item.MidName = mid;
                    //item.DiningType = diningType;
                    newList.Add(item);
                }
                else
                {
                    //item.ErrorMessage = $"商品没有关联{item.OidName}";
                    //errorList.Add(item);
                    errors.Add($"商品没有关联{item.OidName}");
                }

                if (item.storeRome.IsNotEmptyOrNull())
                {
                    var room = erpStoreroomList.FirstOrDefault(x => x.Name == item.storeRome && x.Oid == item.oid);
                    if (room == null)
                    {
                        errors.Add("仓库不存在！");
                    }
                    else
                    {
                        item.storeRomeId = room.Id;
                        if (item.storeRomeArea.IsNotEmptyOrNull())
                        {
                            var area = erpStoreareaList.FirstOrDefault(x => x.Name == item.storeRomeArea && x.Sid == item.storeRomeId);
                            if (area == null)
                            {
                                errors.Add("库区不存在！");
                            }
                            else
                            {
                                item.storeRomeAreaId = area.Id;
                            }
                        }
                    }
                }
            }

            if (errors.IsAny())
            {
                item.ErrorMessage = string.Join(", ", errors);
                errorList.Add(item);
            }

        }

        var dump = newList.GroupBy(x => new { x.oid, x.gid }).Where(x => x.Count() > 1).SelectMany(x => x).ToList();
        if (dump.IsAny())
        {
            // 排除重复
            dump.ForEach(x =>
            {
                if (!errorList.Contains(x))
                {
                    x.ErrorMessage = $"商品重复！";
                    errorList.Add(x);
                }
                else
                {
                    x.ErrorMessage += $", 商品重复！";
                }
                newList.Remove(x);
            });
        }

        // 再重新排除一次异常记录
        userInputList = newList;

        //List<ErpOutorderEntity> addList = new List<ErpOutorderEntity>();
        List<ErpOutrecordEntity> addDetailList = new List<ErpOutrecordEntity>();
        // 根据调出公司，调入公司 分组生成订单


        List<ErpProductcheckMasterCrInput> addList = new List<ErpProductcheckMasterCrInput>();
        foreach (var g in userInputList.GroupBy(it => new { it.oid, it.checkTime }))
        {
            ErpProductcheckMasterCrInput erpOutorderCrInput = new ErpProductcheckMasterCrInput
            {
                oid = g.Key.oid,
                checkTime = g.Key.checkTime,
                erpProductcheckList = new List<ErpProductcheckCrInput>()

            };
            foreach (var x in g)
            {
                var item = new ErpProductcheckCrInput
                {
                    gid = x.gid,
                    realNum = decimal.Parse(x.realNum),
                    price = x.price.IsNotEmptyOrNull() ? decimal.Parse(x.price) : 0,
                    amount = 0 // x.amount.IsNotEmptyOrNull() ? decimal.Parse(x.amount) : 0
                    ,
                    batchNumber = x.batchNumber,
                    productionDate = x.productionDate,
                    remark = x.remark,
                    retention = x.retention,
                    storeRomeAreaId = x.storeRomeAreaId,
                    storeRomeId = x.storeRomeId,
                };

                // 获取库存
                var storeDetailList = await _erpInorderService.GetStore(x.gid, new ErpStoreListQueryInput
                {
                    oid = g.Key.oid
                });

                if (storeDetailList.IsAny())
                {
                    //// 库存不足
                    //x.ErrorMessage = $"库存不足";
                    //errorList.Add(x);
                    //continue;
                    item.systemNum = storeDetailList.Sum(s => s.num);
                }
                else
                {
                    item.systemNum = 0;
                }

                item.loseNum = item.realNum - item.systemNum;

                if (item.loseNum > 0 && item.price > 0)
                {
                    item.amount = item.loseNum * item.price;
                }

                erpOutorderCrInput.erpProductcheckList.Add(item);
            }

            if (erpOutorderCrInput.erpProductcheckList.IsAny())
            {
                addList.Add(erpOutorderCrInput);
            }
        }

        if (addList.IsAny())
        {
            foreach (var item in addList)
            {
                // 导入调出单
                await this.Create(item);

            }
        }


        return new object[] { addList, errorList };
    }

    #endregion

    #region 导出
    /// <summary>
    /// 导出Excel.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("ExportExcel")]
    public async Task<dynamic> ExportExcel([FromQuery] ErpProductcheckMasterListQueryInput input)
    {
        List<string> ids = new List<string>();
        if (input.dataType == 0)
        {
            //var data = await _repository.Context.Queryable<ErpProductcheckMasterEntity>()
            //.WhereIF(!string.IsNullOrEmpty(input.oid), it => it.Oid.Equals(input.oid))
            //.WhereIF(!string.IsNullOrEmpty(input.no), it => it.No.Contains(input.no))
            //.WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
            //    it.Oid.Contains(input.keyword)
            //    )
            //.OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            //.Select(it => new ErpProductcheckMasterListOutput
            //{
            //    id = it.Id,
            //    oid = it.Oid,
            //    checkTime = it.CheckTime,
            //    no = it.No,
            //    auditTime = it.AuditTime,
            //    oidName = SqlFunc.Subqueryable<OrganizeEntity>().Where(d => d.Id == it.Oid).Select(d => d.FullName)
            //}).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort)

            var pageIdList = await _repository.Context.Queryable<ErpProductcheckMasterEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.oid), it => it.Oid.Equals(input.oid))
            .WhereIF(!string.IsNullOrEmpty(input.no), it => it.No.Contains(input.no))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.Oid.Contains(input.keyword)
                )
          .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
          .Select(it => new ErpProductcheckMasterEntity
          {
              Id = it.Id,
          }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort)
          .ToPagedListAsync(input.currentPage, input.pageSize);

            ids = pageIdList.list.Select(x => x.Id).ToList();
        }

        var sqlQuery = _repository.Context.Queryable<ErpProductcheckMasterEntity>()
            .LeftJoin<ErpProductcheckEntity>((a, b) => a.Id == b.Fid)
            .LeftJoin<ErpProductmodelEntity>((a, b, c) => b.Gid == c.Id)
            .WhereIF(ids != null && ids.Any(), (a) => ids.Contains(a.Id))
            .Select((a, b, c) => new ErpProductcheckInfoExportOutput
            {
                amount = b.Amount,
                auditTime = a.AuditTime,
                batchNumber = b.BatchNumber,
                checkTime =  a.CheckTime.Value.ToString("yyyy-MM-dd"),
                gidName = c.Name,
                loseNum = b.LoseNum,
                no = a.No,
                oidName = SqlFunc.Subqueryable<OrganizeEntity>().Where(ddd => ddd.Id == a.Oid).Select(ddd => ddd.FullName),
                price = b.Price,
                productionDate = b.ProductionDate.Value.ToString("yyyy-MM-dd"),
                productName = SqlFunc.Subqueryable<ErpProductEntity>().Where(ddd => ddd.Id == c.Pid).Select(ddd => ddd.Name),
                productUnit = c.Unit,
                realNum = b.RealNum,
                remark = b.Remark,
                systemNum = b.SystemNum,
                retention = b.Retention,
                storeRomeIdName = SqlFunc.Subqueryable<ErpStoreroomEntity>().Where(ddd => ddd.Id == b.StoreRomeId).Select(ddd => ddd.Name),
                storeRomeAreaIdName = SqlFunc.Subqueryable<ErpStoreareaEntity>().Where(ddd => ddd.Id == b.StoreRomeAreaId).Select(ddd => ddd.Name)
            }); ;

        List<ErpProductcheckInfoExportOutput> list = await sqlQuery.ToListAsync();

        var unitOptions = await _dictionaryDataService.GetList("JLDW");

        list.ForEach(item =>
        {
            item.productUnit = unitOptions.Find(x => x.EnCode == item.productUnit)?.FullName ?? "";
            //if (!string.IsNullOrEmpty(item.diningType))
            //{
            //    var arr = item.diningType.Split(",", true);
            //    item.diningType = string.Join(",", diningTypeOptions.Where(x => arr.Contains(x.EnCode)).Select(x => x.FullName).ToArray());
            //}
            //var admintel = item.admintel.Trim().Replace(" ", "");
            //if (!string.IsNullOrEmpty(item.prefix) && item.prefix.EndsWith(admintel))
            //{
            //    item.prefix = item.prefix.Replace(admintel, "");
            //}
            //else
            //{
            //    item.prefix = "";
            //}
        });

        ExcelConfig excelconfig = ExcelConfig.Default(string.Format("{0:yyyy-MM-dd}_盘点单.xls", DateTime.Now));
        Dictionary<string, string>? FileEncode = Common.Extension.Extensions.GetPropertityToMaps<ErpProductcheckInfoExportOutput>();
        //var selectKey = input.selectKey.Split(',').ToList();
        foreach (KeyValuePair<string, string> item in FileEncode)
        {
            string? column = item.Key;
            string? excelColumn = item.Value;
            excelconfig.ColumnModel!.Add(new ExcelColumnModel() { Column = column, ExcelColumn = excelColumn });
        }

        string? addPath = Path.Combine(FileVariable.TemporaryFilePath, excelconfig.FileName);
        var fs = ExcelExportHelper<ErpProductcheckInfoExportOutput>.ExportMemoryStream(list, excelconfig);
        var flag = await _fileManager.UploadFileByType(fs, FileVariable.TemporaryFilePath, excelconfig.FileName);
        if (flag.Item1)
        {
            fs.Flush();
            fs.Close();
        }

        return new { name = excelconfig.FileName, url = flag.Item2 ?? "/api/file/Download?encryption=" + DESCEncryption.Encrypt(_userManager.UserId + "|" + excelconfig.FileName + "|" + addPath, "QT") };
    }
    #endregion
}