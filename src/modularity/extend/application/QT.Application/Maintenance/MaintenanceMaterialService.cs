using Microsoft.AspNetCore.Mvc;
using QT.Common.Const;
using QT.Common.Core.Manager;
using QT.Common.Core;
using QT.DynamicApiController;
using SqlSugar;
using QT.Iot.Application.Entity;
using QT.Iot.Application.Dto.MaintenanceMaterial;
using QT.Common.Extension;

namespace QT.Iot.Application.Service;

/// <summary>
/// 业务实现：物资基础库.
/// </summary>
[ApiDescriptionSettings("维保管理", Tag = "维保物资", Name = "MaintenanceMaterial", Order = 300)]
[Route("api/iot/apply/MaintenanceMaterial")]
public class MaintenanceMaterialService : QTBaseService<MaintenanceMaterialEntity, MaintenanceMaterialCrInput, MaintenanceMaterialUpInput, MaintenanceMaterialOutput, MaintenanceMaterialListQueryInput, MaintenanceMaterialListOutput>, IDynamicApiController
{
    public MaintenanceMaterialService(ISqlSugarRepository<MaintenanceMaterialEntity> repository, ISqlSugarClient context, IUserManager userManager) : base(repository, context, userManager)
    {
    }

    protected override async Task<SqlSugarPagedList<MaintenanceMaterialListOutput>> GetPageList([FromQuery] MaintenanceMaterialListQueryInput input)
    {
        return await _repository.Context.Queryable<MaintenanceMaterialEntity>()
            .WhereIF(input.projectId.IsNotEmptyOrNull(), it=>it.ProjectId == input.projectId)
            .WhereIF(input.keyword.IsNotEmptyOrNull(),it=> it.Code.Contains(input.keyword) || it.Name.Contains(input.keyword))
            .Select<MaintenanceMaterialListOutput>(it=> new MaintenanceMaterialListOutput
            {
                projectIdName = SqlFunc.Subqueryable<MaintenanceProjectEntity>().Where(ddd => ddd.Id == it.ProjectId).Select(ddd => ddd.Name)
                //categoryIdName = SqlFunc.Subqueryable<MaintenanceMaterialCategoryEntity>().Where(ddd=>ddd.Id == it.CategoryId).Select(ddd=>ddd.Name)
            },true)
            .ToPagedListAsync(input.currentPage, input.pageSize);
    }
}