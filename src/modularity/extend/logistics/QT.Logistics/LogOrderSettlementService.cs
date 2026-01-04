using QT.Common.Extension;
using QT.Logistics.Entitys.Dto.LogOrder;
using QT.Logistics.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QT.Common.Security;
using Polly;
using QT.Logistics.Entitys.Dto.LogOrderSettlement;
using QT.Systems.Entitys.Permission;

namespace QT.Logistics;

/// <summary>
/// 订单结算
/// </summary>
[ApiDescriptionSettings(ModuleConst.Logistics, Tag = "订单结算管理", Name = "LogOrderSettlement", Order = 200)]
[Route("api/Logistics/[controller]")]
public class LogOrderSettlementService : IDynamicApiController,ITransient
{
    private ISqlSugarRepository<LogOrderEntity> _repository;
    private readonly IUserManager _userManager;
    private ITenant _db;

    public LogOrderSettlementService(ISqlSugarRepository<LogOrderEntity> logOrderRepository, ISqlSugarClient context,IUserManager userManager)
    {
        _repository = logOrderRepository;
        _userManager = userManager;
        _db = context.AsTenant();
    }

    /// <summary>
    /// 获取订单管理列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] LogOrderSettlementListQueryInput input)
    {
        var data = await CreateQuery(input)
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.id)
            .OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort)
            .ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<LogOrderSettlementListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 根据条件分账
    /// </summary>
    /// <returns></returns>
    [HttpPost("accountByCondition")]
    public async Task<int> AccountByCondition([FromBody] LogOrderSettlementListQueryInput input)
    {
        List<string> data = await CreateQuery(input).Select(it=>it.id).ToListAsync();

        return await AccountByList(new LogOrderSettlementCrInput
        {
            items = data
        });
    }

   
    /// <summary>
    /// 根据条件分账
    /// </summary>
    /// <returns></returns>
    [HttpPost("account")]
    public async Task<int> AccountByList([FromBody] LogOrderSettlementCrInput input)
    {
        if (!input.items.IsAny())
        {
            return 0;
        } 


        var items = input.items.ToArray();

        var isOk = await _repository.Context.Updateable<LogOrderEntity>()
            .SetColumns(it => new LogOrderEntity
            { 
                SettlementUserId = _userManager.UserId,
                SettlementTime = DateTime.Now
            })
            .Where(it => items.Contains(it.Id) && !it.SettlementTime.HasValue)
            .ExecuteCommandAsync();

        return isOk;
    }

    #region 私有方法
    /// <summary>
    /// 创建列表查询语句
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private ISugarQueryable<LogOrderSettlementListOutput> CreateQuery(LogOrderSettlementListQueryInput input)
    {
        List<DateTime> queryOrderDate = input.orderDate?.Split(',').ToObject<List<DateTime>>();
        DateTime? startOrderDate = queryOrderDate?.First();
        DateTime? endOrderDate = queryOrderDate?.Last();
        var qur = _repository.Context
            .Queryable<LogOrderEntity>()
            .Where(it => it.OrderStatus > 0)
            .Select((it) => new LogOrderSettlementListOutput
            {
                id = it.Id,
                orderNo = it.OrderNo,
                sendPointId = it.SendPointId,
                reachPointId = it.ReachPointId,
                orderDate = it.OrderDate,
                orderStatus = SqlFunc.ToString(it.OrderStatus),
                remark = it.Remark,
                shipperName = it.ShipperName,
                shipperPhone = it.ShipperPhone,
                shipperAddress = it.ShipperAddress,
                recipientName = it.RecipientName,
                recipientPhone = it.RecipientPhone,
                recipientAddress = it.RecipientAddress,
                accountUserId = it.AccountUserId,
                accountTime = it.AccountTime,
                accountStatus = it.AccountTime.HasValue ? 1 : 0,
                amount = it.Amount,
                platformAmount = it.PlatformAmount,
                sendPointAmount = it.SendPointAmount,
                reachPointAmount = it.ReachPointAmount,
                settlementTime = it.SettlementTime,
                settlementUserId = it.SettlementUserId,
                settlementStatus = it.SettlementTime.HasValue ? 1 : 0,
                accountUserName = SqlFunc.Subqueryable<UserEntity>().Where(dd => dd.Id == it.AccountUserId).Select(dd => dd.RealName),
                settlementUserName = SqlFunc.Subqueryable<UserEntity>().Where(dd => dd.Id == it.SettlementUserId).Select(dd => dd.RealName)
            })
            .WhereIF(input.scope == "point",it=> SqlFunc.Subqueryable<LogDeliveryPointEntity>().Where(dd=>dd.Id == it.sendPointId && dd.AdminId == _userManager.UserId).Any())
            .WhereIF(input.settlementStatus == 1, it => it.settlementTime.HasValue)
            .WhereIF(input.settlementStatus == 0, it => it.settlementTime == null)
            .WhereIF(!string.IsNullOrEmpty(input.pointId), it => it.sendPointId.Equals(input.pointId) || it.reachPointId.Equals(input.pointId))
            .WhereIF(!string.IsNullOrEmpty(input.orderNo), it => it.orderNo.Contains(input.orderNo))
            .WhereIF(!string.IsNullOrEmpty(input.sendPointId), it => it.sendPointId.Equals(input.sendPointId))
            .WhereIF(!string.IsNullOrEmpty(input.reachPointId), it => it.reachPointId.Equals(input.reachPointId))
            .WhereIF(queryOrderDate != null, it => SqlFunc.Between(it.orderDate, startOrderDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), endOrderDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .WhereIF(!string.IsNullOrEmpty(input.orderStatus), it => it.orderStatus.Equals(input.orderStatus))
            .WhereIF(!string.IsNullOrEmpty(input.shipperName), it => it.shipperName.Contains(input.shipperName))
            .WhereIF(!string.IsNullOrEmpty(input.shipperPhone), it => it.shipperPhone.Contains(input.shipperPhone))
            .WhereIF(!string.IsNullOrEmpty(input.recipientName), it => it.recipientName.Contains(input.recipientName))
            .WhereIF(!string.IsNullOrEmpty(input.recipientPhone), it => it.recipientPhone.Contains(input.recipientPhone))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.orderNo.Contains(input.keyword)
                || it.shipperName.Contains(input.keyword)
                || it.shipperPhone.Contains(input.keyword)
                || it.recipientName.Contains(input.keyword)
                || it.recipientPhone.Contains(input.keyword));
        return qur;
    }

    #endregion
}
