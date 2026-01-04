using QT.Common.Const;
using QT.Common.Extension;
using QT.Common.Security;
using QT.VisualDev.Engine.Model;
using QT.VisualDev.Engine.Security;
using QT.VisualDev.Entitys;

namespace QT.VisualDev.Engine.Core;

/// <summary>
/// 模板解析 基础类.
/// </summary>
public class TemplateParsingBase
{
    /// <summary>
    /// 是否有表 (true 有表, false 无表).
    /// </summary>
    public bool IsHasTable { get; set; }

    /// <summary>
    /// 表单配置JSON模型.
    /// </summary>
    public FormDataModel? FormModel { get; set; }

    /// <summary>
    /// 列配置JSON模型.
    /// </summary>
    public ColumnDesignModel ColumnData { get; set; }

    /// <summary>
    /// App列配置JSON模型.
    /// </summary>
    public ColumnDesignModel AppColumnData { get; set; }

    /// <summary>
    /// 所有控件集合.
    /// </summary>
    public List<FieldsModel> AllFieldsModel { get; set; }

    /// <summary>
    /// 所有控件集合(已剔除布局控件).
    /// </summary>
    public List<FieldsModel> FieldsModelList { get; set; }

    /// <summary>
    /// 主表控件集合.
    /// </summary>
    public List<FieldsModel> MainTableFieldsModelList { get; set; }

    /// <summary>
    /// 副表控件集合.
    /// </summary>
    public List<FieldsModel> AuxiliaryTableFieldsModelList { get; set; }

    /// <summary>
    /// 子表控件集合.
    /// </summary>
    public List<FieldsModel> ChildTableFieldsModelList { get; set; }

    /// <summary>
    /// 主/副表控件集合(列表展示数据控件).
    /// </summary>
    public List<FieldsModel> SingleFormData { get; set; }

    /// <summary>
    /// 所有表.
    /// </summary>
    public List<TableModel> AllTable { get; set; }

    /// <summary>
    /// 主表.
    /// </summary>
    public TableModel? MainTable { get; set; }

    /// <summary>
    /// 主表 表名.
    /// </summary>
    public string? MainTableName { get; set; }

    /// <summary>
    /// 模板解析帮助 构造.
    /// </summary>
    /// <param name="formJson">表单Json.</param>
    /// <param name="tables">涉及表Json.</param>
    /// <param name="isFlowTask"></param>
    public TemplateParsingBase(string formJson, string tables, bool isFlowTask = false)
    {
        FormDataModel formModel = !isFlowTask ? TemplateKeywordsHelper.ReplaceKeywords(formJson).ToObject<FormDataModel>() : formJson.ToObject<FormDataModel>();
        FormModel = formModel; // 表单Json模型
        IsHasTable = !string.IsNullOrEmpty(tables) && !"[]".Equals(tables) && tables.IsNullOrEmpty(); // 是否有表
        AllFieldsModel = formModel.fields; // 所有控件集合
        FieldsModelList = GetInDataFieldsModel(formModel.fields); // 已剔除布局控件集合
        MainTable = tables.ToList<TableModel>().Find(m => m.typeId.Equals("1")); // 主表
        MainTableName = MainTable?.table; // 主表名称

        // 处理旧控件 部分没有 tableName
        FieldsModelList.Where(x => string.IsNullOrWhiteSpace(x.__config__.tableName)).ToList().ForEach(item =>
        {
            if (item.__vModel__.Contains("_qt_")) item.__config__.tableName = item.__vModel__.ReplaceRegex(@"_qt_(\w+)", string.Empty).Replace("qt_", string.Empty); // 副表
            else item.__config__.tableName = MainTableName != null ? MainTableName : string.Empty; // 主表
        });
        AllTable = tables.ToObject<List<TableModel>>(); // 所有表
        AuxiliaryTableFieldsModelList = FieldsModelList.Where(x => x.__vModel__.Contains("_qt_")).ToList(); // 单控件副表集合
        ChildTableFieldsModelList = FieldsModelList.Where(x => x.__config__.qtKey == "table").ToList(); // 子表集合
        MainTableFieldsModelList = FieldsModelList.Except(AuxiliaryTableFieldsModelList).Except(ChildTableFieldsModelList).ToList(); // 主表控件集合
        SingleFormData = FieldsModelList.Where(x => x.__config__.qtKey != "table").ToList(); // 非子表集合
        ColumnData = new ColumnDesignModel();
        AppColumnData = new ColumnDesignModel();
    }

