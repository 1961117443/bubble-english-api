using Microsoft.AspNetCore.Mvc;
using QT.Archive.Dto.Archives;
using QT.Archive.Entity;
using QT.Common.Core;
using QT.Common.Core.Manager;
using QT.Common.Extension;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.LinqBuilder;
using SqlSugar;
using System.Linq.Expressions;

namespace QT.Archive;

/// <summary>
/// 档案文件管理
/// </summary>
[ApiDescriptionSettings("档案管理", Tag = "档案文件管理", Name = "Archives", Order = 601)]
[Route("api/extend/archive/archives")]
public class ArchivesService : QTBaseService<ArchivesEntity, ArchivesCrInput, ArchivesUpInput, ArchivesInfoOutput, ArchivesListPageInput, ArchivesListOutput>, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<ArchivesEntity> _repository;

    /// <summary>
    /// 初始化档案馆管理服务实例
    /// </summary>
    /// <param name="repository">档案馆实体仓库</param>
    /// <param name="context">SQL Sugar客户端</param>
    /// <param name="userManager">用户管理器</param>
    public ArchivesService(ISqlSugarRepository<ArchivesEntity> repository, ISqlSugarClient context, IUserManager userManager) : base(repository, context, userManager)
    {
        _repository = repository;
    } 

    protected async override Task<SqlSugarPagedList<ArchivesListOutput>> GetPageList([FromQuery] ArchivesListPageInput input)
    {
        Expression<Func<ArchivesEntity, bool>> labelWhere = null;
        if (input.label.IsNotEmptyOrNull())
        {
            foreach (var label in input.label.Split(",", true))
            {
                if (labelWhere == null)
                {
                    labelWhere = it => QTSqlFunc.FIND_IN_SET(label, it.Label);
                }
                else
                {
                    labelWhere = labelWhere.Or(it => QTSqlFunc.FIND_IN_SET(label, it.Label));
                }
            }
        }
        return await _repository.Context.Queryable<ArchivesEntity>()
            .WhereIF(input.bid.IsNotEmptyOrNull(), x => x.Bid == input.bid)
            .WhereIF(input.establishmentDate.IsNotEmptyOrNull(), x=> SqlFunc.Equals(x.EstablishmentDate, input.establishmentDate))
            .WhereIF(input.fileName.IsNotEmptyOrNull(), x=> SqlFunc.Subqueryable<ArchivesDocumentEntity>().Where(ddd=>ddd.Aid == x.Id && ddd.FullName.Contains(input.fileName)).Any())
            .WhereIF(labelWhere != null, labelWhere)
            .Select<ArchivesListOutput>()
            .ToPagedListAsync(input.currentPage, input.pageSize);
    }
}



