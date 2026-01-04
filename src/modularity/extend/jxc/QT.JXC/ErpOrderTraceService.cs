using Microsoft.AspNetCore.Authorization;
using QT.Common.Core;
using QT.Common.Core.Manager.Files;
using QT.Common.Extension;
using QT.Common.Models;
using QT.DependencyInjection;
using QT.JXC.Entitys.Dto.ErpOrderTrace;
using QT.JXC.Entitys.Entity;
using QT.JXC.Entitys.Entity.ERP;
using QT.JXC.Interfaces;
using QT.Systems.Entitys.Permission;
using QT.Systems.Interfaces.System;
using System.Web;
using Yitter.IdGenerator;

namespace QT.JXC;

/// <summary>
/// 业务实现：订单商品溯源信息.
/// </summary>
[ApiDescriptionSettings(ModuleConst.ERP, Tag = "Erp", Name = "ErpOrderTrace", Order = 200)]
[Route("api/Erp/[controller]")]
public class ErpOrderTraceService : IDynamicApiController, IErpOrderTraceService, ITransient
{
    private readonly ISqlSugarRepository<ErpOrderTraceEntity> _repository;
    private readonly IWechatAppProxy _wechatAppProxy;
    private readonly IFileManager _fileManager;
    private readonly IUserManager _userManager;
    private readonly IDictionaryDataService _dictionaryDataService;

    public ErpOrderTraceService(ISqlSugarRepository<ErpOrderTraceEntity> repository, IWechatAppProxy wechatAppProxy, IFileManager fileManager, IUserManager userManager, IDictionaryDataService dictionaryDataService)
    {
        _repository = repository;
        _wechatAppProxy = wechatAppProxy;
        _fileManager = fileManager;
        _userManager = userManager;
        _dictionaryDataService = dictionaryDataService;
    }

    /// <summary>
    /// 新建溯源信息
    /// </summary>
    /// <param name="id">订单明细id</param>
    /// <returns></returns>
    [HttpPost("{id}")]
    [SqlSugarUnitOfWork]
    public async Task Create(string id)
    {
        var model = await CreateTraceEntity(id);
        if (model == null)
        {
            throw Oops.Oh(ErrorCode.COM1000);
        }
        var entity = await _repository.SingleAsync(x => x.OrderDetailId == id);
        if (entity != null)
        {
            _repository.Context.Tracking(entity);
            entity.Content = model.ToJsonString();
            await _repository.Context.Updateable(entity).ExecuteCommandAsync();
        }
        else
        {
            ErpOrderTraceEntity erpOrderTraceEntity = new ErpOrderTraceEntity
            {
                Id = YitIdHelper.NextId(),
                Code = $"{model.orderInfo.no}{model.orderInfo.id}",
                Content = model.ToJsonString(),
                CreatorTime = DateTime.Now,
                CreatorUserId = _userManager.UserId,
                Num = 0,
                OrderDetailId = id,
                QRCodeUrl = ""
            };

            var key = KeyVariable.MultiTenancy ? $"{_userManager.TenantId}@{erpOrderTraceEntity.Id}" : $"{erpOrderTraceEntity.Id}";

            var dict = new Dictionary<string, string>();
            if (KeyVariable.MultiTenancy)
            {
                dict.Add("tenant", _userManager.TenantId);
            }
            dict.Add("page", "/pages/erp/trace/index");
            dict.Add("param", HttpUtility.UrlEncode($"scene={erpOrderTraceEntity.Id}"));

            erpOrderTraceEntity.QRCodeUrl = string.Join("&", dict.Select(x => $"{x.Key}={x.Value}"));  // JSON.Serialize(dict);

            // 保存到数据库
            await _repository.InsertAsync(erpOrderTraceEntity);

            //// 生成小程序二维码
            //var result = await _wechatAppProxy.GetWxacodeUnlimit(new Common.Core.Dto.IWechatAppProxy.WxacodeUnlimitRequest
            //{
            //    check_path = false,
            //    page = "pages/erp/trace/index",
            //    scene = key
            //});
            //if (result != null)
            //{
            //    using (MemoryStream memoryStream = new MemoryStream())
            //    {
            //        result.CopyTo(memoryStream);
            //        try
            //        {
            //            byte[] byteArray = memoryStream.ToArray();
            //            // 使用byteArray进行后续操作 
            //            var str = Encoding.Default.GetString(byteArray);
            //            var jo = JObject.Parse(str);
            //            if (jo!=null)
            //            {
            //                throw Oops.Oh("二维码生成失败！");
            //            }
            //        }
            //        catch (Exception)
            //        {
            //        }
            //        memoryStream.Position = 0;
            //        var res = await _fileManager.UploadFileByType(memoryStream, FileVariable.MPMaterialFilePath, $"{erpOrderTraceEntity.Id}.png");

            //        if (res.Item1)
            //        {
            //            erpOrderTraceEntity.QRCodeUrl = res.Item2;

            //            // 保存到数据库
            //            await _repository.InsertAsync(erpOrderTraceEntity);
            //        }
            //    }
            //}
            //else
            //{
            //    throw Oops.Oh("二维码生成失败！");
            //}
        }

        // 创建批次条码
        if (model.originInfo != null && model.originInfo.inRecordList.IsAny())
        {
            foreach (var item in model.originInfo.inRecordList)
            {
                await this.CreateBatch(id, item.id);
            }
        }
    }


