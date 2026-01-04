using QT.Common.Core.Manager;
using QT.Common.Core.Manager.Files;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Systems.Entitys.Dto.DictionaryData;
using QT.Systems.Entitys.System;
using QT.Systems.Interfaces.Common;
using QT.Systems.Interfaces.System;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using QT.Common.Extension;
using QT.Systems.Entitys.Dto.SysEnum;
using QT.Common.Contracts;
using QT.Common.Filter;
using System.Linq;

namespace QT.Systems;

/// <summary>
/// 字典数据



/// 日 期：2021-06-01.
/// </summary>
[ApiDescriptionSettings(Tag = "System", Name = "DictionaryData", Order = 203)]
[Route("api/system/[controller]")]
public class DictionaryDataService : IDictionaryDataService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<DictionaryDataEntity> _repository;

    /// <summary>
    /// 字典类型服务.
    /// </summary>
    private readonly IDictionaryTypeService _dictionaryTypeService;

    /// <summary>
    /// 文件服务.
    /// </summary>
    private readonly IFileManager _fileManager;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 多租户事务.
    /// </summary>
    private readonly ITenant _db;

    /// <summary>
    /// 初始化一个<see cref="DictionaryDataService"/>类型的新实例.
    /// </summary>
    public DictionaryDataService(
        ISqlSugarRepository<DictionaryDataEntity> dictionaryDataRepository,
        IDictionaryTypeService dictionaryTypeService,
        IFileManager fileManager,
        IUserManager userManager,
        ISqlSugarClient context)
    {
        _repository = dictionaryDataRepository;
        _dictionaryTypeService = dictionaryTypeService;
        _fileManager = fileManager;
        _userManager = userManager;
        _db = context.AsTenant();
    }

    #region GET

    /// <summary>
    /// 获取数据字典列表.
    /// </summary>
    /// <param name="dictionaryTypeId">分类id.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpGet("{dictionaryTypeId}")]
    public async Task<dynamic> GetList_Api(string dictionaryTypeId, [FromQuery] DictionaryDataListQuery input)
    {
        var data = await GetList(dictionaryTypeId, false);
        if ("1".Equals(input.isTree))
        {
            var treeList = data.Adapt<List<DictionaryDataTreeOutput>>();
            if (!string.IsNullOrEmpty(input.keyword))
                treeList = treeList.TreeWhere(t => t.enCode.Contains(input.keyword) || t.fullName.Contains(input.keyword), t => t.id, t => t.parentId);
            return new { list = treeList.ToTree() };
        }
        else
        {
            if (!string.IsNullOrEmpty(input.keyword))
                data = data.FindAll(t => t.EnCode.Contains(input.keyword) || t.FullName.Contains(input.keyword));
            var treeList = data.Adapt<List<DictionaryDataTreeOutput>>();
            return new { list = treeList };
        }
    }

    /// <summary>
    /// 获取所有数据字典列表(分类+内容).
    /// </summary>
    /// <returns></returns>
    [HttpGet("All")]
    public async Task<dynamic> GetListAll()
    {
        var dictionaryData = await _repository.AsQueryable().Where(d => d.DeleteMark == null && d.EnabledMark == 1)
            .OrderBy(o => o.SortCode).OrderBy(o => o.CreatorTime, OrderByType.Desc).OrderBy(o => o.LastModifyTime, OrderByType.Desc).ToListAsync();
        var dictionaryType = await _dictionaryTypeService.GetList();
        var data = dictionaryType.Adapt<List<DictionaryDataAllListOutput>>();
        data.ForEach(dataall =>
        {
            if (dataall.isTree == 1)
                dataall.dictionaryList = dictionaryData.FindAll(d => d.DictionaryTypeId == dataall.id).Adapt<List<DictionaryDataTreeOutput>>().ToTree();
            else
                dataall.dictionaryList = dictionaryData.FindAll(d => d.DictionaryTypeId == dataall.id).Adapt<List<DictionaryDataListOutput>>();
        });

        var enumService = App.GetService<SysEnumService>();
        var enumList = enumService.GetEnumTypeList();
        if (enumList.IsAny())
        {
            foreach (var item in enumList)
            {
                DictionaryDataAllListOutput dictionary = new DictionaryDataAllListOutput
                {
                    enCode = item.TypeName,
                    fullName = item.TypeDescribe,
                    id = item.TypeName,
                    isTree = 0,
                };


                dictionary.dictionaryList = enumService.GetEnumDataList(new EnumInput { EnumName = item.TypeName })
                    .Where(x =>
                    {
                        if (x.Attributes.IsAny() && x.Attributes.Any(a => a is DictionaryDataIgnoreAttribute))
                        {
                            return false;
                        }
                        return true;
                    })
                    .Select(it =>
                    {
                        var da = new DictionaryDataListOutput
                        {
                            id = it.Name,
                            enCode = it.Value.ToString(),
                            fullName = it.Describe,
                            enabledMark = 1,
                            parentId = item.TypeName,
                            sortCode = it.Value,
                        };

                        if (it.Attributes.IsAny())
                        {
                            var attr = it.Attributes.FirstOrDefault(x => x is TagStyleAttribute);
                            if (attr !=null && attr is TagStyleAttribute tagStyleAttribute)
                            {
                                da.tagType = tagStyleAttribute.Style;
                            }
                        }

                        return da;
                    }).ToList();

                data.Add(dictionary);
            }
        }

        return new { list = data };

    }

    /// <summary>
    /// 获取字典分类下拉框.
    /// </summary>
    /// <returns></returns>
    [HttpGet("{dictionaryTypeId}/Selector/{id}")]
    public async Task<dynamic> GetSelector(string dictionaryTypeId, string id, string isTree)
    {
        var output = new List<DictionaryDataSelectorOutput>();
        var typeEntity = await _dictionaryTypeService.GetInfo(dictionaryTypeId);

        // 顶级节点
        var dataEntity = typeEntity.Adapt<DictionaryDataSelectorOutput>();
        dataEntity.id = "0";
        dataEntity.parentId = "-1";
        output.Add(dataEntity);
        if ("1".Equals(isTree))
        {
            var dataList = (await GetList(dictionaryTypeId, false)).Adapt<List<DictionaryDataSelectorOutput>>();
            if (!id.Equals("0"))
                dataList.RemoveAll(x => x.id == id);
            output = output.Union(dataList).ToList();
            return new { list = output.ToTree("-1") };
        }

        return new { list = output };
    }

    /// <summary>
    /// 获取字典数据下拉框列表.
    /// </summary>
    /// <returns></returns>
    [HttpGet("{dictionaryTypeId}/Data/Selector")]
    public async Task<dynamic> GetDataSelector(string dictionaryTypeId)
    {
        try
        {
            var isTree = (await _dictionaryTypeService.GetInfo(dictionaryTypeId)).IsTree;
            var datalist = await GetList(dictionaryTypeId);
            var treeList = datalist.Adapt<List<DictionaryDataSelectorDataOutput>>();
            if (isTree == 1)
            {
                var typeEntity = await _dictionaryTypeService.GetInfo(dictionaryTypeId);

                // 顶级节点
                var dataEntity = typeEntity.Adapt<DictionaryDataSelectorDataOutput>();
                dataEntity.id = "0";
                dataEntity.parentId = "-1";
                treeList.Add(dataEntity);
                treeList = treeList.ToTree();
            }

            return new { list = treeList };
        }
        catch (Exception)
        {
            return new { list = new List<object>() };
        }
    }

    /// <summary>
    /// 获取数据字典信息.
    /// </summary>
    /// <param name="id">主键id.</param>
    /// <returns></returns>
    [HttpGet("{id}/Info")]
    public async Task<dynamic> GetInfo_Api(string id)
    {
        var data = await GetInfo(id);
        return data.Adapt<DictionaryDataInfoOutput>();
    }

    /// <summary>
    /// 导出.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/Action/Export")]
    public async Task<dynamic> ActionsExport(string id)
    {
        var output = new DictionaryDataExportInput();
        await _dictionaryTypeService.GetListAllById(id, output.list);
        foreach (var item in output.list)
        {
            var modelList = await GetList(item.Id, false);
            output.modelList = output.modelList.Union(modelList).ToList();
        }

        var jsonStr = output.ToJsonString();
        return await _fileManager.Export(jsonStr, (await _dictionaryTypeService.GetInfo(id)).FullName, ExportFileType.bdd);
    }

    #endregion

    #region Post

    /// <summary>
    /// 添加.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Creater([FromBody] DictionaryDataCrInput input)
    {
        if (await _repository.AnyAsync(x => x.EnCode == input.enCode && x.DictionaryTypeId == input.dictionaryTypeId && x.DeleteMark == null) || await _repository.AnyAsync(x => x.FullName == input.fullName && x.DictionaryTypeId == input.dictionaryTypeId && x.DeleteMark == null))
            throw Oops.Oh(ErrorCode.D3003);
        var entity = input.Adapt<DictionaryDataEntity>();
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
        if (isOk < 1)
            throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 修改.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] DictionaryDataUpInput input)
    {
        if (await _repository.AnyAsync(x => x.EnCode == input.enCode && x.DictionaryTypeId == input.dictionaryTypeId && x.Id != id && x.DeleteMark == null) || await _repository.AnyAsync(x => x.Id != id && x.FullName == input.fullName && x.DictionaryTypeId == input.dictionaryTypeId && x.DeleteMark == null))
            throw Oops.Oh(ErrorCode.D3003);
        var entity = input.Adapt<DictionaryDataEntity>();
        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        if (!await _repository.AnyAsync(x => x.Id == id && x.DeleteMark == null))
            throw Oops.Oh(ErrorCode.D3004);
        if (await _repository.AnyAsync(o => o.ParentId.Equals(id) && o.DeleteMark == null))
            throw Oops.Oh(ErrorCode.D3002);
        var isOk = await _repository.Context.Updateable<DictionaryDataEntity>().SetColumns(it => new DictionaryDataEntity()
        {
            DeleteTime = DateTime.Now,
            DeleteMark = 1,
            DeleteUserId = _userManager.UserId
        }).Where(x => x.Id == id).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1002);
    }

    /// <summary>
    /// 更新字典状态.
    /// </summary>
    /// <param name="id">id.</param>
    /// <returns></returns>
    [HttpPut("{id}/Actions/State")]
    public async Task ActionsState(string id)
    {
        var isOk = await _repository.Context.Updateable<DictionaryDataEntity>().SetColumns(it => new DictionaryDataEntity()
        {
            EnabledMark = SqlFunc.IIF(it.EnabledMark == 0, 1, 0),
            LastModifyTime = DateTime.Now,
            LastModifyUserId = _userManager.UserId
        }).Where(x => x.Id == id).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.D1506);
    }

    /// <summary>
    /// 导入.
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    [HttpPost("Action/Import")]
    public async Task ActionsImport(IFormFile file)
    {
        var fileType = Path.GetExtension(file.FileName).Replace(".", string.Empty);
        if (!fileType.ToLower().Equals(ExportFileType.bdd.ToString()))
            throw Oops.Oh(ErrorCode.D3006);
        var josn = _fileManager.Import(file);
        var model = josn.ToObject<DictionaryDataExportInput>();
        if (model == null || model.list.Count == 0)
            throw Oops.Oh(ErrorCode.D3006);
        if (model.list.Find(x => x.ParentId == "-1") == null && !_dictionaryTypeService.IsExistParent(model.list))
            throw Oops.Oh(ErrorCode.D3007);
        await ImportData(model);
    }
    #endregion

    #region PulicMethod

    /// <summary>
    /// 列表.
    /// </summary>
    /// <param name="dictionaryTypeId">类别主键.</param>
    /// <param name="enabledMark">是否过滤启用状态.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<List<DictionaryDataEntity>> GetList(string dictionaryTypeId, bool enabledMark = true)
    {
        var entity = await _dictionaryTypeService.GetInfo(dictionaryTypeId);
        if (entity == null)
        {
            return new List<DictionaryDataEntity>();
        }
        return await _repository.AsQueryable().Where(d => d.DictionaryTypeId == entity.Id && d.DeleteMark == null).WhereIF(enabledMark, d => d.EnabledMark == 1)
            .OrderBy(o => o.SortCode).OrderBy(o => o.CreatorTime, OrderByType.Desc).OrderBy(o => o.LastModifyTime, OrderByType.Desc).ToListAsync();
    }

    /// <summary>
    /// 信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<DictionaryDataEntity> GetInfo(string id)
    {
        return await _repository.FirstOrDefaultAsync(x => x.Id == id && x.DeleteMark == null);
    }
    #endregion

    #region PrivateMethod

    /// <summary>
    /// 导入数据.
    /// </summary>
    /// <param name="inputList"></param>
    /// <returns></returns>
    private async Task ImportData(DictionaryDataExportInput inputList)
    {
        try
        {
            #region 插入数据

            _db.BeginTran();
            var isOk = await _repository.Context.Storageable(inputList.list).ExecuteCommandAsync();
            var isOk1 = await _repository.Context.Storageable(inputList.modelList).ExecuteCommandAsync();
            if (isOk < 1 && isOk1 < 1)
                throw Oops.Oh(ErrorCode.D3008);
            _db.CommitTran();

            #endregion
        }
        catch (Exception ex)
        {
            _db.RollbackTran();
            throw Oops.Oh(ErrorCode.D3008);
        }
    }
    #endregion
}