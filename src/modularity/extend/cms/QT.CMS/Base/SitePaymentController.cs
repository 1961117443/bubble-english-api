namespace QT.CMS;

/// <summary>
/// 站点支付方式接口
/// </summary>
[Route("api/cms/admin/site/payment")]
[ApiController]
public class SitePaymentController : ControllerBase
{
    private readonly ISqlSugarRepository<SitePayment> _sitePaymentService;
    private readonly IUserService _userService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public SitePaymentController(ISqlSugarRepository<SitePayment> sitePaymentService,IUserService userService)
    {
        _sitePaymentService = sitePaymentService;
        _userService = userService;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 根据ID获取数据
    /// 示例：/admin/site/payment/1
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    [AuthorizeFilter("Payment", ActionType.View)]
    public async Task<IActionResult> GetById([FromRoute] int id, [FromQuery] PaymentParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<SitePaymentDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var model = await _sitePaymentService.AsQueryable()
            .Includes(x => x.Payment)
            .Includes(x => x.Site)
            .Where(x => x.Id == id)
            .FirstAsync();
        //var model = await _sitePaymentService.QueryAsync(x => x.Id == id, WriteRoRead.Write);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        //根据字段进行塑形
        var result = model.Adapt<SitePaymentDto>().ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 获取列表
    /// 示例：/admin/site/payment/view/10
    /// </summary>
    [HttpGet("view/{top}")]
    [Authorize]
    [AuthorizeFilter("Payment", ActionType.View)]
    public async Task<IActionResult> GetList([FromRoute] int top, [FromQuery] PaymentParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<SitePaymentDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<SitePaymentDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //将接口类型转换成IEnumerable
        var listTypes = searchParam.Types.ToIEnumerable<string>();

        //获取数据库列表
        var resultFrom = await _sitePaymentService.AsQueryable()
            .Includes(x => x.Payment)
            .Includes(x => x.Site)
            .WhereIF(searchParam.SiteId > 0, x => x.SiteId == searchParam.SiteId)
            .WhereIF(listTypes.IsAny(), x => listTypes.Contains(x.Type))
            .WhereIF(searchParam.Status >= 0, x => x.Payment.Status == searchParam.Status)
            .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.keyword))
            .OrderBy(searchParam.OrderBy ?? "AddTime")
            .AutoTake(top)
            .ToListAsync();

        //var resultFrom = await _sitePaymentService.QueryListAsync(top,
        //    x => (searchParam.SiteId <= 0 || x.SiteId == searchParam.SiteId)
        //    && (listTypes == null || listTypes.Contains(x.Type))
        //    && (searchParam.Status < 0 || (x.Payment != null && x.Payment.Status == searchParam.Status))
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "AddTime");
        //使用AutoMapper转换成ViewModel
        //根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<SitePaymentDto>>(); // .ShapeData(searchParam.Fields);
        //返回成功200
        return Ok(resultDto);
    }

    /// <summary>
    /// 获取分页列表
    /// 示例：/admin/site/payment?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet]
    [Authorize]
    [AuthorizeFilter("Payment", ActionType.View)]
    public async Task<IActionResult> GetList([FromQuery] PaymentParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.TrimStart('-').IsPropertyExists<SitePaymentDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<SitePaymentDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //将接口类型转换成IEnumerable
        var listTypes = searchParam.Types.ToIEnumerable<string>();

        //获取数据列表，如果站点ID大于0则查询该站点下所有的列表
        var list = await _sitePaymentService.AsQueryable()
          .Includes(x => x.Payment)
          .Includes(x => x.Site)
          .WhereIF(searchParam.SiteId > 0, x => x.SiteId == searchParam.SiteId)
          .WhereIF(listTypes.IsAny(), x => listTypes.Contains(x.Type))
          .WhereIF(searchParam.Status >= 0, x => x.Payment.Status == searchParam.Status)
          .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.keyword))
          .OrderBy(searchParam.OrderBy ?? "-AddTime,-Id")
           .ToPagedListAsync(searchParam.currentPage, searchParam.pageSize);
        return Ok(PageResult<SitePaymentDto>.SqlSugarPageResult(list));

        //var list = await _sitePaymentService.QueryPageAsync(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => (searchParam.SiteId <= 0 || x.SiteId == searchParam.SiteId)
        //    && (listTypes == null || listTypes.Contains(x.Type))
        //    && (searchParam.Status < 0 || (x.Payment != null && x.Payment.Status == searchParam.Status))
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "-AddTime,-Id");

        ////x-pagination
        //var paginationMetadata = new
        //{
        //    totalCount = list.TotalCount,
        //    pageSize = list.PageSize,
        //    pageIndex = list.PageIndex,
        //    totalPages = list.TotalPages
        //};
        //Response.Headers.Add("x-pagination", SerializeHelper.SerializeObject(paginationMetadata));

        ////映射成DTO
        //var resultDto = _mapper.Map<IEnumerable<SitePaymentDto>>(list).ShapeData(searchParam.Fields);
        //return Ok(resultDto);
    }

    /// <summary>
    /// 添加一条记录
    /// 示例：/admin/site/payment
    /// </summary>
    [HttpPost]
    [Authorize]
    [AuthorizeFilter("Payment", ActionType.Add)]
    public async Task<IActionResult> Add([FromBody] SitePaymentEditDto modelDto)
    {
        //映射成实体
        var model = modelDto.Adapt<SitePayment>();
        //查找用户名
        model.AddBy = await _userService.GetUserNameAsync();
        model.AddTime = DateTime.Now;
        //写入数据库
        await _sitePaymentService.InsertReturnEntityAsync(model);
        //映射成DTO再返回，否则出错
        var result = model.Adapt<SitePaymentEditDto>();
        return Ok(result);
    }

    /// <summary>
    /// 修改一条记录
    /// 示例：/admin/site/payment/1
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [AuthorizeFilter("Payment", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] SitePaymentEditDto modelDto)
    {
        //查找记录
        var model = await _sitePaymentService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        _sitePaymentService.Context.Tracking(model);
        //更新操作AutoMapper替我们完成，只需要调用保存即可
        modelDto.Adapt( model);
        var result = await _sitePaymentService.AutoUpdateAsync(model);
        return NoContent();
    }

    /// <summary>
    /// 局部更新一条记录
    /// 示例：/admin/site/payment/1
    /// Body：[{"op":"replace","path":"/title","value":"new title"}]
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize]
    [AuthorizeFilter("Payment", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] JsonPatchDocument<SitePaymentEditDto> patchDocument)
    {
        var model = await _sitePaymentService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }

        var modelToPatch = model.Adapt<SitePaymentEditDto>();
        patchDocument.ApplyTo(modelToPatch, ModelState);
        //验证数据是否合法
        if (!TryValidateModel(modelToPatch))
        {
            return ValidationProblem(ModelState);
        }
        _sitePaymentService.Context.Tracking(model);
        //更新操作AutoMapper替我们完成，只需要调用保存即可
        modelToPatch.Adapt(model);
        await _sitePaymentService.AutoUpdateAsync(model);
        return NoContent();
    }

    /// <summary>
    /// 删除一条记录
    /// 示例：/admin/site/payment/1
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [AuthorizeFilter("Payment", ActionType.Delete)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        //检查参数是否正确
        if (!await _sitePaymentService.AnyAsync(x => x.Id == id))
        {
            throw Oops.Oh($"数据[{id}]不存在或无权删除");
        }
        var result = await _sitePaymentService.DeleteAsync(x => x.Id == id);

        return NoContent();
    }

    /// <summary>
    /// 批量删除记录
    /// 示例：//admin/site/payment?ids=1,2,3
    /// </summary>
    [HttpDelete]
    [Authorize]
    [AuthorizeFilter("Payment", ActionType.Delete)]
    public async Task<IActionResult> DeleteByIds([FromQuery] string Ids)
    {
        //检查参数是否为空
        if (Ids == null)
        {
            throw Oops.Oh("传输参数不可为空");
        }
        //将ID列表转换成IEnumerable
        var listIds = Ids.ToIEnumerable<int>();
        if (listIds == null)
        {
            throw Oops.Oh("传输参数不符合规范");
        }
        //执行批量删除操作
        await _sitePaymentService.DeleteAsync(x => listIds.Contains(x.Id));
        return NoContent();
    }
    #endregion

    #region 前台调用接口============================
    /// <summary>
    /// 获取列表
    /// 示例：/client/payment/view/10
    /// </summary>
    [HttpGet("/client/payment/view/{top}")]
    [CachingFilter]
    [NonUnify]
    public async Task<IActionResult> ClientGetList([FromRoute] int top, [FromQuery] PaymentParameter searchParam)
    {
        searchParam.Status = 0;
        searchParam.Fields = "Id,SiteId,Type,Title,Payment";
        return await GetList(top, searchParam);
    }
    #endregion
}