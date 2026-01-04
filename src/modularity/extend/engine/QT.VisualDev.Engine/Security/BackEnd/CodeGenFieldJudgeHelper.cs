using QT.Common.Const;

namespace QT.VisualDev.Engine.Security;

/// <summary>
/// 代码生成表字段判断帮助类.
/// </summary>
public class CodeGenFieldJudgeHelper
{
    /// <summary>
    /// 是否查询列.
    /// </summary>
    /// <param name="searchList">模板内查询列表.</param>
    /// <param name="fieldName">字段名称.</param>
    /// <returns></returns>
    public static bool IsColumnQueryWhether(List<IndexSearchFieldModel>? searchList, string fieldName)
    {
        var column = searchList?.Any(s => s.prop == fieldName);
        return column ?? false;
    }

    /// <summary>
    /// 列查询类型.
    /// </summary>
    /// <param name="searchList">模板内查询列表.</param>
    /// <param name="fieldName">字段名称.</param>
    /// <returns></returns>
    public static int ColumnQueryType(List<IndexSearchFieldModel>? searchList, string fieldName)
    {
        var column = searchList?.Find(s => s.prop == fieldName);
        return column?.searchType ?? 0;
    }

    /// <summary>
    /// 是否展示列.
    /// </summary>
    /// <param name="columnList">模板内展示字段.</param>
    /// <param name="fieldName">字段名称.</param>
    /// <returns></returns>
    public static bool IsShowColumn(List<IndexGridFieldModel>? columnList, string fieldName)
    {
        bool? column = columnList?.Any(s => s.prop == fieldName);
        return column ?? false;
    }

    /// <summary>
    /// 获取是否多选.
    /// </summary>
    /// <param name="columnList">模板内控件列表.</param>
    /// <param name="fieldName">字段名称.</param>
    /// <returns></returns>
    public static bool IsMultipleColumn(List<FieldsModel> columnList, string fieldName)
    {
        bool isMultiple = false;
        var column = columnList.Find(s => s.__vModel__ == fieldName);
        if (column != null)
        {
            switch (column?.__config__.qtKey)
            {
                case QtKeyConst.CASCADER:
                    isMultiple = column.props.props.multiple;
                    break;
                default:
                    isMultiple = column.multiple;
                    break;
            }
        }

        return isMultiple;
    }

    /// <summary>
    /// 是否datetime.
    /// </summary>
    /// <param name="fields"></param>
    /// <returns></returns>
    public static bool IsDateTime(FieldsModel? fields)
    {
        bool isDateTime = false;
        if (fields?.__config__.qtKey == QtKeyConst.DATE && fields?.type == QtKeyConst.DATETIME)
            isDateTime = true;
        return isDateTime;
    }
}
