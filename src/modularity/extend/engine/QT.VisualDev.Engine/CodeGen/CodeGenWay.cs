using QT.Common.Const;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Security;
using QT.FriendlyException;
using QT.Systems.Entitys.Model.DataBase;
using QT.VisualDev.Engine.Model.CodeGen;
using QT.VisualDev.Engine.Security;
using QT.VisualDev.Entitys;

namespace QT.VisualDev.Engine.CodeGen;

/// <summary>
/// 代码生成方式.
/// </summary>
public class CodeGenWay
{
    /// <summary>
    /// 单表后端.
    /// </summary>
    /// <param name="tableName">表名称.</param>
    /// <param name="dbTableFields">表字段.</param>
    /// <param name="controls">控件列表.</param>
    /// <param name="templateEntity">模板实体.</param>
    /// <returns></returns>
    public static CodeGenConfigModel SingleTableBackEnd(string? tableName, List<DbTableFieldModel> dbTableFields, List<FieldsModel> controls, VisualDevEntity templateEntity)
    {
        // 表单数据
        ColumnDesignModel pcColumnDesignModel = templateEntity.ColumnData?.ToObject<ColumnDesignModel>();
        ColumnDesignModel appColumnDesignModel = templateEntity.AppColumnData?.ToObject<ColumnDesignModel>();
        List<IndexSearchFieldModel> searchList = pcColumnDesignModel?.searchList.Union(appColumnDesignModel?.searchList, EqualityHelper<IndexSearchFieldModel>.CreateComparer(it => it.prop)).ToList();
        List<IndexGridFieldModel> columnList = pcColumnDesignModel?.columnList.Union(appColumnDesignModel?.columnList, EqualityHelper<IndexGridFieldModel>.CreateComparer(it => it.prop)).ToList();
        ColumnDesignModel columnDesignModel = templateEntity.ColumnData?.ToObject<ColumnDesignModel>();
        columnDesignModel ??= new ColumnDesignModel();
        columnDesignModel.searchList = searchList;
        columnDesignModel.columnList = columnList;
        FormDataModel formDataModel = templateEntity.FormData.ToObject<FormDataModel>();

        var defaultFlowId = string.Empty;

        var tableColumnList = new List<TableColumnConfigModel>();
        foreach (DbTableFieldModel? column in dbTableFields)
        {
            var field = column.field.ReplaceRegex("^f_", string.Empty).ParseToPascalCase().ToLowerCase();
            switch (column.primaryKey)
            {
                case true:
                    tableColumnList.Add(new TableColumnConfigModel()
                    {
                        ColumnName = field.ToUpperCase(),
                        OriginalColumnName = column.field,
                        ColumnComment = column.fieldName,
                        DataType = column.dataType,
                        NetType = CodeGenHelper.ConvertDataType(column.dataType),
                        PrimaryKey = true,
                    });
                    break;
                default:
                    if (controls.Any(c => c.__vModel__ == field))
                    {
                        FieldsModel control = controls.Find(c => c.__vModel__ == field);
                        tableColumnList.Add(new TableColumnConfigModel()
                        {
                            ColumnName = field.ToUpperCase(),
                            OriginalColumnName = column.field,
                            ColumnComment = column.fieldName,
                            DataType = column.dataType,
                            NetType = CodeGenHelper.ConvertDataType(column.dataType),
                            PrimaryKey = column.primaryKey.ParseToBool(),
                            QueryWhether = CodeGenFieldJudgeHelper.IsColumnQueryWhether(searchList: columnDesignModel.searchList, field),
                            QueryType = CodeGenFieldJudgeHelper.ColumnQueryType(searchList: columnDesignModel.searchList, field),
                            IsShow = CodeGenFieldJudgeHelper.IsShowColumn(columnDesignModel.columnList, field),
                            IsMultiple = CodeGenFieldJudgeHelper.IsMultipleColumn(controls, field),
                            qtKey = control.__config__.qtKey,
                            Rule = control.__config__.rule,
                            IsDateTime = CodeGenFieldJudgeHelper.IsDateTime(control),
                            ActiveTxt = control.activeTxt,
                            InactiveTxt = control.inactiveTxt
                        });
                    }
                    else
                    {
                        tableColumnList.Add(new TableColumnConfigModel()
                        {
                            ColumnName = field.ToUpperCase(),
                            OriginalColumnName = column.field,
                            ColumnComment = column.fieldName,
                            DataType = column.dataType,
                            NetType = CodeGenHelper.ConvertDataType(column.dataType),
                            PrimaryKey = false
                        });
                    }

                    break;
            }
        }

        if (!tableColumnList.Any(t => t.PrimaryKey))
        {
            throw Oops.Oh(ErrorCode.D2104);
        }

        if ((templateEntity.Type == 3 || templateEntity.WebType == 3) && !tableColumnList.Any(it => it.ColumnName.ToLower().Equals("flowid")))
            throw Oops.Oh(ErrorCode.D2105);
        if (templateEntity.Type == 3 || templateEntity.WebType == 3)
            defaultFlowId = tableColumnList.Find(it => it.ColumnName.ToLower().Equals("flowid")).LowerColumnName;

        // 默认排序 没设置以ID排序.
        var defaultSidx = string.Empty;

        // 是否导出
        bool isExport = false;

        // 是否批量删除
        bool isBatchRemove = false;

        // 是否存在流程id
        bool isFlowId = false;

        switch (templateEntity.WebType)
        {
            case 1:
                // 如果模板为纯表单的情况下给模板赋值默认新增按钮
                columnDesignModel.btnsList = new List<ButtonConfigModel>();
                columnDesignModel.btnsList.Add(new ButtonConfigModel()
                {
                    value = "add",
                    icon = "el-icon-plus",
                    label = "新增",
                });
                break;
            default:
                // 默认排序 没设置以ID排序.
                defaultSidx = columnDesignModel.defaultSidx ?? tableColumnList.Find(t => t.PrimaryKey).ColumnName;
                switch (templateEntity.Type)
                {
                    case 3:
                        break;
                    default:
                        isExport = columnDesignModel.btnsList.Any(it => it.value == "download");
                        isBatchRemove = columnDesignModel.btnsList.Any(it => it.value == "batchRemove");
                        break;
                }

                isFlowId = tableColumnList.Any(it => it.ColumnName.ToLower().Equals("flowid"));
                break;
        }

        // 是否存在上传
        bool isUpload = tableColumnList.Any(it => it.qtKey != null && (it.qtKey.Equals(QtKeyConst.UPLOADIMG) || it.qtKey.Equals(QtKeyConst.UPLOADFZ)));

        // 是否对象映射
        bool isMapper = tableColumnList.Any(it => it.qtKey != null && (it.qtKey.Equals(QtKeyConst.CHECKBOX) || it.qtKey.Equals(QtKeyConst.CASCADER) || it.qtKey.Equals(QtKeyConst.ADDRESS) || it.qtKey.Equals(QtKeyConst.COMSELECT) || it.qtKey.Equals(QtKeyConst.UPLOADIMG) || it.qtKey.Equals(QtKeyConst.UPLOADFZ) || (it.qtKey.Equals(QtKeyConst.SELECT) && it.IsMultiple) || (it.qtKey.Equals(QtKeyConst.USERSELECT) && it.IsMultiple) || (it.qtKey.Equals(QtKeyConst.TREESELECT) && it.IsMultiple) || (it.qtKey.Equals(QtKeyConst.DEPSELECT) && it.IsMultiple) || (it.qtKey.Equals(QtKeyConst.POSSELECT) && it.IsMultiple)));

        // 是否存在单据规则控件
        bool isBillRule = controls.Any(it => it.__config__.qtKey.Equals(QtKeyConst.BILLRULE));

        switch (templateEntity.WebType)
        {
            case 1:
                return new CodeGenConfigModel()
                {
                    NameSpace = formDataModel.areasName,
                    BusName = templateEntity.FullName,
                    ClassName = formDataModel.className,
                    PrimaryKey = tableColumnList.Find(t => t.PrimaryKey).ColumnName,
                    OriginalPrimaryKey = tableColumnList.Find(t => t.PrimaryKey).OriginalColumnName,
                    MainTable = tableName.ParseToPascalCase(),
                    OriginalMainTableName = tableName,
                    hasPage = columnDesignModel.hasPage,
                    Function = CodeGenFunctionHelper.GenerateFunctionList(columnDesignModel.hasPage, columnDesignModel.btnsList, columnDesignModel.columnBtnsList, templateEntity.WebType),
                    TableField = tableColumnList,
                    IsUpload = isUpload,
                    IsTableRelations = false,
                    IsMapper = isMapper,
                    IsBillRule = isBillRule,
                    DbLinkId = templateEntity.DbLinkId,
                    FlowId = templateEntity.Id,
                    WebType = templateEntity.WebType,
                    Type = templateEntity.Type,
                    IsMainTable = true,
                    EnCode = templateEntity.EnCode,
                    IsAuxiliaryTable = false,
                    FlowIdFieldName = defaultFlowId,
                    FullName = templateEntity.FullName,
                };
            default:
                return new CodeGenConfigModel()
                {
                    NameSpace = formDataModel.areasName,
                    BusName = templateEntity.FullName,
                    ClassName = formDataModel.className,
                    PrimaryKey = tableColumnList.Find(t => t.PrimaryKey).ColumnName,
                    OriginalPrimaryKey = tableColumnList.Find(t => t.PrimaryKey).OriginalColumnName,
                    MainTable = tableName.ParseToPascalCase(),
                    OriginalMainTableName = tableName,
                    hasPage = columnDesignModel.hasPage,
                    Function = templateEntity.Type == 3 ? CodeGenFunctionHelper.GetCodeGenFlowFunctionList() : CodeGenFunctionHelper.GenerateFunctionList(columnDesignModel.hasPage, columnDesignModel.btnsList, columnDesignModel.columnBtnsList, templateEntity.WebType),
                    TableField = tableColumnList,
                    DefaultSidx = defaultSidx,
                    IsExport = isExport,
                    IsBatchRemove = isBatchRemove,
                    IsUpload = isUpload,
                    IsTableRelations = false,
                    IsMapper = isMapper,
                    IsBillRule = isBillRule,
                    DbLinkId = templateEntity.DbLinkId,
                    FlowId = templateEntity.Id,
                    WebType = templateEntity.WebType,
                    Type = templateEntity.Type,
                    IsMainTable = true,
                    IsFlowId = isFlowId,
                    EnCode = templateEntity.EnCode,
                    UseDataPermission = (bool)columnDesignModel?.useDataPermission,
                    SearchControlNum = tableColumnList.FindAll(it => it.QueryType.Equals(1) || it.QueryType.Equals(2)).Count(),
                    IsAuxiliaryTable = false,
                    ExportField = templateEntity.Type == 3 ? null : CodeGenExportFieldHelper.ExportColumnField(columnDesignModel.columnList),
                    FlowIdFieldName = defaultFlowId,
                    FullName = templateEntity.FullName,
                };
        }
    }

