using Microsoft.AspNetCore.Mvc;
using QT.Common.Const;
using QT.Common.Core.Manager;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DynamicApiController;
using QT.Iot.Application.Dto.CrmOrder;
using QT.Iot.Application.Dto.CrmOrderCommission;
using QT.Iot.Application.Entity;
using QT.Systems.Entitys.Permission;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Iot.Application.Service;

/// <summary>
/// 业务实现：财务管理-订单提成.
/// </summary>
[ApiDescriptionSettings("营销管理", Tag = "销售订单", Name = "CrmOrderCommission", Order = 300)]
[Route("api/iot/crm/order/commission")]
public class CrmOrderCommissionService: IDynamicApiController
{
    private readonly ISqlSugarRepository<CrmOrderCommissionEntity> _repository;

    public CrmOrderCommissionService(ISqlSugarRepository<CrmOrderCommissionEntity> repository, ISqlSugarClient context, IUserManager userManager)
    {
        _repository = repository;
    }

    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] CrmOrderCommissionListQueryInput input)
    {
        List<DateTime> queryInTime = input.orderDate?.Split(',').ToObject<List<DateTime>>();
        DateTime? startInTime = queryInTime?.First();
        DateTime? endInTime = queryInTime?.Last();
        var data = await _repository.Context.Queryable<CrmOrderEntity>()
             .InnerJoin<CrmOrderCommissionEntity>((a, b) => a.Id == b.FId)
             .WhereIF(queryInTime != null, (a,b) => SqlFunc.Between(a.OrderDate, startInTime.ParseToDateTime("yyyy-MM-dd 00:00:00"), endInTime.ParseToDateTime("yyyy-MM-dd 23:59:59")))
             .WhereIF(input.keyword.IsNotEmptyOrNull(), (a,b)=> a.No.Contains(input.keyword) || SqlFunc.Subqueryable<UserEntity>().Where(ddd=>ddd.Id == b.UserId && ddd.RealName.Contains(input.keyword)).Any())
             .Select((a, b) => new CrmOrderCommissionListOutput
             {
                 id = b.Id,
                 amount = b.Amount,
                 orderAmount = a.Amount,
                 no = b.No,
                 orderDate = a.OrderDate,
                 orderNo = a.No,
                 userIdName = SqlFunc.Subqueryable<UserEntity>().Where(ddd => ddd.Id == b.UserId).Select(ddd => ddd.RealName),
                 proportion = b.Proportion
             })
            .ToPagedListAsync(input.currentPage, input.pageSize);

        return PageResult<CrmOrderCommissionListOutput>.SqlSugarPageResult(data);
    }
}
