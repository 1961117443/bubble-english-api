using System.Text;
using QT.Common.Const;
using QT.Common.Core.Manager;
using QT.Common.Dtos.VisualDev;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Models.VisualDev;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.FriendlyException;
using QT.Systems.Entitys.Model.DataBase;
using QT.Systems.Entitys.Permission;
using QT.Systems.Entitys.System;
using QT.Systems.Interfaces.System;
using QT.VisualDev.Engine;
using QT.VisualDev.Engine.Core;
using QT.VisualDev.Entitys;
using QT.VisualDev.Entitys.Dto.VisualDevModelData;
using QT.VisualDev.Interfaces;
using QT.WorkFlow.Entitys.Entity;
using QT.WorkFlow.Interfaces.Manager;
using QT.WorkFlow.Interfaces.Repository;
using Mapster;
using Newtonsoft.Json.Linq;
using SqlSugar;
using Yitter.IdGenerator;
using QT.UnifyResult;
using QT.DataEncryption;

namespace QT.VisualDev;

/// <summary>
/// 在线开发运行服务.
/// </summary>
public class RunService : IRunService, ITransient
{
    #region 构造

    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<VisualDevEntity> _visualDevRepository;  // 在线开发功能实体

    /// <summary>
    /// 表单数据解析.
    /// </summary>
    private readonly FormDataParsing _formDataParsing;

    /// <summary>
    /// 切库.
    /// </summary>
    private readonly IDataBaseManager _databaseService;

    /// <summary>
    /// 单据.
    /// </summary>
    private readonly IBillRullService _billRuleService;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 数据连接服务.
    /// </summary>
    private readonly IDbLinkService _dbLinkService;

    /// <summary>
    /// 多租户事务.
    /// </summary>
    private readonly ITenant _db;

    /// <summary>
    /// 构造.
    /// </summary>
    public RunService(
        ISqlSugarRepository<VisualDevEntity> visualDevRepository,
        FormDataParsing formDataParsing,
        IUserManager userManager,
        IDbLinkService dbLinkService,
        IDataBaseManager databaseService,
        IBillRullService billRuleService,
        ISqlSugarClient context)
    {
        _visualDevRepository = visualDevRepository;
        _formDataParsing = formDataParsing;
        _userManager = userManager;
        _databaseService = databaseService;
        _dbLinkService = dbLinkService;
        _billRuleService = billRuleService;
        _db = context.AsTenant();
    }
    #endregion

    #region Get

    /// <summary>
    /// 列表数据处理.
    /// </summary>
    /// <param name="entity">功能实体.</param>
    /// <param name="input">查询参数.</param>
    /// <param name="actionType"></param>
    /// <returns></returns>
    public async Task<PageResult<Dictionary<string, object>>> GetListResult(VisualDevEntity entity, VisualDevModelListQueryInput input, string actionType = "List")
    {
        PageResult<Dictionary<string, object>>? realList = new PageResult<Dictionary<string, object>>() { list = new List<Dictionary<string, object>>() }; // 返回结果集
        List<VisualDevModelDataEntity>? list = new List<VisualDevModelDataEntity>(); // 查询结果集
        TemplateParsingBase templateInfo = new TemplateParsingBase(entity); // 解析模板控件
        input.queryJson = templateInfo.TimeControlQueryConvert(input.queryJson); // 时间控件 查询 处理

        string? primaryKey = "F_Id"; // 列表主键

        if (templateInfo.IsHasTable)
        {
            DbLinkEntity link = await _dbLinkService.GetInfo(entity.DbLinkId);
            if (link == null) link = _databaseService.GetTenantDbLink(_userManager.TenantId, _userManager.TenantDbName); // 当前数据库连接
            string? dbType = link?.DbType != null ? link.DbType : _visualDevRepository.Context.CurrentConnectionConfig.DbType.ToString(); // 当前数据库类型
            List<DbTableFieldModel>? tableList = _databaseService.GetFieldList(link, templateInfo.MainTableName); // 获取主表所有列
            DbTableFieldModel? mainPrimary = tableList.Find(t => t.primaryKey); // 主表主键
            if (mainPrimary != null && mainPrimary.IsNullOrEmpty()) throw Oops.Oh(ErrorCode.D1402); // 主表未设置主键
            if (mainPrimary != null) primaryKey = mainPrimary.field;
            StringBuilder feilds = new StringBuilder();
            string? sql = string.Empty; // 查询sql

            // 合计处理
            StringBuilder sumfields = new StringBuilder();

            Dictionary<string, string>? tableFieldKeyValue = new Dictionary<string, string>(); // 联表查询 表字段名称 对应 前端字段名称 (应对oracle 查询字段长度不能超过30个)

            // 是否存在副表 , 没有副表 只查询主表
            if (templateInfo.AuxiliaryTableFieldsModelList.Count < 1)
            {
                if (tableList.Any(x => x.field.Equals(primaryKey, StringComparison.OrdinalIgnoreCase)))
                {
                    feilds.AppendFormat("{0},", primaryKey);
                }

                // 只查询 要显示的列
                if (templateInfo.SingleFormData.Count > 0 || templateInfo.ColumnData.columnList.Count > 0)
                {
                    templateInfo.ColumnData.columnList.ForEach(item =>
                    {
                        if (!item.prop.Equals(primaryKey, StringComparison.OrdinalIgnoreCase)
                        && templateInfo.ColumnData.defaultColumnList.Any(x => x.prop == item.prop))
                        {
                            feilds.AppendFormat("{0},", item.prop);
                            tableFieldKeyValue.Add(item.prop, item.prop);

                            if (item.summary)
                            {
                                sumfields.AppendFormat("sum({0}) as `{0}`,", item.prop);
                            }

                        }
                    });
                    feilds = new StringBuilder(feilds.ToString().TrimEnd(','));
                }
                else
                {
                    tableList.ForEach(item =>
                    {
                        if (templateInfo.MainTableFieldsModelList.Find(x => x.__vModel__ == item.field) != null) feilds.AppendFormat("{0},", item.field); // 主表列名
                        tableFieldKeyValue.Add(item.field, item.field);
                    });

                    feilds = new StringBuilder(feilds.ToString().TrimEnd(','));
                }

                sql = string.Format("select {0} from {1}", feilds, templateInfo.MainTableName);
            }
            else
            {
                #region 所有主、副表 字段名 和 处理查询、排序字段

                // 所有主、副表 字段名
                List<string>? cFieldNameList = new List<string>();
                cFieldNameList.Add(templateInfo.MainTableName + "." + primaryKey);
                tableFieldKeyValue.Add(primaryKey.ToUpper(), primaryKey);
                Dictionary<string, object>? inputJson = input.queryJson?.ToObject<Dictionary<string, object>>();
                for (int i = 0; i < templateInfo.SingleFormData.Count; i++)
                {
                    // 只显示要显示的列
                    if (templateInfo.ColumnData.columnList.Any(x => x.prop == templateInfo.SingleFormData[i].__vModel__))
                    {
                        string? vmodel = templateInfo.SingleFormData[i].__vModel__.ReplaceRegex(@"(\w+)_qt_", string.Empty); // Field

                        if (vmodel.IsNotEmptyOrNull())
                        {
                            cFieldNameList.Add(templateInfo.SingleFormData[i].__config__.tableName + "." + vmodel + " FIELD_" + i); // TableName.Field_0
                            tableFieldKeyValue.Add("FIELD_" + i, templateInfo.SingleFormData[i].__vModel__);

                            // 查询字段替换
                            if (inputJson != null && inputJson.Count > 0 && inputJson.ContainsKey(templateInfo.SingleFormData[i].__vModel__))
                                input.queryJson = input.queryJson.Replace(templateInfo.SingleFormData[i].__vModel__ + "\":", "FIELD_" + i + "\":");

                            templateInfo.ColumnData.searchList.Where(x => x.__vModel__ == templateInfo.SingleFormData[i].__vModel__).ToList().ForEach(item =>
                            {
                                item.__vModel__ = item.__vModel__.Replace(templateInfo.SingleFormData[i].__vModel__, "FIELD_" + i);
                            });

                            // 排序字段替换
                            if (templateInfo.ColumnData.defaultSidx.IsNotEmptyOrNull() && templateInfo.ColumnData.defaultSidx == vmodel)
                                templateInfo.ColumnData.defaultSidx = "FIELD_" + i;

                            if (input.sidx.IsNotEmptyOrNull() && input.sidx == vmodel) input.sidx = "FIELD_" + i;
                        }
                    }
                }

                feilds.Append(string.Join(",", cFieldNameList));

                #endregion

                #region 关联字段

                List<string>? relationKey = new List<string>();
                List<string>? auxiliaryFieldList = templateInfo.AuxiliaryTableFieldsModelList.Select(x => x.__config__.tableName).Distinct().ToList();
                auxiliaryFieldList.ForEach(tName =>
                {
                    string? tableField = templateInfo.AllTable.Find(tf => tf.table == tName)?.tableField;
                    relationKey.Add(templateInfo.MainTableName + "." + primaryKey + "=" + tName + "." + tableField);
                });
                string? whereStr = string.Join(" and ", relationKey);

                #endregion

                sql = string.Format("select {0} from {1} where {2}", feilds, templateInfo.MainTableName + "," + string.Join(",", auxiliaryFieldList), whereStr); // 多表， 联合查询
            }

            // 如果排序字段没有在显示列中，按默认排序
            if (input.sidx.IsNotEmptyOrNull() && !templateInfo.ColumnData.columnList.Any(x => x.prop == input.sidx)) input.sidx = string.Empty;

            // 获取请求端类型，并对应获取请求端数据权限
            bool udp = _userManager.UserOrigin == "pc" ? templateInfo.ColumnData.useDataPermission : templateInfo.AppColumnData.useDataPermission;

            // 获取数据权限
            List<IConditionalModel>? pvalue = _userManager.GetCondition<Dictionary<string, object>>(primaryKey, input.menuId, udp);
            List<IConditionalModel>? newPvalue = new List<IConditionalModel>();

            pvalue.ForEach(item => // 数据权限 字段 别名对应替换
            {
                var vItems = ((ConditionalCollections)item).ConditionalList;
                var cdList = new List<KeyValuePair<WhereType, ConditionalModel>>();
                vItems.ForEach(vItem =>
                {
                    if (vItem.Value.FieldName.Contains(".")) vItem.Value.FieldName = "qt_" + vItem.Value.FieldName.Replace(".", "_qt_"); // 别名处理

                    if (tableFieldKeyValue.ContainsValue(vItem.Value.FieldName))
                    {
                        ConditionalModel? vValue = new ConditionalModel();
                        vValue.FieldName = tableFieldKeyValue.FirstOrDefault(x => x.Value == vItem.Value.FieldName).Key;
                        vValue.FieldValue = vItem.Value.FieldValue;
                        vValue.CSharpTypeName = vItem.Value.CSharpTypeName;
                        vValue.ConditionalType = vItem.Value.ConditionalType;
                        vValue.FieldValueConvertFunc = vItem.Value.FieldValueConvertFunc;
                        cdList.Add(new KeyValuePair<WhereType, ConditionalModel>(vItem.Key, vValue));
                    }
                });

                newPvalue.Add(new ConditionalCollections() { ConditionalList = cdList });
            });
            if (templateInfo.AuxiliaryTableFieldsModelList.Count < 1) tableFieldKeyValue.Clear();

            var columnDesign = templateInfo.ColumnData.Adapt<MainBeltViceQueryModel>();
            columnDesign.mainTableName = templateInfo.MainTableName;
            realList = _databaseService.GetInterFaceData(link, sql, input, columnDesign, newPvalue, tableFieldKeyValue);

            // 2025.2.26 取消汇总统计
            var config = await _visualDevRepository.Context.Queryable<SysConfigEntity>().Where(x => x.Key == "DisableSumField" && x.Value == "1").WithCache(120).FirstAsync();
            if (sumfields.Length > 0 && config == null)
            {
                var conditions = _databaseService.GetSqlCondition(input, columnDesign);
                //var sqlstr = _visualDevRepository.Context.SqlQueryable<object>(sql).Where(conditions).Where(newPvalue).ToSqlString();

                var summary = await _visualDevRepository.Context.SqlQueryable<object>(sql).Where(conditions).Where(newPvalue).MergeTable()
                    .Select(sumfields.ToString().TrimEnd(','))
                    .FirstAsync();
                //string? sumSql = string.Format("select {0} from ({1}) t", sumfields.ToString().TrimEnd(','), sqlstr);
                //string cacheKey = MD5Encryption.Encrypt(sumSql);
                //var summary = await _visualDevRepository.Context.SqlQueryable<object>(sumSql).WithCache(cacheKey, 120).FirstAsync();

                UnifyContext.Fill(new { tableDataSummary = summary });
            }
        }
        else
        {
            list = await _visualDevRepository.Context.Queryable<VisualDevModelDataEntity>().Where(m => m.VisualDevId == entity.Id && m.DeleteMark == null).ToListAsync();
        }

        input.sidx = string.IsNullOrEmpty(input.sidx) ? (templateInfo.ColumnData.defaultSidx == string.Empty ? primaryKey : templateInfo.ColumnData.defaultSidx) : input.sidx;

        if (list.Any() || realList.list.Any())
        {
            if ((!string.IsNullOrEmpty(entity.Tables) && "[]".Equals(entity.Tables)) || string.IsNullOrEmpty(entity.Tables))
            {
                Dictionary<string, object>? keywordJsonDic = string.IsNullOrEmpty(input.queryJson) ? null : input.queryJson.ToObject<Dictionary<string, object>>(); // 将查询的关键字json转成Dictionary

                // 关键字过滤
                realList.list = _formDataParsing.GetNoTableFilteringData(list, keywordJsonDic, templateInfo.FieldsModelList);
                realList.list = await _formDataParsing.GetKeyData(templateInfo.SingleFormData, realList.list, templateInfo.ColumnData, actionType, entity.WebType.ParseToInt(), primaryKey);
            }
            else
            {
                realList.list = await _formDataParsing.GetKeyData(templateInfo.SingleFormData, realList.list, templateInfo.ColumnData, actionType, entity.WebType.ParseToInt(), primaryKey);
            }

            // 如果是无表数据并且排序字段不为空，再进行数据排序
            if (!templateInfo.IsHasTable && input.sidx.IsNotEmptyOrNull())
            {
                if (input.sort == "desc")
                {
                    realList.list = realList.list.OrderByDescending(x =>
                    {
                        var dic = x as IDictionary<string, object>;
                        dic.GetOrAdd(input.sidx, () => null);
                        return dic[input.sidx];
                    }).ToList();
                }
                else
                {
                    realList.list = realList.list.OrderBy(x =>
                    {
                        var dic = x as IDictionary<string, object>;
                        dic.GetOrAdd(input.sidx, () => null);
                        return dic[input.sidx];
                    }).ToList();
                }
            }
        }

        if (input.dataType == "0")
        {
            if (!string.IsNullOrEmpty(entity.Tables) && !"[]".Equals(entity.Tables))
            {
            }
            else
            {
                realList.pagination = new PageResult();
                realList.pagination.total = realList.list.Count;
                realList.pagination.pageSize = input.pageSize;
                realList.pagination.pageIndex = input.currentPage;
                realList.list = realList.list.Skip(input.pageSize * (input.currentPage - 1)).Take(input.pageSize).ToList();
            }

            // 分组表格
            if (templateInfo.ColumnData.type == 3)
            {
                var groupList = templateInfo.ColumnData.columnList.Where(p => p.prop == templateInfo.ColumnData.groupField).ToList();
                var exceptList = templateInfo.ColumnData.columnList.Except(groupList).FirstOrDefault();

                // 分组数据
                Dictionary<string, List<Dictionary<string, object>>> groupDic = new Dictionary<string, List<Dictionary<string, object>>>();
                foreach (var item in realList.list)
                {
                    if (item.ContainsKey(templateInfo.ColumnData.groupField))
                    {
                        var groupDicKey = item[templateInfo.ColumnData.groupField] is null ? string.Empty : item[templateInfo.ColumnData.groupField].ToString();
                        if (!groupDic.ContainsKey(groupDicKey)) groupDic.Add(groupDicKey, new List<Dictionary<string, object>>()); // 初始化
                        item.Remove(templateInfo.ColumnData.groupField);
                        groupDic[groupDicKey].Add(item);
                    }
                    else
                    {
                        var groupDicKey = "null";
                        if (!groupDic.ContainsKey(groupDicKey)) groupDic.Add(groupDicKey, new List<Dictionary<string, object>>()); // 初始化
                        groupDic[groupDicKey].Add(item);
                    }

                }
                List<Dictionary<string, object>> realGroupDic = new List<Dictionary<string, object>>();
                foreach (var item in groupDic)
                {
                    Dictionary<string, object> dataMap = new Dictionary<string, object>();
                    dataMap.Add("top", true);
                    dataMap.Add("id", YitIdHelper.NextId().ToString());
                    dataMap.Add("children", item.Value);
                    if (!exceptList.IsEmpty() && !string.IsNullOrWhiteSpace(exceptList.prop)) dataMap.Add(exceptList.prop, item.Key);
                    else dataMap.Add(templateInfo.ColumnData.groupField, item.Key);
                    realGroupDic.Add(dataMap);
                }
                realList.list = realGroupDic;
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(entity.Tables) && !"[]".Equals(entity.Tables))
            {
            }
            else
            {
                realList.pagination = new PageResult();
                realList.pagination.total = realList.list.Count;
                realList.pagination.pageSize = input.pageSize;
                realList.pagination.pageIndex = input.currentPage;
                realList.list = realList.list.ToList();
            }
        }

        return realList;
    }

