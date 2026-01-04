using System.Data;
using System.Dynamic;
using System.Text;
using QT.Common.Const;
using QT.Common.Dtos.DataBase;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Models;
using QT.Common.Models.VisualDev;
using QT.Common.Options;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.FriendlyException;
using QT.Systems.Entitys.Model.DataBase;
using QT.Systems.Entitys.System;
using QT.VisualDev.Entitys.Dto.VisualDevModelData;
using Mapster;
using Microsoft.Extensions.Options;
using SqlSugar;
using QT.Common.Configuration;
using System.Collections;
using Serilog;
using System.Linq;

namespace QT.Common.Core.Manager;

/// <summary>
/// 实现切换数据库后操作.
/// </summary>
public class DataBaseManager : IDataBaseManager, IScoped
{
    /// <summary>
    /// 查询特殊功能前缀
    /// </summary>
    private static readonly string _qtfuncPrefix = "qtfunc_";

    /// <summary>
    /// 客户端.
    /// </summary>
    private SqlSugarClient _sqlSugarClient;

    /// <summary>
    /// 多库事务.
    /// </summary>
    private readonly ITenant db;

    /// <summary>
    /// 数据库配置选项.
    /// </summary>
    private readonly ConnectionStringsOptions _connectionStrings;

    /// <summary>
    /// 构造函数.
    /// </summary>
    public DataBaseManager(
        IOptions<ConnectionStringsOptions> connectionOptions,
        ISqlSugarClient context)
    {
        _sqlSugarClient = (SqlSugarClient)context;
        _connectionStrings = connectionOptions.Value;
        db = context.AsTenant();
    }

    /// <summary>
    /// 数据库切换.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <returns>切库后的SqlSugarClient.</returns>
    public SqlSugarClient ChangeDataBase(DbLinkEntity link)
    {
        if (!KeyVariable.MultiTenancy)
        {
            _sqlSugarClient.AddConnection(new ConnectionConfig()
            {
            ConfigId = link.Id,
            DbType = ToDbType(link.DbType),
            ConnectionString = ToConnectionString(link),
            InitKeyType = InitKeyType.Attribute,
            IsAutoCloseConnection = true
        });
        }
        _sqlSugarClient.ChangeDatabase(link.Id);



#if DEBUG
        _sqlSugarClient.Aop.OnLogExecuting = (sql, pars) =>
        {
            if (sql.StartsWith("SELECT"))
                Console.ForegroundColor = ConsoleColor.Green;

            if (sql.StartsWith("UPDATE") || sql.StartsWith("INSERT"))
                Console.ForegroundColor = ConsoleColor.White;

            if (sql.StartsWith("DELETE"))
                Console.ForegroundColor = ConsoleColor.Blue;

            // 在控制台输出sql语句
            Log.Information($"【{_sqlSugarClient.CurrentConnectionConfig.ConfigId}】:{_sqlSugarClient.CurrentConnectionConfig.ConnectionString}\r\n{SqlProfiler.ParameterFormat(sql, pars)}");
            //Console.WriteLine($"【{_sqlSugarClient.CurrentConnectionConfig.ConfigId}】:{_sqlSugarClient.CurrentConnectionConfig.ConnectionString}\r\n{SqlProfiler.ParameterFormat(sql, pars)}");
            ////Console.WriteLine(SqlProfiler.ParameterFormat(sql, pars));
            //Console.WriteLine();

            // 在MiniProfiler内显示
            // App.PrintToMiniProfiler("SqlSugar", "Info", SqlProfiler.ParameterFormat(sql, pars));
        };
#endif

        return _sqlSugarClient;
    }

    /// <summary>
    /// 获取多租户Link.
    /// </summary>
    /// <param name="tenantId">租户ID.</param>
    /// <param name="tenantName">租户数据库.</param>
    /// <returns></returns>
    public DbLinkEntity GetTenantDbLink(string tenantId, string tenantName)
    {
        return new DbLinkEntity
        {
            Id = tenantId,
            ServiceName = tenantName,
            DbType = _connectionStrings.DBType,
            Host = _connectionStrings.Host,
            Port = _connectionStrings.Port,
            UserName = _connectionStrings.UserName,
            Password = _connectionStrings.Password
        };
    }

    /// <summary>
    /// 执行Sql(查询).
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="strSql">sql语句.</param>
    /// <returns></returns>
    public async Task<int> ExecuteSql(DbLinkEntity link, string strSql)
    {
        if (link != null)
            _sqlSugarClient = ChangeDataBase(link);

        try
        {
            db.BeginTran();

            int flag = 0;
            if (_sqlSugarClient.CurrentConnectionConfig.DbType == SqlSugar.DbType.Oracle)
                flag = await _sqlSugarClient.Ado.ExecuteCommandAsync(strSql.TrimEnd(';'));
            else
                flag = await _sqlSugarClient.Ado.ExecuteCommandAsync(strSql);

            db.CommitTran();
            return flag;
        }
        catch (Exception)
        {
            db.RollbackTran();
            throw;
        }
    }

    /// <summary>
    /// 条件动态过滤.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="strSql">sql语句.</param>
    /// <returns>条件是否成立.</returns>
    public bool WhereDynamicFilter(DbLinkEntity link, string strSql)
    {
        if (link != null)
            _sqlSugarClient = ChangeDataBase(link);

        return _sqlSugarClient.Ado.SqlQuery<dynamic>(strSql).Count > 0;
    }

    /// <summary>
    /// 执行Sql(新增、修改).
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="table">表名.</param>
    /// <param name="dicList">数据.</param>
    /// <param name="primaryField">主键字段.</param>
    /// <returns></returns>
    public async Task<int> ExecuteSql(DbLinkEntity link, string table, List<Dictionary<string, object>> dicList, string primaryField = "")
    {

        if (link != null)
            _sqlSugarClient = ChangeDataBase(link);

        try
        {
            db.BeginTran();

            int flag = 0;
            if (string.IsNullOrEmpty(primaryField))
                flag = await _sqlSugarClient.Insertable(dicList).AS(table).ExecuteCommandAsync();
            else
                flag = await _sqlSugarClient.Updateable(dicList).AS(table).WhereColumns(primaryField).ExecuteCommandAsync();
            db.CommitTran();
            return flag;
        }
        catch (Exception)
        {
            db.RollbackTran();
            throw;
        }
    }

