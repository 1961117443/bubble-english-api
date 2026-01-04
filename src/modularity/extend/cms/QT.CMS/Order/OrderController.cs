using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QT.CMS.Emum;
using QT.CMS.Entitys;
using QT.CMS.Entitys.Dto.Base;
using QT.CMS.Entitys.Dto.Config;
using QT.CMS.Entitys.Dto.Order;
using QT.CMS.Entitys.Dto.Parameter;
using QT.CMS.Interfaces;
using QT.Common.Core.Emum;
using QT.Common.Core.Filters;
using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Extension;
using QT.FriendlyException;
using QT.JsonSerialization;
using SqlSugar;
using System.Linq.Expressions;

namespace QT.CMS;


/// <summary>
/// 订单信息
/// </summary>
[Route("api/cms/admin/order")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly ISqlSugarRepository<Orders> _orderService;
    private readonly IUserService _userService;
    private readonly IPaymentCollectionService _paymentCollectionService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public OrderController(ISqlSugarRepository<Orders> orderService,  IUserService userService, IPaymentCollectionService paymentCollectionService)
    {
        _orderService = orderService;
        _userService = userService;
        _paymentCollectionService = paymentCollectionService;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 根据ID获取数据
    /// 示例：/admin/order/1
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    [AuthorizeFilter("Order", ActionType.View)]
    public async Task<IActionResult> GetById([FromRoute] long id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<OrdersDto>())
        {
            throw Oops.Oh("请输入正确的塑性参数");
        }
        //查询数据库获取实体
        var model = await _orderService.AsQueryable()
                .Includes(x => x.Collection)
                .Includes(x => x.ShopDelivery)
                .Includes(x => x.OrderGoods)
                .Includes(x => x.OrderPromotion, z=>z.Promotion)
                .Includes(x => x.OrderLog)
                .SingleAsync(x => x.Id == id);
        if (model == null)
        {
           throw Oops.Oh($"数据{id}不存在或已删除");
        }
        //使用AutoMapper转换成ViewModel，根据字段进行塑形
        var result = model.Adapt<OrdersDto>().ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 获取总记录数量
    /// 示例：/admin/order/view/count
    /// </summary>
    [HttpGet("view/count")]
    [Authorize]
    //[AuthorizeFilter("Order", ActionType.View)]
    public async Task<IActionResult> GetCount([FromQuery] OrderParameter searchParam)
    {
        var result = await _orderService.AsQueryable()
            .WhereIF(searchParam.Status > 0, x => x.Status == searchParam.Status)
            .WhereIF(searchParam.PaymentStatus > 0, x => x.PaymentStatus == searchParam.PaymentStatus)
            .WhereIF(searchParam.DeliveryStatus > 0, x => x.DeliveryStatus == searchParam.DeliveryStatus)
            .WhereIF(searchParam.StartTime.HasValue, x => SqlFunc.GreaterThanOrEqual(x.AddTime, searchParam.StartTime.Value))
            .CountAsync();
        //var result = await _orderService.QueryCountAsync(
        //    x => (searchParam.Status <= 0 || x.Status == searchParam.Status)
        //    && (searchParam.PaymentStatus < 0 || x.PaymentStatus == searchParam.PaymentStatus)
        //    && (searchParam.DeliveryStatus < 0 || x.DeliveryStatus == searchParam.DeliveryStatus)
        //    && (searchParam.StartTime == null || DateTime.Compare(x.AddTime, searchParam.StartTime.GetValueOrDefault()) >= 0));
        //返回成功200
        return Ok(result);
    }

    /// <summary>
    /// 获取总记录金额
    /// 示例：/admin/order/view/amount
    /// </summary>
    [HttpGet("view/amount")]
    [Authorize]
    //[AuthorizeFilter("Order", ActionType.View)]
    public async Task<IActionResult> GetAmount([FromQuery] OrderParameter searchParam)
    {
        if (searchParam.StartTime == null)
        {
            searchParam.StartTime = DateTime.Now.AddHours(-DateTime.Now.Hour + 1);
        }
        var result = await _orderService.AsQueryable()
            .WhereIF(searchParam.Status > 0, x => x.Status == searchParam.Status)
            .WhereIF(searchParam.StartTime.HasValue, x => SqlFunc.GreaterThanOrEqual(x.AddTime, searchParam.StartTime.Value))
            .OrderByDescending(x => x.AddTime)
            .GroupBy(x => new { x.AddTime.Month, x.AddTime.Day })
            .Select(g => SqlFunc.AggregateSum(g.OrderAmount))
            .FirstAsync();

        //var result = await _orderService.QueryAmountAsync(
        //    x => (searchParam.Status <= 0 || x.Status == searchParam.Status)
        //    && (DateTime.Compare(x.AddTime, searchParam.StartTime.GetValueOrDefault()) >= 0));
        //返回成功200
        return Ok(result);
    }

    /// <summary>
    /// 获取指定数量列表
    /// 示例：/admin/order/view/10
    /// </summary>
    [HttpGet("view/{top}")]
    [Authorize]
    //[AuthorizeFilter("Order", ActionType.View)]
    public async Task<IActionResult> GetList([FromRoute] int top, [FromQuery] OrderParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<OrdersDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<OrdersDto>())
        {
            throw Oops.Oh("请输入正确的塑性参数");
        }

        //获取数据库列表
        var resultFrom = await _orderService.AsQueryable()
                 .Includes(x => x.Collection)
                .Includes(x => x.ShopDelivery)
                .Includes(x => x.OrderGoods)
            .WhereIF(searchParam.SiteId > 0, x => x.SiteId == searchParam.SiteId)
            .WhereIF(searchParam.OrderType >= 0, x => x.OrderType == searchParam.OrderType)
            .WhereIF(searchParam.PaymentId > 0, x => x.CollectionId == searchParam.PaymentId)
            .WhereIF(searchParam.DeliveryId > 0, x => x.DeliveryId == searchParam.DeliveryId)
            .WhereIF(searchParam.Status > 0, x => x.Status == searchParam.Status)
            .WhereIF(searchParam.StartTime.HasValue, x => SqlFunc.GreaterThanOrEqual(x.AddTime, searchParam.StartTime.Value))
            .WhereIF(searchParam.EndTime.HasValue, x => SqlFunc.LessThanOrEqual(x.AddTime, searchParam.EndTime.Value))
            .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.OrderNo.Contains(searchParam.Keyword))
            .OrderBy(searchParam.OrderBy ?? "-AddTime,-Id")
            .AutoTake(top)
            .ToListAsync();


        //var resultFrom = await _orderService.QueryListAsync(top,
        //     x => (searchParam.SiteId <= 0 || x.SiteId == searchParam.SiteId)
        //    && (searchParam.OrderType < 0 || x.OrderType == searchParam.OrderType)
        //    && (searchParam.PaymentId <= 0 || (x.Collection != null && x.Collection.PaymentId == searchParam.PaymentId))
        //    && (searchParam.DeliveryId <= 0 || x.DeliveryId == searchParam.DeliveryId)
        //    && (searchParam.Status <= 0 || x.Status == searchParam.Status)
        //    && (searchParam.StartTime == null || DateTime.Compare(x.AddTime, searchParam.StartTime.GetValueOrDefault()) >= 0)
        //    && (searchParam.EndTime == null || DateTime.Compare(x.AddTime, searchParam.EndTime.GetValueOrDefault()) <= 0)
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.OrderNo != null && x.OrderNo.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "-AddTime,-Id");

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<OrdersDto>>(); // .ShapeData(searchParam.Fields);
        //返回成功200
        return Ok(resultDto);
    }

    /// <summary>
    /// 获取分页列表
    /// 示例：/admin/order?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet]
    [Authorize]
    //[AuthorizeFilter("Order", ActionType.View)]
    public async Task<IActionResult> GetList([FromQuery] OrderParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<OrdersDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<OrdersDto>())
        {
            throw Oops.Oh("请输入正确的塑性参数");
        }

        //获取数据列表，如果ID大于0则查询该用户下所有的列表
        var list = await _orderService.AsQueryable()
                 .Includes(x => x.Collection)
                .Includes(x => x.ShopDelivery)
                .Includes(x => x.OrderGoods)
          .WhereIF(searchParam.SiteId > 0, x => x.SiteId == searchParam.SiteId)
          .WhereIF(searchParam.OrderType >= 0, x => x.OrderType == searchParam.OrderType)
          .WhereIF(searchParam.PaymentId > 0, x => x.CollectionId == searchParam.PaymentId)
          .WhereIF(searchParam.DeliveryId > 0, x => x.DeliveryId == searchParam.DeliveryId)
          .WhereIF(searchParam.Status > 0, x => x.Status == searchParam.Status)
          .WhereIF(searchParam.StartTime.HasValue, x => SqlFunc.GreaterThanOrEqual(x.AddTime, searchParam.StartTime.Value))
          .WhereIF(searchParam.EndTime.HasValue, x => SqlFunc.LessThanOrEqual(x.AddTime, searchParam.EndTime.Value))
          .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.OrderNo.Contains(searchParam.Keyword))
          .OrderBy(searchParam.OrderBy ?? "-AddTime,-Id")
          .ToPagedListAsync(pageParam.PageIndex, pageParam.PageSize);

        return Ok(PageResult<OrdersDto>.SqlSugarPageResult(list));

        //var list = await _orderService.QueryPageAsync(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => (searchParam.SiteId <= 0 || x.SiteId == searchParam.SiteId)
        //    && (searchParam.OrderType < 0 || x.OrderType == searchParam.OrderType)
        //    && (searchParam.PaymentId <= 0 || (x.Collection != null && x.Collection.PaymentId == searchParam.PaymentId))
        //    && (searchParam.DeliveryId <= 0 || x.DeliveryId == searchParam.DeliveryId)
        //    && (searchParam.Status <= 0 || x.Status == searchParam.Status)
        //    && (searchParam.StartTime == null || DateTime.Compare(x.AddTime, searchParam.StartTime.GetValueOrDefault()) >= 0)
        //    && (searchParam.EndTime == null || DateTime.Compare(x.AddTime, searchParam.EndTime.GetValueOrDefault()) <= 0)
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.OrderNo != null && x.OrderNo.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "-AddTime,-Id");

        ////x-pagination
        //var paginationMetadata = new
        //{
        //    totalCount = list.pagination.Total,
        //    pageSize = list.pagination.PageSize,
        //    pageIndex = list.pagination.PageIndex,
        //    totalPages = 0
        //};
        //Response.Headers.Add("x-pagination", JSON.Serialize(paginationMetadata));

        ////映射成DTO，根据字段进行塑形
        //var resultDto = list.Adapt<IEnumerable<OrdersDto>>().ShapeData(searchParam.Fields);
        //return Ok(resultDto);
    }

    /// <summary>
    /// 添加订单
    /// 示例：/admin/order
    /// </summary>
    [HttpPost]
    [Authorize]
    [SqlSugarUnitOfWork]
    //[AuthorizeFilter("Order", ActionType.Add)]
    public async Task<IActionResult> Add([FromBody] OrdersEditDto modelDto)
    {
        //保存数据
        var result = await this.AddAsync(modelDto);
        return Ok(result);
    }

    /// <summary>
    /// 修改订单
    /// 示例：/admin/order/1
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [SqlSugarUnitOfWork]
    //[AuthorizeFilter("Order", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] OrdersEditDto modelDto)
    {
        //保存数据
        var result = await this.UpdateAsync(id, modelDto);
        return NoContent();
    }

    /// <summary>
    /// 确认收款(管理员)
    /// 示例：/admin/order/confirm/TN2011233211...
    /// </summary>
    [HttpPut("confirm/{tradeNo}")]
    [Authorize]
    [SqlSugarUnitOfWork]
    //[AuthorizeFilter("Order", ActionType.Confirm)]
    public async Task<IActionResult> Confirm([FromRoute] string tradeNo)
    {
        //保存数据
        var result = await _paymentCollectionService.ConfirmAsync(tradeNo);
        if (result)
        {
            return NoContent();
        }
        throw Oops.Oh("保存过程中发生了错误");
    }

    /// <summary>
    /// 签收订单(管理员)
    /// 示例：/admin/order/receipt/1
    /// </summary>
    [HttpPut("receipt/{id}")]
    [Authorize]
    [SqlSugarUnitOfWork]
    //[AuthorizeFilter("Order", ActionType.Complete)]
    public async Task<IActionResult> Receipt([FromRoute] long id)
    {
        //保存数据
        var result = await this.ReceiptAsync(x => x.Id == id);
        if (result)
        {
            return NoContent();
        }
        throw Oops.Oh("保存过程中发生了错误");
    }

    /// <summary>
    /// 完成订单(管理员)
    /// 示例：/admin/order/complete/1
    /// </summary>
    [HttpPut("complete/{id}")]
    [Authorize]
    [SqlSugarUnitOfWork]
    //[AuthorizeFilter("Order", ActionType.Complete)]
    public async Task<IActionResult> Complete([FromRoute] long id)
    {
        //保存数据
        var result = await this.CompleteAsync(id);
        if (result)
        {
            return NoContent();
        }
        throw Oops.Oh("保存过程中发生了错误");
    }

    /// <summary>
    /// 作废订单(管理员)
    /// 示例：/admin/order/invalid/1
    /// </summary>
    [HttpPut("invalid/{id}")]
    [Authorize]
    [SqlSugarUnitOfWork]
    //[AuthorizeFilter("Order", ActionType.Invalid)]
    public async Task<IActionResult> Invalid([FromRoute] long id)
    {
        //保存数据
        var result = await this.InvalidAsync(id);
        if (result)
        {
            return NoContent();
        }
        throw Oops.Oh("保存过程中发生了错误");
    }

    /// <summary>
    /// 删除一条记录
    /// 示例：/admin/order/1
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [SqlSugarUnitOfWork]
    //[AuthorizeFilter("Order", ActionType.Delete)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        if (!await _orderService.AnyAsync(x => x.Id == id && x.Status <= 5))
        {
            throw Oops.Oh($"订单已删除或在交易中");
        }
        var result = await _orderService.Context.DeleteNav<Orders>(x => x.Id == id && x.Status > 5)
                .Include(x => x.OrderPromotion)
                .Include(x => x.OrderLog)
                .Include(x => x.OrderGoods)
                .ExecuteCommandAsync();

        //var list = await _orderService.Context.Queryable<Orders>().IncludesAllFirstLayer().Where(x => x.Id == id && x.Status > 5).ToListAsync();

        //if (list.IsAny())
        //{
        //    await _orderService.Context.DeleteNav<Orders>(list).IncludesAllFirstLayer().ExecuteCommandAsync();
        //}

        //var result = await _orderService.DeleteAsync(x => x.Id == id && x.Status > 5);
        //if (result>0)
        //{
        //   await _orderService.Context.Deleteable<OrderLog>(x => x.OrderId == id).ExecuteCommandAsync();
        //    await _orderService.Context.Deleteable<OrderGoods>(x => x.OrderId == id).ExecuteCommandAsync();
        //    await _orderService.Context.Deleteable<OrderGoods>(x => x.OrderId == id).ExecuteCommandAsync();

        //}
        return NoContent();
    }

    /// <summary>
    /// 批量删除记录(级联数据)
    /// 示例：/admin/order?ids=1,2,3
    /// </summary>
    [HttpDelete]
    [Authorize]
    [SqlSugarUnitOfWork]
    //[AuthorizeFilter("Order", ActionType.Delete)]
    public async Task<IActionResult> DeleteByIds([FromQuery] string Ids)
    {
        if (Ids == null)
        {
            throw Oops.Oh("传输参数不可为空");
        }
        //将ID列表转换成IEnumerable
        var arrIds = Ids.ToIEnumerable<long>();
        if (arrIds == null)
        {
            throw Oops.Oh("传输参数不符合规范");
        }
        //执行批量删除操作
        //var list = await _orderService.Context.Queryable<Orders>().IncludesAllFirstLayer().Where(x => x.Status > 5 && arrIds.Contains(x.Id)).ToListAsync();

        //if (list.IsAny())
        //{
        //    await _orderService.Context.DeleteNav<Orders>(list).IncludesAllFirstLayer().ExecuteCommandAsync();
        //}

        var result = await _orderService.Context.DeleteNav<Orders>(x => x.Status > 5 && arrIds.Contains(x.Id))
               .Include(x => x.Collection)
               .Include(x => x.OrderLog)
               .Include(x => x.OrderGoods)
               .ExecuteCommandAsync();

        //await _orderService.DeleteAsync(x => x.Status > 5 && arrIds.Contains(x.Id));

        return NoContent();
    }
    #endregion

    #region 当前账户调用接口========================
    /// <summary>
    /// 根据ID获取数据
    /// 示例：/account/order/1
    /// </summary>
    [HttpGet("/account/order/{id}")]
    [Authorize]
    [NonUnify]
    public async Task<IActionResult> AccountGetById([FromRoute] long id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<OrdersDto>())
        {
            throw Oops.Oh("请输入正确的塑性参数");
        }
        //获取登录用户ID
        var userId = await  _userService.GetUserIdAsync();
        if (userId.IsNullOrEmpty())
        {
            throw Oops.Oh("用户尚未登录");
        }

        //查询数据库获取实体
        var model = await _orderService.AsQueryable()
    .Includes(x => x.Collection)
    .Includes(x => x.ShopDelivery)
    .Includes(x => x.OrderGoods)
    .Includes(x => x.OrderPromotion, z => z.Promotion)
    .Includes(x => x.OrderLog)
    .Where(x => x.UserId == userId && x.Id == id)
    .FirstAsync();

        //var model = await _orderService.SingleAsync(x => x.UserId == userId && x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"订单{id}不存在或已删除");
        }
        //使用AutoMapper转换成ViewModel，根据字段进行塑形
        var result = model.Adapt<OrdersDto>(); //.ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 根据订单货品ID获取数据
    /// 示例：/account/order/goods/1
    /// </summary>
    [HttpGet("/account/order/goods/{id}")]
    [Authorize]
    [NonUnify]
    public async Task<IActionResult> AccountGetByGoodsId([FromRoute] long id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<OrderGoodsDto>())
        {
            throw Oops.Oh("请输入正确的塑性参数");
        }
        //获取登录用户ID
        var userId =  await  _userService.GetUserIdAsync();
        if (userId.IsNullOrEmpty())
        {
            throw Oops.Oh("用户尚未登录");
        }

        //查询数据库获取实体 
        var model = await _orderService.Context.Queryable<OrderGoods>().Includes(x=>x.Order)
            .SingleAsync(x => x.Order.Id > 0 && x.Order.UserId == userId && x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"订单不存在或已删除");
        }
        //使用AutoMapper转换成ViewModel，根据字段进行塑形
        var result = model.Adapt<OrderGoodsDto>().ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 根据订单号获取数据
    /// 示例：/account/orderno/PN2021031...
    /// </summary>
    [HttpGet("/account/orderno/{orderNo}")]
    [Authorize]
    [NonUnify]
    public async Task<IActionResult> AccountGetByNo([FromRoute] string orderNo, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<OrdersDto>())
        {
            throw Oops.Oh("请输入正确的塑性参数");
        }
        //获取登录用户ID
        var userId =  await  _userService.GetUserIdAsync();
        if (userId.IsNullOrEmpty())
        {
            throw Oops.Oh("用户尚未登录");
        }

        //查询数据库获取实体
        var model = await _orderService.SingleAsync(x => x.UserId == userId && x.OrderNo == orderNo);
        if (model == null)
        {
            throw Oops.Oh($"订单[{orderNo}]不存在或已删除");
        }
        //使用AutoMapper转换成ViewModel，根据字段进行塑形
        var result = model.Adapt<OrdersDto>().ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 获取指定数量列表
    /// 示例：/account/order/view/10
    /// </summary>
    [HttpGet("/account/order/view/{top}")]
    [Authorize]
    [NonUnify]
    public async Task<IActionResult> AccountGetList([FromRoute] int top, [FromQuery] OrderParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<OrdersDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<OrdersDto>())
        {
            throw Oops.Oh("请输入正确的塑性参数");
        }
        //获取登录用户ID
        var userId =  await  _userService.GetUserIdAsync();
        if (userId.IsNullOrEmpty())
        {
            throw Oops.Oh("用户尚未登录");
        }

        //获取数据库列表
        var resultFrom = await _orderService.AsQueryable()
                 .Includes(x => x.Collection)
                .Includes(x => x.ShopDelivery)
                .Includes(x => x.OrderGoods)
            .Where(x=>x.UserId == userId)
            .WhereIF(searchParam.SiteId > 0, x => x.SiteId == searchParam.SiteId)
            .WhereIF(searchParam.OrderType >= 0, x => x.OrderType == searchParam.OrderType)
            .WhereIF(searchParam.PaymentId > 0, x => x.CollectionId == searchParam.PaymentId)
            .WhereIF(searchParam.DeliveryId > 0, x => x.DeliveryId == searchParam.DeliveryId)
            //.Where(x=> searchParam.Status !=1 || x.Status ==1)
            //.Where(x => searchParam.Status != 2 || x.Status == 2)
            //.Where(x => searchParam.Status != 3 || (x.Status == 3 || x.Status == 4))
            //.Where(x => searchParam.Status != 4 || x.Status == 5 && SqlFunc.Subqueryable<OrderGoods>().Where(g => g.OrderId == x.Id && g.DeliveryStatus == 1 && g.EvaluateStatus == 0).Any())
            //.Where(x=> searchParam.StartTime == null || x.AddTime >= searchParam.StartTime)
            // .Where(x => searchParam.EndTime == null || x.AddTime <= searchParam.EndTime)
            .WhereIF(searchParam.Status == 1, x => x.Status == 1)
            .WhereIF(searchParam.Status == 2, x => x.Status == 2)
            .WhereIF(searchParam.Status == 3, x => x.Status == 3 || x.Status == 4)
            .WhereIF(searchParam.Status == 5, x => x.Status == 5 && SqlFunc.Subqueryable<OrderGoods>().Where(g => g.OrderId == x.Id && g.DeliveryStatus == 1 && g.EvaluateStatus == 0).Any())
            //.WhereIF(searchParam.Status > 0, x => x.Status == searchParam.Status)
            .WhereIF(searchParam.StartTime.HasValue, x => SqlFunc.GreaterThanOrEqual(x.AddTime, searchParam.StartTime.Value))
            .WhereIF(searchParam.EndTime.HasValue, x => SqlFunc.LessThanOrEqual(x.AddTime, searchParam.EndTime.Value))
            .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.OrderNo.Contains(searchParam.Keyword))
            .OrderBy(searchParam.OrderBy ?? "-AddTime,-Id")
            .AutoTake(top)
            .ToListAsync();

        //var resultFrom = await _orderService.QueryListAsync(top,
        //    x => x.UserId == userId
        //    && (searchParam.SiteId <= 0 || x.SiteId == searchParam.SiteId)
        //    && (searchParam.OrderType < 0 || x.OrderType == searchParam.OrderType)
        //    && (searchParam.PaymentId <= 0 || (x.Collection != null && x.Collection.PaymentId == searchParam.PaymentId))
        //    && (searchParam.DeliveryId <= 0 || x.DeliveryId == searchParam.DeliveryId)
        //    && (searchParam.Status != 1 || x.Status == 1)
        //    && (searchParam.Status != 2 || x.Status == 2)
        //    && (searchParam.Status != 3 || (x.Status == 3 || x.Status == 4))
        //    && (searchParam.Status != 4 || x.Status == 5 && x.OrderGoods.Any(g => g.DeliveryStatus == 1 && g.EvaluateStatus == 0))
        //    && (searchParam.StartTime == null || DateTime.Compare(x.AddTime, searchParam.StartTime.GetValueOrDefault()) >= 0)
        //    && (searchParam.EndTime == null || DateTime.Compare(x.AddTime, searchParam.EndTime.GetValueOrDefault()) <= 0)
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.OrderNo != null && x.OrderNo.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "-AddTime,-Id");

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<OrdersDto>>(); //.ShapeData(searchParam.Fields);
        //返回成功200
        return Ok(resultDto);
    }

    /// <summary>
    /// 获取分页列表
    /// 示例：/account/order?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet("/account/order")]
    [Authorize]
    [NonUnify]
    public async Task<IActionResult> AccountGetList([FromQuery] OrderParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<OrdersDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<OrdersDto>())
        {
            throw Oops.Oh("请输入正确的塑性参数");
        }
        //获取登录用户ID
        var userId =  await  _userService.GetUserIdAsync();
        if (userId.IsNullOrEmpty())
        {
            throw Oops.Oh("用户尚未登录").StatusCode(StatusCodes.Status404NotFound);
        }

        //获取数据列表，如果ID大于0则查询该用户下所有的列表
        var list = await _orderService.AsQueryable()
                 .Includes(x => x.Collection)
                .Includes(x => x.ShopDelivery)
                .Includes(x => x.OrderGoods)
           .Where(x => x.UserId == userId)
           .WhereIF(searchParam.SiteId > 0, x => x.SiteId == searchParam.SiteId)
           .WhereIF(searchParam.OrderType >= 0, x => x.OrderType == searchParam.OrderType)
           .WhereIF(searchParam.PaymentId > 0, x => x.CollectionId == searchParam.PaymentId)
           .WhereIF(searchParam.DeliveryId > 0, x => x.DeliveryId == searchParam.DeliveryId)
           .WhereIF(searchParam.Status == 1, x => x.Status == 1)
           .WhereIF(searchParam.Status == 2, x => x.Status == 2)
           .WhereIF(searchParam.Status == 3, x => x.Status == 3 || x.Status == 4)
           .WhereIF(searchParam.Status == 5, x => x.Status == 5 && SqlFunc.Subqueryable<OrderGoods>().Where(g => g.OrderId == x.Id && g.DeliveryStatus == 1 && g.EvaluateStatus == 0).Any())
           //.WhereIF(searchParam.Status > 0, x => x.Status == searchParam.Status)
           .WhereIF(searchParam.StartTime.HasValue, x => SqlFunc.GreaterThanOrEqual(x.AddTime, searchParam.StartTime.Value))
           .WhereIF(searchParam.EndTime.HasValue, x => SqlFunc.LessThanOrEqual(x.AddTime, searchParam.EndTime.Value))
           .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.OrderNo.Contains(searchParam.Keyword))
           .OrderBy(searchParam.OrderBy ?? "-AddTime,-Id")
           .ToPagedListAsync(pageParam.PageIndex, pageParam.PageSize);

        return Ok(PageResult<OrdersDto>.SqlSugarPageResult(list));
        //var list = await _orderService.QueryPageAsync(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => x.UserId == userId
        //    && (searchParam.SiteId <= 0 || x.SiteId == searchParam.SiteId)
        //    && (searchParam.OrderType < 0 || x.OrderType == searchParam.OrderType)
        //    && (searchParam.PaymentId <= 0 || (x.Collection != null && x.Collection.PaymentId == searchParam.PaymentId))
        //    && (searchParam.DeliveryId <= 0 || x.DeliveryId == searchParam.DeliveryId)
        //    && (searchParam.Status != 1 || x.Status == 1)
        //    && (searchParam.Status != 2 || x.Status == 2)
        //    && (searchParam.Status != 3 || (x.Status == 3 || x.Status == 4))
        //    && (searchParam.Status != 4 || x.Status == 5 && x.OrderGoods.Any(g => g.DeliveryStatus == 1 && g.EvaluateStatus == 0))
        //    && (searchParam.StartTime == null || DateTime.Compare(x.AddTime, searchParam.StartTime.GetValueOrDefault()) >= 0)
        //    && (searchParam.EndTime == null || DateTime.Compare(x.AddTime, searchParam.EndTime.GetValueOrDefault()) <= 0)
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.OrderNo != null && x.OrderNo.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "-AddTime,-Id");

        //x-pagination
        //var paginationMetadata = new
        //{
        //    totalCount = list.pagination.Total,
        //    pageSize = list.pagination.PageSize,
        //    pageIndex = list.pagination.PageIndex,
        //    totalPages = 0
        //};
        //Response.Headers.Add("x-pagination", JSON.Serialize(paginationMetadata));

        ////映射成DTO，根据字段进行塑形
        //var resultDto = list.Adapt<IEnumerable<OrdersDto>>().ShapeData(searchParam.Fields);
        //return Ok(resultDto);
    }

    /// <summary>
    /// 添加订单
    /// 示例：/account/order
    /// </summary>
    [HttpPost("/account/order")]
    [Authorize]
    [SqlSugarUnitOfWork]
    [NonUnify]
    public async Task<IActionResult> AccountAdd([FromBody] OrdersEditDto modelDto)
    {
        //获取登录用户ID
        var userId = await  _userService.GetUserIdAsync();
        if (userId.IsNullOrEmpty())
        {
            throw Oops.Oh("用户尚未登录");
        }
        modelDto.userId = userId;
        modelDto.realFreight = null; //自动计算运费
        modelDto.discountAmount = 0; //自动计算优惠
        //保存数据
        var result = await this.AddAsync(modelDto);
        return Ok(result);
    }

    /// <summary>
    /// 取消订单(客户)
    /// 示例：/account/order/cancel/1
    /// </summary>
    [HttpPut("/account/order/cancel/{id}")]
    [Authorize]
    [SqlSugarUnitOfWork]
    [NonUnify]
    public async Task<IActionResult> Cancel([FromRoute] long id)
    {
        //保存数据
        var result = await this.CancelAsync(id);
        if (result)
        {
            return NoContent();
        }
        throw Oops.Oh("保存过程中发生了错误");
    }

    /// <summary>
    /// 确认收货(客户)
    /// 示例：/account/order/confirm/1
    /// </summary>
    [HttpPut("/account/order/confirm/{id}")]
    [Authorize]
    [SqlSugarUnitOfWork]
    [NonUnify]
    public async Task<IActionResult> Confirm([FromRoute] long id)
    {
        //保存数据
        var result = await this.ConfirmAsync(id);
        if (result)
        {
            return NoContent();
        }
        throw Oops.Oh("保存过程中发生了错误");
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// 添加订单
    /// </summary>
    private async Task<PaymentCollectionDto> AddAsync(OrdersEditDto modelDto)
    {
        //映射DTO到实体
        PaymentCollection? result;

        //判断订单类型1.定时抢购
        if (modelDto.orderType == 1)
        {
            result = await AddRushOrder(modelDto);
        }
        //2.积分换购
        else if (modelDto.orderType == 2)
        {
            result = await AddPointOrder(modelDto);
        }
        //0.普通订单
        else
        {
            //检查订单商品货品数量
            if (modelDto.orderGoods == null || modelDto.orderGoods.Count == 0)
            {
                throw Oops.Oh("没有找到商品，请确认后操作");
            }
            //检查购买数量是否正确
            if (modelDto.orderGoods.Where(x => x.quantity < 1).Count() > 0)
            {
                throw Oops.Oh("购买数量至少一件");
            }
            result = await AddNormalOrder(modelDto);
        }
        //映射成DTO
        return result.Adapt<PaymentCollectionDto>();
    }

    /// <summary>
    /// 修改一条记录
    /// </summary>
    private async Task<bool> UpdateAsync(long id, OrdersEditDto modelDto)
    {
        //根据ID获取记录
        var model = await _orderService.AsQueryable()
            .Includes(x => x.Collection).SingleAsync(x => x.Id == id);
        //如果不存在则抛出异常
        if (model == null || model.Collection == null)
        {
            throw Oops.Oh("数据不存在或已删除");
        }
        _orderService.Context.Tracking(model);
        //未付款
        if (model.PaymentStatus == 0)
        {
            //查询支付方式
            var paymentModel = await _orderService.Context.Queryable<SitePayment>().SingleAsync(x => x.Id == modelDto.paymentId);
            if (paymentModel == null)
            {
                throw Oops.Oh("支付方式不存在或已删除");
            }
            model.DiscountAmount = modelDto.discountAmount;//优惠金额
            if (modelDto.realFreight != null)
            {
                model.RealFreight = modelDto.realFreight.GetValueOrDefault();//实收运费
            }
            //订单总金额=实付商品总金额+实付总运费-优惠券金额-优惠活动金额-订单折扣或涨价
            model.OrderAmount = model.RealAmount + model.RealFreight - model.CouponAmount - model.PromotionAmount - model.DiscountAmount;
            //更改支付收款信息
            model.Collection.PaymentId = paymentModel.Id;
            model.Collection.PaymentTitle = paymentModel.Title;
            model.Collection.PaymentType = paymentModel.Type == "cash" ? (byte)1 : (byte)0;
            model.Collection.PaymentAmount = model.OrderAmount;
            //如果是线下支付，则更改订单状态为发货状态
            if (model.Collection.PaymentType == 1)
            {
                model.Status = 2;
            }
        }
        //未发货
        if (model.DeliveryStatus == 0)
        {
            model.Province = modelDto.province;
            model.City = modelDto.city;
            model.Area = modelDto.area;
            var addressArr = modelDto.address?.Split(",");
            if (addressArr?.Length > 0)
            {
                model.Address = $"{model.Province},{model.City},{model.Area},{addressArr[^1]}";
            }
            model.AcceptName = modelDto.acceptName;
            model.TelPhone = modelDto.telPhone;
            model.Mobile = modelDto.mobile;
            model.Postscript = modelDto.postscript;
            model.AcceptTime = modelDto.acceptTime;
        }
        model.Remark = modelDto.remark;
        //新增订单日志
        await _orderService.Context.Insertable<OrderLog>(new OrderLog
        {
            OrderId = model.Id,
            OrderNo = model.OrderNo,
            ActionType = ActionType.Edit.ToString(),
            AddBy = await _userService.GetUserNameAsync(),
            AddTime = DateTime.Now,
            Remark = $"订单[{model.OrderNo}]修改"
        }).ExecuteCommandAsync();
        //保存到数据库
        return await _orderService.UpdateAsync(model) >0;
    }

    /// <summary>
    /// 签收订单(管理员)
    /// </summary>
    private async Task<bool> ReceiptAsync(Expression<Func<Orders, bool>> funcWhere)
    {
                                                               //根据ID获取记录
        var model = await _orderService.AsQueryable()
            .Includes(x => x.Collection).SingleAsync(funcWhere);
        //如果不存在则抛出异常
        if (model == null || model.Collection == null)
        {
            throw Oops.Oh("订单不存在或已删除");
        }
        if (model.Status > 3)
        {
            throw Oops.Oh("订单未发货，无法签收操作");
        }
        _orderService.Context.Tracking(model);
        //检查支付状态，如果是未付款，则检查是否到付
        if (model.PaymentStatus == 0)
        {
            if (model.Collection.PaymentType == 0)
            {
                throw Oops.Oh("订单未付款，无法操作");
            }
            //货到付款，则修改成已支付
            model.Collection.Status = 2;
            model.Collection.CompleteTime = DateTime.Now;
            //付款没问题后设置订单付款状态为已支付
            model.PaymentStatus = 1;
        }
        //新增订单日志
        await _orderService.Context.Insertable<OrderLog>(new OrderLog
        {
            OrderId = model.Id,
            OrderNo = model.OrderNo,
            ActionType = ActionType.Accept.ToString(),
            AddBy = await _userService.GetUserNameAsync(),
            AddTime = DateTime.Now,
            Remark = $"订单[{model.OrderNo}]签收完成"
        }).ExecuteCommandAsync();

        //更改订单为已签收状态
        model.Status = 4;
        return await _orderService.UpdateAsync(model) > 0;
    }

    /// <summary>
    /// 完成订单(管理员)
    /// </summary>
    private async Task<bool> CompleteAsync(long id)
    {
        return await CompleteOrderAsync(x => x.Id == id);
    }

    /// <summary>
    /// 作废订单(管理员)
    /// </summary>
    private async Task<bool> InvalidAsync(long id)
    {
                                                               //根据ID获取记录
        var model = await _orderService.SingleAsync(x => x.Id == id);
        //如果不存在则抛出异常
        if (model == null)
        {
            throw Oops.Oh("订单不存在或已删除");
        }
        _orderService.Context.Tracking(model);
        //新增订单日志
        await _orderService.Context.Insertable<OrderLog>(new OrderLog
        {
            OrderId = model.Id,
            OrderNo = model.OrderNo,
            ActionType = ActionType.Invalid.ToString(),
            AddBy = await _userService.GetUserNameAsync(),
            AddTime = DateTime.Now,
            Remark = $"订单[{model.OrderNo}]作废"
        }).ExecuteCommandAsync();
        //返还库存处理
        var productIds = model.OrderGoods.Select(x => x.ProductId);//货品列表
        var productList = await _orderService.Context.Queryable<ShopGoodsProduct>()
            .Includes(x => x.ShopGoods)
            .Where(x => x.ShopGoods != null && x.ShopGoods.Status == 0 && productIds.Contains(x.Id)).ToListAsync();

        List<ShopGoodsProduct> shopGoodsProductUpdateList = new List<ShopGoodsProduct>();
        foreach (var modelt in productList)
        {
            if (modelt.ShopGoods == null)
            {
                continue;
            }
            //取得货品的购买数量
            var orderGoods = model.OrderGoods.FirstOrDefault(x => x.ProductId == modelt.Id);
            //货品及商品减库存，销售增加
            if (orderGoods != null)
            {
                _orderService.Context.Tracking(modelt);
                shopGoodsProductUpdateList.Add(modelt);
                modelt.StockQuantity += orderGoods.Quantity;
                modelt.ShopGoods.StockQuantity += orderGoods.Quantity;
                modelt.ShopGoods.SaleCount = modelt.ShopGoods.SaleCount - orderGoods.Quantity;
            }
        }
        if (shopGoodsProductUpdateList.IsAny())
        {
            await _orderService.Context.Updateable<ShopGoodsProduct>(shopGoodsProductUpdateList).ExecuteCommandAsync();
        }
        //修改订单状态
        model.Status = 7;
        return await _orderService.UpdateAsync(model)>0;
    }

    /// <summary>
    /// 取消订单(客户)
    /// </summary>
    private async Task<bool> CancelAsync(long id)
    {
        //获取当前登录用户
        var userId = await  _userService.GetUserIdAsync();
        if (!await _orderService.Context.Queryable<Members>().AnyAsync(x => x.UserId == userId))
        {
            throw Oops.Oh("会员未登录或已超时");
        }
        //根据ID获取订单记录
        var model = await _orderService.AsQueryable()
            .Includes(x => x.Collection)
            .Includes(x=>x.OrderGoods)
            .SingleAsync(x => x.Id == id && x.UserId == userId);
        //如果不存在则抛出异常
        if (model == null || model.Collection == null)
        {
            throw Oops.Oh("订单不存在或已删除");
        }
        if (model.PaymentStatus == 1)
        {
            throw Oops.Oh("订单已支付，无法取消");
        }
        _orderService.Context.Tracking(model);
        //新增订单日志
        await _orderService.Context.Insertable<OrderLog>(new OrderLog
        {
            OrderId = model.Id,
            OrderNo = model.OrderNo,
            ActionType = ActionType.Invalid.ToString(),
            AddBy = await _userService.GetUserNameAsync(),
            AddTime = DateTime.Now,
            Remark = $"订单[{model.OrderNo}]取消"
        }).ExecuteCommandAsync();
        //返还库存处理
        var productIds = model.OrderGoods.Select(x => x.ProductId);//货品列表
        var productList = await _orderService.Context.Queryable<ShopGoodsProduct>().Includes(x => x.ShopGoods)
            .Where(x => x.ShopGoods.Id > 0  && x.ShopGoods.Status == 0 && productIds.Contains(x.Id)).ToListAsync();
        List<ShopGoodsProduct> shopGoodsProductUpdateList = new List<ShopGoodsProduct>();
        foreach (var modelt in productList)
        {
            if (modelt.ShopGoods == null)
            {
                continue;
            }
            //取得货品的购买数量
            var orderGoods = model.OrderGoods.FirstOrDefault(x => x.ProductId == modelt.Id);
            if (orderGoods != null)
            {
                _orderService.Context.Tracking(modelt);
                shopGoodsProductUpdateList.Add(modelt);
                //货品及商品减库存，销售增加
                modelt.StockQuantity += orderGoods.Quantity;
                modelt.ShopGoods.StockQuantity += orderGoods.Quantity;
                modelt.ShopGoods.SaleCount = modelt.ShopGoods.SaleCount - orderGoods.Quantity;
            }
        }
        if (shopGoodsProductUpdateList.IsAny())
        {
            await _orderService.Context.Updateable<ShopGoodsProduct>(shopGoodsProductUpdateList).ExecuteCommandAsync();
        }
        //修改订单状态
        model.Status = 6;
        model.Collection.Status = 3;//取消付款
        await _orderService.Context.Updateable<PaymentCollection>(model.Collection).UpdateColumns(nameof(PaymentCollection.Status)).ExecuteCommandAsync();
        return await _orderService.UpdateAsync(model) > 0;
    }

    /// <summary>
    /// 确认收货(客户)
    /// </summary>
    private async Task<bool> ConfirmAsync(long id)
    {
                                                               //获取当前登录用户
        var userId = await  _userService.GetUserIdAsync();
        if (!await _orderService.Context.Queryable<Members>().AnyAsync(x => x.UserId == userId))
        {
            throw Oops.Oh("会员未登录或已超时");
        }
        return await CompleteOrderAsync(x => x.Id == id && (userId.IsNullOrEmpty() || x.UserId == userId));
    }


    /// <summary>
    /// 1.新增抢购订单
    /// </summary>
    private async Task<PaymentCollection> AddRushOrder(OrdersEditDto modelDto)
    {
        //获取订单配置信息
        var jsonData = await _orderService.Context.Queryable<SysConfig>().Where(x=>x.Type == ConfigType.OrderConfig.ToString()).FirstAsync();
        var orderConfig = JSON.Deserialize<OrderConfigDto>(jsonData?.JsonData);
        if (orderConfig == null)
        {
            throw Oops.Oh("找不到系统的订单配置信息");
        }
        //查找活动详情
        var speedModel = await _orderService.Context.Queryable<ShopSpeed>().FirstAsync(
            x => x.Status == 0
            && x.SiteId == modelDto.siteId
            && DateTime.Compare(x.StartTime, DateTime.Now) <= 0
            && DateTime.Compare(x.EndTime, DateTime.Now) >= 0
            && x.Id == modelDto.activeId);
        if (speedModel == null)
        {
            throw Oops.Oh($"抢购活动[{modelDto.activeId}]不存在或已过期");
        }
        //检查会员组是否有资格参与本次兑换活动
        var memberModel = await _orderService.Context.Queryable<Members>()
            .Includes(x => x.User)
            .SingleAsync(x => x.UserId == modelDto.userId);
        if (memberModel == null || memberModel.UserId.IsNullOrEmpty())
        {
            throw Oops.Oh("会员账户不存在或已删除");
        }
        var groupIdArr = speedModel.GroupIds.ToIEnumerable<int>();
        if (groupIdArr != null && !groupIdArr.Contains(memberModel.GroupId))
        {
            throw Oops.Oh("没有资格参与本次抢购活动");
        }
        //获取支付方式信息
        var paymentModel = await _orderService.Context.Queryable<SitePayment>()
            .Includes(x => x.Payment)
            .SingleAsync(x => x.Id == modelDto.paymentId);
        if (paymentModel == null || paymentModel.PaymentId.IsNullOrEmpty())
        {
            throw Oops.Oh("支付方式不存在或已删除");
        }
        //查询配送方式
        var deliveryModel = await _orderService.Context.Queryable<ShopDelivery>()
            .Includes(x => x.DeliveryAreas)
            .SingleAsync(x => x.Id == modelDto.deliveryId && x.Status == 0);
        if (deliveryModel == null)
        {
            throw Oops.Oh("配送方式有误，请确认后操作");
        }

        //创建一条收款单与订单关联
        PaymentCollection collectionModel = new()
        {
            UserId = memberModel.UserId,
            TradeNo = $"TN{SnowflakeIdHelper.GetGuidToNumber()}",
            TradeType = 0,
            PaymentId = paymentModel.Id,
            PaymentType = paymentModel.Payment.Type == 0 ? (byte)1 : (byte)0,
            PaymentTitle = paymentModel.Title,
            EndTime = DateTime.Now.AddMinutes(orderConfig.orderExpired),
        };
        //映射DTO到实体
        var model = modelDto.Adapt<Orders>();
        model.OrderGoods.Clear();//清空订单商品以便重新添加
        //model.UserName = await _userService.GetUserNameAsync();
        model.UserName = memberModel.User.Account;//添加用户名
        //拼接收货地址
        var addressArr = model.Address?.Split(",");
        if (addressArr?.Length > 0)
        {
            model.Address = $"{model.Province},{model.City},{model.Area},{addressArr[^1]}";
        }
        //如果是货到付款则将订单更改为待发货状态
        if (paymentModel.Payment.Type == 0)
        {
            model.Status = 2;
        }
        //查询商品货品
        var productModel = await _orderService.Context.Queryable<ShopGoodsProduct>()
            .Includes(x => x.ShopGoods)
            .SingleAsync(x => x.ShopGoods != null && x.ShopGoods.Status == 0 && x.Id == speedModel.ProductId);
        if (productModel == null || productModel.ShopGoods == null)
        {
            throw Oops.Oh("商品不存在或已下架");
        }
        //检查库存数量
        if (productModel.StockQuantity < 1)
        {
            throw Oops.Oh("商品已抢购完毕");
        }
        _orderService.Context.Tracking(productModel);
        //正式赋值给订单货品
        int quantityNum = 1;
        model.OrderGoods.Add(new OrderGoods
        {
            GoodsId = productModel.GoodsId,
            ProductId = productModel.Id,
            GoodsNo = productModel.GoodsNo,
            GoodsTitle = productModel.ShopGoods.Title,
            SpecText = productModel.SpecText,
            ImgUrl = productModel.ImgUrl != null ? productModel.ImgUrl : productModel.ShopGoods.ImgUrl,
            GoodsPrice = productModel.SellPrice,
            RealPrice = speedModel.Price,
            Quantity = quantityNum,
            Weight = productModel.Weight
        });
        //正式赋值给订单总积分和经验值
        model.Point += productModel.ShopGoods.Point * quantityNum;
        model.Exp += productModel.ShopGoods.Exp * quantityNum;
        //货品及商品减库存，销售增加
        productModel.StockQuantity -= quantityNum;
        productModel.ShopGoods.StockQuantity -= quantityNum;
        productModel.ShopGoods.SaleCount += quantityNum;
        await _orderService.Context.Updateable<ShopGoodsProduct>(productModel).ExecuteCommandAsync();

        //生成订单号
        model.OrderNo = $"SN{SnowflakeIdHelper.GetGuidToNumber()}";
        //计算订单应付商品总金额
        model.PayableAmount = model.OrderGoods.Select(x => x.GoodsPrice * x.Quantity).Sum();
        //计算订单实付商品总金额
        model.RealAmount = model.OrderGoods.Select(x => x.RealPrice * x.Quantity).Sum();
        //应付总运费
        int totalWeight = model.OrderGoods.Select(x => x.Weight).Sum();//总商品重量(克)
        model.PayableFreight = GetPayableFreightAsync(deliveryModel,
            model.Province,
            totalWeight > 1000 ? totalWeight / 1000 : 1,
            model.IsInsure, model.InsurePrice);
        //实付总运费
        model.RealFreight = model.PayableFreight;
        if (productModel.ShopGoods.IsDeliveryFee == 1)
        {
            model.RealFreight = 0;
        }
        else if (modelDto.realFreight != null)
        {
            model.RealFreight = modelDto.realFreight.GetValueOrDefault();
        }
        //订单总金额=实付商品总金额+实付总运费-优惠券金额-促销优惠金额-订单折扣或涨价
        model.OrderAmount = model.RealAmount + model.RealFreight - model.CouponAmount - model.PromotionAmount - model.DiscountAmount;
        //同步支付收款
        collectionModel.PaymentAmount = model.OrderAmount;
        collectionModel.Orders.Add(model);

        await _orderService.Context.Insertable<PaymentCollection>(collectionModel).ExecuteCommandAsync();
        var result = await _orderService.InsertAsync(model) > 0;

        ////保存订单
        //await _context.Set<PaymentCollection>().AddAsync(collectionModel);
        //var result = await this.SaveAsync();
        if (!result)
        {
            throw Oops.Oh("订单保存时发生意外错误");
        }
        return collectionModel;
    }

    /// <summary>
    /// 2.新增积分兑换订单
    /// </summary>
    private async Task<PaymentCollection> AddPointOrder(OrdersEditDto modelDto)
    {
        //获取订单配置信息
        var jsonData = await _orderService.Context.Queryable<SysConfig>().Where(x => x.Type == ConfigType.OrderConfig.ToString()).FirstAsync();
        var orderConfig = JSON.Deserialize<OrderConfigDto>(jsonData?.JsonData);
        if (orderConfig == null)
        {
            throw Oops.Oh("找不到系统的订单配置信息");
        }
        //查找积分换购详情
        var pointModel = await _orderService.Context.Queryable<ShopConvert>().SingleAsync(
            x => x.Status == 0
            && x.SiteId == modelDto.siteId
            && x.Id == modelDto.activeId);
        if (pointModel == null)
        {
            throw Oops.Oh($"积分兑换[{modelDto.activeId}]不存在或已失效");
        }
        //检查会员组是否有资格参与本次兑换活动
        var memberModel = await _orderService.Context.Queryable<Members>()
            .Includes(x => x.User)
            .SingleAsync(x => x.UserId == modelDto.userId);
        if (memberModel == null || memberModel.UserId.IsNullOrEmpty())
        {
            throw Oops.Oh($"会员账户不存在或已删除");
        }
        var groupIdArr = pointModel.GroupIds.ToIEnumerable<int>();
        if (groupIdArr != null && !groupIdArr.Contains(memberModel.GroupId))
        {
            throw Oops.Oh($"没有资格参与本次兑换活动");
        }
        //检查会员账户积分是否能抵扣
        if (memberModel.Point < pointModel.Point)
        {
            throw Oops.Oh($"会员账户积分不足，无法兑换");
        }
        //获取支付方式信息
        var paymentModel = await _orderService.Context.Queryable<SitePayment>()
            .Includes(x => x.Payment)
            .SingleAsync(x => x.Id == modelDto.paymentId);
        if (paymentModel == null || paymentModel.Payment == null)
        {
            throw Oops.Oh("支付方式不存在或已删除");
        }
        //查询配送方式
        var deliveryModel = await _orderService.Context.Queryable<ShopDelivery>()
            .Includes(x => x.DeliveryAreas)
            .SingleAsync(x => x.Id == modelDto.deliveryId && x.Status == 0);
        if (deliveryModel == null)
        {
            throw Oops.Oh("配送方式有误，请确认后操作");
        }
        //创建一条收款单与订单关联
        PaymentCollection collectionModel = new()
        {
            UserId = memberModel.UserId,
            TradeNo = $"TN{SnowflakeIdHelper.GetGuidToNumber()}",
            TradeType = 0,
            PaymentId = paymentModel.Id,
            PaymentType = paymentModel.Payment.Type == 0 ? (byte)1 : (byte)0,
            PaymentTitle = paymentModel.Title,
            EndTime = DateTime.Now.AddMinutes(orderConfig.orderExpired),
            Orders = new List<Orders>()
        };
        //映射DTO到实体
        var model = modelDto.Adapt<Orders>();
        model.OrderGoods.Clear();//清空订单商品以便重新添加
        model.UserName = memberModel.User?.Account; // await _userService.GetUserNameAsync();//添加用户名
                                                   //拼接收货地址
        var addressArr = model.Address?.Split(",");
        if (addressArr?.Length > 0)
        {
            model.Address = $"{model.Province},{model.City},{model.Area},{addressArr[^1]}";
        }
        //如果是货到付款则将订单更改为待发货状态
        if (paymentModel.Payment.Type == 0)
        {
            model.Status = 2;
        }
        //查询货品信息
        var productModel = await _orderService.Context.Queryable<ShopGoodsProduct>()
            .Includes(x => x.ShopGoods)
            .SingleAsync(x => x.Id == pointModel.ProductId && x.ShopGoods != null && x.ShopGoods.Status == 0);
        if (productModel == null || productModel.ShopGoods == null)
        {
            throw Oops.Oh("商品不存在或已下架");
        }
        //检查库存数量
        int quantityNum = 1;
        if (productModel.StockQuantity < quantityNum)
        {
            throw Oops.Oh($"商品[{productModel.GoodsId}]库存不足");
        }
        _orderService.Context.Tracking(productModel);
        //正式赋值给订单货品
        model.OrderGoods.Add(new OrderGoods
        {
            GoodsId = productModel.GoodsId,
            ProductId = productModel.Id,
            GoodsNo = productModel.GoodsNo,
            GoodsTitle = productModel.ShopGoods.Title,
            SpecText = productModel.SpecText,
            ImgUrl = productModel.ImgUrl != null ? productModel.ImgUrl : productModel.ShopGoods.ImgUrl,
            GoodsPrice = productModel.SellPrice,
            RealPrice = 0,
            Quantity = quantityNum,
            Weight = productModel.Weight
        });
        //正式赋值给订单总积分和经验值(累加)
        model.Point += -pointModel.Point * quantityNum;
        model.Exp += productModel.ShopGoods.Exp * quantityNum;
        //货品及商品减库存，销售增加
        productModel.StockQuantity -= quantityNum;
        productModel.ShopGoods.StockQuantity -= quantityNum;
        productModel.ShopGoods.SaleCount += quantityNum;
        await _orderService.Context.Updateable<ShopGoodsProduct>(productModel).ExecuteCommandAsync();

        //生成订单号
        model.OrderNo = $"PN{SnowflakeIdHelper.GetGuidToNumber()}";
        //订单应付商品总金额
        model.PayableAmount = model.OrderGoods.Select(x => x.GoodsPrice * x.Quantity).Sum();
        //订单实付商品总金额
        model.RealAmount = 0;
        //应付总运费
        int totalWeight = model.OrderGoods.Select(x => x.Weight * x.Quantity).Sum();//总商品重量(克)
        model.PayableFreight = GetPayableFreightAsync(deliveryModel,
            model.Province,
            totalWeight > 1000 ? totalWeight / 1000 : 1,
            model.IsInsure, model.InsurePrice);
        //实付总运费
        model.RealFreight = model.PayableFreight;
        if (productModel.ShopGoods.IsDeliveryFee == 1)
        {
            model.RealFreight = 0;
        }
        else if (modelDto.realFreight != null)
        {
            model.RealFreight = modelDto.realFreight.GetValueOrDefault();
        }
        //订单总金额=实付商品总金额+实付总运费-优惠券金额-促销优惠金额-订单折扣或涨价
        model.OrderAmount = model.RealAmount + model.RealFreight - model.CouponAmount - model.PromotionAmount - model.DiscountAmount;
        if (model.OrderAmount == 0)
        {
            model.Status = 2;
            model.PaymentStatus = 1;
            model.PaymentTime = DateTime.Now;
            //更改支付收款单
            collectionModel.Status = 2;
            collectionModel.CompleteTime = DateTime.Now;
        }
        //同步支付收款
        collectionModel.PaymentAmount = model.OrderAmount;
        collectionModel.Orders.Add(model);
        //增加会员积分记录
        await _orderService.Context.Insertable<MemberPointLog>(new MemberPointLog
        {
            UserId = model.UserId,
            Value = pointModel.Point * -1,
            Remark = $"兑换商品，订单号:{model.OrderNo}"
        }).ExecuteCommandAsync();
        //扣减会员账户积分
        memberModel.Point -= pointModel.Point;
        await _orderService.Context.Updateable<Members>(memberModel).UpdateColumns(nameof(Members.Point)).ExecuteCommandAsync();
        //保存收款及订单
        await _orderService.Context.Insertable<PaymentCollection>(collectionModel).ExecuteCommandAsync();

        await _orderService.InsertAsync(model);

        //保存积分兑换记录
        await _orderService.Context.Insertable<ShopConvertHistory>(new ShopConvertHistory
        {
            ConvertId = pointModel.Id,
            UserId = model.UserId,
            OrderId = model.Id,
            Status = 1
        }).ExecuteCommandAsync();

        ////开启事务
        //using (var transaction = await _context.Database.BeginTransactionAsync())
        //{
        //    try
        //    {
        //        //先保存之前的修改
        //        await this.SaveAsync();
        //        //保存积分兑换记录
        //        await _context.Set<ShopConvertHistory>().AddAsync(new ShopConvertHistory
        //        {
        //            ConvertId = pointModel.Id,
        //            UserId = model.UserId,
        //            OrderId = model.Id,
        //            Status = 1
        //        });
        //        await this.SaveAsync();
        //        //提交事务
        //        await transaction.CommitAsync();
        //    }
        //    catch
        //    {
        //        //回滚事务
        //        await transaction.RollbackAsync();
        //        throw Oops.Oh("订单保存时发生意外错误");
        //    }
        //}
        return collectionModel;
    }

    /// <summary>
    /// 0.新增普通订单
    /// </summary>
    private async Task<PaymentCollection> AddNormalOrder(OrdersEditDto modelDto)
    {
        //获取订单配置信息
        var jsonData = await _orderService.Context.Queryable<SysConfig>().Where(x => x.Type == ConfigType.OrderConfig.ToString()).FirstAsync();
        var orderConfig = JSON.Deserialize<OrderConfigDto>(jsonData?.JsonData);
        if (orderConfig == null)
        {
            throw Oops.Oh("找不到系统的订单配置信息");
        }
        //获取会员信息
        var memberModel = await _orderService.Context.Queryable<Members>()
            .Includes(x => x.User)
            .Includes(x => x.Group)
            .SingleAsync(x => x.UserId == modelDto.userId);
        if (memberModel == null || memberModel.UserId.IsNullOrEmpty() || memberModel.Group == null)
        {
            throw Oops.Oh($"会员账户不存在或已删除");
        }
        //查询支付方式
        var paymentModel = await _orderService.Context.Queryable<SitePayment>()
            .Includes(x => x.Payment).SingleAsync(x => x.Id == modelDto.paymentId);
        if (paymentModel == null || paymentModel.Payment == null)
        {
            throw Oops.Oh("支付方式有误，请确认后操作");
        }
        //查询配送方式
        var deliveryModel = await _orderService.Context.Queryable<ShopDelivery>()
            .Includes(x => x.DeliveryAreas)
            .SingleAsync(x => x.Id == modelDto.deliveryId && x.Status == 0);
        if (deliveryModel == null)
        {
            throw Oops.Oh("配送方式有误，请确认后操作");
        }

        //创建一条收款单与订单关联
        PaymentCollection collectionModel = new()
        {
            UserId = memberModel.UserId,
            TradeNo = $"TN{SnowflakeIdHelper.GetGuidToNumber()}",
            TradeType = 0,
            PaymentId = paymentModel.Id,
            PaymentType = paymentModel.Payment.Type == 0 ? (byte)1 : (byte)0,
            PaymentTitle = paymentModel.Title,
            EndTime = DateTime.Now.AddMinutes(orderConfig.orderExpired),
            Orders = new List<Orders>()
        };
        //映射DTO到实体
        var model = modelDto.Adapt<Orders>();
        model.OrderGoods.Clear();//清空订单商品以便重新添加
        model.UserName = memberModel.User?.Account; // await _userService.GetUserNameAsync();//添加用户名
                                                   //拼接收货地址
        var addressArr = model.Address?.Split(",");
        if (addressArr?.Length > 0)
        {
            model.Address = $"{model.Province},{model.City},{model.Area},{addressArr[^1]}";
        }
        //如果是货到付款则将订单更改为待发货状态
        if (paymentModel.Payment.Type == 0)
        {
            model.Status = 2;
        }
        //查询商品货品列表
        var productIds = modelDto.orderGoods.Select(x => x.productId);//货品列表
        var productList = await _orderService.Context.Queryable<ShopGoodsProduct>()
            .Includes(x => x.ShopGoods,z=>z.CategoryRelations)
            .Includes(x => x.GroupPrices)
            .Where(x => x.ShopGoods.Status == 0 && productIds.Contains(x.Id)).ToListAsync();
        if (productList == null)
        {
            throw Oops.Oh("商品不存在或已下架");
        }
        //检查是否全部免运费
        bool isDeliveryFee = true;
        //遍历商品货品列表
        List<ShopGoodsProduct> shopGoodsProductUpdateList = new List<ShopGoodsProduct>();
        foreach (var modelt in productList)
        {
            if (modelt.ShopGoods == null)
            {
                continue;
            }
            //如果有任何一件不是免运费，则需要收取运费
            if (modelt.ShopGoods.IsDeliveryFee == 0)
            {
                isDeliveryFee = false;
            }
            //查询会员组价格
            var groupPrice = modelt.GroupPrices.FirstOrDefault(x => x.GroupId == memberModel.GroupId);
            //取得DTO对应货品的购买数量
            var dtoOrderGoods = modelDto.orderGoods.FirstOrDefault(x => x.productId == modelt.Id);
            var quantityNum = dtoOrderGoods != null ? dtoOrderGoods.quantity : 0;
            //检查最少购买数量
            if (quantityNum < modelt.MinQuantity)
            {
                throw Oops.Oh($"商品至少购买[{modelt.MinQuantity}]件");
            }
            //检查库存数量
            if (modelt.StockQuantity < quantityNum)
            {
                throw Oops.Oh($"商品[{modelt.GoodsId}]库存不足");
            }

            //正式赋值给订单货品
            model.OrderGoods.Add(new OrderGoods
            {
                GoodsId = modelt.GoodsId,
                ProductId = modelt.Id,
                GoodsNo = modelt.GoodsNo,
                GoodsTitle = modelt.ShopGoods.Title,
                SpecText = modelt.SpecText,
                ImgUrl = modelt.ImgUrl != null ? modelt.ImgUrl : modelt.ShopGoods.ImgUrl,
                GoodsPrice = modelt.SellPrice,
                RealPrice = groupPrice != null ? groupPrice.Price : modelt.SellPrice * ((decimal)memberModel.Group.Discount / 100),
                Quantity = quantityNum,
                Weight = modelt.Weight
            });
            //正式赋值给订单总积分和经验值(累加)
            model.Point += modelt.ShopGoods.Point * quantityNum;
            model.Exp += modelt.ShopGoods.Exp * quantityNum;
            //货品及商品减库存，销售增加
            _orderService.Context.Tracking(modelt);
            shopGoodsProductUpdateList.Add(modelt);
            modelt.StockQuantity -= quantityNum;
            modelt.ShopGoods.StockQuantity -= quantityNum;
            modelt.ShopGoods.SaleCount += quantityNum;
            //_context.Set<ShopGoodsProduct>().Update(modelt);
        }

        if (shopGoodsProductUpdateList.IsAny())
        {
            await _orderService.Context.Updateable(shopGoodsProductUpdateList).ExecuteCommandAsync();
        }

        //生成订单号
        model.OrderNo = $"BN{SnowflakeIdHelper.GetGuidToNumber()}";
        //计算订单应付商品总金额
        model.PayableAmount = model.OrderGoods.Select(x => x.GoodsPrice * x.Quantity).Sum();
        //计算订单实付商品总金额
        model.RealAmount = model.OrderGoods.Select(x => x.RealPrice * x.Quantity).Sum();
        //应付总运费
        int totalWeight = model.OrderGoods.Select(x => x.Weight * x.Quantity).Sum();//总商品重量(克)
        model.PayableFreight = GetPayableFreightAsync(deliveryModel,
            model.Province,
            totalWeight > 1000 ? totalWeight / 1000 : 1,
            model.IsInsure, model.InsurePrice);
        //实付总运费
        model.RealFreight = model.PayableFreight;
        if (isDeliveryFee)
        {
            model.RealFreight = 0;//所有的商品都是免运费
        }
        else if (modelDto.realFreight != null)
        {
            model.RealFreight = modelDto.realFreight.GetValueOrDefault();
        }

        //获取会员优惠券
        if (modelDto.useCouponId > 0)
        {
            //查询优惠券信息
            var useCouponModel = await _orderService.Context.Queryable<ShopCouponHistory>()
                .Includes(x => x.ShopCoupon,z=>z.GoodsRelations)
                .Where(x=> x.Id == modelDto.useCouponId
                    && x.Status == 0
                    && x.IsUse == 0
                    && x.UserId == model.UserId
                    && x.ShopCoupon.Id>0
                    && SqlFunc.Between(DateTime.Now, x.ShopCoupon.StartTime, x.ShopCoupon.EndTime))
                .FirstAsync();
            if (useCouponModel == null || useCouponModel.ShopCoupon == null)
            {
                throw Oops.Oh($"优惠券[{modelDto.useCouponId}]不存在或已过期");
            }
            //检查优惠券是否可用
            if (model.OrderGoods.Select(x => x.GoodsPrice * x.Quantity).Sum() < useCouponModel.ShopCoupon.Amount)
            {
                throw Oops.Oh($"商品金额不能低于优惠券金额");
            }
            //0.店铺券，需满足消费金额
            if (useCouponModel.ShopCoupon.UseType == 0)
            {
                if (useCouponModel.ShopCoupon.MinAmount > 0 && model.OrderGoods.Select(x => x.GoodsPrice * x.Quantity).Sum() < useCouponModel.ShopCoupon.MinAmount)
                {
                    throw Oops.Oh($"优惠券不可用，请检查重试");
                }
            }
            //1.指定分类,需检查商品类别
            else if (useCouponModel.ShopCoupon.UseType == 1)
            {
                //取出优惠券可用商品分类ID
                var couponCategoryIds = useCouponModel.ShopCoupon.CategoryRelations.Select(x => x.CategoryId);
                //查找符合条件的商品
                var totalList = productList.Where(
                    x => x.ShopGoods != null && x.ShopGoods.CategoryRelations.Select(x => x.CategoryId).Intersect(couponCategoryIds).Count() > 0).ToList();
                if (totalList == null || totalList.Select(x => x.SellPrice).Sum() < useCouponModel.ShopCoupon.MinAmount)
                {
                    throw Oops.Oh($"优惠券不可用，请检查重试");
                }
            }
            //2.商品券，需满足消费金额及对应的商品
            else if (useCouponModel.ShopCoupon.UseType == 2)
            {
                //取出优惠券可用商品ID
                var couponGoodsIds = useCouponModel.ShopCoupon.GoodsRelations.Select(x => x.GoodsId);
                //查找符合条件的商品
                var orderGoodsIds = model.OrderGoods.Where(x => couponGoodsIds.Contains(x.GoodsId)).ToList();
                if (orderGoodsIds == null || orderGoodsIds.Select(x => x.GoodsPrice * x.Quantity).Sum() < useCouponModel.ShopCoupon.MinAmount)
                {
                    throw Oops.Oh($"优惠券不可用，请检查重试");
                }
            }
            _orderService.Context.Tracking(useCouponModel);
            //修改订单中的优惠金额
            model.CouponAmount = useCouponModel.ShopCoupon.Amount;
            //修改可用优惠券的状态
            useCouponModel.OrderNo = model.OrderNo;
            useCouponModel.IsUse = 1;
            useCouponModel.Status = 1;
            useCouponModel.UseTime = DateTime.Now;
            useCouponModel.ShopCoupon.UseCount += 1;
            await _orderService.Context.Updateable<ShopCouponHistory>(useCouponModel).ExecuteCommandAsync();
        }

        //查询有无符合的购物车活动优惠
        var promotionModel = await _orderService.Context.Queryable<ShopPromotion>().SingleAsync(x => x.Status == 0
            && x.SiteId == modelDto.siteId
            && (x.GroupIds == null || x.GroupIds.Contains($",{memberModel.GroupId},"))
            && SqlFunc.Between(DateTime.Now,x.StartTime,x.EndTime));
        if (promotionModel != null && model.RealAmount > promotionModel.Condition)
        {
            //奖励阶梯满减
            if (promotionModel.AwardType == 1)
            {
                //计算阶梯满减倍数
                var discountNum = (int)(model.RealAmount / promotionModel.Condition);
                //阶梯满减金额=阶梯满减倍数*满减金额
                var discountAmount = discountNum * promotionModel.AwardValue;
                //优惠总金额=原优惠金额+阶梯满减金额
                model.PromotionAmount += discountAmount;
            }
            //奖励折扣
            else if (promotionModel.AwardType == 2)
            {
                if (promotionModel.AwardValue < 1 || promotionModel.AwardValue > 100)
                {
                    throw Oops.Oh($"活动{promotionModel.Id}折扣设置有误");
                }
                //折扣金额=原实付*折扣
                var discountAmount = model.RealAmount * ((decimal)promotionModel.AwardValue / 100);
                discountAmount = Math.Round(model.RealAmount - discountAmount);
                //优惠总金额=原优惠金额+折扣金额
                model.PromotionAmount += discountAmount;
            }
            //赠送积分
            else if (promotionModel.AwardType == 3)
            {
                model.Point += Convert.ToInt32(promotionModel.AwardValue);
            }
            //赠送赠品
            else if (promotionModel.AwardType == 5)
            {
                //查询赠品商品货品
                var productModel = await _orderService.Context.Queryable<ShopGoodsProduct>()
                    .Includes(x => x.ShopGoods)
                    .SingleAsync(x => x.Id == promotionModel.AwardValue && x.ShopGoods != null && x.ShopGoods.Status == 0);
                if (productModel != null && productModel.ShopGoods != null)
                {
                    model.OrderGoods.Add(new OrderGoods
                    {
                        GoodsId = productModel.GoodsId,
                        ProductId = productModel.Id,
                        GoodsNo = productModel.GoodsNo,
                        GoodsTitle = $"{productModel.ShopGoods.Title}[赠品]",
                        SpecText = productModel.SpecText,
                        ImgUrl = productModel.ImgUrl != null ? productModel.ImgUrl : productModel.ShopGoods.ImgUrl,
                        GoodsPrice = 0,
                        RealPrice = 0,
                        Quantity = 1,
                        Weight = 0
                    });
                }
            }
            //免运费
            else if (promotionModel.AwardType == 6)
            {
                model.RealFreight = 0;
            }
            //赋值订单参与的促销活动
            model.ActiveId = promotionModel.Id;
            if (model.OrderPromotion==null)
            {
                model.OrderPromotion = new List<OrderPromotion>();
            }
            model.OrderPromotion.Add(new()
            {
                PromotionId = promotionModel.Id
            });
        }

        //订单总金额=实付商品总金额+实付总运费-优惠券金额-促销优惠金额-订单折扣或涨价
        model.OrderAmount = model.RealAmount + model.RealFreight - model.CouponAmount - model.PromotionAmount - model.DiscountAmount;
        //同步支付收款
        collectionModel.PaymentAmount = model.OrderAmount;
        collectionModel.Orders.Add(model);

        //删除购物车对应的货品
        var cartList = await _orderService.Context.Queryable<ShopCart>().Where(x => x.UserId == modelDto.userId && productIds.Contains(x.ProductId)).ToListAsync();
        await _orderService.Context.Deleteable<ShopCart>(cartList).ExecuteCommandAsync();

        //保存订单
        //var result = await _orderService.Context.InsertNav<PaymentCollection>(collectionModel).Include(x=>x.Orders).ExecuteCommandAsync();

        // 1、收付款记录
        await _orderService.Context.Insertable<PaymentCollection>(collectionModel).ExecuteReturnEntityAsync();

        // 2、订单记录 + 促销互动
        model.CollectionId = collectionModel.Id;
        var result = await _orderService.Context.InsertNav<Orders>(model).IncludesAllFirstLayer().ExecuteCommandAsync();

        if (!result)
        {
            throw Oops.Oh("订单保存时发生意外错误");
        }

        return collectionModel;
    }

    /// <summary>
    /// 完成订单
    /// </summary>
    private async Task<bool> CompleteOrderAsync(Expression<Func<Orders, bool>> funcWhere)
    {
        //根据ID获取记录
        var model = await _orderService.AsQueryable()
            .Includes(x => x.Collection).SingleAsync(funcWhere);
        //如果不存在则抛出异常
        if (model == null || model.Collection == null)
        {
            throw Oops.Oh("订单不存在或已删除");
        }
        if (model.Status > 4)
        {
            throw Oops.Oh("订单已完成，无法操作");
        }
        if (model.DeliveryStatus != 1)
        {
            throw Oops.Oh("订单未发货，无法操作");
        }
        //检查是否线下支付
        if (model.Collection.PaymentType == 1)
        {
            model.PaymentStatus = 1;
            model.PaymentTime = DateTime.Now;
            model.Collection.Status = 2;
            model.Collection.CompleteTime = DateTime.Now;
        }
        else if (model.PaymentStatus == 0)
        {
            throw Oops.Oh("订单未付款，无法操作");
        }
        _orderService.Context.Tracking(model);
        //检查订单促销活动
        if (model.OrderType == 0 && model.ActiveId > 0)
        {
            //查询促销活动详情
            var promotionModel = await _orderService.Context.Queryable<ShopPromotion>()
                .SingleAsync(x => x.Id == model.ActiveId);
            //赠送优惠券
            if (promotionModel != null && promotionModel.AwardType == 4)
            {
                if (await _orderService.Context.Queryable<ShopCoupon>().AnyAsync(x => x.Id == promotionModel.AwardValue))
                {
                    await _orderService.Context.Insertable<ShopCouponHistory>(new ShopCouponHistory()
                    {
                        CouponId = promotionModel.AwardValue,
                        UserId = model.UserId,
                        Type = 0,
                        IsUse = 0,
                        Status = 0,
                        AddTime = DateTime.Now
                    }).ExecuteCommandAsync();
                }
            }
        }
        //会员增加积分及经验值
        if (model.Point > 0)
        {
            var memberModel = await _orderService.Context.Queryable<Members>()
                .Includes(x => x.Group)
                .SingleAsync(x => x.UserId == model.UserId);
            if (memberModel != null && memberModel.Group != null)
            {
                _orderService.Context.Tracking(memberModel);
                memberModel.Point += model.Point;
                memberModel.Exp += model.Exp;
                //检查有无可升级的会员组
                var upgradeGroupModel = await _orderService.Context.Queryable<MemberGroup>().SingleAsync(
                    x => x.Id != memberModel.GroupId
                    && x.IsUpgrade == 1
                    && x.MinExp <= memberModel.Exp
                    && x.MaxExp >= memberModel.Exp);
                if (upgradeGroupModel != null && upgradeGroupModel.Amount >= memberModel.Group.Amount)
                {
                    memberModel.GroupId = (int)upgradeGroupModel.Id;
                }
                await _orderService.Context.Updateable<Members>(memberModel).ExecuteCommandAsync();
                //增加会员积分记录
                await _orderService.Context.Insertable<MemberPointLog>(new MemberPointLog
                {
                    UserId = model.UserId,
                    Value = model.Point,
                    Remark = $"获得积分，订单号:{model.OrderNo}"
                }).ExecuteCommandAsync();
            }
        }
        //新增订单日志
        await _orderService.Context.Insertable<OrderLog>(new OrderLog
        {
            OrderId = model.Id,
            OrderNo = model.OrderNo,
            ActionType = ActionType.Complete.ToString(),
            AddBy = await _userService.GetUserNameAsync(),
            AddTime = DateTime.Now,
            Remark = $"订单[{model.OrderNo}]确认完成"
        }).ExecuteCommandAsync();
        //修改订单状态
        model.Status = 5;
        return await _orderService.UpdateAsync(model) >0;
    }

    /// <summary>
    /// 计算配送价格
    /// </summary>
    /// <param name="model">配送方式实体</param>
    /// <param name="province">省份</param>
    /// <param name="totalWeight">总重量(千克)</param>
    /// <param name="isInsure">是否保价</param>
    /// <param name="insurePrice">保价金额</param>
    private decimal GetPayableFreightAsync(ShopDelivery model, string? province, int totalWeight, int isInsure, decimal insurePrice)
    {
        if (model.IsInsure == 0 && isInsure > 0)
        {
            throw Oops.Oh("当前配送方式不支持保价");
        }
        //重量为0时直接返回首重费用
        if (totalWeight == 0)
        {
            return model.FirstPrice;
        }
        //计算配送费用
        decimal firstPrice = model.FirstPrice;//首重费用
        decimal secondPrice = model.SecondPrice;//续重费用
        decimal totalSecondPrice = 0;//续重总费用
                                     //如果符合自定义地区，采用地区费用
        var areaModel = model.DeliveryAreas.FirstOrDefault(x => x.Province == province);
        if (areaModel != null)
        {
            firstPrice = areaModel.FirstPrice;
            secondPrice = areaModel.SecondPrice;
        }
        //如果总重量大于首重才计算续重费用
        if (totalWeight > model.FirstWeight)
        {
            //续重重量=总重量-首重量
            decimal secondWeight = totalWeight - model.FirstWeight;
            //向上取整，只要有小数都加1
            //续重费用=(续重重量/续重量)*续重价格
            totalSecondPrice = Math.Ceiling(secondWeight / model.SecondWeight) * secondPrice;
        }
        //保价费用=保价金额*保价费率
        decimal insureFreight = 0;
        if (isInsure > 0)
        {
            insureFreight = insurePrice * ((decimal)model.InsureRate / 1000);
            if (insureFreight < model.InsurePrice)
            {
                insureFreight = model.InsurePrice;//最低保价
            }
        }
        //总运费=首重费用+续重费用+保价费用
        return firstPrice + totalSecondPrice + insureFreight;
    }
    #endregion
}