    /// <summary>
    /// 关联表单列表数据处理.
    /// </summary>
    /// <param name="entity">功能实体.</param>
    /// <param name="input">查询参数.</param>
    /// <param name="actionType"></param>
    /// <returns></returns>
    public async Task<PageResult<Dictionary<string, object>>> GetRelationFormList(VisualDevEntity entity, VisualDevModelListQueryInput input, string actionType = "List")
    {
        PageResult<Dictionary<string, object>>? realList = new PageResult<Dictionary<string, object>>() { list = new List<Dictionary<string, object>>() }; // 返回结果集
        List<VisualDevModelDataEntity>? list = new List<VisualDevModelDataEntity>(); // 查询结果集
        TemplateParsingBase? templateInfo = new TemplateParsingBase(entity); // 解析模板控件
        input.queryJson = templateInfo.TimeControlQueryConvert(input.queryJson); // 时间控件 查询 处理
        string? primaryKey = "F_Id"; // 列表主键

        if (templateInfo.IsHasTable)
        {
            DbLinkEntity? link = await _dbLinkService.GetInfo(entity.DbLinkId);
            if (link == null) link = _databaseService.GetTenantDbLink(_userManager.TenantId, _userManager.TenantDbName); // 当前数据库连接
            string? dbType = link?.DbType != null ? link.DbType : _visualDevRepository.Context.CurrentConnectionConfig.DbType.ToString(); // 当前数据库类型
            List<DbTableFieldModel>? tableList = _databaseService.GetFieldList(link, templateInfo.MainTableName); // 获取主表所有列
            DbTableFieldModel? mainPrimary = tableList.Find(t => t.primaryKey); // 主表主键
            if (mainPrimary.IsNullOrEmpty()) throw Oops.Oh(ErrorCode.D1402); // 主表未设置主键
            primaryKey = mainPrimary.field;
            StringBuilder feilds = new StringBuilder();
            string? sql = string.Empty; // 查询sql

            Dictionary<string, string>? tableFieldKeyValue = new Dictionary<string, string>(); // 联表查询 表字段名称 对应 前端字段名称 (应对oracle 查询字段长度不能超过30个)

            // 是否存在副表 没有副表 只查询主表
            if (templateInfo.AuxiliaryTableFieldsModelList.Count < 1)
            {
                feilds.AppendFormat("{0},", primaryKey);

                // 只查询 要显示的列
                if (templateInfo.SingleFormData.Count > 0 || templateInfo.ColumnData.columnList.Count > 0)
                {
                    templateInfo.ColumnData.columnList.ForEach(item =>
                    {
                        feilds.AppendFormat("{0},", item.prop);
                    });
                    feilds = new StringBuilder(feilds.ToString().TrimEnd(','));
                }
                else
                {
                    tableList.ForEach(item =>
                    {
                        // 主表列名
                        if (templateInfo.MainTableFieldsModelList.Find(x => x.__vModel__ == item.field) != null) feilds.AppendFormat("{0},", item.field);
                    });

                    feilds = new StringBuilder(feilds.ToString().TrimEnd(','));
                }

                sql = string.Format("select {0} from {1}", feilds, templateInfo.MainTableName);
            }
            else
            {
                #region 所有主、副表 字段名 和 处理查询、排序字段

                // 所有主、副表 字段名
                List<string>? cFieldNameList = new List<string>();
                cFieldNameList.Add(templateInfo.MainTableName + "." + primaryKey);
                tableFieldKeyValue.Add(primaryKey.ToUpper(), primaryKey);
                Dictionary<string, object>? inputJson = input.queryJson?.ToObject<Dictionary<string, object>>();
                for (int i = 0; i < templateInfo.SingleFormData.Count; i++)
                {
                    // 只显示要显示的列
                    if (templateInfo.ColumnData.columnList.Any(x => x.prop == templateInfo.SingleFormData[i].__vModel__))
                    {
                        // Field
                        string? vmodel = templateInfo.SingleFormData[i].__vModel__.ReplaceRegex(@"(\w+)_qt_", string.Empty);

                        if (vmodel.IsNotEmptyOrNull())
                        {
                            cFieldNameList.Add(templateInfo.SingleFormData[i].__config__.tableName + "." + vmodel + " FIELD_" + i); // TableName.Field_0
                            tableFieldKeyValue.Add("FIELD_" + i, templateInfo.SingleFormData[i].__vModel__);

                            // 查询字段替换
                            if (inputJson != null && inputJson.Count > 0 && inputJson.ContainsKey(templateInfo.SingleFormData[i].__vModel__))
                                input.queryJson = input.queryJson.Replace(templateInfo.SingleFormData[i].__vModel__ + "\":", "FIELD_" + i + "\":");

                            templateInfo.ColumnData.searchList.Where(x => x.__vModel__ == templateInfo.SingleFormData[i].__vModel__).ToList().ForEach(item =>
                            {
                                item.__vModel__ = item.__vModel__.Replace(templateInfo.SingleFormData[i].__vModel__, "FIELD_" + i);
                            });

                            // 排序字段替换
                            if (templateInfo.ColumnData.defaultSidx.IsNotEmptyOrNull() && templateInfo.ColumnData.defaultSidx == vmodel)
                                templateInfo.ColumnData.defaultSidx = "FIELD_" + i;

                            if (input.sidx.IsNotEmptyOrNull() && input.sidx == vmodel) input.sidx = "FIELD_" + i;
                        }
                    }
                }

                feilds.Append(string.Join(",", cFieldNameList));

                #endregion

                #region 关联字段

                List<string>? relationKey = new List<string>();
                List<string>? auxiliaryFieldList = templateInfo.AuxiliaryTableFieldsModelList.Select(x => x.__config__.tableName).Distinct().ToList();
                auxiliaryFieldList.ForEach(tName =>
                {
                    string? tableField = templateInfo.AllTable.Find(tf => tf.table == tName)?.tableField;
                    relationKey.Add(templateInfo.MainTableName + "." + primaryKey + "=" + tName + "." + tableField);
                });
                string? whereStr = string.Join(" and ", relationKey);
                #endregion

                sql = string.Format("select {0} from {1} where {2}", feilds, templateInfo.MainTableName + "," + string.Join(",", auxiliaryFieldList), whereStr); // 多表， 联合查询
            }

            // 如果排序字段没有在显示列中，按默认排序
            if (input.sidx.IsNotEmptyOrNull() && !templateInfo.ColumnData.columnList.Any(x => x.prop == input.sidx)) input.sidx = string.Empty;

            // 获取请求端类型，并对应获取请求端数据权限
            bool udp = _userManager.UserOrigin == "pc" ? templateInfo.ColumnData.useDataPermission : templateInfo.AppColumnData.useDataPermission;

            List<IConditionalModel>? pvalue = new List<IConditionalModel>(); // 关联表单调用 数据全部放开
            string? queryJson = input.queryJson;
            input.queryJson = string.Empty;

            var columnDesign = templateInfo.ColumnData.Adapt<MainBeltViceQueryModel>();
            columnDesign.mainTableName = templateInfo.MainTableName;
            realList = _databaseService.GetInterFaceData(link, sql, input, columnDesign, pvalue, tableFieldKeyValue);
            input.queryJson = queryJson;
        }
        else
        {
            list = await _visualDevRepository.Context.Queryable<VisualDevModelDataEntity>().Where(m => m.VisualDevId == entity.Id && m.DeleteMark == null).ToListAsync();
        }

        input.sidx = string.IsNullOrEmpty(input.sidx) ? (templateInfo.ColumnData.defaultSidx == string.Empty ? primaryKey : templateInfo.ColumnData.defaultSidx) : input.sidx;

        if (list.Any() || realList.list.Any())
        {
            if ((!string.IsNullOrEmpty(entity.Tables) && "[]".Equals(entity.Tables)) || string.IsNullOrEmpty(entity.Tables))
            {
                Dictionary<string, object>? keywordJsonDic = string.IsNullOrEmpty(input.queryJson) ? null : input.queryJson.ToObject<Dictionary<string, object>>(); // 将查询的关键字json转成Dictionary

                // 关键字过滤
                realList.list = _formDataParsing.GetNoTableFilteringData(list, keywordJsonDic, templateInfo.FieldsModelList);
                realList.list = await _formDataParsing.GetKeyData(templateInfo.SingleFormData, realList.list, templateInfo.ColumnData, actionType, entity.WebType.ParseToInt(), primaryKey);
            }
            else
            {
                realList.list = await _formDataParsing.GetKeyData(templateInfo.SingleFormData, realList.list, templateInfo.ColumnData, actionType, entity.WebType.ParseToInt(), primaryKey);

                if (input.queryJson.IsNotEmptyOrNull())
                {
                    Dictionary<string, string>? search = input.queryJson.ToObject<Dictionary<string, string>>();
                    if (search.FirstOrDefault().Value.IsNotEmptyOrNull())
                    {
                        string? keword = search.FirstOrDefault().Value;
                        List<Dictionary<string, object>>? newList = new List<Dictionary<string, object>>();
                        List<string>? columnName = templateInfo.ColumnData.columnList.Select(x => x.prop).ToList();
                        realList.list.ForEach(item =>
                        {
                            if (item.Where(x => x.Value != null && columnName.Contains(x.Key)).Where(x => x.Value.ToString().Contains(keword)).Any()) newList.Add(item);
                        });

                        realList.list = newList;
                    }
                }
            }

            if (input.sort == "desc")
            {
                realList.list = realList.list.OrderByDescending(x =>
                {
                    IDictionary<string, object>? dic = x as IDictionary<string, object>;
                    dic.GetOrAdd(input.sidx, () => null);
                    return dic[input.sidx];
                }).ToList();
            }
            else
            {
                realList.list = realList.list.OrderBy(x =>
                {
                    IDictionary<string, object>? dic = x as IDictionary<string, object>;
                    dic.GetOrAdd(input.sidx, () => null);
                    return dic[input.sidx];
                }).ToList();
            }
        }

        if (!string.IsNullOrEmpty(entity.Tables) && !"[]".Equals(entity.Tables))
        {
        }
        else
        {
            realList.pagination = new PageResult();
            realList.pagination.total = realList.list.Count;
            realList.pagination.pageSize = input.pageSize;
            realList.pagination.pageIndex = input.currentPage;
            realList.list = realList.list.ToList();
        }

        return realList;
    }

