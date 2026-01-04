using Microsoft.AspNetCore.Http;
using QT.Application.Entitys.Dto.FreshDelivery.ErpBuyorder;
using QT.Application.Entitys.Dto.FreshDelivery.ErpProductcustomertypeprice;
using QT.Application.Entitys.FreshDelivery;
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
using QT.Logging.Attributes;
using QT.Systems.Interfaces.System;
using System.Data;

namespace QT.Application.FreshDelivery;

/// <summary>
/// 业务实现：商品客户类型定价.
/// </summary>
[ApiDescriptionSettings("生鲜配送", Tag = "客户类型定价", Name = "ErpProductcustomertypeprice", Order = 200)]
[Route("api/apply/FreshDelivery/[controller]")]
public class ErpProductcustomertypepriceService : IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<ErpProductcustomertypepriceEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;
    private readonly IFileManager _fileManager;
    private readonly IDictionaryDataService _dictionaryDataService;
    private readonly ITenant _db;

    /// <summary>
    /// 初始化一个<see cref="ErpProductcustomertypepriceService"/>类型的新实例.
    /// </summary>
    public ErpProductcustomertypepriceService(
        ISqlSugarRepository<ErpProductcustomertypepriceEntity> erpProductcustomertypepriceRepository,
        ISqlSugarClient context,
        IUserManager userManager, IFileManager fileManager,
        IDictionaryDataService dictionaryDataService)
    {
        _repository = erpProductcustomertypepriceRepository;
        _userManager = userManager;
        _fileManager = fileManager;
        _dictionaryDataService = dictionaryDataService;
        _db = context.AsTenant();
    }

    /// <summary>
    /// 获取商品客户类型定价.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        return await _repository.AsQueryable()
            .LeftJoin<ErpProductmodelEntity>((it, a) => it.Gid == a.Id)
            .LeftJoin<ErpProductEntity>((it, a, b) => a.Pid == b.Id)
            .Where(it => it.Id == id)
            .Select((it, a, b) => new ErpProductcustomertypepriceInfoOutput
            {
                gidName = a.Name,
                productName = b.Name
            }, true)
            .FirstAsync();
        //return (await ).Adapt<ErpProductcustomertypepriceInfoOutput>();
    }

    /// <summary>
    /// 获取商品客户类型定价列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] ErpProductcustomertypepriceListQueryInput input)
    {
        var data = await _repository.Context.Queryable<ErpProductcustomertypepriceEntity>()
            .LeftJoin<ErpProductmodelEntity>((it, a) => it.Gid == a.Id)
            .LeftJoin<ErpProductEntity>((it, a, b) => a.Pid == b.Id)
            .WhereIF(!string.IsNullOrEmpty(input.gid), it => it.Gid == input.gid)
            .WhereIF(!string.IsNullOrEmpty(input.productName), (it, a, b) => b.Name.Contains(input.productName))
            .WhereIF(!string.IsNullOrEmpty(input.tid), it => it.Tid.Equals(input.tid))
            .WhereIF(input.rootTypeId.IsNotEmptyOrNull(), (it, a, b) => SqlFunc.Subqueryable<ViewErpProducttypeEx>().Where(ddd => ddd.Id == b.Tid && ddd.RootId == input.rootTypeId).Any())
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.Gid.Contains(input.keyword)
                || it.Tid.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select((it,a,b) => new ErpProductcustomertypepriceListOutput
            {
                id = it.Id,
                gid = it.Gid,
                tid = it.Tid,
                discount = it.Discount,
                price = it.Price,
                gidName = a.Name,
                productName = b.Name,
                pricingType = it.PricingType,
                unit = a.Unit,
                rootProducttype = SqlFunc.Subqueryable<ViewErpProducttypeEx>().Where(ddd => ddd.Id == b.Tid).Select(ddd => ddd.RootName),
                hasQualityReport = SqlFunc.IsNullOrEmpty(it.QualityReportProof) ? 0 : 1,
            },true).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<ErpProductcustomertypepriceListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建商品客户类型定价.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    [OperateLog("客户类型定价", "新增")]
    public async Task Create([FromBody] ErpProductcustomertypepriceCrInput input)
    {
        var entity = input.Adapt<ErpProductcustomertypepriceEntity>();

        if (await _repository.AnyAsync(it => it.Tid == entity.Tid && it.Gid == entity.Gid))
        {
            throw Oops.Oh(ErrorCode.COM1004);
        }

        entity.Id = SnowflakeIdHelper.NextId();
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).EnableDiffLogEvent($"{nameof(ErpProductcustomertypepriceEntity)}:{entity.Id}").ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新商品客户类型定价.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [OperateLog("客户类型定价", "修改")]
    public async Task Update(string id, [FromBody] ErpProductcustomertypepriceUpInput input)
    {
        var entity = input.Adapt<ErpProductcustomertypepriceEntity>();
        if (await _repository.AnyAsync(it => it.Tid == entity.Tid && it.Gid == entity.Gid && it.Id!= entity.Id))
        {
            throw Oops.Oh(ErrorCode.COM1004);
        }

        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).EnableDiffLogEvent($"{nameof(ErpProductcustomertypepriceEntity)}:{entity.Id}").ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除商品客户类型定价.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [OperateLog("客户类型定价", "删除")]
    public async Task Delete(string id)
    {
        var isOk = await _repository.Context.Deleteable<ErpProductcustomertypepriceEntity>().Where(it => it.Id.Equals(id)).EnableDiffLogEvent($"{nameof(ErpProductcustomertypepriceEntity)}:{id}").ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
    }

    /// <summary>
    /// 批量删除.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("Batch")]
    public async Task BatchDelete([FromBody] List<string> input)
    {
        if (input.IsAny())
        {
            var isOk = await _repository.Context.Deleteable<ErpProductcustomertypepriceEntity>().Where(it => input.Contains(it.Id)).EnableDiffLogEvent().ExecuteCommandAsync();
            if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
        }
    }


    #region 导入
    /// <summary>
    /// 模板下载.
    /// </summary>
    /// <returns></returns>
    [HttpGet("TemplateDownload")]
    public dynamic TemplateDownload()
    {
        // 初始化 一条空数据 
        var dataList = new List<ErpProductcustomertypepriceImportDataInput>() { new ErpProductcustomertypepriceImportDataInput() { } };

        ExcelConfig excelconfig = ExcelConfig.Default("客户类型定价导入模板.xls");
        foreach (KeyValuePair<string, string> item in Common.Extension.Extensions.GetPropertityToMaps<ErpProductcustomertypepriceImportDataInput>())
        {
            string? column = item.Key;
            string? excelColumn = item.Value;
            excelconfig.ColumnModel!.Add(new ExcelColumnModel() { Column = column, ExcelColumn = excelColumn });
        }

        string? addPath = Path.Combine(FileVariable.TemporaryFilePath, excelconfig.FileName!);
        ExcelExportHelper<ErpProductcustomertypepriceImportDataInput>.Export(dataList, excelconfig, addPath);

        return new { name = excelconfig.FileName, url = "/api/file/Download?encryption=" + DESCEncryption.Encrypt(_userManager.UserId + "|" + excelconfig.FileName + "|" + addPath, "QT") };
    }
    /// <summary>
    /// 导入数据.
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    [HttpPost("ImportData")]
    public async Task<dynamic> ImportData(IFormFile file)
    {
        var _filePath = _fileManager.GetPathByType(string.Empty);
        var _fileName = DateTime.Now.ToString("yyyyMMdd") + "_" + SnowflakeIdHelper.NextId() + Path.GetExtension(file.FileName);
        var stream = file.OpenReadStream();
        await _fileManager.UploadFileByType(stream, _filePath, _fileName);

        try
        {
            Dictionary<string, string>? FileEncode = Common.Extension.Extensions.GetPropertityToMaps<ErpProductcustomertypepriceImportDataInput>();

            string? filePath = FileVariable.TemporaryFilePath;
            string? savePath = Path.Combine(filePath, _fileName);

            // 得到数据
            var excelData = ExcelImportHelper.ToDataTable(file.OpenReadStream(), ExcelImportHelper.IsXls(_fileName));
            if (excelData != null)
            {
                foreach (DataColumn item in excelData.Columns)
                {
                    var key = FileEncode.Where(x => x.Value == item.ToString()).FirstOrDefault().Key;
                    if (!string.IsNullOrEmpty(key))
                    {
                        item.ColumnName = key;
                        //excelData.Columns[item.ToString()].ColumnName = ;
                    }
                }

            }

            var list = _repository.Context.Utilities.DataTableToList<ErpProductcustomertypepriceImportDataInput>(excelData);


            //转化为实体 ErpProductEntity\ErpProductmodelEntity 
            var insertErpCustomerEntities = new List<ErpProductcustomertypepriceEntity>();
            var updateErpCustomerEntities = new List<ErpProductcustomertypepriceEntity>();

            //分类
            var typeOptions = await _dictionaryDataService.GetList("QTErpCustomType");
 
            //数据库中的记录
            var dbList = await _repository.Context.Queryable<ErpProductcustomertypepriceEntity>().ToListAsync();

            // 获取数据库中的商品规格id
            var productList = await _repository.Context.Queryable<ErpProductmodelEntity>()
                .InnerJoin<ErpProductEntity>((a, b) => a.Pid == b.Id)
                .Select((a, b) => new
                {
                    a.Id,
                    productName = b.Name,
                    gName = a.Name
                }).ToListAsync();


            foreach (var item in list)
            {
                if (string.IsNullOrEmpty(item.customerType))
                {
                    throw Oops.Oh("客户名称不能为空！");
                }

                if (string.IsNullOrEmpty(item.productName))
                {
                    throw Oops.Oh("商品名称不能为空！");
                }
                if (string.IsNullOrEmpty(item.gName))
                {
                    throw Oops.Oh("商品规格不能为空！");
                }
                if (string.IsNullOrEmpty(item.pricingType))
                {
                    throw Oops.Oh("计价类型不能为空！");
                }


                decimal price=0, discount = 0;
                if (!string.IsNullOrEmpty(item.price) && !decimal.TryParse(item.price, out price))
                {
                    throw Oops.Oh("价格有误！");
                }

                if (!string.IsNullOrEmpty(item.discount) && !decimal.TryParse(item.discount, out discount))
                {
                    throw Oops.Oh("折扣有误！");
                }

                var c = typeOptions.Find(x => x.FullName == item.customerType);
                if (c == null)
                {
                    throw Oops.Oh("客户类型[{0}]，不存在！", item.customerType);
                }

 

                var p = productList.Find(x => x.productName == item.productName && x.gName == item.gName);
                if (p == null)
                {
                    throw Oops.Oh("商品[{0}]，规格[{1}]，不存在！", item.productName, item.gName);
                }

                var entity = new ErpProductcustomertypepriceEntity
                {
                    Discount = discount,
                    Gid = p.Id,
                    Price = price,
                    Tid = c.EnCode,
                    PricingType = item.pricingType == "按价格" ? 2 : 1,
                };

                #region
               

                // 判断是否已存在
                var erpEntity = dbList.Find(x => x.Tid == entity.Tid && x.Gid == entity.Gid);
                if (erpEntity != null)
                {
                    _repository.Context.Tracking(erpEntity);

                    erpEntity.Price = entity.Price;
                    erpEntity.Discount = entity.Discount;
                    erpEntity.PricingType = entity.PricingType;

                    updateErpCustomerEntities.Add(erpEntity);
                }
                else
                {
                    erpEntity = entity;
                    erpEntity.Id = SnowflakeIdHelper.NextId();
                    insertErpCustomerEntities.Add(erpEntity);
                }

                #endregion
            }

            if (insertErpCustomerEntities.Any() && insertErpCustomerEntities.GroupBy(x => new { x.Tid, x.Gid })
                    .Where(x => x.Count() > 1).Any())
            {
                throw Oops.Oh("请检查导入文档，规格+客户类型需要唯一！");
            }

            // 开启事务
            _db.BeginTran();

            //更新数据库
            if (insertErpCustomerEntities.Any())
            {
                await _repository.Context.Insertable(insertErpCustomerEntities).EnableDiffLogEvent().ExecuteCommandAsync();
            }
            if (updateErpCustomerEntities.Any())
            {
                await _repository.Context.Updateable(updateErpCustomerEntities).EnableDiffLogEvent().ExecuteCommandAsync();
            }

            // 关闭事务
            _db.CommitTran();

            return new { np = insertErpCustomerEntities.Count, up = updateErpCustomerEntities.Count };
        }
        catch (Exception e)
        {
            // 回滚事务
            _db.RollbackTran();
            if (e is AppFriendlyException ex)
            {
                throw ex;
            }

            throw Oops.Oh(ErrorCode.D1805);
        }
    }
    #endregion

    #region 导出
    /// <summary>
    /// 导出Excel.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("ExportExcel")]
    public async Task<dynamic> ExportExcel([FromQuery] ErpProductcustomertypepriceListQueryInput input)
    {
        List<string> ids = new List<string>();
        if (input.dataType == 0)
        {
            var pageIdList = await _repository.Context.Queryable<ErpProductcustomertypepriceEntity>()
            .LeftJoin<ErpProductmodelEntity>((it, a) => it.Gid == a.Id)
            .LeftJoin<ErpProductEntity>((it, a, b) => a.Pid == b.Id)
            .WhereIF(!string.IsNullOrEmpty(input.gid), it => it.Gid == input.gid)
            .WhereIF(!string.IsNullOrEmpty(input.productName), (it, a, b) => b.Name.Contains(input.productName))
            .WhereIF(!string.IsNullOrEmpty(input.tid), it => it.Tid.Equals(input.tid))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.Gid.Contains(input.keyword)
                || it.Tid.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select((it, a, b) => new ErpProductcustomertypepriceListOutput
            {
                id = it.Id
            }, true).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize); ;

            ids = pageIdList.list.Select(x => x.id).ToList();
        }

        var sqlQuery = _repository.Context.Queryable<ErpProductcustomertypepriceEntity>()
            .InnerJoin<ErpProductmodelEntity>((a, b) => a.Gid == b.Id)
            .LeftJoin<ErpProductEntity>((a, b,c) => b.Pid == c.Id)
            .WhereIF(ids != null && ids.Any(), (a) => ids.Contains(a.Id))
            .Select((a, b,c) => new ErpProductcustomertypepriceImportDataInput
            {
                customerType = a.Tid,
                discount = SqlFunc.ToString(a.Discount),
                price = SqlFunc.ToString(a.Price),
                pricingType = a.PricingType == 1 ? "按折扣" : "按价格",
                gName = b.Name,
                productName = SqlFunc.Subqueryable<ErpProductEntity>().Where(x => x.Id == b.Pid).Select(x => x.Name),
                rootProducttype = SqlFunc.Subqueryable<ViewErpProducttypeEx>().Where(ddd => ddd.Id == c.Tid).Select(ddd => ddd.RootName)
            });

        List<ErpProductcustomertypepriceImportDataInput> list = await sqlQuery.ToListAsync();

        var typeOptions = await _dictionaryDataService.GetList("QTErpCustomType");
        //var diningTypeOptions = await _dictionaryDataService.GetList("ErpOrderDiningType");

        list.ForEach(item =>
        {
            item.customerType = typeOptions.Find(x => x.EnCode == item.customerType)?.FullName ?? "";
        });

        ExcelConfig excelconfig = ExcelConfig.Default(string.Format("{0:yyyy-MM-dd}_客户类型定价.xls", DateTime.Now));
        Dictionary<string, string>? FileEncode = Common.Extension.Extensions.GetPropertityToMaps<ErpProductcustomertypepriceImportDataInput>();
        foreach (KeyValuePair<string, string> item in FileEncode)
        {
            string? column = item.Key;
            string? excelColumn = item.Value;
            excelconfig.ColumnModel!.Add(new ExcelColumnModel() { Column = column, ExcelColumn = excelColumn });
        }

        string? addPath = Path.Combine(FileVariable.TemporaryFilePath, excelconfig.FileName);
        var fs = ExcelExportHelper<ErpProductcustomertypepriceImportDataInput>.ExportMemoryStream(list, excelconfig);
        var flag = await _fileManager.UploadFileByType(fs, FileVariable.TemporaryFilePath, excelconfig.FileName);
        if (flag.Item1)
        {
            fs.Flush();
            fs.Close();
        }

        return new { name = excelconfig.FileName, url = flag.Item2 ?? "/api/file/Download?encryption=" + DESCEncryption.Encrypt(_userManager.UserId + "|" + excelconfig.FileName + "|" + addPath, "QT") };
    }
    #endregion

    /// <summary>
    /// 更新采购任务订单质检报告和自检报告
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpGet("Proof/{id}")]
    public async Task<ErpBuyorderCkUpProofInput> GetDetailProof(string id)
    {
        var entity = await _repository.Context.Queryable<ErpProductcustomertypepriceEntity>().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);

        return entity.Adapt<ErpBuyorderCkUpProofInput>();
    }

    /// <summary>
    /// 更新采购任务订单质检报告和自检报告
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("Proof/{id}")]
    public async Task DetailProof(string id, [FromBody] ErpBuyorderCkUpProofInput input)
    {
        var entity = input.Adapt<ErpProductcustomertypepriceEntity>();

        await _repository.Context.Updateable<ErpProductcustomertypepriceEntity>(entity).UpdateColumns(x => new { x.QualityReportProof }).ExecuteCommandAsync();
    }
}