using QT.Common.Const;
using QT.Common.Core.Manager;
using QT.Common.Extension;
using QT.Common.Models;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.RemoteRequest;
using QT.Systems.Entitys.Model.DataBase;
using QT.Systems.Entitys.Permission;
using QT.Systems.Entitys.System;
using QT.Systems.Interfaces.System;
using QT.UnifyResult;
using QT.VisualDev.Engine.Enum.VisualDevModelData;
using QT.VisualDev.Entitys;
using QT.VisualDev.Entitys.Dto.VisualDevModelData;
using QT.VisualDev.Interfaces;
using QT.WorkFlow.Entitys.Entity;
using Newtonsoft.Json.Linq;
using SqlSugar;

namespace QT.VisualDev.Engine.Core;

/// <summary>
/// 模板表单列表数据解析.
/// </summary>
public class FormDataParsing : ITransient
{
    #region 构造

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 切库.
    /// </summary>
    private readonly IDataBaseManager _databaseService;

    /// <summary>
    /// 数据接口.
    /// </summary>
    private readonly IDataInterfaceService _dataInterfaceService;

    /// <summary>
    /// 缓存管理.
    /// </summary>
    private readonly ICacheManager _cacheManager;

    /// <summary>
    /// 多租户事务.
    /// </summary>
    private readonly ISqlSugarClient _db;

    /// <summary>
    /// 构造.
    /// </summary>
    /// <param name="userManager"></param>
    /// <param name="cacheManager"></param>
    /// <param name="databaseService"></param>
    /// <param name="dataInterfaceService"></param>
    /// <param name="billRuleService"></param>
    /// <param name="context"></param>
    public FormDataParsing(
        IUserManager userManager,
        ICacheManager cacheManager,
        IDataBaseManager databaseService,
        IDataInterfaceService dataInterfaceService,
        ISqlSugarClient context)
    {
        _userManager = userManager;
        _cacheManager = cacheManager;
        _databaseService = databaseService;
        _dataInterfaceService = dataInterfaceService;
        _db = context;
    }
    #endregion

    #region 解析模板数据

