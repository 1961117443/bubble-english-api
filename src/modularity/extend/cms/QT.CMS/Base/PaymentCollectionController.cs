using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QT.CMS.Emum;
using QT.CMS.Entitys;
using QT.CMS.Entitys.Dto.Base;
using QT.CMS.Entitys.Dto.Parameter;
using QT.CMS.Interfaces;
using QT.Common.Core.Manager;
using QT.Common.Extension;
using QT.DependencyInjection;
using QT.FriendlyException;
using QT.JsonSerialization;
using SqlSugar;
using System.Linq.Expressions;

namespace QT.CMS;

/// <summary>
/// 支付收款接口
/// </summary>
[Route("admin/payment/collection")]
[ApiController]
public class PaymentCollectionController : ControllerBase,IScoped, IPaymentCollectionService
{
    private readonly ISqlSugarRepository<PaymentCollection> _paymentCollectionService;
    private readonly IUserService _userService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public PaymentCollectionController(ISqlSugarRepository<PaymentCollection> paymentCollectionService, IUserService userService)
    {
        _paymentCollectionService = paymentCollectionService;
        _userService = userService;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 根据ID获取数据
    /// 示例：/admin/payment/collection/1
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    //[AuthorizeFilter("PaymentCollection", ActionType.View)]
    public async Task<IActionResult> GetById([FromRoute] long id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<PaymentCollectionDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var model = await _paymentCollectionService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        //使用AutoMapper转换成ViewModel，根据字段进行塑形
        var result = model.Adapt<PaymentCollectionDto>().ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 获取指定数量列表
    /// 示例：/admin/payment/collection/view/10
    /// </summary>
    [HttpGet("view/{top}")]
    [Authorize]
    //[AuthorizeFilter("PaymentCollection", ActionType.View)]
    public async Task<IActionResult> GetList([FromRoute] int top, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy!=null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<PaymentCollectionDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<PaymentCollectionDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据库列表
        var resultFrom = await _paymentCollectionService.AsQueryable()
            .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.TradeNo.Contains(searchParam.Keyword))
            .OrderBy(searchParam.OrderBy ?? "-AddTime,-Id")
            .AutoTake(top)
            .ToListAsync();
        //var resultFrom = await _paymentCollectionService.QueryListAsync<PaymentCollection>(top,
        //    x => (!searchParam.Keyword.IsNotNullOrEmpty() || (x.TradeNo != null && x.TradeNo.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "-AddTime,-Id");

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<PaymentCollectionDto>>().ShapeData(searchParam.Fields);
        //返回成功200
        return Ok(resultDto);
    }

    /// <summary>
    /// 获取分页列表
    /// 示例：/admin/payment/collection?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet]
    [Authorize]
    //[AuthorizeFilter("PaymentCollection", ActionType.View)]
    public async Task<IActionResult> GetList([FromQuery] BaseParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy!=null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<PaymentCollectionDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<PaymentCollectionDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表，如果ID大于0则查询该用户下所有的列表
        var list = await _paymentCollectionService.AsQueryable()
            .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.TradeNo.Contains(searchParam.Keyword))
            .OrderBy(searchParam.OrderBy ?? "-AddTime,-Id")
            .ToPagedListAsync(pageParam.PageIndex, pageParam.PageSize);

        return Ok(PageResult<PaymentCollectionDto>.SqlSugarPageResult(list));

        //var list = await _paymentCollectionService.QueryPageAsync<PaymentCollection> (
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => (!searchParam.Keyword.IsNotNullOrEmpty() || (x.TradeNo != null && x.TradeNo.Contains(searchParam.Keyword))),
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
        //var resultDto = list.Adapt<IEnumerable<PaymentCollectionDto>>().ShapeData(searchParam.Fields);
        //return Ok(resultDto);
    }

    /// <summary>
    /// 删除一条记录
    /// 示例：/admin/payment/collection/1
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    //[AuthorizeFilter("PaymentCollection", ActionType.Delete)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        if (!await _paymentCollectionService.AnyAsync(x => x.Id == id))
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        var result = await _paymentCollectionService.DeleteAsync(x => x.Id == id);

