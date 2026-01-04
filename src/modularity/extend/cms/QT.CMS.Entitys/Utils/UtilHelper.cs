using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace QT.CMS;

/// <summary>
/// 扩展字段列表转换
/// </summary>
public class UtilHelper
{
    /// <summary>
    /// 获得多选,单选的选择项值
    /// </summary>
    /// <returns></returns>
    public static object GetCheckboxOrRadioOptions(string? controlType, string? itemOption)
    {
        Dictionary<string, string> dic = new Dictionary<string, string>();
        List<object> list = new List<object>();
        if (controlType == "checkbox" || controlType == "radio")
        {
            if (!string.IsNullOrWhiteSpace(itemOption))
            {
                //按照换行分割
                var options = itemOption.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                for (int i = 0; i < options.Length; i++)
                {
                    //按照竖线分割
                    var op = options[i].Split('|');
                    //检查值是否存在
                    if (!dic.ContainsKey(op[0]))
                    {
                        if (op.Length == 2)
                        {
                            dic.Add(op[0], op[1]);
                            list.Add(op);
                        }
                    }

                }
            }
        }
        return list;
    }

    /// <summary>
    /// 将多选的默认值转换成数组
    /// </summary>
    /// <param name="controlType">控件类型</param>
    /// <param name="defaultValue">用逗号分开的值</param>
    /// <returns></returns>
    public static object GetCheckboxDefaultValue(string? controlType, string? defaultValue)
    {
        //如果是多选
        if (controlType == "checkbox")
        {
            if (!string.IsNullOrWhiteSpace(defaultValue))
            {
                defaultValue = defaultValue.Replace('，', ',').Trim().Trim(',');
                return defaultValue.Split(',');
            }
        }
        return defaultValue;
    }
}