    /// <summary>
    /// 获取模型数据信息.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<VisualDevModelDataEntity> GetInfo(string id)
    {
        return await _visualDevRepository.Context.Queryable<VisualDevModelDataEntity>().FirstAsync(m => m.Id == id);
    }

    /// <summary>
    /// 获取无表详情.
    /// </summary>
    /// <param name="templateEntity">模板实体</param>
    /// <param name="data">真实数据</param>
    /// <returns></returns>
    public async Task<Dictionary<string, object>> GetIsNoTableInfo(VisualDevEntity templateEntity, string data)
    {
        // 解析系统控件
        return await _formDataParsing.GetSystemComponentsData(new TemplateParsingBase(templateEntity).SingleFormData, data);
    }

    /// <summary>
    /// 获取有表详情.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <param name="templateEntity">模板实体.</param>
    /// <returns></returns>
    public async Task<Dictionary<string, object>> GetHaveTableInfo(string id, VisualDevEntity templateEntity)
    {
        TemplateParsingBase? templateInfo = new TemplateParsingBase(templateEntity); // 解析模板控件
        string? sql = string.Empty; // 联表查询sql
        List<string>? feildList = new List<string>(); // 存放所有查询列名

        DbLinkEntity? link = await _dbLinkService.GetInfo(templateEntity.DbLinkId);
        if (link == null) link = _databaseService.GetTenantDbLink(_userManager.TenantId, _userManager.TenantDbName); // 存储当前数据连接
        List<DbTableFieldModel>? tableList = _databaseService.GetFieldList(link, templateInfo.MainTableName); // 获取主表 所有字段名
        string? mainPrimary = tableList.Find(t => t.primaryKey)?.field; // 主表主键
        if (mainPrimary.IsNullOrEmpty()) throw Oops.Oh(ErrorCode.D1402); // 主表未设置主键

        Dictionary<string, string>? tableFieldKeyValue = new Dictionary<string, string>(); // 联表查询 表字段 别名

        // 没有副表,只查询主表
        if (!templateInfo.AuxiliaryTableFieldsModelList.Any())
        {
            feildList.Add(mainPrimary); // 主表主键
            for (int i = 0; i < tableList.Count; i++) if (templateInfo.MainTableFieldsModelList.Any(x => x.__vModel__ == tableList[i].field)) feildList.Add(tableList[i].field); // 主表列名
            sql = string.Format("select {0} from {1} where {2}='{3}'", string.Join(",", feildList), templateInfo.MainTableName, mainPrimary, id);
        }
        else
        {
            #region 所有主表、副表 字段名

            feildList.Add(templateInfo.MainTableName + "." + mainPrimary); // 主表主键
            tableFieldKeyValue.Add(mainPrimary.ToUpper(), mainPrimary);
            for (int i = 0; i < templateInfo.SingleFormData.Count; i++)
            {
                string? vmodel = templateInfo.SingleFormData[i].__vModel__.ReplaceRegex(@"(\w+)_qt_", string.Empty); // Field
                if (vmodel.IsNotEmptyOrNull())
                {
                    feildList.Add(templateInfo.SingleFormData[i].__config__.tableName + "." + vmodel + " FIELD" + i); // TableName.Field_0
                    tableFieldKeyValue.Add("FIELD" + i, templateInfo.SingleFormData[i].__vModel__);
                }
            }
            #endregion

            #region 所有副表 关联字段

            List<string>? ctNameList = templateInfo.AuxiliaryTableFieldsModelList.Select(x => x.__config__.tableName).Distinct().ToList();
            List<string>? relationKey = new List<string>();
            relationKey.Add(string.Format(" {0}.{1}='{2}' ", templateInfo.MainTableName, mainPrimary, id)); // 主表ID
            ctNameList.ForEach(tName =>
            {
                string? tableField = templateInfo.AllTable.Find(tf => tf.table == tName)?.tableField;
                relationKey.Add(string.Format(" {0}.{1}={2}.{3} ", templateInfo.MainTableName, mainPrimary, tName, tableField));
            });
            string? whereStr = string.Join(" and ", relationKey);

            #endregion

            sql = string.Format("select {0} from {1} where {2}", string.Join(",", feildList), templateInfo.MainTableName + "," + string.Join(",", ctNameList), whereStr); // 多表,联合查询
        }

        Dictionary<string, object>? data = _databaseService.GetInterFaceData(link, sql).ToJsonString().ToObject<List<Dictionary<string, object>>>().FirstOrDefault();
        if (data == null) return null;

        // 记录全部数据
        Dictionary<string, object> dataMap = new Dictionary<string, object>();

        // 查询别名转换
        if (templateInfo.AuxiliaryTableFieldsModelList.Any()) foreach (KeyValuePair<string, object> item in data) dataMap.Add(tableFieldKeyValue[item.Key.ToUpper()], item.Value);
        else dataMap = data;

        Dictionary<string, object> newDataMap = new Dictionary<string, object>();

        dataMap = _formDataParsing.GetTableDataInfo(new List<Dictionary<string, object>>() { dataMap }, templateInfo.FieldsModelList, "detail").FirstOrDefault();

        #region 处理子表数据
        foreach (var model in templateInfo.ChildTableFieldsModelList)
        {
            if (!string.IsNullOrEmpty(model.__vModel__))
            {
                if (model.__config__.qtKey.Equals(QtKeyConst.TABLE))
                {
                    List<string> feilds = new List<string>();
                    foreach (FieldsModel? childModel in model.__config__.children) if (!string.IsNullOrEmpty(childModel.__vModel__)) feilds.Add(childModel.__vModel__); // 拼接查询字段
                    string relationMainFeildValue = string.Empty;
                    string childSql = string.Format("select {0} from {1} where 1=1 ", string.Join(",", feilds), model.__config__.tableName); // 查询子表数据
                    foreach (Engine.Model.TableModel? tableMap in templateInfo.AllTable)
                    {
                        if (tableMap.table.Equals(model.__config__.tableName))
                        {
                            if (dataMap.ContainsKey(tableMap.relationField)) childSql += string.Format(" And {0}='{1}'", tableMap.tableField, dataMap[tableMap.relationField]); // 外键
                            List<Dictionary<string, object>>? childData = _databaseService.GetInterFaceData(link, childSql).ToJsonString().ToObject<List<Dictionary<string, object>>>();
                            var childTableData = _formDataParsing.GetTableDataInfo(childData, model.__config__.children, "detail");

                            #region 获取关联表单属性 和 弹窗选择属性
                            foreach (FieldsModel? item in model.__config__.children.Where(x => x.__config__.qtKey == "relationForm").ToList())
                            {
                                foreach (Dictionary<string, object>? dataItem in childTableData)
                                {
                                    if (item.__vModel__.IsNotEmptyOrNull() && dataItem.ContainsKey(item.__vModel__) && dataItem[item.__vModel__] != null)
                                    {
                                        string? relationValueId = dataItem[item.__vModel__].ToString(); // 获取关联表单id
                                        VisualDevEntity? relationInfo = await _visualDevRepository.AsQueryable().FirstAsync(x => x.Id == item.modelId); // 获取 关联表单 转换后的数据
                                        string? relationValueStr = string.Empty;
                                        if (relationInfo.Tables != null && relationInfo.Tables.Equals("[]")) relationValueStr = await GetIsNoTableInfoDetails(relationInfo, await GetInfo(relationValueId));
                                        else relationValueStr = await GetHaveTableInfoDetails(relationValueId, relationInfo);

                                        if (!relationValueStr.IsNullOrEmpty() && !relationValueStr.Equals(relationValueId))
                                        {
                                            Dictionary<string, object>? relationValue = relationValueStr.ToObject<Dictionary<string, object>>();

                                            // 添加到 子表 列
                                            model.__config__.children.Where(x => x.relationField.ReplaceRegex(@"_qtTable_(\w+)", string.Empty) == item.__vModel__).ToList().ForEach(citem =>
                                            {
                                                citem.__vModel__ = item.__vModel__ + "_" + citem.showField;
                                                dataItem.Add(item.__vModel__ + "_" + citem.showField, relationValue[citem.showField]);
                                            });
                                        }
                                    }
                                }
                            }

                            if (model.__config__.children.Where(x => x.__config__.qtKey == "popupAttr").Any())
                            {
                                foreach (FieldsModel? item in model.__config__.children.Where(x => x.__config__.qtKey == "popupSelect").ToList())
                                {
                                    List<Dictionary<string, string>>? pDataList = await _formDataParsing.GetPopupSelectDataList(item.interfaceId, model); // 获取接口数据列表
                                    foreach (Dictionary<string, object>? dataItem in childTableData)
                                    {
                                        if (!string.IsNullOrWhiteSpace(item.__vModel__) && dataItem.ContainsKey(item.__vModel__) && dataItem[item.__vModel__] != null)
                                        {
                                            string? relationValueId = dataItem[item.__vModel__].ToString(); // 获取关联表单id

                                            // 添加到 子表 列
                                            model.__config__.children.Where(x => x.relationField.ReplaceRegex(@"_qtTable_(\w+)", string.Empty) == item.__vModel__).ToList().ForEach(citem =>
                                            {
                                                citem.__vModel__ = item.__vModel__ + "_" + citem.showField;
                                                Dictionary<string, string>? value = pDataList.Where(x => x.Values.Contains(dataItem[item.__vModel__].ToString())).FirstOrDefault();
                                                if (value.Keys.IsNotEmptyOrNull()) dataItem.Add(item.__vModel__ + "_" + citem.showField, value[citem.showField]);
                                            });
                                        }
                                    }
                                }
                            }
                            #endregion

                            if (childTableData.Any()) newDataMap[model.__vModel__] = childTableData;
                        }
                    }
                }
            }
        }
        #endregion

        int dicCount = newDataMap.Keys.Count;
        string[] strKey = new string[dicCount];
        newDataMap.Keys.CopyTo(strKey, 0);
        for (int i = 0; i < strKey.Length; i++)
        {
            FieldsModel? model = templateInfo.FieldsModelList.Where(m => m.__vModel__ == strKey[i]).FirstOrDefault();
            if (model != null)
            {
                List<Dictionary<string, object>> tables = newDataMap[strKey[i]].ToObject<List<Dictionary<string, object>>>();
                List<Dictionary<string, object>> newTables = new List<Dictionary<string, object>>();
                foreach (Dictionary<string, object>? item in tables)
                {
                    Dictionary<string, object> dic = new Dictionary<string, object>();
                    foreach (KeyValuePair<string, object> value in item)
                    {
                        FieldsModel? child = model.__config__.children.Find(c => c.__vModel__ == value.Key);
                        if (child != null) dic.Add(value.Key, value.Value);
                    }

                    newTables.Add(dic);
                }

                if (newTables.Count > 0) newDataMap[strKey[i]] = newTables;
            }
        }

        foreach (KeyValuePair<string, object> entryMap in dataMap)
        {
            if (entryMap.Value != null)
            {
                FieldsModel? model = templateInfo.FieldsModelList.Where(m => m.__vModel__ == entryMap.Key.ToString()).FirstOrDefault();
                if (model != null && entryMap.Key.Equals(model.__vModel__)) newDataMap[entryMap.Key] = entryMap.Value;
            }
        }

        _formDataParsing.GetBARAndQR(templateInfo.FieldsModelList, newDataMap, dataMap); // 处理 条形码 、 二维码 控件

        return await _formDataParsing.GetSystemComponentsData(templateInfo.FieldsModelList, newDataMap.ToJsonString());
    }

