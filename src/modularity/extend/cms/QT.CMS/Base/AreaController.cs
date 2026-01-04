using QT.Common.Security;

namespace QT.CMS;

/// <summary>
/// 省市区接口
/// </summary>
[Route("api/cms/admin/area")]
[ApiController]
public class AreaController : ControllerBase
{
    private readonly ISqlSugarRepository<Areas> _areaService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public AreaController(ISqlSugarRepository<Areas> areaService)
    {
        _areaService = areaService;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 根据ID获取地区
    /// 示例：/admin/area/1
    /// </summary>
    [HttpGet("{areaId}")]
    [Authorize]
    [AuthorizeFilter("Area", ActionType.View)]
    public async Task<IActionResult> GetById([FromRoute] int areaId, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<AreasDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var model = await _areaService.SingleAsync(x => x.Id == areaId);
        if (model == null)
        {
            throw Oops.Oh($"数据[{areaId}]不存在或已删除");
        }
        //使用AutoMapper转换成ViewModel
        //根据字段进行塑形
        var result = model.Adapt<AreasDto>().ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 根据父节点获取一级列表
    /// 示例：/admin/area/view/0
    /// </summary>
    [HttpGet("view/{top}")]
    [Authorize]
    [AuthorizeFilter("Area", ActionType.View)]
    public async Task<IActionResult> GetList([FromRoute] int top, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<AreasDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<AreasDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //获取一条数据
        var parentId = 0;
        if (searchParam.Keyword.IsNotEmptyOrNull())
        {
            var model = await _areaService.AsQueryable()
                .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.Title.Equals(searchParam.keyword))
                .FirstAsync();
            parentId = model != null ? model.Id : -1;
        }
        //获取数据库列表
        var resultFrom = await _areaService.AsQueryable()
            .Where(x => x.ParentId == parentId)
            .OrderBy(searchParam.OrderBy ?? "SortId,Id")
            .AutoTake(top)
            .ToListAsync();
        //使用AutoMapper转换成ViewModel，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<AreasDto>>(); //.ShapeData(searchParam.Fields);
        //返回成功200
        return Ok(resultDto);
    }

    /// <summary>
    /// 获取地区树目录列表
    /// 示例：/admin/area/
    /// </summary>
    [HttpGet]
    [Authorize]
    [AuthorizeFilter("Area", ActionType.View)]
    public async Task<IActionResult> GetList([FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<AreasDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //如果有查询关健字
        var parentId = 0; //父节点ID
        if (param.Keyword.IsNotEmptyOrNull())
        {
            var model = await _areaService.AsQueryable()
               .WhereIF(param.keyword.IsNotEmptyOrNull(), x => x.Title.Equals(param.keyword))
               .FirstAsync();
            if (model == null)
            {
                throw Oops.Oh("暂无查询记录");
            }
            parentId = model.Id;
        }
        //获取数据库列表
        //var resultFrom = await _areaService.QueryListAsync(parentId);

        var listData = await _areaService.ToListAsync();
        //调用递归重新生成目录树
        List<AreasDto> resultDto = await GetChilds(listData, parentId);

        //使用AutoMapper转换成ViewModel
        //根据字段进行塑形
        //var resultDto = resultFrom.AsEnumerable().ShapeData(param.Fields);
        //返回成功200
        return Ok(resultDto);
    }

    /// <summary>
    /// 添加一条记录
    /// 示例：/admin/area/
    /// </summary>
    [HttpPost]
    [Authorize]
    [AuthorizeFilter("Area", ActionType.Add)]
    public async Task<IActionResult> Add([FromBody] AreasEditDto modelDto)
    {
        //映射成实体
        var model = modelDto.Adapt<Areas>();
        //写入数据库
        await _areaService.InsertReturnEntityAsync(model);
        //映射成DTO再返回，否则出错
        var result = model.Adapt<AreasDto>();
        return Ok(result);
    }

    /// <summary>
    /// 修改一条记录
    /// 示例：/admin/area/1
    /// </summary>
    [HttpPut("{areaId}")]
    [Authorize]
    [AuthorizeFilter("Area", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] int areaId, [FromBody] AreasEditDto modelDto)
    {
        //查找记录
        var model = await _areaService.SingleAsync(x => x.Id == areaId);
        if (model == null)
        {
            throw Oops.Oh($"地区{areaId}不存在或已删除");
        }
        _areaService.Context.Tracking(model);
        //更新操作AutoMapper替我们完成，只需要调用保存即可
        modelDto.Adapt(model);
        var result = await _areaService.AutoUpdateAsync(model);
        return NoContent();
    }

    /// <summary>
    /// 局部更新一条记录
    /// 示例：/admin/area/1
    /// Body：[{"op":"replace","path":"/title","value":"new title"}]
    /// </summary>
    [HttpPatch("{areaId}")]
    [Authorize]
    [AuthorizeFilter("Area", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] int areaId, [FromBody] JsonPatchDocument<AreasEditDto> patchDocument)
    {
        var model = await _areaService.SingleAsync(x => x.Id == areaId);
        if (model == null)
        {
            throw Oops.Oh($"数据{areaId}不存在或已删除");
        }

        var modelToPatch = model.Adapt<AreasEditDto>();
        patchDocument.ApplyTo(modelToPatch, ModelState);
        //验证数据是否合法
        if (!TryValidateModel(modelToPatch))
        {
            return ValidationProblem(ModelState);
        }
        _areaService.Context.Tracking(model);
        //更新操作AutoMapper替我们完成，只需要调用保存即可
        modelToPatch.Adapt( model);
        await _areaService.AutoUpdateAsync(model);

        return NoContent();
    }

    /// <summary>
    /// 删除一条记录
    /// 示例：/admin/area/1
    /// </summary>
    [HttpDelete("{areaId}")]
    [Authorize]
    [AuthorizeFilter("Area", ActionType.Delete)]
    public async Task<IActionResult> Delete([FromRoute] int areaId)
    {
        if (!await _areaService.AnyAsync(x => x.Id == areaId))
        {
            throw Oops.Oh($"数据[{areaId}]不存在或已删除");
        }

        //var listData = await _areaService.ToListAsync();//查询所有数据
        //var list = await _areaService.AsQueryable().Where(x => x.Id == areaId).ToListAsync();
        //if (list == null)
        //{
        //    return false;
        //}
        //foreach (var modelt in list)
        //{
        //    await DeleteChilds(listData, modelt.Id);//删除子节点
        //    _context.RemoveRange(modelt);//删除当前节点
        //}

        var listData = await _areaService.AsQueryable().ToChildListAsync(x => x.ParentId, areaId);

        if (listData.IsAny())
        {
            var ids = listData.Select(x => x.Id).ToArray();
            var result = await _areaService.DeleteAsync(x=> ids.Contains(x.Id));
        }

       

        return NoContent();
    }

    /// <summary>
    /// 批量删除记录(级联数据)
    /// 示例：/admin/area?ids=1,2,3
    /// </summary>
    [HttpDelete]
    [Authorize]
    [AuthorizeFilter("Area", ActionType.Delete)]
    public async Task<IActionResult> DeleteByIds([FromQuery] string Ids)
    {
        if (Ids == null)
        {
            throw Oops.Oh("传输参数不可为空");
        }
        //将ID列表转换成IEnumerable
        var areaIds = Ids.ToIEnumerable<int>();
        if (areaIds == null)
        {
            throw Oops.Oh("传输参数不符合规范");
        }
        //执行批量删除操作
        await _areaService.DeleteAsync(x => areaIds.Contains(x.Id));

        return NoContent();
    }

    /// <summary>
    /// 批量导入数据
    /// 示例：/admin/area/import
    /// </summary>
    [HttpPost("import")]
    [Authorize]
    [AuthorizeFilter("Area", ActionType.Add)]
    public async Task<IActionResult> Import([FromBody] AreasImportEditDto modelDto)
    {
        var list = JsonHelper.ToObject<List<AreasImportDto>>(modelDto.jsonData);
        if (list == null)
        {
            throw Oops.Oh("数据格式有误，请检查重试");
        }
        await this.ImportChilds(list,0);
        return NoContent();
    }
    #endregion

    #region 前台调用接口============================
    /// <summary>
    /// 根据父节点获取一级列表
    /// 示例：/client/area/view/0
    /// </summary>
    [HttpGet("/client/area/view/{top}")]
    [NonUnify]
    public async Task<IActionResult> ClientGetList([FromRoute] int top, [FromQuery] BaseParameter searchParam)
    {
        var result = await GetList(top, searchParam);
        return result;
    }

    /// <summary>
    /// 获取地区树目录列表
    /// 示例：/client/area
    /// </summary>
    [HttpGet("/client/area")]
    [CachingFilter]
    [NonUnify]
    public async Task<IActionResult> ClientGetList([FromQuery] BaseParameter param)
    {
        var result = await GetList(param);
        return result;
    }
    #endregion


    #region Private Methods
    /// <summary>
    /// 迭代循环返回目录树(私有方法)
    /// </summary>
    private async Task<List<AreasDto>> GetChilds(IEnumerable<Areas> listData, int parentId)
    {
        List<AreasDto> listDto = new List<AreasDto>();
        IEnumerable<Areas> models = listData.Where(x => x.ParentId == parentId).OrderBy(x=>x.SortId);//查找并排序
        foreach (Areas modelt in models)
        {
            AreasDto modelDto = new AreasDto
            {
                id = modelt.Id,
                parentId = modelt.ParentId,
                title = modelt.Title,
                sortId = modelt.SortId
            };
            modelDto.children.AddRange(
                await GetChilds(listData, modelt.Id)
            );
            listDto.Add(modelDto);
        }
        return listDto;
    }

    /// <summary>
    /// 批量插入数据
    /// </summary>
    private async Task ImportChilds(List<AreasImportDto> listData, int parentId)
    {
        foreach (AreasImportDto modelDto in listData)
        {
            Areas model = new Areas();
            model.ParentId = parentId;
            model.Title = modelDto.name;
            await _areaService.InsertReturnEntityAsync(model);
            if (modelDto.children != null && modelDto.children.Count > 0)
            {
                await this.ImportChilds(modelDto.children, model.Id);
            }
        }
    }
    #endregion
}