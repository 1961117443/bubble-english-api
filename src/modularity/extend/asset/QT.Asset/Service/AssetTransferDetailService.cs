using Microsoft.AspNetCore.Mvc;
using QT.Asset.Dto.AssetTransfer;
using QT.Asset.Entity;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.DynamicApiController;
using QT.Systems.Entitys.Permission;
using SqlSugar;

namespace QT.Asset;

/// <summary>
/// 资产调拨明细服务
/// </summary>
[ApiDescriptionSettings("资产管理", Tag = "调拨管理", Name = "AssetTransferDetail", Order = 609)]
[Route("api/extend/asset/[controller]")]
public class AssetTransferDetailService: IDynamicApiController
{
    private readonly ISqlSugarRepository<AssetTransferDetailEntity> _repository;

    public AssetTransferDetailService(ISqlSugarRepository<AssetTransferDetailEntity> repository)
    {
        _repository = repository;
    }

    [HttpGet("")]
    public async Task<PageResult<AssetTransferDetailDto>> GetList([FromQuery] AssetTransferTaskListPageInput input)
    {
        var data = await _repository.Context.Queryable<AssetTransferDetailEntity>()
            .LeftJoin<AssetEntity>((x, a) => x.AssetId == a.Id)
            .WhereIF(input.assetId.IsNotEmptyOrNull(), (x,a)=>x.AssetId == input.assetId)
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
                barcode = a.Barcode,
                transferTime = SqlFunc.Subqueryable<AssetTransferTaskEntity>().Where(d=>d.Id == x.TaskId).Select(d => d.TransferTime),
            })
            .MergeTable()
            .OrderBy(it => it.transferTime)
            .ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<AssetTransferDetailDto>.SqlSugarPagedList(data);
    }
}