    /// <summary>
    /// 新建溯源信息,按批次
    /// </summary>
    /// <param name="id">订单明细id</param>
    /// <param name="inid">入库明细id</param>
    /// <returns></returns>
    [HttpPost("{id}/batch/{inid}")]
    [SqlSugarUnitOfWork]
    public async Task CreateBatch(string id,string inid)
    {
        var model = await CreateTraceEntity(id);
        if (model == null)
        {
            throw Oops.Oh(ErrorCode.COM1000);
        }

        if (model.originInfo!=null && model.originInfo.inRecordList.IsAny())
        {
            var index = 0;
            foreach (var item in model.originInfo.inRecordList)
            {
                index++;
                var detailid = $"{id}_{item.id}";
                var entity = await _repository.SingleAsync(x => x.OrderDetailId == detailid);

                if (entity != null)
                {
                    _repository.Context.Tracking(entity);
                    entity.Content = model.ToJsonString();
                    await _repository.Context.Updateable(entity).ExecuteCommandAsync();
                }
                else
                {
                    ErpOrderTraceEntity erpOrderTraceEntity = new ErpOrderTraceEntity
                    {
                        Id = YitIdHelper.NextId(),
                        Code = $"{model.orderInfo.no}{model.orderInfo.id}-{index}",
                        Content = model.ToJsonString(),
                        CreatorTime = DateTime.Now,
                        CreatorUserId = _userManager.UserId,
                        Num = 0,
                        OrderDetailId = detailid,
                        QRCodeUrl = ""
                    };

                    var key = KeyVariable.MultiTenancy ? $"{_userManager.TenantId}@{erpOrderTraceEntity.Id}" : $"{erpOrderTraceEntity.Id}";

                    var dict = new Dictionary<string, string>();
                    if (KeyVariable.MultiTenancy)
                    {
                        dict.Add("tenant", _userManager.TenantId);
                    }
                    dict.Add("page", "/pages/erp/trace/index");
                    dict.Add("param", HttpUtility.UrlEncode($"scene={erpOrderTraceEntity.Id}"));

                    erpOrderTraceEntity.QRCodeUrl = string.Join("&", dict.Select(x => $"{x.Key}={x.Value}"));  // JSON.Serialize(dict);

                    // 保存到数据库
                    await _repository.InsertAsync(erpOrderTraceEntity);
                }
            }
        }


        
    }
    /// <summary>
    /// 获取信息
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [SqlSugarUnitOfWork]
    [AllowAnonymous]
    public async Task<ErpOrderTraceInfoOutput> GetInfo(string id, [FromQuery(Name = "x-saas-token")] string tenant)
    {
        if (tenant.IsNotEmptyOrNull())
        {
            _repository.Context.ChangeDatabase(tenant);
        }
        _repository.Context.QueryFilter.Clear();
        var entity = await _repository.Context.Queryable<ErpOrderTraceEntity>().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);