    /// <summary>
    /// 创建表.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="tableModel">表对象.</param>
    /// <param name="tableFieldList">字段对象.</param>
    /// <returns></returns>
    public async Task<bool> Create(DbLinkEntity link, DbTableModel tableModel, List<DbTableFieldModel> tableFieldList)
    {
        if (link != null)
            _sqlSugarClient = ChangeDataBase(link);

        if (_sqlSugarClient.CurrentConnectionConfig.DbType == SqlSugar.DbType.MySql)
            await CreateTableMySql(tableModel, tableFieldList);
        else
            CreateTable(tableModel, tableFieldList);

        return true;
    }

    /// <summary>
    /// sqlsugar添加表字段.
    /// </summary>
    /// <param name="tableName">表名.</param>
    /// <param name="tableFieldList">表字段集合.</param>
    public void AddTableColumn(string tableName, List<DbTableFieldModel> tableFieldList)
    {
        try
        {
            var cloumnList = tableFieldList.Adapt<List<DbColumnInfo>>();
            DelDataLength(cloumnList);
            foreach (var item in cloumnList)
            {
                _sqlSugarClient.DbMaintenance.AddColumn(tableName, item);
                _sqlSugarClient.DbMaintenance.AddColumnRemark(item.DbColumnName, tableName, item.ColumnDescription);
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    /// <summary>
    /// 删除表.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="table">表名.</param>
    /// <returns></returns>
    public bool Delete(DbLinkEntity link, string table)
    {
        if (link != null)
            _sqlSugarClient = ChangeDataBase(link);

        try
        {
            db.BeginTran();

            _sqlSugarClient.DbMaintenance.DropTable(table);

            db.CommitTran();
            return true;
        }
        catch (Exception)
        {
            db.RollbackTran();
            throw;
        }
    }

    /// <summary>
    /// 修改表.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="oldTable">原数据.</param>
    /// <param name="tableModel">表对象.</param>
    /// <param name="tableFieldList">字段对象.</param>
    /// <returns></returns>
    public async Task<bool> Update(DbLinkEntity link, string oldTable, DbTableModel tableModel, List<DbTableFieldModel> tableFieldList)
    {
        if (link != null)
            _sqlSugarClient = ChangeDataBase(link);

        try
        {
            db.BeginTran();

            _sqlSugarClient.DbMaintenance.DropTable(oldTable);

            if (_sqlSugarClient.CurrentConnectionConfig.DbType == SqlSugar.DbType.MySql)
                await CreateTableMySql(tableModel, tableFieldList);
            else
                CreateTable(tableModel, tableFieldList);

            db.CommitTran();

            return true;
        }
        catch (Exception)
        {
            db.RollbackTran();
            throw;
        }
    }

    /// <summary>
    /// 根据链接获取分页数据.
    /// </summary>
    /// <returns></returns>
    public PageResult<Dictionary<string, object>> GetInterFaceData(DbLinkEntity link, string strSql, VisualDevModelListQueryInput pageInput, MainBeltViceQueryModel columnDesign, List<IConditionalModel> dataPermissions, Dictionary<string, string> outColumnName = null)
    {
        if (link != null)
            _sqlSugarClient = ChangeDataBase(link);

        try
        {
            int total = 0;
            List<IConditionalModel> conModels = GetSqlCondition(pageInput, columnDesign);

            if (_sqlSugarClient.CurrentConnectionConfig.DbType == SqlSugar.DbType.Oracle)
            {
                strSql = strSql.Replace(";", string.Empty);
            }

            var sidx = pageInput.sidx.IsNotEmptyOrNull() && pageInput.sort.IsNotEmptyOrNull(); // 按前端参数排序
            var defaultSidx = columnDesign.defaultSidx.IsNotEmptyOrNull() && columnDesign.sort.IsNotEmptyOrNull(); // 按模板默认排序

            // 判断是否为定制的数据处理
            DataTable dt = null;
            bool handle = false;
            if (columnDesign.mainTableName.IsNotEmptyOrNull() && columnDesign.mainTableName!.StartsWith(_qtfuncPrefix))
            {
                var qtfunc = _sqlSugarClient.SqlQueryable<QTFuncModel>($"select * from {columnDesign.mainTableName}").First();
                if (qtfunc != null && qtfunc.__type.HasValue)
                {
                    // 执行qtfun
                    var response = ExecuteQtFunc(qtfunc, pageInput, conModels, dataPermissions);
                    if (response.success)
                    {
                        handle = true;
                        dt = response.dataTable;
                        total = response.total;
                    }
                }
            }
            if (!handle)
            {
                dt = _sqlSugarClient.SqlQueryable<object>(strSql).Where(conModels).Where(dataPermissions)
                .OrderByIF(sidx, pageInput.sidx + " " + pageInput.sort)
                .OrderByIF(!sidx && defaultSidx, columnDesign.defaultSidx + " " + columnDesign.sort)
                .ToDataTablePage(pageInput.currentPage, pageInput.pageSize, ref total);
            }

            // 如果有字段别名 替换 ColumnName
            if (outColumnName != null && outColumnName.Count > 0)
            {
                var resultKey = string.Empty;
                for (var i = 0; i < dt.Columns.Count; i++)
                    dt.Columns[i].ColumnName = outColumnName.TryGetValue(dt.Columns[i].ColumnName.ToUpper(), out resultKey) == true ? outColumnName[dt.Columns[i].ColumnName.ToUpper()] : dt.Columns[i].ColumnName.ToUpper();
            }

            return new PageResult<Dictionary<string, object>>()
            {
                pagination = new PageResult()
                {
                    pageIndex = pageInput.currentPage,
                    pageSize = pageInput.pageSize,
                    total = total
                },
                list = dt.ToObject<List<Dictionary<string, object>>>()
            };
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    /// <summary>
    /// 解析查询条件
    /// </summary>
    /// <param name="pageInput"></param>
    /// <param name="columnDesign"></param>
    /// <returns></returns>
    public List<IConditionalModel> GetSqlCondition(VisualDevModelListQueryInput pageInput, MainBeltViceQueryModel columnDesign)
    {
        // 将查询的关键字json转成Dictionary
        Dictionary<string, object> keywordJsonDic = string.IsNullOrEmpty(pageInput.queryJson) ? null : pageInput.queryJson.ToObject<Dictionary<string, object>>();
        var conModels = new List<IConditionalModel>();
        if (keywordJsonDic != null)
        {
            foreach (KeyValuePair<string, object> item in keywordJsonDic)
            {
                ListSearchParametersModel model = columnDesign.searchList.Find(it => it.vModel.Equals(item.Key));
                switch (model.qtKey)
                {
                    case QtKeyConst.DATE:
                        {
                            if (model.conditionalType == nameof(ConditionalType.Equal))
                            {
                                var startTime = item.Value?.ToString().TimeStampToDateTime();

                                conModels.Add(new ConditionalModel
                                {
                                    FieldName = item.Key,
                                    ConditionalType = ConditionalType.Equal,
                                    FieldValue = startTime.ToString(),
                                    FieldValueConvertFunc = it => Convert.ToDateTime(it)
                                });
                            }
                            else
                            {
                                var timeRange = item.Value.ToObject<List<string>>();
                                var startTime = timeRange.First().TimeStampToDateTime();
                                var endTime = timeRange.Last().TimeStampToDateTime();
                                if (model.format == "yyyy-MM-dd HH:mm:ss")
                                {
                                    conModels.Add(new ConditionalModel
                                    {
                                        FieldName = item.Key,
                                        ConditionalType = ConditionalType.GreaterThanOrEqual,
                                        FieldValue = startTime.ToString(),
                                        FieldValueConvertFunc = it => Convert.ToDateTime(it)
                                    });
                                    conModels.Add(new ConditionalModel
                                    {
                                        FieldName = item.Key,
                                        ConditionalType = ConditionalType.LessThanOrEqual,
                                        FieldValue = endTime.ToString(),
                                        FieldValueConvertFunc = it => Convert.ToDateTime(it)
                                    });
                                }
                                else
                                {
                                    conModels.Add(new ConditionalModel
                                    {
                                        FieldName = item.Key,
                                        ConditionalType = ConditionalType.GreaterThanOrEqual,
                                        FieldValue = new DateTime(startTime.Year, startTime.Month, startTime.Day, 0, 0, 0, 0).ToString(),
                                        FieldValueConvertFunc = it => Convert.ToDateTime(it)
                                    });
                                    conModels.Add(new ConditionalModel
                                    {
                                        FieldName = item.Key,
                                        ConditionalType = ConditionalType.LessThanOrEqual,
                                        FieldValue = new DateTime(endTime.Year, endTime.Month, endTime.Day, 23, 59, 59, 999).ToString(),
                                        FieldValueConvertFunc = it => Convert.ToDateTime(it)
                                    });
                                }
                            }

                        }

                        break;
                    case QtKeyConst.TIME:
                        {
                            var timeRange = item.Value.ToObject<List<string>>();
                            var startTime = timeRange.First().TimeStampToDateTime();
                            var endTime = timeRange.Last().TimeStampToDateTime();
                            conModels.Add(new ConditionalModel
                            {
                                FieldName = item.Key,
                                ConditionalType = ConditionalType.GreaterThanOrEqual,
                                FieldValue = new DateTime(startTime.Year, startTime.Month, startTime.Day, startTime.Hour, startTime.Minute, startTime.Second, 0).ToString(),
                                FieldValueConvertFunc = it => Convert.ToDateTime(it)
                            });
                            conModels.Add(new ConditionalModel
                            {
                                FieldName = item.Key,
                                ConditionalType = ConditionalType.LessThanOrEqual,
                                FieldValue = new DateTime(endTime.Year, endTime.Month, endTime.Day, endTime.Hour, endTime.Minute, endTime.Second, 0).ToString(),
                                FieldValueConvertFunc = it => Convert.ToDateTime(it)
                            });
                        }

                        break;
                    case QtKeyConst.CREATETIME:
                    case QtKeyConst.MODIFYTIME:
                        {
                            var timeRange = item.Value.ToObject<List<string>>();
                            var startTime = timeRange.First().TimeStampToDateTime();
                            var endTime = timeRange.Last().TimeStampToDateTime();
                            conModels.Add(new ConditionalModel
                            {
                                FieldName = item.Key,
                                ConditionalType = ConditionalType.GreaterThanOrEqual,
                                FieldValue = new DateTime(startTime.Year, startTime.Month, startTime.Day, 0, 0, 0, 0).ToString(),
                                FieldValueConvertFunc = it => Convert.ToDateTime(it)
                            });
                            conModels.Add(new ConditionalModel
                            {
                                FieldName = item.Key,
                                ConditionalType = ConditionalType.LessThanOrEqual,
                                FieldValue = new DateTime(endTime.Year, endTime.Month, endTime.Day, 23, 59, 59, 999).ToString(),
                                FieldValueConvertFunc = it => Convert.ToDateTime(it)
                            });
                        }

                        break;
                    case QtKeyConst.NUMINPUT:
                    case QtKeyConst.CALCULATE:
                        {
                            List<string> numArray = item.Value.ToObject<List<string>>();
                            var startNum = numArray.First().ParseToInt();
                            var endNum = numArray.Last() == null ? Int64.MaxValue : numArray.Last().ParseToInt();
                            conModels.Add(new ConditionalModel { FieldName = item.Key, ConditionalType = ConditionalType.GreaterThanOrEqual, FieldValue = startNum.ToString() });
                            conModels.Add(new ConditionalModel { FieldName = item.Key, ConditionalType = ConditionalType.LessThanOrEqual, FieldValue = endNum.ToString() });
                        }

                        break;
                    case QtKeyConst.CHECKBOX:
                        {
                            conModels.Add(new ConditionalModel { FieldName = item.Key, ConditionalType = ConditionalType.Like, FieldValue = item.Value.ToString() });
                        }

                        break;
                    case QtKeyConst.POSSELECT:
                    case QtKeyConst.USERSELECT:
                    case QtKeyConst.DEPSELECT:
                        {
                            if (model.multiple)
                            {
                                conModels.Add(new ConditionalModel { FieldName = item.Key, ConditionalType = ConditionalType.Like, FieldValue = item.Value.ToString() });
                            }
                            else
                            {
                                conModels.Add(new ConditionalModel { FieldName = item.Key, ConditionalType = ConditionalType.Equal, FieldValue = item.Value.ToJsonString() });
                            }
                        }

                        break;
                    case QtKeyConst.TREESELECT:
                        {
                            if (item.Value.IsNotEmptyOrNull() && item.Value.ToString().Contains("["))
                            {
                                var value = item.Value.ToObject<List<string>>();
                                if (value.Any()) conModels.Add(new ConditionalModel { FieldName = item.Key, ConditionalType = ConditionalType.Like, FieldValue = value.LastOrDefault() });
                            }
                            else
                            {
                                conModels.Add(new ConditionalModel { FieldName = item.Key, ConditionalType = ConditionalType.Like, FieldValue = item.Value.ToString() });
                            }
                        }

                        break;
                    case QtKeyConst.CASCADER:
                    case QtKeyConst.ADDRESS:
                    case QtKeyConst.CURRORGANIZE:
                    case QtKeyConst.COMSELECT:
                        {
                            // 多选时为模糊查询
                            if (model.multiple)
                            {
                                var value = item.Value?.ToString().ToObject<List<string>>();
                                if (value.Any()) conModels.Add(new ConditionalModel { FieldName = item.Key, ConditionalType = ConditionalType.Like, FieldValue = item.Value.ToJsonString().Replace("[", string.Empty) });
                            }
                            else
                            {
                                conModels.Add(new ConditionalModel { FieldName = item.Key, ConditionalType = ConditionalType.Equal, FieldValue = item.Value.ToJsonString() });
                            }
                        }

                        break;
                    case QtKeyConst.SELECT:
                        {
                            // 多选时为模糊查询
                            if (model.multiple)
                            {
                                // 判断value 是否可以转 string数组
                                bool isArray = false;

                                try
                                {
                                    var array = item.Value.ToObject<string[]>();
                                    conModels.Add(new ConditionalModel
                                    {
                                        FieldName = item.Key,
                                        ConditionalType = ConditionalType.In,
                                        FieldValue = string.Join(",", array)
                                    });
                                    isArray = true;
                                }
                                catch (Exception ex)
                                {
                                    isArray = false;
                                }

                                if (!isArray)
                                {
                                    conModels.Add(new ConditionalModel
                                    {
                                        FieldName = item.Key,
                                        ConditionalType = ConditionalType.In,
                                        FieldValue = item.Value.ToJsonString(),
                                        FieldValueConvertFunc = it => it.ToObject<string[]>()
                                    });
                                }
                               
                            }
                            else
                            {
                                conModels.Add(new ConditionalModel { FieldName = item.Key, ConditionalType = ConditionalType.Equal, FieldValue = item.Value.ToString() });
                            }
                        }

                        break;
                    default:
                        {
                            if (!string.IsNullOrEmpty(model.conditionalType) && System.Enum.TryParse<ConditionalType>(model.conditionalType, out var type))
                            {
                                conModels.Add(new ConditionalModel { FieldName = item.Key, ConditionalType = type, FieldValue = item.Value.ToString() });
                            }
                            else
                            {
                                if (model.searchType == 2)
                                {
                                    conModels.Add(new ConditionalModel { FieldName = item.Key, ConditionalType = ConditionalType.Like, FieldValue = item.Value.ToString() });
                                }
                                else if (model.searchType == 1)
                                {
                                    conModels.Add(new ConditionalModel { FieldName = item.Key, ConditionalType = ConditionalType.Equal, FieldValue = item.Value.ToJsonString() });
                                }
                            }
                        }

                        break;
                }
            }
        }

        return conModels;
    }


    /// <summary>
    /// 表是否存在.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="table">表名.</param>
    /// <returns></returns>
    public bool IsAnyTable(DbLinkEntity link, string table)
    {
        if (link != null)
            _sqlSugarClient = ChangeDataBase(link);

        return _sqlSugarClient.DbMaintenance.IsAnyTable(table, false);
    }

    /// <summary>
    /// 获取表字段列表.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="tableName">表名.</param>
    /// <returns>TableFieldListModel.</returns>
    public List<DbTableFieldModel> GetFieldList(DbLinkEntity? link, string? tableName)
    {
        if (link != null) _sqlSugarClient = ChangeDataBase(link);

        var list = _sqlSugarClient.DbMaintenance.GetColumnInfosByTableName(tableName, false);
        return list.Adapt<List<DbTableFieldModel>>();
    }

    /// <summary>
    /// 获取表数据.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="tableName">表名.</param>
    /// <returns></returns>
    public DataTable GetData(DbLinkEntity link, string tableName)
    {
        if (link != null)
            _sqlSugarClient = ChangeDataBase(link);

        return _sqlSugarClient.Queryable<dynamic>().AS(tableName).ToDataTable();
    }

    /// <summary>
    /// 根据链接获取数据.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="strSql">sql语句.</param>
    /// <param name="parameters">参数.</param>
    /// <returns></returns>
    public DataTable GetInterFaceData(DbLinkEntity link, string strSql, params SugarParameter[] parameters)
    {
        if (link != null)
            _sqlSugarClient = ChangeDataBase(link);

        if (_sqlSugarClient.CurrentConnectionConfig.DbType == SqlSugar.DbType.Oracle)
            strSql = strSql.Replace(";", string.Empty);

        return _sqlSugarClient.Ado.GetDataTable(strSql, parameters);
    }

    /// <summary>
    /// 执行增删改sql.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="strSql">sql语句.</param>
    /// <param name="parameters">参数.</param>
    public void ExecuteCommand(DbLinkEntity link, string strSql, params SugarParameter[] parameters)
    {
        if (link != null)
            _sqlSugarClient = ChangeDataBase(link);

        if (_sqlSugarClient.CurrentConnectionConfig.DbType == SqlSugar.DbType.Oracle)
            strSql = strSql.Replace(";", string.Empty);

        _sqlSugarClient.Ado.ExecuteCommand(strSql, parameters);
    }

    /// <summary>
    /// 获取表信息.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="tableName">表名.</param>
    /// <returns></returns>
    public DatabaseTableInfoOutput GetDataBaseTableInfo(DbLinkEntity link, string tableName)
    {
        if (link != null)
            _sqlSugarClient = ChangeDataBase(link);

        var data = new DatabaseTableInfoOutput()
        {
            tableInfo = _sqlSugarClient.DbMaintenance.GetTableInfoList(false).Find(m => m.Name == tableName).Adapt<TableInfoOutput>(),
            tableFieldList = _sqlSugarClient.DbMaintenance.GetColumnInfosByTableName(tableName, false).Adapt<List<TableFieldOutput>>()
        };

        data.tableFieldList = ViewDataTypeConversion(data.tableFieldList, _sqlSugarClient.CurrentConnectionConfig.DbType);

        return data;
    }

    /// <summary>
    /// 获取数据库表信息.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <returns></returns>
    public List<DbTableModel> GetDBTableList(DbLinkEntity link)
    {
        if (link != null)
            _sqlSugarClient = ChangeDataBase(link);

        var dbType = link.DbType;
        var sql = DBTableSql(dbType);
        var modelList = _sqlSugarClient.Ado.SqlQuery<DynamicDbTableModel>(sql);
        return modelList.Adapt<List<DbTableModel>>();
    }

    /// <summary>
    /// 获取数据库表信息.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <returns></returns>
    public List<DbTableInfo> GetTableInfos(DbLinkEntity link)
    {
        if (link != null)
            _sqlSugarClient = ChangeDataBase(link);

        return _sqlSugarClient.DbMaintenance.GetTableInfoList(false);
    }

    /// <summary>
    /// 获取数据表分页(SQL语句).
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="dbSql">数据SQL.</param>
    /// <param name="pageIndex">页数.</param>
    /// <param name="pageSize">条数.</param>
    /// <returns></returns>
    public async Task<dynamic> GetDataTablePage(DbLinkEntity link, string dbSql, int pageIndex, int pageSize)
    {
        if (link != null)
            _sqlSugarClient = ChangeDataBase(link);

        RefAsync<int> totalNumber = 0;
        var list = await _sqlSugarClient.SqlQueryable<object>(dbSql).ToDataTablePageAsync(pageIndex, pageSize, totalNumber);

        return PageResult<dynamic>.SqlSugarPageResult(new SqlSugarPagedList<dynamic>()
        {
            list = ToDynamicList(list),
            pagination = new PagedModel()
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                Total = totalNumber
            }
        });
    }

    /// <summary>
    /// 获取数据表分页(实体).
    /// </summary>
    /// <typeparam name="TEntity">T.</typeparam>
    /// <param name="link">数据连接.</param>
    /// <param name="pageIndex">页数.</param>
    /// <param name="pageSize">条数.</param>
    /// <returns></returns>
    public async Task<List<TEntity>> GetDataTablePage<TEntity>(DbLinkEntity link, int pageIndex, int pageSize)
    {
        if (link != null)
            _sqlSugarClient = ChangeDataBase(link);

        return await _sqlSugarClient.Queryable<TEntity>().ToPageListAsync(pageIndex, pageSize);
    }

    /// <summary>
    /// 同步数据.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="dt">同步数据.</param>
    /// <param name="table">表.</param>
    /// <returns></returns>
    public async Task<bool> SyncData(DbLinkEntity link, DataTable dt, string table)
    {
        if (link != null)
            _sqlSugarClient = ChangeDataBase(link);

        List<Dictionary<string, object>> dc = _sqlSugarClient.Utilities.DataTableToDictionaryList(dt); // 5.0.23版本支持
        var isOk = await _sqlSugarClient.Insertable(dc).AS(table).ExecuteCommandAsync();
        return isOk > 0;
    }

    /// <summary>
    /// 同步表操作.
    /// </summary>
    /// <param name="linkFrom">原数据库.</param>
    /// <param name="linkTo">目前数据库.</param>
    /// <param name="table">表名称.</param>
    /// <param name="type">操作类型.</param>
    public void SyncTable(DbLinkEntity linkFrom, DbLinkEntity linkTo, string table, int type)
    {
        try
        {
            switch (type)
            {
                case 2:
                    {
                        if (linkFrom != null)
                            _sqlSugarClient = ChangeDataBase(linkFrom);
                        var columns = _sqlSugarClient.DbMaintenance.GetColumnInfosByTableName(table, false);

                        if (linkTo != null)
                            _sqlSugarClient = ChangeDataBase(linkTo);

                        DelDataLength(columns);

                        _sqlSugarClient.DbMaintenance.CreateTable(table, columns);
                    }

                    break;
                case 3:
                    {
                        if (linkTo != null)
                            _sqlSugarClient = ChangeDataBase(linkTo);

                        _sqlSugarClient.DbMaintenance.TruncateTable(table);
                    }

                    break;
                default:
                    break;
            }
        }
        catch (Exception)
        {

            throw;
        }
    }

    /// <summary>
    /// 使用存储过程.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <param name="stored">存储过程名称.</param>
    /// <param name="parameters">参数.</param>
    public void UseStoredProcedure(DbLinkEntity link, string stored, List<SugarParameter> parameters)
    {
        if (link != null)
            _sqlSugarClient = ChangeDataBase(link);

        _sqlSugarClient.Ado.UseStoredProcedure().GetDataTable(stored, parameters);
    }

    /// <summary>
    /// 测试数据库连接.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <returns></returns>
    public bool IsConnection(DbLinkEntity link)
    {
        try
        {
            if (link != null)
                _sqlSugarClient = ChangeDataBase(link);

            _sqlSugarClient.Open();
            _sqlSugarClient.Close();
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    /// <summary>
    /// 视图数据类型转换.
    /// </summary>
    /// <param name="fields">字段数据.</param>
    /// <param name="databaseType">数据库类型.</param>
    public List<TableFieldOutput> ViewDataTypeConversion(List<TableFieldOutput> fields, SqlSugar.DbType databaseType)
    {
        foreach (var item in fields)
        {
            item.dataType = item.dataType.ToLower();
            switch (item.dataType)
            {
                case "string":
                    {
                        item.dataType = "varchar";
                        if (item.dataLength.ParseToInt() > 2000)
                        {
                            item.dataType = "text";
                            item.dataLength = "50";
                        }
                    }

                    break;
                case "single":
                    item.dataType = "decimal";
                    break;
            }
        }

        return fields;
    }

    /// <summary>
    /// 转换数据库类型.
    /// </summary>
    /// <param name="dbType">数据库类型.</param>
    /// <returns></returns>
    public SqlSugar.DbType ToDbType(string dbType)
    {
        switch (dbType.ToLower())
        {
            case "sqlserver":
                return SqlSugar.DbType.SqlServer;
            case "mysql":
                return SqlSugar.DbType.MySql;
            case "oracle":
                return SqlSugar.DbType.Oracle;
            case "dm8":
            case "dm":
                return SqlSugar.DbType.Dm;
            case "kdbndp":
            case "kingbasees":
                return SqlSugar.DbType.Kdbndp;
            case "postgresql":
                return SqlSugar.DbType.PostgreSQL;
            default:
                throw Oops.Oh(ErrorCode.D1505);
        }
    }

    /// <summary>
    /// 转换连接字符串.
    /// </summary>
    /// <param name="dbLinkEntity">数据连接.</param>
    /// <returns></returns>
    public string ToConnectionString(DbLinkEntity dbLinkEntity)
    {
        switch (dbLinkEntity.DbType.ToLower())
        {
            case "sqlserver":
                return string.Format("Data Source={0},{4};Initial Catalog={1};User ID={2};Password={3};Connection Timeout=5;MultipleActiveResultSets=true", dbLinkEntity.Host, dbLinkEntity.ServiceName, dbLinkEntity.UserName, dbLinkEntity.Password, dbLinkEntity.Port);
            case "oracle":
                var oracleParam = dbLinkEntity.OracleParam.ToObject<OracleParamModel>();
                return string.Format("Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={0})(PORT={1}))(CONNECT_DATA=(SERVER = DEDICATED)(SERVICE_NAME={2})));User Id={3};Password={4}", dbLinkEntity.Host, dbLinkEntity.Port.ToString(), oracleParam.oracleService, dbLinkEntity.UserName, dbLinkEntity.Password);
            case "mysql":
                return string.Format("server={0};port={1};database={2};user={3};password={4};AllowLoadLocalInfile=true", dbLinkEntity.Host, dbLinkEntity.Port.ToString(), dbLinkEntity.ServiceName, dbLinkEntity.UserName, dbLinkEntity.Password);
            case "dm8":
            case "dm":
                return string.Format("server={0};port={1};database={2};User Id={3};PWD={4}", dbLinkEntity.Host, dbLinkEntity.Port.ToString(), dbLinkEntity.ServiceName, dbLinkEntity.UserName, dbLinkEntity.Password);
            case "kdbndp":
            case "kingbasees":
                return string.Format("server={0};port={1};database={2};UID={3};PWD={4}", dbLinkEntity.Host, dbLinkEntity.Port.ToString(), dbLinkEntity.ServiceName, dbLinkEntity.UserName, dbLinkEntity.Password);
            case "postgresql":
                return string.Format("server={0};port={1};Database={2};User Id={3};Password={4}", dbLinkEntity.Host, dbLinkEntity.Port.ToString(), dbLinkEntity.ServiceName, dbLinkEntity.UserName, dbLinkEntity.Password);
            default:
                throw Oops.Oh(ErrorCode.D1505);
        }
    }

    /// <summary>
    /// DataTable转DicList.
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    private List<Dictionary<string, object>> DataTableToDicList(DataTable dt)
    {
        return dt.AsEnumerable().Select(row => dt.Columns.Cast<DataColumn>().ToDictionary(column => column.ColumnName.ToLower(), column => row[column])).ToList();
    }

    /// <summary>
    /// 将DataTable 转换成 List<dynamic>
    /// reverse 反转：控制返回结果中是只存在 FilterField 指定的字段,还是排除.
    /// [flase 返回FilterField 指定的字段]|[true 返回结果剔除 FilterField 指定的字段]
    /// FilterField  字段过滤，FilterField 为空 忽略 reverse 参数；返回DataTable中的全部数
    /// </summary>
    /// <param name="table">DataTable</param>
    /// <param name="reverse">
    /// 反转：控制返回结果中是只存在 FilterField 指定的字段,还是排除.
    /// [flase 返回FilterField 指定的字段]|[true 返回结果剔除 FilterField 指定的字段]
    /// </param>
    /// <param name="FilterField">字段过滤，FilterField 为空 忽略 reverse 参数；返回DataTable中的全部数据</param>
    /// <returns>List<dynamic></returns>
    public static List<dynamic> ToDynamicList(DataTable table, bool reverse = true, params string[] FilterField)
    {
        var modelList = new List<dynamic>();
        foreach (DataRow row in table.Rows)
        {
            dynamic model = new ExpandoObject();
            var dict = (IDictionary<string, object>)model;
            foreach (DataColumn column in table.Columns)
            {
                if (FilterField.Length != 0)
                {
                    if (reverse)
                    {
                        if (!FilterField.Contains(column.ColumnName))
                        {
                            dict[column.ColumnName] = row[column];
                        }
                    }
                    else
                    {
                        if (FilterField.Contains(column.ColumnName))
                        {
                            dict[column.ColumnName] = row[column];
                        }
                    }
                }
                else
                {
                    dict[column.ColumnName.ToLower()] = row[column];
                }
            }

            modelList.Add(model);
        }

        return modelList;
    }

    /// <summary>
    /// 数据库表SQL.
    /// </summary>
    /// <param name="dbType">数据库类型.</param>
    /// <returns></returns>
    private string DBTableSql(string dbType)
    {
        StringBuilder sb = new StringBuilder();
        switch (dbType.ToLower())
        {
            case "sqlserver":
                sb.Append(@"DECLARE @TABLEINFO TABLE ( NAME VARCHAR(50) , SUMROWS VARCHAR(11) , RESERVED VARCHAR(50) , DATA VARCHAR(50) , INDEX_SIZE VARCHAR(50) , UNUSED VARCHAR(50) , PK VARCHAR(50) ) DECLARE @TABLENAME TABLE ( NAME VARCHAR(50) ) DECLARE @NAME VARCHAR(50) DECLARE @PK VARCHAR(50) INSERT INTO @TABLENAME ( NAME ) SELECT O.NAME FROM SYSOBJECTS O , SYSINDEXES I WHERE O.ID = I.ID AND O.XTYPE = 'U' AND I.INDID < 2 ORDER BY I.ROWS DESC , O.NAME WHILE EXISTS ( SELECT 1 FROM @TABLENAME ) BEGIN SELECT TOP 1 @NAME = NAME FROM @TABLENAME DELETE @TABLENAME WHERE NAME = @NAME DECLARE @OBJECTID INT SET @OBJECTID = OBJECT_ID(@NAME) SELECT @PK = COL_NAME(@OBJECTID, COLID) FROM SYSOBJECTS AS O INNER JOIN SYSINDEXES AS I ON I.NAME = O.NAME INNER JOIN SYSINDEXKEYS AS K ON K.INDID = I.INDID WHERE O.XTYPE = 'PK' AND PARENT_OBJ = @OBJECTID AND K.ID = @OBJECTID INSERT INTO @TABLEINFO ( NAME , SUMROWS , RESERVED , DATA , INDEX_SIZE , UNUSED ) EXEC SYS.SP_SPACEUSED @NAME UPDATE @TABLEINFO SET PK = @PK WHERE NAME = @NAME END SELECT F.NAME AS F_TABLE,ISNULL(P.TDESCRIPTION,F.NAME) AS F_TABLENAME, F.RESERVED AS F_SIZE, RTRIM(F.SUMROWS) AS F_SUM, F.PK AS F_PRIMARYKEY FROM @TABLEINFO F LEFT JOIN ( SELECT NAME = CASE WHEN A.COLORDER = 1 THEN D.NAME ELSE '' END , TDESCRIPTION = CASE WHEN A.COLORDER = 1 THEN ISNULL(F.VALUE, '') ELSE '' END FROM SYSCOLUMNS A LEFT JOIN SYSTYPES B ON A.XUSERTYPE = B.XUSERTYPE INNER JOIN SYSOBJECTS D ON A.ID = D.ID AND D.XTYPE = 'U' AND D.NAME <> 'DTPROPERTIES' LEFT JOIN SYS.EXTENDED_PROPERTIES F ON D.ID = F.MAJOR_ID WHERE A.COLORDER = 1 AND F.MINOR_ID = 0 ) P ON F.NAME = P.NAME WHERE 1 = 1 ORDER BY F_TABLE");
                break;
            case "oracle":
                sb.Append(@"SELECT DISTINCT COL.TABLE_NAME AS F_TABLE,TAB.COMMENTS AS F_TABLENAME,0 AS F_SIZE,NVL(T.NUM_ROWS,0)AS F_SUM,COLUMN_NAME AS F_PRIMARYKEY FROM USER_CONS_COLUMNS COL INNER JOIN USER_CONSTRAINTS CON ON CON.CONSTRAINT_NAME=COL.CONSTRAINT_NAME INNER JOIN USER_TAB_COMMENTS TAB ON TAB.TABLE_NAME=COL.TABLE_NAME INNER JOIN USER_TABLES T ON T.TABLE_NAME=COL.TABLE_NAME WHERE CON.CONSTRAINT_TYPE NOT IN('C','R')ORDER BY COL.TABLE_NAME");
                break;
            case "mysql":
                sb.Append(@"SELECT T1.*,(SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.`COLUMNS`WHERE TABLE_SCHEMA=DATABASE()AND TABLE_NAME=T1.F_TABLE AND COLUMN_KEY='PRI')F_PRIMARYKEY FROM(SELECT TABLE_NAME F_TABLE,0 F_SIZE,TABLE_ROWS F_SUM,(SELECT IF(LENGTH(TRIM(TABLE_COMMENT))<1,TABLE_NAME,TABLE_COMMENT))F_TABLENAME FROM INFORMATION_SCHEMA.`TABLES`WHERE TABLE_SCHEMA=DATABASE())T1 ORDER BY T1.F_TABLE");
                break;
            default:
                throw new Exception("不支持");
        }

        return sb.ToString();
    }

    /// <summary>
    /// MySql创建表单+注释.
    /// </summary>
    /// <param name="tableModel">表.</param>
    /// <param name="tableFieldList">字段.</param>
    private async Task CreateTableMySql(DbTableModel tableModel, List<DbTableFieldModel> tableFieldList)
    {
        try
        {
            db.BeginTran();

            StringBuilder strSql = new StringBuilder();
            strSql.Append("CREATE TABLE `" + tableModel.table + "` (\r\n");
            foreach (var item in tableFieldList)
            {
                if (item.primaryKey && item.allowNull == 1)
                    throw Oops.Oh(ErrorCode.D1509);
                strSql.Append(" `" + item.field + "` " + item.dataType.ToUpper() + "");
                if (item.dataType == "varchar" || item.dataType == "nvarchar" || item.dataType == "decimal")
                    strSql.Append(" (" + item.dataLength + ") ");
                if (item.primaryKey)
                    strSql.Append(" primary key ");
                if (item.allowNull != 1)
                    strSql.Append(" NOT NULL ");
                else
                    strSql.Append(" NULL ");
                strSql.Append("COMMENT '" + item.fieldName + "'");
                strSql.Append(",");
            }

            strSql.Remove(strSql.Length - 1, 1);
            strSql.Append("\r\n");
            strSql.Append(") COMMENT = '" + tableModel.tableName + "';");
            await _sqlSugarClient.Ado.ExecuteCommandAsync(strSql.ToString());

            db.CommitTran();
        }
        catch (Exception ex)
        {
            db.RollbackTran();
            throw new Exception(ex.Message);
        }
    }

    /// <summary>
    /// sqlsugar建表.
    /// </summary>
    /// <param name="tableModel">表.</param>
    /// <param name="tableFieldList">字段.</param>
    private void CreateTable(DbTableModel tableModel, List<DbTableFieldModel> tableFieldList)
    {
        try
        {
            db.BeginTran();

            var cloumnList = tableFieldList.Adapt<List<DbColumnInfo>>();
            DelDataLength(cloumnList);
            var isOk = _sqlSugarClient.DbMaintenance.CreateTable(tableModel.table, cloumnList);
            _sqlSugarClient.DbMaintenance.AddTableRemark(tableModel.table, tableModel.tableName);
            foreach (var item in cloumnList)
            {
                _sqlSugarClient.DbMaintenance.AddColumnRemark(item.DbColumnName, tableModel.table, item.ColumnDescription);
            }

            db.CommitTran();
        }
        catch (Exception ex)
        {
            db.RollbackTran();
            throw new Exception(ex.Message);
        }
    }

    /// <summary>
    /// 删除列长度（SqlSugar除了字符串其他不需要类型长度）.
    /// </summary>
    /// <param name="dbColumnInfos"></param>
    private void DelDataLength(List<DbColumnInfo> dbColumnInfos)
    {
        foreach (var item in dbColumnInfos)
        {
            if (item.DataType != "varchar")
                item.Length = 0;
            item.DataType = DataTypeConversion(item.DataType, _sqlSugarClient.CurrentConnectionConfig.DbType);
        }
    }

    /// <summary>
    /// 数据库数据类型转换.
    /// </summary>
    /// <param name="dataType">数据类型.</param>
    /// <param name="databaseType">数据库类型</param>
    /// <returns></returns>
    private string DataTypeConversion(string dataType, SqlSugar.DbType databaseType)
    {
        if (databaseType.Equals(SqlSugar.DbType.Oracle))
        {
            switch (dataType)
            {
                case "text":
                    return "CLOB";
                case "decimal":
                    return "DECIMAL(38,38)";
                case "datetime":
                    return "DATE";
                case "bigint":
                    return "NUMBER";
                default:
                    return dataType.ToUpper();
            }
        }
        else if (databaseType.Equals(SqlSugar.DbType.Dm))
        {
            return dataType.ToUpper();
        }
        else if (databaseType.Equals(SqlSugar.DbType.Kdbndp))
        {
            switch (dataType)
            {
                case "int":
                    return "NUMBER";
                case "datetime":
                    return "DATE";
                case "bigint":
                    return "INT8";
                default:
                    return dataType.ToUpper();
            }
        }
        else if (databaseType.Equals(SqlSugar.DbType.PostgreSQL))
        {
            switch (dataType)
            {
                case "varchar":
                    return "varchar";
                case "int":
                    return "NUMBER";
                case "datetime":
                    return "DATE";
                case "decimal":
                    return "DECIMAL";
                case "bigint":
                    return "INT8";
                case "text":
                    return "TEXT";
                default:
                    return dataType;
            }
        }
        else
        {
            return dataType;
        }
    }

    /// <summary>
    /// 获取数据库视图信息.
    /// </summary>
    /// <param name="link">数据连接.</param>
    /// <returns></returns>
    public List<DbTableInfo> GetViewInfos(DbLinkEntity link)
    {
        if (link != null)
            _sqlSugarClient = ChangeDataBase(link);

        return _sqlSugarClient.DbMaintenance.GetViewInfoList(false);
    }



    #region ExecuteQtFunc 执行特殊的处理方法

    /// <summary>
    /// 执行特殊的处理方法 目前只支持同步方法
    /// </summary>
    /// <param name="qtfunc"></param>
    /// <param name="pageInput"></param>
    /// <param name="conModels"></param>
    /// <param name="dataPermissions"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private ExecuteQtFuncResult ExecuteQtFunc(QTFuncModel qtfunc, VisualDevModelListQueryInput pageInput, List<IConditionalModel> conModels, List<IConditionalModel> dataPermissions)
    {
        ExecuteQtFuncResult result = new ExecuteQtFuncResult();
        switch (qtfunc.__type)
        {
            case 1:
                //存储过程 名称|参数(多个逗号相连)
                {
                    if (qtfunc.__func.IsNullOrEmpty())
                    {
                        throw Oops.Oh("缺少存储过程！");
                    }
                    var arr = qtfunc.__func.Split("|");
                    var sql = $"CALL {arr[0]}";

                    List<SugarParameter> sugarParameters = new List<SugarParameter>();
                    //有配置入参名称
                    if (arr.Length>1)
                    {
                        sugarParameters = arr[1].Split(",", true).Select(it => new SugarParameter(it, DBNull.Value)).ToList();
                    }
                    foreach (var item in conModels)
                    {
                        if (item is ConditionalModel conditionalModel)
                        {
                            var param = sugarParameters.Find(it => it.ParameterName == conditionalModel.FieldName);
                            if (param!=null)
                            {
                                param.Value = conditionalModel.FieldValueConvertFunc != null ? conditionalModel.FieldValueConvertFunc.Invoke(conditionalModel.FieldValue) : conditionalModel.FieldValue;

                                if (param.Value!=null && param.Value is IEnumerable<object> enumable)
                                {
                                    param.Value = string.Join(",", enumable);
                                }
                            } 
                        }
                    }
                    if (sugarParameters.IsAny())
                    {
                        sql = $"{sql} ({string.Join(",", sugarParameters.Select(it => $"@{it.ParameterName}"))})";
                    }
                    //var ado = _sqlSugarClient.Ado.UseStoredProcedure();
                    //var cmd = ado.GetCommand(sql, sugarParameters.ToArray());
                    //result.dataTable = _sqlSugarClient.Ado.GetDataTable(sql, sugarParameters.ToArray());
                    result.dataTable = _sqlSugarClient.Ado.GetDataTable(sql, sugarParameters.ToArray());
                    result.success = true;
                    result.total = result.dataTable.Rows.Count;
                }
                break;
            case 2:
                // 类:方法
                {
                    if (qtfunc.__func.IsNullOrEmpty())
                    {
                        throw Oops.Oh("缺少处理方法！");
                    }
                    var arr = qtfunc.__func.Split(":").ToArray();
                    if (arr.Length != 2)
                    {
                        throw Oops.Oh("缺少处理方法！");
                    }
                    var className = arr[0];
                    var methodName = arr[1];
                    var classType = App.EffectiveTypes.FirstOrDefault(it => it.FullName == className);
                    if (classType == null)
                    {
                        throw Oops.Oh("缺少处理方法！");
                    }
                    var methodInfo = classType.GetMethod(methodName);
                    if (methodInfo == null)
                    {
                        throw Oops.Oh("缺少处理方法！");
                    }
                    var service = App.GetService(classType) as IExecuteQtFunc;
                    if (service == null)
                    {
                        throw Oops.Oh("缺少处理方法！");
                    }
                    result = service.Execute(_sqlSugarClient, pageInput, conModels, dataPermissions);
                }

                break;
            default:
                throw new NotImplementedException();
        }

        return result;
    } 
    #endregion
}