namespace QT.CMS;

/// <summary>
/// 授权记录
/// </summary>
[Route("api/cms/admin/site/oauth/login")]
[ApiController]
public class SiteOAuthLoginController : ControllerBase
{
    private readonly ISqlSugarRepository<SiteOAuthLogin> _siteOAuthLoginService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public SiteOAuthLoginController(ISqlSugarRepository<SiteOAuthLogin> siteOAuthLoginService)
    {
        _siteOAuthLoginService = siteOAuthLoginService;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 获取分页列表
    /// 示例：/admin/site/oauth/login?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet]
    [Authorize]
    [AuthorizeFilter("OAuth", ActionType.View)]
    public async Task<dynamic> GetList([FromQuery] BaseParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.TrimStart('-').IsPropertyExists<SiteOAuthLoginDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<SiteOAuthLoginDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表
        var list = await _siteOAuthLoginService.AsQueryable()
            .Includes(x=>x.User)
            .Includes(x=>x.OAuth)
            .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x=>x.Provider.Contains(searchParam.keyword) || x.User.RealName.Contains(searchParam.keyword))
            .OrderBy(searchParam.OrderBy ?? "Id")
            .ToPagedListAsync(searchParam.currentPage, searchParam.pageSize);
        return PageResult<SiteOAuthLoginDto>.SqlSugarPageResult(list);

        //var list = await _siteOAuthLoginService.QueryPageAsync(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => !searchParam.Keyword.IsNotNullOrEmpty() || (x.Provider != null && x.Provider.Contains(searchParam.Keyword)) || (x.User != null && x.User.UserName.Contains(searchParam.Keyword)),
        //    searchParam.OrderBy ?? "Id");

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
        //var resultDto = _mapper.Map<IEnumerable<SiteOAuthLoginDto>>(list).ShapeData(searchParam.Fields);
        //return Ok(resultDto);
    }

    /// <summary>
    /// 删除一条记录
    /// 示例：/admin/site/oauth/login/1
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [AuthorizeFilter("OAuth", ActionType.Delete)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        if (!await _siteOAuthLoginService.AnyAsync(x => x.Id == id))
        {
            throw Oops.Oh($"数据{id}不存在或已删除");
        }
        var result = await _siteOAuthLoginService.DeleteAsync(x => x.Id == id);

        return NoContent();
    }

    /// <summary>
    /// 批量删除记录(级联数据)
    /// 示例：/admin/site/oauth/login?ids=1,2,3
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
        await _siteOAuthLoginService.DeleteAsync(x => listIds.Contains(x.Id));

        return NoContent();
    }
    #endregion
}
