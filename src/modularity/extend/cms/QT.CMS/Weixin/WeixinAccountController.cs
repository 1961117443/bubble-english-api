using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using QT.CMS.Entitys;
using QT.CMS.Entitys.Dto.Application;
using QT.CMS.Entitys.Dto.Parameter;
using QT.CMS.Entitys.Dto.Weixin;
using QT.Common.Core.Manager;
using QT.Common.Extension;
using QT.FriendlyException;
using QT.JsonSerialization;
using SqlSugar;

namespace QT.CMS;

/// <summary>
/// 微信公众号
/// </summary>
[Route("api/cms/admin/weixin/account")]
[ApiController]
public class WeixinAccountController : ControllerBase
{
    private readonly ISqlSugarRepository<WxAccount> _accountService;
    private readonly IUserService _userService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public WeixinAccountController(ISqlSugarRepository<WxAccount> accountService, IUserService userService)
    {
        _accountService = accountService;
        _userService = userService;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 根据ID获取数据
    /// 示例：/admin/weixin/account/1
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    //[AuthorizeFilter("WeixinAccount", ActionType.View)]
    public async Task<IActionResult> GetById([FromRoute] int id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<WxAccountDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var model = await _accountService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据{id}不存在或已删除");
        }
        //使用AutoMapper转换成ViewModel，根据字段进行塑形
        var result = model.Adapt<WxAccountDto>().ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 获取指定数量列表
    /// 示例：/admin/weixin/account/view/10
    /// </summary>
    [HttpGet("view/{top}")]
    [Authorize]
    //[AuthorizeFilter("WeixinAccount", ActionType.View)]
    public async Task<IActionResult> GetList([FromRoute] int top, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<WxAccountDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<WxAccountDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据库列表
        var resultFrom = await _accountService.AsQueryable()
            .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.Keyword))
            .OrderBy(searchParam.OrderBy ?? "SortId,-Id")
            .AutoTake(top)
            .ToListAsync();
        //var resultFrom = await _accountService.QueryListAsync<WxAccount>(top,
        //    x => (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "SortId,-Id");

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<WxAccountDto>>();//.ShapeData(searchParam.Fields);
        //返回成功200
        return Ok(resultDto);
    }

    /// <summary>
    /// 获取分页列表
    /// 示例：/admin/weixin/account?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet]
    [Authorize]
    //[AuthorizeFilter("WeixinAccount", ActionType.View)]
    public async Task<IActionResult> GetList([FromQuery] BaseParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<WxAccountDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<WxAccountDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表，如果ID大于0则查询该用户下所有的列表
        var list = await _accountService.AsQueryable()
            .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.Keyword))
            .OrderBy(searchParam.OrderBy ?? "SortId,-Id")
            .ToPagedListAsync(pageParam.PageIndex, pageParam.PageSize);


        return Ok(PageResult<WxAccountDto>.SqlSugarPageResult(list));
        //var list = await _accountService.QueryPageAsync<WxAccount>(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "SortId,-Id");

        //x-pagination
       // var paginationMetadata = new
       // {
       //     totalCount = list.pagination.Total,
       //     pageSize = list.pagination.PageSize,
       //     pageIndex = list.pagination.PageIndex,
       //     totalPages = 0
       // };
       //App.HttpContext.Response.Headers.Add("x-pagination", JSON.Serialize(paginationMetadata));

       // //映射成DTO，根据字段进行塑形
       // var resultDto = list.Adapt<IEnumerable<WxAccountDto>>().ShapeData(searchParam.Fields);
       // return Ok(resultDto);
    }

    /// <summary>
    /// 添加一条记录
    /// 示例：/admin/weixin/account
    /// </summary>
    [HttpPost]
    [Authorize]
    //[AuthorizeFilter("WeixinAccount", ActionType.Add)]
    public async Task<IActionResult> Add([FromBody] WxAccountEditDto modelDto)
    {
        //映射成实体
        var model = modelDto.Adapt<WxAccount>();
        //获取当前用户名
        model.AddBy = await _userService.GetUserNameAsync();
        model.AddTime = DateTime.Now;
        //写入数据库
        var mapModel = await _accountService.InsertAsync(model);
        //映射成DTO再返回，否则出错
        var result = mapModel.Adapt<WxAccountDto>();
        return Ok(result);
    }

    /// <summary>
    /// 修改一条记录
    /// 示例：/admin/weixin/account/1
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    //[AuthorizeFilter("WeixinAccount", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] WxAccountEditDto modelDto)
    {
        //查找记录
        var model = await _accountService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        _accountService.Context.Tracking(model);
        modelDto.Adapt(model);
        //获取当前用户名
        model.UpdateBy = await _userService.GetUserNameAsync();
        model.UpdateTime = DateTime.Now;
        //调用保存即可
        var result = await _accountService.UpdateAsync(model);
        return NoContent();
    }

    /// <summary>
    /// 局部更新一条记录
    /// 示例：/admin/weixin/account/1
    /// Body：[{"op":"replace","path":"/title","value":"new title"}]
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize]
    //[AuthorizeFilter("WeixinAccount", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] JsonPatchDocument<WxAccountEditDto> patchDocument)
    {
        var model = await _accountService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据{id}不存在或已删除");
        }

        var modelToPatch = model.Adapt<WxAccountEditDto>();
        patchDocument.ApplyTo(modelToPatch, ModelState);
        //验证数据是否合法
        if (!TryValidateModel(modelToPatch))
        {
            return ValidationProblem(ModelState);
        }
        _accountService.Context.Tracking(model);
        //更新操作AutoMapper替我们完成，只需要调用保存即可
        modelToPatch.Adapt(model);
        await _accountService.UpdateAsync(model);

        return NoContent();
    }

    /// <summary>
    /// 删除一条记录
    /// 示例：/admin/weixin/account/1
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    //[AuthorizeFilter("WeixinAccount", ActionType.Delete)]
    public async Task<IActionResult> Delete([FromRoute] long id)
    {
        if (!await _accountService.AnyAsync(x => x.Id == id))
        {
            throw Oops.Oh($"数据{id}不存在或已删除");
        }
        var result = await _accountService.DeleteAsync(x => x.Id == id);

        return NoContent();
    }

    /// <summary>
    /// 批量删除记录(级联数据)
    /// 示例：/admin/weixin/account?ids=1,2,3
    /// </summary>
    [HttpDelete]
    [Authorize]
    //[AuthorizeFilter("WeixinAccount", ActionType.Delete)]
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
        await _accountService.DeleteAsync(x => arrIds.Contains(x.Id));

        return NoContent();
    }
    #endregion
}
