using Mapster;
using Microsoft.AspNetCore.Mvc;
using QT.Archive.Dto.Rule;
using QT.Archive.Entity;
using QT.Common.Core;
using QT.Common.Core.Manager;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Systems.Entitys.Dto.Module;
using SqlSugar;

namespace QT.Archive;

/// <summary>
/// 识别配置信息管理
/// </summary>
[ApiDescriptionSettings("档案管理", Tag = "识别配置信息管理", Name = "Rule", Order = 601)]
[Route("api/extend/archive/rule")]
public class RuleService : QTBaseService<ArchivesRuleEntity, RuleCrInput, RuleUpInput, RuleInfoOutput, RuleListPageInput, RuleListOutput>, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<ArchivesRuleEntity> _repository;

    /// <summary>
    /// 初始化档案馆管理服务实例
    /// </summary>
    /// <param name="repository">档案馆实体仓库</param>
    /// <param name="context">SQL Sugar客户端</param>
    /// <param name="userManager">用户管理器</param>
    public RuleService(ISqlSugarRepository<ArchivesRuleEntity> repository, ISqlSugarClient context, IUserManager userManager) : base(repository, context, userManager)
    {
        _repository = repository;
    }

    protected override async Task<SqlSugarPagedList<RuleListOutput>> GetPageList([FromQuery] RuleListPageInput input)
    {
        return await _repository.Context.Queryable<ArchivesRuleEntity>()
            .WhereIF(input.keyword.IsNotEmptyOrNull(), x=>x.Code.Contains(input.keyword) || x.Name.Contains(input.keyword))
           .Select<RuleListOutput>()
           .ToPagedListAsync(input.currentPage, input.pageSize);
    }
}



