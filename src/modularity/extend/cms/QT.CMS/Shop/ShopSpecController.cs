using Microsoft.AspNetCore.Http;
using QT.CMS.Emum;
using System.Transactions;

namespace QT.CMS;

/// <summary>
/// 商品规格
/// </summary>
[Route("api/cms/admin/shop/spec")]
[ApiController]
public class ShopSpecController : ControllerBase
{
    private readonly ISqlSugarRepository<ShopSpec> _shopSpecService;
    private readonly IUserService _userService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public ShopSpecController(ISqlSugarRepository<ShopSpec> shopSpecService, IUserService userService)
    {
        _shopSpecService = shopSpecService;
        _userService = userService;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 根据ID获取数据
    /// 示例：/admin/shop/spec/1
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopSpec", ActionType.View)]
    public async Task<IActionResult> GetById([FromRoute] long id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<ShopSpecDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var list = await _shopSpecService.AsQueryable().ToChildListAsync(x => x.ParentId, id);
        var model = list.FirstOrDefault(x => x.ParentId == 0 && x.Id == id);
        //var model = await _shopSpecService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }

        var resultDto = model.Adapt<ShopSpecDto>();

        resultDto.children  = list.Where(x => x.ParentId == model.Id).Adapt<ICollection<ShopSpecChildrenDto>>();

        //使用AutoMapper转换成ViewModel，根据字段进行塑形
        var result = resultDto.ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 获取指定数量列表
    /// 示例：/admin/shop/spec/view/10
    /// </summary>
    [HttpGet("view/{top}")]
    [Authorize]
    [AuthorizeFilter("ShopSpec", ActionType.View)]
    public async Task<IEnumerable<ShopSpecDto>> GetList([FromRoute] int top, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (!searchParam.Fields.IsPropertyExists<ShopSpecDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据库列表
        //var resultFrom = await _shopSpecService.QueryListAsync(top,
        //    x => !searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword)));

        List<ShopSpecDto> resultFrom = new List<ShopSpecDto>();
        var list = await _shopSpecService.AsQueryable()
            .WhereIF(searchParam.keyword.IsNotEmptyOrNull(),x=>x.Title.Contains(searchParam.keyword))
            .ToListAsync();
        var parentList = list.Where(x => x.ParentId == 0).OrderBy(x => x.SortId);
        foreach (var modelt in parentList)
        {
            var modeltDto = modelt.Adapt<ShopSpecDto>();
            var models = list.Where(x => x.ParentId == modelt.Id).OrderBy(x => x.SortId);
            modeltDto.children = models.Adapt<ICollection<ShopSpecChildrenDto>>();
            resultFrom.Add(modeltDto);
        }
        if (top > 0) resultFrom = resultFrom.Take(top).ToList();//等于0显示所有数据


        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.AsEnumerable(); //.ShapeData(searchParam.Fields);
        //返回成功200
        return (resultDto);
    }

    /// <summary>
    /// 获取分页列表
    /// 示例：/admin/shop/spec?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet]
    [Authorize]
    [AuthorizeFilter("ShopSpec", ActionType.View)]
    public async Task<dynamic> GetList([FromQuery] BaseParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ShopSpecDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ShopSpecDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表，如果ID大于0则查询该用户下所有的列表

        var list = await _shopSpecService.AsQueryable()
            .Where(x=>x.ParentId == 0)
            .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.keyword))
            .OrderBy(searchParam.OrderBy ?? "SortId,-Id")
            .ToPagedListAsync(searchParam.currentPage, searchParam.pageSize);

        //var list = await _shopSpecService.QueryPageAsync<ShopSpec>(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => x.ParentId == 0
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
        //var resultDto = _mapper.Adapt<IEnumerable<ShopSpecListDto>>(list).ShapeData(searchParam.Fields);
        //return Ok(resultDto);

        return PageResult<ShopSpecListDto>.SqlSugarPageResult(list);
    }

    /// <summary>
    /// 添加一条记录
    /// 示例：/admin/shop/spec
    /// </summary>
    [HttpPost]
    [Authorize]
    [AuthorizeFilter("ShopSpec", ActionType.Add)]
    [SqlSugarUnitOfWork]
    public async Task<IActionResult> Add([FromBody] ShopSpecEditDto modelDto)
    {
        //写入数据库
        var resultFrom = await this.AddAsync(modelDto);
        var result = await this.GetById(resultFrom.Id,new BaseParameter());
        return Ok(result);
    }

    /// <summary>
    /// 修改一条记录
    /// 示例：/admin/shop/spec/1
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopSpec", ActionType.Edit)]
    [SqlSugarUnitOfWork]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] ShopSpecEditDto modelDto)
    {
        var result = await this.UpdateAsync(id, modelDto);
        return NoContent();
    }

    /// <summary>
    /// 局部更新一条记录
    /// 示例：/admin/shop/spec/1
    /// Body：[{"op":"replace","path":"/title","value":"new title"}]
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopSpec", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] JsonPatchDocument<ShopSpecEditDto> patchDocument)
    {
        var model = await _shopSpecService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }

        var modelToPatch = model.Adapt<ShopSpecEditDto>();
        patchDocument.ApplyTo(modelToPatch, ModelState);
        //验证数据是否合法
        if (!TryValidateModel(modelToPatch))
        {
            return ValidationProblem(ModelState);
        }
        _shopSpecService.Context.Tracking(model);
        //更新操作AutoMapper替我们完成，只需要调用保存即可
        modelToPatch.Adapt(model);
        await _shopSpecService.AutoUpdateAsync(model);

        return NoContent();
    }

    /// <summary>
    /// 删除一条记录
    /// 示例：/admin/shop/spec/1
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopSpec", ActionType.Delete)]
    public async Task<IActionResult> Delete([FromRoute] long id)
    {
        if (!await _shopSpecService.AnyAsync(x => x.Id == id))
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        var result = await _shopSpecService.DeleteAsync(x => x.Id == id || x.ParentId == id);

        return NoContent();
    }

    /// <summary>
    /// 批量删除记录(级联数据)
    /// 示例：/admin/shop/spec?ids=1,2,3
    /// </summary>
    [HttpDelete]
    [Authorize]
    [AuthorizeFilter("ShopSpec", ActionType.Delete)]
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
        await _shopSpecService.DeleteAsync(x => arrIds.Contains(x.Id) || arrIds.Contains(x.ParentId));

        return NoContent();
    }
    #endregion


    #region Private Methods
    /// <summary>
    /// 添加一条记录
    /// </summary>
    private async Task<ShopSpec> AddAsync(ShopSpecEditDto modelDto)
    {

        //映射成源数据
        var model = modelDto.Adapt<ShopSpec>();
        //记录当前用户名
        model.AddBy = await _userService.GetUserNameAsync();
        model.AddTime = DateTime.Now;
        var childrenlList = modelDto.children.Adapt<ICollection<ShopSpec>>();


        //保存规格信息
        await _shopSpecService.InsertReturnEntityAsync(model);
        //新增规格值
        foreach (var modelt in childrenlList)
        {
            modelt.ParentId = model.Id;//父规格ID
        }
        //赋值之后批量添加
        await _shopSpecService.Context.Insertable<ShopSpec>(childrenlList).ExecuteCommandAsync();

        return model;
    }

    /// <summary>
    /// 修改一条记录
    /// </summary>
    private async Task<bool> UpdateAsync(long id, ShopSpecEditDto modelDto, WriteRoRead writeAndRead = WriteRoRead.Write)
    {

        //根据ID获取记录
        var model = await _shopSpecService.SingleAsync(x => x.ParentId == 0 && x.Id == id);
        //如果不存在则抛出异常
        if (model == null)
        {
            throw Oops.Oh("规格不存在或已删除").StatusCode(StatusCodes.Status404NotFound);
        }

        //如果没有规格值则全部删除
        if (modelDto.children == null || modelDto.children.Count == 0)
        {
            //删除所有的规格值
            var removeList = await _shopSpecService.Where(x => x.ParentId == id).ToListAsync();
            await _shopSpecService.DeleteAsync(removeList.Select(x=>(object)x.Id).ToArray());
        }
        else
        {
            //将DTO转换成源数据列表
            var list = modelDto.children.Adapt<ICollection<ShopSpec>>();
            var listIds = list.Where(x => x.Id > 0).Select(x => x.Id);
            //删除已移除的规格值
            var removeList = await _shopSpecService.Where(x => x.ParentId == id && !listIds.Contains(x.Id)).ToListAsync();
            await _shopSpecService.Context.Deleteable(removeList).ExecuteCommandAsync();
            //如果ID大于0则修改，否则添加
            foreach (var modelt in list)
            {
                modelt.ParentId = id;//将父ID设置为规格ID
                if (modelt.Id > 0)
                {
                    //修改
                    await _shopSpecService.UpdateAsync(modelt);
                }
                else
                {
                    //添加
                    await _shopSpecService.InsertReturnEntityAsync(modelt);
                }
            }
        }
        //将DTO映射到源数据
        modelDto.Adapt(model);
        return await _shopSpecService.UpdateAsync(model) > 0;
    } 
    #endregion
}