    /// <summary>
    /// 主表带子表.
    /// </summary>
    /// <param name="tableName">表名称.</param>
    /// <param name="dbTableFields">表字段.</param>
    /// <param name="controls">控件列表.</param>
    /// <param name="templateEntity">模板实体.</param>
    /// <param name="isMainTable">是否主表.</param>
    /// <returns></returns>
    public static CodeGenConfigModel MainBeltBackEnd(string? tableName, List<DbTableFieldModel> dbTableFields, List<FieldsModel> controls, VisualDevEntity templateEntity, bool isMainTable)
    {
        // 表单数据
        ColumnDesignModel pcColumnDesignModel = templateEntity.ColumnData?.ToObject<ColumnDesignModel>();
        ColumnDesignModel appColumnDesignModel = templateEntity.AppColumnData?.ToObject<ColumnDesignModel>();
        List<IndexSearchFieldModel> searchList = pcColumnDesignModel?.searchList.Union(appColumnDesignModel?.searchList, EqualityHelper<IndexSearchFieldModel>.CreateComparer(it => it.prop)).ToList();
        List<IndexGridFieldModel> columnList = pcColumnDesignModel?.columnList.Union(appColumnDesignModel?.columnList, EqualityHelper<IndexGridFieldModel>.CreateComparer(it => it.prop)).ToList();
        ColumnDesignModel columnDesignModel = templateEntity.ColumnData?.ToObject<ColumnDesignModel>();
        columnDesignModel ??= new ColumnDesignModel();
        columnDesignModel.searchList = searchList;
        columnDesignModel.columnList = columnList;
        FormDataModel formDataModel = templateEntity.FormData.ToObject<FormDataModel>();

        string defaultFlowId = string.Empty;

        var tableColumnList = new List<TableColumnConfigModel>();
        foreach (DbTableFieldModel? column in dbTableFields)
        {
            var field = column.field.ReplaceRegex("^f_", string.Empty).ParseToPascalCase().ToLowerCase();
            switch (column.primaryKey)
            {
                case true:
                    tableColumnList.Add(new TableColumnConfigModel()
                    {
                        ColumnName = field.ToUpperCase(),
                        OriginalColumnName = column.field,
                        ColumnComment = column.fieldName,
                        DataType = column.dataType,
                        NetType = CodeGenHelper.ConvertDataType(column.dataType),
                        PrimaryKey = true,
                    });
                    break;
                default:
                    List<FieldsModel> _controls = controls;
                    bool isControls = controls.Any(c => c.__vModel__ == field);
                    if (!isControls && !isMainTable && controls.Any(x => x.__vModel__ == tableName))
                    {
                        _controls = controls.Find(x => x.__vModel__ == tableName).__config__.children ?? new List<FieldsModel>();
                        isControls = _controls.Any(c => c.__vModel__ == field);
                    }
                    if (isControls)
                    {
                        FieldsModel control = _controls.Find(c => c.__vModel__ == field);
                        tableColumnList.Add(new TableColumnConfigModel()
                        {
                            ColumnName = field.ToUpperCase(),
                            OriginalColumnName = column.field,
                            ColumnComment = column.fieldName,
                            DataType = column.dataType,
                            NetType = CodeGenHelper.ConvertDataType(column.dataType),
                            PrimaryKey = column.primaryKey.ParseToBool(),
                            QueryWhether = CodeGenFieldJudgeHelper.IsColumnQueryWhether(searchList: columnDesignModel.searchList, field),
                            QueryType = CodeGenFieldJudgeHelper.ColumnQueryType(searchList: columnDesignModel.searchList, field),
                            IsShow = CodeGenFieldJudgeHelper.IsShowColumn(columnDesignModel.columnList, field),
                            IsMultiple = CodeGenFieldJudgeHelper.IsMultipleColumn(_controls, field),
                            qtKey = control.__config__.qtKey,
                            Rule = control.__config__.rule,
                            IsDateTime = CodeGenFieldJudgeHelper.IsDateTime(control),
                            ActiveTxt = control.activeTxt,
                            InactiveTxt = control.inactiveTxt,
                        });
                    }
                    else
                    {
                        tableColumnList.Add(new TableColumnConfigModel()
                        {
                            ColumnName = field.ToUpperCase(),
                            OriginalColumnName = column.field,
                            ColumnComment = column.fieldName,
                            DataType = column.dataType,
                            NetType = CodeGenHelper.ConvertDataType(column.dataType),
                            PrimaryKey = false
                        });
                    }

                    break;
            }
        }

        if (!tableColumnList.Any(t => t.PrimaryKey)) throw Oops.Oh(ErrorCode.D2104);

        switch (isMainTable)
        {
            case true:
                if ((templateEntity.Type == 3 || templateEntity.WebType == 3) && !tableColumnList.Any(it => it.ColumnName.ToLower().Equals("flowid"))) throw Oops.Oh(ErrorCode.D2105);

                if (templateEntity.Type == 3 || templateEntity.WebType == 3) defaultFlowId = tableColumnList.Find(it => it.ColumnName.ToLower().Equals("flowid")).ColumnName;
                break;
        }

        // 默认排序 没设置以ID排序.
        var defaultSidx = string.Empty;

        // 是否导出
        bool isExport = false;

        // 是否批量删除
        bool isBatchRemove = false;

        // 是否存在流程id
        bool isFlowId = false;

        switch (templateEntity.WebType)
        {
            case 1:
                // 如果模板为纯表单的情况下给模板赋值默认新增按钮
                columnDesignModel.btnsList = new List<ButtonConfigModel>();
                columnDesignModel.btnsList.Add(new ButtonConfigModel()
                {
                    value = "add",
                    icon = "el-icon-plus",
                    label = "新增",
                });
                break;
            default:
                defaultSidx = columnDesignModel.defaultSidx ?? tableColumnList.Find(t => t.PrimaryKey).ColumnName;
                switch (templateEntity.Type)
                {
                    case 3:
                        break;
                    default:
                        isExport = columnDesignModel.btnsList.Any(it => it.value == "download");
                        isBatchRemove = columnDesignModel.btnsList.Any(it => it.value == "batchRemove");
                        break;
                }

                isFlowId = tableColumnList.Any(it => it.ColumnName.ToLower().Equals("flowid"));
                break;
        }

        // 是否存在上传
        bool isUpload = tableColumnList.Any(it => it.qtKey != null && (it.qtKey.Equals(QtKeyConst.UPLOADIMG) || it.qtKey.Equals(QtKeyConst.UPLOADFZ)));

        // 是否对象映射
        bool isMapper = tableColumnList.Any(it => it.qtKey != null && (it.qtKey.Equals(QtKeyConst.CHECKBOX) || it.qtKey.Equals(QtKeyConst.CASCADER) || it.qtKey.Equals(QtKeyConst.ADDRESS) || it.qtKey.Equals(QtKeyConst.COMSELECT) || it.qtKey.Equals(QtKeyConst.UPLOADIMG) || it.qtKey.Equals(QtKeyConst.UPLOADFZ) || (it.qtKey.Equals(QtKeyConst.SELECT) && it.IsMultiple) || (it.qtKey.Equals(QtKeyConst.USERSELECT) && it.IsMultiple) || (it.qtKey.Equals(QtKeyConst.TREESELECT) && it.IsMultiple) || (it.qtKey.Equals(QtKeyConst.DEPSELECT) && it.IsMultiple) || (it.qtKey.Equals(QtKeyConst.POSSELECT) && it.IsMultiple)));

        // 是否存在单据规则控件
        bool isBillRule = controls.Any(it => it.__config__.qtKey.Equals(QtKeyConst.BILLRULE)) || controls.FindAll(it => it.__config__.qtKey.Equals(QtKeyConst.TABLE)).Any(it => it.__config__.children.Any(chidren => chidren.__config__.qtKey.Equals(QtKeyConst.BILLRULE)));

        switch (templateEntity.WebType)
        {
            case 1:
                return new CodeGenConfigModel()
                {
                    NameSpace = formDataModel.areasName,
                    BusName = templateEntity.FullName,
                    ClassName = formDataModel.className,
                    PrimaryKey = tableColumnList.Find(t => t.PrimaryKey).ColumnName,
                    OriginalPrimaryKey = tableColumnList.Find(t => t.PrimaryKey).OriginalColumnName,
                    MainTable = tableName.ParseToPascalCase(),
                    OriginalMainTableName = tableName,
                    hasPage = columnDesignModel.hasPage,
                    Function = CodeGenFunctionHelper.GenerateFunctionList(columnDesignModel.hasPage, columnDesignModel.btnsList, columnDesignModel.columnBtnsList, templateEntity.WebType),
                    TableField = tableColumnList,
                    IsUpload = isUpload,
                    IsTableRelations = false,
                    IsMapper = isMapper,
                    IsBillRule = isBillRule,
                    DbLinkId = templateEntity.DbLinkId,
                    FlowId = templateEntity.Id,
                    WebType = templateEntity.WebType,
                    Type = templateEntity.Type,
                    IsMainTable = isMainTable,
                    EnCode = templateEntity.EnCode,
                    IsAuxiliaryTable = false,
                    FlowIdFieldName = defaultFlowId,
                    FullName = templateEntity.FullName,
                };
            default:
                return new CodeGenConfigModel()
                {
                    NameSpace = formDataModel.areasName,
                    BusName = templateEntity.FullName,
                    ClassName = formDataModel.className,
                    PrimaryKey = tableColumnList.Find(t => t.PrimaryKey).ColumnName,
                    OriginalPrimaryKey = tableColumnList.Find(t => t.PrimaryKey).OriginalColumnName,
                    MainTable = tableName.ParseToPascalCase(),
                    OriginalMainTableName = tableName,
                    hasPage = columnDesignModel.hasPage,
                    Function = templateEntity.Type == 3 ? CodeGenFunctionHelper.GetCodeGenFlowFunctionList() : CodeGenFunctionHelper.GenerateFunctionList(columnDesignModel.hasPage, columnDesignModel.btnsList, columnDesignModel.columnBtnsList, templateEntity.WebType),
                    TableField = tableColumnList,
                    DefaultSidx = defaultSidx,
                    IsExport = isExport,
                    IsBatchRemove = isBatchRemove,
                    IsUpload = isUpload,
                    IsTableRelations = false,
                    IsMapper = isMapper,
                    IsBillRule = isBillRule,
                    DbLinkId = templateEntity.DbLinkId,
                    FlowId = templateEntity.Id,
                    WebType = templateEntity.WebType,
                    Type = templateEntity.Type,
                    IsMainTable = isMainTable,
                    IsFlowId = isFlowId,
                    EnCode = templateEntity.EnCode,
                    UseDataPermission = (bool)columnDesignModel?.useDataPermission,
                    SearchControlNum = tableColumnList.FindAll(it => it.QueryType.Equals(1) || it.QueryType.Equals(2)).Count(),
                    IsAuxiliaryTable = false,
                    ExportField = templateEntity.Type == 3 ? null : CodeGenExportFieldHelper.ExportColumnField(columnDesignModel.columnList),
                    FlowIdFieldName = defaultFlowId,
                    FullName = templateEntity.FullName,
                };
        }
    }

