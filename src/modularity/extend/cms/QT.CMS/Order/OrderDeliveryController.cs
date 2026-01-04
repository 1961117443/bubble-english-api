using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Polly;
using QT.CMS.Emum;
using QT.CMS.Entitys;
using QT.CMS.Entitys.Dto.Config;
using QT.CMS.Entitys.Dto.Order;
using QT.CMS.Entitys.Dto.Parameter;
using QT.Common.Core.Manager;
using QT.Common.Extension;
using QT.Common.Security;
using QT.DataEncryption;
using QT.FriendlyException;
using QT.JsonSerialization;
using QT.RemoteRequest.Extensions;
using SqlSugar;

namespace QT.CMS;

/// <summary>
/// 订单发货
/// </summary>
[Route("/api/cms/admin/order/delivery")]
[ApiController]
public class OrderDeliveryController : ControllerBase
{
    private readonly ISqlSugarRepository<OrderDelivery> _orderDeliveryService;
    //private readonly ISysConfigService _sysConfigService;
    private readonly IUserService _userService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public OrderDeliveryController(ISqlSugarRepository<OrderDelivery> orderDeliveryService,
        //ISysConfigService sysConfigService, 
        IUserService userService)
    {
        _orderDeliveryService = orderDeliveryService;
        //_sysConfigService = sysConfigService;
        _userService = userService;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 根据ID获取数据
    /// 示例：/admin/order/delivery/1
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    //[AuthorizeFilter("OrderDelivery", ActionType.View)]
    public async Task<IActionResult> GetById([FromRoute] long id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<OrderDeliveryDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var model = await _orderDeliveryService.AsQueryable()
            .Includes(x=>x.DeliveryGoods,z=>z.OrderGoods)
            .Includes(x => x.ShopExpress)
            .SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据{id}不存在或已删除");
        }
        //使用AutoMapper转换成ViewModel，根据字段进行塑形
        var result = model.Adapt<OrderDeliveryDto>().ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 获取指定数量列表
    /// 示例：/admin/order/delivery/view/10
    /// </summary>
    [HttpGet("view/{top}")]
    [Authorize]
    //[AuthorizeFilter("OrderDelivery", ActionType.View)]
    public async Task<IActionResult> GetList([FromRoute] int top, [FromQuery] RefundParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<OrderDeliveryDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<OrderDeliveryDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据库列表
        var resultFrom = await _orderDeliveryService.AsQueryable()
            .Includes(x=>x.DeliveryGoods,z=>z.OrderGoods)
            .Includes(x=>x.ShopExpress)
            .WhereIF(searchParam.OrderNo.IsNotEmptyOrNull(),x=>x.OrderNo == searchParam.OrderNo)
            .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.AcceptName.Contains(searchParam.Keyword))
            .OrderBy(searchParam.OrderBy ?? "-AddTime,-Id")
            .AutoTake(top)
            .ToListAsync();
        //var resultFrom = await _orderDeliveryService.QueryListAsync(top,
        //    x => (!searchParam.OrderNo.IsNotNullOrEmpty() || (x.OrderNo != null && x.OrderNo.Equals(searchParam.OrderNo)))
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.AcceptName != null && x.AcceptName.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "-AddTime,-Id");

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<OrderDeliveryDto>>();//.ShapeData(searchParam.Fields);
        //返回成功200
        return Ok(resultDto);
    }

    /// <summary>
    /// 获取分页列表
    /// 示例：/admin/order/delivery?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet]
    [Authorize]
    //[AuthorizeFilter("OrderDelivery", ActionType.View)]
    public async Task<IActionResult> GetList([FromQuery] RefundParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<OrderDeliveryDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<OrderDeliveryDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表，如果ID大于0则查询该用户下所有的列表
        var list = await _orderDeliveryService.AsQueryable()
            .Includes(x => x.DeliveryGoods,z=>z.OrderGoods)
            .Includes(x=>x.ShopExpress)
            .WhereIF(searchParam.OrderNo.IsNotEmptyOrNull(), x => x.OrderNo == searchParam.OrderNo)
            .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.AcceptName.Contains(searchParam.Keyword))
            .OrderBy(searchParam.OrderBy ?? "-AddTime,-Id")
            .ToPagedListAsync(pageParam.PageIndex, pageParam.PageSize);

        return Ok(PageResult<OrderDeliveryDto>.SqlSugarPageResult(list));


        //var list = await _orderDeliveryService.QueryPageAsync(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => (!searchParam.Keyword.IsNotNullOrEmpty() || (x.OrderNo != null && x.OrderNo.Equals(searchParam.OrderNo)))
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.AcceptName != null && x.AcceptName.Contains(searchParam.Keyword))),
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
        //var resultDto = list.Adapt<IEnumerable<OrderDeliveryDto>>().ShapeData(searchParam.Fields);
        //return Ok(resultDto);
    }

    /// <summary>
    /// 添加一条记录
    /// 示例：/admin/order/delivery
    /// </summary>
    [HttpPost]
    [Authorize]
    [SqlSugarUnitOfWork]
    //[AuthorizeFilter("OrderDelivery", ActionType.Add)]
    public async Task<IActionResult> Add([FromBody] OrderDeliveryEditDto modelDto)
    {
        //保存数据
        var result = await this.AddAsync(modelDto);
        return Ok(result);
    }

    /// <summary>
    /// 局部更新一条记录
    /// 示例：/admin/order/delivery/1
    /// Body：[{"op":"replace","path":"/title","value":"new title"}]
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize]
    //[AuthorizeFilter("OrderDelivery", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] JsonPatchDocument<OrderDeliveryEditDto> patchDocument)
    {
        var model = await _orderDeliveryService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }

        var modelToPatch = model.Adapt<OrderDeliveryEditDto>();
        patchDocument.ApplyTo(modelToPatch, ModelState);
        //验证数据是否合法
        if (!TryValidateModel(modelToPatch))
        {
            return ValidationProblem(ModelState);
        }
        _orderDeliveryService.Context.Tracking(model);
        //更新操作AutoMapper替我们完成，只需要调用保存即可
        modelToPatch.Adapt( model);
        await _orderDeliveryService.UpdateAsync(model);

        return NoContent();
    }

    /// <summary>
    /// 删除一条记录
    /// 示例：/admin/order/delivery/1
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [SqlSugarUnitOfWork]
    //[AuthorizeFilter("OrderDelivery", ActionType.Delete)]
    public async Task<IActionResult> Delete([FromRoute] long id)
    {
        if (!await _orderDeliveryService.AnyAsync(x => x.Id == id))
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        var result = await _orderDeliveryService.Context.DeleteNav<OrderDelivery>(x => x.Id == id)
            .Include(x => x.DeliveryGoods)
            .ExecuteCommandAsync();

        return NoContent();
    }

    /// <summary>
    /// 批量删除记录(级联数据)
    /// 示例：/admin/order/delivery?ids=1,2,3
    /// </summary>
    [HttpDelete]
    [Authorize]
    [SqlSugarUnitOfWork]
    //[AuthorizeFilter("OrderDelivery", ActionType.Delete)]
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
        await _orderDeliveryService.Context.DeleteNav<OrderDelivery>(x => arrIds.Contains(x.Id))
            .Include(x => x.DeliveryGoods)
            .ExecuteCommandAsync();

        return NoContent();
    }
    #endregion

    #region 当前账户调用接口========================
    /// <summary>
    /// 获取指定数量列表
    /// 示例：/account/order/delivery/PN2021031...
    /// </summary>
    [HttpGet("/account/order/delivery/{orderNo}")]
    [Authorize]
    [NonUnify]
    public async Task<IActionResult> AccountGetList([FromRoute] string orderNo)
    {
        //获取登录用户ID
        var userId = await  _userService.GetUserIdAsync();
        if (userId.IsNullOrEmpty())
        {
            throw Oops.Oh("用户尚未登录");
        }

        //获取数据库列表
        var model = await _orderDeliveryService.AsQueryable()
              .Includes(x => x.ShopExpress)
                .Includes(x => x.DeliveryGoods,z => z.OrderGoods)
            .Where(x=>x.OrderNo == orderNo)
           .OrderBy( "-AddTime,-Id")
           .ToListAsync();

        //var model = await _orderDeliveryService.QueryListAsync(0, x => x.OrderNo == orderNo, "-AddTime,-Id");
        if (model == null)
        {
            throw Oops.Oh("订单号有误或未发货");
        }
        //映射成DTO，根据字段进行塑形
        var resultDto = model.Adapt<IEnumerable<OrderDeliveryDto>>();
        //返回成功200
        return Ok(resultDto);
    }

    /// <summary>
    /// 获取物流跟踪信息
    /// 示例：/account/order/delivery/express/1
    /// </summary>
    [HttpGet("/account/order/delivery/express/{id}")]
    [Authorize]
    [NonUnify]
    public async Task<IActionResult> GetExpress([FromRoute] long id)
    {
        var jsonStr = await _orderDeliveryService.Context.Queryable<SysConfig>().Where(x => x.Type == ConfigType.OrderConfig.ToString()).FirstAsync();
        var config = jsonStr?.JsonData.ToObject<OrderConfigDto>();
        if (config == null)
        {
            throw Oops.Oh("订单配置格式有误");
        }
        if (config.kuaidiApi == null || config.kuaidiKey == null || config.kuaidiCust == null)
        {
            throw Oops.Oh("物流查询接口未配置");
        }
        //查询发货信息
        var deliveryModel = await _orderDeliveryService.SingleAsync(x => x.Id == id);
        if (deliveryModel == null)
        {
            throw Oops.Oh("未查询到发货信息");
        }
        //组装参数
        string param = $"{{\"com\":\"{deliveryModel.ShopExpress?.ExpressCode}\",\"num\":\"{deliveryModel.ExpressCode}\",\"show\":\"{config.kuaidiShow}\",\"order\":\"{config.kuaidiOrder}\"}}";
        string sign = MD5Encryption.Encrypt($"{param}{config.kuaidiKey}{config.kuaidiCust}",true);
        string body = $"customer={config.kuaidiCust}&sign={sign}&param={param}";
        var result = await config.kuaidiApi.SetBody(body, "application/x-www-form-urlencoded").PostAsStringAsync();
        return Ok(result);
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// 添加一条记录
    /// </summary>
    private async Task<OrderDeliveryDto> AddAsync(OrderDeliveryEditDto modelDto)
    {
        //获取订单详情
        var orderModel = await _orderDeliveryService.Context.Queryable<Orders>()
            .Includes(x => x.OrderGoods).SingleAsync(x => x.Id == modelDto.orderId);
        if (orderModel == null)
        {
            throw Oops.Oh($"订单{modelDto.orderId}不存在或已删除", ErrorCode.NotFound);
        }
        //检查订单状态
        switch (orderModel.Status)
        {
            case 6:
                throw Oops.Oh("订单已被取消，无法发货");
            case 7:
                throw Oops.Oh("订单已被作废，无法发货");
            case 5:
                throw Oops.Oh("订单已完成，无法发货");

        }
        if (orderModel.RefundStatus == 1)
        {
            throw Oops.Oh("订单已全部退款，无法发货");
        }
        //检查发货状态
        if (orderModel.DeliveryStatus == 1)
        {
            throw Oops.Oh($"订单[{modelDto.orderId}]已全部发货完毕");
        }
        //检查配送方式
        var deliveryModel = await _orderDeliveryService.Context.Queryable<ShopDelivery>().SingleAsync(x => x.Id == orderModel.DeliveryId);
        if (deliveryModel == null)
        {
            throw Oops.Oh("配送方式不存在或已删除");
        }
        //检查快递
        if (!await _orderDeliveryService.Context.Queryable<ShopExpress>().AnyAsync(x => x.Id == modelDto.expressId))
        {
            throw Oops.Oh($"快递[{modelDto.expressId}]不存在或已删除");
        }
        _orderDeliveryService.Context.Tracking(orderModel);

        //将省市区用逗号连接起来
        modelDto.address = $"{modelDto.province},{modelDto.city},{modelDto.area}," + modelDto.address;

        //映射成实体,获取用户名及发货时间
        var model = modelDto.Adapt<OrderDelivery>();
        model.OrderNo = orderModel.OrderNo;
        model.AddBy = await _userService.GetUserNameAsync();
        model.AddTime = DateTime.Now;

        //修改订单货品为发货状态
        var deliveryGoodsIds = model.DeliveryGoods.Select(x => x.OrderGoodsId);//发货货品的ID集合
                                                                               //遍历订单中所有货品，将发货的货品设置为已发货
        List<OrderGoods> updateOrderGoods = new List<OrderGoods>();
        foreach (var modelt in orderModel.OrderGoods)
        {
            if (modelt.DeliveryStatus == 0 && deliveryGoodsIds.Contains(modelt.Id))
            {
                //修改订单货品为发货状态
                modelt.DeliveryStatus = 1;

                updateOrderGoods.Add(modelt);
            }
        }
        //判断订单中商品货品全部发货或者部分发货
        if (orderModel.OrderGoods.Where(x => x.DeliveryStatus == 0).Count() > 0)
        {
            orderModel.DeliveryStatus = 2;//部分发货
        }
        else
        {
            orderModel.DeliveryStatus = 1;//全部发货
            orderModel.Status = 3;
        }
        orderModel.DeliveryTime = DateTime.Now;

        //修改订单发货状态
        await _orderDeliveryService.Context.Updateable<Orders>(orderModel).ExecuteCommandAsync();
        // 修改订单货品为发货状态
        if (updateOrderGoods.IsAny())
        {
            await _orderDeliveryService.Context.Updateable<OrderGoods>(updateOrderGoods).UpdateColumns(x=>x.DeliveryStatus).ExecuteCommandAsync();
        }
        //新增发货记录
        await _orderDeliveryService.Context.InsertNav<OrderDelivery>(model)
            .Include(x=>x.DeliveryGoods)
            .ExecuteCommandAsync();
        //新增订单日志
        await _orderDeliveryService.Context.Insertable<OrderLog>(new OrderLog
        {
            OrderId = orderModel.Id,
            OrderNo = orderModel.OrderNo,
            ActionType = ActionType.Delivery.ToString(),
            AddBy = model.AddBy,
            AddTime = model.AddTime,
            Remark = $"订单[{orderModel.OrderNo}]由[{model.AddBy}]发货"
        }).ExecuteCommandAsync();
         
        //映射成DTO
        return model.Adapt<OrderDeliveryDto>();
    } 
    #endregion
}