    /// <summary>
    /// 获取有表详情转换.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <param name="templateEntity">模板实体.</param>
    /// <param name="isFlowTask"></param>
    /// <returns></returns>
    public async Task<string> GetHaveTableInfoDetails(string id, VisualDevEntity templateEntity, bool isFlowTask = false)
    {
        TemplateParsingBase? templateInfo = new TemplateParsingBase(templateEntity, isFlowTask); // 解析模板控件
        DbLinkEntity? link = await _dbLinkService.GetInfo(templateEntity.DbLinkId);
        if (link == null) link = _databaseService.GetTenantDbLink(_userManager.TenantId, _userManager.TenantDbName); // 存储当前数据连接
        List<DbTableFieldModel>? tableList = _databaseService.GetFieldList(link, templateInfo.MainTableName); // 获取主表 所有字段名
        string? mainPrimary = tableList.Find(t => t.primaryKey)?.field; // 主表主键
        if (mainPrimary.IsNullOrEmpty()) throw Oops.Oh(ErrorCode.D1402); // 主表未设置主键

        string? sql = string.Empty; // 联表查询sql
        List<string>? feildList = new List<string>(); // 存放所有查询列名
        Dictionary<string, string>? tableFieldKeyValue = new Dictionary<string, string>(); // 联表查询 表字段 别名

        // 没有副表,只查询主表
        if (!templateInfo.AuxiliaryTableFieldsModelList.Any())
        {
            feildList.Add(mainPrimary); // 主表主键
            for (int i = 0; i < tableList.Count; i++) if (templateInfo.MainTableFieldsModelList.Any(x => x.__vModel__ == tableList[i].field)) feildList.Add(tableList[i].field); // 主表列名
            sql = string.Format("select {0} from {1} where {2}='{3}'", string.Join(",", feildList), templateInfo.MainTableName, mainPrimary, id);
        }
        else
        {
            #region 所有主表、副表 字段名
            feildList.Add(templateInfo.MainTableName + "." + mainPrimary); // 主表主键
            tableFieldKeyValue.Add(mainPrimary.ToUpper(), mainPrimary);
            for (int i = 0; i < templateInfo.SingleFormData.Count; i++)
            {
                string? vmodel = templateInfo.SingleFormData[i].__vModel__.ReplaceRegex(@"(\w+)_qt_", ""); // Field
                if (vmodel.IsNotEmptyOrNull())
                {
                    feildList.Add(templateInfo.SingleFormData[i].__config__.tableName + "." + vmodel + " FIELD" + i); // TableName.Field_0
                    tableFieldKeyValue.Add("FIELD" + i, templateInfo.SingleFormData[i].__vModel__);
                }
            }
            #endregion

            #region 所有副表 关联字段
            List<string>? ctNameList = templateInfo.AuxiliaryTableFieldsModelList.Select(x => x.__config__.tableName).Distinct().ToList();
            List<string>? relationKey = new List<string>();
            relationKey.Add(string.Format(" {0}.{1}='{2}' ", templateInfo.MainTableName, mainPrimary, id)); // 主表ID
            ctNameList.ForEach(tName =>
            {
                string? tableField = templateInfo.AllTable.Find(tf => tf.table == tName)?.tableField;
                relationKey.Add(string.Format(" {0}.{1}={2}.{3} ", templateInfo.MainTableName, mainPrimary, tName, tableField));
            });
            string? whereStr = string.Join(" and ", relationKey);
            #endregion

            sql = string.Format("select {0} from {1} where {2}", string.Join(",", feildList), templateInfo.MainTableName + "," + string.Join(",", ctNameList), whereStr); // 多表， 联合查询
        }

        Dictionary<string, object>? data = _databaseService.GetInterFaceData(link, sql).ToJsonString().ToObject<List<Dictionary<string, object>>>().FirstOrDefault();
        if (data == null) return id;

        // 记录全部数据
        Dictionary<string, object> dataMap = new Dictionary<string, object>();

        // 查询别名转换
        if (templateInfo.AuxiliaryTableFieldsModelList.Any()) foreach (KeyValuePair<string, object> item in data) dataMap.Add(tableFieldKeyValue[item.Key.ToUpper()], item.Value);
        else dataMap = data;

        Dictionary<string, object> newDataMap = new Dictionary<string, object>();

        #region 处理子表数据

        foreach (var model in templateInfo.ChildTableFieldsModelList)
        {
            if (!string.IsNullOrEmpty(model.__vModel__))
            {
                if ("table".Equals(model.__config__.qtKey))
                {
                    List<string> feilds = new List<string>();
                    foreach (FieldsModel? childModel in model.__config__.children) if (!string.IsNullOrEmpty(childModel.__vModel__)) feilds.Add(childModel.__vModel__); // 拼接查询字段
                    string relationMainFeildValue = string.Empty;
                    string childSql = string.Format("select {0} from {1} where 1=1 ", string.Join(",", feilds), model.__config__.tableName); // 查询子表数据
                    foreach (Engine.Model.TableModel? tableMap in templateInfo.AllTable)
                    {
                        if (tableMap.table.Equals(model.__config__.tableName))
                        {
                            if (dataMap.ContainsKey(tableMap.relationField)) childSql += string.Format(" And {0}='{1}'", tableMap.tableField, dataMap[tableMap.relationField]); // 外键
                            List<Dictionary<string, object>>? childTableData = _databaseService.GetInterFaceData(link, childSql).ToJsonString().ToObject<List<Dictionary<string, object>>>();

                            #region 获取关联表单属性 和 弹窗选择属性
                            foreach (var item in model.__config__.children.Where(x => x.__config__.qtKey == "relationForm").ToList())
                            {
                                foreach (var dataItem in childTableData)
                                {
                                    if (item.__vModel__.IsNotEmptyOrNull() && dataItem.ContainsKey(item.__vModel__) && dataItem[item.__vModel__] != null)
                                    {
                                        var relationValueId = dataItem[item.__vModel__].ToString(); // 获取关联表单id
                                        var relationInfo = await _visualDevRepository.AsQueryable().FirstAsync(x => x.Id == item.modelId); // 获取 关联表单 转换后的数据
                                        var relationValueStr = string.Empty;
                                        if (relationInfo.Tables != null && relationInfo.Tables.Equals("[]")) relationValueStr = await GetIsNoTableInfoDetails(relationInfo, await GetInfo(relationValueId));
                                        else relationValueStr = await GetHaveTableInfoDetails(relationValueId, relationInfo);

                                        if (!relationValueStr.IsNullOrEmpty() && !relationValueStr.Equals(relationValueId))
                                        {
                                            var relationValue = relationValueStr.ToObject<Dictionary<string, object>>();

                                            // 添加到 子表 列
                                            model.__config__.children.Where(x => x.relationField.ReplaceRegex(@"_qtTable_(\w+)", string.Empty) == item.__vModel__).ToList().ForEach(citem =>
                                            {
                                                citem.__vModel__ = item.__vModel__ + "_" + citem.showField;
                                                dataItem.Add(item.__vModel__ + "_" + citem.showField, relationValue[citem.showField]);
                                            });
                                        }
                                    }
                                }
                            }

                            if (model.__config__.children.Where(x => x.__config__.qtKey == "popupAttr").Any())
                            {
                                foreach (var item in model.__config__.children.Where(x => x.__config__.qtKey == "popupSelect").ToList())
                                {
                                    var pDataList = await _formDataParsing.GetPopupSelectDataList(item.interfaceId, model); // 获取接口数据列表
                                    foreach (var dataItem in childTableData)
                                    {
                                        if (!string.IsNullOrWhiteSpace(item.__vModel__) && dataItem.ContainsKey(item.__vModel__) && dataItem[item.__vModel__] != null)
                                        {
                                            var relationValueId = dataItem[item.__vModel__].ToString(); // 获取关联表单id

                                            // 添加到 子表 列
                                            model.__config__.children.Where(x => x.relationField.ReplaceRegex(@"_qtTable_(\w+)", string.Empty) == item.__vModel__).ToList().ForEach(citem =>
                                            {
                                                citem.__vModel__ = item.__vModel__ + "_" + citem.showField;
                                                var value = pDataList.Where(x => x.Values.Contains(dataItem[item.__vModel__].ToString())).FirstOrDefault();
                                                if (value.Keys.IsNotEmptyOrNull()) dataItem.Add(item.__vModel__ + "_" + citem.showField, value[citem.showField]);
                                            });
                                        }
                                    }
                                }
                            }
                            #endregion

                            if (childTableData.Count > 0) newDataMap[model.__vModel__] = childTableData;
                        }
                    }
                }
            }
        }
        #endregion

        int dicCount = newDataMap.Keys.Count;
        string[] strKey = new string[dicCount];
        newDataMap.Keys.CopyTo(strKey, 0);
        for (int i = 0; i < strKey.Length; i++)
        {
            FieldsModel? model = templateInfo.FieldsModelList.Find(m => m.__vModel__ == strKey[i]);
            if (model != null)
            {
                List<Dictionary<string, object>> childModelData = new List<Dictionary<string, object>>();
                foreach (Dictionary<string, object>? item in newDataMap[strKey[i]].ToObject<List<Dictionary<string, object>>>())
                {
                    Dictionary<string, object> dic = new Dictionary<string, object>();
                    foreach (KeyValuePair<string, object> value in item)
                    {
                        FieldsModel? child = model.__config__.children.Find(c => c.__vModel__ == value.Key);
                        if (child != null && value.Value != null)
                        {
                            if (child.__config__.qtKey.Equals(QtKeyConst.DATE)) dic.Add(value.Key, value.Value.ToString().ParseToDateTime().ParseToUnixTime());
                            else dic.Add(value.Key, value.Value);
                        }
                    }

                    childModelData.Add(dic);
                }

                if (childModelData.Count > 0)
                {
                    // 将关键字查询传输的id转换成名称
                    newDataMap[strKey[i]] = await _formDataParsing.GetKeyData(model.__config__.children, childModelData, templateInfo.ColumnData.ToObject<ColumnDesignModel>());
                }
            }
        }

        List<Dictionary<string, object>> listEntity = new List<Dictionary<string, object>>() { dataMap };

        foreach (var entryMap in (await _formDataParsing.GetKeyData(templateInfo.SingleFormData, listEntity, templateInfo.ColumnData)).FirstOrDefault())
        {
            if (entryMap.Value != null)
            {
                var model = templateInfo.FieldsModelList.Where(m => m.__vModel__.Contains(entryMap.Key)).FirstOrDefault();
                if (model != null && entryMap.Key.Equals(model.__vModel__)) newDataMap[entryMap.Key] = entryMap.Value;
                else if (templateInfo.FieldsModelList.Where(m => m.__vModel__ == entryMap.Key.Replace("_id", string.Empty)).Any()) newDataMap[entryMap.Key] = entryMap.Value;
            }
        }

        _formDataParsing.GetBARAndQR(templateInfo.FieldsModelList, newDataMap, dataMap); // 处理 条形码 、 二维码 控件

        return newDataMap.ToJsonString();
    }

