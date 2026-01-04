using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using QT.CMS.Entitys;
using QT.CMS.Entitys.Dto.Member;
using QT.CMS.Entitys.Dto.Order;
using QT.CMS.Entitys.Dto.Parameter;
using QT.Common.Core.Filter;
using QT.Common.Core.Manager;
using QT.Common.Extension;
using QT.FriendlyException;
using QT.JsonSerialization;
using SqlSugar;

namespace QT.CMS;

/// <summary>
/// 订单商品评价
/// </summary>
[Route("api/cms/admin/order/evaluate")]
[ApiController]
public class OrderEvaluateController : ControllerBase
{
    private readonly ISqlSugarRepository<OrderEvaluate> _orderEvaluateService;
    private readonly IUserService _userService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public OrderEvaluateController(ISqlSugarRepository<OrderEvaluate> orderEvaluateService,IUserService userService)
    {
        _orderEvaluateService = orderEvaluateService;
        _userService = userService;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 根据ID获取数据
    /// 示例：/admin/order/evaluate/1
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    //[AuthorizeFilter("OrderEvaluate", ActionType.View)]
    public async Task<IActionResult> GetById([FromRoute] long id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<OrderEvaluateDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var model = await _orderEvaluateService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        //使用AutoMapper转换成ViewModel，根据字段进行塑形
        var result = model.Adapt<OrderEvaluateDto>().ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 获取指定数量列表
    /// 示例：/admin/order/evaluate/view/10
    /// </summary>
    [HttpGet("view/{top}")]
    [Authorize]
    //[AuthorizeFilter("OrderEvaluate", ActionType.View)]
    public async Task<IActionResult> GetList([FromRoute] int top, [FromQuery] EvaluateParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<OrderEvaluateDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<OrderEvaluateDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据库列表
        var resultFrom = await _orderEvaluateService.AsQueryable()
            .Where(x => (searchParam.Score == 0 || (searchParam.Score == 3 && x.GoodsScore == 3) || (searchParam.Score <= 2 && x.GoodsScore <= 2)))
            .WhereIF(searchParam.Image > 0, x => SqlFunc.Subqueryable<OrderEvaluateAlbum>().Where(d => d.EvaluateId == x.Id).Any())
            .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => SqlFunc.Subqueryable<Orders>().Where(d => d.Id == x.OrderId && d.OrderNo.Contains(searchParam.Keyword)).Any())
            .OrderBy(searchParam.OrderBy ?? "-Status,-Id")
            .AutoTake(top)
            .ToListAsync();
            
        //var resultFrom = await _orderEvaluateService.QueryListAsync(top,
        //    x => (searchParam.Score == 0 || (searchParam.Score == 3 && x.GoodsScore == 3) || (searchParam.Score <= 2 && x.GoodsScore <= 2))
        //    && (searchParam.Image == 0 || x.EvaluateAlbums.Count > 0)
        //    && (!searchParam.Keyword.IsNotNullOrEmpty()
        //    || (x.OrderGoods != null && x.OrderGoods.Order != null && x.OrderGoods.Order.OrderNo != null && x.OrderGoods.Order.OrderNo.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "-Status,-Id");

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<OrderEvaluateDto>>().ShapeData(searchParam.Fields);
        //返回成功200
        return Ok(resultDto);
    }

    /// <summary>
    /// 获取分页列表
    /// 示例：/admin/order/evaluate?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet]
    [Authorize]
    //[AuthorizeFilter("OrderEvaluate", ActionType.View)]
    public async Task<IActionResult> GetList([FromQuery] EvaluateParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<OrderEvaluateDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<OrderEvaluateDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表，如果ID大于0则查询该用户下所有的列表
        var list = await _orderEvaluateService.AsQueryable()
            .Where(x => (searchParam.Score == 0 || (searchParam.Score == 3 && x.GoodsScore == 3) || (searchParam.Score <= 2 && x.GoodsScore <= 2)))
            .WhereIF(searchParam.Image > 0, x => SqlFunc.Subqueryable<OrderEvaluateAlbum>().Where(d => d.EvaluateId == x.Id).Any())
            .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => SqlFunc.Subqueryable<Orders>().Where(d => d.Id == x.OrderId && d.OrderNo.Contains(searchParam.Keyword)).Any())
            .OrderBy(searchParam.OrderBy ?? "-Status,-Id")
            .ToPagedListAsync(pageParam.PageIndex, pageParam.PageSize);

        return Ok(PageResult<OrderEvaluateDto>.SqlSugarPageResult(list));
        //var list = await _orderEvaluateService.QueryPageAsync(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => (searchParam.Score == 0 || (searchParam.Score == 3 && x.GoodsScore == 3) || (searchParam.Score <= 2 && x.GoodsScore <= 2))
        //    && (searchParam.Image == 0 || x.EvaluateAlbums.Count > 0)
        //    && (!searchParam.Keyword.IsNotNullOrEmpty()
        //    || (x.OrderGoods != null && x.OrderGoods.Order != null && x.OrderGoods.Order.OrderNo != null && x.OrderGoods.Order.OrderNo.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "-Status,-Id");

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
        //var resultDto = list.Adapt<IEnumerable<OrderEvaluateDto>>(); //.ShapeData(searchParam.Fields);
        //return Ok(resultDto);
    }

