using Mapster;
using QT.Common.Core.Manager.Files;
using QT.Common.Extension;
using QT.JXC.Entitys.Dto.Erp.BuyOrder;
using QT.JXC.Entitys.Dto.Erp.ErpProductcustomerprice;
using QT.Systems.Entitys.Permission;
using System.Data;

namespace QT.JXC;

[ApiDescriptionSettings(ModuleConst.ERP, Tag = "Erp", Name = "ErpProductcustomerprice", Order = 200)]
[Route("api/Erp/[controller]")]
public class ErpProductcustomerpriceService : IDynamicApiController //: QTBaseService<ErpProductpriceEntity, ErpProductpriceCrInput, ErpProductpriceCrInput, ErpProductpriceInfoOutput, PageInputBase, ErpProductpriceInfoOutput>
{
    private readonly ISqlSugarRepository<ErpProductpriceEntity> _repository;
    private readonly ITenant _db;
    private readonly IFileManager _fileManager;
    private readonly IUserManager _userManager;

    public ErpProductcustomerpriceService(ISqlSugarRepository<ErpProductpriceEntity> repository, ISqlSugarClient context, IFileManager fileManager, IUserManager userManager) //: base(repository, context, userManager)
    {
        _repository = repository;
        _db = context.AsTenant();
        _fileManager = fileManager;
        _userManager = userManager;
    }

