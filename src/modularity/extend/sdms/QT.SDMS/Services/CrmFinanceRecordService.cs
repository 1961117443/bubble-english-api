//using Microsoft.AspNetCore.Mvc;
//using QT.Common.Const;
//using QT.Common.Core;
//using QT.Common.Core.Manager;
//using QT.DynamicApiController;
//using QT.Iot.Application.Dto.CrmFinanceRecord;
//using QT.Iot.Application.Entity;
//using QT.Systems.Interfaces.System;
//using SqlSugar;

//namespace QT.Iot.Application.Service;

///// <summary>
///// 业务实现：财务记录.
///// </summary>
//[ApiDescriptionSettings("营销管理", Tag = "财务记录", Name = "CrmFinanceRecord", Order = 300)]
//[Route("api/iot/crm/finance")]
//public class CrmFinanceRecordService : QTBaseService<CrmFinanceRecordEntity, CrmFinanceRecordCrInput, CrmFinanceRecordUpInput, CrmFinanceRecordOutput, CrmFinanceRecordListQueryInput, CrmFinanceRecordListOutput>, IDynamicApiController
//{

//    public CrmFinanceRecordService(ISqlSugarRepository<CrmFinanceRecordEntity> repository, ISqlSugarClient context, IUserManager userManager) : base(repository, context, userManager)
//    {
//    }

//    protected override async Task<SqlSugarPagedList<CrmFinanceRecordListOutput>> GetPageList([FromQuery] CrmFinanceRecordListQueryInput input)
//    {
//        //List<DateTime> queryInTime = input.orderDate?.Split(',').ToObject<List<DateTime>>();
//        //DateTime? startInTime = queryInTime?.First();
//        //DateTime? endInTime = queryInTime?.Last();
//        return await _repository.Context.Queryable<CrmFinanceRecordEntity>()
//            .LeftJoin<CrmProjectEntity>((it, a) => it.Pid == a.Id)
//            .WhereIF(!string.IsNullOrEmpty(input.keyword), (it,a) => it.Content.Contains(input.keyword) || a.Name.Contains(input.keyword))
//            .WhereIF(!_userManager.IsAdministrator, it => SqlFunc.Subqueryable<CrmProjectUserEntity>().Where(ddd => ddd.Pid == it.Pid && ddd.Uid == _userManager.UserId).Any())
//            //.WhereIF(queryInTime != null, it => SqlFunc.Between(it.OrderDate, startInTime.ParseToDateTime("yyyy-MM-dd 00:00:00"), endInTime.ParseToDateTime("yyyy-MM-dd 23:59:59")))
//            .Select<CrmFinanceRecordListOutput>((it, a) => new CrmFinanceRecordListOutput
//            {
//                //userIdName = SqlFunc.Subqueryable<UserEntity>().Where(ddd => ddd.Id == it.UserId).Select(ddd => ddd.RealName)
//                id = it.Id,
//                pid = it.Pid,
//                content = it.Content,
//                financeTime = it.FinanceTime,
//                projectName = a.Name,
//                amount = it.Amount
//            }, true)
//            .ToPagedListAsync(input.currentPage, input.pageSize);
//    }
//}