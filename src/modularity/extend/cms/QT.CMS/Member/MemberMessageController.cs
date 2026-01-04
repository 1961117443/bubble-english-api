using QT.CMS.Entitys.Dto.Member;
using QT.Systems.Entitys.Permission;
using SqlSugar;

namespace QT.CMS;

/// <summary>
/// 会员站内消息
/// </summary>
[Route("api/cms/admin/member/message")]
[ApiController]
public class MemberMessageController : ControllerBase
{
    private readonly ISqlSugarRepository<MemberMessage> _memberMessageService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public MemberMessageController(ISqlSugarRepository<MemberMessage> memberMessageService)
    {
        _memberMessageService = memberMessageService;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 根据ID获取数据
    /// 示例：/admin/member/message/1
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    [AuthorizeFilter("MemberMessage", ActionType.View)]
    public async Task<IActionResult> GetById([FromRoute] long id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<MemberMessageDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var resultFrom = await _memberMessageService.AsQueryable()
            .InnerJoin<UserEntity>((a, u) => a.UserId == u.Id)
            .Where((a,u)=> a.Id == id)
            .Select((a, u) => new MemberMessageDto
            {
                id = SqlFunc.ToString(a.Id),
                userId = a.UserId,
                userName = u.Account,
                title = a.Title,
                content = a.Content,
                addTime = a.AddTime,
                isRead = a.IsRead,
                readTime = a.ReadTime
            })
            .FirstAsync();
        if (resultFrom == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        //根据字段进行塑形
        var result = resultFrom.ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 获取指定数量列表
    /// 示例：/admin/member/message/view/0
    /// </summary>
    [HttpGet("view/{top}")]
    [Authorize]
    public async Task<IActionResult> GetList([FromRoute] int top, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<MemberMessageDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<MemberMessageDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据库列表
        var result = await _memberMessageService.AsQueryable()
            .InnerJoin<UserEntity>((a, u) => a.UserId == u.Id)
            .Select((a, u) => new MemberMessageDto
            {
                id = SqlFunc.ToString(a.Id),
                userId = a.UserId,
                userName = u.Account,
                title = a.Title,
                content = a.Content,
                addTime = a.AddTime,
                isRead = a.IsRead,
                readTime = a.ReadTime
            }).MergeTable()
            .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.userName.Contains(searchParam.keyword))
            .OrderBy(searchParam.OrderBy ?? "-AddTime,-Id")
            .AutoTake(top)
            .ToListAsync();
        //根据字段进行塑形
        //var result = resultFrom.ShapeData(searchParam.Fields);
        //返回成功200
        return Ok(result);
    }

    /// <summary>
    /// 获取分页列表
    /// 示例：/admin/member/message?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet]
    [Authorize]
    [AuthorizeFilter("MemberMessage", ActionType.View)]
    public async Task<dynamic> GetList([FromQuery] BaseParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<MemberMessageDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<MemberMessageDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表，如果ID大于0则查询该用户下所有的列表
        var list = await _memberMessageService.AsQueryable()
            .InnerJoin<UserEntity>((a, u) => a.UserId == u.Id)
            .Select((a, u) => new MemberMessageDto
            {
                id = SqlFunc.ToString(a.Id),
                userId = a.UserId,
                userName = u.Account,
                title = a.Title,
                content = a.Content,
                addTime = a.AddTime,
                isRead = a.IsRead,
                readTime = a.ReadTime
            }).MergeTable()
           .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.userName.Contains(searchParam.keyword))
           .OrderBy(searchParam.OrderBy ?? "-AddTime,-Id")
            .ToPagedListAsync(searchParam.currentPage, searchParam.pageSize);


        return PageResult<MemberMessageDto>.SqlSugarPageResult(list);
    }

    /// <summary>
    /// 添加一条记录
    /// 示例：/admin/member/message
    /// </summary>
    [HttpPost]
    [Authorize]
    [AuthorizeFilter("MemberMessage", ActionType.Add)]
    public async Task<IActionResult> Add([FromBody] MemberMessageEditDto modelDto)
    {
        //检查会员是否存在
        if (!await _memberMessageService.Context.Queryable<Members>().AnyAsync(x => x.UserId == modelDto.userId))
        {
            throw Oops.Oh($"会员ID[{modelDto.userId}]不存在");
        }
        //映射成实体
        var model = modelDto.Adapt<MemberMessage>();
        //写入数据库
        await _memberMessageService.InsertReturnEntityAsync(model);
        //查询刚添加的记录
        //var result = await _memberMessageService.QueryAsync(x => x.Id == model.Id);
        var result = await _memberMessageService.AsQueryable()
            .InnerJoin<UserEntity>((a, u) => a.UserId == u.Id)
            .Where((a, u) => a.Id == model.Id)
            .Select((a, u) => new MemberMessageDto
            {
                id = SqlFunc.ToString(a.Id),
                userId = a.UserId,
                userName = u.Account,
                title = a.Title,
                content = a.Content,
                addTime = a.AddTime,
                isRead = a.IsRead,
                readTime = a.ReadTime
            })
            .FirstAsync();
        return Ok(result);
    }

    /// <summary>
    /// 修改一条记录
    /// 示例：/admin/member/message/1
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [AuthorizeFilter("MemberMessage", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] MemberMessageEditDto modelDto)
    {
        //查找记录
        var model = await _memberMessageService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据{id}不存在或已删除");
        }
        _memberMessageService.Context.Tracking(model);
        //因为多次进行查询，所以手动调用Update方法保存
        modelDto.Adapt(model);
        var result = await _memberMessageService.AutoUpdateAsync(model);
        return NoContent();
    }

