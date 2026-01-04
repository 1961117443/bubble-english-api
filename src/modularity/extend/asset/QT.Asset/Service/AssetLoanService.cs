using Microsoft.AspNetCore.Mvc;
using QT.Asset.Dto.AssetLoan;
using QT.Asset.Entity;
using QT.Common.Core;
using QT.Common.Core.Manager;
using QT.Common.Dtos.DataBase;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.Systems.Entitys.Permission;
using Senparc.Weixin.Work.AdvancedAPIs.MailList.Member;
using SqlSugar;

namespace QT.Asset;

/// <summary>
/// 资产领用服务
/// </summary>
[ApiDescriptionSettings("资产管理", Tag = "领用管理", Name = "AssetLoan", Order = 607)]
[Route("api/extend/asset/[controller]")]
public class AssetLoanService : QTBaseService<AssetLoanEntity, AssetLoanCrInput, AssetLoanUpInput, AssetLoanInfoOutput, AssetLoanListPageInput, AssetLoanListOutput>, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<AssetLoanEntity> _repository;
    private readonly AssetService _assetService;
    private readonly AssetChangeLogService _assetChangeLogService;

    public AssetLoanService(
        ISqlSugarRepository<AssetLoanEntity> repository,
        ISqlSugarClient context,
        IUserManager userManager,
        AssetService assetService, AssetChangeLogService assetChangeLogService)
        : base(repository, context, userManager)
    {
        _repository = repository;
        _assetService = assetService;
        _assetChangeLogService = assetChangeLogService;
    }

    public override async Task<PageResult<AssetLoanListOutput>> GetList([FromQuery] AssetLoanListPageInput input)
    {
        var data = await _repository.Context.Queryable<AssetLoanEntity>()
            .WhereIF(input.assetId.IsNotEmptyOrNull(), it => it.AssetId == input.assetId)
            .Select<AssetLoanListOutput>(it => new AssetLoanListOutput
            {
                // 自定义输出字段
                assetCode = SqlFunc.Subqueryable<AssetEntity>().Where(c => c.Id == it.AssetId)
                    .Select(c => c.AssetCode),
                assetName = SqlFunc.Subqueryable<AssetEntity>().Where(c => c.Id == it.AssetId)
                    .Select(c => c.Name),
                barcode = SqlFunc.Subqueryable<AssetEntity>().Where(c => c.Id == it.AssetId)
                    .Select(c => c.Barcode),
                userName = SqlFunc.Subqueryable<UserEntity>().Where(c => c.Id == it.UserId)
                    .Select(c => c.RealName),
            }, true)
            .ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<AssetLoanListOutput>.SqlSugarPagedList(data);
    }

    public override async Task<AssetLoanInfoOutput> GetInfo(string id)
    {
        var entity = await base.GetInfo(id);

        entity.assetInfo = await _assetService.GetInfo(entity.assetId);

        return entity;
    }

    protected override async Task AfterCreate(AssetLoanEntity entity)
    {
        await base.AfterCreate(entity);

        // 更新资产状态为在用
        var assetEntity = await _repository.Context.Queryable<AssetEntity>().SingleAsync(x => x.Id == entity.AssetId);
        _repository.Context.Tracking(assetEntity);
        assetEntity.Status = AssetStatus.InUse;
        assetEntity.UserId = entity.UserId;

        var changes =  _repository.Context.GetChangeFields(assetEntity);

        await _repository.Context.Updateable<AssetEntity>(assetEntity).UpdateColumns(x => new {x.Status,x.UserId}).ExecuteCommandAsync();

        // 记录日志
        if (changes.IsAny())
        {
            changes.ForEach(x => x.tableName = nameof(AssetLoanEntity)); 
            await _assetChangeLogService.LogAssetChangesAsync(assetEntity.Id, changes, _userManager.UserId, _userManager.RealName, entity.Id, "资产领用");
        }
        
    }
}
