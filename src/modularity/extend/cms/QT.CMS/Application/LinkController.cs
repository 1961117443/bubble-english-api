using QT.CMS.Entitys.Dto.Application;

namespace QT.CMS;

/// <summary>
/// 友情链接
/// </summary>
[Route("/api/cms/admin/link")]
[ApiController]
public class LinkController : ControllerBase
{
    private readonly ISqlSugarRepository<LinksEntity> _linkService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public LinkController(ISqlSugarRepository<LinksEntity> linkService)
    {
        _linkService = linkService;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 根据ID获取数据
    /// 示例：/admin/link/1
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    [AuthorizeFilter("Link", ActionType.View)]
    public async Task<IActionResult> GetById([FromRoute] int id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<LinksDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var model = await _linkService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据{id}不存在或已删除");
        }
        //使用AutoMapper转换成ViewModel
        //根据字段进行塑形
        var result = model.Adapt<LinksDto>();//.ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 获取站点列表
    /// 示例：/admin/link?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet]
    [Authorize]
    [AuthorizeFilter("Link", ActionType.View)]
    public async Task<IActionResult> GetList([FromQuery] BaseParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null && !searchParam.OrderBy.TrimStart('-').IsPropertyExists<LinksDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<LinksDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表
        var list = await _linkService.AsQueryable()
            .WhereIF(searchParam.keyword.IsNotEmptyOrNull(),x => x.Title.Contains(searchParam.Keyword))
            .OrderBy(searchParam.OrderBy ?? "SortId,Id")
            .ToPagedListAsync(pageParam.PageIndex,pageParam.PageSize);

        return Ok(PageResult<LinksDto>.SqlSugarPageResult(list));

        //var list = await _linkService.QueryPageAsync<Links>(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "SortId,Id");

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
        //var resultDto = _mapper.Map<IEnumerable<LinksDto>>(list).ShapeData(searchParam.Fields);
        //return Ok(resultDto);
    }

    /// <summary>
    /// 添加一条记录
    /// 示例：/admin/link
    /// </summary>
    [HttpPost]
    [Authorize]
    [AuthorizeFilter("Link", ActionType.Add)]
    public async Task<IActionResult> Add([FromBody] LinksEditDto modelDto)
    {
        //映射成实体
        var model = modelDto.Adapt<LinksEntity>();
        //写入数据库
        await _linkService.InsertReturnEntityAsync(model);
        //映射成DTO再返回，否则出错
        var result = model.Adapt<LinksDto>();
        return Ok(result);
    }

    /// <summary>
    /// 修改一条记录
    /// 示例：/admin/link/1
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [AuthorizeFilter("Link", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] LinksEditDto modelDto)
    {
        //查找记录
        var model = await _linkService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"记录{id}不存在或已删除");
        }
        //更新操作AutoMapper替我们完成，只需要调用保存即可
        _linkService.Context.Tracking(model);
        modelDto.Adapt(model);
        var result = await _linkService.AutoUpdateAsync(model);
        return NoContent();
    }

    /// <summary>
    /// 批量审核记录
    /// 示例：/admin/link?ids=1,2,3
    /// </summary>
    [HttpPut]
    [Authorize]
    [AuthorizeFilter("Link", ActionType.Audit)]
    public async Task<IActionResult> AuditByIds([FromQuery] string Ids)
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
        //找出符合条件的记录
        var list = await _linkService.AsQueryable().Where(x=>x.Status == 0 && arrIds.Contains(x.Id)).Take(1000).ToListAsync();
        //var list = await _linkService.QueryListAsync<Links>(1000, x => x.Status == 0 && arrIds.Contains(x.Id), WriteRoRead.Write);
        if (list == null || list.Count() == 0)
        {
            throw Oops.Oh("暂无需要审核的记录");
        }
        foreach (var item in list)
        {
            item.Status = 1;
        }
        //保存到数据库
        await _linkService.Context.Updateable<LinksEntity>(list).UpdateColumns(x => x.Status).ExecuteCommandAsync();

        return NoContent();
    }

    /// <summary>
    /// 局部更新一条记录
    /// 示例：/admin/link/1
    /// Body：[{"op":"replace","path":"/title","value":"new title"}]
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize]
    [AuthorizeFilter("Link", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] JsonPatchDocument<LinksEditDto> patchDocument)
    {
        var model = await _linkService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }

        var modelToPatch = model.Adapt<LinksEditDto>();
        patchDocument.ApplyTo(modelToPatch, ModelState);
        //验证数据是否合法
        if (!TryValidateModel(modelToPatch))
        {
            return ValidationProblem(ModelState);
        }
        _linkService.Context.Tracking(model);
        //更新操作AutoMapper替我们完成，只需要调用保存即可
        modelToPatch.Adapt(model);
        await _linkService.AutoUpdateAsync(model);

        return NoContent();
    }

    /// <summary>
    /// 删除一条记录
    /// 示例：/admin/link/1
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [AuthorizeFilter("Link", ActionType.Delete)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        if (!await _linkService.AnyAsync(x => x.Id == id))
        {
            throw Oops.Oh($"记录[{id}]不存在或已删除");
        }
        var result = await _linkService.DeleteAsync(x => x.Id == id);

        return NoContent();
    }

    /// <summary>
    /// 批量删除记录(级联数据)
    /// 示例：/admin/link?ids=1,2,3
    /// </summary>
    [HttpDelete]
    [Authorize]
    [AuthorizeFilter("Link", ActionType.Delete)]
    public async Task<IActionResult> DeleteByIds([FromQuery] string Ids)
    {
        if (Ids == null)
        {
            throw Oops.Oh("传输参数不可为空");
        }
        //将ID列表转换成IEnumerable
        var ids = Ids.ToIEnumerable<int>();
        if (ids == null)
        {
            throw Oops.Oh("传输参数不符合规范");
        }
        //执行批量删除操作
        await _linkService.DeleteAsync(x => ids.Contains(x.Id));

        return NoContent();
    }
    #endregion

    #region 前台调用接口============================
    /// <summary>
    /// 获取指定数量列表
    /// 示例：/client/link/view/10
    /// </summary>
    [HttpGet("/client/link/view/{top}")]
    [CachingFilter]
    public async Task<IActionResult> ClientGetList([FromRoute] int top, [FromQuery] LinkParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<LinksDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<LinksDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据库列表
        var resultFrom = await _linkService.AsQueryable()
            .Where(x => x.Status == 1)
            .WhereIF(searchParam.SiteId > 0, x => x.SiteId == searchParam.SiteId)
            .WhereIF(searchParam.IsRecom >= 0, x => x.IsRecom == searchParam.IsRecom)
            .WhereIF(searchParam.IsImage == 1, x => !SqlFunc.IsNullOrEmpty(x.LogoUrl))
            .WhereIF(searchParam.IsImage == 0, x => SqlFunc.IsNullOrEmpty(x.LogoUrl))
            .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.Keyword))
            .OrderBy(searchParam.OrderBy ?? "SortId,-Id")
            .AutoTake(top)
            .ToListAsync();

        //var resultFrom = await _linkService.QueryListAsync<Links>(top,
        //    x => x.Status == 1
        //    && (searchParam.SiteId < 0 || x.SiteId == searchParam.SiteId)
        //    && (searchParam.IsRecom < 0 || x.IsRecom == searchParam.IsRecom)
        //    && (searchParam.IsImage < 0 || (searchParam.IsImage == 1 && x.LogoUrl != null && x.LogoUrl.Length > 0) || (searchParam.IsImage == 0 && x.LogoUrl == null))
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "SortId,-Id");

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<LinksDto>>(); //.ShapeData(searchParam.Fields);
        //返回成功200
        return Ok(resultDto);
    }

    /// <summary>
    /// 获取分页列表
    /// 示例：/client/link?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet("/client/link")]
    [CachingFilter]
    public async Task<IActionResult> ClientGetList([FromQuery] LinkParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<LinksDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<LinksDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取所有的列表
        var list = await _linkService.AsQueryable()
           .Where(x => x.Status == 1)
           .WhereIF(searchParam.SiteId > 0, x => x.SiteId == searchParam.SiteId)
           .WhereIF(searchParam.IsRecom >= 0, x => x.IsRecom == searchParam.IsRecom)
           .WhereIF(searchParam.IsImage == 1, x => !SqlFunc.IsNullOrEmpty(x.LogoUrl))
           .WhereIF(searchParam.IsImage == 0, x => SqlFunc.IsNullOrEmpty(x.LogoUrl))
           .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.Keyword))
           .OrderBy(searchParam.OrderBy ?? "SortId,-Id")
           .ToPagedListAsync(pageParam.PageIndex, pageParam.PageSize);

        return Ok(PageResult<LinksDto>.SqlSugarPageResult(list));

        //var list = await _linkService.QueryPageAsync<Links>(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => x.Status == 1
        //    && (searchParam.SiteId < 0 || x.SiteId == searchParam.SiteId)
        //    && (searchParam.IsRecom < 0 || x.IsRecom == searchParam.IsRecom)
        //    && (searchParam.IsImage < 0 || (searchParam.IsImage == 1 && x.LogoUrl != null && x.LogoUrl.Length > 0) || (searchParam.IsImage == 0 && x.LogoUrl == null))
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "SortId,-Id");

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
        //var resultDto = _mapper.Map<IEnumerable<LinksDto>>(list).ShapeData(searchParam.Fields);
        //return Ok(resultDto);
    }

    /// <summary>
    /// 添加一条记录
    /// 示例：/client/link
    /// </summary>
    [HttpPost("/client/link")]
    public async Task<IActionResult> ClientAdd([FromBody] LinksClientDto modelDto)
    {
        //检查验证码
        var code = RedisHelper.Get(modelDto.CodeKey);
        if (code == null)
        {
            throw Oops.Oh("验证码已过期，请重试");
        }
        if (code.ToString()?.ToLower() != modelDto.CodeValue?.ToLower())
        {
            throw Oops.Oh("验证码有误，请重试");
        }
        //验证完毕，删除验证码
        RedisHelper.Del(modelDto.CodeKey);
        //映射成实体
        var model = modelDto.Adapt<LinksEntity>();
        //写入数据库
        await _linkService.InsertReturnEntityAsync(model);
        //映射成DTO再返回，否则出错
        var result = model.Adapt<LinksDto>();
        return Ok(result);
    }
    #endregion
}