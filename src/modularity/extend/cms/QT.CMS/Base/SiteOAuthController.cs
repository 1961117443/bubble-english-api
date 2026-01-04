namespace QT.CMS;

/// <summary>
/// 授权登录
/// </summary>
[Route("api/cms/admin/site/oauth")]
[ApiController]
public class SiteOAuthController : ControllerBase
{
    private readonly ISqlSugarRepository<SiteOAuths> _siteOAuthService;
    private readonly IUserService _userService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public SiteOAuthController(ISqlSugarRepository<SiteOAuths> siteOAuthService, IUserService userService)
    {
        _siteOAuthService = siteOAuthService;
        _userService = userService;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 根据ID获取数据
    /// 示例：/admin/site/oauth/1
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    [AuthorizeFilter("OAuth", ActionType.View)]
    public async Task<IActionResult> GetById([FromRoute] int id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<SiteOAuthsDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var model = await _siteOAuthService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        //使用AutoMapper转换成ViewModel
        //根据字段进行塑形
        var result = model.Adapt<SiteOAuthsDto>().ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 获取指定数量列表
    /// 示例：/admin/site/oauth/view/10
    /// </summary>
    [HttpGet("view/{top}")]
    [Authorize]
    [AuthorizeFilter("OAuth", ActionType.View)]
    public async Task<IEnumerable<SiteOAuthsDto>> GetList([FromRoute] int top, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<SiteOAuthsDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<SiteOAuthsDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //获取数据库列表
        var resultFrom = await _siteOAuthService.AsQueryable()
            .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.keyword))
            .OrderBy(searchParam.OrderBy ?? "SortId,Id")
            .AutoTake(top)
            .ToListAsync();
        //var resultFrom = await _siteOAuthService.QueryListAsync<SiteOAuths>(top,
        //    x => (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "SortId,Id");
        //使用AutoMapper转换成ViewModel，根据字段进行塑形
        //var resultDto = _mapper.Adapt<IEnumerable<SiteOAuthsDto>>(resultFrom).ShapeData(searchParam.Fields);
        ////返回成功200
        //return Ok(resultDto);

        return resultFrom.Adapt<IEnumerable<SiteOAuthsDto>>();
    }

    /// <summary>
    /// 获取分页列表
    /// 示例：/admin/site/oauth?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet]
    [Authorize]
    [AuthorizeFilter("OAuth", ActionType.View)]
    public async Task<dynamic> GetList([FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.TrimStart('-').IsPropertyExists<SiteOAuthsDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<SiteOAuthsDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表
        var resultFrom = await _siteOAuthService.AsQueryable()
            .Includes(x=>x.Site)
            .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.keyword))
            .OrderBy(searchParam.OrderBy ?? "SortId,Id")
            .ToPagedListAsync(searchParam.currentPage, searchParam.pageSize);
                
        //var list = await _siteOAuthService.QueryPageAsync(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => !searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword)),
        //    searchParam.OrderBy ?? "SortId,Id");

        //x-pagination
        //var paginationMetadata = new
        //{
        //    totalCount = list.TotalCount,
        //    pageSize = list.PageSize,
        //    pageIndex = list.PageIndex,
        //    totalPages = list.TotalPages
        //};
        //Response.Headers.Add("x-pagination", SerializeHelper.SerializeObject(paginationMetadata));

        ////映射成DTO
        //var resultDto = _mapper.Adapt<IEnumerable<SiteOAuthsDto>>(list).ShapeData(searchParam.Fields);
        //return Ok(resultDto);

