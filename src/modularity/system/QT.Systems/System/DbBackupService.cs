using QT.Common.Configuration;
using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Filter;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.LinqBuilder;
using QT.Systems.Entitys.Dto.DbBackup;
using QT.Systems.Entitys.System;
using QT.Systems.Interfaces.System;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using Yitter.IdGenerator;

namespace QT.Systems;

/// <summary>
/// 数据备份



/// 日 期：2021-06-01.
/// </summary>
[ApiDescriptionSettings(Tag = "System", Name = "DataBackup", Order = 207)]
[Route("api/system/[controller]")]
public class DbBackupService : IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<DbBackupEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="DbBackupService"/>类型的新实例.
    /// </summary>
    public DbBackupService(
        ISqlSugarRepository<DbBackupEntity> dbBackupRepository,
        IUserManager userManager)
    {
        _repository = dbBackupRepository;
        _userManager = userManager;
    }

    #region GET

    /// <summary>
    /// 列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] PageInputBase input)
    {
        var queryWhere = LinqExpression.And<DbBackupEntity>();
        if (!string.IsNullOrEmpty(input.keyword))
            queryWhere = queryWhere.And(m => m.FileName.Contains(input.keyword) || m.FilePath.Contains(input.keyword));
        var list = await _repository.AsQueryable().Where(queryWhere).OrderBy(x => x.CreatorTime, OrderByType.Desc).ToPagedListAsync(input.currentPage, input.pageSize);
        var pageList = new SqlSugarPagedList<DbBackupListOutput>()
        {
            list = list.list.Adapt<List<DbBackupListOutput>>(),
            pagination = list.pagination
        };
        return PageResult<DbBackupListOutput>.SqlSugarPageResult(pageList);
    }

    #endregion

    #region POST

    /// <summary>
    /// 创建备份(不支持跨库备份).
    /// </summary>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create()
    {
        await DbBackup();
    }

    /// <summary>
    /// 删除.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        if (await _repository.AnyAsync(m => m.Id == id && m.DeleteMark == null))
            throw Oops.Oh(ErrorCode.COM1005);
        await _repository.Context.Updateable<DbBackupEntity>().SetColumns(it => new DbBackupEntity()
        {
            DeleteTime = DateTime.Now,
            DeleteMark = 1,
            DeleteUserId = _userManager.UserId
        }).Where(it => it.Id.Equals(id)).ExecuteCommandAsync();
    }

    #endregion

    #region PrivateMethod

    /// <summary>
    /// 备份数据.
    /// </summary>
    private async Task DbBackup()
    {
        var fileName = SnowflakeIdHelper.NextId() + ".bak";
        var filePath = Path.Combine(FileVariable.DataBackupFilePath , fileName);

        // 备份数据
        var dataBase = App.Configuration["ConnectionStrings:DBName"];
        _repository.Context.DbMaintenance.BackupDataBase(dataBase, filePath);

        // 备份记录
        DbBackupEntity entity = new DbBackupEntity();
        entity.Id = SnowflakeIdHelper.NextId();
        entity.CreatorTime = DateTime.Now;
        entity.CreatorUserId = _userManager.UserId;
        entity.EnabledMark = 1;
        entity.FileName = fileName;
        entity.FilePath = "/api/Common/Download?encryption=" + _userManager.UserId + "|" + fileName + "|dataBackup";
        entity.FileSize = new FileInfo(filePath).Length.ToString();
        await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteReturnEntityAsync();
    }

    /// <summary>
    /// 还原.
    /// </summary>
    /// <param name="disk">路径</param>
    private void DbRestore(string disk)
    {
        var dataBase = App.Configuration["ConnectionStrings:DBName"];
        _repository.Context.DbMaintenance.CreateDatabase(dataBase, disk);
    }

    #endregion
}