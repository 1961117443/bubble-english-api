//using Microsoft.AspNetCore.Mvc;
//using QT.Common.Const;
//using QT.Common.Contracts;
//using QT.Common.Core;
//using QT.Common.Core.Configs;
//using QT.Common.Core.Manager;
//using QT.Common.Core.Service;
//using QT.DynamicApiController;
//using QT.Iot.Application.Dto.CrmUserDelayApply;
//using QT.Iot.Application.Entity;
//using QT.Systems.Entitys.Crm;
//using QT.Systems.Entitys.Permission;
//using QT.Systems.Interfaces.System;
//using SqlSugar;

//namespace QT.Iot.Application.Service;

///// <summary>
///// 业务实现：客户管理.
///// </summary>
//[ApiDescriptionSettings("营销管理", Tag = "客户管理", Name = "CrmUserDelayApply", Order = 300)]
//[Route("api/iot/crm/user-delay-apply")]
//public class CrmUserDelayApplyService : QTBaseService<CrmUserDelayApplyEntity, CrmUserDelayApplyCrInput, CrmUserDelayApplyUpInput, CrmUserDelayApplyOutput, CrmUserDelayApplyListQueryInput, CrmUserDelayApplyListOutput>, IDynamicApiController
//{
//    private readonly ICoreSysConfigService _coreSysConfigService;

//    public CrmUserDelayApplyService(ISqlSugarRepository<CrmUserDelayApplyEntity> repository, ISqlSugarClient context, IUserManager userManager, ICoreSysConfigService coreSysConfigService) : base(repository, context, userManager)
//    {
//        _coreSysConfigService = coreSysConfigService;
//    }

//    protected override async Task<SqlSugarPagedList<CrmUserDelayApplyListOutput>> GetPageList([FromQuery] CrmUserDelayApplyListQueryInput input)
//    {
//        //List<DateTime> queryInTime = input.orderDate?.Split(',').ToObject<List<DateTime>>();
//        //DateTime? startInTime = queryInTime?.First();
//        //DateTime? endInTime = queryInTime?.Last();
//        return await _repository.Context.Queryable<CrmUserDelayApplyEntity>()
//            .InnerJoin<UserEntity>((it,a)=>it.UserId == a.Id)
//            .WhereIF(input.status.HasValue, (it,a)=> it.Status == input.status)
//            .WhereIF(!string.IsNullOrEmpty(input.keyword), (it,a) => a.RealName.Contains(input.keyword) || a.Account.Contains(input.keyword))
//            //.WhereIF(queryInTime != null, it => SqlFunc.Between(it.OrderDate, startInTime.ParseToDateTime("yyyy-MM-dd 00:00:00"), endInTime.ParseToDateTime("yyyy-MM-dd 23:59:59")))
//            .Select<CrmUserDelayApplyListOutput>((it,a) => new CrmUserDelayApplyListOutput
//            {
//                //userIdName = SqlFunc.Subqueryable<UserEntity>().Where(ddd => ddd.Id == it.UserId).Select(ddd => ddd.RealName)
//                id = it.Id,
//                content = it.Content,
//                userId = it.UserId,
//                expireTime = it.ExpireTime,
//                status = it.Status,
//                userIdAccount = a.Account,
//                userIdName = a.RealName,
//                creatorTime = it.CreatorTime
//            }, true)
//            .ToPagedListAsync(input.currentPage, input.pageSize);
//    }

//    [HttpPut("{id}")]
//    [SqlSugarUnitOfWork]
//    public override async Task Update(string id, [FromBody] CrmUserDelayApplyUpInput input)
//    {
//        if (input.status == 1)
//        {
//            if (!input.expireTime.HasValue)
//            {
//                var config = await _coreSysConfigService.GetConfig<IotConfigs>();
//                input.expireTime = DateTime.Now.AddDays(Math.Max(config.defaultExperienceDays, 1));
//            }
//        }
//        await base.Update(id, input);

//        // 更新用户过期日期
//        var entity = await _repository.Context.Queryable<CrmUserDelayApplyEntity>().InSingleAsync(id);
//        if (entity.Status == 1)
//        {
//            UserEntity userEntity = new UserEntity
//            {
//                Id = entity.UserId,
//                ExpireTime = entity.ExpireTime
//            };
//            await _repository.Context.Updateable<UserEntity>(userEntity).UpdateColumns(x => x.ExpireTime).ExecuteCommandAsync();
//        }
//    }
//}