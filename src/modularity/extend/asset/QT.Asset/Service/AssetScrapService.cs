using Microsoft.AspNetCore.Mvc;
using QT.Asset.Dto.AssetCategory;
using QT.Asset.Dto.AssetScrap;
using QT.Asset.Entity;
using QT.Common.Core;
using QT.Common.Core.Manager;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.Systems.Entitys.Permission;
using SqlSugar;

namespace QT.Asset;

/// <summary>
/// 资产维修服务
/// </summary>
[ApiDescriptionSettings("资产管理", Tag = "资产报废", Name = "AssetScrap", Order = 608)]
[Route("api/extend/asset/[controller]")]
public class AssetScrapService : QTBaseService<AssetScrapEntity, AssetScrapCrInput, AssetScrapUpInput, AssetScrapInfoOutput, AssetScrapListPageInput, AssetScrapListOutput>, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<AssetScrapEntity> _repository;
    private readonly AssetService _assetService;
    private readonly AssetChangeLogService _assetChangeLogService;

    public AssetScrapService(
        ISqlSugarRepository<AssetScrapEntity> repository,
        ISqlSugarClient context,
        IUserManager userManager, AssetService assetService, AssetChangeLogService assetChangeLogService)
        : base(repository, context, userManager)
    {
        _repository = repository;
        _assetService = assetService;
        _assetChangeLogService = assetChangeLogService;
    }

    public override async Task<PageResult<AssetScrapListOutput>> GetList([FromQuery] AssetScrapListPageInput input)
    {
        var data = await _repository.Context.Queryable<AssetScrapEntity>()
            .WhereIF(input.assetId.IsNotEmptyOrNull(),it=>it.AssetId == input.assetId)
            .Select<AssetScrapListOutput>(it => new AssetScrapListOutput
            {
                // 自定义输出字段
                assetCode = SqlFunc.Subqueryable<AssetEntity>().Where(c => c.Id == it.AssetId)
                    .Select(c => c.AssetCode),
                assetName = SqlFunc.Subqueryable<AssetEntity>().Where(c => c.Id == it.AssetId)
                    .Select(c => c.Name),
                barcode = SqlFunc.Subqueryable<AssetEntity>().Where(c => c.Id == it.AssetId)
                    .Select(c => c.Barcode),
            }, true)
            .ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<AssetScrapListOutput>.SqlSugarPagedList(data);
    }

    public override async Task<AssetScrapInfoOutput> GetInfo(string id)
    {
        var entity = await base.GetInfo(id);

        entity.assetInfo = await _assetService.GetInfo(entity.assetId);

        return entity;
    }

    protected override async Task AfterCreate(AssetScrapEntity entity)
    {
        await base.AfterCreate(entity);

        // 更新资产状态为在用
        var assetEntity = await _repository.Context.Queryable<AssetEntity>().SingleAsync(x => x.Id == entity.AssetId);
        _repository.Context.Tracking(assetEntity);
        assetEntity.Status = AssetStatus.Scrapped;

        var changes = _repository.Context.GetChangeFields(assetEntity);

        await _repository.Context.Updateable<AssetEntity>(assetEntity).UpdateColumns(x => new { x.Status, x.UserId }).ExecuteCommandAsync();

        // 记录日志
        if (changes.IsAny())
        {
            changes.ForEach(x => x.tableName = nameof(AssetScrapEntity));
            await _assetChangeLogService.LogAssetChangesAsync(assetEntity.Id, changes, _userManager.UserId, _userManager.RealName, entity.Id, "资产报废");
        }

    }
}
