using NPOI.SS.Formula.Functions;
using QT.Common.Models.NPOI;
using QT.FriendlyException;
using QT.Reflection.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QT.Common.Extension;

/// <summary>
/// 类型扩展类
/// </summary>
public partial class Extensions
{

    /// <summary>
    /// 把类型的公共属性转为字典
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="all">是否全部属性，否（只获取包含描述 DescriptionAttribute | DisplayAttribute的字段）</param>
    /// <returns></returns>
    public static Dictionary<string, string> GetPropertityToMaps<T>(bool all = false) where T : class,new()
    {
        Dictionary<string, (string,int)> result = new Dictionary<string, (string,int)>();        
        foreach (var prop in EntityHelper<T>.InstanceProperties)
        {
            if (!all && !prop.HasAttribute<DescriptionAttribute>() && !prop.HasAttribute<DisplayAttribute>())
            {
                continue;
            }

            int order = 0;
            if (prop.HasAttribute<DisplayAttribute>())
            {
                var attr = prop.GetCustomAttribute<DisplayAttribute>();
                if (attr!=null && attr.GetOrder().HasValue)
                {
                    order = attr.GetOrder().Value;
                }
            }

            result.Add(prop.Name, (prop.GetDescription(), order));
        }

        var dict = result.OrderBy(x => x.Value.Item2)
              .ToDictionary(x => x.Key, x => x.Value.Item1);

        // 判断值是否重复
        if (dict.Values.Distinct().Count() != dict.Values.Count())
        {
            throw Oops.Oh($"字典值重复，请检查类型[{typeof(T).Name}]的描述");
        }

        return dict;
    }

    /// <summary>
    /// 把类型的公共属性转为字典
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="all">是否全部属性，否（只获取包含描述 DescriptionAttribute | DisplayAttribute的字段）</param>
    /// <returns></returns>
    public static Dictionary<string, (string,PropertyInfo)> GetPropertityToMapsWithProperty<T>(bool all = false) where T : class, new()
    {
        Dictionary<string, (string, int,PropertyInfo)> result = new Dictionary<string, (string, int, PropertyInfo)>();
        foreach (var prop in EntityHelper<T>.InstanceProperties)
        {
            if (!all && !prop.HasAttribute<DescriptionAttribute>() && !prop.HasAttribute<DisplayAttribute>())
            {
                continue;
            }

            int order = 0;
            if (prop.HasAttribute<DisplayAttribute>())
            {
                var attr = prop.GetCustomAttribute<DisplayAttribute>();
                if (attr != null && attr.GetOrder().HasValue)
                {
                    order = attr.GetOrder().Value;
                }
            }

            result.Add(prop.Name, (prop.GetDescription(), order,prop));
        }

        var dict = result.OrderBy(x => x.Value.Item2)
              .ToDictionary(x => x.Key, x => (x.Value.Item1,x.Value.Item3));

        // 判断值是否重复
        if (dict.Values.Distinct().Count() != dict.Values.Count())
        {
            throw Oops.Oh($"字典值重复，请检查类型[{typeof(T).Name}]的描述");
        }

        return dict;
    }


    /// <summary>
    /// 把类型的公共属性转为Excel列
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="all">是否全部属性，否（只获取包含描述 DescriptionAttribute | DisplayAttribute的字段）</param>
    /// <returns></returns>
    public static List<ExcelColumnModel> GetExcelColumnModels<T>(bool all = false) where T : class, new()
    {
        var ColumnModel = new List<ExcelColumnModel>();

        foreach (KeyValuePair<string, string> item in GetPropertityToMaps<T>())
        {
            string? column = item.Key;
            string? excelColumn = item.Value;
            ColumnModel.Add(new ExcelColumnModel() { Column = column, ExcelColumn = excelColumn });
        }
        return ColumnModel;
    }


    #region 类型判断
    /// <summary>
    /// 判断类型是否为数值类型
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsNumericType(this Type type)
    {
        if (type == typeof(byte) ||
            type == typeof(sbyte) ||
            type == typeof(short) ||
            type == typeof(ushort) ||
            type == typeof(int) ||
            type == typeof(uint) ||
            type == typeof(long) ||
            type == typeof(ulong) ||
            type == typeof(float) ||
            type == typeof(double) ||
            type == typeof(decimal))
        {
            return true;
        }

        return false;
    } 
    #endregion
}
