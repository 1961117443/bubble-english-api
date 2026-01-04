using Microsoft.AspNetCore.Http;
using QT.Application.Entitys.Dto.FreshDelivery.ErpSupplier;
using QT.Application.Entitys.FreshDelivery;
using QT.ClayObject;
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
using QT.Systems.Interfaces.System;

namespace QT.Application.FreshDelivery;

/// <summary>
/// 业务实现：供货商信息.
/// </summary>
[ApiDescriptionSettings("生鲜配送", Tag = "供货商信息", Name = "ErpSupplier", Order = 200)]
[Route("api/apply/FreshDelivery/[controller]")]
public class ErpSupplierService : IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<ErpSupplierEntity> _repository;

    /// <summary>
    /// 多租户事务.
    /// </summary>
    private readonly ITenant _db;
    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;
    private readonly IFileManager _fileManager;
    private readonly IDictionaryDataService _dictionaryDataService;

    /// <summary>
    /// 初始化一个<see cref="ErpSupplierService"/>类型的新实例.
    /// </summary>
    public ErpSupplierService(
        ISqlSugarRepository<ErpSupplierEntity> erpSupplierRepository,
        ISqlSugarClient context,
        IUserManager userManager,
        IFileManager fileManager,
        IDictionaryDataService dictionaryDataService)
    {
        _repository = erpSupplierRepository;
        _userManager = userManager;
        _fileManager = fileManager;
        _dictionaryDataService = dictionaryDataService;
        _db = context.AsTenant();
    }

    /// <summary>
    /// 获取供货商信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        return (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<ErpSupplierInfoOutput>();
    }

    /// <summary>
    /// 获取供货商信息列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] ErpSupplierListQueryInput input)
    {
        var data = await _repository.Context.Queryable<ErpSupplierEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.name), it => it.Name.Contains(input.name))
            .WhereIF(!string.IsNullOrEmpty(input.admin), it => it.Admin.Contains(input.admin))
            .WhereIF(!string.IsNullOrEmpty(input.adminTel), it => it.AdminTel.Contains(input.adminTel))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.Name.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new ErpSupplierListOutput
            {
                id = it.Id,
                name = it.Name,
                address = it.Address,
                admin = it.Admin,
                adminTel = it.AdminTel,
                joinTime = it.JoinTime,
                workCycle = it.WorkCycle,
                workTime = it.WorkTime,
                salesman = it.Salesman,
                businessLicense = it.BusinessLicense,
                productionLicense = it.ProductionLicense,
                foodBusinessLicense = it.FoodBusinessLicense
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<ErpSupplierListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建供货商信息.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] ErpSupplierCrInput input)
    {
        var entity = input.Adapt<ErpSupplierEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        entity.FirstChar = PinyinHelper.PinyinString(entity.Name);
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 获取供货商信息无分页列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    private async Task<dynamic> GetNoPagingList([FromQuery] ErpSupplierListQueryInput input)
    {
        return await _repository.Context.Queryable<ErpSupplierEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.name), it => it.Name.Contains(input.name))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.Name.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new ErpSupplierListOutput
            {
                id = it.Id,
                name = it.Name,
                address = it.Address,
                admin = it.Admin,
                adminTel = it.AdminTel,
                joinTime = it.JoinTime,
                workCycle = it.WorkCycle,
                workTime = it.WorkTime,
                salesman = it.Salesman,
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToListAsync();
    }

    /// <summary>
    /// 导出供货商信息.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("Actions/Export")]
    public async Task<dynamic> Export([FromQuery] ErpSupplierListQueryInput input)
    {
        var exportData = new List<ErpSupplierListOutput>();
        if (input.dataType == 0)
            exportData = Clay.Object(await GetList(input)).Solidify<PageResult<ErpSupplierListOutput>>().list;
        else
            exportData = await GetNoPagingList(input);
        List<ParamsModel> paramList = "[{\"value\":\"名称\",\"field\":\"name\"},{\"value\":\"地址\",\"field\":\"address\"},{\"value\":\"负责人\",\"field\":\"admin\"},{\"value\":\"负责人电话\",\"field\":\"adminTel\"},{\"value\":\"入驻时间\",\"field\":\"joinTime\"},{\"value\":\"经营周期\",\"field\":\"workCycle\"},{\"value\":\"经营时间\",\"field\":\"workTime\"},{\"value\":\"业务人员\",\"field\":\"salesman\"},]".ToList<ParamsModel>();
        ExcelConfig excelconfig = new ExcelConfig();
        excelconfig.FileName = "供货商信息.xls";
        excelconfig.HeadFont = "微软雅黑";
        excelconfig.HeadPoint = 10;
        excelconfig.IsAllSizeColumn = true;
        excelconfig.ColumnModel = new List<ExcelColumnModel>();
        foreach (var item in input.selectKey.Split(',').ToList())
        {
            var isExist = paramList.Find(p => p.field == item);
            if (isExist != null)
                excelconfig.ColumnModel.Add(new ExcelColumnModel() { Column = isExist.field, ExcelColumn = isExist.value });
        }

        var addPath = FileVariable.TemporaryFilePath + excelconfig.FileName;
        ExcelExportHelper<ErpSupplierListOutput>.Export(exportData, excelconfig, addPath);
        var fileName = _userManager.UserId + "|" + addPath + "|xls";
        return new
        {
            name = excelconfig.FileName,
            url = "/api/File/Download?encryption=" + DESCEncryption.Encrypt(fileName, "QT")
        };
    }

    /// <summary>
    /// 更新供货商信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] ErpSupplierUpInput input)
    {
        var entity = input.Adapt<ErpSupplierEntity>();
        entity.FirstChar = PinyinHelper.PinyinString(entity.Name);
        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除供货商信息.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var isOk = await _repository.Context.Deleteable<ErpSupplierEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
    }

    /// <summary>
    /// 获取下拉框.
    /// </summary>
    /// <returns></returns>
    [HttpGet("Selector")]
    public async Task<dynamic> GetSelector([FromQuery]bool all)
    {
        //List<ErpSupplierEntity>? data = await _repository.Entities.WithCache().ToListAsync();

        //return new { list = data.Select(x => new { id = x.Id, name = x.Name,oid= x.Oid /*string.IsNullOrEmpty(x.FirstChar) ? x.Name : $"[{x.FirstChar}]{x.Name}"*/ }).ToList() };

        var qur = all ? _repository.AsQueryable().ClearFilter<ICompanyEntity>() : _repository.AsQueryable();
        var data = await qur.Select(x => new { id = x.Id, name = x.Name, oid = x.Oid }).ToListAsync();

        return new { list = data };
    }



    #region 导入导出
    /// <summary>
    /// 模板下载.
    /// </summary>
    /// <returns></returns>
    [HttpGet("TemplateDownload")]
    public dynamic TemplateDownload()
    {
        // 初始化 一条空数据 
        var dataList = new List<ErpSupplierListImportDataInput>() { new ErpSupplierListImportDataInput() { } };

        ExcelConfig excelconfig = ExcelConfig.Default("供应商信息导入模板.xls");
        foreach (KeyValuePair<string, string> item in Common.Extension.Extensions.GetPropertityToMaps<ErpSupplierListImportDataInput>())
        {
            string? column = item.Key;
            string? excelColumn = item.Value;
            excelconfig.ColumnModel!.Add(new ExcelColumnModel() { Column = column, ExcelColumn = excelColumn });
        }

        string? addPath = Path.Combine(FileVariable.TemporaryFilePath, excelconfig.FileName!);
        ExcelExportHelper<ErpSupplierListImportDataInput>.Export(dataList, excelconfig, addPath);

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
            Dictionary<string, string>? FileEncode = Common.Extension.Extensions.GetPropertityToMaps<ErpSupplierListImportDataInput>();

            string? filePath = FileVariable.TemporaryFilePath;
            string? savePath = Path.Combine(filePath, _fileName);

            // 得到数据
            var excelData = ExcelImportHelper.ToDataTable(file.OpenReadStream(), ExcelImportHelper.IsXls(_fileName));
            if (excelData != null)
            {
                foreach (global::System.Data.DataColumn item in excelData.Columns)
                {
                    var key = FileEncode.Where(x => x.Value == item.ToString()).FirstOrDefault().Key;
                    if (!string.IsNullOrEmpty(key))
                    {
                        item.ColumnName = key;
                        //excelData.Columns[item.ToString()].ColumnName = ;
                    }
                }

            }

            var list = _repository.Context.Utilities.DataTableToList<ErpSupplierListImportDataInput>(excelData);

            //转化为实体 ErpProductEntity\ErpProductmodelEntity 
            var insertErpEntities = new List<ErpSupplierEntity>();
            var updateErpEntities = new List<ErpSupplierEntity>();

            //分类
            var typeOptions = await _dictionaryDataService.GetList("QTErpCustomType");

            //数据库中的记录
            var dbErpSupplierEntities = await _repository.Context.Queryable<ErpSupplierEntity>().ToListAsync();

            //Dictionary<string, List<ErpProductmodelEntity>> kv = new Dictionary<string, List<ErpProductmodelEntity>>();
            //Dictionary<string, ErpProductEntity> kvProduct = new Dictionary<string, ErpProductEntity>();

            foreach (var item in list)
            {
                if (string.IsNullOrEmpty(item.name))
                {
                    throw Oops.Oh("供应商名称不能为空！");
                }

                // 判断是否已存在
                var erpEntity = dbErpSupplierEntities.Find(x => x.Name == item.name);
                if (erpEntity != null)
                {
                    _repository.Context.Tracking(erpEntity);

                    item.Adapt(erpEntity);

                    updateErpEntities.Add(erpEntity);
                }
                else
                {
                    erpEntity = item.Adapt<ErpSupplierEntity>();
                    erpEntity.Id = SnowflakeIdHelper.NextId();
                    insertErpEntities.Add(erpEntity);
                }
            }

            // 开启事务
            _db.BeginTran();

            //更新数据库
            if (insertErpEntities.Any())
            {
                await _repository.Context.Insertable(insertErpEntities).ExecuteCommandAsync();
            }
            if (updateErpEntities.Any())
            {
                await _repository.Context.Updateable(updateErpEntities).ExecuteCommandAsync();
            }

            // 关闭事务
            _db.CommitTran();

            return new { np = insertErpEntities.Count, up = updateErpEntities.Count };
        }
        catch (Exception e)
        {
            // 回滚事务
            _db.RollbackTran();
            throw Oops.Oh(e.Message);
            //throw Oops.Oh(ErrorCode.D1805);
        }
    }

    /// <summary>
    /// 导出Excel.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("ExportExcel")]
    public async Task<dynamic> ExportExcel([FromQuery] ErpSupplierListQueryInput input)
    {
        List<string> ids = new List<string>();
        if (input.dataType == 0)
        {
            var pageIdList = await _repository.Context.Queryable<ErpSupplierEntity>()
          .WhereIF(!string.IsNullOrEmpty(input.name), it => it.Name.Contains(input.name))
          .WhereIF(!string.IsNullOrEmpty(input.keyword), it => it.Name.Contains(input.keyword))
          .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
          .Select(it => new ErpSupplierListOutput
          {
              id = it.Id,
          }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort)
          .ToPagedListAsync(input.currentPage, input.pageSize);

            ids = pageIdList.list.Select(x => x.id).ToList();
        }

        var sqlQuery = _repository.Context.Queryable<ErpSupplierEntity>()
            .WhereIF(ids != null && ids.Any(), (a) => ids.Contains(a.Id))
            .Select((a) => new ErpSupplierListImportDataInput
            {
                name = a.Name,
                address = a.Address,
                admin = a.Admin,
                admintel = a.AdminTel,
                joinTime = a.JoinTime.Value.ToString("yyyy-MM-dd"),
                salesman = a.Salesman,
                workCycle = a.WorkCycle,
                workTime = a.WorkTime,
            });

        var list = await sqlQuery.ToListAsync();

        ExcelConfig excelconfig = ExcelConfig.Default(string.Format("{0:yyyy-MM-dd}_供应商信息.xls", DateTime.Now));
        Dictionary<string, string>? FileEncode = Common.Extension.Extensions.GetPropertityToMaps<ErpSupplierListImportDataInput>();
        //var selectKey = input.selectKey.Split(',').ToList();
        foreach (KeyValuePair<string, string> item in FileEncode)
        {
            string? column = item.Key;
            string? excelColumn = item.Value;
            excelconfig.ColumnModel!.Add(new ExcelColumnModel() { Column = column, ExcelColumn = excelColumn });
        }

        string? addPath = Path.Combine(FileVariable.TemporaryFilePath, excelconfig.FileName);
        var fs = ExcelExportHelper<ErpSupplierListImportDataInput>.ExportMemoryStream(list, excelconfig);
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