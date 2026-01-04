using QT.Common.Const;

namespace QT.VisualDev.Engine;

/// <summary>
/// 模板解析.
/// </summary>
public static class TemplateAnalysis
{
    /// <summary>
    /// 解析模板数据
    /// 移除模板内的布局类型控件.
    /// </summary>
    public static List<FieldsModel> AnalysisTemplateData(List<FieldsModel> fieldsModelList)
    {
        var template = new List<FieldsModel>();

        // 将模板内的无限children解析出来
        // 不包含子表children
        foreach (var item in fieldsModelList)
        {
            var config = item.__config__;
            switch (config.qtKey)
            {
                case QtKeyConst.TABLE:
                    template.Add(item);
                    break;
                case QtKeyConst.ROW:
                case QtKeyConst.CARD:
                    template.AddRange(AnalysisTemplateData(config.children));
                    break;
                case QtKeyConst.COLLAPSE:
                case QtKeyConst.TAB:
                    foreach (FieldsModel? collapse in config.children)
                    {
                        template.AddRange(AnalysisTemplateData(collapse.__config__.children));
                    }

                    break;
                case QtKeyConst.QTTEXT:
                case QtKeyConst.DIVIDER:
                case QtKeyConst.GROUPTITLE:
                case QtKeyConst.BUTTON:
                    break;
                default:
                    template.Add(item);
                    break;
            }
        }

        return template;
    }
}