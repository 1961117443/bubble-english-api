using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using QT.CMS.Entitys;
using QT.CMS.Entitys.Dto.Article;
using QT.CMS.Entitys.Dto.Parameter;
using QT.Common.Const;
using QT.Common.Core.Filter;
using QT.Common.Core.Manager;
using QT.Common.Extension;
using QT.DynamicApiController;
using QT.FriendlyException;
using SqlSugar;
using System.Threading.Channels;

namespace QT.CMS;

/// <summary>
/// 文章类别
/// </summary>
//[Route("admin/article/category")]
[ApiController]
[ApiDescriptionSettings(ModuleConst.CMS, Tag = "admin", Name = "category", Order = 200)]
[Route("api/cms/admin/article/[controller]")]
public class ArticleCategoryController : IDynamicApiController
{
    //private readonly IArticleCategoryService _articleCategoryService;
    private readonly ISqlSugarRepository<ArticleCategory> _repository;
    //private readonly IUserService _userService;
    //private readonly IMapper _mapper;
    private readonly IUserService _userManager;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public ArticleCategoryController(ISqlSugarRepository<ArticleCategory> repository,
        //IUserService userService,
        //IMapper mapper,
        IUserService userManager)
    {
        //_articleCategoryService = articleCategoryService;
        _repository = repository;
        //_userService = userService;
        //_mapper = mapper;
        _userManager = userManager;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 根据ID获取数据
    /// 示例：/admin/article/category/1/1
    /// </summary>
    [HttpGet("{channelId}/{id}")]
    [Authorize]
    //[AuthorizeFilter("ArticleCategory", ActionType.View, "channelId")]
    public async Task<dynamic> GetById([FromRoute] long id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<ArticleCategoryDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var model = await _repository.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        //使用AutoMapper转换成ViewModel
        //根据字段进行塑形
        var result = model.Adapt<ArticleCategoryDto>().ShapeData(param.Fields);
        return (result);
    }

    /// <summary>
    /// 获取指定数量列表
    /// 示例：/admin/article/category/view/1/1/0
    /// </summary>
    [HttpGet("view/{channelId}/{parentId}/{top}")]
    [Authorize]
    //[AuthorizeFilter("ArticleCategory", ActionType.View, "channelId")]
    public async Task<dynamic> GetList([FromRoute] long parentId, [FromRoute] int top, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (!searchParam.Fields.IsPropertyExists<ArticleCategoryDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //获取数据库列表
        var listData = await _repository.Where(x => x.ChannelId.Equals(top)).ToListAsync();
        //调用递归重新生成目录树
        List<ArticleCategoryDto> resultFrom = await GetChilds(listData, parentId);

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<ArticleCategoryDto>>().ShapeData(searchParam.Fields);
        //返回成功200
        return (resultDto);
    }

    /// <summary>
    /// 获取树目录列表
    /// 示例：/admin/article/category/1/1
    /// </summary>
    [HttpGet("{channelId}")]
    [Authorize]
    //[AuthorizeFilter("ArticleCategory", ActionType.View, "channelId")]
    public async Task<List<ArticleCategoryDto>> GetList([FromQuery] BaseParameter param, [FromRoute] int channelId)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<ArticleCategoryDto>() || channelId <= 0)
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //获取数据库列表
        var listData = await _repository.Where(x => x.ChannelId.Equals(channelId)).ToListAsync();
        //调用递归重新生成目录树
        List<ArticleCategoryDto> resultFrom = await GetChilds(listData, 0);
        //使用AutoMapper转换成ViewModel
        //根据字段进行塑形
        //var resultDto = resultFrom.AsEnumerable().ShapeData(param.Fields);
        //返回成功200
        return (resultFrom);
    }

    /// <summary>
    /// 添加一条记录
    /// 示例：/admin/article/category/1
    /// </summary>
    [HttpPost("{channelId}")]
    //[Authorize]
    //[AuthorizeFilter("ArticleCategory", ActionType.Add, "channelId")]
    public async Task<dynamic> Add([FromBody] ArticleCategoryEditDto modelDto)
    {
        ////验证数据是否合法
        //if (!TryValidateModel(modelDto))
        //{
        //    return ValidationProblem(ModelState);
        //}
        //检查频道是否存在
        var channelModel = await _repository.Context.Queryable<SiteChannel>().FirstAsync(x => x.Id.Equals(modelDto.channelId));
        if (channelModel == null)
        {
            throw Oops.Oh("频道不存在或已删除");
        }

        //映射成实体
        var model = modelDto.Adapt<ArticleCategory>();
        model.SiteId = channelModel.SiteId;
        //获取当前用户名
        model.AddBy = await _userManager.GetUserIdAsync();
        model.AddTime = DateTime.Now;
        //写入数据库
        var mapModel = await _repository.InsertAsync(model);
        //映射成DTO再返回，否则出错
        var result = mapModel.Adapt<ArticleCategoryDto>();
        return (result);
    }