    /// <summary>
    /// 控制模板数据转换.
    /// </summary>
    /// <param name="data">数据.</param>
    /// <param name="fieldsModel">数据模板.</param>
    /// <param name="actionType">操作类型(List-列表值,create-创建值,update-更新值,detail-详情值,transition-过渡值,query-查询).</param>
    /// <returns>object.</returns>
    public object TemplateControlsDataConversion(object data, FieldsModel fieldsModel, string? actionType = null)
    {
        if (fieldsModel == null || data == null || data.Equals("[]") || string.IsNullOrEmpty(data.ToString())) return string.Empty;
        try
        {
            object conversionData = new object();
            switch (fieldsModel.__config__.qtKey)
            {
                case QtKeyConst.SWITCH: // 开关
                case QtKeyConst.RATE: // 评分
                case QtKeyConst.NUMINPUT: conversionData = data.ParseToInt(); // 数字输入
                    break;
                case QtKeyConst.QTAMOUNT: conversionData = data.ParseToDecimal(); // 金额输入
                    break;
                case QtKeyConst.CHECKBOX: // 多选框组
                    {
                        switch (actionType)
                        {
                            case "update":
                            case "create":
                                if (data.ToString().Contains("[")) conversionData = data.ToJsonString();
                                else conversionData = data.ToString();
                                break;
                            default:
                                if (data.ToString().Contains("[")) conversionData = data.ToString().ToObject<List<string>>();
                                else conversionData = data.ToString();
                                break;
                        }
                    }

                    break;
                case QtKeyConst.SELECT: // 下拉选择
                    {
                        switch (actionType)
                        {
                            case "transition": conversionData = data;
                                break;
                            case "update":
                            case "create":
                                if (data.ToString().Contains("[")) conversionData = data.ToJsonString();
                                else conversionData = data.ToString();
                                break;
                            default:
                                if (fieldsModel.multiple && actionType != "query")
                                {
                                    if (data.ToString().Contains("[")) conversionData = data.ToString().ToObject<List<string>>();
                                    else if (data.ToString().Contains(",")) conversionData = string.Join(",", data.ToString().Split(',').ToArray());
                                    else conversionData = data.ToString();
                                }
                                else
                                {
                                    conversionData = data.ToString();
                                }

                                break;
                        }
                    }

                    break;
                case QtKeyConst.TIME: // 时间选择
                    {
                        switch (actionType)
                        {
                            case "List":
                                DateTime dtDate;
                                if (DateTime.TryParse(data.ToString(), out dtDate))
                                    conversionData = string.Format("{0:HH:mm:ss} ", data.ToString());
                                else
                                    conversionData = string.Format("{0:HH:mm:ss} ", data.ToString().TimeStampToDateTime());
                                break;
                            case "create":
                                string? date = string.Format("{0:yyyy-MM-dd}", DateTime.MaxValue);
                                conversionData = string.Format("{0:yyyy-MM-dd HH:mm:ss}", date + " " + data);
                                break;
                            case "update":
                                string? udate = string.Format("{0:yyyy-MM-dd}", DateTime.MaxValue);
                                conversionData = string.Format("{0:yyyy-MM-dd HH:mm:ss}", udate + " " + data);
                                try
                                {
                                    DateTime.Parse(conversionData.ToString());
                                }
                                catch
                                {
                                    conversionData = string.Format("{0:yyyy-MM-dd HH:mm:ss} ", data.ToString()).TimeStampToDateTime().ToString("yyyy-MM-dd HH:mm:ss");
                                }

                                break;
                            case "detail":
                                try
                                {
                                    DateTime.Parse(data.ToString());
                                    conversionData = string.Format("{0:HH:mm:ss} ", data.ToString());
                                }
                                catch
                                {
                                    conversionData = string.Format("{0:HH:mm:ss} ", data.ToString().TimeStampToDateTime());
                                }

                                break;
                            default:
                                conversionData = data;
                                break;
                        }
                    }

                    break;
                case QtKeyConst.TIMERANGE: // 时间范围
                    {
                        switch (actionType)
                        {
                            case "transition":
                                conversionData = data;
                                break;
                            default:
                                conversionData = data.ToString().ToObject<List<string>>();
                                break;
                        }
                    }

                    break;
                case QtKeyConst.DATE: // 日期选择
                    {
                        switch (actionType)
                        {
                            case "List":
                                DateTime dtDate;
                                if (DateTime.TryParse(data.ToString(), out dtDate))
                                    conversionData = data.ToString();
                                else
                                    conversionData = string.Format("{0:yyyy-MM-dd HH:mm:ss}", data.ToString().TimeStampToDateTime());
                                break;
                            case "create":
                                conversionData = string.Format("{0:yyyy-MM-dd HH:mm:ss}", data.ToString().TimeStampToDateTime());
                                break;
                            case "detail":
                                conversionData = data;
                                break;
                            default:
                                try
                                {
                                    DateTime.Parse(data.ToString());
                                    conversionData = string.Format("{0:yyyy-MM-dd HH:mm:ss} ", data.ToString());
                                }
                                catch
                                {
                                    conversionData = string.Format("{0:yyyy-MM-dd HH:mm:ss} ", data.ToString().TimeStampToDateTime());
                                }
                                break;
                        }
                    }

                    break;
                case QtKeyConst.DATERANGE: // 日期范围
                    {
                        switch (actionType)
                        {
                            case "transition":
                                conversionData = data;
                                break;
                            default:
                                conversionData = data.ToString().ToObject<List<string>>();
                                break;
                        }
                    }

                    break;
                case QtKeyConst.CREATETIME: // 创建时间
                case QtKeyConst.MODIFYTIME: // 修改时间
                    {
                        switch (actionType)
                        {
                            case "create":
                                conversionData = data.ToString();
                                break;
                            default:
                                DateTime dtDate;
                                if (DateTime.TryParse(data.ToString(), out dtDate))
                                    conversionData = data.ToString();
                                else
                                    conversionData = string.Format("{0:yyyy-MM-dd HH:mm:ss}", data.ToString().TimeStampToDateTime());
                                break;
                        }
                    }

                    break;
                case QtKeyConst.UPLOADFZ: // 文件上传
                    switch (actionType)
                    {
                        case "update":
                        case "create":
                            if (data.ToString().Contains("[")) conversionData = data.ToJsonString();
                            else conversionData = data.ToString();
                            break;
                        default:
                            if (data.ToJsonString() != "[]")
                            {
                                if (data is List<FileControlsModel>) conversionData = data.ToJsonString().ToObject<List<FileControlsModel>>();
                                else conversionData = data.ToString().ToObject<List<FileControlsModel>>();
                            }
                            else
                            {
                                conversionData = null;
                            }

                            break;
                    }

                    break;
                case QtKeyConst.UPLOADIMG: // 图片上传
                    {
                        switch (actionType)
                        {
                            case "update":
                            case "create":
                                if (data.ToString().Contains("[")) conversionData = data.ToJsonString();
                                else conversionData = data.ToString();
                                break;
                            default:
                                if (data.ToJsonString() != "[]") conversionData = data.ToString().ToObject<List<FileControlsModel>>();
                                else conversionData = null;
                                break;
                        }
                    }

                    break;
                case QtKeyConst.SLIDER: // 滑块
                    if (fieldsModel.range) conversionData = data.ToString().ToObject<List<int>>();
                    else conversionData = data.ParseToInt();
                    break;
                case QtKeyConst.ADDRESS: // 省市区联动
                    {
                        switch (actionType)
                        {
                            case "transition":
                                conversionData = data;
                                break;
                            case "update":
                            case "create":
                                if (data.ToString().Contains("[")) conversionData = data.ToJsonString();
                                else conversionData = data.ToString();
                                break;
                            default:
                                conversionData = data.ToString().ToObject<List<object>>();
                                break;
                        }
                    }

                    break;
                case QtKeyConst.TABLE: // 设计子表
                    break;
                case QtKeyConst.CASCADER: // 级联
                    switch (actionType)
                    {
                        case "transition":
                            conversionData = data;
                            break;
                        case "update":
                        case "create":
                            if (data.ToString().Contains("[")) conversionData = data.ToJsonString();
                            else conversionData = data.ToString();
                            break;
                        default:
                            {
                                if (fieldsModel.props.props.multiple) conversionData = data.ToString().ToObject<List<object>>();
                                else if (data.ToString().Contains("[")) conversionData = data.ToString().ToObject<List<object>>();
                                else conversionData = data;
                            }

                            break;
                    }
                    break;
                case QtKeyConst.COMSELECT: // 公司组件
                    {
                        switch (actionType)
                        {
                            case "transition":
                                {
                                    conversionData = data;
                                }

                                break;
                            case "update":
                            case "create":
                                if (data.ToString().Contains("[")) conversionData = data.ToJsonString();
                                else conversionData = data.ToString();
                                break;
                            case "List":
                                {
                                    if (fieldsModel.multiple)
                                    {
                                        conversionData = data.ToString().ToObject<List<List<object>>>();
                                    }
                                    else
                                    {
                                        conversionData = data;
                                    }
                                }

                                break;
                            default:
                                {
                                    if (fieldsModel.multiple)
                                    {
                                        conversionData = data.ToString().ToObject<List<List<string>>>();
                                    }
                                    else
                                    {
                                        conversionData = data.ToString().ToObject<List<string>>();
                                    }
                                }

                                break;
                        }
                    }

                    break;
                case QtKeyConst.DEPSELECT: // 部门组件
                case QtKeyConst.POSSELECT: // 岗位组件
                case QtKeyConst.USERSELECT: // 用户组件
                case QtKeyConst.TREESELECT: // 树形选择
                    {
                        switch (actionType)
                        {
                            case "transition":
                                conversionData = data;
                                break;
                            case "update":
                            case "create":
                                if (data.ToString().Contains("[")) conversionData = data.ToJsonString();
                                else conversionData = data.ToString();
                                break;
                            default:
                                if (fieldsModel.multiple) conversionData = data.ToString().ToObject<List<string>>();
                                else conversionData = data.ToString();

                                break;
                        }
                    }
                    break;
                default:
                    conversionData = data.ToString();
                    break;
            }

            return conversionData;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    /// <summary>
    /// 获取有表单条数据.
    /// </summary>
    /// <param name="dataList"></param>
    /// <param name="fieldsModels"></param>
    /// <param name="actionType"></param>
    /// <returns></returns>
    public List<Dictionary<string, object>> GetTableDataInfo(List<Dictionary<string, object>> dataList, List<FieldsModel> fieldsModels, string actionType)
    {
        // 转换表字符串成数组
        foreach (var dataMap in dataList)
        {
            int dicCount = dataMap.Keys.Count;
            string[] strKey = new string[dicCount];
            dataMap.Keys.CopyTo(strKey, 0);
            for (int i = 0; i < strKey.Length; i++)
            {
                var dataValue = dataMap[strKey[i]];
                if (dataValue != null && !string.IsNullOrEmpty(dataValue.ToString()))
                {
                    var model = fieldsModels.Find(f => f.__vModel__.Equals(strKey[i]));
                    if (model != null)
                    {
                        dataMap[strKey[i]] = TemplateControlsDataConversion(dataMap[strKey[i]], model, actionType);
                    }
                }
            }
        }
        return dataList;
    }

    /// <summary>
    /// 解析处理 Sql 插入数据的 value.
    /// </summary>
    /// <param name="dbType">数据库 类型</param>
    /// <param name="_tableList">表数据</param>
    /// <param name="field">前端字段名</param>
    /// <param name="data">插入的数据</param>
    /// <param name="_fieldsModelList"></param>
    /// <returns>string.</returns>
    public string InsertValueHandle(string dbType, List<DbTableFieldModel> _tableList, string field, object data, List<FieldsModel> _fieldsModelList)
    {
        // 根据KEY查找模板
        FieldsModel? model = _fieldsModelList.Find(f => f.__vModel__ == field);

        // 单独处理 Oracle,Kdbndp 日期格式转换
        if (dbType == "Oracle" && _tableList.Find(x => x.dataType == "DateTime" && x.field == field.ReplaceRegex(@"(\w+)_qt_", string.Empty)) != null)
            return string.Format("to_date('{0}','yyyy-mm-dd HH24/MI/SS')", TemplateControlsDataConversion(data, model, "create"));
        else
            return string.Format("'{0}'", TemplateControlsDataConversion(data, model, "create"));
    }
    #endregion

    #region 缓存模板数据

    /// <summary>
    /// 获取可视化开发模板可缓存数据.
    /// </summary>
    /// <param name="moldelId">模型id.</param>
    /// <param name="formData">模板数据结构.</param>
    /// <returns>控件缓存数据.</returns>
    public async Task<Dictionary<string, object>> GetVisualDevCaCheData(List<FieldsModel> formData)
    {
        Dictionary<string, object> templateData = new Dictionary<string, object>();
        string? cacheKey = CommonConst.VISUALDEV + _userManager.TenantId + "_";

        // 获取或设置控件缓存数据
        foreach (FieldsModel? model in formData)
        {
            if (model != null && model.__vModel__ != null)
            {
                ConfigModel configModel = model.__config__;
                string fieldName1 = configModel.renderKey + "_" + model.__vModel__;
                switch (configModel.qtKey)
                {
                    case QtKeyConst.RADIO: // 单选框
                    case QtKeyConst.SELECT: // 下拉框
                    case QtKeyConst.CHECKBOX: // 复选框
                        {
                            if (_cacheManager.Exists(cacheKey + fieldName1))
                            {
                                List<Dictionary<string, object>>? list = _cacheManager.Get(cacheKey + fieldName1).ToObject<List<Dictionary<string, object>>>();
                                templateData.Add(cacheKey + fieldName1, list);
                            }
                            else
                            {
                                List<Dictionary<string, string>>? list = new List<Dictionary<string, string>>();
                                if (vModelType.DICTIONARY.ToDescription() == configModel.dataType) list = await GetDictionaryList(configModel.dictionaryType);
                                if (vModelType.DYNAMIC.ToDescription() == configModel.dataType) list = await GetDynamicList(model);
                                if (vModelType.STATIC.ToDescription() == configModel.dataType)
                                {
                                    foreach (Dictionary<string, object>? item in model.__slot__.options)
                                    {
                                        Dictionary<string, string> option = new Dictionary<string, string>();
                                        option.Add(item[model.__config__.props.value].ToString(), item[model.__config__.props.label].ToString());
                                        list.Add(option);
                                    }
                                }

                                templateData.Add(cacheKey + fieldName1, list);
                                _cacheManager.Set(cacheKey + fieldName1, list, TimeSpan.FromMinutes(3));
                            }
                        }

                        break;
                    case QtKeyConst.TREESELECT: // 树形选择
                        {
                            if (_cacheManager.Exists(cacheKey + fieldName1))
                            {
                                List<Dictionary<string, object>>? list = _cacheManager.Get(cacheKey + fieldName1).ToObject<List<Dictionary<string, object>>>();
                                templateData.Add(cacheKey + fieldName1, list);
                            }
                            else
                            {
                                List<Dictionary<string, string>>? list = new List<Dictionary<string, string>>();
                                if (vModelType.DICTIONARY.ToDescription() == configModel.dataType) list = await GetDictionaryList(configModel.dictionaryType);
                                if (vModelType.DYNAMIC.ToDescription() == configModel.dataType) list = await GetDynamicList(model);
                                if (vModelType.STATIC.ToDescription() == configModel.dataType) list = GetStaticList(model);

                                templateData.Add(cacheKey + fieldName1, list);
                                _cacheManager.Set(cacheKey + fieldName1, list, TimeSpan.FromMinutes(3));
                            }
                        }

                        break;
                    case QtKeyConst.CASCADER: // 级联选择
                        {
                            if (_cacheManager.Exists(cacheKey + fieldName1))
                            {
                                List<Dictionary<string, object>>? list = _cacheManager.Get(cacheKey + fieldName1).ToObject<List<Dictionary<string, object>>>();
                                templateData.Add(cacheKey + fieldName1, list);
                            }
                            else
                            {
                                List<Dictionary<string, string>>? list = new List<Dictionary<string, string>>();
                                if (vModelType.DICTIONARY.ToDescription() == configModel.dataType) list = await GetDictionaryList(configModel.dictionaryType);
                                if (vModelType.STATIC.ToDescription() == configModel.dataType) list = GetStaticList(model);

                                templateData.Add(cacheKey + fieldName1, list);
                                _cacheManager.Set(cacheKey + fieldName1, list, TimeSpan.FromMinutes(3));
                            }
                        }

                        break;
                    case QtKeyConst.DEPSELECT: // 部门
                        if (_cacheManager.Exists(cacheKey + fieldName1))
                        {
                            List<Dictionary<string, object>>? list = _cacheManager.Get(cacheKey + fieldName1).ToObject<List<Dictionary<string, object>>>();
                            templateData.Add(cacheKey + fieldName1, list);
                        }
                        else
                        {
                            List<Dictionary<string, object>>? vlist = await GetOrgList(QtKeyConst.DEPSELECT);
                            templateData.Add(cacheKey + fieldName1, vlist);
                            _cacheManager.Set(cacheKey + fieldName1, vlist, TimeSpan.FromMinutes(3));
                        }

                        break;
                    case QtKeyConst.COMSELECT: // 公司
                        if (_cacheManager.Exists(cacheKey + fieldName1))
                        {
                            List<Dictionary<string, object>>? list = _cacheManager.Get(cacheKey + fieldName1).ToObject<List<Dictionary<string, object>>>();
                            templateData.Add(cacheKey + fieldName1, list);
                        }
                        else
                        {
                            List<Dictionary<string, object>>? vlist = await GetOrgList(QtKeyConst.COMSELECT);
                            templateData.Add(cacheKey + fieldName1, vlist);
                            _cacheManager.Set(cacheKey + fieldName1, vlist, TimeSpan.FromMinutes(3));
                        }

                        break;
                    case QtKeyConst.CURRORGANIZE: // 所属组织
                        if (_cacheManager.Exists(cacheKey + fieldName1))
                        {
                            List<Dictionary<string, object>>? list = _cacheManager.Get(cacheKey + fieldName1).ToObject<List<Dictionary<string, object>>>();
                            templateData.Add(cacheKey + fieldName1, list);
                        }
                        else
                        {
                            List<Dictionary<string, object>>? vlist = await GetOrgList(QtKeyConst.CURRORGANIZE);
                            templateData.Add(cacheKey + fieldName1, vlist);
                            _cacheManager.Set(cacheKey + fieldName1, vlist, TimeSpan.FromMinutes(3));
                        }

                        break;
                    case QtKeyConst.POSSELECT: // 岗位
                        {
                            if (_cacheManager.Exists(cacheKey + fieldName1))
                            {
                                List<Dictionary<string, object>>? list = _cacheManager.Get(cacheKey + fieldName1).ToObject<List<Dictionary<string, object>>>();
                                templateData.Add(cacheKey + fieldName1, list);
                            }
                            else
                            {
                                List<PositionEntity>? positionEntityList = await _db.Queryable<PositionEntity>().Where(u => u.DeleteMark == null).ToListAsync();
                                List<Dictionary<string, string>> positionList = new List<Dictionary<string, string>>();
                                foreach (PositionEntity? item in positionEntityList)
                                {
                                    Dictionary<string, string> position = new Dictionary<string, string>();
                                    position.Add(item.Id, item.FullName);
                                    positionList.Add(position);
                                }

                                templateData.Add(cacheKey + fieldName1, positionList);
                                _cacheManager.Set(cacheKey + fieldName1, positionList, TimeSpan.FromMinutes(3));
                            }
                        }

                        break;
                    case QtKeyConst.DICTIONARY: // 数据字典

                        if (_cacheManager.Exists(cacheKey + fieldName1))
                        {
                            List<Dictionary<string, object>>? list = _cacheManager.Get(cacheKey + fieldName1).ToObject<List<Dictionary<string, object>>>();
                            templateData.Add(cacheKey + fieldName1, list);
                        }
                        else
                        {
                            List<Dictionary<string, string>>? vlist = await GetDictionaryList();
                            templateData.Add(cacheKey + fieldName1, vlist);
                            _cacheManager.Set(cacheKey + fieldName1, vlist, TimeSpan.FromMinutes(3));
                        }

                        break;
                }
            }
        }

        #region 省市区 单独处理

        if (formData.Where(x => x.__config__.qtKey == QtKeyConst.ADDRESS).Any())
        {
            bool level = formData.Where(x => x.__config__.qtKey == QtKeyConst.ADDRESS && x.level != 3).Any();
            bool level3 = formData.Where(x => x.__config__.qtKey == QtKeyConst.ADDRESS && x.level == 3).Any();

            string? addCacheKey = CommonConst.VISUALDEV + "_address1";
            string? addCacheKey2 = CommonConst.VISUALDEV + "_address2";
            if (level3)
            {
                if (_cacheManager.Exists(addCacheKey2))
                {
                    templateData.Add(addCacheKey2, _cacheManager.Get(addCacheKey2).ToObject<List<Dictionary<string, object>>>());
                }
                else
                {
                    List<ProvinceEntity>? addressEntityList = await _db.Queryable<ProvinceEntity>().Select(x => new ProvinceEntity { Id = x.Id, ParentId = x.ParentId, Type = x.Type, FullName = x.FullName }).ToListAsync();

                    // 处理省市区树
                    addressEntityList.Where(x => x.Type == "1").ToList().ForEach(item => item.QuickQuery = item.FullName);
                    addressEntityList.Where(x => x.Type == "2").ToList().ForEach(item => item.QuickQuery = addressEntityList.Find(x => x.Id == item.ParentId).QuickQuery + "/" + item.FullName);
                    addressEntityList.Where(x => x.Type == "3").ToList().ForEach(item => item.QuickQuery = addressEntityList.Find(x => x.Id == item.ParentId).QuickQuery + "/" + item.FullName);
                    addressEntityList.Where(x => x.Type == "4").ToList().ForEach(item =>
                    {
                        ProvinceEntity? it = addressEntityList.Find(x => x.Id == item.ParentId);
                        if (it != null) item.QuickQuery = it.QuickQuery + "/" + item.FullName;
                    });

                    // 分开 省市区街道 数据
                    List<Dictionary<string, string>> addressList = new List<Dictionary<string, string>>();
                    foreach (ProvinceEntity? item in addressEntityList.Where(x => x.Type == "4").ToList())
                    {
                        Dictionary<string, string> address = new Dictionary<string, string>();
                        address.Add(item.Id, item.QuickQuery);
                        addressList.Add(address);
                    }

                    // 缓存七天
                    _cacheManager.Set(addCacheKey2, addressList, TimeSpan.FromDays(7));
                    templateData.Add(addCacheKey2, addressList);
                }
            }

            if (level)
            {
                if (_cacheManager.Exists(addCacheKey))
                {
                    templateData.Add(addCacheKey, _cacheManager.Get(addCacheKey).ToObject<List<Dictionary<string, object>>>());
                }
                else
                {
                    List<ProvinceEntity>? addressEntityList = await _db.Queryable<ProvinceEntity>().Select(x => new ProvinceEntity { Id = x.Id, ParentId = x.ParentId, Type = x.Type, FullName = x.FullName }).ToListAsync();

                    // 处理省市区树
                    addressEntityList.Where(x => x.Type == "1").ToList().ForEach(item => item.QuickQuery = item.FullName);
                    addressEntityList.Where(x => x.Type == "2").ToList().ForEach(item => item.QuickQuery = addressEntityList.Find(x => x.Id == item.ParentId).QuickQuery + "/" + item.FullName);
                    addressEntityList.Where(x => x.Type == "3").ToList().ForEach(item => item.QuickQuery = addressEntityList.Find(x => x.Id == item.ParentId).QuickQuery + "/" + item.FullName);

                    // 分开 省市区街道 数据
                    List<Dictionary<string, string>> addressList = new List<Dictionary<string, string>>();
                    foreach (ProvinceEntity? item in addressEntityList.Where(x => x.Type == "1").ToList())
                    {
                        Dictionary<string, string> address = new Dictionary<string, string>();
                        address.Add(item.Id, item.QuickQuery);
                        addressList.Add(address);
                    }

                    foreach (ProvinceEntity? item in addressEntityList.Where(x => x.Type == "2").ToList())
                    {
                        Dictionary<string, string> address = new Dictionary<string, string>();
                        address.Add(item.Id, item.QuickQuery);
                        addressList.Add(address);
                    }

                    foreach (ProvinceEntity? item in addressEntityList.Where(x => x.Type == "3").ToList())
                    {
                        Dictionary<string, string> address = new Dictionary<string, string>();
                        address.Add(item.Id, item.QuickQuery);
                        addressList.Add(address);
                    }

                    // 缓存七天
                    _cacheManager.Set(addCacheKey, addressList, TimeSpan.FromDays(7));
                    templateData.Add(addCacheKey, addressList);
                }
            }
        }
        #endregion

        #region 用户单独处理
        if (formData.Where(x => x.__config__.qtKey == QtKeyConst.USERSELECT).Any())
        {
            string? userCacheKey = CommonConst.VISUALDEV + "_userSelect";
            if (_cacheManager.Exists(userCacheKey))
            {
                templateData.Add(userCacheKey, _cacheManager.Get(userCacheKey).ToObject<List<Dictionary<string, object>>>());
            }
            else
            {
                List<UserEntity>? userEntityList = await _db.Queryable<UserEntity>().Where(x => x.DeleteMark == null).Select(x => new UserEntity() { Id = x.Id, RealName = x.RealName, Account = x.Account }).ToListAsync();

                List<Dictionary<string, string>> userList = new List<Dictionary<string, string>>();
                foreach (UserEntity? item in userEntityList)
                {
                    Dictionary<string, string> user = new Dictionary<string, string>();
                    user.Add(item.Id, item.RealName + "/" + item.Account);
                    userList.Add(user);
                }

                // 缓存30分钟
                _cacheManager.Set(userCacheKey, userList, TimeSpan.FromMinutes(30));
                templateData.Add(userCacheKey, userList);
            }
        }
        #endregion

        return templateData;
    }

    /// <summary>
    /// 处理组织信息.
    /// </summary>
    /// <param name="orgType"></param>
    /// <returns></returns>
    private async Task<List<Dictionary<string, object>>> GetOrgList(string orgType)
    {
        List<OrganizeEntity>? dep_organizeEntityList = await _db.Queryable<OrganizeEntity>().Where(d => d.EnabledMark == 1 && d.DeleteMark == null)
            .WhereIF(orgType.Equals(QtKeyConst.DEPSELECT), d => d.Category.Equals("department")).ToListAsync();

        List<Dictionary<string, object>> organizeList = new List<Dictionary<string, object>>();
        foreach (OrganizeEntity? item in dep_organizeEntityList)
        {
            Dictionary<string, object> organize = new Dictionary<string, object>();
            if (orgType.Equals(QtKeyConst.DEPSELECT)) organize.Add(item.Id, item.FullName); // 部门
            if (orgType.Equals(QtKeyConst.COMSELECT)) organize.Add(item.Id, new string[] { item.OrganizeIdTree, item.FullName }); // 公司
            if (orgType.Equals(QtKeyConst.CURRORGANIZE)) organize.Add(item.Id, new string[] { item.OrganizeIdTree, item.Category, item.FullName }); // 所属组织
            organizeList.Add(organize);
        }

        return organizeList;
    }

    /// <summary>
    /// 处理远端数据.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    private async Task<List<Dictionary<string, string>>> GetDynamicList(FieldsModel model)
    {
        List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();

        // 远端数据 配置参数
        List<SugarParameter>? parameter = new List<SugarParameter>();
        if (_userManager.ToKen != null)
        {
            parameter.Add(new SugarParameter("@user", _userManager.UserId));
            parameter.Add(new SugarParameter("@organize", _userManager.User.OrganizeId));
            parameter.Add(new SugarParameter("@department", _userManager.User.OrganizeId));
            parameter.Add(new SugarParameter("@postion", _userManager.User.PositionId));
        }

        // 获取远端数据
        DataInterfaceEntity? dynamic = await _dataInterfaceService.GetInfo(model.__config__.propsUrl);
        if (dynamic != null && 1.Equals(dynamic.DataType))
        {
            DbLinkEntity? linkEntity = await _db.Queryable<DbLinkEntity>().Where(m => m.Id == dynamic.DBLinkId && m.DeleteMark == null).FirstAsync();
            if (linkEntity == null) linkEntity = _databaseService.GetTenantDbLink(_userManager.TenantId, _userManager.TenantDbName);
            _dataInterfaceService.ReplaceParameterValue(dynamic, new Dictionary<string, string>());
            System.Data.DataTable? dt = _databaseService.GetInterFaceData(linkEntity, dynamic.Query, parameter.ToArray());
            List<Dictionary<string, object>> dynamicDataList = dt.ToJsonString().ToObject<List<Dictionary<string, object>>>();
            foreach (Dictionary<string, object>? item in dynamicDataList)
            {
                Dictionary<string, string> dynamicDic = new Dictionary<string, string>();
                dynamicDic.Add(item[model.__config__.props.value].ToString(), item[model.__config__.props.label].ToString());
                list.Add(dynamicDic);
            }
        }

        return list;
    }

    /// <summary>
    /// 处理静态数据.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    private List<Dictionary<string, string>> GetStaticList(FieldsModel model)
    {
        PropsBeanModel? props = model.props.props;
        List<OptionsModel>? optionList = GetTreeOptions(model.options, props);
        List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
        foreach (OptionsModel? item in optionList)
        {
            Dictionary<string, string> option = new Dictionary<string, string>();
            option.Add(item.value, item.label);
            list.Add(option);
        }

        return list;
    }

    /// <summary>
    /// 获取数据字典数据 根据 类型Id.
    /// </summary>
    /// <param name="dictionaryTypeId"></param>
    /// <returns>List.</returns>
    private async Task<List<Dictionary<string, string>>> GetDictionaryList(string? dictionaryTypeId = null)
    {
        List<DictionaryDataEntity> dictionaryDataEntityList = await _db.Queryable<DictionaryDataEntity, DictionaryTypeEntity>((a, b) => new JoinQueryInfos(JoinType.Left, b.Id == a.DictionaryTypeId))
            .WhereIF(dictionaryTypeId.IsNotEmptyOrNull(), (a, b) => b.Id == dictionaryTypeId || b.EnCode == dictionaryTypeId).Where(a => a.DeleteMark == null).ToListAsync();

        List<Dictionary<string, string>> dictionaryDataList = new List<Dictionary<string, string>>();
        foreach (DictionaryDataEntity? item in dictionaryDataEntityList)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add(item.Id, item.FullName);
            dictionary.Add(item.EnCode, item.FullName);
            dictionaryDataList.Add(dictionary);
        }

        return dictionaryDataList;
    }

