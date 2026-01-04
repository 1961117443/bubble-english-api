using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using QT.CMS.Entitys;
using QT.CMS.Entitys.Dto.Article;
using QT.CMS.Entitys.Dto.Base;
using QT.CMS.Entitys.Dto.Parameter;
using QT.Common.Const;
using QT.Common.Core.Filter;
using QT.Common.Core.Manager;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.FriendlyException;
using QT.JsonSerialization;
using SqlSugar;

namespace QT.CMS;

/// <summary>
/// 商品标签
/// </summary>
[Route("api/cms/admin/article/label")]
[ApiController]
[ApiDescriptionSettings(ModuleConst.CMS, Tag = "admin", Name = "label", Order = 200)]
public class ArticleLabelController : ControllerBase
{
    private readonly ISqlSugarRepository<ArticleLabel> _articleLabelService;
    private readonly IUserService _userService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public ArticleLabelController(ISqlSugarRepository<ArticleLabel> articleLabelService, IUserService userService)
    {
        _articleLabelService = articleLabelService;
        _userService = userService;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 根据ID获取数据
    /// 示例：/admin/article/label/1/1
    /// </summary>
    [HttpGet("{channelId}/{id}")]
    [Authorize]
    //[AuthorizeFilter("ArticleLabel", ActionType.View, "channelId")]
    public async Task<IActionResult> GetById([FromRoute] long id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<ArticleLabelDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var model = await _articleLabelService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        //使用AutoMapper转换成ViewModel，根据字段进行塑形
        var result = model.Adapt<ArticleLabelDto>().ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 获取指定数量列表
    /// 示例：/admin/article/label/view/1/10
    /// </summary>
    [HttpGet("view/{channelId}/{top}")]
    [Authorize]
    //[AuthorizeFilter("ArticleLabel", ActionType.View, "channelId")]
    public async Task<IActionResult> GetList([FromRoute] int top, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ArticleLabelDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ArticleLabelDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据库列表
        var resultFrom = await _articleLabelService.AsQueryable()
            .Where(x => x.Status == searchParam.Status)
            .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.Keyword))
            .OrderBy(searchParam.OrderBy ?? "SortId,Id")
            .AutoTake(top)
            .ToListAsync();
        //var resultFrom = await _articleLabelService.QueryListAsync<ArticleLabel>(top,
        //    x => x.Status == searchParam.Status && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "SortId,Id");

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<ArticleLabelDto>>(); //.ShapeData(searchParam.Fields);
        //返回成功200
        return Ok(resultDto);
    }

    /// <summary>
    /// 获取分页列表
    /// 示例：/admin/article/label/1?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet("{channelId}")]
    [Authorize]
    //[AuthorizeFilter("ArticleLabel", ActionType.View, "channelId")]
    public async Task<IActionResult> GetList([FromQuery] BaseParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ArticleLabelDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ArticleLabelDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表，如果ID大于0则查询该用户下所有的列表
        var list = await _articleLabelService.AsQueryable()
            .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(),x=>x.Title.Contains(searchParam.Keyword))
            .OrderBy(searchParam.OrderBy ?? "SortId,Id")
            .ToPagedListAsync(pageParam.PageIndex,pageParam.PageSize);
        //var list = await _articleLabelService.QueryPageAsync<ArticleLabel>(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "SortId,Id");

        //x-pagination
        //var paginationMetadata = new
        //{
        //    totalCount = list.pagination.Total,
        //    pageSize = list.pagination.PageSize,
        //    pageIndex = list.pagination.PageIndex,
        //    totalPages = 0
        //};
        //App.HttpContext.Response.Headers.Add("x-pagination", JSON.Serialize(paginationMetadata));