        _repository.Context.Tracking(entity);
        entity.Num++;
        entity.LastQueryTime = DateTime.Now;

        if (!entity.FirstQueryTime.HasValue)
        {
            entity.FirstQueryTime = entity.LastQueryTime;
        }
        await _repository.Context.Updateable(entity).ExecuteCommandAsync();

        var result = entity.Content.ToObject<ErpOrderTraceInfoOutput>();

        var pars = entity.OrderDetailId.Split('_');
        var realOrderDetailId = pars[0];
        var inId = pars.Length > 1 ? pars[1] : "";

        // 最新的数据
        var erpOrderTraceInfoBase = await CreateTraceEntity(realOrderDetailId);

        result.originInfo.qualityReport = erpOrderTraceInfoBase.originInfo.qualityReport;
        result.originInfo.selfReport = erpOrderTraceInfoBase.originInfo.selfReport;

        // 批次查询
        if (inId.IsNotEmptyOrNull())
        {
            var inrecordInfo = erpOrderTraceInfoBase.originInfo.inRecordList.Where(x => x.id == inId).FirstOrDefault();

            if (inrecordInfo!=null)
            {
                result.originInfo.inRecordList = new List<OriginInrecordInfo> { inrecordInfo };

                if (inrecordInfo.qualityReportProof.IsNotEmptyOrNull())
                {
                    result.originInfo.qualityReport.Clear();
                    inrecordInfo.qualityReportProof.ToObject<List<FileControlsModel>>()?.ForEach(x =>
                    {
                        if (x.url.IsNotEmptyOrNull() && !result.originInfo.qualityReport.Contains(x.url))
                        {
                            result.originInfo.qualityReport.Add(x.url);
                        }
                    });
                }

                if (inrecordInfo.productDate.IsNotEmptyOrNull())
                {
                    result.orderInfo.productDate = inrecordInfo.productDate;
                    result.orderInfo.retention = inrecordInfo.retention;
                }
            }

           
        }


        if (result.orderInfo.productDate.IsNullOrEmpty())
        {
            result.orderInfo.productDate = erpOrderTraceInfoBase.orderInfo.productDate ?? "";
            result.orderInfo.retention = erpOrderTraceInfoBase.orderInfo.retention ?? "";
        }

        result.orderInfo.psTime = erpOrderTraceInfoBase.orderInfo.psTime ?? "";
        result.orderInfo.fjTime = erpOrderTraceInfoBase.orderInfo.fjTime ?? "";

        result.traceInfo = new TraceInfo
        {
            id = entity.Id.ToString(),
            num = entity.Num,
            firstQueryTime = entity.FirstQueryTime.Value.ToString("yyyy-MM-dd HH:mm:ss"),
            code = entity.Code,
        };

