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
using QT.Logistics.Entitys.Dto.LogOrderAccount;
using QT.Systems.Entitys.Permission;

namespace QT.Logistics;

/// <summary>
/// 订单分账
/// </summary>
[ApiDescriptionSettings(ModuleConst.Logistics, Tag = "订单分账管理", Name = "LogOrderAccount", Order = 200)]
[Route("api/Logistics/[controller]")]
public class LogOrderAccountService :IDynamicApiController,ITransient
{
    private ISqlSugarRepository<LogOrderEntity> _repository;
    private readonly IUserManager _userManager;
    private ITenant _db;

    public LogOrderAccountService(ISqlSugarRepository<LogOrderEntity> logOrderRepository, ISqlSugarClient context,IUserManager userManager)
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
    public async Task<dynamic> GetList([FromQuery] LogOrderAccountListQueryInput input)
    {
        var data = await CreateQuery(input)
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.id)
            .OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort)
            .ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<LogOrderAccountListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 根据条件分账
    /// </summary>
    /// <returns></returns>
    [HttpPost("accountByCondition")]
    public async Task<int> AccountByCondition([FromBody] LogOrderAccountListQueryInput input)
    {
        List<string> data = await CreateQuery(input).Select(it=>it.id).ToListAsync();

        return await AccountByList(new LogOrderAccountCrInput
        {
            items = data
        });
    }

   
    /// <summary>
    /// 根据条件分账
    /// </summary>
    /// <returns></returns>
    [HttpPost("account")]
    public async Task<int> AccountByList([FromBody] LogOrderAccountCrInput input)
    {
        if (!input.items.IsAny())
        {
            return 0;
        }
        var entity = await _repository.Context.Queryable<LogOrderFinancialConfigurationEntity>().Where(it => it.Status == 1).FirstAsync();
        if (entity == null)
        {
            throw Oops.Oh("请管理员设置订单分账配置");
        }
        if (entity.PlatformProportion + entity.PointProportion + entity.ReachPointProportion != 100)
        {
            throw Oops.Oh("订单分账配置有误！");
        }

        var pointProportion = entity.PointProportion * 0.01m;
        var reachPointProportion = entity.ReachPointProportion * 0.01m;


        var items = input.items.ToArray();

        var isOk = await _repository.Context.Updateable<LogOrderEntity>()
            .SetColumns(it => new LogOrderEntity
            {
                SendPointAmount = SqlFunc.Round(pointProportion * it.Amount, 2),
                ReachPointAmount = SqlFunc.Round(reachPointProportion * it.Amount, 2),
                PlatformAmount = it.Amount - SqlFunc.Round(pointProportion * it.Amount, 2)- SqlFunc.Round(reachPointProportion * it.Amount, 2),

                AccountUserId = _userManager.UserId,
                AccountTime = DateTime.Now
            })
            .Where(it => items.Contains(it.Id) && !it.AccountTime.HasValue)
            .ExecuteCommandAsync();

        return isOk;
    }

    #region 私有方法
    /// <summary>
    /// 创建列表查询语句
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private ISugarQueryable<LogOrderAccountListOutput> CreateQuery(LogOrderAccountListQueryInput input)
    {
        List<DateTime> queryOrderDate = input.orderDate?.Split(',').ToObject<List<DateTime>>();
        DateTime? startOrderDate = queryOrderDate?.First();
        DateTime? endOrderDate = queryOrderDate?.Last();
        var qur = _repository.Context
            .Queryable<LogOrderEntity>()
            .Where(it => it.OrderStatus > 0)
            .Select((it) => new LogOrderAccountListOutput
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
                accountUserName = SqlFunc.Subqueryable<UserEntity>().Where(dd=>dd.Id == it.AccountUserId).Select(dd=>dd.RealName)
            })
            .WhereIF(input.scope == "point", it => SqlFunc.Subqueryable<LogDeliveryPointEntity>().Where(dd => dd.Id == it.sendPointId && dd.AdminId == _userManager.UserId).Any())
            .WhereIF(input.accountStatus == 1, it => it.accountTime.HasValue)
            .WhereIF(input.accountStatus == 0, it => it.accountTime == null)
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
