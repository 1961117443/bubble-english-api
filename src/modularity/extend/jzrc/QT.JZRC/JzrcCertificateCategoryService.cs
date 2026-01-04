using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.JZRC.Entitys.Dto.JzrcCertificateCategory;
using QT.JZRC.Entitys;
using QT.JZRC.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using QT.Common.Const;

namespace QT.JZRC;

/// <summary>
/// 业务实现：证书分类.
/// </summary>
[ApiDescriptionSettings(ModuleConst.JZRC, Tag = "JZRC", Name = "JzrcCertificateCategory", Order = 200)]
[Route("api/JZRC/[controller]")]
public class JzrcCertificateCategoryService : IJzrcCertificateCategoryService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<JzrcCertificateCategoryEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="JzrcCertificateCategoryService"/>类型的新实例.
    /// </summary>
    public JzrcCertificateCategoryService(
        ISqlSugarRepository<JzrcCertificateCategoryEntity> jzrcCertificateCategoryRepository,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = jzrcCertificateCategoryRepository;
        _userManager = userManager;
    }

    /// <summary>
    /// 获取证书分类.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        return (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<JzrcCertificateCategoryInfoOutput>();
    }

    /// <summary>
    /// 获取证书分类列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] JzrcCertificateCategoryListQueryInput input)
    {
        var data = await _repository.Context.Queryable<JzrcCertificateCategoryEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.name), it => it.Name.Contains(input.name))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.Name.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new JzrcCertificateCategoryListOutput
            {
                id = it.Id,
                name = it.Name,
                remark = it.Remark,
                order = it.Order,
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<JzrcCertificateCategoryListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建证书分类.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] JzrcCertificateCategoryCrInput input)
    {
        var entity = input.Adapt<JzrcCertificateCategoryEntity>();
        if (await _repository.Context.Queryable<JzrcCertificateCategoryEntity>().AnyAsync(x => x.Name == entity.Name))
        {
            throw Oops.Oh(ErrorCode.COM1004);
        }
        entity.Id = SnowflakeIdHelper.NextId();
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新证书分类.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] JzrcCertificateCategoryUpInput input)
    {
        var entity = input.Adapt<JzrcCertificateCategoryEntity>();
        if (await _repository.Context.Queryable<JzrcCertificateCategoryEntity>().AnyAsync(x => x.Name == entity.Name && x.Id!= entity.Id))
        {
            throw Oops.Oh(ErrorCode.COM1004);
        }
        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除证书分类.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var entity = await _repository.Context.Queryable<JzrcCertificateCategoryEntity>().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);

        _repository.Context.Tracking(entity);
        entity.Delete();
        var isOk = await _repository.Context.Updateable(entity).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);

        //var isOk = await _repository.Context.Deleteable<JzrcCertificateCategoryEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();
        //if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
    }

    /// <summary>
    /// 证书下拉选项
    /// </summary>
    /// <returns></returns>
    [HttpGet("Actions/getSelector")]
    public async Task<dynamic> Selector()
    {
       var list = await _repository.Context.Queryable<JzrcCertificateCategoryEntity>()
            .Select(x => new
            {
                id = x.Id,
                fullName = x.Name
            })
            .ToListAsync();

        return new { list = list };
    }
}