    /// <summary>
    /// options无限级.
    /// </summary>
    /// <returns></returns>
    private List<OptionsModel> GetTreeOptions(List<object> model, PropsBeanModel props)
    {
        List<OptionsModel> options = new List<OptionsModel>();
        foreach (object? item in model)
        {
            OptionsModel option = new OptionsModel();
            Dictionary<string, object>? dicObject = item.ToJsonString().ToObject<Dictionary<string, object>>();
            option.label = dicObject[props.label].ToString();
            option.value = dicObject[props.value].ToString();
            if (dicObject.ContainsKey(props.children))
            {
                List<object>? children = dicObject[props.children].ToJsonString().ToObject<List<object>>();
                options.AddRange(GetTreeOptions(children, props));
            }

            options.Add(option);
        }

        return options;
    }

    #endregion

    #region 系统组件生成与解析

    /// <summary>
    /// 将系统组件生成的数据转换为数据.
    /// </summary>
    /// <param name="moldelId">功能id.</param>
    /// <param name="formData">表单模板.</param>
    /// <param name="modelData">真实数据.</param>
    /// <returns></returns>
    public async Task<Dictionary<string, object>> GetSystemComponentsData(List<FieldsModel> formData, string modelData)
    {
        // 获取控件缓存数据
        Dictionary<string, object> templateData = await GetVisualDevCaCheData(formData);

        Dictionary<string, object> dataMap = modelData.ToObject<Dictionary<string, object>>(); // 数据库保存的F_Data

        // 序列化后时间戳转换处理
        List<FieldsModel>? timeList = formData.Where(x => x.__config__.qtKey == "createTime" || x.__config__.qtKey == "modifyTime").ToList();
        if (timeList.Any())
        {
            timeList.ForEach(item =>
            {
                if (dataMap.ContainsKey(item.__vModel__))
                {
                    string? value = dataMap[item.__vModel__].ToString();
                    if (value.IsNotEmptyOrNull())
                    {
                        dataMap.Remove(item.__vModel__);
                        DateTime dtDate;
                        if (!DateTime.TryParse(value, out dtDate)) value = string.Format("{0:yyyy-MM-dd HH:mm:ss}", value.TimeStampToDateTime());
                        else value = string.Format("{0:yyyy-MM-dd HH:mm:ss}", value.ParseToDateTime());
                        dataMap.Add(item.__vModel__, value);
                    }
                }
            });
        }

        int dicCount = dataMap.Keys.Count;
        string[] strKey = new string[dicCount];
        dataMap.Keys.CopyTo(strKey, 0);

        // 自动生成的数据不在模板数据内
        foreach (string? key in dataMap.Keys.ToList())
        {
            if (dataMap[key] != null)
            {
                string? dataValue = dataMap[key].ToString();
                if (!string.IsNullOrEmpty(dataValue))
                {
                    FieldsModel? model = formData.Where(f => f.__vModel__ == key).FirstOrDefault();
                    if (model != null)
                    {
                        ConfigModel configModel = model.__config__;
                        if (string.IsNullOrWhiteSpace(model.separator)) model.separator = ",";
                        switch (configModel.qtKey)
                        {
                            case QtKeyConst.CREATEUSER:
                            case QtKeyConst.MODIFYUSER:
                                dataMap[key] = (await _db.Queryable<UserEntity>().FirstAsync(x => x.Id == dataMap[key].ToString()))?.RealName;
                                break;
                            case QtKeyConst.CURRPOSITION:
                                dataMap[key] = (await _db.Queryable<PositionEntity>().FirstAsync(p => p.Id == dataMap[key].ToString()))?.FullName;
                                break;
                            case QtKeyConst.CURRORGANIZE:
                                {
                                    var currOrganizeTemplateValue = templateData.Where(t => t.Key.Equals(CommonConst.VISUALDEV + _userManager.TenantId + "_"+configModel.renderKey + "_" + key)).FirstOrDefault().Value?.ToJsonString().ToObject<List<Dictionary<string, string[]>>>();
                                    var valueId = currOrganizeTemplateValue.Where(x => x.Keys.Contains(dataMap[key].ToString())).FirstOrDefault();

                                    if (valueId != null)
                                    {
                                        if (model.showLevel == "all")
                                        {
                                            string[]? cascaderData = valueId[dataMap[key].ToString()];
                                            if (cascaderData != null && !string.IsNullOrWhiteSpace(cascaderData.FirstOrDefault()))
                                            {
                                                List<string>? treeFullName = new List<string>();
                                                cascaderData.FirstOrDefault()?.Split(',').ToList().ForEach(item =>
                                                {
                                                    treeFullName.Add(currOrganizeTemplateValue.Find(x => x.Keys.Contains(item)).FirstOrDefault().Value?.LastOrDefault());
                                                });
                                                dataMap[key] = string.Join("/", treeFullName);
                                            }
                                            else
                                            {
                                                dataMap[key] = string.Empty;
                                            }
                                        }
                                        else
                                        {
                                            string[]? cascaderData = valueId[dataMap[key].ToString()];
                                            if (cascaderData != null && cascaderData[1] == "department") dataMap[key] = valueId[dataMap[key].ToString()]?.LastOrDefault();
                                            else dataMap[key] = string.Empty;
                                        }
                                    }
                                    else
                                    {
                                        dataMap[key] = string.Empty;
                                    }
                                }

                                break;
                        }
                    }
                }
            }
        }

        return dataMap;
    }