    /// <summary>
    /// 主表带副表.
    /// </summary>
    /// <param name="tableName">表名称.</param>
    /// <param name="dbTableFields">表字段.</param>
    /// <param name="auxiliaryTableColumnList">副表字段配置.</param>
    /// <param name="controls">控件列表.</param>
    /// <param name="templateEntity">模板实体.</param>
    /// <returns></returns>
    public static CodeGenConfigModel MainBeltViceBackEnd(string? tableName, List<DbTableFieldModel> dbTableFields, List<TableColumnConfigModel> auxiliaryTableColumnList, List<FieldsModel> controls, VisualDevEntity templateEntity)
    {
        // 表单数据
        ColumnDesignModel pcColumnDesignModel = templateEntity.ColumnData?.ToObject<ColumnDesignModel>();
        ColumnDesignModel appColumnDesignModel = templateEntity.AppColumnData?.ToObject<ColumnDesignModel>();
        List<IndexSearchFieldModel> searchList = pcColumnDesignModel?.searchList.Union(appColumnDesignModel?.searchList, EqualityHelper<IndexSearchFieldModel>.CreateComparer(it => it.prop)).ToList();
        List<IndexGridFieldModel> columnList = pcColumnDesignModel?.columnList.Union(appColumnDesignModel?.columnList, EqualityHelper<IndexGridFieldModel>.CreateComparer(it => it.prop)).ToList();
        ColumnDesignModel columnDesignModel = templateEntity.ColumnData?.ToObject<ColumnDesignModel>();
        columnDesignModel ??= new ColumnDesignModel();
        columnDesignModel.searchList = searchList;
        columnDesignModel.columnList = columnList;
        FormDataModel formDataModel = templateEntity.FormData.ToObject<FormDataModel>();

        string defaultFlowId = string.Empty;

        var tableColumnList = new List<TableColumnConfigModel>();

        foreach (DbTableFieldModel? column in dbTableFields)
        {
            var field = column.field.ReplaceRegex("^f_", string.Empty).ParseToPascalCase().ToLowerCase();
            switch (column.primaryKey)
            {
                case true:
                    tableColumnList.Add(new TableColumnConfigModel()
                    {
                        ColumnName = field.ToUpperCase(),
                        OriginalColumnName = column.field,
                        ColumnComment = column.fieldName,
                        DataType = column.dataType,
                        NetType = CodeGenHelper.ConvertDataType(column.dataType),
                        PrimaryKey = true,
                        IsAuxiliary = false
                    });
                    break;
                default:
                    if (controls.Any(c => c.__vModel__ == field))
                    {
                        FieldsModel control = controls.Find(c => c.__vModel__ == field);
                        tableColumnList.Add(new TableColumnConfigModel()
                        {
                            ColumnName = field.ToUpperCase(),
                            OriginalColumnName = column.field,
                            ColumnComment = column.fieldName,
                            DataType = column.dataType,
                            NetType = CodeGenHelper.ConvertDataType(column.dataType),
                            PrimaryKey = column.primaryKey.ParseToBool(),
                            QueryWhether = CodeGenFieldJudgeHelper.IsColumnQueryWhether(searchList: columnDesignModel.searchList, field),
                            QueryType = CodeGenFieldJudgeHelper.ColumnQueryType(searchList: columnDesignModel.searchList, field),
                            IsShow = CodeGenFieldJudgeHelper.IsShowColumn(columnDesignModel.columnList, field),
                            IsMultiple = CodeGenFieldJudgeHelper.IsMultipleColumn(controls, field),
                            qtKey = control.__config__.qtKey,
                            Rule = control.__config__.rule,
                            IsDateTime = CodeGenFieldJudgeHelper.IsDateTime(control),
                            ActiveTxt = control.activeTxt,
                            InactiveTxt = control.inactiveTxt,
                            IsAuxiliary = false,
                            TableName = tableName,
                        });
                    }
                    else
                    {
                        tableColumnList.Add(new TableColumnConfigModel()
                        {
                            ColumnName = field.ToUpperCase(),
                            OriginalColumnName = column.field,
                            ColumnComment = column.fieldName,
                            DataType = column.dataType,
                            NetType = CodeGenHelper.ConvertDataType(column.dataType),
                            PrimaryKey = false,
                            IsAuxiliary = false
                        });
                    }

                    break;
            }
        }

        if (!tableColumnList.Any(t => t.PrimaryKey))
        {
            throw Oops.Oh(ErrorCode.D2104);
        }

        if ((templateEntity.Type == 3 || templateEntity.WebType == 3) && !tableColumnList.Any(it => it.ColumnName.ToLower().Equals("flowid")))
            throw Oops.Oh(ErrorCode.D2105);
        if (templateEntity.Type == 3 || templateEntity.WebType == 3)
            defaultFlowId = tableColumnList.Find(it => it.ColumnName.ToLower().Equals("flowid")).LowerColumnName;

        // 默认排序 没设置以ID排序.
        var defaultSidx = string.Empty;

        // 是否导出
        bool isExport = false;

        // 是否批量删除
        bool isBatchRemove = false;

        // 是否存在流程id
        bool isFlowId = false;

        switch (templateEntity.WebType)
        {
            case 1:
                // 如果模板为纯表单的情况下给模板赋值默认新增按钮
                columnDesignModel.btnsList = new List<ButtonConfigModel>();
                columnDesignModel.btnsList.Add(new ButtonConfigModel()
                {
                    value = "add",
                    icon = "el-icon-plus",
                    label = "新增",
                });
                break;
            default:
                // 默认排序 没设置以ID排序.
                defaultSidx = columnDesignModel.defaultSidx ?? tableColumnList.Find(t => t.PrimaryKey).ColumnName;
                switch (templateEntity.Type)
                {
                    case 3:
                        break;
                    default:
                        isExport = columnDesignModel.btnsList.Any(it => it.value == "download");
                        isBatchRemove = columnDesignModel.btnsList.Any(it => it.value == "batchRemove");
                        break;
                }

                isFlowId = tableColumnList.Any(it => it.ColumnName.ToLower().Equals("flowid"));
                break;
        }

        tableColumnList.AddRange(auxiliaryTableColumnList);

        // 是否存在上传
        bool isUpload = tableColumnList.Any(it => it.qtKey != null && (it.qtKey.Equals(QtKeyConst.UPLOADIMG) || it.qtKey.Equals(QtKeyConst.UPLOADFZ)));

        // 是否对象映射
        bool isMapper = tableColumnList.Any(it => it.qtKey != null && (it.qtKey.Equals(QtKeyConst.CHECKBOX) || it.qtKey.Equals(QtKeyConst.CASCADER) || it.qtKey.Equals(QtKeyConst.ADDRESS) || it.qtKey.Equals(QtKeyConst.COMSELECT) || it.qtKey.Equals(QtKeyConst.UPLOADIMG) || it.qtKey.Equals(QtKeyConst.UPLOADFZ) || (it.qtKey.Equals(QtKeyConst.SELECT) && it.IsMultiple) || (it.qtKey.Equals(QtKeyConst.USERSELECT) && it.IsMultiple) || (it.qtKey.Equals(QtKeyConst.TREESELECT) && it.IsMultiple) || (it.qtKey.Equals(QtKeyConst.DEPSELECT) && it.IsMultiple) || (it.qtKey.Equals(QtKeyConst.POSSELECT) && it.IsMultiple)));

        // 是否存在单据规则控件
        bool isBillRule = controls.Any(it => it.__config__.qtKey.Equals(QtKeyConst.BILLRULE));

        switch (templateEntity.WebType)
        {
            case 1:
                return new CodeGenConfigModel()
                {
                    NameSpace = formDataModel.areasName,
                    BusName = templateEntity.FullName,
                    ClassName = formDataModel.className,
                    PrimaryKey = tableColumnList.Find(t => t.PrimaryKey).ColumnName,
                    OriginalPrimaryKey = tableColumnList.Find(t => t.PrimaryKey).OriginalColumnName,
                    MainTable = tableName.ParseToPascalCase(),
                    OriginalMainTableName = tableName,
                    hasPage = columnDesignModel.hasPage,
                    Function = CodeGenFunctionHelper.GenerateFunctionList(columnDesignModel.hasPage, columnDesignModel.btnsList, columnDesignModel.columnBtnsList, templateEntity.WebType),
                    TableField = tableColumnList,
                    IsUpload = isUpload,
                    IsTableRelations = false,
                    IsMapper = isMapper,
                    IsBillRule = isBillRule,
                    DbLinkId = templateEntity.DbLinkId,
                    FlowId = templateEntity.Id,
                    WebType = templateEntity.WebType,
                    Type = templateEntity.Type,
                    IsMainTable = true,
                    EnCode = templateEntity.EnCode,
                    IsAuxiliaryTable = true,
                    FlowIdFieldName = defaultFlowId,
                    FullName = templateEntity.FullName,
                };
            default:
                return new CodeGenConfigModel()
                {
                    NameSpace = formDataModel.areasName,
                    BusName = templateEntity.FullName,
                    ClassName = formDataModel.className,
                    PrimaryKey = tableColumnList.Find(t => t.PrimaryKey).ColumnName,
                    OriginalPrimaryKey = tableColumnList.Find(t => t.PrimaryKey).OriginalColumnName,
                    MainTable = tableName.ParseToPascalCase(),
                    OriginalMainTableName = tableName,
                    hasPage = columnDesignModel.hasPage,
                    Function = templateEntity.Type == 3 ? CodeGenFunctionHelper.GetCodeGenFlowFunctionList() : CodeGenFunctionHelper.GenerateFunctionList(columnDesignModel.hasPage, columnDesignModel.btnsList, columnDesignModel.columnBtnsList, templateEntity.WebType),
                    TableField = tableColumnList,
                    DefaultSidx = defaultSidx,
                    IsExport = isExport,
                    IsBatchRemove = isBatchRemove,
                    IsUpload = isUpload,
                    IsTableRelations = false,
                    IsMapper = isMapper,
                    IsBillRule = isBillRule,
                    DbLinkId = templateEntity.DbLinkId,
                    FlowId = templateEntity.Id,
                    WebType = templateEntity.WebType,
                    Type = templateEntity.Type,
                    IsMainTable = true,
                    IsFlowId = isFlowId,
                    EnCode = templateEntity.EnCode,
                    UseDataPermission = (bool)columnDesignModel?.useDataPermission,
                    SearchControlNum = tableColumnList.FindAll(it => it.QueryType.Equals(1) || it.QueryType.Equals(2)).Count(),
                    IsAuxiliaryTable = true,
                    ExportField = templateEntity.Type == 3 ? null : CodeGenExportFieldHelper.ExportColumnField(columnDesignModel.columnList),
                    FlowIdFieldName = defaultFlowId,
                    FullName = templateEntity.FullName,
                };
        }
    }

