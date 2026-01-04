using Mapster;
using Microsoft.AspNetCore.Mvc;
using QT.Asset.Dto.AssetInventory;
using QT.Asset.Dto.AssetTransfer;
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
using System.Threading.Channels;

namespace QT.Asset;

/// <summary>
/// 资产调拨服务
/// </summary>
[ApiDescriptionSettings("资产管理", Tag = "调拨管理", Name = "AssetTransferTask", Order = 609)]
[Route("api/extend/asset/[controller]")]
public class AssetTransferTaskService : QTBaseService<AssetTransferTaskEntity, AssetTransferTaskCrInput, AssetTransferTaskUpInput, AssetTransferTaskDto, AssetTransferTaskListPageInput, AssetTransferTaskListOutput>, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<AssetTransferTaskEntity> _repository;
    private readonly AssetChangeLogService _assetChangeLogService;

    public AssetTransferTaskService(
        ISqlSugarRepository<AssetTransferTaskEntity> repository,
        ISqlSugarClient context,
        IUserManager userManager, AssetChangeLogService assetChangeLogService)
        : base(repository, context, userManager)
    {
        _repository = repository;
        _assetChangeLogService = assetChangeLogService;
    }

    public override async Task<PageResult<AssetTransferTaskListOutput>> GetList([FromQuery] AssetTransferTaskListPageInput input)
    {
        var data = await _repository.Context.Queryable<AssetTransferTaskEntity>()
            .Select<AssetTransferTaskListOutput>(it => new AssetTransferTaskListOutput
            {
                // 自定义输出字段
            }, true)
            .ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<AssetTransferTaskListOutput>.SqlSugarPagedList(data);
    }

    public override async Task<AssetTransferTaskDto> GetInfo(string id)
    {
        var entity = await base.GetInfo(id);

        entity.assetTransferDetailEntitys = await _repository.Context.Queryable<AssetTransferDetailEntity>()
            .LeftJoin<AssetEntity>((x, a) => x.AssetId == a.Id)
            .Where(x => x.TaskId == id)
            .Select((x, a) => new AssetTransferDetailDto
            {
                assetCode = a.AssetCode,
                assetId = x.AssetId,
                assetName = a.Name,
                id = x.Id,
                newDepartmentId = x.NewDepartmentId,
                newDepartmentName = SqlFunc.Subqueryable<OrganizeEntity>().Where(d => d.Id == x.NewDepartmentId).Select(d => d.FullName),
                newDutyUserId = x.NewDutyUserId,
                newDutyUserName = SqlFunc.Subqueryable<UserEntity>().Where(d => d.Id == x.NewDutyUserId).Select(d => d.RealName),
                newUserId = x.NewUserId,
                newUserName = SqlFunc.Subqueryable<UserEntity>().Where(d => d.Id == x.NewUserId).Select(d => d.RealName),
                newWarehouseId = x.NewWarehouseId,
                newWarehouseName = SqlFunc.Subqueryable<AssetWarehouseEntity>().Where(d => d.Id == x.NewWarehouseId).Select(d => d.Name),
                newLocation = x.NewLocation,
                oldDepartmentId = x.OldDepartmentId,
                oldDepartmentName = SqlFunc.Subqueryable<OrganizeEntity>().Where(d => d.Id == x.OldDepartmentId).Select(d => d.FullName),
                oldDutyUserId = x.OldDutyUserId,
                oldDutyUserName = SqlFunc.Subqueryable<UserEntity>().Where(d => d.Id == x.OldDutyUserId).Select(d => d.RealName),
                oldUserId = x.OldUserId,
                oldUserName = SqlFunc.Subqueryable<UserEntity>().Where(d => d.Id == x.OldUserId).Select(d => d.RealName),
                oldWarehouseId = x.OldWarehouseId,
                oldWarehouseName = SqlFunc.Subqueryable<AssetWarehouseEntity>().Where(d => d.Id == x.OldWarehouseId).Select(d => d.Name),
                oldLocation = x.OldLocation,
                barcode = a.Barcode
            })
            .ToListAsync();
        return entity;
    }


    protected override async Task BeforeCreate(AssetTransferTaskCrInput input, AssetTransferTaskEntity entity)
    {
        if (input.assetTransferDetailEntitys.IsAny())
        {
            var assetIds = input.assetTransferDetailEntitys.Select(x => x.assetId).ToList();

            // 获取资产集合
            var assets = assetIds.IsAny() ? await _repository.Context.Queryable<AssetEntity>().Where(it => assetIds.Contains(it.Id)).ToListAsync() : new List<AssetEntity>();

            List<AssetEntity> assetChangeEntities = new List<AssetEntity>();

            var details = input.assetTransferDetailEntitys.Adapt<List<AssetTransferDetailEntity>>();
            details.ForEach(it =>
            {
                it.TaskId = entity.Id;
                it.Id = SnowflakeIdHelper.NextId().ToString();

                var asset = assets.FirstOrDefault(x => x.Id == it.AssetId);

                if (asset != null)
                {
                    _repository.Context.Tracking(asset);
                    asset.DeptId = it.NewDepartmentId;
                    asset.DutyUserId = it.NewDutyUserId;
                    asset.UserId = it.NewUserId;
                    asset.WarehouseId = it.NewWarehouseId;
                    asset.Location = it.NewLocation;
                    assetChangeEntities.Add(asset);
                }
            });
            await _repository.Context.Insertable<AssetTransferDetailEntity>(details).ExecuteCommandAsync();


            await AssetLogAsync(entity, assetChangeEntities);
        }
    }

    protected override async Task AfterUpdate(AssetTransferTaskUpInput input, AssetTransferTaskEntity entity)
    {

        //newDepartmentId = x.NewDepartmentId,
        //        newDutyUserId = x.NewDutyUserId,
        //        newUserId = x.NewUserId,
        //        newWarehouseId = x.NewWarehouseId,
        //        newLocation = x.NewLocation,
        var assetIds = input.assetTransferDetailEntitys.Select(x => x.assetId).ToList();

        // 获取资产集合
        var assets = assetIds.IsAny() ? await _repository.Context.Queryable<AssetEntity>().Where(it => assetIds.Contains(it.Id)).ToListAsync() : new List<AssetEntity>();

        List<AssetEntity> assetChangeEntities = new List<AssetEntity>();

        await _repository.Context.CUDSaveAsnyc<AssetTransferDetailEntity, AssetTransferDetailDto>(it => it.TaskId == entity.Id, input.assetTransferDetailEntitys, onAdd: (it) =>
        {
            it.TaskId = entity.Id;

            var asset = assets.FirstOrDefault(x => x.Id == it.AssetId);

            if (asset != null)
            {
                _repository.Context.Tracking(asset);
                asset.DeptId = it.NewDepartmentId;
                asset.DutyUserId = it.NewDutyUserId;
                asset.UserId = it.NewUserId;
                asset.WarehouseId = it.NewWarehouseId;
                asset.Location = it.NewLocation;
                assetChangeEntities.Add(asset);
            }


        }, onUpdate: (it) =>
        {
            var asset = assets.FirstOrDefault(x => x.Id == it.AssetId);

            if (asset != null)
            {
                _repository.Context.Tracking(asset);
                asset.DeptId = it.NewDepartmentId;
                asset.DutyUserId = it.NewDutyUserId;
                asset.UserId = it.NewUserId;
                asset.WarehouseId = it.NewWarehouseId;
                asset.Location = it.NewLocation;
                assetChangeEntities.Add(asset);
            }
        }, onDelete: (it) =>
        {
            // 删除的话，可能更新为原来的
        });

        await AssetLogAsync(entity, assetChangeEntities);

    }

    private async Task AssetLogAsync(AssetTransferTaskEntity entity, List<AssetEntity> assetChangeEntities)
    {
        Dictionary<string, List<FieldChangeDto>> changeDtos = new Dictionary<string, List<FieldChangeDto>>();
        if (assetChangeEntities.IsAny())
        {
            foreach (var asset in assetChangeEntities)
            {
                var changes = _repository.Context.GetChangeFields(asset);

                if (changes.IsAny())
                {
                    changes.ForEach(x => x.tableName = nameof(AssetTransferDetailEntity));
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
            await _assetChangeLogService.LogAssetChangesAsync(changeDtos, _userManager.UserId, _userManager.RealName, entity.Id, "资产变更");
        }
    }
}
