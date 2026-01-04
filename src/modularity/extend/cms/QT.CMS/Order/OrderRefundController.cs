using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QT.CMS.Emum;
using QT.CMS.Entitys;
using QT.CMS.Entitys.Dto.Order;
using QT.CMS.Entitys.Dto.Parameter;
using QT.CMS.Interfaces;
using QT.Common.Core.Manager;
using QT.Common.Extension;
using QT.FriendlyException;
using QT.JsonSerialization;
using SqlSugar;

namespace QT.CMS;


/// <summary>
/// 订单商品退换货
/// </summary>
[Route("/api/cms/admin/order/refund")]
[ApiController]
public class OrderRefundController : ControllerBase
{
    private readonly ISqlSugarRepository<OrderRefund> _orderRefundService;
    private readonly IUserService _userService;
    private readonly IWeChatExecuteService _weChatExecuteService;
    private readonly IAlipayExecuteService _alipayExecuteService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public OrderRefundController(ISqlSugarRepository<OrderRefund> orderRefundService, IUserService userService,
        IWeChatExecuteService weChatExecuteService, IAlipayExecuteService alipayExecuteService)
    {
        _orderRefundService = orderRefundService;
        _userService = userService;
        _weChatExecuteService = weChatExecuteService;
        _alipayExecuteService = alipayExecuteService;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 根据ID获取数据
    /// type：1退款2换货
    /// 示例：/admin/order/refund/1/1
    /// </summary>
    [HttpGet("{type}/{id}")]
    [Authorize]
    //[AuthorizeFilter("OrderRefund", ActionType.View, "type")]
    public async Task<IActionResult> GetById([FromRoute] int type, [FromRoute] long id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<OrderRefundDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        //.Include(x => x.RefundGoods).ThenInclude(x => x.OrderGoods)
           
        var model = await _orderRefundService.AsQueryable()
            .Includes(x => x.RefundGoods,z=>z.OrderGoods)
            .Includes(x=>x.RefundAlbums)
            .SingleAsync(x => x.Type == type && x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        //使用AutoMapper转换成ViewModel，根据字段进行塑形
        var result = model.Adapt<OrderRefundDto>().ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 获取总记录数量
    /// type：1退款2换货
    /// 示例：/admin/order/refund/view/1/count
    /// </summary>
    [HttpGet("view/{type}/count")]
    [Authorize]
    //[AuthorizeFilter("OrderRefund", ActionType.View, "type")]
    public async Task<IActionResult> GetCount([FromRoute] int type, [FromQuery] RefundParameter searchParam)
    {
        var result = await _orderRefundService.CountAsync(
            x => x.Type == type
            && (searchParam.Status < 0 || x.HandleStatus == searchParam.Status));
        //返回成功200
        return Ok(result);
    }

    /// <summary>
    /// 获取指定数量列表
    /// type：1退款2换货
    /// 示例：/admin/order/refund/view/1/10
    /// </summary>
    [HttpGet("view/{type}/{top}")]
    [Authorize]
    //[AuthorizeFilter("OrderRefund", ActionType.View, "type")]
    public async Task<IActionResult> GetList([FromRoute] int type, [FromRoute] int top, [FromQuery] RefundParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<OrderRefundDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<OrderRefundDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据库列表
        var resultFrom = await _orderRefundService.AsQueryable().Includes(x => x.RefundGoods)
            .Includes(x => x.RefundAlbums)
            .Where(x => x.Type == type)
            .WhereIF(searchParam.OrderNo.IsNotEmptyOrNull(), x => x.OrderNo == searchParam.OrderNo)
            .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.OrderNo.Contains(searchParam.Keyword) || x.UserName.Contains(searchParam.Keyword))
            .OrderBy(searchParam.OrderBy ?? "HandleStatus,-Id")
            .AutoTake(top)
            .ToListAsync();

        //var resultFrom = await _orderRefundService.QueryListAsync(top,
        //    x => x.Type == type
        //    && (!searchParam.OrderNo.IsNotNullOrEmpty()
        //    || (x.OrderNo != null && x.OrderNo.Equals(searchParam.OrderNo)))
        //    && (!searchParam.Keyword.IsNotNullOrEmpty()
        //    || (x.OrderNo != null && x.OrderNo.Contains(searchParam.Keyword))
        //    || (x.UserName != null && x.UserName.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "HandleStatus,-Id");

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<OrderRefundDto>>(); //.ShapeData(searchParam.Fields);
        //返回成功200
        return Ok(resultDto);
    }

    /// <summary>
    /// 获取分页列表
    /// type：1退款2换货
    /// 示例：/admin/order/refund/1?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet("{type}")]
    [Authorize]
    //[AuthorizeFilter("OrderRefund", ActionType.View, "type")]
    public async Task<dynamic> GetList([FromRoute] int type, [FromQuery] RefundParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<OrderRefundDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<OrderRefundDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表，如果ID大于0则查询该用户下所有的列表
        var list = await _orderRefundService.AsQueryable().Includes(x => x.RefundGoods)
            .Includes(x => x.RefundAlbums)
            .Where(x => x.Type == type)
            .WhereIF(searchParam.OrderNo.IsNotEmptyOrNull(), x => x.OrderNo == searchParam.OrderNo)
            .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.OrderNo.Contains(searchParam.Keyword) || x.UserName.Contains(searchParam.Keyword))
            .OrderBy(searchParam.OrderBy ?? "HandleStatus,-Id")
            .ToPagedListAsync(pageParam.PageIndex,pageParam.PageSize);


        return PageResult<OrderRefundDto>.SqlSugarPageResult(list);

        //var list = await _orderRefundService.QueryPageAsync(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => x.Type == type
        //    && (!searchParam.OrderNo.IsNotNullOrEmpty()
        //    || (x.OrderNo != null && x.OrderNo.Equals(searchParam.OrderNo)))
        //    && (!searchParam.Keyword.IsNotNullOrEmpty()
        //    || (x.OrderNo != null && x.OrderNo.Contains(searchParam.Keyword))
        //    || (x.UserName != null && x.UserName.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "HandleStatus,-Id");

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
        //var resultDto = list.Adapt<IEnumerable<OrderRefundDto>>().ShapeData(searchParam.Fields);
        //return Ok(resultDto);
    }

    /// <summary>
    /// 申请退换货
    /// type：1退款2换货
    /// 示例：/admin/order/refund/1
    /// </summary>
    [HttpPost("{type}")]
    [Authorize]
    [SqlSugarUnitOfWork]
    //[AuthorizeFilter("OrderRefund", ActionType.Add, "type")]
    public async Task<IActionResult> Apply([FromRoute] int type, [FromBody] OrderRefundApplyDto modelDto)
    {
        //保存数据
        var result = await this.ApplyAsync(modelDto);
        return Ok(result);
    }

    /// <summary>
    /// 卖家处理:同意,拒绝,退款
    /// type：1退款2换货
    /// 示例：/admin/order/refund/handle/1/1
    /// </summary>
    [HttpPut("handle/{type}/{id}")]
    [Authorize]
    [SqlSugarUnitOfWork]
    //[AuthorizeFilter("OrderRefund", ActionType.Edit, "type")]
    public async Task<IActionResult> Handle([FromRoute] int type, [FromRoute] long id, [FromBody] OrderRefundHandleDto modelDto)
    {
        //保存数据
        var result = await this.HandleAsync(id, modelDto);
        return NoContent();
    }

    /// <summary>
    /// 买家发货
    /// type：1退款2换货
    /// 示例：/admin/order/refund/return/2/1
    /// </summary>
    [HttpPut("return/{type}/{id}")]
    [Authorize]
    [SqlSugarUnitOfWork]
    //[AuthorizeFilter("OrderRefund", ActionType.Edit, "type")]
    public async Task<IActionResult> BuyDelivery([FromRoute] int type, [FromRoute] long id, [FromBody] OrderRefundBuyDto modelDto)
    {
        //保存数据
        var result = await this.BuyDeliveryAsync(id, modelDto);
        return NoContent();
    }

    /// <summary>
    /// 卖家发货
    /// type：1退款2换货
    /// 示例：/admin/order/refund/delivery/2/1
    /// </summary>
    [HttpPut("delivery/{type}/{id}")]
    [Authorize]
    //[AuthorizeFilter("OrderRefund", ActionType.Edit, "type")]
    [SqlSugarUnitOfWork]
    public async Task<IActionResult> SellerDelivery([FromRoute] int type, [FromRoute] long id, [FromBody] OrderRefundSellerDto modelDto)
    {
        //保存数据
        var result = await this.SellerDeliveryAsync(id, modelDto);
        return NoContent();
    }

    /// <summary>
    /// 删除一条记录
    /// type：1退款2换货
    /// 示例：/admin/order/refund/1/1
    /// </summary>
    [HttpDelete("{type}/{id}")]
    [Authorize]
    //[AuthorizeFilter("OrderRefund", ActionType.Delete, "type")]
    public async Task<IActionResult> Delete([FromRoute] int type, [FromRoute] int id)
    {
        if (!await _orderRefundService.AnyAsync(x => x.Id == id))
        {
            throw Oops.Oh($"数据不存在或已删除");
        }
        var result = await _orderRefundService.Context.DeleteNav<OrderRefund>(x => x.Id == id)
            .Include(x => x.RefundAlbums)
            .Include(x => x.RefundGoods)
            .ExecuteCommandAsync();

        return NoContent();
    }

    /// <summary>
    /// 批量删除记录(级联数据)
    /// type：1退款2换货
    /// 示例：/admin/order/refund/1?ids=1,2,3
    /// </summary>
    [HttpDelete("{type}")]
    [Authorize]
    //[AuthorizeFilter("OrderRefund", ActionType.Delete, "type")]
    public async Task<IActionResult> DeleteByIds([FromRoute] int type, [FromQuery] string Ids)
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
        await _orderRefundService.Context.DeleteNav<OrderRefund>(x => arrIds.Contains(x.Id))
            .Include(x => x.RefundAlbums)
            .Include(x => x.RefundGoods)
            .ExecuteCommandAsync();

        return NoContent();
    }
    #endregion

