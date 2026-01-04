using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using QT.CMS.Entitys;
using QT.CMS.Entitys.Dto.Base;
using QT.CMS.Entitys.Dto.Parameter;
using QT.CMS.Entitys.Dto.Shop;
using QT.Common.Core.Emum;
using QT.Common.Core.Filter;
using QT.Common.Core.Filters;
using QT.Common.Core.Manager;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.FriendlyException;
using SqlSugar;

namespace QT.CMS;

/// <summary>
/// 商品品牌
/// </summary>
[Route("api/cms/admin/shop/brand")]
[ApiController]
public class ShopBrandController : ControllerBase
{
    private readonly ISqlSugarRepository<ShopBrand> _shopBrandService;
    private readonly IUserService _userService;

    /// <summary>
    /// 依赖注入
    /// </summary>

    public ShopBrandController(ISqlSugarRepository<ShopBrand> shopBrandService, IUserService userService)
    {
        _shopBrandService = shopBrandService;
        _userService = userService;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 根据ID获取数据
    /// 示例：/admin/shop/brand/1
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopBrand", ActionType.View)]
    public async Task<IActionResult> GetById([FromRoute] long id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<ShopBrandDto>())
        {
            throw Oops.Oh("请输入正确的塑性参数");
        }
        //查询数据库获取实体
        var model = await _shopBrandService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        //使用AutoMapper转换成ViewModel，根据字段进行塑形
        var result = model.Adapt<ShopBrandDto>().ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 获取指定数量列表
    /// 示例：/admin/shop/brand/view/10
    /// </summary>
    [HttpGet("view/{top}")]
    [Authorize]
    [AuthorizeFilter("ShopBrand", ActionType.View)]
    public async Task<IEnumerable<ShopBrandDto>> GetList([FromRoute] int top, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ShopBrandDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ShopBrandDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据库列表
        var resultFrom = await _shopBrandService.AsQueryable()
            .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.Keyword))
            .OrderBy(searchParam.OrderBy ?? "SortId,-Id")
            .AutoTake(top)
            .ToListAsync();

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<ShopBrandDto>>(); //.ShapeData(searchParam.Fields);
        //返回成功200
        return (resultDto);
    }

    /// <summary>
    /// 获取分页列表
    /// 示例：/admin/shop/brand?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet]
    [Authorize]
    [AuthorizeFilter("ShopBrand", ActionType.View)]
    public async Task<dynamic> GetList([FromQuery] BaseParameter searchParam/*, [FromQuery] PageParamater pageParam*/)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ShopBrandDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ShopBrandDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表，如果ID大于0则查询该用户下所有的列表
        var list = await _shopBrandService.AsQueryable()
            .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.keyword))
            .OrderBy(searchParam.OrderBy ?? "SortId,-Id")
            .ToPagedListAsync(searchParam.currentPage, searchParam.pageSize);


        ////x-pagination
        //var paginationMetadata = new
        //{
        //    totalCount = list.TotalCount,
        //    pageSize = list.PageSize,
        //    pageIndex = list.PageIndex,
        //    totalPages = list.TotalPages
        //};
        //Response.Headers.Add("x-pagination", SerializeHelper.SerializeObject(paginationMetadata));

        //映射成DTO，根据字段进行塑形
        //var resultDto = list.Adapt<IEnumerable<ShopBrandDto>>().ShapeData(searchParam.Fields);

        var resultDto = PageResult<ShopBrandDto>.SqlSugarPageResult(list);
        return (resultDto);
    }

    /// <summary>
    /// 添加一条记录
    /// 示例：/admin/shop/brand
    /// </summary>
    [HttpPost]
    [Authorize]
    [AuthorizeFilter("ShopBrand", ActionType.Add)]
    public async Task<ShopBrandDto> Add([FromBody] ShopBrandEditDto modelDto)
    {
        //映射成实体
        var model = modelDto.Adapt<ShopBrand>();
        //获取当前用户名
        model.AddBy = await _userService.GetUserNameAsync();
        model.AddTime = DateTime.Now;
        //写入数据库
        var mapModel = await _shopBrandService.InsertReturnIdentityAsync(model);
        //映射成DTO再返回，否则出错
        var result = model.Adapt<ShopBrandDto>();
        return (result);
    }

    /// <summary>
    /// 修改一条记录
    /// 示例：/admin/shop/brand/1
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopBrand", ActionType.Edit)]
    public async Task Update([FromRoute] long id, [FromBody] ShopBrandEditDto modelDto)
    {
        //查找记录
        var model = await _shopBrandService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        _shopBrandService.Context.Tracking(model);
        modelDto.Adapt(model);
        //获取当前用户名
        model.UpdateBy = await _userService.GetUserNameAsync();
        model.UpdateTime = DateTime.Now;
        await _shopBrandService.UpdateAsync(model);
    }

    /// <summary>
    /// 局部更新一条记录
    /// 示例：/admin/shop/brand/1
    /// Body：[{"op":"replace","path":"/title","value":"new title"}]
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopBrand", ActionType.Edit)]
    public async Task Update([FromRoute] long id, [FromBody] JsonPatchDocument<ShopBrandEditDto> patchDocument)
    {
        var model = await _shopBrandService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }

        var modelToPatch = model.Adapt<ShopBrandEditDto>();
        patchDocument.ApplyTo(modelToPatch, ModelState);
        //验证数据是否合法
        if (!TryValidateModel(modelToPatch))
        {
            throw Oops.Oh("验证数据是否合法");
            //return ValidationProblem(ModelState);
        }
        _shopBrandService.Context.Tracking(model);
        //更新操作AutoMapper替我们完成，只需要调用保存即可
        modelToPatch.Adapt(model);
        await _shopBrandService.AutoUpdateAsync(model);
    }

    /// <summary>
    /// 删除一条记录
    /// 示例：/admin/shop/brand/1
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopBrand", ActionType.Delete)]
    public async Task Delete([FromRoute] long id)
    {
        if (!await _shopBrandService.AnyAsync(x => x.Id == id))
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        var result = await _shopBrandService.DeleteAsync(id);
    }

    /// <summary>
    /// 批量删除记录(级联数据)
    /// 示例：/admin/shop/brand?ids=1,2,3
    /// </summary>
    [HttpDelete]
    [Authorize]
    [AuthorizeFilter("ShopBrand", ActionType.Delete)]
    public async Task DeleteByIds([FromQuery] string Ids)
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
        await _shopBrandService.DeleteAsync(x => arrIds.Contains(x.Id));
    }
    #endregion

    #region 前台调用接口============================
    /// <summary>
    /// 根据ID获取数据
    /// 示例：/client/shop/brand/1
    /// </summary>
    [HttpGet("/client/shop/brand/{id}")]
    [CachingFilter]
    [NonUnify]
    public async Task<IActionResult> ClientGetById([FromRoute] long id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<ShopBrandDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var model = await _shopBrandService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        //使用AutoMapper转换成ViewModel，根据字段进行塑形
        var result = model.Adapt<ShopBrandDto>();//.ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 获取指定数量列表
    /// 示例：/client/shop/brand/view/10
    /// </summary>
    [HttpGet("/client/shop/brand/view/{top}")]
    [CachingFilter]
    [NonUnify]
    public async Task<IActionResult> ClientGetList([FromRoute] int top, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ShopBrandDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ShopBrandDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据库列表
        var resultFrom = await _shopBrandService.AsQueryable()
            .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.keyword))
            .OrderBy(searchParam.OrderBy ?? "SortId,-Id")
            .AutoTake(top)
            .ToListAsync();

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<ShopBrandDto>>(); //.ShapeData(searchParam.Fields);
        //返回成功200
        return Ok(resultDto);
    }

    /// <summary>
    /// 获取分页列表
    /// 示例：/client/shop/brand?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet("/client/shop/brand")]
    [CachingFilter]
    [NonUnify]
    public async Task<dynamic> ClientGetList([FromQuery] BaseParameter searchParam/*, [FromQuery] PageParamater pageParam*/)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ShopBrandDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ShopBrandDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表，如果ID大于0则查询该用户下所有的列表
        var list = await _shopBrandService.AsQueryable()
            .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.keyword))
            .OrderBy(searchParam.OrderBy ?? "SortId,-Id")
            .ToPagedListAsync(searchParam.currentPage,searchParam.pageSize);

        return PageResult<ShopBrandDto>.SqlSugarPageResult(list);

        ////x-pagination
        //var paginationMetadata = new
        //{
        //    totalCount = list.TotalCount,
        //    pageSize = list.PageSize,
        //    pageIndex = list.PageIndex,
        //    totalPages = list.TotalPages
        //};
        //Response.Headers.Add("x-pagination", SerializeHelper.SerializeObject(paginationMetadata));

        //映射成DTO，根据字段进行塑形
        //var resultDto = list.Adapt<IEnumerable<ShopBrandDto>>().ShapeData(searchParam.Fields);
        //return Ok(resultDto);
    }
    #endregion
}