    /// <summary>
    /// 获取无表信息详情.
    /// </summary>
    /// <param name="templateEntity">模板实体.</param>
    /// <param name="data">真实数据.</param>
    /// <returns></returns>
    public async Task<string> GetIsNoTableInfoDetails(VisualDevEntity templateEntity, VisualDevModelDataEntity data)
    {
        TemplateParsingBase? templateInfo = new TemplateParsingBase(templateEntity); // 解析模板控件
        Dictionary<string, object>? realData = data.Data.ToObject<Dictionary<string, object>>(); // 获取真实数据
        IEnumerable<KeyValuePair<string, object>>? childRealData = realData.Where(p => p.Key.Contains("tableField")); // 数据分离子表数据
        List<FieldsModel>? relationFormList = templateInfo.SingleFormData.FindAll(m => m.__config__.qtKey == "relationForm"); // 获取关联表单
        if (relationFormList.Any()) foreach (FieldsModel? item in relationFormList) realData.Add(item.__vModel__ + "_id", null);
        Dictionary<string, object> newDataMap = new Dictionary<string, object>();

        // 循环子表数据
        foreach (KeyValuePair<string, object> item in childRealData)
        {
            realData.Remove(item.Key);
            List<Dictionary<string, object>> childModelData = new List<Dictionary<string, object>>();
            FieldsModel? childTemplate = templateInfo.ChildTableFieldsModelList.Find(c => c.__vModel__ == item.Key);
            if (childTemplate != null)
            {
                foreach (Dictionary<string, object>? childColumn in item.Value.ToObject<List<Dictionary<string, object>>>())
                {
                    Dictionary<string, object> dic = new Dictionary<string, object>();
                    foreach (KeyValuePair<string, object> column in childColumn)
                    {
                        FieldsModel? child = childTemplate.__config__.children.Find(c => c.__vModel__ == column.Key);
                        if (child != null && column.Value != null) dic.Add(column.Key, column.Value);
                    }

                    childModelData.Add(dic);
                }

                if (childModelData.Count > 0)
                {
                    // 将关键字查询传输的id转换成名称
                    List<Dictionary<string, object>>? childKeyAndList = await _formDataParsing.GetKeyData(childTemplate.__config__.children, childModelData, templateEntity.ColumnData.ToObject<ColumnDesignModel>());

                    #region 获取关联表单属性
                    foreach (FieldsModel? itemForm in childTemplate.__config__.children.Where(x => x.__config__.qtKey == "relationForm").ToList())
                    {
                        foreach (Dictionary<string, object>? dataItem in childModelData)
                        {
                            if (!string.IsNullOrWhiteSpace(itemForm.__vModel__) && dataItem.ContainsKey(itemForm.__vModel__) && dataItem[itemForm.__vModel__] != null)
                            {
                                string? relationValueId = dataItem[itemForm.__vModel__ + "_id"].ToString(); // 获取关联表单id
                                VisualDevEntity? relationInfo = await _visualDevRepository.AsQueryable().FirstAsync(x => x.Id == itemForm.modelId); // 获取 关联表单 转换后的数据
                                string? relationValueStr = string.Empty;
                                if (relationInfo.Tables.IsNullOrWhiteSpace() || relationInfo.Tables.Equals("[]")) relationValueStr = await GetIsNoTableInfoDetails(relationInfo, await GetInfo(relationValueId));
                                else relationValueStr = await GetHaveTableInfoDetails(relationValueId, relationInfo);

                                Dictionary<string, object>? relationValue = relationValueStr.ToObject<Dictionary<string, object>>();

                                // 添加到 子表 列
                                childTemplate.__config__.children.Where(x => x.relationField.ReplaceRegex(@"_qtTable_(\w+)", string.Empty) == itemForm.__vModel__).ToList().ForEach(citem =>
                                {
                                    citem.__vModel__ = itemForm.__vModel__ + "_" + citem.showField;
                                    dataItem.Add(itemForm.__vModel__ + "_" + citem.showField, relationValue[citem.showField]);
                                });
                            }
                        }
                    }

                    if (childTemplate.__config__.children.Where(x => x.__config__.qtKey == "popupAttr").Any())
                    {
                        foreach (var itemForm in childTemplate.__config__.children.Where(x => x.__config__.qtKey == "popupSelect").ToList())
                        {
                            var pDataList = await _formDataParsing.GetPopupSelectDataList(itemForm.interfaceId, childTemplate); // 获取接口数据列表
                            foreach (var dataItem in childModelData)
                            {
                                if (!string.IsNullOrWhiteSpace(itemForm.__vModel__) && dataItem.ContainsKey(itemForm.__vModel__) && dataItem[itemForm.__vModel__] != null)
                                {
                                    string? relationValueId = dataItem[itemForm.__vModel__].ToString(); // 获取关联表单id

                                    // 添加到 子表 列
                                    childTemplate.__config__.children.Where(x => x.relationField.ReplaceRegex(@"_qtTable_(\w+)", string.Empty) == itemForm.__vModel__).ToList().ForEach(citem =>
                                    {
                                        citem.__vModel__ = itemForm.__vModel__ + "_" + citem.showField;
                                        Dictionary<string, string>? value = pDataList.Where(x => x.Values.Contains(dataItem[itemForm.__vModel__].ToString())).FirstOrDefault();
                                        if (value.Keys.IsNotEmptyOrNull()) dataItem.Add(itemForm.__vModel__ + "_" + citem.showField, value[citem.showField]);
                                    });
                                }
                            }
                        }
                    }
                    #endregion

                    newDataMap[item.Key] = childModelData;
                }
            }
        }

        List<Dictionary<string, object>> listEntity = new List<Dictionary<string, object>>() { realData };

        // 将关键字查询传输的id转换成名称
        var keyAndList = (await _formDataParsing.GetKeyData(templateInfo.SingleFormData, listEntity, templateInfo.ColumnData.ToObject<ColumnDesignModel>())).FirstOrDefault();

        foreach (KeyValuePair<string, object> entryMap in keyAndList)
        {
            if (entryMap.Value != null)
            {
                FieldsModel? model = templateInfo.FieldsModelList.Where(m => m.__vModel__ == entryMap.Key).FirstOrDefault();
                if (model != null) newDataMap[entryMap.Key] = entryMap.Value;
                else if (templateInfo.FieldsModelList.Where(m => m.__vModel__ == entryMap.Key.Replace("_id", string.Empty)).Any()) newDataMap[entryMap.Key] = entryMap.Value;
            }
        }

        _formDataParsing.GetBARAndQR(templateInfo.SingleFormData, newDataMap, realData); // 处理 条形码 、 二维码 控件

        return newDataMap.ToJsonString();
    }

    #endregion

    #region Post

    /// <summary>
    /// 创建在线开发功能.
    /// </summary>
    /// <param name="templateEntity">功能模板实体.</param>
    /// <param name="dataInput">数据输入.</param>
    /// <returns></returns>
    public async Task Create(VisualDevEntity templateEntity, VisualDevModelDataCrInput dataInput)
    {
        if (!string.IsNullOrEmpty(templateEntity.Tables) && !"[]".Equals(templateEntity.Tables))
        {
            string? mainId = YitIdHelper.NextId().ToString();
            DbLinkEntity? link = await _dbLinkService.GetInfo(templateEntity.DbLinkId);
            if (link == null) link = _databaseService.GetTenantDbLink(_userManager.TenantId, _userManager.TenantDbName);
            List<string>? haveTableSql = await CreateHaveTableSql(templateEntity, dataInput, mainId);

            try
            {
                _db.BeginTran(); // 开启事务
                foreach (string? item in haveTableSql) await _databaseService.ExecuteSql(link, item); // 新增功能数据

                if (templateEntity.WebType == 3 && dataInput.status == 0)
                {
                    IFlowTaskManager? flowTaskService = App.GetService<IFlowTaskManager>();
                    IFlowTaskRepository? flowEngineService = App.GetService<IFlowTaskRepository>();
                    string? flowTitle = _userManager.User.RealName + "的" + templateEntity.FullName; // 流程标题
                    bool isSysTable = false; // 流程是否系统表单
                    FlowEngineEntity? eModel = await flowEngineService.GetEngineInfo(templateEntity.FlowId);
                    if (eModel?.FormType == 1) isSysTable = true;
                    await flowTaskService.Submit(null, templateEntity.FlowId, mainId, flowTitle, 1, null, dataInput.data.ToObject<JObject>(), 0, 0, isSysTable, true, dataInput.candidateList);
                }

                _db.CommitTran(); // 关闭事务
            }
            catch (Exception)
            {
                _db.RollbackTran();
                throw;
            }
        }
        else
        {
            Dictionary<string, object>? allDataMap = dataInput.data.ToObject<Dictionary<string, object>>();
            TemplateParsingBase templateInfo = new TemplateParsingBase(templateEntity); // 解析模板控件
            allDataMap = await GenerateFeilds(templateInfo.FieldsModelList.ToJsonString(), allDataMap, true); // 生成系统自动生成字段
            VisualDevModelDataEntity entity = new VisualDevModelDataEntity();
            entity.Data = allDataMap.ToJsonString();
            entity.VisualDevId = templateEntity.Id;
            try
            {
                _db.BeginTran(); // 开启事务
                VisualDevModelDataEntity? visualDevModelData = await _visualDevRepository.Context.Insertable(entity).CallEntityMethod(m => m.Creator()).ExecuteReturnEntityAsync(); // 新增功能数据

                if (templateEntity.WebType == 3 && dataInput.status == 0)
                {
                    IFlowTaskManager? flowTaskService1 = App.GetService<IFlowTaskManager>();
                    IFlowTaskRepository? flowEngineService1 = App.GetService<IFlowTaskRepository>();
                    string? flowTitle1 = _userManager.User.RealName + "的" + templateEntity.FullName; // 流程标题
                    bool isSysTable1 = false; // 流程是否系统表单
                    FlowEngineEntity? eModel = await flowEngineService1.GetEngineInfo(templateEntity.FlowId);
                    if (eModel?.FormType == 1) isSysTable1 = true;
                    await flowTaskService1.Submit(null, templateEntity.FlowId, visualDevModelData.Id, flowTitle1, 1, null, dataInput.data.ToObject<JObject>(), 0, 0, isSysTable1, true, dataInput.candidateList);
                }

                _db.CommitTran(); // 关闭事务
            }
            catch (Exception)
            {
                _db.RollbackTran();
                throw;
            }
        }
    }