    #region 当前账户调用接口========================
    /// <summary>
    /// 根据ID获取数据
    /// type：1退款2换货
    /// 示例：/account/order/refund/1
    /// </summary>
    [HttpGet("/account/order/refund/{id}")]
    [Authorize]
    [NonUnify]
    public async Task<IActionResult> AccountGetById([FromRoute] long id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<OrderRefundDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        ////获取登录用户ID
        string userId = await _userService.GetUserIdAsync();
        if (userId.IsNullOrEmpty())
        {
            throw Oops.Oh("用户尚未登录");
        }

        //查询数据库获取实体
        var model = await _orderRefundService.AsQueryable()
             .Includes(x => x.RefundGoods , z => z.OrderGoods)
             .Includes(x => x.RefundAlbums)
            .SingleAsync(x => x.UserId == userId && x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        //使用AutoMapper转换成ViewModel，根据字段进行塑形
        var result = model.Adapt<OrderRefundDto>().ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 获取分页列表
    /// 示例：/account/order/refund?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet("/account/order/refund")]
    [Authorize]
    [NonUnify]
    public async Task<dynamic> AccountGetList([FromQuery] RefundParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<OrderRefundDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<OrderRefundDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //获取登录用户ID
        string userId = await _userService.GetUserIdAsync();
        if (userId.IsNullOrEmpty())
        {
            throw Oops.Oh("用户尚未登录").StatusCode(StatusCodes.Status404NotFound);
        }

        //获取数据列表，如果ID大于0则查询该用户下所有的列表
        var list = await _orderRefundService.AsQueryable()
            .Includes(x => x.RefundGoods,z=>z.OrderGoods)
            .Includes(x => x.RefundAlbums)
            .Where(x => x.UserId == userId)
            .WhereIF(searchParam.OrderNo.IsNotEmptyOrNull(), x => x.OrderNo == searchParam.OrderNo)
            .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.OrderNo.Contains(searchParam.Keyword) || x.UserName.Contains(searchParam.Keyword))
            .OrderBy(searchParam.OrderBy ?? "HandleStatus,-Id")
            .ToPagedListAsync(pageParam.PageIndex, pageParam.PageSize);

        return PageResult<OrderRefundDto>.SqlSugarPageResult(list);

        //var list = await _orderRefundService.QueryPageAsync(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => x.UserId == userId
        //    && (!searchParam.OrderNo.IsNotNullOrEmpty() || (x.OrderNo != null && x.OrderNo.Equals(searchParam.OrderNo)))
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.OrderNo != null && x.OrderNo.Contains(searchParam.Keyword)) || (x.UserName != null && x.UserName.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "HandleStatus,-Id");

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
        //var resultDto = list.Adapt<IEnumerable<OrderRefundDto>>().ShapeData(searchParam.Fields);
        //return Ok(resultDto);
    }

