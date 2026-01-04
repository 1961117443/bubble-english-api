using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using NPOI.POIFS.Crypt.Dsig;
using NPOI.SS.Formula.Functions;
using QT.CMS.Entitys;
using QT.CMS.Entitys.Dto.Base;
using QT.CMS.Entitys.Dto.Parameter;
using QT.Common.Const;
using QT.Common.Core.Filter;
using QT.Common.Core.Manager;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.DynamicApiController;
using QT.FriendlyException;
using SqlSugar;

namespace QT.CMS;

/// <summary>
/// 站点接口
/// </summary>
//[Route("admin/site")]
[Route("api/cms/admin/[controller]")]
//[ApiController]
[ApiDescriptionSettings(ModuleConst.CMS)]
public class SiteController : IDynamicApiController
{
    private readonly ISqlSugarRepository<Sites> _siteService;
    private readonly IUserService _userService;

    /// <summary>
    /// 构造函数依赖注入
    /// </summary>
    public SiteController(ISqlSugarRepository<Sites> siteService,IUserService userService)
    {
        _siteService = siteService;
        _userService = userService;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 根据ID获取站点
    /// 示例：/admin/site/1
    /// </summary>
    [HttpGet("{siteId}")]
    [Authorize]
    //[AuthorizeFilter("Site", ActionType.View)]
    public async Task<dynamic> GetById([FromRoute] int siteId, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<SitesDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var model = await _siteService.AsQueryable().Includes(x=>x.Domains).SingleAsync(x => x.Id == siteId);
        if (model == null)
        {
            throw Oops.Oh($"数据[{siteId}]不存在或已删除");
        }
        //使用AutoMapper转换成ViewModel
        //根据字段进行塑形
        var result = model.Adapt<SitesDto>().ShapeData(param.Fields);
        return (result);
    }

    /// <summary>
    /// 获取总记录数量
    /// 示例：/admin/site/view/count
    /// </summary>
    [HttpGet("view/count")]
    [Authorize]
    //[AuthorizeFilter("Site", ActionType.View)]
    public async Task<int> GetCount([FromQuery] BaseParameter searchParam)
    {
        var result = await _siteService.CountAsync(x => searchParam.Status < 0 || x.Status == searchParam.Status);
        //返回成功200
        return (result);
    }

    /// <summary>
    /// 获取指定数量列表
    /// 示例：/admin/site/view/0
    /// </summary>
    [HttpGet("view/{top}")]
    [Authorize]
    //[AuthorizeFilter("Site", ActionType.View)]
    public async Task<dynamic> GetList([FromRoute] int top, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<SitesDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<SitesDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        if (top == 0)
        {
            top = 1000;
        }
        //获取数据库列表
        var resultFrom = await _siteService.AsQueryable()
            .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.Keyword))
            .OrderBy(searchParam.OrderBy ?? "SortId,Id")
            .AutoTake(top)
            .ToListAsync();
        //var resultFrom = await _siteService.QueryListAsync<Sites>(top,
        //    x => (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "SortId,Id");

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<SitesDto>>();
        //var resultDto = .ShapeData(searchParam.Fields);
        //返回成功200
        return (resultDto);
    }

    /// <summary>
    /// 获取站点列表
    /// 示例：/admin/site?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet("")]
    [Authorize]
    //[AuthorizeFilter("Site", ActionType.View)]
    public async Task<dynamic> GetList([FromQuery] BaseParameter searchParam/*, [FromQuery] PageInputBase pageParam*/)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null && !searchParam.OrderBy.TrimStart('-').IsPropertyExists<SitesDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<SitesDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表
        var list = await _siteService.AsQueryable()
           .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.keyword))
           .OrderBy(searchParam.OrderBy ?? "SortId,Id")
           .ToPagedListAsync(searchParam.currentPage, searchParam.pageSize);

        //var list = await _siteService.QueryPageAsync(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => (!searchParam.Keyword.IsNotNullOrEmpty() || x.Title != null && x.Title.Contains(searchParam.Keyword)),
        //    searchParam.OrderBy ?? "SortId,Id");

        //x-pagination
        //var paginationMetadata = new
        //{
        //    totalCount = list.pagination.Total,
        //    pageSize = list.pagination.PageSize,
        //    pageIndex = list.pagination.PageIndex,
        //    totalPages = 0
        //};
        //Response.Headers.Add("x-pagination", JSON.Serialize(paginationMetadata));

        //映射成DTO
        //var dto = list.Adapt<SqlSugarPagedList<SitesDto>>();
        //var resultDto = dto.ShapeData(searchParam.Fields);
        var resultDto = PageResult<SitesDto>.SqlSugarPageResult(list);
        //var x = PageResult<SitesDto>.SqlSugarPageResult(dto);
        return (resultDto);
    }

    /// <summary>
    /// 添加一条记录
    /// 示例：/admin/site/
    /// </summary>
    [HttpPost]
    [Authorize]
    //[AuthorizeFilter("Site", ActionType.Add)]
    public async Task<SitesDto> Add([FromBody] SitesEditDto modelDto)
    {
        if (modelDto.name == null)
        {
            throw Oops.Oh($"站名英文名称不能为空");
        }
        //检查站点名称是否重复
        if (await _siteService.AnyAsync(x => x.Name != null && x.Name.ToLower() == modelDto.name.ToLower()))
        {
            throw Oops.Oh($"站点名称[{modelDto.name}]已存在");
        }

        //映射成实体
        var model = modelDto.Adapt<Sites>();
        model.AddBy = await _userService.GetUserNameAsync();
        model.AddTime = DateTime.Now;

        var result = await _siteService.Context.InsertNav(model).Include(x=>x.Domains).ExecuteCommandAsync();
        return (model.Adapt<SitesDto>());
    }

    /// <summary>
    /// 修改一条记录
    /// 示例：/admin/site/1
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    //[AuthorizeFilter("Site", ActionType.Edit)]
    public async Task Update([FromRoute] int id, [FromBody] SitesEditDto modelDto)
    {
        if (modelDto.name == null)
        {
            throw Oops.Oh($"站名英文名称不能为空");
        }
        //检查数据是否存在
        var model = await _siteService.AsQueryable().Includes(x => x.Domains).SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"站点不存在或已删除");
        }
        //检查站点名称是否变更
        if (!modelDto.name.Equals(model.Name)
            && await _siteService.AnyAsync(x => x.Name != null && x.Name.ToLower() == modelDto.name.ToLower()))
        {
            throw Oops.Oh($"站点名称[{modelDto.name}]已存在");
        }
       
        //_siteService.Context.Tracking(model);
        //将DTO映射到源数据,修改站点信息
        modelDto.Adapt( model);
       //var cs =  _siteService.Context.GetChanges(model);
        await _siteService.Context.UpdateNav<Sites>(model)
            .Include(x => x.Domains)
            .ExecuteCommandAsync();
    }

    /// <summary>
    /// 局部更新一条记录
    /// 示例：/admin/site/1
    /// Body：[{"op":"replace","path":"/title","value":"new title"}]
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize]
    //[AuthorizeFilter("Site", ActionType.Edit)]
    public async Task Update([FromRoute] int id, [FromBody] JsonPatchDocument<SitesEditDto> patchDocument)
    {
        //注意：要使用写的数据库进行查询，才能正确写入数据主库
        var model = await _siteService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }

        var modelToPatch = model.Adapt<SitesEditDto>();
        patchDocument.ApplyTo(modelToPatch);
        ////验证数据是否合法
        //if (!TryValidateModel(modelToPatch))
        //{
        //    return ValidationProblem(ModelState);
        //}
        _siteService.Context.Tracking(model);
        //更新操作AutoMapper替我们完成，只需要调用保存即可
        modelToPatch.Adapt( model);
        await _siteService.UpdateAsync(model);
    }

    /// <summary>
    /// 删除一条记录(级联数据)
    /// 示例：/admin/site/1
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    //[AuthorizeFilter("Site", ActionType.Delete)]
    public async Task Delete([FromRoute] int id)
    {
        //取得站点实体信息(带域名)
        var model = await _siteService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }

        var result = await _siteService.DeleteAsync(x => x.Id == id);
    }

    /// <summary>
    /// 批量删除记录(级联数据)
    /// 示例：/admin/site?ids=1,2,3
    /// </summary>
    [HttpDelete]
    [Authorize]
    //[AuthorizeFilter("Site", ActionType.Delete)]
    public async Task DeleteByIds([FromQuery] string Ids)
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
        //执行删除操作
        var result = await _siteService.DeleteAsync(x => arrIds.Contains(x.Id));
    }
    #endregion

    #region 前台调用接口============================
    /// <summary>
    /// 根据ID或名称获取站点信息
    /// 示例：/client/site/1
    /// </summary>
    [HttpGet("/client/site/{siteKey}")]
    [CachingFilter]
    public async Task<dynamic> GetClientById([FromRoute] string siteKey, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<SitesDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        Sites? model = null;
        if (int.TryParse(siteKey, out int siteId))
        {
            model = await _siteService.SingleAsync(x => x.Id == siteId);
        }
        if (model == null)
        {
            model = await _siteService.SingleAsync(x => x.Name == siteKey);
        }
        //查询数据库获取实体
        if (model == null)
        {
            throw Oops.Oh($"数据{siteKey}不存在或已删除");
        }
        //使用AutoMapper转换成ViewModel
        //根据字段进行塑形
        var result = model.Adapt<SitesDto>().ShapeData(param.Fields);
        return (result);
    }

    /// <summary>
    /// 获取指定数量列表
    /// 示例：/client/site/view/0
    /// </summary>
    [HttpGet("/client/site/view/{top}")]
    [AllowAnonymous]
    [CachingFilter]
    [NonUnify]
    public async Task<dynamic> GetClientList([FromRoute] int top, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<SitesDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<SitesDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        if (top ==0)
        {
            top = 999;
        }

        //获取数据库列表
        var resultFrom = await _siteService.AsQueryable()
            .Includes(x=>x.Domains)
           .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.Keyword))
           .OrderBy(searchParam.OrderBy ?? "SortId,Id")
           .AutoTake(top)
           .ToListAsync();

        //var resultFrom = await _siteService.QueryListAsync(top,
        //    x => (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "SortId,Id");

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<SitesDto>>();//.ShapeData(searchParam.Fields);
        //返回成功200
        return (resultDto);
    }
    #endregion

}