        return result;
    }

    [HttpGet("Detail/{id}")]
    public async Task<List<OriginInrecordInfo>> GetOrderOutList(string id)
    {
        var inRecordList = await _repository.Context.Queryable<ErpInrecordEntity>().
             InnerJoin<ErpInorderEntity>((a, b) => a.InId == b.Id)
             .LeftJoin<ErpBuyorderdetailEntity>((a, b, c) => a.Bid == c.Id)
              .LeftJoin<ErpBuyorderEntity>((a, b, c, d) => c.Fid == d.Id)
              .LeftJoin<ErpOutdetailRecordEntity>((a,b,c,d,e)=> e.InId == a.Id && e.OutId == id)
              .Where((a,b,c,d,e)=>e.OutId == id)
             //.Where((a, b) => SqlFunc.Subqueryable<ErpOutdetailRecordEntity>().Where(x => x.InId == a.Id && x.OutId == id).Any())
             .Select((a, b, c, d,e) => new OriginInrecordInfo
             {
                 id = a.Id,
                 pid = b.Id,
                 rkNo = b.No,
                 cgNo = d.No, // b.CgNo,
                 num = SqlFunc.ToString(a.InNum),
                 supplierId = c.Supplier,
                 supplierName = SqlFunc.Subqueryable<ErpSupplierEntity>().Where(ddd => ddd.Id == c.Supplier).Select(ddd => ddd.Name),
                 cgId = c.Fid,
                 productDate = SqlFunc.IIF(a.ProductionDate.HasValue, a.ProductionDate.Value.ToString("yyyy年MM月dd日"), c.ProductionDate.Value.ToString("yyyy年MM月dd日")),
                 //productDate = SqlFunc.IIF(c.LastModifyTime> a.LastModifyTime, c.ProductionDate.Value.ToString("yyyy-MM-dd"), a.ProductionDate.Value.ToString("yyyy-MM-dd")),
                 retention = SqlFunc.IIF(SqlFunc.HasValue(a.Retention), a.Retention, c.Retention),
                 qualityReportProof = c.QualityReportProof,
                 useNum = SqlFunc.ToString(e.Num),
             })
             .ToListAsync();

        return inRecordList;
    }

    #region Private Methods
    private async Task<ErpOrderTraceInfoBase> CreateTraceEntity(string id)
    {
        ErpOrderTraceInfoBase erpOrderTraceInfoBase = new ErpOrderTraceInfoBase();

        // 订单明细
        var orderInfo = await _repository.Context.Queryable<ErpOrderdetailEntity>().
             InnerJoin<ErpOrderEntity>((a, b) => a.Fid == b.Id)
             .Where((a, b) => a.Id == id)
             .Select((a, b) => new OrderInfo
             {
                 id = a.Id,
                 cid = b.Cid,
                 cidName = SqlFunc.Subqueryable<ErpCustomerEntity>().Where(ddd => ddd.Id == b.Cid).Select(ddd => ddd.Name),
                 cidType = SqlFunc.Subqueryable<ErpCustomerEntity>().Where(ddd => ddd.Id == b.Cid).Select(ddd => ddd.Type),
                 fid = b.Id,
                 fjTime = a.SorterTime.Value.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
                 //psTime = b.DeliveryTime.Value.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
                 psTime = b.Posttime.Value.ToString("yyyy-MM-dd") ?? "",
                 oid = b.Oid,
                 oidName = SqlFunc.Subqueryable<OrganizeEntity>().Where(ddd => ddd.Id == b.Oid).Select(ddd => ddd.FullName),
                 gid = a.Mid,
                 no = b.No
             })
             .FirstAsync();

        if (orderInfo != null)
        {
            // 商品信息
            var productInfo = await _repository.Context.Queryable<ErpProductmodelEntity>().
               InnerJoin<ErpProductEntity>((a, b) => a.Pid == b.Id)
               .Where((a, b) => a.Id == orderInfo.gid)
               .Select((a, b) => new ProductInfo
               {
                   id = b.Id,
                   gid = a.Id,
                   gidName = a.Name,
                   productName = b.Name,
                   typeId = b.Tid,
                   productType = SqlFunc.Subqueryable<ErpProducttypeEntity>().Where(ddd => ddd.Id == b.Tid).Select(ddd => ddd.Name),
                   unit = a.Unit,
                   origin = b.Producer ?? "",
                   brand = ""
               })
               .FirstAsync();

            if (productInfo != null)
            {
                productInfo.imageList = await _repository.Context.Queryable<ErpProductpicEntity>().Where(x => x.Pid == productInfo.id).Select(x => x.Pic).ToListAsync();



                var unitOptions = await _dictionaryDataService.GetList("JLDW");

                productInfo.unitCn = unitOptions.Find(x => x.EnCode == productInfo.unit)?.FullName ?? "";
            }
            var inRecordList = await GetOrderOutList(orderInfo.id);
           //var inRecordList = await _repository.Context.Queryable<ErpInrecordEntity>().
           //   InnerJoin<ErpInorderEntity>((a, b) => a.InId == b.Id)
           //   .LeftJoin<ErpBuyorderdetailEntity>((a, b, c) => a.Bid == c.Id)
           //   .LeftJoin<ErpBuyorderEntity>((a, b, c, d) => c.Fid == d.Id)
           //   .Where((a, b) => SqlFunc.Subqueryable<ErpOutdetailRecordEntity>().Where(x => x.InId == a.Id && x.OutId == orderInfo.id).Any())
           //   .Select((a, b, c, d) => new OriginInrecordInfo
           //   {
           //       id = a.Id,
           //       pid = b.Id,
           //       cgNo = d.No, // b.CgNo,
           //       num = SqlFunc.ToString(a.InNum),
           //       supplierId = c.Supplier,
           //       supplierName = SqlFunc.Subqueryable<ErpSupplierEntity>().Where(ddd => ddd.Id == c.Supplier).Select(ddd => ddd.Name),
           //       cgId = c.Fid,
           //       cgdId = c.Id,
           //       retention = a.Retention,
           //       productDate = a.ProductionDate.Value.ToString("yyyy年MM月dd日")
           //   })
           //   .ToListAsync();


            // 获取检测报告
            // 优先级别：
            // 1、采购明细
            // 2、采购订单
            // 3、客户类型定价
            // 4、用户类型定价
            // 5、销售订单明细

            List<string> QualityReportProof = new List<string>();

            // 1、采购明细
            var cglist = await _repository.Context.Queryable<ErpBuyorderdetailEntity>()
                .Where(x => inRecordList.Any(t => t.cgdId == x.Id))
                .Where(x => !SqlFunc.IsNullOrEmpty(x.QualityReportProof))
                .Select(x => new ErpBuyorderdetailEntity
                {
                    QualityReportProof = x.QualityReportProof,
                    ProductionDate = x.ProductionDate,
                    Retention = x.Retention
                })
                .ToListAsync();

            QualityReportProof = cglist.Select(x => x.QualityReportProof).ToList();

            if (orderInfo.productDate.IsNullOrEmpty())
            {
                var cgitem = cglist.FirstOrDefault(x => x.ProductionDate.HasValue);
                if (cgitem != null)
                {
                    orderInfo.productDate = cgitem.ProductionDate.Value.ToString("yyyy年MM月dd日");

                    orderInfo.retention = cgitem.Retention;
                }
            }

            // 采购订单质检报告
            var buyorders = await _repository.Context.Queryable<ErpBuyorderEntity>()
                .Where(x => inRecordList.Any(t => t.cgId == x.Id))
                .Where(x => !SqlFunc.IsNullOrEmpty(x.QualityReportProof))
                .Select(x => new ErpBuyorderEntity
                {
                    QualityReportProof = x.QualityReportProof,
                    SelfReportProof = x.SelfReportProof
                })
                .ToListAsync();

            // 2、采购订单
            if (!QualityReportProof.IsAny())
            {
                QualityReportProof = buyorders.Select(x => x.QualityReportProof).ToList();
            }

            // 3、客户类型定价
            if (!QualityReportProof.IsAny())
            {
                QualityReportProof = await _repository.Context.Queryable<ErpProductcustomertypepriceEntity>()
                .Where(x => x.Gid == orderInfo.gid && x.Tid == orderInfo.cidType && x.Oid == orderInfo.oid)
                .Where(x => !SqlFunc.IsNullOrEmpty(x.QualityReportProof))
                .Select(x => x.QualityReportProof)
                .ToListAsync();
            }

            // 4、用户类型定价
            if (!QualityReportProof.IsAny())
            {
                QualityReportProof = await _repository.Context.Queryable<ErpProductpriceEntity>()
                .Where(x => x.Gid == orderInfo.gid && x.Oid == orderInfo.oid && x.Cid == orderInfo.cid)
                .Where(x => !SqlFunc.IsNullOrEmpty(x.QualityReportProof))
                .Select(x => x.QualityReportProof)
                .ToListAsync();
            }

            // 5、销售订单明细
            if (!QualityReportProof.IsAny())
            {
                QualityReportProof = await _repository.Context.Queryable<ErpOrderdetailEntity>()
                .Where(x => x.Id == orderInfo.id)
                .Where(x => !SqlFunc.IsNullOrEmpty(x.QualityReportProof))
                .Select(x => x.QualityReportProof)
                .ToListAsync();
            }

            erpOrderTraceInfoBase.orderInfo = orderInfo;
            erpOrderTraceInfoBase.productInfo = productInfo;

            erpOrderTraceInfoBase.originInfo = new OriginInfo
            {
                inRecordList = inRecordList,
                //detectionList = new List<string>(),
                qualityReport = new List<string>(),
                selfReport = new List<string>()
            };

            foreach (var item in buyorders)
            {
                //if (item.QualityReportProof.IsNotEmptyOrNull())
                //{
                //    item.QualityReportProof.ToObject<List<FileControlsModel>>()?.ForEach(x =>
                //    {
                //        if (x.url.IsNotEmptyOrNull() && !erpOrderTraceInfoBase.originInfo.qualityReport.Contains(x.url))
                //        {
                //            erpOrderTraceInfoBase.originInfo.qualityReport.Add(x.url);
                //        }
                //    });
                //}

                if (item.SelfReportProof.IsNotEmptyOrNull())
                {
                    item.SelfReportProof.ToObject<List<FileControlsModel>>()?.ForEach(x =>
                    {
                        if (x.url.IsNotEmptyOrNull() && !erpOrderTraceInfoBase.originInfo.selfReport.Contains(x.url))
                        {
                            erpOrderTraceInfoBase.originInfo.selfReport.Add(x.url);
                        }
                    });
                }
            }

            // 质检报告
            foreach (var item in QualityReportProof)
            {
                if (item.IsNotEmptyOrNull())
                {
                    item.ToObject<List<FileControlsModel>>()?.ForEach(x =>
                    {
                        if (x.url.IsNotEmptyOrNull() && !erpOrderTraceInfoBase.originInfo.qualityReport.Contains(x.url))
                        {
                            erpOrderTraceInfoBase.originInfo.qualityReport.Add(x.url);
                        }
                    });
                }
            }

        }






        // // 订单明细
        // var detail = await _repository.Context.Queryable<ErpOrderdetailEntity>().SingleAsync(x => x.Id == id) ?? throw Oops.Oh(ErrorCode.COM1005);

        // // 订单记录
        //var order = await _repository.Context.Queryable<ErpOrderEntity>().SingleAsync(x=>x.Id == detail.Fid) ?? throw Oops.Oh(ErrorCode.COM1005);

        // // 规格信息
        // var productModel = await _repository.Context.Queryable<ErpProductmodelEntity>().SingleAsync(x => x.Id == detail.Mid) ?? throw Oops.Oh(ErrorCode.COM1005);

        // // 商品信息
        // var product = await _repository.Context.Queryable<ErpProductEntity>().SingleAsync(x => x.Id == productModel.Pid) ?? throw Oops.Oh(ErrorCode.COM1005);

        // // 商品图片
        // var productImageList = await _repository.Context.Queryable<ErpProductpicEntity>().Where(x => x.Pid == product.Id).Select(x=>x.Pic).ToListAsync();

        //// 入库记录
        //var inrecords = await _repository.Context.Queryable<ErpInrecordEntity>().Where(it => SqlFunc.Subqueryable<ErpOutdetailRecordEntity>().Where(x => x.InId == it.Id && x.OutId == orderInfo.id).Any()).ToListAsync();

        //// 采购记录
        //var buyorderdetails = await _repository.Context.Queryable<ErpBuyorderdetailEntity>().Where(x => inrecords.Any(d => d.InId == x.Id)).ToListAsync();


        return erpOrderTraceInfoBase;

    }
    #endregion
}