    /// <summary>
    /// 申请退换货
    /// 示例：/account/order/refund
    /// </summary>
    [HttpPost("/account/order/refund")]
    [Authorize]
    [SqlSugarUnitOfWork]
    [NonUnify]
    public async Task<IActionResult> AccountApply([FromBody] OrderRefundApplyDto modelDto)
    {
        //获取登录用户ID
        var userId = await _userService.GetUserIdAsync();
        if (userId.IsNullOrEmpty())
        {
            throw Oops.Oh("用户尚未登录").StatusCode(StatusCodes.Status404NotFound);
        }
        modelDto.userId = userId;
        //保存数据
        var result = await this.ApplyAsync(modelDto);
        return Ok(result);
    }

    /// <summary>
    /// 买家发货
    /// 示例：/account/order/refund/return/1
    /// </summary>
    [HttpPut("/account/order/refund/return/{id}")]
    [Authorize]
    [SqlSugarUnitOfWork]
    [NonUnify]
    public async Task<IActionResult> AccountDelivery([FromRoute] long id, [FromBody] OrderRefundBuyDto modelDto)
    {
        //获取登录用户ID
        var userId = await _userService.GetUserIdAsync();
        if (userId.IsNullOrEmpty())
        {
            throw Oops.Oh("用户尚未登录").StatusCode(StatusCodes.Status404NotFound);
        }
        //保存数据
        await this.BuyDeliveryAsync(id, modelDto);
        return NoContent();
    }
    #endregion


