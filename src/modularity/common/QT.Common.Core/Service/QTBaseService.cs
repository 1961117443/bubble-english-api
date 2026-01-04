using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QT.Common.Contracts;
using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.LinqBuilder;
using QT.Reflection.Extensions;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace QT.Common.Core;

/// <summary>
/// 业务实现基类.
/// </summary>
public abstract class QTBaseService<TEntity, TCrInput, TUpInput, TInfoOutput, TListQueryInput, TListOutput> : IDynamicApiController, ITransient 
    where TEntity: EntityBase<string>, new()
    where TListQueryInput: PageInputBase
    where TListOutput : class, new()
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    protected readonly ISqlSugarRepository<TEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    protected readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="TEntity"/>类型的新实例.
    /// </summary>
    public QTBaseService(
        ISqlSugarRepository<TEntity> repository,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = repository;
        _userManager = userManager;
    }

    /// <summary>
    /// 获取业务信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public virtual async Task<TInfoOutput> GetInfo(string id)
    {
        var entity = (await _repository.FirstOrDefaultAsync(x => x.Id == id)) ?? throw Oops.Oh(ErrorCode.COM1005);
        return entity.Adapt<TInfoOutput>();
    }

    /// <summary>
    /// 获取列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public virtual async Task<PageResult<TListOutput>> GetList([FromQuery] TListQueryInput input)
    {
        var data = await GetPageList(input);
        return PageResult<TListOutput>.SqlSugarPagedList(data);
    }

    /// <summary>
    /// 获取分页结果
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    protected virtual async Task<SqlSugarPagedList<TListOutput>> GetPageList([FromQuery] TListQueryInput input)
    {
        //foreach (var item in typeof(TListQueryInput).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly))
        //{

        //}
        return await _repository.Context.Queryable<TEntity>()
            .Select<TListOutput>()
            .ToPagedListAsync(input.currentPage, input.pageSize);
    }

    /// <summary>
    /// 获取列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("all-list")]
    public virtual async Task<List<TListOutput>> GetAllList([FromQuery] KeywordInput input)
    {
        var data = await _repository.Context.Queryable<TEntity>()
            .Select<TListOutput>()
            .Take(1000)
            .ToListAsync();
        return data;
    }

    #region 新建 Create
    /// <summary>
    /// 新建.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    [SqlSugarUnitOfWork]
    public virtual async Task Create([FromBody] TCrInput input)
    {
        var entity = input.Adapt<TEntity>();
        //// 判断车牌号码是否存在
        //if (await _repository.AnyAsync(it => it.CarNo == entity.CarNo))
        //{
        //    throw Oops.Oh(ErrorCode.COM1004);
        //}
        entity.Id = SnowflakeIdHelper.NextId();
        await BeforeCreate(input, entity);

        // 校验唯一
        var check = await ValidateEntity(entity, true);
        if (!check.Item1)
        {
            throw Oops.Oh(check.Item2);
        }

        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);

        await AfterCreate(entity);
    }

    /// <summary>
    /// 新建后
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    protected virtual Task AfterCreate(TEntity entity)
    {
        return Task.CompletedTask;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="input"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    protected virtual Task BeforeCreate(TCrInput input, TEntity entity)
    {
        return Task.CompletedTask;
    }
    #endregion

    #region 更新 Update
    /// <summary>
    /// 更新.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [SqlSugarUnitOfWork]
    public virtual async Task Update(string id, [FromBody] TUpInput input)
    {
        var entity = await _repository.SingleAsync(it => it.Id == id) ?? throw Oops.Oh(ErrorCode.COM1005);

        _repository.Context.Tracking(entity);

        await this.BeforeUpdate(input, entity);

        input.Adapt(entity);

        // 校验唯一
        var check = await ValidateEntity(entity, false);
        if (!check.Item1)
        {
            throw Oops.Oh(check.Item2);
        }

        //var changes = _repository.Context.GetChanges(entity);

        var isOk = await _repository.Context.Updateable<TEntity>(entity).ExecuteCommandAsync();
        //if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);

        await this.AfterUpdate(input, entity);
    }
    /// <summary>
    /// 更新后操作
    /// </summary>
    /// <param name="input">入参</param>
    /// <param name="entity">数据库实体</param>
    /// <returns></returns>
    protected virtual Task AfterUpdate(TUpInput input, TEntity entity)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="input"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    protected virtual Task BeforeUpdate(TUpInput input, TEntity entity)
    {
        return Task.CompletedTask;
    }
    #endregion

    #region 删除
    /// <summary>
    /// 删除.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public virtual async Task Delete(string id)
    {
        var entity = await _repository.Context.Queryable<TEntity>().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);

        int isOk = 0;
        // 判断是否可以软删除
        if (entity is CLDEntityBase entityBase)
        {
            entityBase.Delete();
            isOk = await _repository.Context.Updateable(entity).UpdateColumns(new string[] { nameof(CLDEntityBase.DeleteTime), nameof(CLDEntityBase.DeleteMark), nameof(CLDEntityBase.DeleteUserId) }).ExecuteCommandAsync();
        }
        else
        {
            isOk = await _repository.Context.Deleteable<TEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();
        }
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);


        await this.AfterDelete(entity);
    }

    /// <summary>
    /// 更新后操作
    /// </summary>
    /// <param name="input">入参</param>
    /// <param name="entity">数据库实体</param>
    /// <returns></returns>
    protected virtual Task AfterDelete(TEntity entity)
    {
        return Task.CompletedTask;
    }
    #endregion

    /// <summary>
    /// 验证实体数据
    /// </summary>
    /// <param name="mainEntity"></param>
    /// <param name="detailEntity"></param>
    /// <param name="isNew">是否新增</param>
    /// <returns></returns>
    protected virtual async Task<(bool, string)> ValidateEntity(TEntity mainEntity, bool isNew)
    {
        var errorMessage = string.Empty;

        #region 判断数据是否唯一
        if (EntityHelper<TEntity>.UniquePropertyCollection.Any())
        {
            //var key = EntityHelper<T>.GetKeyProperty();
            //var keyExp = key.CreateExpression<T>(key.GetValue(mainEntity, null), LinqExpressionType.NotEqual);

            var key = mainEntity.Id;
            var keyExp = LinqExpression.Create<TEntity>(x => x.Id != mainEntity.Id);
            foreach (var unique in EntityHelper<TEntity>.UniquePropertyCollection)
            {
                var qur = _repository.AsQueryable();
                if (!isNew)
                {
                    qur = qur.Where(x => x.Id != mainEntity.Id);
                }
                //Expression<Func<TEntity, bool>> exp = isNew ? x => true : keyExp;
                List<string> displayName = new List<string>();
                List<IConditionalModel> conditional = new List<IConditionalModel>();
                foreach (var prop in unique)
                {
                    var value = prop.GetValue(mainEntity, null);
                    displayName.Add($"{prop.GetDescription()}[{value}]");


                    conditional.Add(new ConditionalModel
                    {
                        ConditionalType = ConditionalType.Equal,
                        FieldName = prop.GetCustomAttribute<SugarColumn>()?.ColumnName ?? prop.Name,
                        FieldValueConvertFunc = (x) => value
                    });
                    //exp = exp == null ? prop.CreateExpression<TEntity>(value, LinqExpressionType.Equal) : exp.And(prop.CreateExpression<TEntity>(value, LinqExpressionType.Equal));
                }

                if (conditional.IsAny())
                {
                    qur = qur.Where(conditional);
                }

                if (await qur.AnyAsync())
                {
                    errorMessage = $"{string.Join("+", displayName)}，已存在";
                    break;
                }
                ;
                //if (exp != null)
                //{

                //    if (await _repository.AsQueryable().AnyAsync(exp))
                //    {
                //        errorMessage = $"{string.Join("+", displayName)}，已存在";
                //        break;
                //    };
                //}

            }
        }
        #endregion
        return (string.IsNullOrEmpty(errorMessage), errorMessage);
    }
}