    /// <summary>
    /// 模板解析帮助 构造.
    /// </summary>
    /// <param name="entity">功能实体</param>
    /// <param name="isFlowTask"></param>
    public TemplateParsingBase(VisualDevEntity entity, bool isFlowTask = false)
    {
        // 报表不需要处理表单
        if (entity.Type != 6)
        {
            FormDataModel formModel = !isFlowTask ? TemplateKeywordsHelper.ReplaceKeywords(entity.FormData).ToObject<FormDataModel>() : entity.FormData.ToObject<FormDataModel>();
            FormModel = formModel; // 表单Json模型
            IsHasTable = !string.IsNullOrEmpty(entity.Tables) && !"[]".Equals(entity.Tables); // 是否有表
            AllFieldsModel = formModel.fields; // 所有控件集合
            FieldsModelList = GetInDataFieldsModel(formModel.fields); // 已剔除布局控件集合
            MainTable = entity.Tables.ToList<TableModel>().Find(m => m.typeId.Equals("1")); // 主表
            MainTableName = MainTable?.table; // 主表名称

            // 处理旧控件 部分没有 tableName
            FieldsModelList.Where(x => string.IsNullOrWhiteSpace(x.__config__.tableName)).ToList().ForEach(item =>
            {
                if (item.__vModel__.Contains("_qt_")) item.__config__.tableName = item.__vModel__.ReplaceRegex(@"_qt_(\w+)", string.Empty).Replace("qt_", string.Empty); // 副表
                else item.__config__.tableName = MainTableName != null ? MainTableName : string.Empty; // 主表
            });
            AllTable = entity.Tables.ToObject<List<TableModel>>(); // 所有表
            AuxiliaryTableFieldsModelList = FieldsModelList.Where(x => x.__vModel__.Contains("_qt_")).ToList(); // 单控件副表集合
            ChildTableFieldsModelList = FieldsModelList.Where(x => x.__config__.qtKey == "table").ToList(); // 子表集合
            MainTableFieldsModelList = FieldsModelList.Except(AuxiliaryTableFieldsModelList).Except(ChildTableFieldsModelList).ToList(); // 主表控件集合
            SingleFormData = FieldsModelList.Where(x => x.__config__.qtKey != "table").ToList(); // 非子表集合
        }
        else
        {
            IsHasTable = !string.IsNullOrEmpty(entity.Tables) && !"[]".Equals(entity.Tables); // 是否有表
            MainTable = entity.Tables.ToList<TableModel>().Find(m => m.typeId.Equals("1")); // 主表
            MainTableName = MainTable?.table; // 主表名称

            AuxiliaryTableFieldsModelList = new List<FieldsModel>();
            MainTableFieldsModelList = new List<FieldsModel>();
            SingleFormData = new List<FieldsModel>();
            FieldsModelList = new List<FieldsModel>();
        }


        if (!string.IsNullOrWhiteSpace(entity.ColumnData)) ColumnData = TemplateKeywordsHelper.ReplaceKeywords(entity.ColumnData).ToObject<ColumnDesignModel>(); // 列配置模型
        else ColumnData = new ColumnDesignModel();

        if (!string.IsNullOrWhiteSpace(entity.AppColumnData)) AppColumnData = TemplateKeywordsHelper.ReplaceKeywords(entity.AppColumnData).ToObject<ColumnDesignModel>(); // 列配置模型
        else AppColumnData = new ColumnDesignModel();

        if (AppColumnData.columnList != null && AppColumnData.columnList.Any())
        {
            AppColumnData.columnList.ForEach(item =>
            {
                var addColumn = ColumnData.columnList.Find(x => x.prop == item.prop);
                if (addColumn == null) ColumnData.columnList.Add(item);
            });
        }

        if (AppColumnData.searchList != null && AppColumnData.searchList.Any())
        {
            AppColumnData.searchList.ForEach(item =>
            {
                var addSearch = ColumnData.searchList.Find(x => x.__config__.qtKey == item.__config__.qtKey);
                if (addSearch == null) ColumnData.searchList.Add(item);
            });
        }

        if (ColumnData.searchList != null && ColumnData.searchList.Any())
        {
            ColumnData.searchList.Where(x => x.__config__.qtKey == QtKeyConst.CASCADER).ToList().ForEach(item =>
            {
                var it = SingleFormData.FirstOrDefault(x => x.__vModel__ == item.__vModel__);
                if (it != null) item.multiple = it.props.props.multiple;
            });
        }
    }