    /// <summary>
    /// 主表带子副表.
    /// </summary>
    /// <param name="tableName">表名称.</param>
    /// <param name="dbTableFields">表字段.</param>
    /// <param name="auxiliaryTableColumnList">副表字段配置.</param>
    /// <param name="controls">控件列表.</param>
    /// <param name="templateEntity">模板实体.</param>
    /// <param name="isMainTable">是否主表.</param>
    /// <returns></returns>
    public static CodeGenConfigModel PrimarySecondaryBackEnd(string? tableName, List<DbTableFieldModel> dbTableFields, List<TableColumnConfigModel> auxiliaryTableColumnList, List<FieldsModel> controls, VisualDevEntity templateEntity, bool isMainTable)
    {
        // 表单数据
        ColumnDesignModel pcColumnDesignModel = templateEntity.ColumnData?.ToObject<ColumnDesignModel>();
        ColumnDesignModel appColumnDesignModel = templateEntity.AppColumnData?.ToObject<ColumnDesignModel>();
        List<IndexSearchFieldModel> searchList = pcColumnDesignModel?.searchList.Union(appColumnDesignModel?.searchList, EqualityHelper<IndexSearchFieldModel>.CreateComparer(it => it.prop)).ToList();
        List<IndexGridFieldModel> columnList = pcColumnDesignModel?.columnList.Union(appColumnDesignModel?.columnList, EqualityHelper<IndexGridFieldModel>.CreateComparer(it => it.prop)).ToList();
        ColumnDesignModel columnDesignModel = templateEntity.ColumnData?.ToObject<ColumnDesignModel>();
        columnDesignModel ??= new ColumnDesignModel();
        columnDesignModel.searchList = searchList;
        columnDesignModel.columnList = columnList;
        FormDataModel formDataModel = templateEntity.FormData.ToObject<FormDataModel>();

        string defaultFlowId = string.Empty;

        var tableColumnList = new List<TableColumnConfigModel>();

        foreach (DbTableFieldModel? column in dbTableFields)
        {
            var field = column.field.ReplaceRegex("^f_", string.Empty).ParseToPascalCase().ToLowerCase();
            switch (column.primaryKey)
            {
                case true:
                    tableColumnList.Add(new TableColumnConfigModel()
                    {
                        ColumnName = field.ToUpperCase(),
                        OriginalColumnName = column.field,
                        ColumnComment = column.fieldName,
                        DataType = column.dataType,
                        NetType = CodeGenHelper.ConvertDataType(column.dataType),
                        PrimaryKey = true,
                        IsAuxiliary = false
                    });
                    break;
                default:
                    if (controls.Any(c => c.__vModel__ == field))
                    {
                        FieldsModel control = controls.Find(c => c.__vModel__ == field);
                        tableColumnList.Add(new TableColumnConfigModel()
                        {
                            ColumnName = field.ToUpperCase(),
                            OriginalColumnName = column.field,
                            ColumnComment = column.fieldName,
                            DataType = column.dataType,
                            NetType = CodeGenHelper.ConvertDataType(column.dataType),
                            PrimaryKey = column.primaryKey.ParseToBool(),
                            QueryWhether = CodeGenFieldJudgeHelper.IsColumnQueryWhether(searchList: columnDesignModel.searchList, field),
                            QueryType = CodeGenFieldJudgeHelper.ColumnQueryType(searchList: columnDesignModel.searchList, field),
                            IsShow = CodeGenFieldJudgeHelper.IsShowColumn(columnDesignModel.columnList, field),
                            IsMultiple = CodeGenFieldJudgeHelper.IsMultipleColumn(controls, field),
                            qtKey = control.__config__.qtKey,
                            Rule = control.__config__.rule,
                            IsDateTime = CodeGenFieldJudgeHelper.IsDateTime(control),
                            ActiveTxt = control.activeTxt,
                            InactiveTxt = control.inactiveTxt,
                            IsAuxiliary = false,
                            TableName = tableName,
                        });
                    }
                    else
                    {
                        tableColumnList.Add(new TableColumnConfigModel()
                        {
                            ColumnName = field.ToUpperCase(),
                            OriginalColumnName = column.field,
                            ColumnComment = column.fieldName,
                            DataType = column.dataType,
                            NetType = CodeGenHelper.ConvertDataType(column.dataType),
                            PrimaryKey = false,
                            IsAuxiliary = false
                        });
                    }

                    break;
            }
        }

        if (!tableColumnList.Any(t => t.PrimaryKey)) throw Oops.Oh(ErrorCode.D2104);

        if ((templateEntity.Type == 3 || templateEntity.WebType == 3) && !tableColumnList.Any(it => it.ColumnName.ToLower().Equals("flowid"))) throw Oops.Oh(ErrorCode.D2105);

        if (templateEntity.Type == 3 || templateEntity.WebType == 3) defaultFlowId = tableColumnList.Find(it => it.ColumnName.ToLower().Equals("flowid")).LowerColumnName;

        // 默认排序 没设置以ID排序.
        var defaultSidx = string.Empty;

        // 是否导出
        bool isExport = false;

        // 是否批量删除
        bool isBatchRemove = false;

        // 是否存在流程id
        bool isFlowId = false;

        switch (templateEntity.WebType)
        {
            case 1:
                // 如果模板为纯表单的情况下给模板赋值默认新增按钮
                columnDesignModel.btnsList = new List<ButtonConfigModel>();
                columnDesignModel.btnsList.Add(new ButtonConfigModel()
                {
                    value = "add",
                    icon = "el-icon-plus",
                    label = "新增",
                });
                break;
            default:
                // 默认排序 没设置以ID排序.
                defaultSidx = columnDesignModel.defaultSidx ?? tableColumnList.Find(t => t.PrimaryKey).ColumnName;
                switch (templateEntity.Type)
                {
                    case 3:
                        break;
                    default:
                        isExport = columnDesignModel.btnsList.Any(it => it.value == "download");
                        isBatchRemove = columnDesignModel.btnsList.Any(it => it.value == "batchRemove");
                        break;
                }

                isFlowId = tableColumnList.Any(it => it.ColumnName.ToLower().Equals("flowid"));
                break;
        }

        tableColumnList.AddRange(auxiliaryTableColumnList);

        // 是否存在上传
        bool isUpload = tableColumnList.Any(it => it.qtKey != null && (it.qtKey.Equals(QtKeyConst.UPLOADIMG) || it.qtKey.Equals(QtKeyConst.UPLOADFZ)));

        // 是否对象映射
        bool isMapper = tableColumnList.Any(it => it.qtKey != null && (it.qtKey.Equals(QtKeyConst.CHECKBOX) || it.qtKey.Equals(QtKeyConst.CASCADER) || it.qtKey.Equals(QtKeyConst.ADDRESS) || it.qtKey.Equals(QtKeyConst.COMSELECT) || it.qtKey.Equals(QtKeyConst.UPLOADIMG) || it.qtKey.Equals(QtKeyConst.UPLOADFZ) || (it.qtKey.Equals(QtKeyConst.SELECT) && it.IsMultiple) || (it.qtKey.Equals(QtKeyConst.USERSELECT) && it.IsMultiple) || (it.qtKey.Equals(QtKeyConst.TREESELECT) && it.IsMultiple) || (it.qtKey.Equals(QtKeyConst.DEPSELECT) && it.IsMultiple) || (it.qtKey.Equals(QtKeyConst.POSSELECT) && it.IsMultiple)));

        // 是否存在单据规则控件
        bool isBillRule = controls.Any(it => it.__config__.qtKey.Equals(QtKeyConst.BILLRULE)) || controls.FindAll(it => it.__config__.qtKey.Equals(QtKeyConst.TABLE)).Any(it => it.__config__.children.Any(chidren => chidren.__config__.qtKey.Equals(QtKeyConst.BILLRULE)));

        switch (templateEntity.WebType)
        {
            case 1:
                return new CodeGenConfigModel()
                {
                    NameSpace = formDataModel.areasName,
                    BusName = templateEntity.FullName,
                    ClassName = formDataModel.className,
                    PrimaryKey = tableColumnList.Find(t => t.PrimaryKey).ColumnName,
                    OriginalPrimaryKey = tableColumnList.Find(t => t.PrimaryKey).OriginalColumnName,
                    MainTable = tableName.ParseToPascalCase(),
                    OriginalMainTableName = tableName,
                    hasPage = columnDesignModel.hasPage,
                    Function = CodeGenFunctionHelper.GenerateFunctionList(columnDesignModel.hasPage, columnDesignModel.btnsList, columnDesignModel.columnBtnsList, templateEntity.WebType),
                    TableField = tableColumnList,
                    IsUpload = isUpload,
                    IsTableRelations = false,
                    IsMapper = isMapper,
                    IsBillRule = isBillRule,
                    DbLinkId = templateEntity.DbLinkId,
                    FlowId = templateEntity.Id,
                    WebType = templateEntity.WebType,
                    Type = templateEntity.Type,
                    IsMainTable = true,
                    EnCode = templateEntity.EnCode,
                    IsAuxiliaryTable = true,
                    FlowIdFieldName = defaultFlowId,
                    FullName = templateEntity.FullName,
                };
            default:
                return new CodeGenConfigModel()
                {
                    NameSpace = formDataModel.areasName,
                    BusName = templateEntity.FullName,
                    ClassName = formDataModel.className,
                    PrimaryKey = tableColumnList.Find(t => t.PrimaryKey).ColumnName,
                    OriginalPrimaryKey = tableColumnList.Find(t => t.PrimaryKey).OriginalColumnName,
                    MainTable = tableName.ParseToPascalCase(),
                    OriginalMainTableName = tableName,
                    hasPage = columnDesignModel.hasPage,
                    Function = templateEntity.Type == 3 ? CodeGenFunctionHelper.GetCodeGenFlowFunctionList() : CodeGenFunctionHelper.GenerateFunctionList(columnDesignModel.hasPage, columnDesignModel.btnsList, columnDesignModel.columnBtnsList, templateEntity.WebType),
                    TableField = tableColumnList,
                    DefaultSidx = defaultSidx,
                    IsExport = isExport,
                    IsBatchRemove = isBatchRemove,
                    IsUpload = isUpload,
                    IsTableRelations = false,
                    IsMapper = isMapper,
                    IsBillRule = isBillRule,
                    DbLinkId = templateEntity.DbLinkId,
                    FlowId = templateEntity.Id,
                    WebType = templateEntity.WebType,
                    Type = templateEntity.Type,
                    IsMainTable = true,
                    IsFlowId = isFlowId,
                    EnCode = templateEntity.EnCode,
                    UseDataPermission = (bool)columnDesignModel?.useDataPermission,
                    SearchControlNum = tableColumnList.FindAll(it => it.QueryType.Equals(1) || it.QueryType.Equals(2)).Count(),
                    IsAuxiliaryTable = true,
                    ExportField = templateEntity.Type == 3 ? null : CodeGenExportFieldHelper.ExportColumnField(columnDesignModel.columnList),
                    FlowIdFieldName = defaultFlowId,
                    FullName = templateEntity.FullName,
                };
        }
    }