    /// <summary>
    /// 修改一条记录
    /// 示例：/admin/article/category/1/1
    /// </summary>
    [HttpPut("{channelId}/{id}")]
    [Authorize]
    //[AuthorizeFilter("ArticleCategory", ActionType.Edit, "channelId")]
    public async Task Update([FromRoute] long id, [FromBody] ArticleCategoryEditDto modelDto)
    {
        ////验证数据是否合法
        //if (!TryValidateModel(modelDto))
        //{
        //    return ValidationProblem(ModelState);
        //}

        //查找记录
        var model = await _repository.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        //检查频道是否存在
        var channelModel = await _repository.Context.Queryable<SiteChannel>().FirstAsync(x => x.Id == modelDto.channelId);
        if (channelModel == null)
        {
            throw Oops.Oh($"频道[{modelDto.channelId}]不存在或已删除");
        }
        //需要更新的集合
        List<ArticleCategory> updateList = new();
        //检查是否有子集
        var listData = await _repository.Context.Queryable<ArticleCategory>().Where(x => x.ChannelId.Equals(model.ChannelId)).ToListAsync();
        //检查是否有子集
        List<ArticleCategory> childList = GetChildList(listData, new List<ArticleCategory>(), model.Id);
        //检查现在父级是否在原来的子集中
        var parentModel = childList.Find(t => t.Id.Equals(modelDto.parentId));
        if (parentModel != null)
        {
            //交换父级元素
            parentModel.ParentId = model.ParentId;
            updateList.Add(parentModel);
        }
        _repository.Context.Tracking(model);
        //AutoMapper将DTO映射到源数据
        modelDto.Adapt(model);
        //添加站点主键
        model.SiteId = channelModel.SiteId;
        //获取当前用户名
        model.UpdateBy = await _userManager.GetUserIdAsync();
        model.UpdateTime = DateTime.Now;
        //实体加入要修改的集合
        updateList.Add(model);
        //批量更新
        //foreach (var t in updateList)
        //{
        //    _context.Set<ArticleCategory>().Attach(t);
        //    _context.Entry<ArticleCategory>(t).State = EntityState.Modified;
        //}
        await _repository.UpdateAsync(updateList);
    }

