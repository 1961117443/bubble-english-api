using System.Text;

namespace QT.VisualDev.Engine.Security;

/// <summary>
/// 代码生成导出字段帮助类.
/// </summary>
public class CodeGenExportFieldHelper
{
    /// <summary>
    /// 获取主表字段名.
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public static string ExportColumnField(List<IndexGridFieldModel>? list)
    {
        StringBuilder columnSb = new StringBuilder();
        foreach (var item in list)
        {
            columnSb.AppendFormat("{{\\\"value\\\":\\\"{0}\\\",\\\"field\\\":\\\"{1}\\\"}},", item.label, item.prop);
        }

        return columnSb.ToString();
    }
}
