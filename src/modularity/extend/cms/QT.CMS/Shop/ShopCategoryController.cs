using Microsoft.AspNetCore.Mvc.RazorPages;
using NPOI.XWPF.UserModel;
using System.Linq.Expressions;

namespace QT.CMS;

/// <summary>
/// 商城类别
/// </summary>
[Route("api/cms/admin/shop/category")]
[ApiController]
public class ShopCategoryController : ControllerBase
{
    private readonly ISqlSugarRepository<ShopCategory> _shopCategoryService;
    private readonly IUserService _userService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public ShopCategoryController(ISqlSugarRepository<ShopCategory> shopCategoryService, IUserService userService)
    {
        _shopCategoryService = shopCategoryService;
        _userService = userService;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 根据ID获取数据
    /// 示例：/admin/shop/category/1
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopCategory", ActionType.View)]
    public async Task<IActionResult> GetById([FromRoute] long id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<ShopCategoryDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var model = await _shopCategoryService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        //使用AutoMapper转换成ViewModel
        //根据字段进行塑形
        var result = model.Adapt<ShopCategoryDto>().ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 获取指定数量列表
    /// 示例：/admin/shop/category/view/1/0
    /// </summary>
    [HttpGet("view/{parentId}/{top}")]
    [Authorize]
    [AuthorizeFilter("ShopCategory", ActionType.View)]
    public async Task<IActionResult> GetList([FromRoute] long parentId, [FromRoute] int top, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (!searchParam.Fields.IsPropertyExists<ShopCategoryDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //获取数据库列表
        var resultFrom = await _shopCategoryService.AsQueryable()
            .Where(x => x.ParentId == parentId)
            .WhereIF(searchParam.Status > -1, x => x.Status == searchParam.Status)
            .OrderBy("SortId,Id")
            .AutoTake(top)
            .ToListAsync();
        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<ShopCategoryDto>>().ShapeData(searchParam.Fields);
        //返回成功200
        return Ok(resultDto);
    }

    /// <summary>
    /// 获取树目录列表
    /// 示例：/admin/shop/category
    /// </summary>
    [HttpGet]
    [Authorize]
    [AuthorizeFilter("ShopCategory", ActionType.View)]
    public async Task<dynamic> GetList([FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<ShopCategoryListDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //如果有查询关健字
        long parentId = 0; //父节点ID
        if (param.Keyword.IsNotEmptyOrNull())
        {
            var model = await _shopCategoryService.AsQueryable()
                .WhereIF(param.keyword.IsNotEmptyOrNull(),x=>x.Title.Contains(param.keyword))
                .FirstAsync();
            if (model == null)
            {
                throw Oops.Oh("暂无查询记录");
            }
            parentId = model.Id;
        }
        //获取数据库列表
        var listData = await _shopCategoryService.AsQueryable().ToListAsync();
        //调用递归重新生成目录树
        List<ShopCategoryListDto> resultFrom = await GetChilds(listData, parentId, param.Status ?? -1);

        ////使用AutoMapper转换成ViewModel，根据字段进行塑形
        //var resultDto = resultFrom.AsEnumerable().ShapeData(param.Fields);
        ////返回成功200
        //return Ok(resultDto);

        return new
        {
            pagination = new Common.Filter.PageResult(),
            list = resultFrom
        };
        //return PageResult<ShopCategoryListDto>.SqlSugarPageResult(resultFrom)
    }

    /// <summary>
    /// 添加一条记录
    /// 示例：/admin/shop/category
    /// </summary>
    [HttpPost]
    [Authorize]
    [AuthorizeFilter("ShopCategory", ActionType.Add)]
    public async Task<IActionResult> Add([FromBody] ShopCategoryEditDto modelDto)
    {
        //映射成实体
        var model = modelDto.Adapt<ShopCategory>();
        //获取当前用户名
        model.AddBy =  await _userService.GetUserNameAsync();
        model.AddTime = DateTime.Now;
        //写入数据库
        var mapModel = await _shopCategoryService.InsertAsync(model);
        //映射成DTO再返回，否则出错
        var result = mapModel.Adapt<ShopCategoryDto>();
        return Ok(result);
    }

    /// <summary>
    /// 修改一条记录
    /// 示例：/admin/shop/category/1
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopCategory", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] ShopCategoryEditDto modelDto)
    {
        //查找记录
        var model = await _shopCategoryService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        _shopCategoryService.Context.Tracking(model);
        //获取当前用户名
        model.UpdateBy = await _userService.GetUserNameAsync();
        model.UpdateTime = DateTime.Now;

        //更新操作AutoMapper替我们完成，只需要调用保存即可
        modelDto.Adapt(model);
        var result = await _shopCategoryService.UpdateAsync(model);
        return NoContent();
    }

    /// <summary>
    /// 局部更新一条记录
    /// 示例：/admin/shop/category/1
    /// Body：[{"op":"replace","path":"/title","value":"new title"}]
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopCategory", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] JsonPatchDocument<ShopCategoryEditDto> patchDocument)
    {
        var model = await _shopCategoryService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }

        var modelToPatch = model.Adapt<ShopCategoryEditDto>();
        patchDocument.ApplyTo(modelToPatch, ModelState);
        //验证数据是否合法
        if (!TryValidateModel(modelToPatch))
        {
            return ValidationProblem(ModelState);
        }
        _shopCategoryService.Context.Tracking(model);
        //更新操作AutoMapper替我们完成，只需要调用保存即可
        modelToPatch.Adapt(model);
        await _shopCategoryService.AutoUpdateAsync(model);

        return NoContent();
    }

    /// <summary>
    /// 删除一条记录
    /// 示例：/admin/shop/category/1
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopCategory", ActionType.Delete)]
    public async Task<IActionResult> Delete([FromRoute] long id)
    {
        if (!await _shopCategoryService.AnyAsync(x => x.Id == id))
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }

        var treeList = await _shopCategoryService.AsQueryable().ToChildListAsync(x => x.ParentId, id);

        if (treeList.IsAny())
        {
            await _shopCategoryService.DeleteAsync(treeList.Select(x=>(object)x.Id).ToArray());
        }

        return NoContent();
    }

    /// <summary>
    /// 批量删除记录(级联数据)
    /// 示例：/admin/shop/category?ids=1,2,3
    /// </summary>
    [HttpDelete]
    [Authorize]
    [AuthorizeFilter("ShopCategory", ActionType.Delete)]
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

        var treeList = await _shopCategoryService.AsQueryable().ToChildListAsync(x => x.ParentId, arrIds.Select(x=> (object)x).ToArray());

        if (treeList.IsAny())
        {
            await _shopCategoryService.DeleteAsync(treeList.Select(x => (object)x.Id).ToArray());
        }

        return NoContent();
    }
    #endregion

    #region 前台调用接口============================
    /// <summary>
    /// 根据ID获取数据
    /// 示例：/client/shop/category/1
    /// </summary>
    [HttpGet("/client/shop/category/{id}")]
    [CachingFilter]
    [NonUnify]
    public async Task<IActionResult> ClientGetById([FromRoute] long id, [FromQuery] BaseParameter param)
    {
        return await GetById(id, param);
    }

    /// <summary>
    /// 获取指定数量列表
    /// 示例：/client/shop/category/view/1/0
    /// </summary>
    [HttpGet("/client/shop/category/view/{parentId}/{top}")]
    [CachingFilter]
    [NonUnify]
    public async Task<IActionResult> ClientGetList([FromRoute] long parentId, [FromRoute] int top, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (!searchParam.Fields.IsPropertyExists<ShopCategoryDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //获取数据库列表
        //var resultFrom = await _shopCategoryService.QueryListAsync(top, parentId, 0);
        var resultFrom = await _shopCategoryService.Context.Queryable<ShopCategory>()
            .Where(x => x.ParentId == parentId && x.Status == 0)
            .OrderBy("SortId,Id")
            .AutoTake(top)
            .ToListAsync();  

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<ShopCategoryDto>>().ShapeData(searchParam.Fields);
        //返回成功200
        return Ok(resultDto);
    }

    /// <summary>
    /// 获取树目录列表
    /// 示例：/client/shop/category
    /// </summary>
    [HttpGet("/client/shop/category")]
    [CachingFilter]
    [NonUnify]
    public async Task<IActionResult> ClientGetList([FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<ShopCategoryListDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //如果有查询关健字
        long parentId = 0; //父节点ID
        if (param.Keyword.IsNotEmptyOrNull())
        {
            var model = await _shopCategoryService.AsQueryable()
                .WhereIF(param.keyword.IsNotEmptyOrNull(),x=>x.Title.Contains(param.keyword))
                .FirstAsync();
            if (model == null)
            {
                throw Oops.Oh("暂无查询记录");
            }
            parentId = model.Id;
        }
        //获取数据库列表
        var listData = await _shopCategoryService.Context.Queryable<ShopCategory>()
              //.Where(x => x.ParentId == parentId && x.Status == 0)
              //.OrderBy("SortId,Id")
              .ToListAsync();

        //调用递归重新生成目录树
        List<ShopCategoryListDto> resultFrom = await GetChilds(listData, parentId, 0);

        //使用AutoMapper转换成ViewModel，根据字段进行塑形
        //var resultDto = resultFrom.AsEnumerable(); //.ShapeData(param.Fields);
        //返回成功200
        return Ok(resultFrom);
    }
    #endregion

    #region 辅助私有方法

    ///// <summary>
    ///// 根据条件删除数据(迭代删除)
    ///// </summary>
    //private async Task<bool> DeleteAsync(Expression<Func<ShopCategory, bool>> funcWhere)
    //{
    //    var listData = await _shopCategoryService.Context.Queryable<ShopCategory>().ToListAsync();//查询所有数据
    //    var list = await _shopCategoryService.Context.Queryable<ShopCategory>().Where(funcWhere).ToListAsync();
    //    if (list == null)
    //    {
    //        return false;
    //    }
    //    foreach (var modelt in list)
    //    {
    //        await DeleteChilds(listData, modelt.Id);//删除子节点
    //        _context.RemoveRange(modelt);//删除当前节点
    //    }
    //    return await this.SaveAsync();
    //}



    ///// <summary>
    ///// 迭代循环删除
    ///// </summary>
    //private async Task DeleteChilds(IEnumerable<ShopCategory> listData, long parentId)
    //{
    //    IEnumerable<ShopCategory> models = listData.Where(x => x.ParentId == parentId);
    //    foreach (var modelt in models)
    //    {
    //        await DeleteChilds(listData, modelt.Id);//迭代
    //        _context.RemoveRange(modelt);
    //    }
    //}

    /// <summary>
    /// 迭代循环返回目录树(私有方法)
    /// </summary>
    private async Task<List<ShopCategoryListDto>> GetChilds(IEnumerable<ShopCategory> listData, long parentId, int status)
    {
        List<ShopCategoryListDto> listDto = new();
        IEnumerable<ShopCategory> models = listData.Where(x => x.ParentId == parentId && (status == -1 || x.Status == status)).OrderBy(x=>x.SortId);//查找并排序
        foreach (ShopCategory modelt in models)
        {
            ShopCategoryListDto modelDto = new()
            {
                id = modelt.Id,
                parentId = modelt.ParentId,
                siteId = modelt.SiteId,
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
                updateTime = modelt.UpdateTime
            };
            modelDto.children.AddRange(
                await GetChilds(listData, modelt.Id, status)
            );
            listDto.Add(modelDto);
        }
        return listDto;
    }
    #endregion
}