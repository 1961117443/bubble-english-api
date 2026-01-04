using QT.Extensions;
using QT.UnifyResult;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Mvc;

/// <summary>
/// 规范化结果配置
/// </summary>
[SuppressSniffer, AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class UnifyResultAttribute : ProducesResponseTypeAttribute
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="statusCode"></param>
    public UnifyResultAttribute(int statusCode) : base(statusCode)
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="type"></param>
    public UnifyResultAttribute(Type type) : base(type, StatusCodes.Status200OK)
    {
        WrapType(type);
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="type"></param>
    /// <param name="statusCode"></param>
    public UnifyResultAttribute(Type type, int statusCode) : base(type, statusCode)
    {
        WrapType(type);
    }

    /// <summary>
    /// 包装类型
    /// </summary>
    /// <param name="type"></param>
    private void WrapType(Type type)
    {
        if (type != null)
        {
            if (!type.HasImplementedRawGeneric(UnifyContext.RESTfulResultType))
            {
                Type = UnifyContext.RESTfulResultType.MakeGenericType(type);
            }
            else Type = default;
        }
    }
}