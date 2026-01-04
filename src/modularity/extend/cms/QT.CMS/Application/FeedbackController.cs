using Microsoft.ClearScript.JavaScript;
using QT.CMS.Entitys.Dto.Application;

namespace QT.CMS;

/// <summary>
/// 在线留言
/// </summary>
[Route("/api/cms/admin/feedback")]
[ApiController]
public class FeedbackController : ControllerBase
{
    private readonly ISqlSugarRepository<FeedbackEntity> _feedbackService;
    private readonly IUserService _userService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public FeedbackController(ISqlSugarRepository<FeedbackEntity> feedbackService, IUserService userService)
    {
        _feedbackService = feedbackService;
        _userService = userService;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 根据ID获取数据
    /// 示例：/admin/feedback/1
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    [AuthorizeFilter("Feedback", ActionType.View)]
    public async Task<IActionResult> GetById([FromRoute] int id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<FeedbackDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var model = await _feedbackService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据{id}不存在或已删除");
        }
        //使用AutoMapper转换成ViewModel
        //根据字段进行塑形
        var result = model.Adapt<FeedbackDto>();//.ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 获取留言列表
    /// 示例：/admin/feedback?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet]
    [Authorize]
    [AuthorizeFilter("Feedback", ActionType.View)]
    public async Task<IActionResult> GetList([FromQuery] BaseParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null && !searchParam.OrderBy.TrimStart('-').IsPropertyExists<FeedbackDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<FeedbackDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表
        var list = await _feedbackService.AsQueryable()
            .WhereIF(searchParam.keyword.IsNotEmptyOrNull(),x=>x.Content.Contains(searchParam.keyword))
            .OrderBy(searchParam.OrderBy ?? "-Id")
            .ToPagedListAsync(pageParam.PageIndex,pageParam.PageSize);

        return Ok(PageResult<FeedbackDto>.SqlSugarPageResult(list));

        //var list = await _feedbackService.QueryPageAsync<Feedback>(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Content != null && x.Content.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "-Id");

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
        //var resultDto = _mapper.Map<IEnumerable<FeedbackDto>>(list).ShapeData(searchParam.Fields);
        //return Ok(resultDto);
    }

    /// <summary>
    /// 添加一条记录
    /// 示例：/admin/feedback
    /// </summary>
    [HttpPost]
    [Authorize]
    [AuthorizeFilter("Feedback", ActionType.Add)]
    public async Task<IActionResult> Add([FromBody] FeedbackEditDto modelDto)
    {
        //映射成实体
        var model = modelDto.Adapt<FeedbackEntity>();
        //获取当前用户名
        model.AddBy = await _userService.GetUserNameAsync();
        model.AddTime = DateTime.Now;
        //写入数据库
        await _feedbackService.InsertReturnEntityAsync(model);
        //映射成DTO再返回，否则出错
        var result = model.Adapt<FeedbackDto>();
        return Ok(result);
    }

    /// <summary>
    /// 修改一条记录
    /// 示例：/admin/feedback/1
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [AuthorizeFilter("Feedback", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] FeedbackEditDto modelDto)
    {
        //查找记录
        var model = await _feedbackService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"记录[{id}]不存在或已删除");
        }
        //更新操作AutoMapper替我们完成，只需要调用保存即可
        _feedbackService.Context.Tracking(model);
        modelDto.Adapt(model);
        //如果已答复,则更新答复时间
        if (model.ReplyContent != null)
        {
            model.ReplyBy = await _userService.GetUserNameAsync();
            model.ReplyTime = DateTime.Now;
        }
        var result = await _feedbackService.AutoUpdateAsync(model);
        return NoContent();
    }

    /// <summary>
    /// 局部更新一条记录
    /// 示例：/admin/feedback/1
    /// Body：[{"op":"replace","path":"/title","value":"new title"}]
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize]
    [AuthorizeFilter("Feedback", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] JsonPatchDocument<FeedbackEditDto> patchDocument)
    {
        var model = await _feedbackService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据{id}不存在或已删除");
        }

        var modelToPatch = model.Adapt<FeedbackEditDto>();
        patchDocument.ApplyTo(modelToPatch, ModelState);
        //验证数据是否合法
        if (!TryValidateModel(modelToPatch))
        {
            return ValidationProblem(ModelState);
        }
        _feedbackService.Context.Tracking(model);
        //更新操作AutoMapper替我们完成，只需要调用保存即可
        modelToPatch.Adapt(model);
        await _feedbackService.AutoUpdateAsync(model);

        return NoContent();
    }

    /// <summary>
    /// 批量审核记录
    /// 示例：/admin/feedback?ids=1,2,3
    /// </summary>
    [HttpPut]
    [Authorize]
    [AuthorizeFilter("Feedback", ActionType.Audit)]
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
        var list = await _feedbackService.AsQueryable().Where(x => x.Status == 0 && arrIds.Contains(x.Id)).Take(1000).ToListAsync();
        //var list = await _feedbackService.QueryListAsync<Feedback>(1000, x => x.Status == 0 && arrIds.Contains(x.Id), WriteRoRead.Write);
        if (list == null || list.Count() == 0)
        {
            throw Oops.Oh("暂无需要审核的记录");
        }
        foreach (var item in list)
        {
            item.Status = 1;
        }
        //保存到数据库
        await _feedbackService.Context.Updateable<FeedbackEntity>(list).UpdateColumns(x => x.Status).ExecuteCommandAsync();

        return NoContent();
    }

    /// <summary>
    /// 删除一条记录
    /// 示例：/admin/feedback/1
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [AuthorizeFilter("Feedback", ActionType.Delete)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        if (!await _feedbackService.AnyAsync(x => x.Id == id))
        {
            throw Oops.Oh($"记录[{id}]不存在或已删除");
        }
        var result = await _feedbackService.DeleteAsync(x => x.Id == id);

        return NoContent();
    }

    /// <summary>
    /// 批量删除记录(级联数据)
    /// 示例：/admin/feedback?ids=1,2,3
    /// </summary>
    [HttpDelete]
    [Authorize]
    [AuthorizeFilter("Feedback", ActionType.Delete)]
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
        await _feedbackService.DeleteAsync(x => ids.Contains(x.Id));

        return NoContent();
    }
    #endregion

    #region 前台调用接口============================
    /// <summary>
    /// 获取指定数量列表
    /// 示例：/client/feedback/view/10
    /// </summary>
    [HttpGet("/client/feedback/view/{top}")]
    [CachingFilter]
    public async Task<IActionResult> ClientGetList([FromRoute] int top, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<FeedbackDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<FeedbackDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据库列表
        var resultFrom = await _feedbackService.AsQueryable()
            .Where(x=>x.Status == 1)
            .WhereIF(searchParam.SiteId >= 0, x => x.SiteId == searchParam.SiteId)
            .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.Content.Contains(searchParam.Keyword))
            .OrderBy(searchParam.OrderBy ?? "-Id")
            .AutoTake(top)
            .ToListAsync();


        //var resultFrom = await _feedbackService.QueryListAsync<Feedback>(top,
        //    x => x.Status == 1
        //    && (searchParam.SiteId < 0 || x.SiteId == searchParam.SiteId)
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Content != null && x.Content.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "-Id");

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<FeedbackDto>>(); //.ShapeData(searchParam.Fields);
        //返回成功200
        return Ok(resultDto);
    }

    /// <summary>
    /// 获取分页列表
    /// 示例：/client/feedback?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet("/client/feedback")]
    [CachingFilter]
    public async Task<IActionResult> ClientGetList([FromQuery] BaseParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<FeedbackDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<FeedbackDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取所有的列表
        var list = await _feedbackService.AsQueryable()
            .Where(x => x.Status == 1)
            .WhereIF(searchParam.SiteId >= 0, x => x.SiteId == searchParam.SiteId)
            .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.Content.Contains(searchParam.Keyword))
            .OrderBy(searchParam.OrderBy ?? "-Id")
             .ToPagedListAsync(pageParam.PageIndex, pageParam.PageSize);

        return Ok(PageResult<FeedbackDto>.SqlSugarPageResult(list));

        //var list = await _feedbackService.QueryPageAsync<Feedback>(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => x.Status == 1
        //    && (searchParam.SiteId < 0 || x.SiteId == searchParam.SiteId)
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Content != null && x.Content.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "-Id");

        //x-pagination
        //var paginationMetadata = new
        //{
        //    totalCount = list.TotalCount,
        //    pageSize = list.PageSize,
        //    pageIndex = list.PageIndex,
        //    totalPages = list.TotalPages
        //};
        //Response.Headers.Add("x-pagination", SerializeHelper.SerializeObject(paginationMetadata));

        ////映射成DTO，根据字段进行塑形
        //var resultDto = _mapper.Map<IEnumerable<FeedbackDto>>(list).ShapeData(searchParam.Fields);
        //return Ok(resultDto);
    }

    /// <summary>
    /// 添加一条记录
    /// 示例：/client/feedback
    /// </summary>
    [HttpPost("/client/feedback")]
    public async Task<IActionResult> ClientAdd([FromBody] FeedbackClientDto modelDto)
    {
        //检查验证码
        var code = await RedisHelper.GetAsync(modelDto.CodeKey);
        if (code == null)
        {
            throw Oops.Oh("验证码已过期，请重试");
        }
        if (code?.ToString()?.ToLower() != modelDto?.CodeValue?.ToLower())
        {
            throw Oops.Oh("验证码有误，请重试");
        }
        //验证完毕，删除验证码
        await RedisHelper.DelAsync(modelDto?.CodeKey);
        //映射成实体
        var model = modelDto.Adapt<FeedbackEntity>();
        //获取当前用户名
        model.AddBy = await _userService.GetUserNameAsync();
        model.AddTime = DateTime.Now;
        //写入数据库
        await _feedbackService.InsertReturnEntityAsync(model);
        //映射成DTO再返回，否则出错
        var result = model.Adapt<FeedbackDto>();
        return Ok(result);
    }
    #endregion
}