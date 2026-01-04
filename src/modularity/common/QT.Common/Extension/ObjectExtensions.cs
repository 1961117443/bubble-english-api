using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using System.Text;

namespace QT.Common.Extension;

/// <summary>
/// Object扩展(用于数据塑形)
/// </summary>
public static class ObjectExtensions
{
    public static ExpandoObject ShapeData<TSource>(this TSource source, string? fields)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        var dataShapedObject = new ExpandoObject();
        if (string.IsNullOrWhiteSpace(fields))
        {
            // all public properties should be in the ExpandoObject 
            var propertyInfos = typeof(TSource).GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            foreach (var propertyInfo in propertyInfos)
            {
                // get the value of the property on the source object
                var propertyValue = propertyInfo.GetValue(source);

                // add the field to the ExpandoObject
                ((IDictionary<string, object?>)dataShapedObject)
                    .Add(propertyInfo.Name, propertyValue);
            }

            return dataShapedObject;
        }

        // the field are separated by ",", so we split it.
        var fieldsAfterSplit = fields.Split(',');

        foreach (var field in fieldsAfterSplit)
        {
            // trim each field, as it might contain leading 
            // or trailing spaces. Can't trim the var in foreach,
            // so use another var.
            var propertyName = field.Trim();

            // use reflection to get the property on the source object
            // we need to include public and instance, b/c specifying a 
            // binding flag overwrites the already-existing binding flags.
            var propertyInfo = typeof(TSource)
                .GetProperty(propertyName,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (propertyInfo == null)
            {
                throw new Exception($"Property {propertyName} wasn't found " +
                    $"on {typeof(TSource)}");
            }

            // get the value of the property on the source object
            var propertyValue = propertyInfo.GetValue(source);

            // add the field to the ExpandoObject
            ((IDictionary<string, object?>)dataShapedObject)
                .Add(propertyInfo.Name, propertyValue);
        }

        // return the list
        return dataShapedObject;
    }

    public static IEnumerable<ExpandoObject> ShapeDatas<TSource>(this IEnumerable<TSource> source, string? fields)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        var expandoObjectList = new List<ExpandoObject>();

        //避免在列表中遍历数据，创建一个属性信息列表
        var propertyInfoList = new List<PropertyInfo>();

        if (string.IsNullOrWhiteSpace(fields))
        {
            // 希望返回动态类型对象ExpandoObject所有的属性
            var propertyInfos = typeof(TSource)
                .GetProperties(BindingFlags.IgnoreCase
                | BindingFlags.Public | BindingFlags.Instance);

            propertyInfoList.AddRange(propertyInfos);
        }
        else
        {
            //逗号来分隔字段字符串
            var fieldsAfterSplit = fields.Split(',');

            foreach (var filed in fieldsAfterSplit)
            {
                // 去掉首尾多余的空格，获得属性名称
                var propertyName = filed.Trim();

                var propertyInfo = typeof(TSource)
                    .GetProperty(propertyName, BindingFlags.IgnoreCase
                | BindingFlags.Public | BindingFlags.Instance);

                if (propertyInfo == null)
                {
                    throw new Exception($"属性 {propertyName} 找不到" +
                        $" {typeof(TSource)}");
                }

                propertyInfoList.Add(propertyInfo);
            }
        }

        foreach (TSource sourceObject in source)
        {
            // 创建动态类型对象, 创建数据塑性对象
            var dataShapedObject = new ExpandoObject();

            foreach (var propertyInfo in propertyInfoList)
            {
                //获得对应属性的真实数据
                var propertyValue = propertyInfo.GetValue(sourceObject);

                ((IDictionary<string, object?>)dataShapedObject)
                    .Add(propertyInfo.Name, propertyValue);
            }

            expandoObjectList.Add(dataShapedObject);
        }

        return expandoObjectList;
    }
}
