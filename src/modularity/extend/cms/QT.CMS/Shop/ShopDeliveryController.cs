using NPOI.XWPF.UserModel;

namespace QT.CMS;

/// <summary>
/// 配送方式
/// </summary>
[Route("api/cms/admin/shop/delivery")]
[ApiController]
public class ShopDeliveryController : ControllerBase
{
    private readonly ISqlSugarRepository<ShopDelivery> _shopDeliveryService;
    private readonly IUserService _userService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public ShopDeliveryController(ISqlSugarRepository<ShopDelivery> shopDeliveryService,IUserService userService)
    {
        _shopDeliveryService = shopDeliveryService;
        _userService = userService;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 根据ID获取数据
    /// 示例：/admin/shop/delivery/1
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopDelivery", ActionType.View)]
    public async Task<IActionResult> GetById([FromRoute] int id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<ShopDeliveryDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var model = await _shopDeliveryService.AsQueryable().Includes(x=>x.DeliveryAreas).SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        //使用AutoMapper转换成ViewModel，根据字段进行塑形
        var result = model.Adapt<ShopDeliveryDto>().ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 获取指定数量列表
    /// 示例：/admin/shop/delivery/view/10
    /// </summary>
    [HttpGet("view/{top}")]
    [Authorize]
    [AuthorizeFilter("ShopDelivery", ActionType.View)]
    public async Task<IActionResult> GetList([FromRoute] int top, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ShopDeliveryDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ShopDeliveryDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据库列表
        //var resultFrom = await _shopDeliveryService.QueryListAsync(top,
        //    x => (searchParam.Status < 0 || x.Status == searchParam.Status)
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "SortId,-Id");

        var resultFrom = await _shopDeliveryService.AsQueryable().Includes(x => x.DeliveryAreas)
            .WhereIF(searchParam.Status > 0, x => x.Status == searchParam.Status)
            .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.keyword))
            .OrderBy(searchParam.OrderBy ?? "SortId,-Id")
            .AutoTake(top)
            .ToListAsync();

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<ShopDeliveryDto>>(); //.ShapeData(searchParam.Fields);
        //返回成功200
        return Ok(resultDto);
    }

    /// <summary>
    /// 获取分页列表
    /// 示例：/admin/shop/delivery?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet]
    [Authorize]
    [AuthorizeFilter("ShopDelivery", ActionType.View)]
    public async Task<IActionResult> GetList([FromQuery] BaseParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ShopDeliveryDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ShopDeliveryDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表，如果ID大于0则查询该用户下所有的列表
        //var list = await _shopDeliveryService.QueryPageAsync(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => (searchParam.Status < 0 || x.Status == searchParam.Status)
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "SortId,-Id");

        var list = await _shopDeliveryService.AsQueryable().Includes(x => x.DeliveryAreas)
           .WhereIF(searchParam.Status > 0, x => x.Status == searchParam.Status)
           .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.keyword))
           .OrderBy(searchParam.OrderBy ?? "SortId,-Id")
           .ToPagedListAsync(searchParam.currentPage,searchParam.pageSize);


        ////x-pagination
        //var paginationMetadata = new
        //{
        //    totalCount = list.TotalCount,
        //    pageSize = list.PageSize,
        //    pageIndex = list.PageIndex,
        //    totalPages = list.TotalPages
        //};
        //Response.Headers.Add("x-pagination", SerializeHelper.SerializeObject(paginationMetadata));

        ////映射成DTO，根据字段进行塑形
        //var resultDto = _mapper.Adapt<IEnumerable<ShopDeliveryDto>>(list).ShapeData(searchParam.Fields);
        //return Ok(resultDto);
        return Ok(PageResult<ShopDeliveryDto>.SqlSugarPageResult(list));
    }

    /// <summary>
    /// 添加一条记录
    /// 示例：/admin/shop/delivery
    /// </summary>
    [HttpPost]
    [Authorize]
    [AuthorizeFilter("ShopDelivery", ActionType.Add)]
    public async Task<ShopDeliveryDto> Add([FromBody] ShopDeliveryEditDto modelDto)
    {
        //保存数据
        //映射成实体
        var model = modelDto.Adapt<ShopDelivery>();
        model.AddBy = await _userService.GetUserNameAsync();
        model.AddTime = DateTime.Now;
        //保存
        var result = await _shopDeliveryService.Context.InsertNav(model).Include(x => x.DeliveryAreas).ExecuteReturnEntityAsync();
        //if (!result)
        //{
        //    throw Oops.Oh("数据保存时发生意外错误");
        //}
        //映射成DTO
        return result.Adapt<ShopDeliveryDto>();
    }

    /// <summary>
    /// 修改一条记录
    /// 示例：/admin/shop/delivery/1
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopDelivery", ActionType.Edit)]
    public async Task Update([FromRoute] int id, [FromBody] ShopDeliveryEditDto modelDto)
    {
        ////保存数据
        //var result = await _shopDeliveryService.UpdateAsync(id, modelDto);
        //return NoContent();

        //根据ID获取记录
        var model = await _shopDeliveryService.AsQueryable().Includes(x => x.DeliveryAreas).SingleAsync(x => x.Id == id);
        //如果不存在则抛出异常
        if (model == null)
        {
            throw Oops.Oh("数据不存在或已删除");
        }
        _shopDeliveryService.Context.Tracking(model);
        //将DTO映射到源数据
        var mapModel = modelDto.Adapt(model);
        //设置更新人和更新时间
        mapModel.UpdateBy = await _userService.GetUserNameAsync();
        mapModel.UpdateTime = DateTime.Now;
        //保存
        await _shopDeliveryService.Context.UpdateNav(mapModel).Include(x => x.DeliveryAreas).ExecuteCommandAsync();
        //await _shopDeliveryService.AutoUpdateAsync(mapModel);
    }

    /// <summary>
    /// 局部更新一条记录
    /// 示例：/admin/shop/delivery/1
    /// Body：[{"op":"replace","path":"/title","value":"new title"}]
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopDelivery", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] JsonPatchDocument<ShopDeliveryEditDto> patchDocument)
    {
        var model = await _shopDeliveryService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }

        var modelToPatch = model.Adapt<ShopDeliveryEditDto>();
        patchDocument.ApplyTo(modelToPatch, ModelState);
        //验证数据是否合法
        if (!TryValidateModel(modelToPatch))
        {
            return ValidationProblem(ModelState);
        }

        _shopDeliveryService.Context.Tracking(model);
        //更新操作AutoMapper替我们完成，只需要调用保存即可
        modelToPatch.Adapt(model);
        await _shopDeliveryService.AutoUpdateAsync(model);

        return NoContent();
    }

    /// <summary>
    /// 删除一条记录
    /// 示例：/admin/shop/delivery/1
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopDelivery", ActionType.Delete)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        if (!await _shopDeliveryService.AnyAsync(x => x.Id == id))
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        var result = await _shopDeliveryService.DeleteAsync(x => x.Id == id);

        return NoContent();
    }

    /// <summary>
    /// 批量删除记录(级联数据)
    /// 示例：/admin/shop/delivery?ids=1,2,3
    /// </summary>
    [HttpDelete]
    [Authorize]
    [AuthorizeFilter("ShopDelivery", ActionType.Delete)]
    public async Task<IActionResult> DeleteByIds([FromQuery] string Ids)
    {
        if (Ids == null)
        {
            throw Oops.Oh("传输参数不可为空");
        }
        //将ID列表转换成IEnumerable
        var arrIds = Ids.ToIEnumerable<int>();
        if (arrIds == null)
        {
            throw Oops.Oh("传输参数不符合规范");
        }
        //执行批量删除操作
        await _shopDeliveryService.DeleteAsync(x => arrIds.Contains(x.Id));

        return NoContent();
    }
    #endregion

    #region 前台调用接口============================
    /// <summary>
    /// 获取指定数量列表
    /// 示例：/client/shop/delivery/view/10
    /// </summary>
    [HttpGet("/client/shop/delivery/view/{top}")]
    [CachingFilter]
    [NonUnify]
    public async Task<IActionResult> ClientGetList([FromRoute] int top, [FromQuery] BaseParameter searchParam)
    {
        searchParam.Status = 0; //只查启用记录
        return await GetList(top, searchParam);
    }

    /// <summary>
    /// 获取运费统计信息
    /// 示例：/client/shop/delivery/total/1
    /// </summary>
    [HttpGet("/client/shop/delivery/total/{id}")]
    [NonUnify]
    public async Task<ShopDeliveryTotalDto> ClientGetTotal([FromRoute] int id, [FromQuery] DeliveryParameter param)
    {
        //查询配送方式
        var model = await _shopDeliveryService.AsQueryable().Includes(x => x.DeliveryAreas)
            .FirstAsync(x => x.Id == id && x.Status == 0);
        if (model == null)
        {
            throw Oops.Oh("配送方式有误，请确认后操作");
        }
        if (model.IsInsure == 0 && param.IsInsure > 0)
        {
            throw Oops.Oh("当前配送方式不支持保价");
        }
        //创建待赋值实体
        ShopDeliveryTotalDto result = new ShopDeliveryTotalDto
        {
            id = model.Id,
            title = model.Title,
            freight = model.FirstPrice
        };
        //重量为0时直接返回首重费用
        if (param.TotalWeight == 0)
        {
            return result;
        }
        //转换成千克
        param.TotalWeight = param.TotalWeight > 1000 ? param.TotalWeight / 1000 : 1;

        //计算配送费用
        decimal firstPrice = model.FirstPrice;//首重费用
        decimal secondPrice = model.SecondPrice;//续重费用
        decimal totalSecondPrice = 0;//续重总费用
                                     //如果符合自定义地区，采用地区费用
        var areaModel = model.DeliveryAreas.FirstOrDefault(x => x.Province == param.Province);
        if (areaModel != null)
        {
            firstPrice = areaModel.FirstPrice;
            secondPrice = areaModel.SecondPrice;
        }
        //如果总重量大于首重才计算续重费用
        if (param.TotalWeight > model.FirstWeight)
        {
            //续重重量=总重量-首重量
            decimal secondWeight = param.TotalWeight - model.FirstWeight;
            //向上取整，只要有小数都加1
            //续重费用=(续重重量/续重量)*续重价格
            totalSecondPrice = Math.Ceiling(secondWeight / model.SecondWeight) * secondPrice;
        }
        //保价费用=保价金额*保价费率
        decimal insureFreight = 0;
        if (param.IsInsure > 0)
        {
            insureFreight = param.IsInsure * ((decimal)model.InsureRate / 1000);
            if (insureFreight < model.InsurePrice)
            {
                insureFreight = model.InsurePrice;//最低保价
            }
        }
        //总运费=首重费用+续重费用+保价费用
        result.freight = firstPrice + totalSecondPrice + insureFreight;

        return result;
    }
    #endregion
}