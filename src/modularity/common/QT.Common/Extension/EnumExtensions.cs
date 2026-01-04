using QT.Common.Enum;
using QT.DependencyInjection;
using QT.FriendlyException;
using QT.Reflection.Extensions;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;

namespace QT.Common.Extension;

/// <summary>
/// 枚举<see cref="Enum"/>的扩展辅助操作方法.
/// </summary>
[SuppressSniffer]
public static class EnumExtensions
{
    // 枚举显示字典缓存
    private static readonly ConcurrentDictionary<Type, Dictionary<int, string>> EnumDisplayValueDict = new();

    /// <summary>
    /// 获取枚举项上的<see cref="DescriptionAttribute"/>特性的文字描述.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string ToDescription(this System.Enum value)
    {
        Type type = value.GetType();
        MemberInfo member = type.GetMember(value.ToString()).FirstOrDefault();
        return member != null ? member.GetDescription() : value.ToString();
    }

    /// <summary>
    /// 获取枚举类型key与描述的字典（缓存）.
    /// </summary>
    /// <param name="enumType"></param>
    /// <returns></returns>
    public static Dictionary<int, string> GetEnumDescDictionary(Type enumType)
    {
        if (!enumType.IsEnum)
            throw Oops.Oh(ErrorCode.D1503);

        // 查询缓存
        Dictionary<int, string> enumDic = EnumDisplayValueDict.ContainsKey(enumType) ? EnumDisplayValueDict[enumType] : new Dictionary<int, string>();
        if (enumDic.Count == 0)
        {
            // 取枚举类型的Key/Value字典集合
            enumDic = GetEnumDescDictionaryItems(enumType);

            // 缓存
            EnumDisplayValueDict[enumType] = enumDic;
        }

        return enumDic;
    }

    /// <summary>
    /// 获取枚举类型key与描述的字典（没有描述则获取name）.
    /// </summary>
    /// <param name="enumType"></param>
    /// <returns></returns>
    private static Dictionary<int, string> GetEnumDescDictionaryItems(Type enumType)
    {
        // 获取类型的字段，初始化一个有限长度的字典
        FieldInfo[] enumFields = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);
        Dictionary<int, string> enumDic = new(enumFields.Length);

        // 遍历字段数组获取key和name
        foreach (FieldInfo enumField in enumFields)
        {
            int intValue = (int)enumField.GetValue(enumType);
            var desc = enumField.GetDescriptionValue<DescriptionAttribute>();
            enumDic[intValue] = desc != null && !string.IsNullOrEmpty(desc.Description) ? desc.Description : enumField.Name;
        }

        return enumDic;
    }

    /// <summary>
    /// 获取字段特性.
    /// </summary>
    /// <param name="field"></param>
    /// <typeparam name="T">.</typeparam>
    /// <returns></returns>
    public static T GetDescriptionValue<T>(this FieldInfo field) where T : Attribute
    {
        // 获取字段的指定特性，不包含继承中的特性
        object[] customAttributes = field.GetCustomAttributes(typeof(T), false);

        // 如果没有数据返回null
        return customAttributes.Length > 0 ? (T)customAttributes[0] : null;
    }
}