    /// <summary>
    /// 验证模板.
    /// </summary>
    /// <returns>true 通过.</returns>
    public bool VerifyTemplate()
    {
        if (FieldsModelList != null && FieldsModelList.Any(x => x.__config__.qtKey == "table"))
        {
            foreach (FieldsModel? item in ChildTableFieldsModelList)
            {
                FieldsModel? tc = AuxiliaryTableFieldsModelList.Find(x => x.__vModel__.Contains(item.__config__.tableName + "_qt_"));
                if (tc != null) return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 时间控件 查询时转换：DateTime.MaxValue + 查询时间.
    /// </summary>
    /// <param name="queryJson"></param>
    /// <returns>queryJson</returns>
    public string TimeControlQueryConvert(string queryJson)
    {
        if (!string.IsNullOrWhiteSpace(queryJson))
        {
            List<FieldsModel>? mainFormData = FieldsModelList.Where(x => x.__config__.qtKey == "time").ToList(); // 获取所有 时间 控件
            Dictionary<string, object>? newJson = new Dictionary<string, object>();

            List<KeyValuePair<string, object>>? sjson = queryJson.ToObject<Dictionary<string, object>>().ToList();
            sjson.ForEach(item =>
            {
                List<object>? vlist = new List<object>();
                FieldsModel? vmodel = mainFormData.Find(x => x.__vModel__ == item.Key);
                if (vmodel != null)
                {
                    item.Value.ToJsonString().ToList<string>().ForEach(it =>
                    {
                        vlist.Add((DateTime.MaxValue.ToShortDateString() + " " + it).ParseToDateTime().ParseToUnixTime());
                    });
                    newJson.Add(item.Key, vlist);
                }
                else
                {
                    newJson.Add(item.Key, item.Value);
                }
            });

            return newJson.ToJsonString();
        }

        return queryJson;
    }

    /// <summary>
    /// 获取带数据转换的控件
    /// 移除模板内的布局类型控件.
    /// </summary>
    /// <param name="fieldsModelList">组件集合.</param>
    /// <returns>带数据的控件.</returns>
    public List<FieldsModel> GetInDataFieldsModel(List<FieldsModel> fieldsModelList)
    {
        List<FieldsModel>? template = new List<FieldsModel>();

        // 将模板内的无限children解析出来
        // 不包含子表children
        foreach (FieldsModel? item in fieldsModelList)
        {
            ConfigModel? config = item.__config__;
            switch (config.qtKey)
            {
                case QtKeyConst.TABLE: // 表格
                    template.Add(item);
                    break;
                case QtKeyConst.ROW: // 卡片
                case QtKeyConst.CARD: // 栅格布局
                    template.AddRange(GetInDataFieldsModel(config.children));
                    break;
                case QtKeyConst.COLLAPSE: // 折叠面板
                case QtKeyConst.TAB: // Tab标签
                    config.children.ForEach(item => template.AddRange(GetInDataFieldsModel(item.__config__.children)));
                    break;
                case QtKeyConst.QTTEXT: // 文本
                case QtKeyConst.DIVIDER: // 分割线
                case QtKeyConst.BUTTON: // 按钮
                case QtKeyConst.GROUPTITLE: // 分组标题
                    break;
                default:
                    template.Add(item);
                    break;
            }
        }

        return template;
    }


}

