using Microsoft.AspNetCore.Mvc;
using QT.Asset.Dto.AssetInventory;
using QT.Asset.Dto.AssetTransfer;
using QT.Asset.Entity;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.DynamicApiController;
using QT.Systems.Entitys.Permission;
using SqlSugar;

namespace QT.Asset;

/// <summary>
/// 资产盘点明细服务
/// </summary>
[ApiDescriptionSettings("资产管理", Tag = "盘点管理", Name = "AssetInventoryDetail", Order = 609)]
[Route("api/extend/asset/[controller]")]
public class AssetInventoryDetailService : IDynamicApiController
{
    private readonly ISqlSugarRepository<AssetTransferDetailEntity> _repository;

    public AssetInventoryDetailService(ISqlSugarRepository<AssetTransferDetailEntity> repository)
    {
        _repository = repository;
    }

    [HttpGet("")]
    public async Task<PageResult<AssetInventoryRecordDto>> GetList([FromQuery] AssetInventoryTaskListPageInput input)
    {
        var data = await _repository.Context.Queryable<AssetInventoryRecordEntity>()
            .LeftJoin<AssetEntity>((x, a) => x.AssetId == a.Id)
            .WhereIF(input.assetId.IsNotEmptyOrNull(), (x, a) => x.AssetId == input.assetId)
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
                barcode = a.Barcode,
                inventoryTime = SqlFunc.Subqueryable<AssetInventoryTaskEntity>().Where(d => d.Id ==x.TaskId).Select(d => d.InventoryTime),
                //remark = x.Remark
            })
            .MergeTable()
            .OrderBy(it=>it.inventoryTime)
            .ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<AssetInventoryRecordDto>.SqlSugarPagedList(data);
    }
}