        return NoContent();
    }

    /// <summary>
    /// 批量删除记录(级联数据)
    /// 示例：/admin/payment/collection?ids=1,2,3
    /// </summary>
    [HttpDelete]
    [Authorize]
    //[AuthorizeFilter("PaymentCollection", ActionType.Delete)]
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
        await _paymentCollectionService.DeleteAsync(x => arrIds.Contains(x.Id));

        return NoContent();
    }
    #endregion

    #region 当前账户调用接口========================
    /// <summary>
    /// 根据交易号获取数据
    /// 示例：/account/payment/collection/RN2021031...
    /// </summary>
    [HttpGet("/account/payment/collection/{tradeNo}")]
    [Authorize]
    [NonUnify]
    public async Task<IActionResult> AccountGetByNo([FromRoute] string tradeNo, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<PaymentCollectionDto>())
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
        var model = await _paymentCollectionService
            .AsQueryable().Includes(x => x.Orders).Includes(x => x.Recharges)
            .Where(x => x.UserId == userId && x.TradeNo == tradeNo && x.StartTime <= DateTime.Now)
            .FirstAsync();
        if (model == null)
        {
            throw Oops.Oh($"收款单[{tradeNo}]未开始或已失效");
        }
        //使用AutoMapper转换成ViewModel，根据字段进行塑形
        var result = model.Adapt<PaymentCollectionDto>(); //.ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 取消付款
    /// 示例：/account/payment/collection/cancel/RN2021031...
    /// </summary>
    [HttpPut("/account/payment/collection/cancel/{tradeNo}")]
    [Authorize]
    [SqlSugarUnitOfWork]
    public async Task<IActionResult> AccountCancel([FromRoute] string tradeNo)
    {
        //获取登录用户ID
        var userId = await  _userService.GetUserIdAsync();
        if (userId.IsNullOrEmpty())
        {
            throw Oops.Oh("用户尚未登录");
        }
        //保存数据
        var result = await this.CancelAsync(x => x.UserId == userId && x.TradeNo == tradeNo);
        if (result)
        {
            return NoContent();
        }
        throw Oops.Oh("保存过程中发生了错误");
    }

    /// <summary>
    /// 更改支付方式(客户)
    /// 示例：/account/payment/collection/edit
    /// </summary>
    [HttpPut("/account/payment/collection/edit")]
    [Authorize]
    [SqlSugarUnitOfWork]
    public async Task<IActionResult> AccountPayment([FromBody] PaymentCollectionEditDto modelDto)
    {
        var result = await this.PayAsync(modelDto);
        if (result)
        {
            return NoContent();
        }
        throw Oops.Oh("保存过程中发生了错误");
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// 取消收款
    /// </summary>
    private async Task<bool> CancelAsync(Expression<Func<PaymentCollection, bool>> funcWhere)
    {
        //检查收款单
        var model = await _paymentCollectionService.Context.Queryable<PaymentCollection>()
            .Includes(x => x.Orders)
            .SingleAsync(funcWhere);
        if (model == null)
        {
            throw Oops.Oh("交易记录不存在或已删除");
        }
        //如果已经付款或取消
        if (model.Status == 2)
        {
            throw Oops.Oh("交易已经付款，无法进行取消");
        }
        if (model.Status == 3)
        {
            throw Oops.Oh("交易已经取消，无法重复操作");
        }
        _paymentCollectionService.Context.Tracking(model);
        //判断收款类型，如果是商品订单
        if (model.TradeType == 0)
        {
            //查询订单列表
            var orderIds = model.Orders.Select(x => x.Id);
            var orderList = await _paymentCollectionService.Context.Queryable<Orders>()
                .Includes(x => x.OrderGoods)
                .Where(x => orderIds.Contains(x.Id)).ToListAsync();
            //检查订单是否已付款
            var payOrderList = orderList.Where(x => x.PaymentStatus > 0);
            if (payOrderList != null)
            {
                throw Oops.Oh("订单已经付款，无法取消");
            }

            List<ShopGoodsProduct> shopGoodsProductUpdateList = new List<ShopGoodsProduct>();
            //遍历订单，修改订单状态
            foreach (var order in orderList)
            {
                _paymentCollectionService.Context.Tracking(order);
                //查询货品列表
                var productIds = order.OrderGoods.Select(x => x.ProductId);//货品列表
                var productList = await _paymentCollectionService.Context.Queryable<ShopGoodsProduct>()
                    .Includes(x => x.ShopGoods)
                    .Where(x => (x.ShopGoods != null && x.ShopGoods.Status == 0) && productIds.Contains(x.Id)).ToListAsync();
                //遍历货品，返还库存
                foreach (var modelt in productList)
                {
                    if (modelt.ShopGoods == null)
                    {
                        continue;
                    }
                    //取得货品的购买数量
                    var orderGoods = order.OrderGoods.FirstOrDefault(x => x.ProductId == modelt.Id);
                    if (orderGoods != null)
                    {
                        _paymentCollectionService.Context.Tracking(orderGoods);
                        shopGoodsProductUpdateList.Add(modelt);
                        //货品及商品减库存，销售减少
                        modelt.StockQuantity += orderGoods.Quantity;
                        modelt.ShopGoods.StockQuantity += orderGoods.Quantity;
                        modelt.ShopGoods.SaleCount = modelt.ShopGoods.SaleCount - orderGoods.Quantity;
                    }
                }
                //修改货口及商品数量
                if (shopGoodsProductUpdateList.IsAny())
                {
                    await _paymentCollectionService.Context.Updateable<ShopGoodsProduct>(shopGoodsProductUpdateList).ExecuteCommandAsync();
                }


                //修改订单状态
                order.Status = 6;
            }
            //批量修改订单
            await _paymentCollectionService.Context.Updateable<Orders>(orderList).ExecuteCommandAsync();
        }
        //修改收款状态
        model.Status = 3;
        model.CompleteTime = DateTime.Now;
        return await _paymentCollectionService.UpdateAsync(model) > 0;
    }

    /// <summary>
    /// 修改支付方式(客户)
    /// </summary>
    private async Task<bool> PayAsync(PaymentCollectionEditDto modelDto)
    {
                                                               //获取当前登录用户
        var userId = await  _userService.GetUserIdAsync();
        //根据ID获取记录
        var model = await _paymentCollectionService
            .FirstOrDefaultAsync(x => x.Id == modelDto.id && x.UserId == userId && x.StartTime <= DateTime.Now);
        //如果不存在则抛出异常
        if (model == null)
        {
            throw Oops.Oh("交易单号有误或已过期");
        }
        //判断支付方式是否改变
        if (model.PaymentId == modelDto.paymentId)
        {
            return true;
        }
        //判断是否已支付
        if (model.Status != 1)
        {
            throw Oops.Oh("订单已支付或已取消，无法更改支付方式");
        }
        //查询支付方式
        var paymentModel = await _paymentCollectionService.Context.Queryable<SitePayment>().SingleAsync(x => x.Id == modelDto.paymentId);
        if (paymentModel == null)
        {
            throw Oops.Oh("支付方式有误，请确认后操作");
        }
        //再次支付不允许线下支付
        if (paymentModel.Type == "cash")
        {
            throw Oops.Oh("交易无法更换线下支付，请重试");
        }
        _paymentCollectionService.Context.Tracking(model);
        model.PaymentId = paymentModel.Id;
        model.PaymentTitle = paymentModel.Title;
        return await _paymentCollectionService.UpdateAsync(model) > 0;
    }
    #endregion


    #region Impl
    /// <summary>
    /// 确认收款
    /// </summary>
    [NonAction]
    public async Task<bool> ConfirmAsync(string tradeNo)
    {
                                                               //检查收款单
        var model = await _paymentCollectionService.AsQueryable()
            .Includes(x => x.SitePayment)
            .Includes(x => x.Recharges)
            .Includes(x => x.Orders)
            .SingleAsync(x => x.TradeNo == tradeNo);
        if (model == null)
        {
            throw Oops.Oh("订单不存在或已删除");
        }
        //如果已经支付则直接返回
        if (model.Status == 2)
        {
            return true;
        }
        //如果已取消则不能支付
        if (model.Status == 3)
        {
            throw Oops.Oh("订单已取消无法确认收款");
        }
        //查找会员账户
        var memberModel = await _paymentCollectionService.Context.Queryable<Members>()
            .Includes(x => x.Group)
            .SingleAsync(x => x.UserId == model.UserId);
        if (memberModel == null)
        {
            throw Oops.Oh("会员账户不存在或已删除");
        }
        _paymentCollectionService.Context.Tracking(memberModel);
        _paymentCollectionService.Context.Tracking(model);
        //判断收款类型，如果是充值订单
        if (model.TradeType == 1)
        {
            if (model.Recharges.Count == 0)
            {
                throw Oops.Oh("没有找到充值订单，请重试");
            }
            memberModel.Amount += model.PaymentAmount;//增加余额
                                                      //检查有无可升级的会员组
            if (memberModel.Group != null)
            {
                //条件筛选并排序，返回第一条
                var upgradeGroupModel = await _paymentCollectionService.Context.Queryable<MemberGroup>().OrderByDescending(x=>x.Amount)
                    .SingleAsync(x => x.Id != memberModel.Group.Id && x.IsUpgrade == 1 && x.Amount <= model.PaymentAmount);
                if (upgradeGroupModel != null && upgradeGroupModel.Amount >= memberModel.Group.Amount)
                {
                    memberModel.GroupId = (int)upgradeGroupModel.Id;//升级会员组
                }
            }
            //更改订单状态
            model.Status = 2;
            model.CompleteTime = DateTime.Now;

            //添加消费记录
            await _paymentCollectionService.Context.Insertable<MemberAmountLog>(new MemberAmountLog()
            {
                UserId = model.UserId,
                Value = model.PaymentAmount,
                Remark = $"在线充值，充值交易号:{model.TradeNo}",
                AddTime = DateTime.Now
            }).ExecuteCommandAsync();
            //更新会员
            await _paymentCollectionService.Context.Updateable<Members>(memberModel).ExecuteCommandAsync();
            //更新收款单
            return await _paymentCollectionService.UpdateAsync(model) > 0;
        }
        //商品订单
        else
        {
            //如果是余额支付则直接扣款
            if (model.SitePayment?.Type == "balance")
            {
                //检查会员余额是否够扣减
                if (memberModel.Amount < model.PaymentAmount)
                {
                    throw Oops.Oh("会员账户余额不足本次支付");
                }
                memberModel.Amount -= model.PaymentAmount;//扣取余额
                                                          //修改会员信息
                await _paymentCollectionService.Context.Updateable<Members>(memberModel).ExecuteCommandAsync();
                //添加消费记录
                await _paymentCollectionService.Context.Insertable<MemberAmountLog>(new MemberAmountLog()
                {
                    UserId = memberModel.UserId,
                    Value = model.PaymentAmount * -1,
                    Remark = $"交易单号[{model.TradeNo}]付款",
                    AddTime = DateTime.Now
                }).ExecuteCommandAsync();
            }
            //遍历订单，修改订单支付状态
            foreach (var modelt in model.Orders)
            {
                _paymentCollectionService.Context.Tracking(modelt);
                modelt.PaymentStatus = 1;
                modelt.PaymentTime = DateTime.Now;
                modelt.Status = 2;
                //新增订单日志
                await _paymentCollectionService.Context.Insertable<OrderLog>(new OrderLog
                {
                    OrderId = modelt.Id,
                    OrderNo = modelt.OrderNo,
                    ActionType = ActionType.Payment.ToString(),
                    AddBy = await _userService.GetUserNameAsync(),
                    AddTime = model.AddTime,
                    Remark = $"订单[{modelt.OrderNo}]付款{model.PaymentAmount}元"
                }).ExecuteCommandAsync();

                await _paymentCollectionService.Context.Updateable<Orders>(modelt).ExecuteCommandAsync();
            }
            //修改支付收款单状态
            model.Status = 2;
            model.CompleteTime = DateTime.Now;
            return await _paymentCollectionService.Context.AutoUpdateAsync(model) > 0;
        }
    } 
    #endregion
}
