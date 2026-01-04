using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using QT.CMS.Emum;
using QT.CMS.Entitys;
using QT.CMS.Entitys.Dto.Base;
using QT.CMS.Entitys.Dto.Parameter;
using QT.Common.Core.Filter;
using QT.Common.Core.Manager;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.FriendlyException;
using SqlSugar;
using Yitter.IdGenerator;

namespace QT.CMS;

/// <summary>
/// 频道接口
/// </summary>
[Route("api/cms/admin/channel")]
[ApiController]
public class SiteChannelController : ControllerBase
{
    private readonly ISqlSugarRepository<SiteChannel> _siteChannelService;
    private readonly IUserService _userService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public SiteChannelController(ISqlSugarRepository<SiteChannel> siteChannelService,IUserService userService)
    {
        _siteChannelService = siteChannelService;
        _userService = userService;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 根据ID获取频道
    /// 示例：/admin/channel/1
    /// </summary>
    [HttpGet("{channelId}")]
    [Authorize]
    //[AuthorizeFilter("Channel", ActionType.View)]
    public async Task<IActionResult> GetById([FromRoute] int channelId, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<SiteChannelDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var model = await _siteChannelService.AsQueryable().Includes(x=>x.Fields).SingleAsync(x => x.Id == channelId);
        if (model == null)
        {
            throw Oops.Oh($"频道[{channelId}]不存在或已删除");
        }
        //使用AutoMapper转换成ViewModel
        //根据字段进行塑形
        var result = model.Adapt<SiteChannelDto>().ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 获取指定数量列表
    /// 示例：/admin/channel/view/0
    /// </summary>
    [HttpGet("view/{top}")]
    [Authorize]
    //[AuthorizeFilter("Channel", ActionType.View)]
    public async Task<IActionResult> GetList([FromRoute] int top, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<SiteChannelDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<SiteChannelDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        if (top <=0)
        {
            top = 999;
        }

        //获取数据库列表
        var resultFrom = await _siteChannelService.AsQueryable()
            .WhereIF(searchParam.SiteId > 0, x => x.SiteId == searchParam.SiteId)
            .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.Keyword) || x.Name.Contains(searchParam.Keyword))
            .OrderBy(searchParam.OrderBy ?? "SortId,AddTime")
            .AutoTake(top)
            .ToListAsync();

        //var resultFrom = await _siteChannelService.QueryListAsync<SiteChannel>(top,
        //    x => (searchParam.SiteId <= 0 || x.SiteId == searchParam.SiteId)
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "SortId,AddTime");

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<SiteChannelDto>>();//.ShapeData(searchParam.Fields);
        //返回成功200
        return Ok(resultDto);
    }

    /// <summary>
    /// 获取频道分页列表
    /// 示例：/admin/channel?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet]
    [Authorize]
    //[AuthorizeFilter("Channel", ActionType.View)]
    public async Task<IActionResult> GetList([FromQuery] BaseParameter searchParam/*, [FromQuery] PageParamater pageParam*/)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.TrimStart('-').IsPropertyExists<SiteChannelDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<SiteChannelDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表
        var list = await _siteChannelService.AsQueryable()
           .WhereIF(searchParam.SiteId > 0, x => x.SiteId == searchParam.SiteId)
           .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.Keyword) || x.Name.Contains(searchParam.Keyword))
           .OrderBy(searchParam.OrderBy ?? "SortId,AddTime")
           .ToPagedListAsync(searchParam.currentPage, searchParam.pageSize);

        //var list = await _siteChannelService.QueryPageAsync(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => (searchParam.SiteId <= 0 || x.SiteId == searchParam.SiteId)
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "SortId,AddTime");

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
        //var resultDto = _mapper.Map<IEnumerable<SiteChannelDto>>(list).ShapeData(searchParam.Fields);
        return Ok(PageResult<SiteChannelDto>.SqlSugarPageResult(list));
    }

    /// <summary>
    /// 添加一条记录
    /// 示例：/admin/channel/
    /// </summary>
    [HttpPost]
    [Authorize]
    //[AuthorizeFilter("Channel", ActionType.Add)]
    public async Task Add([FromBody] SiteChannelEditDto modelDto)
    {
        if (modelDto.name == null)
        {
            throw Oops.Oh($"频道英文名称不能为空");
        }
        //检查频道名称是否重复(同一点站点频道名不能重复)
        if (await _siteChannelService.AnyAsync(
            x => x.SiteId == modelDto.siteId
            && x.Name != null
            && x.Name.ToLower().Equals(modelDto.name.ToLower())))
        {
            throw Oops.Oh($"频道名称[{modelDto.name}]已重复", ErrorCode.RepeatField);
        }
        //检查站点信息是否正确
        if (!await _siteChannelService.Context.Queryable<Sites>().AnyAsync(x => x.Id == modelDto.siteId))
        {
            throw Oops.Oh($"站点不存在或已删除");
        }
        ////联合查询站点菜单
        //var navModel = await _navigationService.QueryBySiteIdAsync(modelDto.SiteId);
        //if (navModel == null)
        //{
        //    throw Oops.Oh("站点菜单不存在或已删除");
        //}
        ////查找菜单模型列表
        //var modelList = await _navigationService.QueryModelAsync(NavType.Channel);
        //if (modelList == null)
        //{
        //    throw Oops.Oh("频道菜单模型数据不存在");
        //}
        //映射成实体
        var model = modelDto.Adapt<SiteChannel>();
        model.AddBy = await _userService.GetUserNameAsync();
        model.AddTime = DateTime.Now;

        await _siteChannelService.InsertAsync(model);
    }

    /// <summary>
    /// 修改一条记录
    /// 示例：/admin/channel/1
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    //[AuthorizeFilter("Channel", ActionType.Edit)]
    public async Task Update([FromRoute] int id, [FromBody] SiteChannelEditDto modelDto)
    {
        //检查数据是否存在
        var model = await _siteChannelService.AsQueryable().Includes(x => x.Fields).SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据不存在或已删除");
        }
        //频道名称不可更改
        if (model.Name != modelDto.name)
        {
            throw Oops.Oh($"频道名称不可更改", ErrorCode.ParameterError);
        }
        //所属站点不可更改
        if (model.SiteId != modelDto.siteId)
        {
            throw Oops.Oh($"所属站点不可更改", ErrorCode.ParameterError);
        }
        ////联合查询站点菜单
        //var siteNavModel = await _navigationService.QueryBySiteIdAsync(modelDto.SiteId);
        //if (siteNavModel == null)
        //{
        //    throw Oops.Oh("站点菜单不存在或已删除");
        //}
        ////修改菜单
        //var navModel = await _context.Set<ManagerMenu>()
        //    .FirstOrDefaultAsync(x => x.ParentId == siteNavModel.Id && x.ChannelId == model.Id);
        //if (navModel != null)
        //{
        //    navModel.Title = modelDto.Title;
        //    _context.Set<ManagerMenu>().Update(navModel);
        //}
        //_siteChannelService.Context.Tracking(model);
        ////将DTO映射到源数据,修改站点信息
        modelDto.Adapt(model);
        //await _siteChannelService.UpdateAsync(model);

        await _siteChannelService.Context.UpdateNav(model).Include(x => x.Fields).ExecuteCommandAsync();
    }

    /// <summary>
    /// 局部更新一条记录
    /// 示例：/admin/channel/1
    /// Body：[{"op":"replace","path":"/title","value":"new title"}]
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize]
    //[AuthorizeFilter("Channel", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] JsonPatchDocument<SiteChannelEditDto> patchDocument)
    {
        //注意：要使用写的数据库进行查询，才能正确写入数据主库
        var model = await _siteChannelService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"频道{id}不存在或已删除");
        }

        var modelToPatch = model.Adapt<SiteChannelEditDto>();
        patchDocument.ApplyTo(modelToPatch, ModelState);
        //验证数据是否合法
        if (!TryValidateModel(modelToPatch))
        {
            return ValidationProblem(ModelState);
        }
        _siteChannelService.Context.Tracking(model);
        //更新操作AutoMapper替我们完成，只需要调用保存即可
        modelToPatch.Adapt(model);
        await _siteChannelService.UpdateAsync(model);
        return NoContent();
    }

    /// <summary>
    /// 删除一条记录(级联数据)
    /// 示例：/admin/channel/1
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    //[AuthorizeFilter("Channel", ActionType.Delete)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        //取得站点实体信息(带域名)
        var model = await _siteChannelService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"频道{id}不存在或已删除");
        }
        //应还要检查频道下是否有文章，有则删除失败
        var result = await _siteChannelService.DeleteAsync(model);
        return NoContent();
    }

    /// <summary>
    /// 批量删除记录(级联数据)
    /// 示例：/admin/channel?ids=1,2,3
    /// </summary>
    [HttpDelete]
    [Authorize]
    //[AuthorizeFilter("Channel", ActionType.Delete)]
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
        //执行删除操作
        var result = await _siteChannelService.DeleteAsync(x => arrIds.Contains(x.Id));
        return NoContent();
    }

    /// <summary>
    /// 获取频道扩展字段
    /// 示例：/admin/channel/1/field
    /// </summary>
    [HttpGet("{id}/field")]
    [Authorize]
    //[AuthorizeFilter("Channel", ActionType.View)]
    public async Task<IActionResult> GetFieldList([FromRoute] int id, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<SiteChannelFieldDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<SiteChannelFieldDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据库列表
        var resultFrom = await _siteChannelService.Context.Queryable<SiteChannelField>().Where(x => x.ChannelId == id)
            .OrderBy(searchParam.OrderBy ?? "SortId,Id")
            .ToListAsync();
        //var resultFrom = await _siteChannelService.QueryListAsync<SiteChannelField>(0,
        //    x => x.ChannelId == id, searchParam.OrderBy ?? "SortId,Id");

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<SiteChannelFieldDto>>().ShapeData(searchParam.Fields);
        //返回成功200
        return Ok(resultDto);
    }

    /// <summary>
    /// 获取站点菜单列表
    /// 示例：/admin/channel/view/menu
    /// </summary>
    [HttpGet("view/menu")]
    [Authorize]
    //[AuthorizeFilter("Channel", ActionType.View)]
    public async Task<IActionResult> GetMenuList([FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<SiteChannelDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<SiteChannelDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        // 获取所有的站点
        var siteList = await _siteChannelService.Context.Queryable<Sites>().Where(x => x.Status == 0).OrderBy(x => x.SortId).OrderBy(x => x.Id).ToListAsync();

        //获取数据库列表
        var resultFrom = await _siteChannelService.AsQueryable()
            .Includes(x=>x.Site)
            .WhereIF(searchParam.SiteId > 0, x => x.SiteId == searchParam.SiteId)
            .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.Keyword))
            .OrderBy(searchParam.OrderBy ?? "SortId,AddTime")
        .ToListAsync();

        var modelList = await _siteChannelService.Context.Queryable<ManagerMenuModel>().Where(x => x.NavType == NavType.Channel.ToString()).ToListAsync();

        List<SiteChannelMenuTreeModel> resultList = new List<SiteChannelMenuTreeModel>();
        

        foreach (var model in siteList)
        {
            //添加菜单
            SiteChannelMenuTreeModel navModel = new SiteChannelMenuTreeModel
            {
                NumParentId = 1,
                ChannelId = 0,
                Name = "site_" + model.DirPath,
                Title = model.Title,
                IsSystem = 1,
                Controller = "Site",
                Resource = "Show",
                AddBy = model.AddBy,
                AddTime = DateTime.Now,
                NumId = YitIdHelper.NextId(),
            };
            navModel.id = navModel.NumId.ToString();
            navModel.parentId = "0";
            resultList.Add(navModel);

            var g = resultFrom.Where(x => x.SiteId == model.Id).ToList();
            if (g.Any())
            {
                foreach (var channel in g)
                {
                    await AddNavigation(modelList, channel, navModel.Name ?? string.Empty, navModel.NumId, 0, resultList);
                }
            }            
        }

           

        //List<SiteChannelMenuTreeModel> resultList = new List<SiteChannelMenuTreeModel>();
        //foreach (var g in resultFrom.GroupBy(x=>x.SiteId))
        //{
        //    var firstId = SnowflakeIdHelper.NextId();
        //    SiteChannelMenuTreeModel first = new SiteChannelMenuTreeModel
        //    {
        //        id = firstId,
        //        parentId = "0",
        //        data = g.FirstOrDefault()?.Site
        //    };
        //    resultList.Add(first);
        //    foreach (var item in g)
        //    {
        //        var seconedId = SnowflakeIdHelper.NextId();
        //        SiteChannelMenuTreeModel seconed = new SiteChannelMenuTreeModel
        //        {
        //            id = seconedId,
        //            parentId = firstId,
        //            data = item
        //        };

        //        resultList.Add(seconed);
        //    }
        //}

        var resultDto = resultList.ToTree();
        //var resultFrom = await _siteChannelService.QueryListAsync<SiteChannel>(top,
        //    x => (searchParam.SiteId <= 0 || x.SiteId == searchParam.SiteId)
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "SortId,AddTime");
        //var resultDto = resultFrom.Adapt<IEnumerable<SiteChannelDto>>();
        //映射成DTO，根据字段进行塑形
        //var resultDto = resultFrom.Adapt<IEnumerable<SiteChannelDto>>().ShapeData(searchParam.Fields);
        //返回成功200
        return Ok(resultDto);
    }

    /// <summary>
    /// 迭代添加导航菜单(私有方法)
    /// </summary>
    /// <param name="modelData">菜单模型列表</param>
    /// <param name="channelModel">当前频道</param>
    /// <param name="navParentName">导航菜单父名称</param>
    /// <param name="navParentId">导航菜单父ID</param>
    /// <param name="modelParentId">菜单模型父ID</param>
    private async Task AddNavigation(IEnumerable<ManagerMenuModel> modelData, SiteChannel channelModel,
        string navParentName, long navParentId, long modelParentId, List<SiteChannelMenuTreeModel> list)
    {
        SiteChannelMenuTreeModel navModel;//创建导航菜单
                             //查找并排序
        IEnumerable<ManagerMenuModel> models = modelData.Where(x => x.ParentId == modelParentId).OrderBy(x => x.SortId).OrderBy(x => x.Id).ToList();
        foreach (var modelt in models)
        {
            //实例化菜单
            navModel = new SiteChannelMenuTreeModel
            {
                NumParentId = navParentId,
                ChannelId = channelModel.Id,
                Name = $"{navParentName}_{channelModel.Name}_{modelt.Name}",
                Title = modelParentId == 0 ? channelModel.Title : modelt.Title,
                SubTitle = modelt.SubTitle,
                IconUrl = modelt.IconUrl,
                LinkUrl = modelt.LinkUrl,
                SortId = modelt.SortId,
                IsSystem = 1,
                Controller = $"{modelt.Controller}", //$"{modelt.Controller}@{channelModel.Id}",
                Resource = modelt.Resource,
                AddBy = channelModel.AddBy,
                NumId = YitIdHelper.NextId()
            };
            navModel.id = navModel.NumId.ToString();
            navModel.parentId = navModel.NumParentId.ToString();
            list.Add(navModel);
            //迭代循环查找并添加，直到结束
            await AddNavigation(modelData, channelModel, navParentName, navModel.NumId, modelt.Id, list);
        }
    }
    #endregion

    #region 前台调用接口============================
    /// <summary>
    /// 根据ID获取频道
    /// 示例：/client/channel/1
    /// </summary>
    [HttpGet("/client/channel/{channelId}")]
    [CachingFilter]
    [AllowAnonymous,NonUnify]
    public async Task<IActionResult> ClientGetById([FromRoute] int channelId, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<SiteChannelDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var model = await _siteChannelService.SingleAsync(x => x.Id == channelId);
        if (model == null)
        {
            throw Oops.Oh($"频道{channelId}不存在或已删除");
        }
        //使用AutoMapper转换成ViewModel
        //根据字段进行塑形
        var result = model.Adapt<SiteChannelDto>().ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 根据名称获取频道
    /// 示例：/client/channel/1
    /// </summary>
    [HttpGet("/client/channelKey/{channelKey}")]
    [CachingFilter]
    [AllowAnonymous, NonUnify]
    public async Task<IActionResult> ClientGetByKey([FromRoute] string channelKey, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<SiteChannelDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var model = await _siteChannelService.SingleAsync(x => x.Name == channelKey && x.SiteId == param.SiteId);
        if (model == null)
        {
            throw Oops.Oh($"频道{channelKey}不存在或已删除");
        }
        //使用AutoMapper转换成ViewModel
        //根据字段进行塑形
        var result = model.Adapt<SiteChannelDto>().ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 获取指定数量列表
    /// 示例：/client/channel/view/0
    /// </summary>
    [HttpGet("/client/channel/view/{top}")]
    [CachingFilter]
    [AllowAnonymous, NonUnify]
    public async Task<IActionResult> ClientGetList([FromRoute] int top, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<SiteChannelDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<SiteChannelDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        if (top ==0)
        {
            top = 999;
        }
        //获取数据库列表
        var resultFrom = await _siteChannelService.AsQueryable()
           .WhereIF(searchParam.SiteId > 0, x => x.SiteId == searchParam.SiteId)
           .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.Keyword))
           .OrderBy(searchParam.OrderBy ?? "SortId,AddTime")
           .AutoTake(top)
           .ToListAsync();

        //var resultFrom = await _siteChannelService.QueryListAsync<SiteChannel>(top,
        //    x => (searchParam.SiteId <= 0 || x.SiteId == searchParam.SiteId)
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "SortId,AddTime");

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<SiteChannelDto>>().ShapeData(searchParam.Fields);
        //返回成功200
        return Ok(resultDto);
    }

    /// <summary>
    /// 获取频道扩展字段
    /// 示例：/client/channel/1/field
    /// </summary>
    [HttpGet("/client/channel/{id}/field")]
    [CachingFilter]
    [AllowAnonymous, NonUnify]
    public async Task<IActionResult> ClientGetFieldList([FromRoute] int id, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<SiteChannelFieldDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<SiteChannelFieldDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据库列表
        var resultFrom = await _siteChannelService.Context.Queryable<SiteChannelField>()
            .Where(x=>x.ChannelId == id)
           .OrderBy(searchParam.OrderBy ?? "SortId,Id")
           .ToListAsync();

        //var resultFrom = await _siteChannelService.QueryListAsync<SiteChannelField>(0,
        //    x => x.ChannelId == id, searchParam.OrderBy ?? "SortId,Id");

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<SiteChannelFieldDto>>().ShapeData(searchParam.Fields);
        //返回成功200
        return Ok(resultDto);
    }

    /// <summary>
    /// 获取站点下所有的频道
    /// 示例：/client/channel/1/field
    /// </summary>
    [HttpGet("/client/channel/{id}/list")]
    [CachingFilter]
    [AllowAnonymous, NonUnify]
    public async Task<IActionResult> ClientGetChannelList([FromRoute] int id, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<SiteChannelFieldDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<SiteChannelFieldDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据库列表
        var resultFrom = await _siteChannelService.AsQueryable()
            .Where(x => x.SiteId == id)
           .OrderBy(searchParam.OrderBy ?? "SortId,Id")
           .ToListAsync();

        //var resultFrom = await _siteChannelService.QueryListAsync<SiteChannelField>(0,
        //    x => x.ChannelId == id, searchParam.OrderBy ?? "SortId,Id");

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<SiteChannelDto>>(); //.ShapeData(searchParam.Fields);
        //返回成功200
        return Ok(resultDto);
    }
    #endregion
}