using Mapster;
using QT.Common.Extension;
using QT.JXC.Entitys.Dto.Erp.gz_cycs;
using QT.JXC.Entitys.Entity.ERP.gz_cycs;

namespace QT.JXC.gz_cycs;

/// <summary>
/// 业务实现：贵州超市追溯云平台 对接.
/// </summary>
[ApiDescriptionSettings(ModuleConst.ERP, Tag = "Cycs", Name = "BuyorderCycs", Order = 200)]
[Route("api/Erp/[controller]")]
public class ErpBuyorderCycsService : IDynamicApiController
{
    private readonly ISqlSugarRepository<ErpBuyorderEntity> _repository;
    private readonly IUserManager _userManager;
    private readonly IHttpProxy _httpProxy;

    public ErpBuyorderCycsService(ISqlSugarRepository<ErpBuyorderEntity> repository, IUserManager userManager, IHttpProxy httpProxy)
    {
        _repository = repository;
        _userManager = userManager;
        _httpProxy = httpProxy;
    }

    [HttpGet("tCycsInAdd/{id}")]
    public async Task<dynamic> tCycsInAdd(string id)
    {
        var list = await _repository.Context.Queryable<ErpBuyorderdetailEntity>()
            .LeftJoin<ErpProductmodelEntity>((x, a) => x.Gid == a.Id)
            //.LeftJoin<ErpCycsGoodsEntity>((x,a,b)=>a.Pid == b.PId)
            .Where((x, a) => x.Fid == id)
            .Where((x,a)=>SqlFunc.Subqueryable<ErpCycsInEntity>().Where(ddd=>ddd.buyId == x.Id).NotAny())
            .Select((x, a) => new CycsInAddInfoInput
            {
                amount = x.Num ?? 0,
                batchNumber = x.BatchNumber,
                buyId = x.Id,
                cycsId = SqlFunc.Subqueryable<ErpCycsOrganizeEntity>().Where(ddd => ddd.OId == _userManager.CompanyId).Select(ddd => ddd.CycsId),
                divisionCode = SqlFunc.Subqueryable<ErpCycsSupplierEntity>().Where(ddd => ddd.SId == x.Supplier).Select(ddd => ddd.DivisionCode),
                //expiryDate = x.ExpiryDate,
                goodsCode = SqlFunc.Subqueryable<ErpCycsGoodsEntity>().Where(ddd => ddd.PId == a.Pid).Select(ddd => ddd.GoodsId),
                code = SqlFunc.Subqueryable<ErpCycsGoodsEntity>().Where(ddd => ddd.PId == a.Pid).Select(ddd => ddd.Code),
                manufactureDate = x.ProductionDate,
                supplyId = SqlFunc.Subqueryable<ErpCycsSupplierEntity>().Where(ddd => ddd.SId == x.Supplier).Select(ddd => ddd.SupplierId),
                ticketDate = SqlFunc.Subqueryable<ErpBuyorderEntity>().Where(ddd => ddd.Id == x.Fid).Select(ddd => ddd.TaskBuyTime),
                productName = SqlFunc.Subqueryable<ErpProductEntity>().Where(ddd => ddd.Id == a.Pid).Select(ddd => ddd.Name),
                supplierName = SqlFunc.Subqueryable<ErpSupplierEntity>().Where(ddd => ddd.Id == x.Supplier).Select(ddd => ddd.Name),
                pid = a.Pid,
                sid = x.Supplier
            }).ToListAsync();

        if (list.IsAny())
        {
            list.ForEach(x =>
            {
                if (x.goodsCode == 0)
                {
                    x.lossPid = true;
                }
                if (x.supplyId == 0)
                {
                    x.lossSid = true;
                }

            });
        }
        var cycsId = list.FirstOrDefault()?.cycsId ?? 0;
        var pids = list.Where(x => x.goodsCode == 0).Select(x => x.pid).ToList();
        var sids = list.Where(x => x.supplyId == 0).Select(x => x.sid).ToList();
        return new
        {
            list,
            lossPid = await _repository.Context.Queryable<ErpProductEntity>().Where(x => pids.Contains(x.Id)).Select(x => new ErpCycsGoodsAddInfoInput
            {
                barCode = "",
                code = "",
                cycsId = cycsId,
                goodsId = 0,
                localName = x.Name ?? "",
                manufacturer = "",
                pId = x.Id
            }).ToListAsync(),
            lossSid = await _repository.Context.Queryable<ErpSupplierEntity>().Where(x => sids.Contains(x.Id)).Select(x => new ErpCycsSupplierAddInfoInput
            {
                sId = x.Id,
                address = x.Address ?? "",
                supplierId = 0,
                contactPhone = x.AdminTel ?? "",
                divisionCode = "",
                gType = "",
                papersId = "",
                url = "",
                cycsId = cycsId,
                supplyName = x.Name ?? ""
            }).ToListAsync()
        };
    }


