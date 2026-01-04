using COSXML.Model.Tag;
using Microsoft.AspNetCore.Mvc;
using Minio.DataModel;
using NPOI.SS.Formula.Functions;
using QT.Asset.Dto.AssetBarcode;
using QT.Asset.Entity;
using QT.Common.Core;
using QT.Common.Core.Manager;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using Spire.Barcode;
using SqlSugar;

namespace QT.Asset;

/// <summary>
/// 条码规则服务
/// </summary>
[ApiDescriptionSettings("资产管理", Tag = "条码管理", Name = "AssetBarcode", Order = 605)]
[Route("api/extend/asset/[controller]")]
public class AssetBarcodeService : IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<AssetBarcodeEntity> _repository;

    public AssetBarcodeService(
        ISqlSugarRepository<AssetBarcodeEntity> repository,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = repository;
    }

    [HttpGet("")]
    public async Task<PageResult<AssetBarcodeDto>> GetList([FromQuery] AssetBarcodeListPageInput input)
    {
        var data = await _repository.Context.Queryable<AssetBarcodeEntity>()
            .WhereIF(input.keyword.IsNotEmptyOrNull(),it=>it.BarcodeNumber.Contains(input.keyword))
            .WhereIF(input.status.HasValue,it=>it.Status ==  input.status)
            .Select<AssetBarcodeDto>(it => new AssetBarcodeDto
            {
                // 自定义输出字段
                ruleName = SqlFunc.Subqueryable<AssetBarcodeRuleEntity>().Where(x=>x.Id == it.RuleId).Select(x=>x.RuleName)
            }, true)
            .ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<AssetBarcodeDto>.SqlSugarPagedList(data);
    }


    /// <summary>
    /// 条码绑定
    /// </summary>
    [HttpPost("Actions/Bind")]
    public async Task BarcodeBindAsync([FromBody] AssetBarcodeBindInput input)
    {
        var entity = await _repository.AsQueryable().SingleAsync(x => x.BarcodeNumber == input.barcodeNumber) ?? throw Oops.Oh("条码不存在");

        if (entity.Status == AssetBarcodeStatus.Unbound)
        {
            // 判断bindId 是否已经绑定了条码
            if (await _repository.AsQueryable().AnyAsync(x => x.BindId == input.bindId))
            {
                throw Oops.Oh("绑定对象已存在条码，不允许重复绑定！");
            }


            _repository.Context.Tracking(entity);
            entity.BindId = input.bindId;
            entity.BindTime = DateTime.Now;
            entity.Status = AssetBarcodeStatus.Bound;
            entity.BindType = input.bindType;
            entity.LastModify();

            await _repository.AutoUpdateAsync(entity);
        }
        else
        {
            throw Oops.Oh("条码状态异常");
        }
    }


    /// <summary>
    /// 条码查询
    /// </summary>
    [HttpGet("Actions/Query")]
    public async Task<dynamic> BarcodeQuery([FromQuery] string barcode)
    {
        var entity = await _repository.AsQueryable().SingleAsync(x => x.BarcodeNumber == barcode) ?? throw Oops.Oh("条码不存在");

        //if (entity.Status == AssetBarcodeStatus.Unbound)
        //{
        //    _repository.Context.Tracking(entity);
        //    entity.BindId = input.bindId;
        //    entity.BindTime = DateTime.Now;
        //    entity.Status = AssetBarcodeStatus.Bound;
        //    entity.BindType = input.bindType;
        //    entity.LastModify();

        //    await _repository.AutoUpdateAsync(entity);
        //}

        throw Oops.Oh("条码状态异常");
    }
}