    /// <summary>
    /// 创建有表SQL.
    /// </summary>
    /// <param name="templateEntity"></param>
    /// <param name="dataInput"></param>
    /// <param name="mainId"></param>
    /// <returns></returns>
    public async Task<List<string>> CreateHaveTableSql(VisualDevEntity templateEntity, VisualDevModelDataCrInput dataInput, string mainId)
    {
        return await GetInsertSql(templateEntity, dataInput, mainId);
    }

    /// <summary>
    /// 修改在线开发功能.
    /// </summary>
    /// <param name="id">修改ID.</param>
    /// <param name="templateEntity"></param>
    /// <param name="visualdevModelDataUpForm"></param>
    /// <returns></returns>
    public async Task Update(string id, VisualDevEntity templateEntity, VisualDevModelDataUpInput visualdevModelDataUpForm)
    {
        if (!string.IsNullOrEmpty(templateEntity.Tables) && !"[]".Equals(templateEntity.Tables))
        {
            var link = await _dbLinkService.GetInfo(templateEntity.DbLinkId);
            if (link == null) link = _databaseService.GetTenantDbLink(_userManager.TenantId, _userManager.TenantDbName);
            var haveTableSql = await UpdateHaveTableSql(templateEntity, visualdevModelDataUpForm, id);

            try
            {
                _db.BeginTran(); // 开启事务
                foreach (var item in haveTableSql) await _databaseService.ExecuteSql(link, item); // 修改功能数据

                if (templateEntity.WebType == 3 && visualdevModelDataUpForm.status == 0)
                {
                    var _flowTaskService = App.GetService<IFlowTaskManager>();
                    var _flowTaskRepository = App.GetService<IFlowTaskRepository>();
                    var taskEntity = await _flowTaskRepository.GetTaskInfo(id);
                    var FlowTitle = _userManager.User.RealName + "的" + templateEntity.FullName; // 流程标题
                    var IsSysTable = false; // 流程是否系统表单
                    var eModel = await _flowTaskRepository.GetEngineInfo(templateEntity.FlowId);
                    if (eModel?.FormType == 1) IsSysTable = true;
                    if (taskEntity == null)
                        await _flowTaskService.Submit(null, templateEntity.FlowId, id, FlowTitle, 1, null, visualdevModelDataUpForm.data.ToObject<JObject>(), 0, 0, IsSysTable, true, visualdevModelDataUpForm.candidateList);
                    else
                        await _flowTaskService.Submit(id, templateEntity.FlowId, id, FlowTitle, 1, null, visualdevModelDataUpForm.data.ToObject<JObject>(), 0, 0, IsSysTable, true, visualdevModelDataUpForm.candidateList);
                }

                _db.CommitTran(); // 关闭事务
            }
            catch (Exception ex)
            {
                _db.RollbackTran();
                throw;
            }
        }
        else
        {
            Dictionary<string, object> allDataMap = visualdevModelDataUpForm.data.ToObject<Dictionary<string, object>>();
            TemplateParsingBase templateInfo = new TemplateParsingBase(templateEntity); // 解析模板控件
            allDataMap = await GenerateFeilds(templateInfo.FieldsModelList.ToJsonString(), allDataMap, false); // 生成系统自动生成字段
            VisualDevModelDataEntity? oldEntity = await _visualDevRepository.Context.Queryable<VisualDevModelDataEntity>().FirstAsync(x => x.Id == id); // 获取旧数据
            Dictionary<string, object>? oldAllDataMap = oldEntity.Data.ToObject<Dictionary<string, object>>();
            List<string>? curr = templateInfo.FieldsModelList.Where(x => x.__config__.qtKey == QtKeyConst.CURRORGANIZE || x.__config__.qtKey == QtKeyConst.CURRPOSITION).Select(x => x.__vModel__).ToList();
            foreach (string? item in curr) allDataMap[item] = oldAllDataMap[item]; // 当前组织和当前岗位不做修改

            VisualDevModelDataEntity entity = new VisualDevModelDataEntity();
            entity.Data = allDataMap.ToJsonString();
            entity.VisualDevId = templateEntity.Id;
            entity.Id = id;
            try
            {
                _db.BeginTran(); // 开启事务
                await _visualDevRepository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync(); // 修改功能数据

                if (templateEntity.WebType == 3 && visualdevModelDataUpForm.status == 0)
                {
                    IFlowTaskRepository? flowTaskRepository = App.GetService<IFlowTaskRepository>();
                    IFlowTaskManager? flowTaskService = App.GetService<IFlowTaskManager>();
                    FlowTaskEntity? taskEntity = await flowTaskRepository.GetTaskInfo(id);
                    string? flowTitle = _userManager.User.RealName + "的" + templateEntity.FullName; // 流程标题
                    bool isSysTable = false; // 流程是否系统表单
                    FlowEngineEntity? eModel = await flowTaskRepository.GetEngineInfo(templateEntity.FlowId);
                    if (eModel?.FormType == 1) isSysTable = true;
                    if (taskEntity == null)
                        await flowTaskService.Submit(null, templateEntity.FlowId, id, flowTitle, 1, null, visualdevModelDataUpForm.data.ToObject<JObject>(), 0, 0, isSysTable, true, visualdevModelDataUpForm.candidateList);
                    else
                        await flowTaskService.Submit(id, templateEntity.FlowId, id, flowTitle, 1, null, visualdevModelDataUpForm.data.ToObject<JObject>(), 0, 0, isSysTable, true, visualdevModelDataUpForm.candidateList);
                }

                _db.CommitTran(); // 提交事务
            }
            catch (Exception)
            {
                _db.RollbackTran();
                throw;
            }
        }
    }

    /// <summary>
    /// 修改有表SQL.
    /// </summary>
    /// <param name="templateEntity"></param>
    /// <param name="visualdevModelDataUpForm"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<List<string>> UpdateHaveTableSql(VisualDevEntity templateEntity, VisualDevModelDataUpInput visualdevModelDataUpForm, string id)
    {
        List<string> mainSql = new List<string>(); // 执行sql语句

        Dictionary<string, object> allDataMap = visualdevModelDataUpForm.data.ToObject<Dictionary<string, object>>();
        TemplateParsingBase templateInfo = new TemplateParsingBase(templateEntity); // 解析模板控件
        allDataMap = await GenerateFeilds(templateInfo.FieldsModelList.ToJsonString(), allDataMap, false); // 生成系统自动生成字段
        if (!templateInfo.VerifyTemplate()) throw Oops.Oh(ErrorCode.D1401); // 验证模板

        DbLinkEntity? link = await _dbLinkService.GetInfo(templateEntity.DbLinkId);
        if (link == null) link = _databaseService.GetTenantDbLink(_userManager.TenantId, _userManager.TenantDbName);
        string? dbType = link?.DbType != null ? link.DbType : _visualDevRepository.Context.CurrentConnectionConfig.DbType.ToString();

        List<DbTableFieldModel>? tableList = _databaseService.GetFieldList(link, templateInfo.MainTableName); // 获取主表 表结构 信息
        DbTableFieldModel? mainPrimary = tableList.Find(t => t.primaryKey); // 主表主键
        mainSql.Add(string.Format("delete from {0} where {2}='{1}';", templateInfo.MainTable?.table, id, mainPrimary?.field)); // 删除主表 sql
        if (templateInfo.AllTable.Any(x => x.typeId.Equals("0")))
        {
            templateInfo.AllTable.Where(x => x.typeId.Equals("0")).ToList()
                .ForEach(item => mainSql.Add(string.Format("delete from {0} where {1}='{2}';", item.table, item.tableField, id))); // 删除所有涉及表数据 sql
        }

        List<string>? insertSql = await GetInsertSql(templateEntity, visualdevModelDataUpForm, id, true); // 获取Insert 语句

        mainSql.AddRange(insertSql); // 拼接删除和插入 sql 语句

        return mainSql;
    }

    #endregion

    #region 公用方法

