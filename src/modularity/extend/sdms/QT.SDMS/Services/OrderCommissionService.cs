using Microsoft.AspNetCore.Mvc;
using QT.Common.Core.Manager;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DynamicApiController;
using QT.SDMS.Entitys.Dto.OrderCommission;
using QT.SDMS.Entitys.Entity;
using QT.Systems.Entitys.Permission;
using SqlSugar;

namespace QT.SDMS;

/// <summary>
/// 业务实现：财务管理-订单提成.
/// </summary>
[ApiDescriptionSettings("售电系统", Tag = "销售订单", Name = "OrderCommission", Order = 300)]
[Route("api/sdms/order/commission")]
public class OrderCommissionService : IDynamicApiController
{
    private readonly ISqlSugarRepository<OrderCommissionEntity> _repository;

    public OrderCommissionService(ISqlSugarRepository<OrderCommissionEntity> repository, ISqlSugarClient context, IUserManager userManager)
    {
        _repository = repository;
    }

    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] OrderCommissionListQueryInput input)
    {
        List<DateTime> queryInTime = input.orderDate?.Split(',').ToObject<List<DateTime>>();
        DateTime? startInTime = queryInTime?.First();
        DateTime? endInTime = queryInTime?.Last();
        var data = await _repository.Context.Queryable<OrderEntity>()
             .InnerJoin<OrderCommissionEntity>((a, b) => a.Id == b.FId)
             .WhereIF(queryInTime != null, (a, b) => SqlFunc.Between(a.OrderDate, startInTime.ParseToDateTime("yyyy-MM-dd 00:00:00"), endInTime.ParseToDateTime("yyyy-MM-dd 23:59:59")))
             .WhereIF(input.keyword.IsNotEmptyOrNull(), (a, b) => a.No.Contains(input.keyword) || SqlFunc.Subqueryable<UserEntity>().Where(ddd => ddd.Id == b.UserId && ddd.RealName.Contains(input.keyword)).Any())
             .WhereIF(input.status.HasValue, (a, b) => b.Status == input.status)
             .WhereIF(input.userId.IsNotEmptyOrNull(), (a, b) => b.UserId == input.userId)
             .Select((a, b) => new OrderCommissionListOutput
             {
                 id = b.Id,
                 amount = b.Amount,
                 orderAmount = b.OrderAmount ?? a.Amount,
                 no = b.No,
                 orderDate = a.OrderDate,
                 orderNo = a.No,
                 userIdName = SqlFunc.Subqueryable<UserEntity>().Where(ddd => ddd.Id == b.UserId).Select(ddd => ddd.RealName),
                 proportion = b.Proportion,
                 status = SqlFunc.ToInt32(b.Status),
             })
            .ToPagedListAsync(input.currentPage, input.pageSize);

        return PageResult<OrderCommissionListOutput>.SqlSugarPageResult(data);
    }
}