    /// <summary>
    /// 局部更新一条记录
    /// 示例：/admin/member/message/1
    /// Body：[{"op":"replace","path":"/title","value":"new title"}]
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize]
    [AuthorizeFilter("MemberMessage", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] JsonPatchDocument<MemberMessageEditDto> patchDocument)
    {
        //检查记录是否存在
        var model = await _memberMessageService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }

        var modelToPatch = model.Adapt<MemberMessageEditDto>();
        patchDocument.ApplyTo(modelToPatch, ModelState);
        //验证数据是否合法
        if (!TryValidateModel(modelToPatch))
        {
            return ValidationProblem(ModelState);
        }
        _memberMessageService.Context.Tracking(model);
        //更新操作AutoMapper替我们完成，只需要调用保存即可
        modelToPatch.Adapt(model);
        await _memberMessageService.AutoUpdateAsync(model);
        return NoContent();
    }

    /// <summary>
    /// 删除一条记录
    /// 示例：/admin/member/message/1
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [AuthorizeFilter("MemberMessage", ActionType.Delete)]
    public async Task<IActionResult> Delete([FromRoute] long id)
    {
        //检查参数是否正确
        if (!await _memberMessageService.AnyAsync(x => x.Id == id))
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        var result = await _memberMessageService.DeleteAsync(x => x.Id == id);
        return NoContent();
    }

    /// <summary>
    /// 批量删除记录
    /// 示例：/admin/member/message?ids=1,2,3
    /// </summary>
    [HttpDelete]
    [Authorize]
    [AuthorizeFilter("MemberMessage", ActionType.Delete)]
    public async Task<IActionResult> DeleteByIds([FromQuery] string Ids)
    {
        //检查参数是否为空
        if (Ids == null)
        {
            throw Oops.Oh("传输参数不可为空");
        }
        //将ID列表转换成IEnumerable
        var listIds = Ids.ToIEnumerable<long>();
        if (listIds == null)
        {
            throw Oops.Oh("传输参数不符合规范");
        }
        //执行批量删除操作
        await _memberMessageService.DeleteAsync(x => listIds.Contains(x.Id));
        return NoContent();
    }
    #endregion
}