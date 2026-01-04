using Mapster;
using Microsoft.AspNetCore.Mvc;
using NPOI.Util;
using QT.Asset.Dto.AssetAttributeDefinition;
using QT.Asset.Dto.AssetInventory;
using QT.Asset.Entity;
using QT.Common.Core;
using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Dtos.DataBase;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.Systems.Entitys.Permission;
using SqlSugar;

namespace QT.Asset;

/// <summary>
/// 资产盘点服务
/// </summary>
[ApiDescriptionSettings("资产管理", Tag = "盘点管理", Name = "AssetInventoryTask", Order = 610)]
[Route("api/extend/asset/[controller]")]
public class AssetInventoryTaskService : QTBaseService<AssetInventoryTaskEntity, AssetInventoryTaskCrInput, AssetInventoryTaskUpInput, AssetInventoryTaskInfoOutput, AssetInventoryTaskListPageInput, AssetInventoryTaskListOutput>, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<AssetInventoryTaskEntity> _repository;
    private readonly AssetChangeLogService _assetChangeLogService;

    public AssetInventoryTaskService(
        ISqlSugarRepository<AssetInventoryTaskEntity> repository,
        ISqlSugarClient context,
        IUserManager userManager, AssetChangeLogService assetChangeLogService)
        : base(repository, context, userManager)
    {
        _repository = repository;
        _assetChangeLogService = assetChangeLogService;
    }

    public override async Task<PageResult<AssetInventoryTaskListOutput>> GetList([FromQuery] AssetInventoryTaskListPageInput input)
    {
        var data = await _repository.Context.Queryable<AssetInventoryTaskEntity>()
            .Select<AssetInventoryTaskListOutput>(it => new AssetInventoryTaskListOutput
            {
                // 自定义输出字段
                creatorUserName = SqlFunc.Subqueryable<UserEntity>().Where(x => x.Id == it.CreatorUserId).Select(x => x.RealName),
            }, true)
            .ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<AssetInventoryTaskListOutput>.SqlSugarPagedList(data);
    }

    public override async Task<AssetInventoryTaskInfoOutput> GetInfo(string id)
    {
        var entity = await base.GetInfo(id);

        entity.assetInventoryRecordEntitys = await _repository.Context.Queryable<AssetInventoryRecordEntity>()
            .LeftJoin<AssetEntity>((x, a) => x.AssetId == a.Id)
            .Where(x => x.TaskId == id)
            .Select((x, a) => new AssetInventoryRecordDto
            {
                assetCode = a.AssetCode,
                assetId = x.AssetId,
                assetName = a.Name,
                id = x.Id,
                location = a.Location,
                status = x.Status,
                taskId = x.TaskId,
                categoryName = SqlFunc.Subqueryable<AssetCategoryEntity>().Where(d => d.Id == a.CategoryId).Select(d => d.Name),
                dutyUserName = SqlFunc.Subqueryable<UserEntity>().Where(d => d.Id == a.DutyUserId).Select(d => d.RealName),
                barcode = a.Barcode
                //remark = x.Remark
            })
            .ToListAsync();
        return entity;
    }

    protected override async Task BeforeCreate(AssetInventoryTaskCrInput input, AssetInventoryTaskEntity entity)
    {
        if (input.assetInventoryRecordEntitys.IsAny())
        {
            var assetIds = input.assetInventoryRecordEntitys.Select(x => x.assetId).ToList();

            // 获取资产集合
            var assets = assetIds.IsAny() ? await _repository.Context.Queryable<AssetEntity>().Where(it => assetIds.Contains(it.Id)).ToListAsync() : new List<AssetEntity>();

            List<AssetEntity> assetChangeEntities = new List<AssetEntity>();

            var details = input.assetInventoryRecordEntitys.Adapt<List<AssetInventoryRecordEntity>>();
            details.ForEach(it =>
            {
                it.TaskId = entity.Id;
                it.Id = SnowflakeIdHelper.NextId().ToString();

                var asset = assets.FirstOrDefault(x => x.Id == it.AssetId);

                if (asset != null)
                {
                    _repository.Context.Tracking(asset);
                    asset.Status = it.Status;
                    assetChangeEntities.Add(asset);
                }
            });
            await _repository.Context.Insertable<AssetInventoryRecordEntity>(details).ExecuteCommandAsync();

            await AssetLogAsync(entity, assetChangeEntities);
        }
    }

    protected override async Task AfterUpdate(AssetInventoryTaskUpInput input, AssetInventoryTaskEntity entity)
    {
        var assetIds = input.assetInventoryRecordEntitys.Select(x => x.assetId).ToList();

        // 获取资产集合
        var assets = assetIds.IsAny() ? await _repository.Context.Queryable<AssetEntity>().Where(it => assetIds.Contains(it.Id)).ToListAsync() : new List<AssetEntity>();

        List<AssetEntity> assetChangeEntities = new List<AssetEntity>();

        await _repository.Context.CUDSaveAsnyc<AssetInventoryRecordEntity, AssetInventoryRecordDto>(it => it.TaskId == entity.Id, input.assetInventoryRecordEntitys, onAdd: (it) =>
        {
            it.TaskId = entity.Id;

            var asset = assets.FirstOrDefault(x => x.Id == it.AssetId);

            if (asset != null)
            {
                _repository.Context.Tracking(asset);
                asset.Status = it.Status;
                assetChangeEntities.Add(asset);
            }


        }, onUpdate: (it) =>
        {
            var asset = assets.FirstOrDefault(x => x.Id == it.AssetId);

            if (asset != null)
            {
                _repository.Context.Tracking(asset);
                asset.Status = it.Status;
                assetChangeEntities.Add(asset);
            }
        }, onDelete: (it) =>
        {
            // 删除的话，可能更新为原来的
        });

        await AssetLogAsync(entity, assetChangeEntities);
    }

    private async Task AssetLogAsync(AssetInventoryTaskEntity entity, List<AssetEntity> assetChangeEntities)
    {
        Dictionary<string, List<FieldChangeDto>> changeDtos = new Dictionary<string, List<FieldChangeDto>>();
        if (assetChangeEntities.IsAny())
        {
            foreach (var asset in assetChangeEntities)
            {
                var changes = _repository.Context.GetChangeFields(asset);

                if (changes.IsAny())
                {
                    changes.ForEach(x => x.tableName = nameof(AssetInventoryRecordEntity));
                    changeDtos.Add(asset.Id, changes);
                }
            }

            // 更新资产信息
            await _repository.Context.Updateable<AssetEntity>(assetChangeEntities).ExecuteCommandAsync();
        }


        // 记录日志
        if (changeDtos.IsAny())
        {
            //await _assetChangeLogService.RemoveAssetChangesAsync(entity.Id);
            await _assetChangeLogService.LogAssetChangesAsync(changeDtos, _userManager.UserId, _userManager.RealName, entity.Id, "资产盘点");
        }
    }
}
