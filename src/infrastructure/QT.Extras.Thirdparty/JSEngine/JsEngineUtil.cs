using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JavaScriptEngineSwitcher.ChakraCore;
using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.V8;
using Newtonsoft.Json.Linq;

namespace QT.Extras.Thirdparty.JSEngine;

/// <summary>
/// js处理引擎.
/// </summary>
public class JsEngineUtil
{
    /// <summary>
    /// 执行Js(返回结果请用result).
    /// 如：var result = function(a,b){}.
    /// </summary>
    /// <param name="jsContent">js内容.</param>
    /// <param name="args">参数.</param>
    /// <returns></returns>
    public static object CallFunction(string jsContent,params object[] args)
    {
        try
        {
            V8JsEngine engine = new V8JsEngine();
            engine.Execute(jsContent);
            return engine.CallFunction("result", args);
        }
        catch (Exception e)
        {
            throw new Exception("不支持的JS数据");
        }
    }
}