    #endregion

    #region 无表的数据查询筛选

    /// <summary>
    /// 无表的数据筛选.
    /// </summary>
    /// <param name="list">数据列表.</param>
    /// <param name="keyJsonMap">查询条件值.</param>
    /// <param name="formData"></param>
    /// <returns></returns>
    public List<Dictionary<string, object>> GetNoTableFilteringData(List<VisualDevModelDataEntity> list, Dictionary<string, object> keyJsonMap, List<FieldsModel> formData)
    {
        List<Dictionary<string, object>> realList = new List<Dictionary<string, object>>();
        foreach (var entity in list)
        {
            Dictionary<string, object> query = keyJsonMap;
            Dictionary<string, object> realEntity = entity.Data.ToObject<Dictionary<string, object>>();
            if (query != null && query.Count != 0)
            {
                int m = 0;
                int dicCount = query.Keys.Count;
                string[] strKey = new string[dicCount];
                query.Keys.CopyTo(strKey, 0);
                for (int i = 0; i < strKey.Length; i++)
                {
                    var keyValue = keyJsonMap[strKey[i]];
                    var queryEntity = realEntity.Where(e => e.Key == strKey[i]).FirstOrDefault();
                    var model = formData.Where(f => f.__vModel__ == strKey[i]).FirstOrDefault();
                    if (queryEntity.Value != null && !string.IsNullOrWhiteSpace(keyValue.ToString()))
                    {
                        var realValue = queryEntity.Value.ObjToString();
                        var type = model.__config__.qtKey;
                        switch (type)
                        {
                            case QtKeyConst.TIME:
                                {
                                    var queryTime = new List<string>();
                                    keyValue.ToObject<List<string>>().ForEach(item => { if (!string.IsNullOrWhiteSpace(item)) queryTime.Add(item.ParseToDateTime().ToLongTimeString()); });

                                    if (Common.Extension.Extensions.IsInTimeRange(realValue.ParseToDateTime(), queryTime.First(), queryTime.Last(), 3)) m++;
                                }
                                break;
                            case QtKeyConst.DATE:
                                {
                                    List<string> queryTime = keyValue.ToObject<List<string>>();
                                    int formatType = 0;
                                    if (model.format == "yyyy-MM") formatType = 1;
                                    else if (model.format == "yyyy") formatType = 2;
                                    string value1 = string.Format("{0:yyyy-MM-dd}", queryTime.First().ParseToDateTime());
                                    string value2 = string.Format("{0:yyyy-MM-dd}", queryTime.Last().ParseToDateTime());
                                    if (Common.Extension.Extensions.IsInTimeRange(realValue.ParseToDateTime(), value1, value2, formatType)) m++;
                                }
                                break;
                            case QtKeyConst.CREATETIME:
                            case QtKeyConst.MODIFYTIME:
                                {
                                    List<string> dayTime1 = keyValue.ToObject<List<string>>();
                                    string value1 = string.Format("{0:yyyy-MM-dd 00:00:00}", dayTime1.First().ParseToDateTime());
                                    string value2 = string.Format("{0:yyyy-MM-dd 23:59:59}", dayTime1.Last().ParseToDateTime());
                                    if (!string.IsNullOrEmpty(realValue) && Common.Extension.Extensions.IsInTimeRange(Convert.ToDateTime(realValue), value1, value2)) m++;
                                }
                                break;
                            case QtKeyConst.NUMINPUT:
                            case QtKeyConst.CALCULATE:
                                {
                                    List<string> numArray = keyValue.ToObject<List<string>>();
                                    var numA = numArray.First().ParseToInt();
                                    var numB = numArray.Last() == null ? long.MaxValue : numArray.Last().ParseToInt();
                                    var numC = realValue.ParseToInt();
                                    if (numC >= numA && numC <= numB) m++;
                                }
                                break;
                            default:
                                if (realValue.IsNotEmptyOrNull() && keyValue != null)
                                {
                                    string keyV = keyValue.ToString();

                                    if (model.searchType == 2 && realValue.Contains(keyV)) m++;
                                    else if (model.searchType == 1)
                                    {
                                        // 多选时为模糊查询
                                        if ((model.multiple || type == "checkbox") && realValue.Contains(keyV)) m++;
                                        else if (realValue.Equals(keyV)) m++;
                                    }
                                    else if (realValue.Replace(" ", "").Contains(keyV.Replace(" ", ""))) m++;
                                }

                                break;
                        }
                    }

                    if (m == dicCount) realList.Add(realEntity);
                }
            }
            else
            {
                realList.Add(realEntity);
            }
        }

        return realList;
    }