    /// <summary>
    /// 修改一条记录
    /// 示例：/admin/order/evaluate/1
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [SqlSugarUnitOfWork]
    //[AuthorizeFilter("OrderEvaluate", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] OrderEvaluateEditDto modelDto)
    {
        //根据ID获取记录
        var model = await _orderEvaluateService.Where(x => x.Id == id)
            .Includes(x => x.EvaluateAlbums)
            .FirstAsync();
        //如果不存在则抛出异常
        if (model == null)
        {
            throw Oops.Oh("数据不存在或已删除");
        }
        
        _orderEvaluateService.Context.Tracking(model);

        List<OrderEvaluateAlbum> orderEvaluateAlbumUpdateList = new List<OrderEvaluateAlbum>();
        List<OrderEvaluateAlbum> orderEvaluateAlbumInsertList = new List<OrderEvaluateAlbum>();
        foreach (var item in modelDto.evaluateAlbums)
        {
            var entity = model.EvaluateAlbums.FirstOrDefault(x => x.Id == item.id);
            if (entity!=null)
            {
                _orderEvaluateService.Context.Tracking(entity);
                item.Adapt(entity);
                orderEvaluateAlbumUpdateList.Add(entity);
                model.EvaluateAlbums.Remove(entity);
            }
            else
            {
                orderEvaluateAlbumInsertList.Add(item.Adapt<OrderEvaluateAlbum>());
            }
        }
        List<OrderEvaluateAlbum> orderEvaluateAlbumDeleteList = model.EvaluateAlbums.ToList();
        //将DTO映射到源数据
        var mapModel = modelDto.Adapt(model);

        if (orderEvaluateAlbumUpdateList.IsAny())
        {
            await _orderEvaluateService.Context.Updateable<OrderEvaluateAlbum>(orderEvaluateAlbumUpdateList).ExecuteCommandAsync();
        }
        if (orderEvaluateAlbumInsertList.IsAny())
        {
            await _orderEvaluateService.Context.Insertable<OrderEvaluateAlbum>(orderEvaluateAlbumInsertList).ExecuteCommandAsync();
        }
        if (orderEvaluateAlbumDeleteList.IsAny())
        {
            await _orderEvaluateService.Context.Deleteable<OrderEvaluateAlbum>(orderEvaluateAlbumDeleteList).ExecuteCommandAsync();
        }

        //保存
        await _orderEvaluateService.UpdateAsync(mapModel);
        return NoContent();
    }

