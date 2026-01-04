using Mapster;
using Microsoft.AspNetCore.Mvc;
using QT.Asset.Dto.Asset;
using QT.Asset.Dto.AssetAttributeDefinition;
using QT.Asset.Dto.AssetBarcode;
using QT.Asset.Dto.AssetCategory;
using QT.Asset.Entity;
using QT.Common.Core;
using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Dtos.DataBase;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Systems.Entitys.Permission;
using QT.Systems.Interfaces.System;
using SqlSugar;

namespace QT.Asset;

/// <summary>
/// 资产服务
/// </summary>
[ApiDescriptionSettings("资产管理", Tag = "资产", Name = "Asset", Order = 602)]
[Route("api/extend/asset/[controller]")]
public class AssetService : QTBaseService<AssetEntity, AssetCrInput, AssetUpInput, AssetInfoOutput, AssetListPageInput, AssetListOutput>, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<AssetEntity> _repository;
    private readonly IBillRullService _billRullService;
    private readonly AssetChangeLogService _assetChangeLogService;

    public AssetService(
        ISqlSugarRepository<AssetEntity> repository,
        ISqlSugarClient context,
        IUserManager userManager, IBillRullService billRullService, 
        AssetChangeLogService assetChangeLogService)
        : base(repository, context, userManager)
    {
        _repository = repository;
        _billRullService = billRullService;
        _assetChangeLogService = assetChangeLogService;
    }

    public override async Task<PageResult<AssetListOutput>> GetList([FromQuery] AssetListPageInput input)
    {
        List<string> categoryList = new List<string>();
        if (input.categoryId.IsNotEmptyOrNull())
        {
            var categorys = await _repository.Context.Queryable<AssetCategoryEntity>().ToChildListAsync(x => x.ParentId, input.categoryId);
            categoryList = categorys.Select(x => x.Id).ToList();
        }

        var data = await _repository.Context.Queryable<AssetEntity>()
            .WhereIF(input.code.IsNotEmptyOrNull(), it => it.AssetCode.Contains(input.code))
            .WhereIF(input.name.IsNotEmptyOrNull(), it => it.Name.Contains(input.name))
            .WhereIF(input.status.HasValue, it => it.Status == input.status)
            .WhereIF(categoryList.IsAny(), it => categoryList.Contains(it.CategoryId))
            .Select<AssetListOutput>(it => new AssetListOutput
            {
                // 自定义输出字段
                categoryName = SqlFunc.Subqueryable<AssetCategoryEntity>().Where(c => c.Id == it.CategoryId)
                    .Select(c => c.Name),
                dutyUserName = SqlFunc.Subqueryable<UserEntity>().Where(u => u.Id == it.DutyUserId).Select(u => u.RealName)
            }, true)
            .ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<AssetListOutput>.SqlSugarPagedList(data);
    }


    public override async Task<AssetInfoOutput> GetInfo(string id)
    {
        var entity = await _repository.Context.Queryable<AssetEntity>().Where(x => x.Id == id)
            .Select(x => new AssetInfoOutput
            {
                deptName = SqlFunc.Subqueryable<OrganizeEntity>().Where(d => d.Id == x.DeptId).Select(d => d.FullName),
                dutyUserName = SqlFunc.Subqueryable<UserEntity>().Where(d => d.Id == x.DutyUserId).Select(d => d.RealName),
                userName = SqlFunc.Subqueryable<UserEntity>().Where(d => d.Id == x.UserId).Select(d => d.RealName),
                warehouseName = SqlFunc.Subqueryable<AssetWarehouseEntity>().Where(d => d.Id == x.WarehouseId).Select(d => d.Name),
            }, true)
            .FirstAsync();
        ;

        var list = await _repository.Context.Queryable<AssetAttributeValueEntity>().Where(x => x.AssetId == id).ToListAsync();

        entity.assetFields = list.Adapt<List<AssetAttributeValueDto>>();

        return entity;
    }

    protected override async Task BeforeCreate(AssetCrInput input, AssetEntity entity)
    {
        if (entity.AssetCode.IsNullOrEmpty())
        {
            // 如果传入了资产编号，则不自动生成
            entity.AssetCode = await _billRullService.GetBillNumber("AssetCode");
        }
        if (input.assetFields.IsAny())
        {
            var fields = input.assetFields.Adapt<List<AssetAttributeValueEntity>>();
            fields.ForEach(it =>
            {
                it.AssetId = entity.Id;
                it.Id = SnowflakeIdHelper.NextId().ToString();
            });
            await _repository.Context.Insertable<AssetAttributeValueEntity>(fields).ExecuteCommandAsync();
        }
    }

    /// <summary>
    /// 更新.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [SqlSugarUnitOfWork]
    public override async Task Update(string id, [FromBody] AssetUpInput input)
    {
        var entity = await _repository.SingleAsync(it => it.Id == id) ?? throw Oops.Oh(ErrorCode.COM1005);

        var assetAttributeDefinitionEntitys = await _repository.Context.Queryable<AssetAttributeDefinitionEntity>().Where(x => x.CategoryId == input.categoryId || x.CategoryId == entity.CategoryId).ToListAsync();

        _repository.Context.Tracking(entity);

        await this.BeforeUpdate(input, entity);

        input.Adapt(entity);

        // 校验唯一
        var check = await ValidateEntity(entity, false);
        if (!check.Item1)
        {
            throw Oops.Oh(check.Item2);
        }

        var changes = _repository.Context.GetChangeFields(entity);


        var isOk = await _repository.Context.Updateable<AssetEntity>(entity).ExecuteCommandAsync();
        //if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);

        await _repository.Context.CUDSaveAsnyc<AssetAttributeValueEntity, AssetAttributeValueDto>(it => it.AssetId == entity.Id, input.assetFields,
            onAdd: attr =>
        {
            attr.AssetId = entity.Id;
           
            changes.Add(new FieldChangeDto
            {
                description = assetAttributeDefinitionEntitys.Find(x => x.Id == attr.FieldId)?.Title ?? attr.FieldName,
                fieldName = attr.FieldName,
                newValue = attr.FieldValue,
                oldValue = "",
                tableName = "AssetAttributeValueEntity"
            });
        }, onUpdate: attr =>
        {
            var description = assetAttributeDefinitionEntitys.Find(x => x.Id == attr.FieldId)?.Title ?? attr.FieldName;
            var cfs = _repository.Context.GetChangeFields(attr);
            if (cfs.IsAny())
            {
                var cf = cfs.FirstOrDefault(x => x.fieldName == nameof(AssetAttributeValueEntity.FieldValue));
                changes.Add(new FieldChangeDto
                {
                    description = $"扩展属性.{description}",
                    fieldName = attr.FieldName,
                    newValue = attr.FieldValue,
                    oldValue = cf?.oldValue ?? "",
                    tableName = "AssetAttributeValueEntity"
                });
            }
            
        }, onDelete: attr =>
        {

        });


        if (changes.IsAny())
        {
            // 假设taskid 为这一次修改 
            var taskId = SnowflakeIdHelper.NextId();
            await _assetChangeLogService.LogAssetChangesAsync(entity.Id, changes,
                 _userManager.UserId, _userManager.RealName, taskId, "编辑资产信息");
        }
    }


    /// <summary>
    /// 条码查询
    /// </summary>
    [HttpGet("Actions/BarcodeQuery")]
    public async Task<AssetInfoOutput> BarcodeQuery([FromQuery] string barcode)
    {
        var entity = await _repository.Context.Queryable<AssetBarcodeEntity>().SingleAsync(x => x.BarcodeNumber == barcode) ?? throw Oops.Oh("条码不存在");

        if (entity.Status != AssetBarcodeStatus.Bound)
        {
            throw Oops.Oh("条码未绑定或状态异常");
        }

        return await GetInfo(entity.BindId);
    }


    /// <summary>
    /// 条码绑定
    /// </summary>
    [HttpPost("Actions/Bind")]
    public async Task BarcodeBindAsync([FromBody] AssetBarcodeBindInput input)
    {
        var entity = await _repository.Context.Queryable<AssetBarcodeEntity>().SingleAsync(x => x.BarcodeNumber == input.barcodeNumber) ?? throw Oops.Oh("条码不存在");

        if (entity.Status == AssetBarcodeStatus.Unbound)
        {
            var asset = await _repository.Context.Queryable<AssetEntity>().SingleAsync(x => x.Id == input.bindId) ?? throw Oops.Oh("资产不存在");

            if (asset.Barcode.IsNotEmptyOrNull())
            {
                throw Oops.Oh("资产已绑定条码，请勿重复绑定");
            }

            // 判断bindId 是否已经绑定了条码
            if (await _repository.Context.Queryable<AssetBarcodeEntity>().AnyAsync(x => x.BindId == input.bindId))
            {
                throw Oops.Oh("绑定对象已存在条码，不允许重复绑定！");
            }

            asset.Barcode = input.barcodeNumber;

            await _repository.Context.Updateable<AssetEntity>(asset).UpdateColumns(x => x.Barcode).ExecuteCommandAsync();

            _repository.Context.Tracking(entity);
            entity.BindId = input.bindId;
            entity.BindTime = DateTime.Now;
            entity.Status = AssetBarcodeStatus.Bound;
            entity.BindType = input.bindType;
            entity.LastModify();



            await _repository.Context.Updateable<AssetBarcodeEntity>(entity).ExecuteCommandAsync();
        }
        else
        {
            throw Oops.Oh("条码状态异常");
        }
    }
}