    [HttpPost("tCycsGoodsAdd")]
    public async Task tCycsGoodsAdd(List<ErpCycsGoodsAddInfoInput> input)
    {
        var erpCycsGoodsEntities = input.Where(x => x.goodsId > 0).Select(x => new ErpCycsGoodsEntity
        {
            PId = x.pId,
            GoodsId = x.goodsId,
            Code = x.code,
            BarCode = x.barCode,
            CycsId = x.cycsId,
            LocalName = x.localName,
            Manufacturer = x.manufacturer
        }).ToList();

        var newList = input.Where(x => x.goodsId == 0).ToList();
        if (newList.IsAny())
        {
            var inputList = newList.Select(x => new CycsGoodsAddInfo
            {
                cycsId = x.cycsId,
                barCode = "",
                code = x.code,
                localName = x.localName ?? "",
                manufacturer = "",
            }).ToList();
            var response = await _httpProxy.tCycsGoodsAdd(inputList);

            if (response.code == 0)
            {
                var ids = response.ids.Split(",");
                for (int i = 0; i < ids.Length; i++)
                {
                    newList[i].goodsId = int.Parse(ids[i]);
                    erpCycsGoodsEntities.Add(new ErpCycsGoodsEntity
                    {
                        Code = newList[i].code,
                        GoodsId = newList[i].goodsId,
                        PId = newList[i].pId,
                        BarCode = newList[i].barCode,
                        CycsId = newList[i].cycsId,
                        LocalName = newList[i].localName ?? "",
                        Manufacturer = newList[i].manufacturer ?? "",
                    });
                }
            }
            else
            {
                throw Oops.Oh(response.msg);
            }
        }


        if (erpCycsGoodsEntities.IsAny())
        {
            await _repository.Context.Insertable<ErpCycsGoodsEntity>(erpCycsGoodsEntities).ExecuteCommandAsync();
        }
    }

    [HttpPost("tCycsSupplierAdd")]
    public async Task tCycsSupplierAdd(List<ErpCycsSupplierAddInfoInput> input)
    {
        var erpCycsGoodsEntities = input.Where(x => x.supplierId > 0).ToList().Adapt<List<ErpCycsSupplierEntity>>();

        var newList = input.Where(x => x.supplierId == 0).ToList();
        if (newList.IsAny())
        {
            var inputList = newList.Select(x => new CycsSupplierAddInfo
            {
                cycsId = x.cycsId,
                url = x.url,
                papersId = x.papersId,
                gType = x.gType,
                divisionCode = x.divisionCode,
                address = x.address,
                supplyName = x.supplyName,
                contactPhone = x.contactPhone,
            }).ToList();
            var response = await _httpProxy.tCycsSupplierAdd(inputList);

            if (response.code == 0)
            {
                var ids = response.ids.Split(",");
                for (int i = 0; i < ids.Length; i++)
                {
                    newList[i].supplierId = int.Parse(ids[i]);
                    erpCycsGoodsEntities.Add(new ErpCycsSupplierEntity
                    {
                        SId = newList[i].sId,
                        SupplierId = newList[i].supplierId,
                        Address = newList[i].address,
                        ContactPhone = newList[i].contactPhone,
                        DivisionCode = newList[i].divisionCode,
                        GType = newList[i].gType,
                        PapersId = newList[i].papersId,
                        Url = newList[i].url,
                    });
                }
            }
            else
            {
                throw Oops.Oh(response.msg);
            }
        }


        if (erpCycsGoodsEntities.IsAny())
        {
            await _repository.Context.Insertable(erpCycsGoodsEntities).ExecuteCommandAsync();
        }
    }


    [HttpPost("tCycsInAdd")]
    public async Task tCycsInAdd([FromBody] List<CycsInAddInfoInput> input)
    {
        if (input.IsAny())
        {
            // 判断是否已经上报
            var ids = input.Select(x => x.buyId).ToList();
            if (await _repository.Context.Queryable<ErpCycsInEntity>().AnyAsync(x => ids.Contains(x.buyId)))
            {
                throw Oops.Oh("采购单已上报！");
            }
            //3、上报进货记录
            var response = await _httpProxy.tCycsInAdd(input.Adapt<List<CycsInAddInfo>>());

            if (response.code == 0)
            {
                //4、写入本地表
                await _repository.Context.Insertable(input.Adapt<List<ErpCycsInEntity>>()).ExecuteCommandAsync();
            }
            else
            {
                throw Oops.Oh(response.msg);
            }
        }
      

    }


    [HttpGet("tCycsDivision/list")]
    public async Task<List<tCycsDivisionInfo>> GetDivision()
    {
        var rep = await _httpProxy.tCycsDivision();

        return rep?.data ?? new List<tCycsDivisionInfo>();
    }


    [HttpGet("tCycgoodsType/list")]
    public async Task<List<tCycgoodsTypeInfo>> GetCycgoodsType()
    {
        var rep = await _httpProxy.tCycgoodsType();

        return rep?.data ?? new List<tCycgoodsTypeInfo>();
    }

    [HttpGet("tCycsGoodsList/list")]
    public async Task<List<CycsGoodsInfo>> GetCycsGoodsList()
    {
        var cycsId = await _repository.Context.Queryable<ErpCycsOrganizeEntity>().Where(x => x.OId == _userManager.CompanyId).Select(x => x.CycsId).FirstAsync();
        var rep = await _httpProxy.tCycsGoodsList(cycsId);

        return rep?.data ?? new List<CycsGoodsInfo>();
    }


    [HttpGet("tCycsSupplier/list")]
    public async Task<List<CycsSupplierInfo>> GetCycsSupplier()
    {
        var cycsId = await _repository.Context.Queryable<ErpCycsOrganizeEntity>().Where(x => x.OId == _userManager.CompanyId).Select(x => x.CycsId).FirstAsync();
        var rep = await _httpProxy.tCycsSupplier(cycsId);

        return rep?.data ?? new List<CycsSupplierInfo>();
    }
}