        //映射成DTO，根据字段进行塑形
        //var resultDto = list.Adapt<IEnumerable<ArticleLabelDto>>().ShapeData(searchParam.Fields);
        return Ok(PageResult<ArticleLabelDto>.SqlSugarPageResult(list));
    }

    /// <summary>
    /// 添加一条记录
    /// 示例：/admin/article/label/1
    /// </summary>
    [HttpPost("{channelId}")]
    [Authorize]
    //[AuthorizeFilter("ArticleLabel", ActionType.Add, "channelId")]
    public async Task<IActionResult> Add([FromBody] ArticleLabelEditDto modelDto)
    {
        //映射成实体
        var model = modelDto.Adapt<ArticleLabel>();
        //获取当前用户名
        model.AddBy = await _userService.GetUserNameAsync();
        model.AddTime = DateTime.Now;
        //写入数据库
        var mapModel = await _articleLabelService.InsertAsync(model);
        //映射成DTO再返回，否则出错
        var result = mapModel.Adapt<ArticleLabelDto>();
        return Ok(result);
    }

    /// <summary>
    /// 修改一条记录
    /// 示例：/admin/article/label/1/1
    /// </summary>
    [HttpPut("{channelId}/{id}")]
    [Authorize]
    //[AuthorizeFilter("ArticleLabel", ActionType.Edit, "channelId")]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] ArticleLabelEditDto modelDto)
    {
        //查找记录
        var model = await _articleLabelService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        _articleLabelService.Context.Tracking(model);

        //更新操作AutoMapper替我们完成，只需要调用保存即可
        modelDto.Adapt(model);

        //获取当前用户名
        model.UpdateBy = await _userService.GetUserNameAsync();
        model.UpdateTime = DateTime.Now;
        var result = await _articleLabelService.UpdateAsync(model);
        return NoContent();
    }

    /// <summary>
    /// 局部更新一条记录
    /// 示例：/admin/article/label/1/1
    /// Body：[{"op":"replace","path":"/title","value":"new title"}]
    /// </summary>
    [HttpPatch("{channelId}/{id}")]
    [Authorize]
    //[AuthorizeFilter("ArticleLabel", ActionType.Edit, "channelId")]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] JsonPatchDocument<ArticleLabelEditDto> patchDocument)
    {
        var model = await _articleLabelService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }

        var modelToPatch = model.Adapt<ArticleLabelEditDto>();
        patchDocument.ApplyTo(modelToPatch, ModelState);
        //验证数据是否合法
        if (!TryValidateModel(modelToPatch))
        {
            return ValidationProblem(ModelState);
        }
        _articleLabelService.Context.Tracking(model);
        //更新操作AutoMapper替我们完成，只需要调用保存即可
        modelToPatch.Adapt(model);
        await _articleLabelService.UpdateAsync(model);

        return NoContent();
    }

    /// <summary>
    /// 删除一条记录
    /// 示例：/admin/article/label/1/1
    /// </summary>
    [HttpDelete("{channelId}/{id}")]
    [Authorize]
    //[AuthorizeFilter("ArticleLabel", ActionType.Delete, "channelId")]
    public async Task<IActionResult> Delete([FromRoute] long id)
    {
        if (!await _articleLabelService.AnyAsync(x => x.Id == id))
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        var result = await _articleLabelService.DeleteAsync(x => x.Id == id);

        return NoContent();
    }

    /// <summary>
    /// 批量删除记录(级联数据)
    /// 示例：/admin/article/label/1?ids=1,2,3
    /// </summary>
    [HttpDelete("{channelId}")]
    [Authorize]
    //[AuthorizeFilter("ArticleLabel", ActionType.Delete, "channelId")]
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
        await _articleLabelService.DeleteAsync(x => arrIds.Contains(x.Id));

        return NoContent();
    }
    #endregion

    #region 前台调用接口============================
    /// <summary>
    /// 获取指定数量列表
    /// 示例：/client/article/label/view/10
    /// </summary>
    [HttpGet("/client/article/label/view/{top}")]
    [CachingFilter]
    [AllowAnonymous,NonUnify]
    public async Task<IActionResult> ClientGetList([FromRoute] int top, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ArticleLabelDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ArticleLabelDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        if (top == 0)
        {
            top = 999;
        }

        //获取数据库列表
        var resultFrom = await _articleLabelService.AsQueryable()
            .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.Keyword))
            .OrderBy(searchParam.OrderBy ?? "SortId,Id")
            .AutoTake(top)
            .ToListAsync();
        //var resultFrom = await _articleLabelService.QueryListAsync<ArticleLabel>(top,
        //    x => !searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword)),
        //    searchParam.OrderBy ?? "SortId,Id");

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<ArticleLabelDto>>(); //.ShapeData(searchParam.Fields);
        //返回成功200
        return Ok(resultDto);
    }
    #endregion

}