    /// <summary>
    /// 局部更新一条记录
    /// 示例：/admin/order/evaluate/1
    /// Body：[{"op":"replace","path":"/title","value":"new title"}]
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize]
    //[AuthorizeFilter("OrderEvaluate", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] JsonPatchDocument<OrderEvaluateEditDto> patchDocument)
    {
        var model = await _orderEvaluateService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }

        var modelToPatch = model.Adapt<OrderEvaluateEditDto>();
        patchDocument.ApplyTo(modelToPatch, ModelState);
        //验证数据是否合法
        if (!TryValidateModel(modelToPatch))
        {
            return ValidationProblem(ModelState);
        }
        _orderEvaluateService.Context.Tracking(model);
        //更新操作AutoMapper替我们完成，只需要调用保存即可
        modelToPatch.Adapt(model);
        await _orderEvaluateService.UpdateAsync(model);

        return NoContent();
    }

    /// <summary>
    /// 删除一条记录
    /// 示例：/admin/order/evaluate/1
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    //[AuthorizeFilter("OrderEvaluate", ActionType.Delete)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        if (!await _orderEvaluateService.AnyAsync(x => x.Id == id))
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }

        await _orderEvaluateService.DeleteAsync(x => x.Id == id);
        return NoContent();
    }

    /// <summary>
    /// 批量删除记录(级联数据)
    /// 示例：/admin/order/evaluate?ids=1,2,3
    /// </summary>
    [HttpDelete]
    [Authorize]
    //[AuthorizeFilter("OrderEvaluate", ActionType.Delete)]
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
        await _orderEvaluateService.DeleteAsync(x => arrIds.Contains(x.Id));
        return NoContent();
    }
    #endregion

    #region 当前账户调用接口========================
    /// <summary>
    /// 添加一条记录
    /// 示例：/account/order/evaluate/1
    /// </summary>
    [HttpPost("/account/order/evaluate/{orderId}")]
    [Authorize]
    [SqlSugarUnitOfWork]
    [NonUnify]
    public async Task<IActionResult> AccountAdd([FromRoute] long orderId, [FromBody] ICollection<OrderEvaluateEditDto> listDto)
    {
        //保存数据
        var result = await this.AddAsync(orderId, listDto);
        return Ok(result);
    }
    #endregion

    #region 前台调用接口============================
    /// <summary>
    /// 获取指定商品评价总数
    /// 示例：/client/goods/evaluate/count/1
    /// </summary>
    [HttpGet("/client/goods/evaluate/count/{goodsId}")]
    [CachingFilter]
    [NonUnify]
    public async Task<IActionResult> GetByGoodsCount([FromRoute] long goodsId, [FromQuery] EvaluateParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<OrderEvaluateDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }

        //获取数据库列表
        var result = await _orderEvaluateService
            .Where(x => x.Status == 0)
            .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => SqlFunc.Subqueryable<Orders>().Where(d => d.Id == x.OrderId && d.OrderNo.Contains(searchParam.Keyword)).Any())
            .CountAsync();
        //var result = await _orderEvaluateService.QueryCountAsync(
        //    x => x.Status == 0
        //    && x.OrderGoods != null
        //    && x.OrderGoods.Order != null
        //    && x.OrderGoods.GoodsId == goodsId
        //    && (!searchParam.Keyword.IsNotNullOrEmpty()
        //    || (x.OrderGoods.Order.OrderNo != null && x.OrderGoods.Order.OrderNo.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "-Id");
        //返回成功200
        return Ok(result);
    }

    /// <summary>
    /// 获取指定商品评价列表
    /// 示例：/client/goods/evaluate/view/1/10
    /// </summary>
    [HttpGet("/client/goods/evaluate/view/{goodsId}/{top}")]
    [CachingFilter]
    [NonUnify]
    public async Task<IActionResult> GetByGoodsList([FromRoute] long goodsId, [FromRoute] int top, [FromQuery] EvaluateParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<OrderEvaluateDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<OrderEvaluateDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据库列表
        var resultFrom = await _orderEvaluateService.AsQueryable()
           .Where(x => x.Status == 0 && x.OrderGoodsId == goodsId)
           .WhereIF(searchParam.Image > 0, x => SqlFunc.Subqueryable<OrderEvaluateAlbum>().Where(d => d.EvaluateId == x.Id).Any())
           .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => SqlFunc.Subqueryable<Orders>().Where(d => d.Id == x.OrderId && d.OrderNo.Contains(searchParam.Keyword)).Any())
           .WhereIF(searchParam.Score == 3, x => x.GoodsScore == 3)
           .WhereIF(searchParam.Score > 0 && searchParam.Score <= 2, x => x.GoodsScore <= 2)
           .OrderBy(searchParam.OrderBy ?? "-Id")
           .AutoTake(top)
           .ToListAsync();

        //var resultFrom = await _orderEvaluateService.QueryListAsync(top,
        //    x => x.Status == 0
        //    && x.OrderGoods != null
        //    && x.OrderGoods.Order != null
        //    && x.OrderGoods.GoodsId == goodsId
        //    && (searchParam.Score == 0 || (searchParam.Score == 3 && x.GoodsScore == 3) || (searchParam.Score <= 2 && x.GoodsScore <= 2))
        //    && (searchParam.Image == 0 || x.EvaluateAlbums.Count > 0)
        //    && (!searchParam.Keyword.IsNotNullOrEmpty()
        //    || (x.OrderGoods.Order.OrderNo != null && x.OrderGoods.Order.OrderNo.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "-Id");

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<OrderEvaluateDto>>(); //.ShapeData(searchParam.Fields);
        //返回成功200
        return Ok(resultDto);
    }

    /// <summary>
    /// 获取指定商品评价分页列表
    /// 示例：/client/goods/evaluate/1?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet("/client/goods/evaluate/{goodsId}")]
    [CachingFilter]
    [NonUnify]
    public async Task<dynamic> GetByGoodsList([FromRoute] long goodsId, [FromQuery] EvaluateParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<OrderEvaluateDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<OrderEvaluateDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表，如果ID大于0则查询该用户下所有的列表
        var list = await _orderEvaluateService.AsQueryable()
            .Includes(x => x.EvaluateAlbums)
           .Where(x => x.Status ==0 && x.OrderGoodsId == goodsId)
           .WhereIF(searchParam.Image > 0, x => SqlFunc.Subqueryable<OrderEvaluateAlbum>().Where(d => d.EvaluateId == x.Id).Any())
           .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => SqlFunc.Subqueryable<Orders>().Where(d => d.Id == x.OrderId && d.OrderNo.Contains(searchParam.Keyword)).Any())
           .WhereIF(searchParam.Score == 3,x=>x.GoodsScore ==3)
           .WhereIF(searchParam.Score>0 && searchParam.Score <=2, x=>x.GoodsScore <=2)
           .OrderBy(searchParam.OrderBy ?? "-Id")
           .ToPagedListAsync(pageParam.PageIndex, pageParam.PageSize);

        return PageResult<OrderEvaluateDto>.SqlSugarPageResult(list);

        //var list = await _orderEvaluateService.QueryPageAsync(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => x.Status == 0
        //    && x.OrderGoods != null
        //    && x.OrderGoods.Order != null
        //    && x.OrderGoods.GoodsId == goodsId
        //    && (searchParam.Score == 0 || (searchParam.Score == 3 && x.GoodsScore == 3) || (searchParam.Score <= 2 && x.GoodsScore <= 2))
        //    && (searchParam.Image == 0 || x.EvaluateAlbums.Count > 0)
        //    && (!searchParam.Keyword.IsNotNullOrEmpty()
        //    || (x.OrderGoods.Order.OrderNo != null && x.OrderGoods.Order.OrderNo.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "-Id");

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
        //var resultDto = list.Adapt<IEnumerable<OrderEvaluateDto>>().ShapeData(searchParam.Fields);
        //return Ok(resultDto);
    }
    #endregion


    /// <summary>
    /// 添加一条记录
    /// </summary>
    private async Task<bool> AddAsync(long orderId, ICollection<OrderEvaluateEditDto> listDto)
    { 

        //检查会员是否登录
        var userId = await  _userService.GetUserIdAsync();
        if (listDto == null || listDto.Count == 0)
        {
            throw Oops.Oh("找不到评价的商品");
        }
        //检查订单商品是否存在
        var orderModel = await _orderEvaluateService.Context.Queryable<Orders>().Includes(x => x.OrderGoods).SingleAsync(x => x.Id == orderId && x.UserId == userId);
        if (orderModel == null)
        {
            throw Oops.Oh("订单不存在或已删除");
        }
        //循环写入
        var shopGoodsList = new List<ShopGoods>(); //待修改的商品列表
        var orderGoodsList = new List<OrderGoods>(); //待修改的订单商品列表
        var evaluateList = new List<OrderEvaluate>(); //待添加的评价列表
        foreach (var modelt in listDto)
        {
            //查找对应的订单商品
            var orderGoodsModel = orderModel.OrderGoods.FirstOrDefault(x => x.Id == modelt.orderGoodsId);
            if (orderGoodsModel == null)
            {
                throw Oops.Oh("订单商品不存在或已删除");
            }
            if (orderGoodsModel.EvaluateStatus > 0)
            {
                throw Oops.Oh("订单商品已评价");
            }
            _orderEvaluateService.Context.Tracking(orderGoodsModel);
            orderGoodsModel.EvaluateStatus = 1;//更改为已评价
            orderGoodsList.Add(orderGoodsModel);
            //查找对应的商品
            var goodsModel = shopGoodsList.FirstOrDefault(x => x.Id == orderGoodsModel.GoodsId);
            if (goodsModel != null)
            {
                _orderEvaluateService.Context.Tracking(goodsModel);
                goodsModel.EvaluateCount++;
            }
            else
            {
                goodsModel = await _orderEvaluateService.Context.Queryable<ShopGoods>().SingleAsync(x => x.Id == orderGoodsModel.GoodsId);
                if (goodsModel != null)
                {
                    _orderEvaluateService.Context.Tracking(goodsModel);
                    goodsModel.EvaluateCount++;
                    shopGoodsList.Add(goodsModel);
                }
            }

            //映射成实体
            var model = modelt.Adapt<OrderEvaluate>();
            model.UserId = userId;
            model.UserName = await _userService.GetUserNameAsync();
            model.AddTime = DateTime.Now;
            model.Status = 0;
            model.OrderId = orderId;
            evaluateList.Add(model);
        }
        //保存评价和修改订单商品评价状态
        await _orderEvaluateService.Context.Updateable<ShopGoods>(shopGoodsList).ExecuteCommandAsync();
        await _orderEvaluateService.Context.Updateable<OrderGoods>(orderGoodsList).ExecuteCommandAsync();
        var result = await _orderEvaluateService.InsertAsync(evaluateList) > 0;
        if (!result)
        {
            throw Oops.Oh("数据保存时发生意外错误");
        }
        return result;
    }
}