    /// <summary>
    /// 局部更新一条记录
    /// 示例：/admin/article/category/1/1
    /// Body：[{"op":"replace","path":"/title","value":"new title"}]
    /// </summary>
    [HttpPatch("{channelId}/{id}")]
    [Authorize]
    //[AuthorizeFilter("ArticleCategory", ActionType.Edit, "channelId")]
    public async Task Update([FromRoute] long id, [FromBody] JsonPatchDocument<ArticleCategoryEditDto> patchDocument)
    {
        //注意：要使用写的数据库进行查询，才能正确写入数据主库
        var model = await _repository.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据{id}不存在或已删除");
        }
        var modelToPatch = model.Adapt<ArticleCategoryEditDto>();
        patchDocument.ApplyTo(modelToPatch);
        ////验证数据是否合法
        //if (!TryValidateModel(modelToPatch))
        //{
        //    return ValidationProblem(ModelState);
        //}
        //更新操作AutoMapper替我们完成，只需要调用保存即可
        _repository.Context.Tracking(model);
        modelToPatch.Adapt(model);
        await _repository.UpdateAsync(model);
    }

    /// <summary>
    /// 删除一条记录
    /// 示例：/admin/article/category/1/1
    /// </summary>
    [HttpDelete("{channelId}/{id}")]
    //[Authorize]
    //[AuthorizeFilter("ArticleCategory", ActionType.Delete, "channelId")]
    public async Task Delete([FromRoute] long id)
    {
        if (!await _repository.AnyAsync(x => x.Id == id))
        {
            throw Oops.Oh($"数据{id}不存在或已删除");
        }
        var result = await _repository.DeleteAsync(x => x.Id == id);
    }

    /// <summary>
    /// 批量删除记录(级联数据)
    /// 示例：/admin/article/category/1?ids=1,2,3
    /// </summary>
    [HttpDelete("{channelId}")]
    //[Authorize]
    //[AuthorizeFilter("ArticleCategory", ActionType.Delete, "channelId")]
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
        await _repository.DeleteAsync(x => arrIds.Contains(x.Id));
    }
    #endregion

    #region 前台调用接口============================
    /// <summary>
    /// 根据ID获取数据
    /// 示例：/client/article/category/1
    /// </summary>
    [HttpGet("/client/article/category/{id}")]
    [AllowAnonymous, NonUnify]
    [CachingFilter]
    public async Task<dynamic> ClientGetById([FromRoute] long id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<ArticleCategoryDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var model = await _repository.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        //使用AutoMapper转换成ViewModel
        //根据字段进行塑形
        var result = model.Adapt<ArticleCategoryDto>().ShapeData(param.Fields);
        return (result);
    }

    /// <summary>
    /// 获取指定数量列表
    /// 示例：/client/article/category/view/1/1/0
    /// </summary>
    [HttpGet("/client/article/category/view/{channelId}/{parentId}/{top}")]
    [AllowAnonymous, NonUnify]
    [CachingFilter]
    public async Task<dynamic> ClientGetList([FromRoute] int channelId, [FromRoute] long parentId, [FromRoute] int top, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (!searchParam.Fields.IsPropertyExists<ArticleCategoryDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        ////获取数据库列表
        //var listData = await _repository.Where(x => x.ChannelId.Equals(top)).ToListAsync();
        ////调用递归重新生成目录树
        //List<ArticleCategoryDto> resultFrom = await GetChilds(listData, parentId);

        var qur = _repository.Where(x => x.ParentId == parentId && x.ChannelId == channelId).OrderBy("SortId,Id");
        if (top > 0) qur = qur.Take(top);//等于0显示所有数据
        var resultFrom = await qur.ToListAsync(); 

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<ArticleCategoryDto>>().ShapeData(searchParam.Fields);
        //返回成功200
        return (resultDto);
    }

    /// <summary>
    /// 根据频道ID或名称获取树目录列表
    /// 示例：/client/article/news/category
    ///       /client/article/1/category
    /// </summary>
    [HttpGet("/client/article/{channelKey}/category")]
    [AllowAnonymous,NonUnify]
    [CachingFilter]
    public async Task<dynamic> ClientGetList([FromRoute] string channelKey, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<ArticleCategoryDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //检查是否频道ID或者名称
        SiteChannel? channelModel = null;
        if (int.TryParse(channelKey, out int channelId))
        {
            channelModel = await _repository.Context.Queryable<SiteChannel>().FirstAsync(x => x.Id.Equals(channelId) && x.SiteId.Equals(param.SiteId));
        }
        if (channelModel == null)
        {
            channelModel = await _repository.Context.Queryable<SiteChannel>().FirstAsync(x => x.Name != null && x.Name.Equals(channelKey) && x.SiteId.Equals(param.SiteId));
        }
        if (channelModel == null)
        {
            throw Oops.Oh("频道不存在或已删除");
        }
        //如果有查询关健字
        long parentId = 0; //父节点ID
        if (param.Keyword.IsNotEmptyOrNull())
        {
            var model = await _repository.SingleAsync(x => x.Title != null && x.Title.Contains(param.Keyword));
            if (model == null)
            {
                throw Oops.Oh("暂无查询记录");
            }
            parentId = model.Id;
        }
        //获取数据库列表
        var listData = await _repository.Where(x => x.ChannelId.Equals(channelModel.Id)).ToListAsync();
        //调用递归重新生成目录树
        List<ArticleCategoryDto> resultFrom = await GetChilds(listData, parentId); 

        //使用AutoMapper转换成ViewModel，根据字段进行塑形
        //var resultDto = resultFrom.AsEnumerable().ShapeData(param.Fields);
        //返回成功200
        return (resultFrom);
    }
    #endregion


    #region Private Methods

    /// <summary>
    /// 递归返回子级数，不是目录树
    /// </summary>
    /// <param name="data">主数据</param>
    /// <param name="parentId">父级</param>
    private List<ArticleCategory> GetChildList(List<ArticleCategory> listData, List<ArticleCategory> list, long parentId)
    {
        IEnumerable<ArticleCategory> models = listData.Where(x => x.ParentId == parentId).OrderBy(x=>x.SortId);//查找并排序
        if (models != null)
        {
            foreach (var m in models)
            {
                list.Add(m);
                GetChildList(listData, list, m.Id);
            }
        }
        return list;
    }


    /// <summary>
    /// 迭代循环返回目录树(私有方法)
    /// </summary>
    private async Task<List<ArticleCategoryDto>> GetChilds(IEnumerable<ArticleCategory> listData, long parentId)
    {
        List<ArticleCategoryDto> listDto = new();
        IEnumerable<ArticleCategory> models = listData.Where(x => x.ParentId == parentId).OrderBy(x=>x.SortId);//查找并排序
        foreach (ArticleCategory modelt in models)
        {
            ArticleCategoryDto modelDto = new()
            {
                id = modelt.Id,
                parentId = modelt.ParentId,
                siteId = modelt.SiteId,
                callIndex = modelt.CallIndex,
                title = modelt.Title,
                imgUrl = modelt.ImgUrl,
                linkUrl = modelt.LinkUrl,
                content = modelt.Content,
                sortId = modelt.SortId,
                seoTitle = modelt.SeoTitle,
                seoKeyword = modelt.SeoKeyword,
                seoDescription = modelt.SeoDescription,
                status = modelt.Status,
                addBy = modelt.AddBy,
                addTime = modelt.AddTime,
                updateBy = modelt.UpdateBy,
                updateTime = modelt.UpdateTime,


            };
            modelDto.children.AddRange(
                await GetChilds(listData, modelt.Id)
            );
            listDto.Add(modelDto);
        }
        return listDto;
    } 
    #endregion
}