    /// <summary>
    /// 删除无表数据.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="templateEntity"></param>
    /// <returns></returns>
    public async Task DelIsNoTableInfo(string id, VisualDevEntity templateEntity)
    {
        List<string>? ids = new List<string>();
        if (templateEntity.WebType == 3) ids = await GetAllowDeleteFlowTaskList(new List<string>() { id });

        id = ids.FirstOrDefault();
        if (id.IsNotEmptyOrNull())
        {
            VisualDevModelDataEntity? entity = await _visualDevRepository.Context.Queryable<VisualDevModelDataEntity>().FirstAsync(v => v.Id == id && v.DeleteMark == null);
            _ = entity ?? throw Oops.Oh(ErrorCode.COM1007);

            try
            {
                _db.BeginTran(); // 开启事务

                // 删除无表表数据
                await _visualDevRepository.Context.Updateable(entity).CallEntityMethod(m => m.Delete()).UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId }).ExecuteCommandAsync();

                if (templateEntity.WebType == 3)
                {
                    await Scoped.Create(async (_, scope) =>
                    {
                        IServiceProvider? services = scope.ServiceProvider;
                        IFlowTaskRepository? flowTaskRepository = App.GetService<IFlowTaskRepository>(services);
                        FlowTaskEntity? entity = await flowTaskRepository.GetTaskInfo(id);
                        if (entity != null) await flowTaskRepository.DeleteTask(entity);
                    });
                }

                _db.CommitTran(); // 关闭事务
            }
            catch (Exception)
            {
                _db.RollbackTran();
                throw;
            }
        }
    }

    /// <summary>
    /// 批量删除无表数据.
    /// </summary>
    /// <param name="ids">ID数组.</param>
    /// <param name="templateEntity"></param>
    /// <returns></returns>
    public async Task BatchDelIsNoTableData(List<string> ids, VisualDevEntity templateEntity)
    {
        ids = ids.Where(i => i != null).ToList();
        if (templateEntity.WebType == 3)
        {
            List<string>? notAllow = await GetAllowDeleteFlowTaskList(ids);
            ids = ids.Except(notAllow).ToList();
        }

        if (ids.Any())
        {
            List<VisualDevModelDataEntity>? dataList = await _visualDevRepository.Context.Queryable<VisualDevModelDataEntity>().Where(r => ids.Contains(r.Id)).Where(v => v.DeleteMark == null).ToListAsync();

            try
            {
                _db.BeginTran(); // 开启事务

                // 删除有表数据
                await _visualDevRepository.Context.Updateable(dataList).CallEntityMethod(m => m.Delete()).UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId }).ExecuteCommandAsync();

                if (templateEntity.WebType == 3)
                {
                    Scoped.Create((_, scope) =>
                    {
                        IServiceProvider? services = scope.ServiceProvider;
                        IFlowTaskRepository? flowTaskRepository = App.GetService<IFlowTaskRepository>(services);
                        ids.ForEach(async it =>
                        {
                            FlowTaskEntity? entity = await flowTaskRepository.GetTaskInfo(it);
                            if (entity != null) await flowTaskRepository.DeleteTask(entity);
                        });
                    });
                }

                _db.CommitTran(); // 关闭事务
            }
            catch (Exception)
            {
                _db.RollbackTran();
                throw;
            }

        }
    }

    /// <summary>
    /// 删除有表信息.
    /// </summary>
    /// <param name="id">主键</param>
    /// <param name="templateEntity">模板实体</param>
    /// <returns></returns>
    public async Task DelHaveTableInfo(string id, VisualDevEntity templateEntity)
    {
        List<string>? ids = new List<string>();
        ids.Add(id);
        if (templateEntity.WebType == 3) ids = await GetAllowDeleteFlowTaskList(new List<string>() { id });

        id = ids.FirstOrDefault();
        if (id.IsNotEmptyOrNull())
        {
            TemplateParsingBase templateInfo = new TemplateParsingBase(templateEntity); // 解析模板控件
            DbLinkEntity? link = await _dbLinkService.GetInfo(templateEntity.DbLinkId);
            if (link == null) link = _databaseService.GetTenantDbLink(_userManager.TenantId, _userManager.TenantDbName);
            List<DbTableFieldModel>? mainTable = _databaseService.GetFieldList(link, templateInfo.MainTable.table); // 获取表信息
            DbTableFieldModel? mainPrimary = mainTable.Find(t => t.primaryKey); // 获取主键
            List<string>? allDelSql = new List<string>(); // 拼接语句
            StringBuilder queryMain = new StringBuilder(); // 查询主表信息语句
            allDelSql.Add(string.Format("delete from {0} where {1} = '{2}';", templateInfo.MainTable.table, mainPrimary.field, id));
            if (templateInfo.AllTable.Any(x => x.typeId.Equals("0")))
            {
                templateInfo.AllTable.Where(x => x.typeId.Equals("0")).ToList()
                    .ForEach(item => allDelSql.Add(string.Format("delete from {0} where {1}='{2}';", item.table, item.tableField, id))); // 删除所有涉及表数据 sql
            }

            try
            {
                _db.BeginTran(); // 开启事务
                foreach (string? item in allDelSql) await _databaseService.ExecuteSql(link, item); // 删除有表数据

                if (templateEntity.WebType == 3)
                {
                    await Scoped.Create(async (_, scope) =>
                    {
                        IServiceProvider? services = scope.ServiceProvider;
                        IFlowTaskRepository? flowTaskRepository = App.GetService<IFlowTaskRepository>(services);
                        FlowTaskEntity? entity = await flowTaskRepository.GetTaskInfo(id);
                        if (entity != null) await flowTaskRepository.DeleteTask(entity);
                    });
                }

                _db.CommitTran(); // 关闭事务
            }
            catch (Exception)
            {
                _db.RollbackTran();
                throw;
            }
        }
    }

    /// <summary>
    /// 批量删除有表数据.
    /// </summary>
    /// <param name="ids">id数组</param>
    /// <param name="templateEntity">模板实体</param>
    /// <returns></returns>
    public async Task BatchDelHaveTableData(List<string> ids, VisualDevEntity templateEntity)
    {
        if (templateEntity.WebType == 3) ids = await GetAllowDeleteFlowTaskList(ids);

        if (ids.Count > 0)
        {
            TemplateParsingBase templateInfo = new TemplateParsingBase(templateEntity); // 解析模板控件
            DbLinkEntity? link = await _dbLinkService.GetInfo(templateEntity.DbLinkId);
            if (link == null) link = _databaseService.GetTenantDbLink(_userManager.TenantId, _userManager.TenantDbName);
            List<DbTableFieldModel>? mainTable = _databaseService.GetFieldList(link, templateInfo.MainTable.table); // 获取表信息
            DbTableFieldModel? mainPrimary = mainTable.Find(t => t.primaryKey); // 获取主键
            StringBuilder allDelSql = new StringBuilder(); // 总删除语句

            allDelSql.AppendFormat("delete from {0} where {1} in ('{2}');", templateInfo.MainTable.table, mainPrimary.field, string.Join("','", ids)); // 主表数据
            if (templateInfo.AllTable.Any(x => x.typeId.Equals("0")))
            {
                templateInfo.AllTable.Where(x => x.typeId.Equals("0")).ToList()
                    .ForEach(item => allDelSql.AppendFormat(string.Format("delete from {0} where {1} in ('{2}');", item.table, item.tableField, string.Join("','", ids))));
            }

            try
            {
                _db.BeginTran(); // 开启事务
                await _databaseService.ExecuteSql(link, allDelSql.ToString()); // 删除有表数据

                if (templateEntity.WebType == 3)
                {
                    Scoped.Create((_, scope) =>
                    {
                        IServiceProvider? services = scope.ServiceProvider;
                        IFlowTaskRepository? flowTaskRepository = App.GetService<IFlowTaskRepository>(services);
                        ids.ForEach(it =>
                        {
                            FlowTaskEntity? entity = flowTaskRepository.GetTaskFirstOrDefault(it);
                            if (entity != null) flowTaskRepository.DeleteTaskNoAwait(entity);
                        });
                    });
                }

                _db.CommitTran(); // 关闭事务
            }
            catch (Exception)
            {
                _db.RollbackTran();
                throw;
            }
        }
    }

    /// <summary>
    /// 生成系统自动生成字段.
    /// </summary>
    /// <param name="fieldsModelListJson">模板数据.</param>
    /// <param name="allDataMap">真实数据.</param>
    /// <param name="IsCreate">创建与修改标识 true创建 false 修改.</param>
    /// <returns></returns>
    public async Task<Dictionary<string, object>> GenerateFeilds(string fieldsModelListJson, Dictionary<string, object> allDataMap, bool IsCreate)
    {
        List<FieldsModel> fieldsModelList = fieldsModelListJson.ToList<FieldsModel>();
        UserEntity? userInfo = _userManager.User;
        int dicCount = allDataMap.Keys.Count;
        string[] strKey = new string[dicCount];
        allDataMap.Keys.CopyTo(strKey, 0);
        for (int i = 0; i < strKey.Length; i++)
        {
            // 根据KEY查找模板
            FieldsModel? model = fieldsModelList.Find(f => f.__vModel__ == strKey[i]);
            if (model != null)
            {
                // 如果模板qtKey为table为子表数据
                if (model.__config__.qtKey.Equals(QtKeyConst.TABLE) && allDataMap[strKey[i]] != null)
                {
                    List<FieldsModel> childFieldsModelList = model.__config__.children;
                    object? objectData = allDataMap[strKey[i]];
                    List<Dictionary<string, object>> childAllDataMapList = objectData.ToJsonString().ToObject<List<Dictionary<string, object>>>();
                    if (childAllDataMapList != null && childAllDataMapList.Count > 0)
                    {
                        List<Dictionary<string, object>> newChildAllDataMapList = new List<Dictionary<string, object>>();
                        foreach (Dictionary<string, object>? childmap in childAllDataMapList)
                        {
                            Dictionary<string, object>? newChildData = new Dictionary<string, object>();
                            foreach (KeyValuePair<string, object> item in childmap)
                            {
                                FieldsModel? childFieldsModel = childFieldsModelList.Where(c => c.__vModel__ == item.Key).FirstOrDefault();
                                if (childFieldsModel != null && childFieldsModel.__vModel__.Equals(item.Key))
                                {
                                    switch (childFieldsModel.__config__.qtKey)
                                    {
                                        case QtKeyConst.BILLRULE:
                                            if (IsCreate)
                                            {
                                                string billNumber = await _billRuleService.GetBillNumber(childFieldsModel.__config__.rule);
                                                if (!"单据规则不存在".Equals(billNumber)) newChildData[item.Key] = billNumber;
                                                else newChildData[item.Key] = string.Empty;
                                            }
                                            else
                                            {
                                                newChildData[item.Key] = childmap[item.Key];
                                            }

                                            break;
                                        case QtKeyConst.CREATEUSER:
                                            if (IsCreate) newChildData[item.Key] = userInfo.Id;
                                            break;
                                        case QtKeyConst.MODIFYUSER:
                                            if (!IsCreate) newChildData[item.Key] = userInfo.Id;
                                            break;
                                        case QtKeyConst.CREATETIME:
                                            if (IsCreate) newChildData[item.Key] = string.Format("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now);
                                            break;
                                        case QtKeyConst.MODIFYTIME:
                                            if (!IsCreate) newChildData[item.Key] = string.Format("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now);
                                            break;
                                        case QtKeyConst.CURRPOSITION:
                                            if (IsCreate)
                                            {
                                                string? pid = await _visualDevRepository.Context.Queryable<UserEntity, PositionEntity>((a, b) => new JoinQueryInfos(JoinType.Left, b.Id == a.PositionId))
                                                    .Where((a, b) => a.Id == userInfo.Id && a.DeleteMark == null).Select((a, b) => a.PositionId).FirstAsync();
                                                if (pid.IsNotEmptyOrNull()) newChildData[item.Key] = pid;
                                                else newChildData[item.Key] = string.Empty;
                                            }

                                            break;
                                        case QtKeyConst.CURRORGANIZE:
                                            if (IsCreate)
                                            {
                                                if (userInfo.OrganizeId != null) newChildData[item.Key] = userInfo.OrganizeId;
                                                else newChildData[item.Key] = string.Empty;
                                            }

                                            break;
                                        case QtKeyConst.UPLOADFZ: // 文件上传
                                            if (allDataMap[strKey[i]].IsNullOrEmpty()) newChildData[item.Key] = new string[] { };
                                            else newChildData[item.Key] = childmap[item.Key];
                                            break;
                                        default:
                                            newChildData[item.Key] = childmap[item.Key];
                                            break;
                                    }
                                }
                            }

                            newChildAllDataMapList.Add(newChildData);
                            allDataMap[strKey[i]] = newChildAllDataMapList;
                        }
                    }
                }
                else
                {
                    if (model.__vModel__.Equals(strKey[i]))
                    {
                        switch (model.__config__.qtKey)
                        {
                            case QtKeyConst.BILLRULE:
                                if (IsCreate)
                                {
                                    string billNumber = await _billRuleService.GetBillNumber(model.__config__.rule);
                                    if (!"单据规则不存在".Equals(billNumber)) allDataMap[strKey[i]] = billNumber;
                                    else allDataMap[strKey[i]] = string.Empty;
                                }

                                break;
                            case QtKeyConst.CREATEUSER:
                                if (IsCreate) allDataMap[strKey[i]] = userInfo.Id;
                                else allDataMap[strKey[i]] = (await _visualDevRepository.Context.Queryable<UserEntity>().FirstAsync(u => u.RealName == allDataMap[strKey[i]].ToString() && u.DeleteMark == null)).Id;
                                break;
                            case QtKeyConst.CREATETIME:
                                if (IsCreate) allDataMap[strKey[i]] = string.Format("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now);
                                break;
                            case QtKeyConst.MODIFYUSER:
                                if (!IsCreate) allDataMap[strKey[i]] = userInfo.Id;
                                break;
                            case QtKeyConst.MODIFYTIME:
                                if (!IsCreate) allDataMap[strKey[i]] = string.Format("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now);
                                break;
                            case QtKeyConst.CURRPOSITION:
                                if (IsCreate)
                                {
                                    string? pid = await _visualDevRepository.Context.Queryable<UserEntity, PositionEntity>((a, b) => new JoinQueryInfos(JoinType.Left, b.Id == a.PositionId))
                                        .Where((a, b) => a.Id == userInfo.Id && a.DeleteMark == null).Select((a, b) => a.PositionId).FirstAsync();
                                    if (pid.IsNotEmptyOrNull()) allDataMap[strKey[i]] = pid;
                                    else allDataMap[strKey[i]] = string.Empty;
                                }

                                break;
                            case QtKeyConst.CURRORGANIZE:
                                if (IsCreate)
                                {
                                    if (userInfo.OrganizeId != null) allDataMap[strKey[i]] = userInfo.OrganizeId;
                                    else allDataMap[strKey[i]] = string.Empty;
                                }

                                break;
                            case QtKeyConst.UPLOADFZ: // 文件上传
                                if (allDataMap[strKey[i]].IsNullOrEmpty()) allDataMap[strKey[i]] = new string[] { };
                                break;
                        }
                    }
                }
            }
        }

        return allDataMap;
    }

    #endregion

    #region 私有方法

    /// <summary>
    ///  生成 Insert Sql语句.
    /// </summary>
    /// <param name="templateEntity"></param>
    /// <param name="dataInput"></param>
    /// <param name="mainId"></param>
    /// <param name="isUpdate"></param>
    /// <returns></returns>
    private async Task<List<string>> GetInsertSql(VisualDevEntity templateEntity, VisualDevModelDataCrInput dataInput, string mainId, bool isUpdate = false)
    {
        Dictionary<string, object>? allDataMap = dataInput.data.ToObject<Dictionary<string, object>>();
        TemplateParsingBase templateInfo = new TemplateParsingBase(templateEntity); // 解析模板控件
        allDataMap = await GenerateFeilds(templateInfo.FieldsModelList.ToJsonString(), allDataMap, !isUpdate); // 生成系统自动生成字段
        if (!templateInfo.VerifyTemplate()) throw Oops.Oh(ErrorCode.D1401); // 验证模板
        DbLinkEntity link = await _dbLinkService.GetInfo(templateEntity.DbLinkId);
        if (link == null) link = _databaseService.GetTenantDbLink(_userManager.TenantId, _userManager.TenantDbName);
        string? dbType = link?.DbType != null ? link.DbType : _visualDevRepository.Context.CurrentConnectionConfig.DbType.ToString();
        List<DbTableFieldModel>? tableList = _databaseService.GetFieldList(link, templateInfo.MainTableName); // 获取主表 表结构 信息
        DbTableFieldModel? mainPrimary = tableList.Find(t => t.primaryKey); // 主表主键
        StringBuilder mainFelid = new StringBuilder(); // 主表字段集合
        List<string> mainFelidList = new List<string>();

        #region 处理当前组织和当前岗位 (不做修改)

        List<string>? curr = templateInfo.FieldsModelList.Where(x => x.__config__.qtKey == QtKeyConst.CURRORGANIZE || x.__config__.qtKey == QtKeyConst.CURRPOSITION).Select(x => x.__vModel__).ToList();
        if (isUpdate)
        {
            string? queryMain = string.Format("select * from {0} where {2}='{1}';", templateInfo.MainTable?.table, mainId, mainPrimary?.field);
            Dictionary<string, object>? mainMap = _databaseService.GetInterFaceData(link, queryMain).ToJsonString().ToObject<List<Dictionary<string, object>>>().FirstOrDefault();
            curr.ForEach(item => { if (mainMap.ContainsKey(item)) allDataMap[item] = mainMap[item]; }); // 当前组织和当前岗位不做修改
        }
        #endregion

        // 主表查询语句
        List<string> mainSql = new List<string>();
        List<string> mainColumn = new List<string>();
        List<string> mainValues = new List<string>();

        // 拼接主表 sql
        templateInfo?.MainTableFieldsModelList.ForEach(item =>
        {
            if (allDataMap.ContainsKey(item.__vModel__))
            {
                object? itemData = allDataMap[item.__vModel__];
                if (itemData != null && !string.IsNullOrEmpty(itemData.ToString()) && itemData.ToString() != "[]")
                {
                    mainColumn.Add(item.__vModel__); // Column部分
                    mainValues.Add(_formDataParsing.InsertValueHandle(dbType, tableList, item.__vModel__, itemData, templateInfo.MainTableFieldsModelList)); // Values部分
                }
            }
        });
        if (mainColumn.Any()) mainSql.Add(string.Format("insert into {0} ({1},{2}) values('{3}',{4});", templateInfo?.MainTableName, mainPrimary?.field, string.Join(",", mainColumn), mainId, string.Join(",", mainValues)));
        else mainSql.Add(string.Format("insert into {0} ({1}) values('{2}');", templateInfo?.MainTableName, mainPrimary?.field, mainId));

        // 拼接副表 sql
        if (templateInfo.AuxiliaryTableFieldsModelList.Any())
        {
            templateInfo.AuxiliaryTableFieldsModelList.Select(x => x.__config__.tableName).Distinct().ToList().ForEach(tbname =>
            {
                List<DbTableFieldModel>? tableAllField = _databaseService.GetFieldList(link, tbname); // 数据库里获取表的所有字段

                #region 处理当前组织和当前岗位 (不做修改)

                if (isUpdate && curr.Any(x => x.Contains("_qt_")))
                {
                    string? querTableSql = string.Format("select * from {0} where {1}='{2}';", tbname, templateInfo.AllTable.Find(t => t.table == tbname)?.tableField, mainId);

                    // 获取表数据
                    List<Dictionary<string, object>>? tableData = _databaseService.GetInterFaceData(link, querTableSql).ToJsonString().ToObject<List<Dictionary<string, object>>>();
                    curr.ForEach(item =>
                    {
                        string? itemKey = item.ReplaceRegex(@"(\w+)_qt_", string.Empty);
                        if (tableData.Any() && tableData.FirstOrDefault().ContainsKey(itemKey)) allDataMap[item] = tableData.FirstOrDefault()[itemKey]; // 当前组织和当前岗位不做修改
                    });
                }
                #endregion

                List<string>? tableFieldList = new List<string>();

                // 剔除空值控件
                templateInfo.AuxiliaryTableFieldsModelList.Select(x => x.__vModel__).Where(x => x.Contains("qt_" + tbname + "_qt_")).ToList().ForEach(item =>
                {
                    object? itemData = allDataMap.Where(x => x.Key == item).Any() ? allDataMap[item] : null;
                    if (itemData != null && itemData.ToString() != string.Empty) tableFieldList.Add(item);
                });

                string? fieldList = string.Join(",", tableFieldList.Select(x => x.ReplaceRegex(@"(\w+)_qt_", string.Empty)).ToList()); // 插入的字段名
                List<string>? valueList = new List<string>(); // 对应的插入值

                tableFieldList.ForEach(item =>
                {
                    // 前端未填写数据的字段，默认会找不到字段名，需要验证
                    object? itemData = allDataMap.Where(x => x.Key == item).Count() > 0 ? allDataMap[item] : null;
                    if (itemData != null) valueList.Add(_formDataParsing.InsertValueHandle(dbType, tableList, item, itemData, templateInfo.FieldsModelList)); // Values部分
                });

                // 没有插入数据，只插入主键和外键数据
                if (fieldList.Length > 1)
                {
                    mainSql.Add(string.Format("insert into {0}({1},{2},{3}) values('{4}','{5}',{6});",
                        tbname,
                        _databaseService.GetFieldList(link, tbname)?.Find(x => x.primaryKey).field, // 主键字段名,
                        templateInfo?.AllTable?.Find(t => t.table == tbname).tableField, // 外键字段名,
                        fieldList,
                        YitIdHelper.NextId().ToString(),
                        mainId,
                        string.Join(",", valueList)));
                }
                else
                {
                    mainSql.Add(string.Format("insert into {0}({1},{2}) values('{3}','{4}');",
                        tbname,
                        _databaseService.GetFieldList(link, tbname)?.Find(x => x.primaryKey).field, // 主键字段名,
                        templateInfo?.AllTable?.Find(t => t.table == tbname).tableField, // 外键字段名,
                        YitIdHelper.NextId().ToString(),
                        mainId));
                }
            });
        }

        // 拼接子表 sql
        foreach (string? item in allDataMap.Where(d => d.Key.Contains("tableField")).Select(d => d.Key).ToList())
        {
            // 查找到该控件数据
            object? objectData = allDataMap[item];
            List<Dictionary<string, object>>? model = objectData.ToObject<List<Dictionary<string, object>>>();
            if (model != null && model.Count > 0)
            {
                // 利用key去找模板
                FieldsModel? fieldsModel = templateInfo.FieldsModelList.Find(f => f.__vModel__ == item);
                ConfigModel? fieldsConfig = fieldsModel?.__config__;
                StringBuilder childColumn = new StringBuilder();
                List<string>? childValues = new List<string>();
                Engine.Model.TableModel? childTable = templateInfo.AllTable.Find(t => t.table == fieldsModel.__config__.tableName);
                tableList = new List<DbTableFieldModel>();
                tableList = _databaseService.GetFieldList(link, childTable?.table);
                DbTableFieldModel? childPrimary = tableList.Find(t => t.primaryKey);
                foreach (Dictionary<string, object>? data in model)
                {
                    if (data.Count > 0)
                    {
                        #region 处理当前组织和当前岗位 (不做修改)

                        curr = fieldsConfig?.children.Where(x => x.__config__.qtKey == QtKeyConst.CURRORGANIZE || x.__config__.qtKey == QtKeyConst.CURRPOSITION).Select(x => x.__vModel__).ToList();
                        if (isUpdate && curr.Any())
                        {
                            string? querTableSql = string.Format("select * from {0} where {1}='{2}';", fieldsModel?.__config__.tableName, childTable?.tableField, mainId);

                            // 获取表数据
                            List<Dictionary<string, object>>? tableData = _databaseService.GetInterFaceData(link, querTableSql).ToJsonString().ToObject<List<Dictionary<string, object>>>();

                            // 当前组织和当前岗位不做修改
                            foreach (string? it in curr) if (tableData.Any() && tableData.FirstOrDefault().ContainsKey(it)) data[it] = tableData.FirstOrDefault()[it];
                        }
                        #endregion

                        foreach (KeyValuePair<string, object> child in data)
                        {
                            if (child.Value != null && child.Value.ToString() != "[]" && child.Value.ToString() != string.Empty)
                            {
                                childColumn.AppendFormat("{0},", child.Key); // Column部分
                                childValues.Add(_formDataParsing.InsertValueHandle(dbType, tableList, child.Key, child.Value, fieldsConfig.children)); // Values部分
                            }
                        }

                        if (!string.IsNullOrEmpty(childColumn.ToString()))
                        {
                            mainSql.Add(string.Format(
                                "insert into {0}({6},{4},{1}) values('{3}','{5}',{2});",
                                fieldsModel.__config__.tableName,
                                childColumn.ToString().Trim(','),
                                string.Join(",", childValues),
                                YitIdHelper.NextId().ToString(),
                                childTable.tableField,
                                mainId,
                                childPrimary.field));
                        }

                        childColumn = new StringBuilder();
                        childValues = new List<string>();
                    }
                }
            }
        }

        return mainSql;
    }

    /// <summary>
    /// 获取允许删除任务列表.
    /// </summary>
    /// <param name="ids">id数组</param>
    /// <returns></returns>
    private async Task<List<string>> GetAllowDeleteFlowTaskList(List<string> ids)
    {
        List<string>? idList = await _visualDevRepository.Context.Queryable<FlowTaskEntity>().Where(f => ids.Contains(f.Id)).Select(f => f.Id).ToListAsync();

        return ids.Except(idList).ToList();
    }

    #endregion
}
