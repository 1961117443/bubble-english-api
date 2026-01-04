using Microsoft.AspNetCore.Mvc;
using QT.Asset.Dto.AssetCategory;
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
[ApiDescriptionSettings("资产管理", Tag = "维修管理", Name = "AssetRepair", Order = 608)]
[Route("api/extend/asset/[controller]")]
public class AssetRepairService : QTBaseService<AssetRepairEntity, AssetRepairCrInput, AssetRepairUpInput, AssetRepairInfoOutput, AssetRepairListPageInput, AssetRepairListOutput>, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<AssetRepairEntity> _repository;
    private readonly AssetService _assetService;
    private readonly AssetChangeLogService _assetChangeLogService;

    public AssetRepairService(
        ISqlSugarRepository<AssetRepairEntity> repository,
        ISqlSugarClient context,
        IUserManager userManager, AssetService assetService, AssetChangeLogService assetChangeLogService)
        : base(repository, context, userManager)
    {
        _repository = repository;
        _assetService = assetService;
        _assetChangeLogService = assetChangeLogService;
    }

    public override async Task<PageResult<AssetRepairListOutput>> GetList([FromQuery] AssetRepairListPageInput input)
    {
        var data = await _repository.Context.Queryable<AssetRepairEntity>()
            .WhereIF(input.assetId.IsNotEmptyOrNull(), it => it.AssetId == input.assetId)
            .Select<AssetRepairListOutput>(it => new AssetRepairListOutput
            {
                // 自定义输出字段
                assetCode = SqlFunc.Subqueryable<AssetEntity>().Where(c => c.Id == it.AssetId)
                    .Select(c => c.AssetCode),
                assetName = SqlFunc.Subqueryable<AssetEntity>().Where(c => c.Id == it.AssetId)
                    .Select(c => c.Name),
                barcode = SqlFunc.Subqueryable<AssetEntity>().Where(c => c.Id == it.AssetId)
                    .Select(c => c.Barcode),
                reportUserName = SqlFunc.Subqueryable<UserEntity>().Where(c => c.Id == it.ReportUserId)
                    .Select(c => c.RealName),
            }, true)
            .ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<AssetRepairListOutput>.SqlSugarPagedList(data);
    }

    public override async Task<AssetRepairInfoOutput> GetInfo(string id)
    {
        var entity = await base.GetInfo(id);

        entity.assetInfo = await _assetService.GetInfo(entity.assetId);

        return entity;
    }

    protected override async Task AfterCreate(AssetRepairEntity entity)
    {
        await base.AfterCreate(entity);

        // 更新资产状态为在用
        var assetEntity = await _repository.Context.Queryable<AssetEntity>().SingleAsync(x => x.Id == entity.AssetId);
        _repository.Context.Tracking(assetEntity);
        assetEntity.Status = AssetStatus.Repairing;

        var changes = _repository.Context.GetChangeFields(assetEntity);

        await _repository.Context.Updateable<AssetEntity>(assetEntity).UpdateColumns(x => new { x.Status, x.UserId }).ExecuteCommandAsync();

        // 记录日志
        if (changes.IsAny())
        {
            changes.ForEach(x => x.tableName = nameof(AssetRepairEntity));
            await _assetChangeLogService.LogAssetChangesAsync(assetEntity.Id, changes, _userManager.UserId, _userManager.RealName, entity.Id, "资产维修");
        }

    }
}