        return PageResult<SiteOAuthsDto>.SqlSugarPageResult(resultFrom);
    }

    /// <summary>
    /// 添加一条记录
    /// 示例：/admin/site/oauth/
    /// </summary>
    [HttpPost]
    [Authorize]
    [AuthorizeFilter("OAuth", ActionType.Add)]
    public async Task<IActionResult> Add([FromBody] SiteOAuthsEditDto modelDto)
    {
        //映射成实体
        var model = modelDto.Adapt<SiteOAuths>();
        //获取当前用户名
        model.AddBy = await  _userService.GetUserIdAsync();
        model.AddTime = DateTime.Now;
        //写入数据库
        await _siteOAuthService.InsertReturnEntityAsync(model);
        //映射成DTO再返回，否则出错
        var result = model.Adapt<SiteOAuthsDto>();
        return Ok(result);
    }

    /// <summary>
    /// 修改一条记录
    /// 示例：/admin/site/oauth/1
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [AuthorizeFilter("OAuth", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] SiteOAuthsEditDto modelDto)
    {
        //查找记录
        var model = await _siteOAuthService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
           throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        _siteOAuthService.Context.Tracking(model);
        //更新操作AutoMapper替我们完成，只需要调用保存即可
        modelDto.Adapt(model);
        var result = await _siteOAuthService.AutoUpdateAsync(model);
        return NoContent();
    }

    /// <summary>
    /// 局部更新一条记录
    /// 示例：/admin/site/oauth/1
    /// Body：[{"op":"replace","path":"/title","value":"new title"}]
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize]
    [AuthorizeFilter("OAuth", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] JsonPatchDocument<SiteOAuthsEditDto> patchDocument)
    {
        var model = await _siteOAuthService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }

        var modelToPatch = model.Adapt<SiteOAuthsEditDto>();
        patchDocument.ApplyTo(modelToPatch, ModelState);
        //验证数据是否合法
        if (!TryValidateModel(modelToPatch))
        {
            return ValidationProblem(ModelState);
        }
        _siteOAuthService.Context.Tracking(model);
        //更新操作AutoMapper替我们完成，只需要调用保存即可
        modelToPatch.Adapt(model);
        await _siteOAuthService.AutoUpdateAsync(model);
        return NoContent();
    }

    /// <summary>
    /// 删除一条记录
    /// 示例：/admin/site/oauth/1
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [AuthorizeFilter("OAuth", ActionType.Delete)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        if (!await _siteOAuthService.AnyAsync(x => x.Id == id))
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        var result = await _siteOAuthService.DeleteAsync(x => x.Id == id);

        return NoContent();
    }

    /// <summary>
    /// 批量删除记录(级联数据)
    /// 示例：/admin/site/oauth?ids=1,2,3
    /// </summary>
    [HttpDelete]
    [Authorize]
    [AuthorizeFilter("OAuth", ActionType.Delete)]
    public async Task<IActionResult> DeleteByIds([FromQuery] string Ids)
    {
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
        await _siteOAuthService.DeleteAsync(x => listIds.Contains(x.Id));

        return NoContent();
    }
    #endregion

    #region 前台调用接口============================
    /// <summary>
    /// 获取指定数量列表
    /// 示例：/client/site/oauth/view/10
    /// </summary>
    [HttpGet("/client/site/oauth/view/{top}")]
    [CachingFilter,AllowAnonymous,NonUnify]
    public async Task<IEnumerable<SiteOAuthsDto>> GetClientList([FromRoute] int top, [FromQuery] OAuthParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<SiteOAuthsDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<SiteOAuthsDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //将接口类型转换成IEnumerable
        var listTypes = searchParam.Types.ToIEnumerable<string>();
        //获取数据库列表
        var resultFrom = await _siteOAuthService.AsQueryable()
            .Where(x => x.Status == 0)
            .WhereIF(searchParam.SiteId > 0, x => x.SiteId == searchParam.SiteId)
            .WhereIF(listTypes.IsAny(), x => listTypes.Contains(x.Type))
            .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.keyword))
            .OrderBy(searchParam.OrderBy ?? "SortId,Id")
            .AutoTake(top)
            .ToListAsync();
        //var resultFrom = await _siteOAuthService.QueryListAsync<SiteOAuths>(top,
        //    x => x.Status == 0
        //    && (searchParam.SiteId < 0 || x.SiteId == searchParam.SiteId)
        //    && (listTypes == null || listTypes.Contains(x.Type))
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "SortId,Id");
        ////使用AutoMapper转换成ViewModel，根据字段进行塑形
        //var resultDto = _mapper.Adapt<IEnumerable<SiteOAuthsDto>>(resultFrom).ShapeData(searchParam.Fields);
        ////返回成功200
        //return Ok(resultDto);

        return resultFrom.Adapt<IEnumerable<SiteOAuthsDto>>();
    }
    #endregion
}