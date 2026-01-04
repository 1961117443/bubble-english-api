using Microsoft.AspNetCore.Mvc;
using QT.Common.Const;
using QT.Common.Core.Manager;
using QT.Common.Core;
using QT.DynamicApiController;
using SqlSugar;
using QT.Iot.Application.Entity;
using QT.Iot.Application.Dto.Material;
using QT.Common.Extension;

namespace QT.Iot.Application.Service;

/// <summary>
/// 业务实现：物资基础库.
/// </summary>
[ApiDescriptionSettings("扩展应用", Tag = "物资基础库", Name = "Material", Order = 300)]
[Route("api/iot/apply/material")]
public class MaterialService : QTBaseService<MaterialEntity, MaterialCrInput, MaterialUpInput, MaterialOutput, MaterialListQueryInput, MaterialListOutput>, IDynamicApiController
{
    public MaterialService(ISqlSugarRepository<MaterialEntity> repository, ISqlSugarClient context, IUserManager userManager) : base(repository, context, userManager)
    {
    }

    protected override async Task<SqlSugarPagedList<MaterialListOutput>> GetPageList([FromQuery] MaterialListQueryInput input)
    {
        return await _repository.Context.Queryable<MaterialEntity>()
            .WhereIF(input.keyword.IsNotEmptyOrNull(),it=> it.Code.Contains(input.keyword) || it.Name.Contains(input.keyword))
            .Select<MaterialListOutput>(it=> new MaterialListOutput
            {
                categoryIdName = SqlFunc.Subqueryable<MaterialCategoryEntity>().Where(ddd=>ddd.Id == it.CategoryId).Select(ddd=>ddd.Name)
            },true)
            .ToPagedListAsync(input.currentPage, input.pageSize);
    }
}