    /// <summary>
    /// 副表表字段配置.
    /// </summary>
    /// <param name="tableName">表名称.</param>
    /// <param name="mainTableName">主表名称.</param>
    /// <param name="dbTableFields">表字段.</param>
    /// <param name="controls">控件列表.</param>
    /// <param name="templateEntity">模板实体.</param>
    /// <param name="tableNo">表序号.</param>
    /// <returns></returns>
    public static CodeGenConfigModel AuxiliaryTableBackEnd(string? tableName, string? mainTableName, List<DbTableFieldModel> dbTableFields, List<FieldsModel> controls, VisualDevEntity templateEntity, int tableNo)
    {
        ColumnDesignModel pcColumnDesignModel = templateEntity.ColumnData?.ToObject<ColumnDesignModel>();
        ColumnDesignModel appColumnDesignModel = templateEntity.AppColumnData?.ToObject<ColumnDesignModel>();
        List<IndexSearchFieldModel> searchList = pcColumnDesignModel?.searchList.Union(appColumnDesignModel?.searchList, EqualityHelper<IndexSearchFieldModel>.CreateComparer(it => it.prop)).ToList();
        List<IndexGridFieldModel> columnList = pcColumnDesignModel?.columnList.Union(appColumnDesignModel?.columnList, EqualityHelper<IndexGridFieldModel>.CreateComparer(it => it.prop)).ToList();
        ColumnDesignModel columnDesignModel = templateEntity.ColumnData?.ToObject<ColumnDesignModel>();
        columnDesignModel ??= new ColumnDesignModel();
        columnDesignModel.searchList = searchList;
        columnDesignModel.columnList = columnList;
        FormDataModel formDataModel = templateEntity.FormData.ToObject<FormDataModel>();

        var tableColumnList = new List<TableColumnConfigModel>();
        foreach (DbTableFieldModel? column in dbTableFields)
        {
            var field = column.field.ReplaceRegex("^f_", string.Empty).ParseToPascalCase().ToLowerCase();
            switch (column.primaryKey)
            {
                case true:
                    tableColumnList.Add(new TableColumnConfigModel()
                    {
                        ColumnName = field.ToUpperCase(),
                        OriginalColumnName = column.field,
                        ColumnComment = column.fieldName,
                        DataType = column.dataType,
                        NetType = CodeGenHelper.ConvertDataType(column.dataType),
                        PrimaryKey = true,
                        IsAuxiliary = true,
                        TableNo = tableNo,
                    });
                    break;
                default:
                    if (controls.Any(c => c.__vModel__.Equals(string.Format("qt_{0}_qt_{1}", tableName, field))))
                    {
                        FieldsModel control = controls.Find(c => c.__vModel__.Equals(string.Format("qt_{0}_qt_{1}", tableName, field)));
                        tableColumnList.Add(new TableColumnConfigModel()
                        {
                            ColumnName = field.ToUpperCase(),
                            OriginalColumnName = column.field,
                            ColumnComment = column.fieldName,
                            DataType = column.dataType,
                            NetType = CodeGenHelper.ConvertDataType(column.dataType),
                            PrimaryKey = column.primaryKey.ParseToBool(),
                            QueryWhether = CodeGenFieldJudgeHelper.IsColumnQueryWhether(searchList: columnDesignModel.searchList, string.Format("qt_{0}_qt_{1}", tableName, field)),
                            QueryType = CodeGenFieldJudgeHelper.ColumnQueryType(searchList: columnDesignModel.searchList, string.Format("qt_{0}_qt_{1}", tableName, field)),
                            IsShow = CodeGenFieldJudgeHelper.IsShowColumn(columnDesignModel.columnList, string.Format("qt_{0}_qt_{1}", tableName, field)),
                            IsMultiple = CodeGenFieldJudgeHelper.IsMultipleColumn(controls, string.Format("qt_{0}_qt_{1}", tableName, field)),
                            qtKey = control.__config__.qtKey,
                            Rule = control.__config__.rule,
                            IsDateTime = CodeGenFieldJudgeHelper.IsDateTime(control),
                            ActiveTxt = control.activeTxt,
                            InactiveTxt = control.inactiveTxt,
                            IsAuxiliary = true,
                            TableNo = tableNo,
                            TableName = tableName
                        });
                    }
                    else
                    {
                        tableColumnList.Add(new TableColumnConfigModel()
                        {
                            ColumnName = field.ToUpperCase(),
                            OriginalColumnName = column.field,
                            ColumnComment = column.fieldName,
                            DataType = column.dataType,
                            NetType = CodeGenHelper.ConvertDataType(column.dataType),
                            PrimaryKey = false,
                            IsAuxiliary = true,
                            TableNo = tableNo,
                        });
                    }

                    break;
            }
        }

        if (!tableColumnList.Any(t => t.PrimaryKey))
        {
            throw Oops.Oh(ErrorCode.D2104);
        }

        // 是否存在上传
        bool isUpload = tableColumnList.Any(it => it.qtKey != null && (it.qtKey.Equals(QtKeyConst.UPLOADIMG) || it.qtKey.Equals(QtKeyConst.UPLOADFZ)));

        return new CodeGenConfigModel()
        {
            NameSpace = formDataModel.areasName,
            BusName = templateEntity.FullName,
            ClassName = formDataModel.className,
            PrimaryKey = tableColumnList.Find(t => t.PrimaryKey).ColumnName,
            OriginalPrimaryKey = tableColumnList.Find(t => t.PrimaryKey).OriginalColumnName,
            MainTable = mainTableName.ParseToPascalCase(),
            OriginalMainTableName = mainTableName,
            TableField = tableColumnList,
            IsUpload = isUpload,
            WebType = templateEntity.WebType,
            Type = templateEntity.Type,
            IsMainTable = false,
            IsAuxiliaryTable = true
        };
    }

