using QT.Common;
using QT.Common.Core.Manager.Files;
using QT.Common.Extension;
using QT.DependencyInjection;
using QT.JXC.Entitys.Dto.Erp.BuyOrder;
using QT.JXC.Entitys.Dto.ErpOutorderDb;
using QT.JXC.Interfaces;
using QT.Reflection.Extensions;
using QT.Systems.Entitys.Permission;
using QT.Systems.Interfaces.System;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace QT.JXC;

/// <summary>
/// 业务实现：出库订单表.
/// </summary>
[ApiDescriptionSettings(ModuleConst.ERP, Tag = "Erp", Name = "ErpOutorderDb", Order = 200)]
[Route("api/Erp/ErpOutorder/Db")]
public class ErpOutorderDbService : IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<ErpOutorderEntity> _repository;

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
    private readonly IFileManager _fileManager;
    private readonly IDictionaryDataService _dictionaryDataService;
    private readonly IErpInorderService _erpInorderService;
    private readonly IErpOutorderService _erpOutorderService;

    /// <summary>
    /// 初始化一个<see cref="ErpOutorderService"/>类型的新实例.
    /// </summary>
    public ErpOutorderDbService(
        ISqlSugarRepository<ErpOutorderEntity> erpOutorderRepository,
        IBillRullService billRullService,
        ISqlSugarClient context,
        IUserManager userManager,
        IFileManager fileManager, IDictionaryDataService dictionaryDataService, IErpInorderService erpInorderService, IErpOutorderService erpOutorderService)
    {
        _repository = erpOutorderRepository;
        _billRullService = billRullService;
        _db = context.AsTenant();
        _userManager = userManager;
        _fileManager = fileManager;
        _dictionaryDataService = dictionaryDataService;
        _erpInorderService = erpInorderService;
        _erpOutorderService = erpOutorderService;
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
        List<ErpOutorderDbImportData>? dataList = new List<ErpOutorderDbImportData>() { new ErpOutorderDbImportData() { } };

        ExcelConfig excelconfig = ExcelConfig.Default("调拨出库导入模板.xls");
        foreach (KeyValuePair<string, string> item in Common.Extension.Extensions.GetPropertityToMaps<ErpOutorderDbImportData>())
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

        var flag = await _fileManager.UploadFileByType(ExcelExportHelper<ErpOutorderDbImportData>.ExportMemoryStream(dataList, excelconfig), FileVariable.TemporaryFilePath, excelconfig.FileName);


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
            Dictionary<string, string>? FileEncode = Common.Extension.Extensions.GetPropertityToMaps<ErpOutorderDbImportData>();

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
    [SqlSugarUnitOfWork]
    public async Task<dynamic> ImportData([FromBody] ErpOutorderDbImportDataInput list)
    {
        object[]? res = await ImportOrderData(list.list);
        List<ErpOutorderCrInput>? addlist = res.First() as List<ErpOutorderCrInput>;
        List<ErpOutorderDbImportData>? errorlist = res.Last() as List<ErpOutorderDbImportData>;
        var output = new ErpOutorderDbImportResultOutput()
        {
            snum = addlist.Count,
            fnum = errorlist.Count,
            //failResult = errorlist,
            resultType = errorlist.Count < 1 ? 0 : 1
        };
        output.failResult = errorlist;

        if (errorlist.IsAny())
        {
            ExcelConfig excelconfig = ExcelConfig.Default($"{DateTime.Now.ToString("yyyyMMddHHmmss")}_调拨出库导入错误报告.xls");
            foreach (KeyValuePair<string, string> item in Common.Extension.Extensions.GetPropertityToMaps<ErpOutorderDbImportErrorData>())
            {
                string? column = item.Key;
                string? excelColumn = item.Value;
                excelconfig.ColumnModel.Add(new ExcelColumnModel() { Column = column, ExcelColumn = excelColumn });
            }

            string? addPath = Path.Combine(FileVariable.TemporaryFilePath, excelconfig.FileName);
            //ExcelExportHelper<ErpOrderListImportDataInput>.Export(errorlist, excelconfig, addPath);

            var flag = await _fileManager.UploadFileByType(ExcelExportHelper<ErpOutorderDbImportData>.ExportMemoryStream(errorlist, excelconfig), FileVariable.TemporaryFilePath, excelconfig.FileName);

            //output.failFileUrl = new { name = excelconfig.FileName, url = flag.Item2 ?? "/api/file/Download?encryption=" + DESCEncryption.Encrypt(_userManager.UserId + "|" + excelconfig.FileName + "|" + addPath + "|" + _userManager.TenantId, "QT") };

            output.failFileUrl = flag.Item2 ?? "/api/file/Download?encryption=" + DESCEncryption.Encrypt(_userManager.UserId + "|" + excelconfig.FileName + "|" + addPath + "|" + _userManager.TenantId, "QT");
        }
        return output;
    }

    ///// <summary>
    ///// 04.导出错误报告.
    ///// </summary>
    ///// <param name="list"></param>
    ///// <returns></returns>
    //[HttpPost("ExportExceptionData")]
    //public async Task<dynamic> ExportExceptionData([FromBody] ErpOutorderDbImportDataInput list)
    //{
    //    object[]? res = await ImportOrderData(list.list);

    //    // 错误数据
    //    List<ErpOutorderDbImportData>? errorlist = res.Last() as List<ErpOutorderDbImportData>;

    //    ExcelConfig excelconfig = ExcelConfig.Default($"{DateTime.Now.ToString("yyyyMMddHHmmss")}_调拨出库导入错误报告.xls");
    //    foreach (KeyValuePair<string, string> item in Common.Extension.Extensions.GetPropertityToMaps<ErpOutorderDbImportData>())
    //    {
    //        string? column = item.Key;
    //        string? excelColumn = item.Value;
    //        excelconfig.ColumnModel.Add(new ExcelColumnModel() { Column = column, ExcelColumn = excelColumn });
    //    }

    //    string? addPath = Path.Combine(FileVariable.TemporaryFilePath, excelconfig.FileName);
    //    //ExcelExportHelper<ErpOrderListImportDataInput>.Export(errorlist, excelconfig, addPath);

    //    var flag = await _fileManager.UploadFileByType(ExcelExportHelper<ErpOutorderDbImportData>.ExportMemoryStream(errorlist, excelconfig), FileVariable.TemporaryFilePath, excelconfig.FileName);
    //    return new { name = excelconfig.FileName, url = flag.Item2 ?? "/api/file/Download?encryption=" + DESCEncryption.Encrypt(_userManager.UserId + "|" + excelconfig.FileName + "|" + addPath + "|" + _userManager.TenantId, "QT") };
    //}

    /// <summary>
    /// 导入用户数据函数.
    /// </summary>
    /// <param name="list">list.</param>
    /// <returns>[成功列表,失败列表].</returns>
    private async Task<object[]> ImportOrderData(List<ErpOutorderDbImportData> list)
    {
        List<ErpOutorderDbImportData> userInputList = list;
        //var oid = _userManager.CompanyId;
        //var oidName = "当前公司";
        //if (oid.IsNotEmptyOrNull())
        //{
        //    oidName = await _repository.Context.Queryable<OrganizeEntity>().Where(it => it.Id == oid).Select(it => it.FullName).FirstAsync();
        //}
        #region 初步排除错误数据

        if (userInputList == null || userInputList.Count() < 1)
            throw Oops.Oh(ErrorCode.D5019);

        List<ErpOutorderDbImportData>? errorList = new List<ErpOutorderDbImportData>();
        // 必填字段验证 (配送日期、客户名称、商品名称、单位、数量、餐别)
        var requiredList = EntityHelper<ErpOutorderDbImportData>.InstanceProperties.Where(p => p.HasAttribute<RequiredAttribute>());
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
                       !decimal.TryParse(x.Num, out var n1)
                    || !string.IsNullOrEmpty(x.Amount) && !decimal.TryParse(x.Amount, out var n2)
                    || !string.IsNullOrEmpty(x.Price) && !decimal.TryParse(x.Price, out var n3) // 价格填了值才去判断
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

        ////餐别字典
        //var diningTypeOptions = await _dictionaryDataService.GetList("ErpOrderDiningType");

        // 带出关联表的数据
        // 客户集合
        //var customerList = await _repository.Context.Queryable<ErpCustomerEntity>().Where(it => it.Oid == oid).Select(it => new ErpCustomerEntity { Id = it.Id, Name = it.Name }).ToListAsync();
        //// 用户集合
        //var userList = await _repository.Context.Queryable<UserEntity>().Select(it => new UserEntity { Id = it.Id, RealName = it.RealName }).ToListAsync();

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


        userInputList = userInputList.Except(errorList).ToList();
        List<ErpOutorderDbImportData> newList = new List<ErpOutorderDbImportData>();
        foreach (var item in userInputList)
        {
            // 调出公司id
            item.oid1 = organizeList.Find(x => x.FullName == item.Oid1Name)?.Id ?? "";
            // 调入公司
            item.oid2 = organizeList.Find(x => x.FullName == item.Oid2Name)?.Id ?? "";

            // 转规格
            var qur = productList.Where(x => x.productName == item.ProductName && x.midName == item.MidName);
            //if (!string.IsNullOrEmpty(item.ProductType))
            //{
            //    qur = qur.Where(x => x.productType == item.ProductType);
            //}
            //if (!string.IsNullOrEmpty(unit))
            //{
            //    qur = qur.Where(x => x.unit == unit);
            //}
            var midEntity = qur.FirstOrDefault();
            item.mid = midEntity?.id ?? "";

            //var mid = productList.Find(x => x.productName == item.ProductName && x.productType == item.ProductType && x.midName == item.MidName && x.unit == unit)?.id ?? "";

            // 转换后判断客户id，下单员、规格是否有空，如果是则加入错误列表
            if (!item.oid1.IsNotEmptyOrNull() || !item.oid2.IsNotEmptyOrNull() || !item.mid.IsNotEmptyOrNull() /*|| (item.DiningType.IsNotEmptyOrNull() && !diningType.IsNotEmptyOrNull())*/)
            {
                var errors = new List<string>();
                if (!item.oid1.IsNotEmptyOrNull())
                {
                    errors.Add("调出公司不存在！");
                }
                if (!item.oid2.IsNotEmptyOrNull())
                {
                    errors.Add("调入公司不存在！");
                }
                if (!item.mid.IsNotEmptyOrNull())
                {
                    errors.Add("商品规格不存在！");
                }
                //if (!diningType.IsNotEmptyOrNull())
                //{
                //    errors.Add("餐别不存在！");
                //}
                item.ErrorMessage = string.Join(", ", errors);
                errorList.Add(item);
            }
            else
            {
                // 判断规格是否绑定了当前公司
                if (relationCompanys.Any(x => x.Oid == item.oid1 && (x.Pid == midEntity!.id || x.Pid == midEntity.pid))
                    || !relationCompanys.Any(x => x.Pid == midEntity!.id || x.Pid == midEntity.pid))
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
                    item.ErrorMessage = $"商品没有关联{item.Oid1Name}";
                    errorList.Add(item);
                }
            }
        }

        var dump = newList.GroupBy(x => new { x.oid1, x.oid2,x.mid }).Where(x => x.Count() > 1).SelectMany(x => x).ToList();
        if (dump.IsAny())
        {
            // 排除重复
            dump.ForEach(x =>
            {
                x.ErrorMessage = $"商品重复！";
                errorList.Add(x);
                newList.Remove(x);
            });
        }

        // 再重新排除一次异常记录
        userInputList = newList;

        //List<ErpOutorderEntity> addList = new List<ErpOutorderEntity>();
        List<ErpOutrecordEntity> addDetailList = new List<ErpOutrecordEntity>();
        // 根据调出公司，调入公司 分组生成订单


        List<ErpOutorderCrInput> addList = new List<ErpOutorderCrInput>();
        foreach (var g in userInputList.GroupBy(it => new { it.oid1, it.oid2 }))
        {
            ErpOutorderCrInput erpOutorderCrInput = new ErpOutorderCrInput
            {
                oid = g.Key.oid1,
                inOid = g.Key.oid2,
                inType = "2",
                state = "1",
                erpOutrecordList = new List<ErpOutrecordCrInput>()
                //erpOutrecordList = g.Select(x=> new ErpOutrecordCrInput
                //{
                //    gid = x.mid,
                //    num = decimal.Parse(x.Num),
                //    price = x.Price.IsNotEmptyOrNull() ? decimal.Parse(x.Price) : 0,
                //    amount = x.Amount.IsNotEmptyOrNull() ? decimal.Parse(x.Amount) : 0,
                //    storeDetailList = new List<ErpStorerecordInput>()
                //}).ToList()
            };
            foreach (var x in g)
            {
                var item = new ErpOutrecordCrInput
                {
                    gid = x.mid,
                    num = decimal.Parse(x.Num),
                    price = x.Price.IsNotEmptyOrNull() ? decimal.Parse(x.Price) : 0,
                    amount = x.Amount.IsNotEmptyOrNull() ? decimal.Parse(x.Amount) : 0,
                    storeDetailList = new List<ErpStorerecordInput>()
                };

                // 获取库存
                var storeDetailList = await _erpInorderService.GetStore(x.mid, new ErpStoreListQueryInput
                {
                    oid = g.Key.oid1
                });

                if (!storeDetailList.IsAny())
                {
                    // 库存不足
                    x.ErrorMessage = $"库存不足";
                    errorList.Add(x);
                    continue;
                }
                if (storeDetailList.Count>1)
                {
                    x.ErrorMessage = $"存在多个批次";
                    errorList.Add(x);
                    continue;
                }

                var store = storeDetailList[0];

                if (item.num > store.num)
                {
                    // 库存不足
                    x.ErrorMessage = $"库存不足";
                    errorList.Add(x);
                    continue;
                }

                item.storeDetailList.Add(new ErpStorerecordInput
                {
                    id = store.id,
                    num = store.num,
                });

                erpOutorderCrInput.erpOutrecordList.Add(item);
            }

            if (erpOutorderCrInput.erpOutrecordList.IsAny())
            {
                addList.Add(erpOutorderCrInput);
            }

            //ErpOrderXsCrInput erpOrderCrInput = new ErpOrderXsCrInput()
            //{
            //    cid = g.Key.CidName,
            //    createTime = createTime,
            //    posttime = posttime,
            //    diningType = g.Key.DiningType,
            //    //createUid = g.Key.CreateUidName,
            //    erpOrderdetailList = new List<ErpOrderdetailCrInput>()
            //};
            //var customerType = await _repository.Context.Queryable<ErpCustomerEntity>().Where(x => x.Id == erpOrderCrInput.cid).Select(x => x.Type).FirstAsync();

            //var q1 = _repository.Context.Queryable<ErpProductcustomertypepriceEntity>().Where(xxx => xxx.Oid == _userManager.CompanyId);
            //var q2 = _repository.Context.Queryable<ErpProductcustomertypepriceEntity>()
            //    .Where(yyy => (yyy.Oid ?? "") == "" && !SqlFunc.Subqueryable<ErpProductcustomertypepriceEntity>().Where(xxx => xxx.Oid == _userManager.CompanyId && xxx.Tid == yyy.Tid && xxx.Gid == yyy.Gid).Any());
            //var qur = _repository.Context.Union(q1, q2);

            ////没有单价的自动带出单价
            //var customerPriceList = await _repository.Context.Queryable<ErpProductmodelEntity>()
            //    .LeftJoin<ErpProductEntity>((a, b) => a.Pid == b.Id)
            //    .LeftJoin<ErpProductpriceEntity>((a, b, c) => a.Id == c.Gid && c.Cid == erpOrderCrInput.cid)
            //    .LeftJoin(qur, (a, b, c, d) => a.Id == d.Gid && d.Tid == customerType)
            //    //.LeftJoin<ErpProductcustomertypepriceEntity>((a, b, c, d) => a.Id == d.Gid && d.Tid == customerType)
            //    .Where((a, b) => b.State == "1")
            //    .Select((a, b, c, d) => new ErpProductSelectorOutput
            //    {
            //        id = a.Id,
            //        salePrice = c.Price > 0 ? c.Price : (d.PricingType == 1 ? (a.SalePrice * d.Discount * 0.01m) : (d.PricingType == 2 ? d.Price : a.SalePrice)), //优先使用客户定价，然后是客户类型折扣，再是客户类型价格，最后是原价
            //    }).ToListAsync();
            //foreach (var item in g)
            //{
            //    decimal salePrice = 0;
            //    decimal amount = 0;
            //    if (item.SalePrice.IsNullOrEmpty())
            //    {
            //        salePrice = customerPriceList.FirstOrDefault(x => x.id == item.MidName)?.salePrice ?? 0;
            //    }
            //    else
            //    {
            //        salePrice = decimal.Parse(item.SalePrice);
            //    }

            //    // 计算金额
            //    amount = salePrice * decimal.Parse(item.Num);
            //    //if (item.Amount.IsNullOrEmpty())
            //    //{
            //    //    amount = salePrice * decimal.Parse(item.Num);
            //    //}
            //    //else
            //    //{
            //    //    amount = decimal.Parse(item.Amount);
            //    //}
            //    erpOrderCrInput.erpOrderdetailList.Add(new ErpOrderdetailCrInput
            //    {
            //        mid = item.MidName,
            //        num = item.Num.IsNullOrEmpty() ? 0 : decimal.Parse(item.Num),
            //        amount = amount,// item.Amount.IsNullOrEmpty() ? 0 : decimal.Parse(item.Amount),
            //        salePrice = salePrice, // item.SalePrice.IsNullOrEmpty() ? 0 : decimal.Parse(item.SalePrice),
            //        remark = item.Remark
            //    });
            //}

            //// 处理主表信息
            //var entity = erpOrderCrInput.Adapt<ErpOrderEntity>();
            //entity.Id = SnowflakeIdHelper.NextId();
            //entity.CreateUid = g.Key.CreateUidName;
            //entity.No = await _billRullService.GetBillNumber("QTErpOrder");
            //entity.State = OrderStateEnum.Draft;
            //entity.Amount = erpOrderCrInput.erpOrderdetailList?.Sum(a => a.amount) ?? 0;
            //addList.Add(entity);


            //// 处理明细表信息
            //var erpOrderdetailEntityList = erpOrderCrInput.erpOrderdetailList.Adapt<List<ErpOrderdetailEntity>>();
            //if (erpOrderdetailEntityList != null)
            //{
            //    foreach (var item in erpOrderdetailEntityList)
            //    {
            //        item.Id = SnowflakeIdHelper.NextId();
            //        item.Fid = entity.Id;
            //    }
            //    addDetailList.AddRange(erpOrderdetailEntityList);
            //}
        }

        if (addList.IsAny())
        {
            foreach (var item in addList)
            {
                // 导入调出单
                await _erpOutorderService.Create(item);

            }
        }


        //if (addList.Any())
        //{
        //    try
        //    {
        //        // 开启事务
        //        _db.BeginTran();

        //        // 新增订单记录
        //        await _repository.Context.Insertable<ErpOrderEntity>(addList).ExecuteCommandAsync();

        //        //新增订单明细
        //        await _repository.Context.Insertable<ErpOrderdetailEntity>(addDetailList).ExecuteCommandAsync();

        //        _db.CommitTran();
        //    }
        //    catch (Exception)
        //    {
        //        _db.RollbackTran();
        //        errorList.AddRange(userInputList);
        //        userInputList = new List<ErpOutorderDbImportData>();
        //    }
        //}

        return new object[] { addList, errorList };
    }

    #endregion


    /// <summary>
    /// 根据入库明细id获取待选记录
    /// </summary>
    /// <param name="inid">入库明细id</param>
    /// <param name="mid">规格id</param>
    /// <returns></returns>
    [HttpGet("Actions/Transfer/v2/{inid}/{mid}")]
    public async Task<dynamic> GetTransferListV2(string inid, string mid)
    {
        var qur = _repository.Context.Queryable<ErpInrecordTsEntity>().GroupBy(c => c.TsId).Select(c => new
        {
            c.TsId,
            Num = SqlFunc.AggregateSum(c.Num)
        });
        // 当前选中的记录
        var value = await _repository.Context.Queryable<ErpInrecordTsEntity>().Where(x => x.InId == inid).Select(x => x.TsId).ToListAsync();
        var list = await _repository.Context.Queryable<ErpInorderEntity>()
            .InnerJoin<ErpInrecordEntity>((a, b) => a.Id == b.InId)
            .LeftJoin(qur, (a, b, c) => b.Id == c.TsId)
            .Where((a, b) => a.InType == "5" /*&& (a.SpecialState ?? "") == ""*/ && b.Gid == mid)  // 未完成的特殊入库记录
                                                                                                   //.Where((a, b) => SqlFunc.Subqueryable<ErpInrecordTsEntity>().Where(ddd => ddd.TsId == b.Id).NotAny() || value.Contains(b.Id))
            .Where((a, b, c) => b.InNum > SqlFunc.IsNull(c.Num, 0) || value.Contains(b.Id))
            .OrderBy((a, b) => a.CreatorTime, OrderByType.Desc)
            .Select((a, b) => new ErpBuyorderTransferListOutput
            {
                id = b.Id,
                creatorTime = a.CreatorTime,
                num = b.InNum,
                no = a.No
            })
            .ToListAsync();

        var inDetailIds = list.Select(x => x.id).ToList();
        var relationOrder = await _repository.Context.Queryable<ErpOutdetailRecordEntity>().InnerJoin<ErpOrderdetailEntity>((a, b) => a.OutId == b.Id)
                   .InnerJoin<ErpOrderEntity>((a, b, c) => b.Fid == c.Id)
                   .InnerJoin<ErpInrecordEntity>((a, b, c, d) => a.InId == d.Id)
                   .Where((a, b, c, d) => inDetailIds.Contains(d.Id))
                   .Select((a, b, c, d) => new
                   {
                       no = c.No,
                       id = d.Id,
                       posttime = c.Posttime,
                       customerName = SqlFunc.Subqueryable<ErpCustomerEntity>().Where(ddd => ddd.Id == c.Cid).Select(ddd => ddd.Name)
                   }).ToListAsync();

        foreach (var item in relationOrder)
        {
            var xitem = list.FirstOrDefault(x => x.id == item.id);
            if (xitem != null)
            {
                xitem.posttime = item.posttime;
                xitem.orderNo = item.no;
                xitem.customerName = item.customerName;
            }
        }

        // 汇总特殊入库的数量
        var tsids = list.Select(x => x.id);
        var tsAll = await _repository.Context.Queryable<ErpInrecordTsEntity>().Where(it => tsids.Contains(it.TsId)).ToListAsync();

        foreach (var item in list)
        {
            item.num1 = tsAll.Where(x => x.TsId == item.id && x.InId != inid).Sum(x => x.Num);
            item.num2 = tsAll.Where(x => x.TsId == item.id && x.InId == inid).Sum(x => x.Num);

            if (item.num2 > 0)
            {
                item.ifselected = true;
            }
        }


        //var nlist = list.Select(it => new
        //{
        //    it.id,
        //    it.num,
        //    creatorTime = relationOrder.FirstOrDefault(x => x.id == it.id)?.posttime,
        //    no = relationOrder.FirstOrDefault(x => x.id == it.id)?.no,
        //}).ToList();

        return new
        {
            value = value ?? new List<string>(),
            data = list,
        };
    }

    /// <summary>
    /// 保存关联关系
    /// </summary>
    /// <param name="inid"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("Actions/Transfer/v2/{inid}")]
    [SqlSugarUnitOfWork]
    public async Task<dynamic> PostTransferListV2(string inid, [FromBody] ErpInrecordTransferInputV2 input)
    {
        // 本次关联的特殊入库id集合
        if (input.items.IsAny())
        {
            var tsids = input.items.Select(x => x.id).ToArray();
            // 1、判断累计关联特殊入库数量是否大于特殊入库数量
            var relate_tslist = await _repository.Context.Queryable<ErpInrecordTsEntity>().Where(x => x.InId != inid && tsids.Contains(x.Id)).Select(x => new ErpInrecordTransferInputV2Item
            {
                id = x.Id,
                num = x.Num
            }).ToListAsync();

            relate_tslist.AddRange(input.items);
            relate_tslist = relate_tslist.GroupBy(x => x.id).Select(x => new ErpInrecordTransferInputV2Item
            {
                id = x.Key,
                num = x.Sum(a => a.num)
            }).ToList();
            // 特殊入库记录
            var tsList = await _repository.Context.Queryable<ErpInrecordEntity>().Where(x => tsids.Contains(x.Id)).ToListAsync();

            foreach (var item in tsList)
            {
                var xitem = relate_tslist.FirstOrDefault(x => x.id == item.Id);
                if (xitem != null && xitem.num > item.InNum)
                {
                    throw Oops.Oh("累计关联特殊入库数据大于特殊入库数量，禁止保存！");
                }
            }
        }



        // 获取出库记录
        var outrecord = await _repository.Context.Queryable<ErpOutrecordEntity>().InSingleAsync(inid) ?? throw Oops.Oh(ErrorCode.COM1005);
        //_repository.Context.Tracking(outrecord);
        //// 当前的入库数量
        //if (input.inNum.HasValue)
        //{
        //    outrecord.InNum = input.inNum;
        //}
        //// 汇总特殊入库数量
        ////var arr = input.value ?? new List<string>();
        //outrecord.TsNum = input.items.Sum(x => x.num); // await _repository.Context.Queryable<ErpInrecordEntity>().Where(x => input.value.Contains(x.Id)).SumAsync(x => x.InNum);
        ////decimal? inNum = null;
        //if (outrecord.TsNum > outrecord.Num)
        //{
        //    //throw Oops.Oh("特殊入库数量大于实际采购数量，禁止操作！");
        //}
        //else
        //{
        //    if (!outrecord.InNum.HasValue)
        //    {
        //        //buydetail.InNum = buydetail.Num - buydetail.TsNum;
        //    }
        //}

        //// 更新实际采购入库数
        ////buydetail.InNum = buydetail.Num - buydetail.TsNum;
        //var num = (outrecord.InNum ?? 0) + (outrecord.TsNum);
        //outrecord.Price = num != 0 ? Math.Round(outrecord.Amount / num, 2) : 0;

        // 当前选中的记录
        var list = await _repository.Context.Queryable<ErpInrecordTsEntity>().Where(x => x.InId == inid).ToListAsync();

        var items = input.items?.Select(x => new ErpInrecordTsEntity
        {
            InId = inid,
            TsId = x.id,
            Num = x.num
        }).ToList() ?? new List<ErpInrecordTsEntity>();

        if (items.IsAny())
        {
            foreach (var item in items)
            {
                var entity = list.Find(x => x.InId == item.InId && x.TsId == item.TsId);
                if (entity != null)
                {
                    item.Id = entity.Id;
                }
                else
                {
                    item.Id = SnowflakeIdHelper.NextId();
                }
            }
        }

        await _repository.Context.Deleteable<ErpInrecordTsEntity>().Where(x => x.InId == inid).ExecuteCommandAsync();

        if (items.IsAny())
        {
            await _repository.Context.Insertable<ErpInrecordTsEntity>(items).ExecuteCommandAsync();
        }

        //// 更新特殊入库的数量
        //await _repository.Context.Updateable<ErpBuyorderdetailEntity>(outrecord).ExecuteCommandAsync();
        var tsNum = input.items.Sum(x => x.num);
        var num = Math.Max(0, outrecord.Num - tsNum);
        //tsNum > 0 ? Math.Round((outrecord.Num - tsNum) * outrecord.Price, 2) : outrecord.Amount
        var amount = tsNum > 0 ? Math.Round(num * outrecord.Price, 2) : outrecord.Amount;
        return new
        {
            tsNum = Math.Round(tsNum,2),
            num = Math.Round(num, 2),
            amount = Math.Round(amount, 2)
            //price = outrecord.Price,
            //inNum = buydetail.InNum
        };
    }
}