    #region Private Methods
    /// <summary>
    /// 申请退换货(客户)
    /// </summary>
    private async Task<bool> ApplyAsync(OrderRefundApplyDto modelDto)
    {
        //检查是否重复提交申请
        if (await _orderRefundService.AnyAsync(x => x.UserId == modelDto.userId
            && x.OrderId == modelDto.orderId && x.HandleStatus < 3))
        {
            throw Oops.Oh("已有申请尚未处理，请勿重复提交");
        }
        //查找订单信息
        var orderModel = await _orderRefundService.Context.Queryable<Orders>()
            .FirstAsync(x => x.UserId == modelDto.userId && x.Id == modelDto.orderId);
        if (orderModel == null)
        {
            throw Oops.Oh("订单不存在或已删除");
        }
        //未付款提示
        if (orderModel.PaymentStatus == 0)
        {
            throw Oops.Oh("订单未付款，请勿申请售后");
        }
        //换货：未发货时不能发起换货申请
        if (modelDto.type == 2 && orderModel.DeliveryStatus == 0)
        {
            throw Oops.Oh("订单未发货，请勿申请换货");
        }
        //退款：订单金额等于零时无法发起退款申请
        if (modelDto.type == 1 && orderModel.OrderAmount <= 0)
        {
            throw Oops.Oh("订单金额须大于零才能申请退款");
        }

        //映射成实体
        var model = modelDto.Adapt<OrderRefund>();
        model.OrderNo = orderModel.OrderNo;
        model.UserId = orderModel.UserId;
        model.UserName = orderModel.UserName;
        model.ApplyTime = DateTime.Now;
        //保存数据
        return await _orderRefundService.Context.InsertNav<OrderRefund>(model)
            .Include(x => x.RefundAlbums)
            .Include(x => x.RefundGoods)
            .ExecuteCommandAsync();
    }

    /// <summary>
    /// 2.卖家处理:同意,拒绝,退款
    /// </summary>
    private async Task<bool> HandleAsync(long id, OrderRefundHandleDto modelDto)
    {
        var model = await _orderRefundService.AsQueryable()
            .Includes(x => x.RefundGoods)
            .SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据不存在或已删除");
        }

