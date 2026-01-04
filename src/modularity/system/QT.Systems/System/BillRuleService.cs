using System.Text;
using QT.Common.Const;
using QT.Common.Core.Manager;
using QT.Common.Core.Manager.Files;
using QT.Common.Enum;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.LinqBuilder;
using QT.Logging.Attributes;
using QT.Systems.Entitys.Dto.BillRule;
using QT.Systems.Entitys.Permission;
using QT.Systems.Entitys.System;
using QT.Systems.Interfaces.System;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using QT.Common.Extension;

namespace QT.Systems;

/// <summary>
/// 单据规则.
/// </summary>
[ApiDescriptionSettings(Tag = "System", Name = "BillRule", Order = 200)]
[Route("api/system/[controller]")]
public class BillRuleService : IBillRullService, IDynamicApiController, ITransient, QT.Common.Contracts.IBillRule
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<BillRuleEntity> _repository;

    /// <summary>
    /// 文件服务.
    /// </summary>
    private readonly IFileManager _fileManager;

    /// <summary>
    /// 缓存管理器.
    /// </summary>
    private readonly ICacheManager _cacheManager;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="BillRuleService"/>类型的新实例.
    /// </summary>
    public BillRuleService(
        ISqlSugarRepository<BillRuleEntity> billRuleRepository,
        ICacheManager cacheManager,
        IUserManager userManager,
        IFileManager fileManager)
    {
        _repository = billRuleRepository;
        _cacheManager = cacheManager;
        _userManager = userManager;
        _fileManager = fileManager;
    }

    #region Get

    /// <summary>
    /// 列表.
    /// </summary>
    /// <returns></returns>
    [HttpGet("Selector")]
    public async Task<dynamic> GetList()
    {
        var data = (await _repository.ToListAsync(x => x.DeleteMark == null && x.EnabledMark == 1)).OrderByDescending(x => x.CreatorTime);
        return new { list = data.Adapt<List<BillRuleListOutput>>() };
    }

    /// <summary>
    /// 获取单据规则列表(带分页).
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    [OperateLog("单据规则", "查询")]
    public async Task<dynamic> GetList([FromQuery] PageInputBase input)
    {
        var list = await _repository.Context.Queryable<BillRuleEntity, UserEntity>((a, b) => new JoinQueryInfos(JoinType.Left, a.CreatorUserId == b.Id))
            .Where(a => a.DeleteMark == null)
            .WhereIF(!string.IsNullOrEmpty(input.keyword), a => a.FullName.Contains(input.keyword) || a.EnCode.Contains(input.keyword))
            .Select((a, b) => new BillRuleListOutput
            {
                id = a.Id,
                lastModifyTime = a.LastModifyTime,
                enabledMark = a.EnabledMark,
                creatorUser = SqlFunc.MergeString(b.RealName, "/", b.Account),
                fullName = a.FullName,
                enCode = a.EnCode,
                outputNumber = a.OutputNumber,
                digit = a.Digit,
                startNumber = a.StartNumber,
                sortCode = a.SortCode,
                creatorTime = a.CreatorTime
            }).MergeTable()
            .OrderBy(a => a.sortCode).OrderBy(a => a.creatorTime, OrderByType.Desc)
            .OrderByIF(!string.IsNullOrEmpty(input.keyword), a => a.lastModifyTime, OrderByType.Desc).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<BillRuleListOutput>.SqlSugarPageResult(list);
    }

    /// <summary>
    /// 获取单据流水号（工作流程调用）.
    /// </summary>
    /// <param name="enCode">编码.</param>
    /// <returns></returns>
    [HttpGet("BillNumber/{enCode}")]
    public async Task<dynamic> GetBillNumber(string enCode)
    {
        return await GetBillNumber(enCode, true);
    }

    /// <summary>
    /// 信息.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        return (await _repository.FirstOrDefaultAsync(x => x.Id == id && x.DeleteMark == null)).Adapt<BillRuleInfoOutput>();
    }

    /// <summary>
    /// 导出.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    [HttpGet("{id}/Action/Export")]
    public async Task<dynamic> ActionsExport(string id)
    {
        var data = await _repository.FirstOrDefaultAsync(x => x.Id == id && x.DeleteMark == null);
        var jsonStr = data.ToJsonString();
        return await _fileManager.Export(jsonStr, data.FullName, ExportFileType.bb);
    }

    #endregion

    #region Post

    /// <summary>
    /// 新建.
    /// </summary>
    /// <param name="input">实体对象.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] BillRuleCrInput input)
    {
        if (await _repository.AnyAsync(x => (x.EnCode == input.enCode || x.FullName == input.fullName) && x.DeleteMark == null))
            throw Oops.Oh(ErrorCode.COM1004);
        var entity = input.Adapt<BillRuleEntity>();
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
        if (isOk < 1)
            throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 修改.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">实体对象.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] BillRuleUpInput input)
    {
        if (await _repository.AnyAsync(x => x.Id != id && (x.EnCode == input.enCode || x.FullName == input.fullName) && x.DeleteMark == null))
            throw Oops.Oh(ErrorCode.COM1004);
        var entity = input.Adapt<BillRuleEntity>();
        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var entity = await _repository.FirstOrDefaultAsync(x => x.Id == id && x.DeleteMark == null);
        if (entity == null)
            throw Oops.Oh(ErrorCode.COM1005);
        if (!string.IsNullOrEmpty(entity.OutputNumber))
            throw Oops.Oh(ErrorCode.BR0001);
        var isOk = await _repository.Context.Updateable(entity).CallEntityMethod(m => m.Delete()).UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId }).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1002);
    }

    /// <summary>
    /// 修改单据规则状态.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpPut("{id}/Actions/State")]
    public async Task ActionsState(string id)
    {
        var isOk = await _repository.Context.Updateable<BillRuleEntity>().SetColumns(it => new BillRuleEntity()
        {
            EnabledMark = SqlFunc.IIF(it.EnabledMark == 1, 0, 1),
            LastModifyUserId = _userManager.UserId,
            LastModifyTime = SqlFunc.GetDate()
        }).Where(it => it.Id.Equals(id)).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1003);
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
        if (!fileType.ToLower().Equals(ExportFileType.bb.ToString()))
            throw Oops.Oh(ErrorCode.D3006);
        var josn = _fileManager.Import(file);
        var data = josn.ToObject<BillRuleEntity>();
        if (data == null || data.Prefix.IsNullOrEmpty())
            throw Oops.Oh(ErrorCode.D3006);
        var isOk = await _repository.Context.Storageable(data).ExecuteCommandAsync();
        if (isOk < 1)
            throw Oops.Oh(ErrorCode.D3008);
    }
    #endregion

    #region PublicMethod

    /// <summary>
    /// 获取流水号.
    /// </summary>
    /// <param name="enCode">流水编码.</param>
    /// <param name="isCache">是否缓存：每个用户会自动占用一个流水号，这个刷新页面也不会跳号.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<string> GetBillNumber(string enCode, bool isCache = false)
    {
        string cacheKey = string.Format("{0}{1}_{2}", CommonConst.CACHEKEYBILLRULE, _userManager.TenantId, _userManager.UserId + enCode);
        string strNumber = string.Empty;
        if (isCache)
        {
            if (!_cacheManager.Exists(cacheKey))
            {
                strNumber = await GetNumber(enCode);
                await _cacheManager.SetAsync(cacheKey, strNumber, new TimeSpan(0, 3, 0));
            }
            else
            {
                strNumber = await _cacheManager.GetAsync(cacheKey);
            }
        }
        else
        {
            strNumber = await GetNumber(enCode);
            string cacheKey1 = string.Format("{0}{1}_{2}_{3}", CommonConst.CACHEKEYBILLRULE, _userManager.TenantId, enCode, strNumber);
            // 判断单号是否已存在
            var cacheValue = await _cacheManager.GetAsync<string>(cacheKey1);
            if (cacheValue.IsNotEmptyOrNull() && !cacheValue.Equals(_userManager.UserId))
            {
                throw Oops.Oh("单号获取失败，请稍后再试！");
            }
            _cacheManager.Set(cacheKey1, _userManager.UserId, TimeSpan.FromMinutes(1));
            _cacheManager.Set(cacheKey, strNumber, new TimeSpan(0, 3, 0));
        }

        return strNumber;
    }

    #endregion

    #region PrivateMethod

    /// <summary>
    /// 获取流水号.
    /// </summary>
    /// <param name="enCode"></param>
    /// <returns></returns>
    private async Task<string> GetNumber(string enCode)
    {
        StringBuilder strNumber = new StringBuilder();
        BillRuleEntity? entity = null;
        // 判断是否有公司独立的流水号，根据公司编号作为前缀： {公司编号}_{enCode}
        var organizeEnCode = await _cacheManager.GetOrCreateAsync<string>($"u:{_userManager.UserId}:o:{_userManager.User.OrganizeId}:enCode", async (entry) =>
        {
            return await _repository.Context.Queryable<OrganizeEntity>().Where(it => it.Id == _userManager.User.OrganizeId).Select(it => it.EnCode).FirstAsync();
        });
        //var organizeEnCode = await _repository.Context.Queryable<OrganizeEntity>().Where(it => it.Id == _userManager.User.OrganizeId).Select(it => it.EnCode).FirstAsync();
        if (organizeEnCode.IsNotEmptyOrNull())
        {
            var org = $"{organizeEnCode}_{enCode}";
            var list = await _repository.Where(m => (m.EnCode == enCode || m.EnCode == org) && m.DeleteMark == null).ToListAsync();
            entity = list.Find(x => x.EnCode == org) ?? list.FirstOrDefault();
        }
        else
        {
            entity = await _repository.FirstOrDefaultAsync(m => m.EnCode == enCode && m.DeleteMark == null);
        }        
        if (entity != null)
        {
            //_repository.Context.Tracking(entity);
            // 处理隔天流水号归0
            if (entity.OutputNumber != null)
            {
                var serialDate = entity.OutputNumber.Remove(entity.OutputNumber.Length - (int)entity.Digit).Replace(entity.Prefix, string.Empty);
                var thisDate = entity.DateFormat == "no" ? string.Empty : DateTime.Now.ToString(entity.DateFormat);
                if (serialDate != thisDate)
                    entity.ThisNumber = 0;
                entity.ThisNumber++;
            }
            else
            {
                entity.ThisNumber = 0;
            }

            // 拼接单据编码
            // 前缀
            strNumber.Append(entity.Prefix);
            if (entity.DateFormat != "no")
                strNumber.Append(DateTime.Now.ToString(entity.DateFormat)); // 日期格式

            var number = int.Parse(entity.StartNumber) + entity.ThisNumber;
            strNumber.Append(number.ToString().PadLeft((int)entity.Digit, '0')); // 流水号
            entity.OutputNumber = strNumber.ToString();
            var str = strNumber.ToString();
            // 更新流水号
            var query = _repository.Context.Updateable(entity)
                .CallEntityMethod(m => m.LastModify())
                .UpdateColumns(new string[] { nameof(BillRuleEntity.OutputNumber), nameof(BillRuleEntity.ThisNumber), nameof(BillRuleEntity.LastModifyUserId), nameof(BillRuleEntity.LastModifyTime) });

            var sql = query.ToSqlString();

            await _repository.Ado.ExecuteCommandAsync(sql);

            //await _repository.Context.Updateable(entity)
            //    .CallEntityMethod(m => m.LastModify())
            //    .UpdateColumns(new string[] { nameof(BillRuleEntity.OutputNumber), nameof(BillRuleEntity.ThisNumber) ,nameof(BillRuleEntity.LastModifyUserId),nameof(BillRuleEntity.LastModifyTime) })
            //    .ExecuteCommandHasChangeAsync();
        }
        else
        {
            //strNumber.Append("单据规则不存在");
            strNumber.Append(DateTime.Now.ToString("yyyyMMddHHmmssfff"));
        }

        return strNumber.ToString();
    }
    #endregion
}