    #endregion

    #region 列表转换数据(Id 转 Name)

    /// <summary>
    /// 将关键字key查询传输的id转换成名称，还有动态数据id成名称.
    /// </summary>
    /// <param name="moldelId">功能id.</param>
    /// <param name="formData">数据库模板数据.</param>
    /// <param name="list">真实数据.</param>
    /// <param name="columnDesign"></param>
    /// <param name="actionType"></param>
    /// <param name="webType">表单类型1-纯表单、2-普通表单、3-工作流表单.</param>
    /// <param name="primaryKey">数据主键.</param>
    /// <returns></returns>
    public async Task<List<Dictionary<string, object>>> GetKeyData(List<FieldsModel> formData, List<Dictionary<string, object>> list, ColumnDesignModel? columnDesign = null, string actionType = "List", int webType = 2, string primaryKey = "F_Id")
    {
        // 获取控件缓存数据
        Dictionary<string, object> templateData = await GetVisualDevCaCheData(formData);

        // 转换数据
        Dictionary<string, object>? convData = new Dictionary<string, object>();

        // 存放 预缓存数据的控件的缓存数据 ， 避免循环时重复序列化 耗资源
        List<Dictionary<string, string[]>>? currOrganizeTemplateValue = new List<Dictionary<string, string[]>>(); // 组织
        List<Dictionary<string, string>>? addressTemplateValue = new List<Dictionary<string, string>>(); // 地址 省市区 缓存
        List<Dictionary<string, string>>? streetTemplateValue = new List<Dictionary<string, string>>(); // 地址 街道 缓存
        List<Dictionary<string, string[]>>? comselectTemplateValue = new List<Dictionary<string, string[]>>(); // 公司
        List<Dictionary<string, string>>? depselectTemplateValue = new List<Dictionary<string, string>>(); // 部门
        List<Dictionary<string, string>>? userselectTemplateValue = new List<Dictionary<string, string>>(); // 用户
        List<Dictionary<string, string>>? posselectTemplateValue = new List<Dictionary<string, string>>(); // 岗位
        List<Dictionary<string, string>>? checkboxTemplateValue = new List<Dictionary<string, string>>(); // 复选框
        List<Dictionary<string, string>>? selectTemplateValue = new List<Dictionary<string, string>>(); // 下拉框
        List<Dictionary<string, string>>? treeSelectTemplateValue = new List<Dictionary<string, string>>(); // 树
        List<Dictionary<string, string>>? cascaderTemplateValue = new List<Dictionary<string, string>>(); // 级联选择
        Dictionary<string, List<Dictionary<string, string>>>? templateValues = new Dictionary<string, List<Dictionary<string, string>>>(); // 其他

        // 转换列表数据
        foreach (Dictionary<string, object>? dataMap in list)
        {
            if (dataMap.ContainsKey(primaryKey)) dataMap["id"] = dataMap[primaryKey].ToString(); // 主键

            if (webType == 3)
            {
                FlowTaskEntity? flowTask = await _db.Queryable<FlowTaskEntity>().FirstAsync(x => x.DeleteMark == null && x.Id == dataMap["id"].ToString());
                if (flowTask != null) dataMap["flowState"] = flowTask.Status;
                else dataMap["flowState"] = 0;
            }

            int dicCount = dataMap.Keys.Count;
            string[] strKey = new string[dicCount];
            dataMap.Keys.CopyTo(strKey, 0);

            // 处理有缓存
            for (int i = 0; i < strKey.Length; i++)
            {
                if (!(dataMap[strKey[i]] is null))
                {
                    FieldsModel? form = formData.Where(f => f.__vModel__ == strKey[i]).FirstOrDefault();
                    if (form != null)
                    {
                        if (form.__vModel__.Contains(form.__config__.qtKey + "Field")) dataMap[strKey[i]] = TemplateControlsDataConversion(dataMap[strKey[i]], form);
                        else dataMap[strKey[i]] = TemplateControlsDataConversion(dataMap[strKey[i]], form, actionType);

                        string? qtKey = form.__config__.qtKey;
                        KeyValuePair<string, object> templateValue = templateData.Where(t => t.Key.Equals(CommonConst.VISUALDEV + _userManager.TenantId + "_" + form.__config__.renderKey + "_" + strKey[i])).FirstOrDefault();

                        #region 处理 单独存储缓存
                        if (qtKey == QtKeyConst.USERSELECT)
                        {
                            templateValue = templateData.Where(t => t.Key.Equals(CommonConst.VISUALDEV + "_userSelect")).FirstOrDefault();
                            if (!templateData.ContainsKey(form.__config__.renderKey + "_" + strKey[i])) templateData.Add(form.__config__.renderKey + "_" + strKey[i], string.Empty);
                        }

                        if (qtKey == QtKeyConst.ADDRESS)
                        {
                            if (form.level == 3) templateValue = templateData.Where(t => t.Key.Equals(CommonConst.VISUALDEV + "_address2")).FirstOrDefault();
                            else templateValue = templateData.Where(t => t.Key.Equals(CommonConst.VISUALDEV + "_address1")).FirstOrDefault();
                            if (!templateData.ContainsKey(form.__config__.renderKey + "_" + strKey[i])) templateData.Add(form.__config__.renderKey + "_" + strKey[i], string.Empty);
                        }
                        #endregion

                        // 转换后的数据值
                        object? dataDicValue = dataMap[strKey[i]];
                        if (templateValue.Key != null && !(dataDicValue is null) && dataDicValue.ToString() != "[]")
                        {
                            IEnumerable<object>? moreValue = dataDicValue as IEnumerable<object>;
                            if (dataDicValue.IsNullOrEmpty()) qtKey = string.Empty; // 空数据直接赋值

                            // 不是List数据直接赋值 用逗号切割成 List
                            if (moreValue == null && !dataDicValue.ToString().Contains("[")) moreValue = dataDicValue.ToString().Split(",");
                            if (moreValue == null && dataDicValue.ToString().Contains("[")) moreValue = dataDicValue.ToString().ToObject<List<string>>();

                            if (string.IsNullOrWhiteSpace(form.separator)) form.separator = ",";

                            switch (qtKey)
                            {
                                case QtKeyConst.COMSELECT:
                                    {
                                        var currOrganizeValues = new List<List<object>>();
                                        if (form.multiple) currOrganizeValues = moreValue != null ? moreValue.ToJsonString().ToObject<List<List<object>>>() : dataDicValue.ToString().ToObject<List<List<object>>>();
                                        else currOrganizeValues.Add(moreValue != null ? moreValue.ToJsonString().ToObject<List<object>>() : dataDicValue.ToString().ToObject<List<object>>());

                                        var addNames = new List<string>();

                                        if (comselectTemplateValue.Count < 1) comselectTemplateValue = templateValue.Value.ToJsonString().ToObject<List<Dictionary<string, string[]>>>();

                                        foreach (var item in currOrganizeValues)
                                        {
                                            var addName = new List<string>();
                                            foreach (var it in item)
                                            {
                                                var currOrganizeData = comselectTemplateValue.Where(a => a.ContainsKey(it.ToString())).FirstOrDefault();
                                                if (currOrganizeData != null) addName.Add(currOrganizeData[it.ToString()].LastOrDefault());
                                            }

                                            addNames.Add(string.Join("/", addName));
                                        }
                                        if (addNames.Any()) dataMap[strKey[i]] = string.Join(form.separator, addNames);
                                    }
                                    break;
                                case QtKeyConst.ADDRESS:
                                    {
                                        var addressValues = new List<List<object>>();
                                        if (form.multiple) addressValues = dataDicValue.ToJsonString().ToObject<List<List<object>>>();
                                        else addressValues.Add(dataDicValue.ToJsonString().ToObject<List<object>>());

                                        var addNames = new List<string>();

                                        if (addressTemplateValue.Count < 1) addressTemplateValue = templateValue.Value.ToObject<List<Dictionary<string, string>>>();
                                        if (form.level == 3 && streetTemplateValue.Count < 1) streetTemplateValue = templateValue.Value.ToObject<List<Dictionary<string, string>>>();

                                        foreach (var item in addressValues)
                                        {
                                            if (item.Any())
                                            {
                                                var value = addressTemplateValue.Where(a => a.ContainsKey(item.LastOrDefault().ToString())).FirstOrDefault();
                                                if (form.level == 3) value = streetTemplateValue.Where(a => a.ContainsKey(item.LastOrDefault().ToString())).FirstOrDefault();
                                                addNames.Add(value[item.LastOrDefault().ToString()]);
                                            }
                                        }
                                        if (addNames.Count != 0) dataMap[strKey[i]] = string.Join(form.separator, addNames);
                                    }
                                    break;
                                case QtKeyConst.CURRORGANIZE:
                                    {
                                        if (currOrganizeTemplateValue.Count < 1) currOrganizeTemplateValue = templateValue.Value.ToJsonString().ToObject<List<Dictionary<string, string[]>>>();

                                        var valueId = currOrganizeTemplateValue.Where(x => x.Keys.Contains(dataDicValue.ToString())).FirstOrDefault();

                                        if (valueId != null)
                                        {
                                            if (form.showLevel == "all")
                                            {
                                                var cascaderData = valueId[dataDicValue.ToString()];
                                                if (cascaderData != null && !string.IsNullOrWhiteSpace(cascaderData.FirstOrDefault()))
                                                {
                                                    var treeFullName = new List<string>();
                                                    cascaderData.FirstOrDefault()?.Split(',').ToList().ForEach(item =>
                                                    {
                                                        treeFullName.Add(currOrganizeTemplateValue.Find(x => x.Keys.Contains(item)).FirstOrDefault().Value?.LastOrDefault());
                                                    });
                                                    dataMap[strKey[i]] = string.Join("/", treeFullName);
                                                }
                                                else
                                                {
                                                    dataMap[strKey[i]] = string.Empty;
                                                }
                                            }
                                            else
                                            {
                                                var cascaderData = valueId[dataDicValue.ToString()];
                                                if (cascaderData != null && cascaderData[1] == "department") dataMap[strKey[i]] = valueId[dataDicValue.ToString()]?.LastOrDefault();
                                                else dataMap[strKey[i]] = string.Empty;
                                            }
                                        }
                                        else
                                        {
                                            dataMap[strKey[i]] = string.Empty;
                                        }
                                    }
                                    break;
                                case QtKeyConst.DEPSELECT:
                                    dataMap[strKey[i]] = GetTemplateDataValueByKey(depselectTemplateValue, templateValue, moreValue, form);
                                    break;
                                case QtKeyConst.USERSELECT:
                                    dataMap[strKey[i]] = GetTemplateDataValueByKey(userselectTemplateValue, templateValue, moreValue, form);
                                    break;
                                case QtKeyConst.POSSELECT:
                                    dataMap[strKey[i]] = GetTemplateDataValueByKey(posselectTemplateValue, templateValue, moreValue, form);
                                    break;
                                case QtKeyConst.CHECKBOX:
                                    dataMap[strKey[i]] = GetTemplateDataValueByKey(checkboxTemplateValue, templateValue, moreValue, form);
                                    break;
                                case QtKeyConst.SELECT:
                                    dataMap[strKey[i]] = GetTemplateDataValueByKey(selectTemplateValue, templateValue, moreValue, form);
                                    break;
                                case QtKeyConst.TREESELECT:
                                    dataMap[strKey[i]] = GetTemplateDataValueByKey(treeSelectTemplateValue, templateValue, moreValue, form);
                                    break;
                                case QtKeyConst.CASCADER:
                                    dataMap[strKey[i]] = GetTemplateDataValueByKey(cascaderTemplateValue, templateValue, moreValue, form);
                                    break;
                                default:
                                    {
                                        if (qtKey.IsNotEmptyOrNull())
                                        {
                                            if (!templateValues.ContainsKey(strKey[i])) templateValues.Add(strKey[i], templateValue.Value.ToJsonString().ToObject<List<Dictionary<string, string>>>());

                                            var convertData = templateValues[strKey[i]].Where(t => t.ContainsKey(dataMap[strKey[i]].ToString())).FirstOrDefault();
                                            if (convertData != null) dataMap[strKey[i]] = convertData.Values.FirstOrDefault().ToString();
                                        }
                                        else
                                        {
                                            dataMap[strKey[i]] = string.Empty;
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                }
            }

            // 剪掉有缓存的，获取无缓存的 继续解析
            List<string>? record = dataMap.Keys.Except(templateData.Keys.Select(x => x = x.Substring(14, x.Length - 14))).ToList();

            // 针对有表的模板去除"rownum,主键"
            record.RemoveAll(r => r == "ROWNUM");
            record.RemoveAll(r => r == primaryKey);

            // 处理无缓存
            foreach (string? key in record)
            {
                if (!(dataMap[key] is null) && dataMap[key].ToString() != string.Empty)
                {
                    var dataValue = dataMap[key];
                    var model = formData.Where(f => f.__vModel__ == key).FirstOrDefault();
                    if (model != null)
                    {
                        ConfigModel configModel = model.__config__;
                        string type = configModel.qtKey;
                        if (string.IsNullOrWhiteSpace(model.separator)) model.separator = ",";
                        switch (type)
                        {
                            case QtKeyConst.SWITCH: // switch开关
                                dataMap[key] = dataMap[key].ParseToInt() == 0 ? model.inactiveTxt : model.activeTxt;
                                break;
                            case QtKeyConst.DATERANGE: // 日期范围
                            case QtKeyConst.TIMERANGE: // 时间范围
                                dataMap[key] = QueryDateTimeToString(dataValue, model.format, model.format);
                                break;
                            case QtKeyConst.DATE: // 日期选择
                                {
                                    string value = string.Empty;
                                    var keyValue = dataMap[key].ToString();
                                    DateTime dtDate;
                                    if (DateTime.TryParse(keyValue, out dtDate)) value = string.Format("{0:yyyy-MM-dd HH:mm:ss} ", keyValue);
                                    else value = string.Format("{0:yyyy-MM-dd HH:mm:ss} ", keyValue.ParseToDateTime());

                                    if (!string.IsNullOrEmpty(model.format))
                                    {
                                        value = string.Format("{0:" + model.format + "}", Convert.ToDateTime(value));
                                    }
                                    else
                                    {
                                        switch (model.type)
                                        {
                                            case "date":
                                                value = string.Format("{0:yyyy-MM-dd}", Convert.ToDateTime(value));
                                                break;
                                            default:
                                                value = string.Format("{0:yyyy-MM-dd HH:mm:ss}", Convert.ToDateTime(value));
                                                break;
                                        }
                                    }
                                    dataMap[key] = value;
                                }
                                break;
                            case QtKeyConst.TIME: // 日期选择
                                {
                                    string value = string.Empty;
                                    var keyValue = dataMap[key].ToString();
                                    DateTime dtDate;
                                    if (DateTime.TryParse(keyValue, out dtDate)) value = string.Format("{0:yyyy-MM-dd HH:mm:ss} ", keyValue);
                                    else value = string.Format("{0:yyyy-MM-dd HH:mm:ss} ", keyValue.ParseToDateTime());
                                    if (!string.IsNullOrEmpty(model.format)) value = string.Format("{0:" + model.format + "}", Convert.ToDateTime(value));
                                    else value = dataMap[key].ToString();
                                    dataMap[key] = value;
                                }
                                break;
                            case QtKeyConst.CURRDEPT:
                                dataMap[key] = (await _db.Queryable<OrganizeEntity>().FirstAsync(x => x.Id == dataValue.ToString()))?.FullName;
                                break;
                            case QtKeyConst.MODIFYUSER:
                            case QtKeyConst.CREATEUSER:
                                dataMap[key] = (await _db.Queryable<UserEntity>().FirstAsync(x => x.Id == dataValue.ToString()))?.RealName;
                                break;
                            case QtKeyConst.MODIFYTIME:
                            case QtKeyConst.CREATETIME:
                                dataMap[key] = string.Format("{0:yyyy-MM-dd HH:mm:ss}", dataMap[key].ToString().ParseToDateTime());
                                break;
                            case QtKeyConst.CURRPOSITION:
                                dataMap[key] = (await _db.Queryable<PositionEntity>().FirstAsync(x => x.Id == dataValue.ToString()))?.FullName;
                                break;
                            case QtKeyConst.POPUPSELECT:
                                {
                                    // 获取远端数据
                                    var dynamic = await _dataInterfaceService.GetInfo(model.interfaceId);
                                    if (dynamic == null)
                                        break;
                                    switch (dynamic.DataType)
                                    {
                                        case 1: // SQL数据
                                            {
                                                _dataInterfaceService.ReplaceParameterValue(dynamic, new Dictionary<string, string>());
                                                var pObj = await _dataInterfaceService.GetData(dynamic);
                                                var popupsList = pObj.ToJsonString().ToObject<List<Dictionary<string, string>>>();

                                                var specificData = popupsList.Where(it => it.ContainsKey(model.propsValue) && it.ContainsValue(dataMap[key].ToString())).FirstOrDefault();
                                                if (specificData != null)
                                                {
                                                    // 要用模板的 “显示字段 - relationField”来展示数据
                                                    if (model.relationField.IsNullOrEmpty())
                                                    {
                                                        var showField = model.columnOptions.First();
                                                        dataMap[key] = specificData[showField.value];
                                                    }
                                                    else
                                                    {
                                                        dataMap[key + "_id"] = dataValue;
                                                        dataMap[key] = specificData[model.relationField];
                                                    }
                                                }
                                            }
                                            break;
                                        case 2: // 静态数据
                                            {
                                                List<Dictionary<string, string>> dynamicList = new List<Dictionary<string, string>>();
                                                var children = model.props.props.children;
                                                foreach (var data in JValue.Parse(dynamic.Query))
                                                {
                                                    Dictionary<string, string> dic = new Dictionary<string, string>();
                                                    dic[model.props.props.value] = data.Value<string>(model.props.props.value);
                                                    dic[model.props.props.label] = data.Value<string>(model.props.props.label);
                                                    dynamicList.Add(dic);
                                                    if (data.Value<object>(children) != null && data.Value<object>(children).ToString() != "")
                                                        dynamicList.AddRange(GetDynamicInfiniteData(data.Value<object>(children).ToString(), model.props.props));
                                                }
                                                List<string> dataList = dataMap[key].ToJsonString().ToObject<List<string>>();
                                                List<string> cascaderList = new List<string>();
                                                foreach (var items in dataList)
                                                {
                                                    var vara = dynamicList.Where(a => a.ContainsValue(items)).FirstOrDefault();
                                                    if (vara != null) cascaderList.Add(vara[model.props.props.label]);
                                                }

                                                dataMap[key + "_id"] = dataValue;
                                                if (actionType == "List") dataMap[key] = string.Join(model.separator, cascaderList);
                                                else dataMap[key] = cascaderList;
                                            }
                                            break;
                                        case 3: // Api数据
                                            {
                                                var response = await new HttpRequestPart().SetRequestUrl(dynamic.Path).GetAsStringAsync(); // 请求接口
                                                RESTfulResult<object>? result = response.ToObject<RESTfulResult<object>>();
                                                var data = result.data?.ToString().ToObject<Dictionary<string, List<dynamic>>>().FirstOrDefault().Value;

                                                List<object> cascaderList = new List<object>();

                                                if (data != null) data.ForEach(obj => { if (obj[model.propsValue] == dataValue.ToString()) cascaderList.Add(obj[model.relationField]); });

                                                dataMap[key + "_id"] = dataValue;
                                                dataMap[key] = string.Join(model.separator, cascaderList);
                                            }
                                            break;
                                    }
                                }
                                break;
                            case QtKeyConst.RELATIONFORM: // 关联表单
                                {
                                    List<Dictionary<string, object>> relationFormDataList = new List<Dictionary<string, object>>();

                                    var redisName = CommonConst.VISUALDEV + _userManager.TenantId + "_" + model.__config__.qtKey + "_" + model.__config__.renderKey;
                                    if (_cacheManager.Exists(redisName))
                                    {
                                        relationFormDataList = _cacheManager.Get(redisName).ToObject<List<Dictionary<string, object>>>();
                                    }
                                    else
                                    {
                                        // 根据可视化功能ID获取该模板全部数据
                                        var relationFormModel = await _db.Queryable<VisualDevEntity>().FirstAsync(v => v.Id == model.modelId);
                                        var newFieLdsModelList = relationFormModel.FormData.ToObject<FormDataModel>().fields.FindAll(x => model.relationField.Equals(x.__vModel__));
                                        VisualDevModelListQueryInput listQueryInput = new VisualDevModelListQueryInput
                                        {
                                            dataType = "1",
                                            sidx = columnDesign.defaultSidx,
                                            sort = columnDesign.sort
                                        };

                                        await Scoped.Create(async (_, scope) =>
                                        {
                                            var services = scope.ServiceProvider;
                                            var _runService = App.GetService<IRunService>(services);
                                            var res = await _runService.GetListResult(relationFormModel, listQueryInput);
                                            _cacheManager.Set(redisName, res.list.ToList(), TimeSpan.FromMinutes(15)); // 缓存15分钟
                                        });
                                        relationFormDataList = _cacheManager.Get(redisName).ToObject<List<Dictionary<string, object>>>();
                                    }

                                    var relationFormRealData = relationFormDataList.Where(it => it["id"].Equals(dataMap[key])).FirstOrDefault();
                                    if (relationFormRealData != null && relationFormRealData.Count > 0)
                                    {
                                        dataMap[key + "_id"] = relationFormRealData["id"];
                                        dataMap[key] = relationFormRealData[model.relationField];
                                    }
                                    else
                                    {
                                        dataMap[key] = string.Empty;
                                    }
                                }

                                break;
                        }
                    }
                }
            }

        }

        return list;
    }

    /// <summary>
    /// 从缓存读取数据,根据Key.
    /// </summary>
    /// <param name="keyTData">控件对应的缓存</param>
    /// <param name="tValue">所有的缓存</param>
    /// <param name="mValue">要转换的key</param>
    /// <param name="form">组件</param>
    /// <returns></returns>
    private string GetTemplateDataValueByKey(List<Dictionary<string, string>> keyTData, KeyValuePair<string, object> tValue, IEnumerable<object> mValue, FieldsModel? form)
    {
        List<string>? data = new List<string>();
        if (keyTData.Count < 1) keyTData = tValue.Value.ToObject<List<Dictionary<string, string>>>();

        if (form != null && form.props != null && form.props.props != null && form.props.props.multiple)
        {
            foreach (object? item in mValue)
            {
                List<string>? sb = new List<string>();
                item.ToJsonString().ToObject<List<string>>().ForEach(items =>
                {
                    var cascaderData = keyTData.Where(c => c.ContainsKey(items)).FirstOrDefault();
                    if (cascaderData != null) sb.Add(cascaderData[items]);
                });
                if (sb.Count != 0) data.Add(string.Join("/", sb));
            }
        }
        else
        {
            foreach (object? item in mValue)
            {
                Dictionary<string, string>? comData = keyTData.Where(a => a.ContainsKey(item.ToString())).FirstOrDefault();
                if (comData != null) data.Add(comData[item.ToString()]);
            }
        }

        return string.Join(form.separator, data);
    }

    /// <summary>
    /// 查询时间转成设定字符串.
    /// </summary>
    /// <returns></returns>
    private List<string> QueryDateTimeToString(object value, string format1, string format2)
    {
        List<string>? jsonArray = value.ToJsonString().ToObject<List<string>>();
        string value1 = string.Format("{0:" + format1 + "}", jsonArray.FirstOrDefault().ParseToDateTime());
        string value2 = string.Format("{0:" + format2 + "}", jsonArray.LastOrDefault().ParseToDateTime());
        jsonArray.Clear();
        jsonArray.Add(value1 + "至");
        jsonArray.Add(value2);
        return jsonArray;
    }

    #endregion

    #region 公用方法

    /// <summary>
    /// 解析 处理 条形码和二维码.
    /// </summary>
    /// <param name="fieldsModels"></param>
    /// <param name="_newDataMap"></param>
    /// <param name="_dataMap"></param>
    public void GetBARAndQR(List<FieldsModel> fieldsModels, Dictionary<string, object> _newDataMap, Dictionary<string, object> _dataMap)
    {
        fieldsModels.Where(x => x.__config__.qtKey == "barcode" || x.__config__.qtKey == "qrcode").Where(x => !string.IsNullOrWhiteSpace(x.relationField)).ToList().ForEach(item =>
        {
            if (!_newDataMap.ContainsKey(item.relationField + "_id") && _dataMap.ContainsKey(item.relationField))
                _newDataMap.Add(item.relationField + "_id", _dataMap[item.relationField]);
        });
    }

    /// <summary>
    /// 获取弹窗选择 数据列表.
    /// </summary>
    /// <param name="interfaceId"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<List<Dictionary<string, string>>?> GetPopupSelectDataList(string interfaceId, FieldsModel model)
    {
        List<Dictionary<string, string>>? result = new List<Dictionary<string, string>>();

        // 获取远端数据
        DataInterfaceEntity? dynamic = await _dataInterfaceService.GetInfo(interfaceId);
        if (dynamic == null) return null;
        switch (dynamic.DataType)
        {
            case 1: // SQL数据
                {
                    _dataInterfaceService.ReplaceParameterValue(dynamic, new Dictionary<string, string>());
                    System.Data.DataTable? pObj = await _dataInterfaceService.GetData(dynamic);
                    result = pObj.ToJsonString().ToObject<List<Dictionary<string, string>>>();
                }

                break;
            case 2: // 静态数据
                {
                    List<Dictionary<string, string>> dynamicList = new List<Dictionary<string, string>>();
                    string? value = model.props.props.value;
                    string? label = model.props.props.label;
                    string? children = model.props.props.children;
                    foreach (JToken? data in JToken.Parse(dynamic.Query))
                    {
                        Dictionary<string, string> dic = new Dictionary<string, string>();
                        dic[value] = data.Value<string>(value);
                        dic[label] = data.Value<string>(label);
                        dynamicList.Add(dic);
                        if (data.Value<object>(children) != null && data.Value<object>(children).ToString() != string.Empty)
                        {
                            dynamicList.AddRange(GetDynamicInfiniteData(data.Value<object>(children).ToString(), model.props.props));
                        }
                    }

                    result = dynamicList;
                }

                break;
            case 3: // Api数据
                {
                    string? response = await new HttpRequestPart().SetRequestUrl(dynamic.Path).GetAsStringAsync(); // 请求接口
                    RESTfulResult<object>? res = response.ToObject<RESTfulResult<object>>();
                    result = res.data?.ToString().ToObject<Dictionary<string, List<Dictionary<string, string>>>>().FirstOrDefault().Value;
                }

                break;
            default:
                break;
        }

        return result;
    }

    #endregion

    #region 私有方法

    /// <summary>
    /// 获取动态无限级数据.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="props"></param>
    /// <returns></returns>
    private List<Dictionary<string, string>> GetDynamicInfiniteData(string data, PropsBeanModel props)
    {
        List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
        string? value = props.value;
        string? label = props.label;
        string? children = props.children;
        foreach (JToken? info in JToken.Parse(data))
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic[value] = info.Value<string>(value);
            dic[label] = info.Value<string>(label);
            list.Add(dic);
            if (info.Value<object>(children) != null && info.Value<object>(children).ToString() != "")
            {
                list.AddRange(GetDynamicInfiniteData(info.Value<object>(children).ToString(), props));
            }
        }

        return list;
    }

    #endregion

}
