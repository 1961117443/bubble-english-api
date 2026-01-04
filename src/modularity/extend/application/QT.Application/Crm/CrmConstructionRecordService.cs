using Microsoft.AspNetCore.Mvc;
using QT.Common.Const;
using QT.Common.Core;
using QT.Common.Core.Manager;
using QT.DynamicApiController;
using QT.Iot.Application.Dto.CrmConstructionRecord;
using QT.Iot.Application.Entity;
using QT.Systems.Interfaces.System;
using SqlSugar;

namespace QT.Iot.Application.Service;

/// <summary>
/// 业务实现：实施记录.
/// </summary>
[ApiDescriptionSettings("营销管理", Tag = "实施记录", Name = "CrmConstructionRecord", Order = 300)]
[Route("api/iot/crm/construction")]
public class CrmConstructionRecordService : QTBaseService<CrmConstructionRecordEntity, CrmConstructionRecordCrInput, CrmConstructionRecordUpInput, CrmConstructionRecordOutput, CrmConstructionRecordListQueryInput, CrmConstructionRecordListOutput>, IDynamicApiController
{

    public CrmConstructionRecordService(ISqlSugarRepository<CrmConstructionRecordEntity> repository, ISqlSugarClient context, IUserManager userManager) : base(repository, context, userManager)
    {
    }

    protected override async Task<SqlSugarPagedList<CrmConstructionRecordListOutput>> GetPageList([FromQuery] CrmConstructionRecordListQueryInput input)
    {
        //List<DateTime> queryInTime = input.orderDate?.Split(',').ToObject<List<DateTime>>();
        //DateTime? startInTime = queryInTime?.First();
        //DateTime? endInTime = queryInTime?.Last();
        return await _repository.Context.Queryable<CrmConstructionRecordEntity>()
            .LeftJoin<CrmProjectEntity>((it, a) => it.Pid == a.Id)
            .WhereIF(!string.IsNullOrEmpty(input.keyword), (it,a) => it.Content.Contains(input.keyword) || a.Name.Contains(input.keyword))
            .WhereIF(!_userManager.IsAdministrator, it => SqlFunc.Subqueryable<CrmProjectUserEntity>().Where(ddd => ddd.Pid == it.Pid && ddd.Uid == _userManager.UserId).Any())
            //.WhereIF(queryInTime != null, it => SqlFunc.Between(it.OrderDate, startInTime.ParseToDateTime("yyyy-MM-dd 00:00:00"), endInTime.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .Select<CrmConstructionRecordListOutput>((it, a) => new CrmConstructionRecordListOutput
            {
                //userIdName = SqlFunc.Subqueryable<UserEntity>().Where(ddd => ddd.Id == it.UserId).Select(ddd => ddd.RealName)
                id = it.Id,
                pid = it.Pid,
                content = it.Content,
                constructionTime = it.ConstructionTime,
                projectName = a.Name
            }, true)
            .ToPagedListAsync(input.currentPage, input.pageSize);
    }
}