    /// <summary>
    /// 前端.
    /// </summary>
    /// <param name="logic">生成逻辑;4-pc,5-app.</param>
    /// <param name="formDataModel">表单Json包.</param>
    /// <param name="controls">移除布局控件后的控件列表.</param>
    /// <param name="tableColumns">表字段.</param>
    /// <param name="templateEntity">模板实体.</param>
    /// <returns></returns>
    public static FrontEndGenConfigModel SingleTableFrontEnd(int logic, FormDataModel formDataModel, List<FieldsModel> controls, List<TableColumnConfigModel> tableColumns, VisualDevEntity templateEntity)
    {
        ColumnDesignModel columnDesignModel = new ColumnDesignModel();
        switch (logic)
        {
            case 4:
                columnDesignModel = templateEntity.ColumnData?.ToObject<ColumnDesignModel>();
                columnDesignModel ??= new ColumnDesignModel();
                break;
            case 5:
                ColumnDesignModel pcColumnDesignModel = templateEntity.ColumnData?.ToObject<ColumnDesignModel>();
                columnDesignModel = templateEntity.AppColumnData?.ToObject<ColumnDesignModel>();
                columnDesignModel ??= new ColumnDesignModel();

                // 移动端的分页遵循PC端
                columnDesignModel.hasPage = templateEntity.WebType == 1 ? false : pcColumnDesignModel.hasPage;
                break;
        }

        Dictionary<string, List<string>> listQueryControls = CodeGenQueryControlClassificationHelper.ListQueryControl(logic);
        Dictionary<string, List<string>> listColumnControlsType = CodeGenQueryControlClassificationHelper.ListColumnControls();

        // 表单脚本设计
        var formScriptDesign = CodeGenFormControlDesignHelper.FormScriptDesign("SingleTable", controls, tableColumns);

        // 整个表单控件
        var formControlList = CodeGenFormControlDesignHelper.FormControlDesign(formDataModel.fields, formDataModel.gutter, formDataModel.labelWidth, true);

        // 列表控件Option
        var indnxControlOption = CodeGenFormControlDesignHelper.FormControlProps(formDataModel.fields, logic, true);

        // 列表查询字段设计
        var indexSearchFieldDesign = new List<IndexSearchFieldDesignModel>();

        // 列表顶部按钮
        var indexTopButton = new List<IndexButtonDesign>();

        // 列表行按钮
        var indexColumnButtonDesign = new List<IndexButtonDesign>();

        // 列表页列表
        var indexColumnDesign = new List<IndexColumnDesign>();

        switch (templateEntity.Type)
        {
            case 3:

                break;
            default:
                switch (templateEntity.WebType)
                {
                    case 2:
                    case 3:
                        // 本身查询列表内带有控件全属性 单表不需要匹配表字段
                        foreach (var item in columnDesignModel?.searchList)
                        {
                            // 查询控件分类
                            var queryControls = listQueryControls.Where(q => q.Value.Contains(item.__config__.qtKey)).FirstOrDefault();

                            // 表单真实控件
                            var column = controls.Find(c => c.__vModel__ == item.__vModel__);

                            indexSearchFieldDesign.Add(new IndexSearchFieldDesignModel()
                            {
                                OriginalName = column.__vModel__,
                                Name = column.__vModel__,
                                LowerName = column.__vModel__,
                                Tag = column.__config__.tag,
                                Clearable = item.clearable ? "clearable " : string.Empty,
                                Format = column.format,
                                ValueFormat = column.valueformat,
                                Label = column.__config__.label,
                                QueryControlsKey = queryControls.Key != null ? queryControls.Key : null,
                                Props = column.__config__.props,
                                Index = columnDesignModel.searchList.IndexOf(item),
                                Type = column.type,
                                ShowAllLevels = column.showalllevels ? "true" : "false",
                                Level = column.level
                            });
                        }

                        // 生成头部按钮信息
                        foreach (var item in columnDesignModel?.btnsList)
                        {
                            indexTopButton.Add(new IndexButtonDesign()
                            {
                                Type = columnDesignModel.btnsList.IndexOf(item) == 0 ? "primary" : "text",
                                Icon = item.icon,
                                Method = GetCodeGenIndexButtonHelper.IndexTopButton(item.value),
                                Value = item.value,
                                Label = item.label
                            });
                        }

                        // 生成行按钮信息
                        foreach (var item in columnDesignModel.columnBtnsList)
                        {

                            indexColumnButtonDesign.Add(new IndexButtonDesign()
                            {
                                Type = item.value == "remove" ? "class='QT-table-delBtn' " : string.Empty,
                                Icon = item.icon,
                                Method = GetCodeGenIndexButtonHelper.IndexColumnButton(item.value, tableColumns.Find(it => it.PrimaryKey.Equals(true))?.LowerColumnName, templateEntity.Type == 3 || templateEntity.WebType == 3 ? true : false),
                                Value = item.value,
                                Label = item.label,
                                Disabled = GetCodeGenIndexButtonHelper.WorkflowIndexColumnButton(item.value),
                                IsDetail = item.value == "detail" ? true : false
                            });
                        }

                        // 生成列信息
                        foreach (var item in columnDesignModel.columnList)
                        {
                            // 控件对应的控件类型
                            var conversion = listColumnControlsType.Where(q => q.Value.Contains(item.qtKey)).FirstOrDefault();
                            indexColumnDesign.Add(new IndexColumnDesign()
                            {
                                Name = item.prop,
                                OptionsName = item.prop,
                                LowerName = item.prop,
                                qtKey = item.qtKey,
                                Label = item.label,
                                Width = item.width.ToString() == "0" ? string.Empty : string.Format("width='{0}' ", item.width),
                                Align = item.align,
                                IsAutomatic = conversion.Key == null ? false : true,
                                IsSort = item.sortable ? string.Format("sortable='custom' ") : string.Empty
                            });
                        }

                        break;
                }

                break;
        }

        var isBatchRemoveDel = indexTopButton.Any(it => it.Value == "batchRemove");
        var isDownload = indexTopButton.Any(it => it.Value == "download");
        var isRemoveDel = indexColumnButtonDesign.Any(it => it.Value == "remove");
        var isEdit = indexColumnButtonDesign.Any(it => it.Value == "edit");
        var isDetail = indexColumnButtonDesign.Any(it => it.IsDetail.Equals(true));
        var isSort = columnDesignModel?.columnList?.Any(it => it.sortable) ?? false;
        var isSummary = formScriptDesign.Any(it => it.qtKey.Equals("table") && it.ShowSummary.Equals(true));
        var isAdd = indexTopButton.Any(it => it.Value == "add");
        var isTreeRelation = !string.IsNullOrEmpty(columnDesignModel?.treeRelation);

        switch (templateEntity.WebType)
        {
            case 1:
                return new FrontEndGenConfigModel()
                {
                    NameSpace = formDataModel.areasName,
                    ClassName = formDataModel.className,
                    FormRef = formDataModel.formRef,
                    FormModel = formDataModel.formModel,
                    Size = formDataModel.size,
                    LabelPosition = formDataModel.labelPosition,
                    LabelWidth = formDataModel.labelWidth,
                    FormRules = formDataModel.formRules,
                    GeneralWidth = formDataModel.generalWidth,
                    FullScreenWidth = formDataModel.fullScreenWidth,
                    DrawerWidth = formDataModel.drawerWidth,
                    FormStyle = formDataModel.formStyle,
                    Type = columnDesignModel.type,
                    PrimaryKey = tableColumns?.Find(it => it.PrimaryKey.Equals(true))?.LowerColumnName,
                    FormList = formScriptDesign,
                    PopupType = formDataModel.popupType,
                    OptionsList = indnxControlOption,
                    IsRemoveDel = isRemoveDel,
                    IsDetail = isDetail,
                    IsEdit = isEdit,
                    IsAdd = isAdd,
                    IsSort = isSort,
                    HasPage = columnDesignModel.hasPage,
                    FormAllContols = formControlList,
                    CancelButtonText = formDataModel.cancelButtonText,
                    ConfirmButtonText = formDataModel.confirmButtonText,
                    UseBtnPermission = columnDesignModel.useBtnPermission,
                    UseColumnPermission = columnDesignModel.useColumnPermission,
                    UseFormPermission = columnDesignModel.useFormPermission,
                    IsSummary = isSummary,
                    PageSize = columnDesignModel.pageSize,
                    Sort = columnDesignModel.sort,
                    HasPrintBtn = formDataModel.hasPrintBtn,
                    PrintButtonText = formDataModel.printButtonText,
                    PrintId = formDataModel.printId,
                };
                break;
            default:
                return new FrontEndGenConfigModel()
                {
                    NameSpace = formDataModel.areasName,
                    ClassName = formDataModel.className,
                    FormRef = formDataModel.formRef,
                    FormModel = formDataModel.formModel,
                    Size = formDataModel.size,
                    LabelPosition = formDataModel.labelPosition,
                    LabelWidth = formDataModel.labelWidth,
                    FormRules = formDataModel.formRules,
                    GeneralWidth = formDataModel.generalWidth,
                    FullScreenWidth = formDataModel.fullScreenWidth,
                    DrawerWidth = formDataModel.drawerWidth,
                    FormStyle = formDataModel.formStyle,
                    Type = columnDesignModel.type,
                    TreeRelation = columnDesignModel?.treeRelation,
                    TreeTitle = columnDesignModel?.treeTitle,
                    TreePropsValue = columnDesignModel?.treePropsValue,
                    TreeDataSource = columnDesignModel?.treeDataSource,
                    TreeDictionary = columnDesignModel?.treeDictionary,
                    TreePropsUrl = columnDesignModel?.treePropsUrl,
                    TreePropsChildren = columnDesignModel?.treePropsChildren,
                    TreePropsLabel = columnDesignModel?.treePropsLabel,
                    IsExistQuery = templateEntity.Type == 3 ? false : (bool)columnDesignModel?.searchList?.Any(it => it.prop.Equals(columnDesignModel?.treeRelation)),
                    PrimaryKey = tableColumns?.Find(it => it.PrimaryKey.Equals(true))?.LowerColumnName,
                    FormList = formScriptDesign,
                    PopupType = formDataModel.popupType,
                    SearchColumnDesign = indexSearchFieldDesign,
                    TopButtonDesign = indexTopButton,
                    ColumnButtonDesign = indexColumnButtonDesign,
                    ColumnDesign = indexColumnDesign,
                    OptionsList = indnxControlOption,
                    IsBatchRemoveDel = isBatchRemoveDel,
                    IsDownload = isDownload,
                    IsRemoveDel = isRemoveDel,
                    IsDetail = isDetail,
                    IsEdit = isEdit,
                    IsAdd = isAdd,
                    IsSort = isSort,
                    HasPage = columnDesignModel.hasPage,
                    FormAllContols = formControlList,
                    CancelButtonText = formDataModel.cancelButtonText,
                    ConfirmButtonText = formDataModel.confirmButtonText,
                    UseBtnPermission = columnDesignModel.useBtnPermission,
                    UseColumnPermission = columnDesignModel.useColumnPermission,
                    UseFormPermission = columnDesignModel.useFormPermission,
                    IsSummary = isSummary,
                    PageSize = columnDesignModel.pageSize,
                    Sort = columnDesignModel.sort,
                    HasPrintBtn = formDataModel.hasPrintBtn,
                    PrintButtonText = formDataModel.printButtonText,
                    PrintId = formDataModel.printId,
                };
                break;
        }
    }
}