        _orderRefundService.Context.Tracking(model);
        //退款
        if (model.Type == 1)
        {
            //如果是大于0则已处理，提示错误
            if (model.HandleStatus > 0)
            {
                throw Oops.Oh("单据已处理，请勿重复操作");
            }
            //同意退款
            if (modelDto.handleStatus == 3)
            {
                //查询订单状态
                var orderModel = await _orderRefundService.Context.Queryable<Orders>()
                    .Includes(x => x.OrderGoods)
                    .Includes(x => x.Collection)
                    .SingleAsync(x => x.Id == model.OrderId);
                if (orderModel == null || orderModel.Collection == null)
                {
                    throw Oops.Oh("订单有误或收款已被删除，请确认后重试。");
                }
                //订单未付款提示错误
                if (orderModel.PaymentStatus != 1)
                {
                    throw Oops.Oh($"订单[{orderModel.OrderNo}]未付款");
                }
                //退款金额大于订单金额时提示错误
                if (modelDto.refundAmount > orderModel.OrderAmount)
                {
                    throw Oops.Oh("退款金额不能大于订单金额");
                }

                //把退货订单商品ID转换成列表以便对比
                var refundIds = model.RefundGoods.Select(x => x.OrderGoodsId);
                //新增记录器，记录是否已全部退款或部分退款
                bool isAllRefund = true;
                //遍历订单货品
                foreach (var item in orderModel.OrderGoods)
                {
                    if (refundIds.Contains(item.Id))
                    {
                        item.DeliveryStatus = 2;
                    }
                    else if (item.DeliveryStatus < 2)
                    {
                        isAllRefund = false;
                    }
                }
                _orderRefundService.Context.Tracking(orderModel);
                //映射到实体
                modelDto.Adapt(model);

                //如果退款到会员账户则增加会员余额
                if (model.RefundMode == 1)
                {
                    //修改会员余额
                    var memberModel = await _orderRefundService.Context.Queryable<Members>().SingleAsync(x => x.UserId == model.UserId);
                    if (memberModel == null)
                    {
                        throw Oops.Oh("会员账户不存在或已删除");
                    }
                    _orderRefundService.Context.Tracking(memberModel);

                    memberModel.Amount += model.RefundAmount;
                    await _orderRefundService.Context.Updateable<Members>(memberModel).ExecuteCommandAsync();
                    //新增余额记录
                    await _orderRefundService.Context.Insertable<MemberAmountLog>(new MemberAmountLog()
                    {
                        UserId = model.UserId,
                        Value = model.RefundAmount,
                        Remark = $"订单[{orderModel.OrderNo}]退款",
                        AddTime = DateTime.Now
                    }).ExecuteCommandAsync();
                }

                //如果退款是原路返回则调用退款接口
                if (model.RefundMode == 2)
                {
                    //判断支付接口类型
                    var pay = await _orderRefundService.Context.Queryable<SitePayment>()
                        .Includes(x => x.Payment)
                        .SingleAsync(x => x.Id == orderModel.Collection.PaymentId);
                    if (pay == null || pay.Payment == null)
                    {
                        throw Oops.Oh("支付方式有误，请检查重试。");
                    }
                    if (pay.Payment.Type == 0)
                    {
                        throw Oops.Oh("此订单是线下支付，无法原路返回，请检查重试。");
                    }
                    if (pay.Payment.Type == 1)
                    {
                        throw Oops.Oh("此订单是余额支付，请选择退回会员账户。");
                    }
                    //微信
                    if (pay.Payment.Type == 2)
                    {
                        var refundStatus = await _weChatExecuteService.RefundAsync(new WeChatPayRefundDto()
                        {
                            OutTradeNo = orderModel.Collection.TradeNo,
                            OutRefundId = model.Id,
                            PaymentId = orderModel.Collection.PaymentId,
                            Refund = model.RefundAmount,
                            Total = orderModel.Collection.PaymentAmount,
                            Reason = model.ApplyReason
                        });
                        if (!refundStatus)
                        {
                            throw Oops.Oh("退款原路返回失败，请检查重试。");
                        }
                    }
                    //支付宝
                    if (pay.Payment.Type == 3)
                    {
                        var refundStatus = await _alipayExecuteService.RefundAsync(new AlipayRefundDto()
                        {
                            OutTradeNo = orderModel.Collection.TradeNo,
                            OutRefundId = model.Id,
                            PaymentId = orderModel.Collection.PaymentId,
                            Refund = model.RefundAmount,
                            Reason = model.ApplyReason
                        });
                        if (!refundStatus)
                        {
                            throw Oops.Oh("退款原路返回失败，请检查重试。");
                        }
                    }
                }

                //新增订单日志
                await _orderRefundService.Context.Insertable<OrderLog>(new OrderLog
                {
                    OrderId = orderModel.Id,
                    OrderNo = orderModel.OrderNo,
                    ActionType = ActionType.Refund.ToString(),
                    AddBy = await _userService.GetUserNameAsync(),
                    AddTime = DateTime.Now,
                    Remark = $"订单[{orderModel.OrderNo}]退款金额:{model.RefundAmount}元"
                }).ExecuteCommandAsync();
                //修改订单状态
                if (isAllRefund)
                {
                    orderModel.RefundStatus = 1;
                }
                else
                {
                    orderModel.RefundStatus = 2;
                }
                await _orderRefundService.Context.Updateable<Orders>(orderModel).ExecuteCommandAsync();
                //修改退换货状态为3已完成
                model.HandleTime = DateTime.Now;
                return await _orderRefundService.UpdateAsync(model) > 0;
            }
            //拒绝退款
            if (modelDto.handleStatus == 4)
            {
                modelDto.Adapt( model);
                model.HandleTime = DateTime.Now;
                return await _orderRefundService.UpdateAsync(model) > 0;
            }
        }
        //换货
        if (model.Type == 2)
        {
            //如果0未处理且3同意换货时，修改状态为1待买家发货
            //如果0未处理且4拒绝换货时，修改状态为4拒绝
            if (model.HandleStatus == 0 && (modelDto.handleStatus == 1 || modelDto.handleStatus == 4))
            {
                modelDto.Adapt(model);
                model.HandleTime = DateTime.Now;
                return await _orderRefundService.UpdateAsync(model) > 0;
            }
            else
            {
                throw Oops.Oh("单据已处理，请勿重复操作");
            }
        }
        return false;
    }

    /// <summary>
    /// 3.客户发货(客户)
    /// </summary>
    private async Task<bool> BuyDeliveryAsync(long id, OrderRefundBuyDto modelDto)
    {
        //检查快递信息是否正确
        if (!await _orderRefundService.Context.Queryable<ShopExpress>().AnyAsync(x => x.Id == modelDto.uExpressId))
        {
            throw Oops.Oh($"快递信息不正确");
        }
        //获取退换货信息
        var model = await _orderRefundService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据不存在或已删除");
        }
        //如果处理状态不是待买家发货，提示错误
        if (model.HandleStatus != 1)
        {
            throw Oops.Oh("商家未处理或已完成，请勿重复操作");
        }

        _orderRefundService.Context.Tracking(model);
        //DTO映射到源数据
        modelDto.Adapt(model);
        //设置处理状态为2待卖家发货
        model.HandleStatus = 2;
        return await _orderRefundService.UpdateAsync(model) > 0;
    }

    /// <summary>
    /// 4.卖家发货(管理员)
    /// </summary>
    private async Task<bool> SellerDeliveryAsync(long id, OrderRefundSellerDto modelDto)
    {
        //检查快递信息是否正确
        if (!await _orderRefundService.Context.Queryable<ShopExpress>().AnyAsync(x => x.Id == modelDto.sExpressId))
        {
            throw Oops.Oh($"快递信息不正确");
        }
        //获取退换货信息
        var model = await _orderRefundService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据不存在或已删除");
        }
        //如果处理状态不是待卖家发货，提示错误
        if (model.HandleStatus != 2)
        {
            throw Oops.Oh("买家未寄出或已完成，请勿重复提交");
        }
        _orderRefundService.Context.Tracking(model);
        //DTO映射到源数据
        modelDto.Adapt( model);
        //设置处理状态为3已完成
        model.HandleStatus = 3;
        return await _orderRefundService.UpdateAsync(model) > 0;
    } 
    #endregion
}