    /// <summary>
    /// 获取列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] PageInputBase input,string rootTypeId)
    {
        var data = await _repository.Context.Queryable<ErpProductpriceEntity>()
            .LeftJoin<ErpProductmodelEntity>((t, a) => t.Gid == a.Id)
            .LeftJoin<ErpProductEntity>((t, a, b) => a.Pid == b.Id)
            .LeftJoin<ErpCustomerEntity>((t, a, b, c) => t.Cid == c.Id)
            .WhereIF(rootTypeId.IsNotEmptyOrNull(), (t, a, b, c) => SqlFunc.Subqueryable<ViewErpProducttypeEx>().Where(ddd=>ddd.Id == b.Tid && ddd.RootId == rootTypeId).Any())
            .WhereIF(!string.IsNullOrEmpty(input.keyword),(t,a,b,c)=> a.Name.Contains(input.keyword) || b.Name.Contains(input.keyword) 
            || c.Name.Contains(input.keyword))
            //.ClearFilter<ICompanyEntity>()
             .Select<ErpProductpriceInfoOutput>((t, a, b, c) => new ErpProductpriceInfoOutput
             {
                 id = t.Id,
                 gid = t.Gid,
                 price = t.Price,
                 time = t.Time,
                 gidName = a.Name,
                 productName = b.Name,
                 salePrice = a.SalePrice,
                 cid = t.Cid,
                 cName = c.Name,
                 cidOName = SqlFunc.Subqueryable<OrganizeEntity>().Where(it=>it.Id==c.Oid).Select(it=>it.FullName),
                 rootProducttype = SqlFunc.Subqueryable<ViewErpProducttypeEx>().Where(ddd => ddd.Id == b.Tid).Select(ddd => ddd.RootName)
             })
             .ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<ErpProductpriceInfoOutput>.SqlSugarPageResult(data);
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
        List<ErpProductcustomerpriceImportDataInput>? dataList = new List<ErpProductcustomerpriceImportDataInput>() { new ErpProductcustomerpriceImportDataInput() { } };

        ExcelConfig excelconfig = ExcelConfig.Default("用户定价导入模板.xls");
        foreach (KeyValuePair<string, string> item in Common.Extension.Extensions.GetPropertityToMaps<ErpProductcustomerpriceImportDataInput>())
        {
            string? column = item.Key;
            string? excelColumn = item.Value;
            excelconfig.ColumnModel!.Add(new ExcelColumnModel() { Column = column, ExcelColumn = excelColumn });
        }

        string? addPath = Path.Combine(FileVariable.TemporaryFilePath, excelconfig.FileName!);
        ExcelExportHelper<ErpProductcustomerpriceImportDataInput>.Export(dataList, excelconfig, addPath);

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
            Dictionary<string, string>? FileEncode = Common.Extension.Extensions.GetPropertityToMaps<ErpProductcustomerpriceImportDataInput>();

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

            var list = _repository.Context.Utilities.DataTableToList<ErpProductcustomerpriceImportDataInput>(excelData);


            //转化为实体 ErpProductEntity\ErpProductmodelEntity 
            var insertErpCustomerEntities = new List<ErpProductpriceEntity>();
            var updateErpCustomerEntities = new List<ErpProductpriceEntity>();

            ////分类
            //var typeOptions = await _dictionaryDataService.GetList("QTErpCustomType");

            //数据库中的记录
            var dbList = await _repository.Context.Queryable<ErpProductpriceEntity>().ToListAsync();

            //Dictionary<string, List<ErpProductmodelEntity>> kv = new Dictionary<string, List<ErpProductmodelEntity>>();
            //Dictionary<string, ErpProductEntity> kvProduct = new Dictionary<string, ErpProductEntity>();

            //List<UserInCrInput> userList = new List<UserInCrInput>();

            // 获取数据库中的商品规格id
            var productList = await _repository.Context.Queryable<ErpProductmodelEntity>()
                .InnerJoin<ErpProductEntity>((a, b) => a.Pid == b.Id)
                .Select((a, b) => new
                {
                    a.Id,
                    productName = b.Name,
                    gName = a.Name
                }).ToListAsync();

            // 客户集合
            var customerList = await _repository.Context.Queryable<ErpCustomerEntity>().Select(x => new ErpCustomerEntity
            {
                Id = x.Id,
                Name = x.Name
            }).ToListAsync();

            foreach (var item in list)
            {
                if (string.IsNullOrEmpty(item.customerName))
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


                if (string.IsNullOrEmpty(item.price) || !decimal.TryParse(item.price,out var price))
                {
                    throw Oops.Oh("价格有误！");
                }

                var c = customerList.Find(x => x.Name == item.customerName);
                if (c == null)
                {
                    throw Oops.Oh("客户[{0}]，不存在！", item.customerName);
                }

                var p = productList.Find(x => x.productName == item.productName && x.gName == item.gName);
                if (p == null)
                {
                    throw Oops.Oh("商品[{0}]，规格[{1}]，不存在！", item.productName, item.gName);
                }

                var entity = new ErpProductpriceEntity
                {
                    Cid = c.Id,
                    Gid = p.Id,
                    Price = price,
                };

                #region
                if (string.IsNullOrEmpty(item.date))
                {
                    entity.Time = DateTime.Now.Date;
                }else if (DateTime.TryParse(item.date,out var date))
                {
                    entity.Time = date;
                }

                // 判断是否已存在
                var erpEntity = dbList.Find(x => x.Cid == entity.Cid && x.Gid == entity.Gid);
                if (erpEntity != null)
                {
                    _repository.Context.Tracking(erpEntity);

                    erpEntity.Price = entity.Price;
                    erpEntity.Time = entity.Time;

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

            // 开启事务
            _db.BeginTran();

            //更新数据库
            if (insertErpCustomerEntities.Any())
            {
                await _repository.Context.Insertable(insertErpCustomerEntities).ExecuteCommandAsync();
            }
            if (updateErpCustomerEntities.Any())
            {
                await _repository.Context.Updateable(updateErpCustomerEntities).ExecuteCommandAsync();
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
    public async Task<dynamic> ExportExcel([FromQuery] PageInputBase input,int dataType)
    {
        List<string> ids = new List<string>();
        if (dataType == 0)
        {
            var pageIdList = await _repository.Context.Queryable<ErpProductpriceEntity>()
            .LeftJoin<ErpProductmodelEntity>((t, a) => t.Gid == a.Id)
            .LeftJoin<ErpProductEntity>((t, a, b) => a.Pid == b.Id)
            .LeftJoin<ErpCustomerEntity>((t, a, b, c) => t.Cid == c.Id)
            .WhereIF(!string.IsNullOrEmpty(input.keyword), (t, a, b, c) => a.Name.Contains(input.keyword) || b.Name.Contains(input.keyword)
            || c.Name.Contains(input.keyword))
            //.ClearFilter<ICompanyEntity>()
             .Select<ErpProductpriceInfoOutput>((t, a, b, c) => new ErpProductpriceInfoOutput
             {
                 id = t.Id
             })
             .ToPagedListAsync(input.currentPage, input.pageSize);

            ids = pageIdList.list.Select(x => x.id).ToList();
        }

        var sqlQuery = _repository.Context.Queryable<ErpProductpriceEntity>()
            .InnerJoin<ErpProductmodelEntity>((a, b) => a.Gid == b.Id)
            .WhereIF(ids != null && ids.Any(), (a) => ids.Contains(a.Id))
            .Select((a, b) => new ErpProductcustomerpriceImportDataInput
            {
                customerName = SqlFunc.Subqueryable<ErpCustomerEntity>().Where(ddd => ddd.Id == a.Cid).Select(ddd => ddd.Name),
                date = a.Time.Value.ToString("yyyy-MM-dd"),
                price = SqlFunc.ToString(a.Price),
                gName = b.Name,
                productName = SqlFunc.Subqueryable<ErpProductEntity>().Where(x => x.Id == b.Pid).Select(x => x.Name)
            });

        List<ErpProductcustomerpriceImportDataInput> list = await sqlQuery.ToListAsync();

        //var typeOptions = await _dictionaryDataService.GetList("QTErpCustomType");
        ////var diningTypeOptions = await _dictionaryDataService.GetList("ErpOrderDiningType");

        //list.ForEach(item =>
        //{
        //    item.customerType = typeOptions.Find(x => x.EnCode == item.customerType)?.FullName ?? "";
        //});

        ExcelConfig excelconfig = ExcelConfig.Default(string.Format("{0:yyyy-MM-dd}_用户定价.xls", DateTime.Now));
        Dictionary<string, string>? FileEncode = Common.Extension.Extensions.GetPropertityToMaps<ErpProductcustomerpriceImportDataInput>();
        foreach (KeyValuePair<string, string> item in FileEncode)
        {
            string? column = item.Key;
            string? excelColumn = item.Value;
            excelconfig.ColumnModel!.Add(new ExcelColumnModel() { Column = column, ExcelColumn = excelColumn });
        }

        string? addPath = Path.Combine(FileVariable.TemporaryFilePath, excelconfig.FileName);
        var fs = ExcelExportHelper<ErpProductcustomerpriceImportDataInput>.ExportMemoryStream(list, excelconfig);
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
    /// 批量删除.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("Batch")]
    public async Task BatchDelete([FromBody] List<string> input)
    {
        if (input.IsAny())
        {
            var isOk = await _repository.Context.Deleteable<ErpProductpriceEntity>().Where(it => input.Contains(it.Id)).ExecuteCommandAsync();
            if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
        }
    }

    /// <summary>
    /// 更新采购任务订单质检报告和自检报告
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpGet("Proof/{id}")]
    public async Task<ErpBuyorderCkUpProofInput> GetDetailProof(string id)
    {
        var entity = await _repository.Context.Queryable<ErpProductpriceEntity>().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);

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
        var entity = input.Adapt<ErpProductpriceEntity>();

        await _repository.Context.Updateable<ErpProductpriceEntity>(entity).UpdateColumns(x => new { x.QualityReportProof }).ExecuteCommandAsync();
    }
}
