using QT.VisualDev.Engine.Model.CodeGen;

namespace QT.VisualDev.Engine.Security;

/// <summary>
/// 代码生成方法帮助类.
/// </summary>
public class CodeGenFunctionHelper
{
    /// <summary>
    /// 生成方法列表.
    /// </summary>
    /// <param name="hasPage">是否分页.</param>
    /// <param name="btnsList">头部按钮.</param>
    /// <param name="columnBtnsList">列表按钮.</param>
    /// <param name="webType">页面类型（1、纯表单，2、表单加列表，3、表单列表工作流）</param>
    /// <returns></returns>
    public static List<CodeGenFunctionModel> GenerateFunctionList(bool hasPage, List<ButtonConfigModel> btnsList, List<ButtonConfigModel> columnBtnsList, int webType)
    {
        List<CodeGenFunctionModel> functionList = new List<CodeGenFunctionModel>();
        switch (webType)
        {
            case 1:
                // 纯表单只存在ADD方法.
                functionList.Add(new CodeGenFunctionModel()
                {
                    FullName = "add",
                    IsInterface = true
                });
                break;
            default:

                // 默认注入获取信息方法
                functionList.Add(new CodeGenFunctionModel()
                {
                    FullName = "info",
                    IsInterface = true
                });

                // 根据是否分页注入默认列表方法.
                if (!hasPage)
                {
                    functionList.Add(new CodeGenFunctionModel()
                    {
                        FullName = "noPage",
                        IsInterface = true
                    });
                }
                else
                {
                    functionList.Add(new CodeGenFunctionModel()
                    {
                        FullName = "page",
                        IsInterface = true
                    });
                }

                btnsList.ForEach(b =>
                {
                    if (b.value == "download" && !hasPage)
                    {
                        functionList.Add(new CodeGenFunctionModel()
                        {
                            FullName = "page",
                            IsInterface = false
                        });
                    }
                    else if (b.value == "download" && hasPage)
                    {
                        functionList.Add(new CodeGenFunctionModel()
                        {
                            FullName = "noPage",
                            IsInterface = false
                        });
                    }

                    functionList.Add(new CodeGenFunctionModel()
                    {
                        FullName = b.value,
                        IsInterface = true
                    });
                });
                columnBtnsList.ForEach(c =>
                {
                    functionList.Add(new CodeGenFunctionModel()
                    {
                        FullName = c.value,
                        IsInterface = true
                    });
                });
                break;
        }

        return functionList;
    }

    /// <summary>
    /// 获取代码生成流程方法列表.
    /// </summary>
    /// <returns></returns>
    public static List<CodeGenFunctionModel> GetCodeGenFlowFunctionList()
    {
        List<CodeGenFunctionModel> functionList = new List<CodeGenFunctionModel>();

        // 信息
        functionList.Add(new CodeGenFunctionModel()
        {
            FullName = "info",
            IsInterface = true
        });
        functionList.Add(new CodeGenFunctionModel()
        {
            FullName = "add",
            IsInterface = true
        });
        functionList.Add(new CodeGenFunctionModel()
        {
            FullName = "edit",
            IsInterface = true
        });
        functionList.Add(new CodeGenFunctionModel()
        {
            FullName = "remove",
            IsInterface = true
        });
        return functionList;
    }
}