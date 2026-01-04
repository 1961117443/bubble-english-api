using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QT.Common;

/// <summary>
/// 实体帮助类
/// </summary>
/// <typeparam name="T"></typeparam>
public static class EntityHelper<T> where T : class,new()
{
    #region 静态属性变量
    private static PropertyInfo[] _totalPropertyInfo;

    /// <summary>
    /// 公开实例属性
    /// </summary>
    private static PropertyInfo[] _instancePropertyInfo;
    /// <summary>
    /// 实体全部属性
    /// </summary>
    public static IReadOnlyCollection<PropertyInfo> PropertyInfo => _totalPropertyInfo;


    /// <summary>
    /// 实体全部属性
    /// </summary>
    public static IReadOnlyCollection<PropertyInfo> InstanceProperties => _instancePropertyInfo;


    private static List<PropertyInfo[]> _uniquePropertyCollection;
    #endregion


    #region 静态构造函数
    /// <summary>
    /// 静态构造函数，初始化类型 T
    /// </summary>
    static EntityHelper()
    {
        _totalPropertyInfo = typeof(T).GetProperties();

        _instancePropertyInfo = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        //_propertyInfoWithoutChildren = _totalPropertyInfo.Where(x => x.PropertyType.Name != "List`1").ToArray();

        //_keyPropertyInfo = _totalPropertyInfo.Where(x => x.GetCustomAttributes<KeyAttribute>(inherit: false).Any()).ToArray();

        //foreach (var p in _totalPropertyInfo)
        //{
        //    var attr = p.GetCustomAttribute<ForeignKeyAttribute>();
        //    if (attr != null)
        //    {
        //        _foreignKeyName = attr.Name;
        //        break;
        //    }
        //}

        //var attribute = typeof(T).GetCustomAttribute<EntityAttribute>();
        //_entityTableName = attribute != null && !string.IsNullOrEmpty(attribute.TableName) ? attribute.TableName : typeof(T).Name;
        //_entityTableCnName = attribute != null && !string.IsNullOrEmpty(attribute.TableCnName) ? attribute.TableCnName : string.Empty;

        #region [UniqueProperties]
        var uniqueAttribute = typeof(T).GetCustomAttribute<EntityUniquePropertyAttribute>();
        _uniquePropertyCollection = new List<PropertyInfo[]>();
        if (uniqueAttribute != null && uniqueAttribute.UniqueProperties != null && uniqueAttribute.UniqueProperties.Any())
        {
            foreach (var item in uniqueAttribute.UniqueProperties)
            {
                var props = item.Split(",", StringSplitOptions.RemoveEmptyEntries)
                     .Select(x => _totalPropertyInfo.FirstOrDefault(p => p.Name == x))
                     .ToArray();

                if (props.Any())
                {
                    _uniquePropertyCollection.Add(props);
                }
            }
        }
        #endregion

        //if (typeof(ITenant).IsAssignableFrom(typeof(T)))
        //{
        //    _tenantProperty = _totalPropertyInfo.FirstOrDefault(x => x.Name == nameof(ITenant.TenantId));
        //}
    }
    #endregion

    /// <summary>
    /// 需要检查唯一性的属性集合
    /// </summary>
    public static ICollection<PropertyInfo[]> UniquePropertyCollection => _uniquePropertyCollection;
}
