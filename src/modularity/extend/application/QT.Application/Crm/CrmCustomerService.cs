using Microsoft.AspNetCore.Mvc;
using QT.Common.Const;
using QT.Common.Core;
using QT.Common.Core.Filter;
using QT.Common.Core.Manager;
using QT.DynamicApiController;
using QT.Iot.Application.Dto.CrmCustomer;
using QT.Iot.Application.Entity;
using QT.Systems.Interfaces.System;
using SqlSugar;

namespace QT.Iot.Application.Service;

/// <summary>
/// 业务实现：客户管理.
/// </summary>
[ApiDescriptionSettings("营销管理", Tag = "客户管理", Name = "CrmCustomer", Order = 300)]
[Route("api/iot/crm/customer")]
[ProhibitOperation(ProhibitOperationEnum.Allow)]
public class CrmCustomerService : QTBaseService<CrmCustomerEntity, CrmCustomerCrInput, CrmCustomerUpInput, CrmCustomerOutput, CrmCustomerListQueryInput, CrmCustomerListOutput>, IDynamicApiController
{
    private readonly IBillRullService _billRullService;

    public CrmCustomerService(ISqlSugarRepository<CrmCustomerEntity> repository, ISqlSugarClient context, IUserManager userManager, IBillRullService billRullService) : base(repository, context, userManager)
    {
        _billRullService = billRullService;
    }

    protected override async Task<SqlSugarPagedList<CrmCustomerListOutput>> GetPageList([FromQuery] CrmCustomerListQueryInput input)
    {
        //List<DateTime> queryInTime = input.orderDate?.Split(',').ToObject<List<DateTime>>();
        //DateTime? startInTime = queryInTime?.First();
        //DateTime? endInTime = queryInTime?.Last();
        return await _repository.Context.Queryable<CrmCustomerEntity>()
            .WhereIF(!_userManager.IsAdministrator, it => it.CreatorUserId == _userManager.UserId)
            //.WhereIF(!string.IsNullOrEmpty(input.keyword), it => it.No.Contains(input.keyword))
            //.WhereIF(queryInTime != null, it => SqlFunc.Between(it.OrderDate, startInTime.ParseToDateTime("yyyy-MM-dd 00:00:00"), endInTime.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .Select<CrmCustomerListOutput>(it => new CrmCustomerListOutput
            {
                //userIdName = SqlFunc.Subqueryable<UserEntity>().Where(ddd => ddd.Id == it.UserId).Select(ddd => ddd.RealName)
                id = it.Id,
                name = it.Name,
                adminName = it.AdminName,
                adminTel = it.AdminTel,
                remark = it.Remark,
            }, true)
            .ToPagedListAsync(input.currentPage, input.pageSize);
    }
}