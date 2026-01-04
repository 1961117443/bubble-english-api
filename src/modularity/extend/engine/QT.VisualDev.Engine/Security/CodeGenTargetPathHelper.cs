using QT.Common.Configuration;

namespace QT.VisualDev.Engine.Security;

/// <summary>
/// 代码生成目标路径帮助类.
/// </summary>
public class CodeGenTargetPathHelper
{
    #region 前端相关文件

    /// <summary>
    /// 前端页面生成文件路径.
    /// </summary>
    /// <param name="tableName">主表名称.</param>
    /// <param name="fileName">压缩包名称.</param>
    /// <param name="webType">页面类型（1、纯表单，2、表单加列表，3、表单列表工作流）.</param>
    /// <param name="isDetail">是否有详情.</param>
    /// <returns></returns>
    public static List<string> FrontEndTargetPathList(string tableName, string fileName, int webType, bool isDetail = false)
    {
        var frontendPath = Path.Combine(KeyVariable.SystemPath, "CodeGenerate", fileName, "NetCode");
        var indexPath = Path.Combine(frontendPath, "html", "PC", tableName, "index.vue");
        var formPath = Path.Combine(frontendPath, "html", "PC", tableName, "Form.vue");
        var detailPath = Path.Combine(frontendPath, "html", "PC", tableName, "Detail.vue");
        var exportBoxPath = Path.Combine(frontendPath, "html", "PC", tableName, "ExportBox.vue");
        var exportJsonPath = Path.Combine(frontendPath, "ffe", "ExportJson.ffe");
        var pathList = new List<string>();
        switch (webType)
        {
            case 1:
                pathList.Add(indexPath);
                break;
            case 2:
                pathList.Add(indexPath);
                pathList.Add(formPath);
                if (isDetail)
                    pathList.Add(detailPath);
                pathList.Add(exportBoxPath);
                break;
            case 3:
                pathList.Add(indexPath);
                pathList.Add(formPath);
                if (isDetail)
                    pathList.Add(detailPath);
                pathList.Add(exportBoxPath);
                pathList.Add(exportJsonPath);
                break;
        }

        return pathList;
    }

    /// <summary>
    /// 前端页面模板文件路径集合.
    /// </summary>
    /// <param name="webType">页面类型（1、纯表单，2、表单加列表，3、表单列表工作流）.</param>
    /// <param name="isDetail">是否有详情.</param>
    /// <returns>返回前端模板地址列表.</returns>
    public static List<string> FrontEndTemplatePathList(int webType, bool isDetail = false)
    {
        var templatePath = Path.Combine(App.WebHostEnvironment.WebRootPath, "Template");
        var pathList = new List<string>();
        switch (webType)
        {
            case 1:
                pathList.Add(Path.Combine(templatePath, "Form.vue.vm"));
                break;
            case 2:
                pathList.Add(Path.Combine(templatePath, "index.vue.vm"));
                pathList.Add(Path.Combine(templatePath, "Form.vue.vm"));
                if (isDetail)
                    pathList.Add(Path.Combine(templatePath, "Detail.vue.vm"));
                pathList.Add(Path.Combine(templatePath, "ExportBox.vue.vm"));
                break;
            case 3:
                pathList.Add(Path.Combine(templatePath, "WorkflowIndex.vue.vm"));
                pathList.Add(Path.Combine(templatePath, "WorkflowForm.vue.vm"));
                if (isDetail)
                    pathList.Add(Path.Combine(templatePath, "Detail.vue.vm"));
                pathList.Add(Path.Combine(templatePath, "ExportBox.vue.vm"));
                pathList.Add(Path.Combine(templatePath, "ExportJson.json.vm"));
                break;
        }

        return pathList;
    }

    /// <summary>
    /// App前端页面模板文件路径集合.
    /// </summary>
    /// <param name="webType">页面类型（1、纯表单，2、表单加列表，3、表单列表工作流）.</param>
    /// <returns></returns>
    public static List<string> AppFrontEndTemplatePathList(int webType)
    {
        var templatePath = Path.Combine(App.WebHostEnvironment.WebRootPath, "Template");
        var pathList = new List<string>();
        switch (webType)
        {
            case 1:
                pathList.Add(Path.Combine(templatePath, "appForm.vue.vm"));
                break;
            case 2:
                pathList.Add(Path.Combine(templatePath, "appIndex.vue.vm"));
                pathList.Add(Path.Combine(templatePath, "appForm.vue.vm"));
                break;
            case 3:
                pathList.Add(Path.Combine(templatePath, "appWorkflowIndex.vue.vm"));
                pathList.Add(Path.Combine(templatePath, "appWorkflowForm.vue.vm"));
                break;
        }

        return pathList;
    }

    /// <summary>
    /// 设置App前端页面生成文件路径.
    /// </summary>
    /// <param name="tableName">主表名称.</param>
    /// <param name="fileName">压缩包名称.</param>
    /// <param name="webType">页面类型（1、纯表单，2、表单加列表，3、表单列表工作流）.</param>
    /// <returns></returns>
    public static List<string> AppFrontEndTargetPathList(string tableName, string fileName, int webType)
    {
        var frontendPath = Path.Combine(KeyVariable.SystemPath, "CodeGenerate", fileName, "NetCode");
        var indexPath = Path.Combine(frontendPath, "html", "App", tableName, "index.vue");
        var formPath = Path.Combine(frontendPath, "html", "App", tableName, "form.vue");
        var pathList = new List<string>();
        switch (webType)
        {
            case 1:
                pathList.Add(indexPath);
                break;
            case 2:
                pathList.Add(indexPath);
                pathList.Add(formPath);
                break;
            case 3:
                pathList.Add(Path.Combine(frontendPath, "html", "app", "index", "index.vue"));
                pathList.Add(Path.Combine(frontendPath, "html", "app", "form", "index.vue"));
                break;
        }

        return pathList;
    }

    /// <summary>
    /// 流程前端页面模板文件路径集合.
    /// </summary>
    /// <returns></returns>
    public static List<string> FlowFrontEndTemplatePathList()
    {
        var templatePath = Path.Combine(App.WebHostEnvironment.WebRootPath, "Template");
        return new List<string>()
            {
                Path.Combine(templatePath, "WorkflowForm.vue.vm"),
                Path.Combine(templatePath, "appWorkflowForm.vue.vm")
            };
    }

    /// <summary>
    /// 流程前端页面生成文件路径.
    /// </summary>
    /// <param name="tableName">主表名称.</param>
    /// <param name="fileName">压缩包名称.</param>
    /// <returns></returns>
    public static List<string> FlowFrontEndTargetPathList(string tableName, string fileName)
    {
        var frontendPath = Path.Combine(KeyVariable.SystemPath, "CodeGenerate", fileName, "NetCode");
        var indexPath = Path.Combine(frontendPath, "html", "PC", tableName, "index.vue");
        var indexAppPath = Path.Combine(frontendPath, "html", "APP", tableName, "index.vue");
        return new List<string>()
            {
                indexPath,
                indexAppPath
            };
    }

    #endregion

    #region 单主表相关文件

    /// <summary>
    /// 后端模板文件路径集合.
    /// </summary>
    /// <param name="genModel">SingleTable-单主表,MainBelt-主带子,,,.</param>
    /// <param name="webType">生成模板类型（1、纯表单，2、表单加列表，3、表单列表工作流）.</param>
    /// <returns></returns>
    public static List<string> BackendTemplatePathList(string genModel, int webType)
    {
        List<string> templatePathList = new List<string>();
        var templatePath = Path.Combine(App.WebHostEnvironment.WebRootPath, "Template");
        switch (webType)
        {
            case 1:
                templatePathList.Add(Path.Combine(templatePath, genModel, "Service.cs.vm"));
                templatePathList.Add(Path.Combine(templatePath, "IService.cs.vm"));
                templatePathList.Add(Path.Combine(templatePath, genModel, "Entity.cs.vm"));
                templatePathList.Add(Path.Combine(templatePath, genModel, "Mapper.cs.vm"));
                templatePathList.Add(Path.Combine(templatePath, genModel, "CrInput.cs.vm"));
                break;
            case 2:
                templatePathList.Add(Path.Combine(templatePath, genModel, "Service.cs.vm"));
                templatePathList.Add(Path.Combine(templatePath, "IService.cs.vm"));
                templatePathList.Add(Path.Combine(templatePath, genModel, "Entity.cs.vm"));
                templatePathList.Add(Path.Combine(templatePath, genModel, "Mapper.cs.vm"));
                templatePathList.Add(Path.Combine(templatePath, genModel, "CrInput.cs.vm"));
                templatePathList.Add(Path.Combine(templatePath, "UpInput.cs.vm"));
                templatePathList.Add(Path.Combine(templatePath, genModel, "ListQueryInput.cs.vm"));
                templatePathList.Add(Path.Combine(templatePath, genModel, "InfoOutput.cs.vm"));
                templatePathList.Add(Path.Combine(templatePath, genModel, "ListOutput.cs.vm"));
                break;
            case 3:
                templatePathList.Add(Path.Combine(templatePath, genModel, "Workflow", "Service.cs.vm"));
                templatePathList.Add(Path.Combine(templatePath, "IService.cs.vm"));
                templatePathList.Add(Path.Combine(templatePath, genModel, "Entity.cs.vm"));
                templatePathList.Add(Path.Combine(templatePath, genModel, "Mapper.cs.vm"));
                templatePathList.Add(Path.Combine(templatePath, genModel, "CrInput.cs.vm"));
                templatePathList.Add(Path.Combine(templatePath, "UpInput.cs.vm"));
                templatePathList.Add(Path.Combine(templatePath, genModel, "ListQueryInput.cs.vm"));
                templatePathList.Add(Path.Combine(templatePath, genModel, "InfoOutput.cs.vm"));
                templatePathList.Add(Path.Combine(templatePath, genModel, "ListOutput.cs.vm"));
                break;
        }

        return templatePathList;
    }

    /// <summary>
    /// 后端主表生成文件路径.
    /// </summary>
    /// <param name="tableName">表名.</param>
    /// <param name="fileName">文件价名称.</param>
    /// <param name="webType">页面类型（1、纯表单，2、表单加列表，3、表单列表工作流）.</param>
    /// <returns></returns>
    public static List<string> BackendTargetPathList(string tableName, string fileName, int webType)
    {
        List<string> targetPathList = new List<string>();
        var backendPath = Path.Combine(KeyVariable.SystemPath, "CodeGenerate", fileName, "NetCode");
        var servicePath = Path.Combine(backendPath, "Extend", tableName + "Service.cs");
        var iservicePath = Path.Combine(backendPath, "Interfaces","I" + tableName + "Service.cs");
        var entityPath = Path.Combine(backendPath, "Entitys", "Entity", tableName + "Entity.cs");
        var mapperPath = Path.Combine(backendPath, "Entitys", "Mapper", tableName + "Mapper.cs");
        var inputCrPath = Path.Combine(backendPath, "Entitys", "Dto", tableName + "CrInput.cs");
        var inputUpPath = Path.Combine(backendPath, "Entitys", "Dto", tableName + "UpInput.cs");
        var inputListQueryPath = Path.Combine(backendPath, "Entitys", "Dto", tableName + "ListQueryInput.cs");
        var outputInfoPath = Path.Combine(backendPath, "Entitys", "Dto", tableName + "InfoOutput.cs");
        var outputListPath = Path.Combine(backendPath, "Entitys", "Dto", tableName + "ListOutput.cs");
        switch (webType)
        {
            case 1:
                targetPathList.Add(servicePath);
                targetPathList.Add(iservicePath);
                targetPathList.Add(entityPath);
                targetPathList.Add(mapperPath);
                targetPathList.Add(inputCrPath);
                break;
            case 2:
            case 3:
                targetPathList.Add(servicePath);
                targetPathList.Add(iservicePath);
                targetPathList.Add(entityPath);
                targetPathList.Add(mapperPath);
                targetPathList.Add(inputCrPath);
                targetPathList.Add(inputUpPath);
                targetPathList.Add(inputListQueryPath);
                targetPathList.Add(outputInfoPath);
                targetPathList.Add(outputListPath);
                break;
        }

        return targetPathList;
    }

    #endregion

    #region 后端子表

    /// <summary>
    /// 后端模板文件路径集合.
    /// </summary>
    /// <param name="genModel">SingleTable-单主表,MainBelt-主带子,,,.</param>
    /// <param name="webType">生成模板类型（1、纯表单，2、表单加列表，3、表单列表工作流）.</param>
    /// <returns></returns>
    public static List<string> BackendChildTableTemplatePathList(string genModel, int webType)
    {
        List<string> templatePathList = new List<string>();
        var templatePath = Path.Combine(App.WebHostEnvironment.WebRootPath, "Template");
        switch (webType)
        {
            case 1:
                templatePathList.Add(Path.Combine(templatePath, genModel, "Entity.cs.vm"));
                templatePathList.Add(Path.Combine(templatePath, genModel, "Mapper.cs.vm"));
                templatePathList.Add(Path.Combine(templatePath, genModel, "CrInput.cs.vm"));
                break;
            case 2:
            case 3:
                templatePathList.Add(Path.Combine(templatePath, genModel, "Entity.cs.vm"));
                templatePathList.Add(Path.Combine(templatePath, genModel, "Mapper.cs.vm"));
                templatePathList.Add(Path.Combine(templatePath, genModel, "CrInput.cs.vm"));
                templatePathList.Add(Path.Combine(templatePath, "UpInput.cs.vm"));
                templatePathList.Add(Path.Combine(templatePath, genModel, "InfoOutput.cs.vm"));
                break;
        }

        return templatePathList;
    }

    /// <summary>
    /// 后端主表生成文件路径.
    /// </summary>
    /// <param name="tableName">表名.</param>
    /// <param name="fileName">文件价名称.</param>
    /// <param name="webType">页面类型（1、纯表单，2、表单加列表，3、表单列表工作流）.</param>
    /// <returns></returns>
    public static List<string> BackendChildTableTargetPathList(string tableName, string fileName, int webType)
    {
        List<string> targetPathList = new List<string>();
        var backendPath = Path.Combine(KeyVariable.SystemPath, "CodeGenerate", fileName, "NetCode");
        var entityPath = Path.Combine(backendPath, "Entitys", "Entity", tableName + "Entity.cs");
        var mapperPath = Path.Combine(backendPath, "Entitys", "Mapper", tableName + "Mapper.cs");
        var inputCrPath = Path.Combine(backendPath, "Entitys", "Dto", tableName + "CrInput.cs");
        var inputUpPath = Path.Combine(backendPath, "Entitys", "Dto", tableName + "UpInput.cs");
        var outputInfoPath = Path.Combine(backendPath, "Entitys", "Dto", tableName + "InfoOutput.cs");
        switch (webType)
        {
            case 1:
                targetPathList.Add(entityPath);
                targetPathList.Add(mapperPath);
                targetPathList.Add(inputCrPath);
                break;
            case 2:
            case 3:
                targetPathList.Add(entityPath);
                targetPathList.Add(mapperPath);
                targetPathList.Add(inputCrPath);
                targetPathList.Add(inputUpPath);
                targetPathList.Add(outputInfoPath);
                break;
        }

        return targetPathList;
    }

    #endregion

    #region 后端副表

    /// <summary>
    /// 后端副表生成文件路径.
    /// </summary>
    /// <param name="tableName">表名.</param>
    /// <param name="fileName">文件价名称.</param>
    /// <param name="webType">生成模板类型（1、纯表单，2、表单加列表，3、表单列表工作流）.</param>
    /// <returns></returns>
    public static List<string> BackendAuxiliaryTargetPathList(string tableName, string fileName, int webType)
    {
        List<string> targetPathList = new List<string>();
        var backendPath = Path.Combine(KeyVariable.SystemPath, "CodeGenerate", fileName, "NetCode");
        var entityPath = Path.Combine(backendPath, "Entitys", "Entity", tableName + "Entity.cs");
        var mapperPath = Path.Combine(backendPath, "Entitys", "Mapper", tableName + "Mapper.cs");
        var inputCrPath = Path.Combine(backendPath, "ModEntitysels", "Dto", tableName + "CrInput.cs");
        var outputInfoPath = Path.Combine(backendPath, "Entitys", "Dto", tableName + "InfoOutput.cs");

        switch (webType)
        {
            case 1:
                targetPathList.Add(entityPath);
                targetPathList.Add(mapperPath);
                targetPathList.Add(inputCrPath);
                break;
            default:
                targetPathList.Add(entityPath);
                targetPathList.Add(mapperPath);
                targetPathList.Add(inputCrPath);
                targetPathList.Add(outputInfoPath);
                break;
        }

        return targetPathList;
    }

    /// <summary>
    /// 后端副表模板文件路径集合.
    /// </summary>
    /// <param name="genModel">SingleTable-单主表,MainBelt-主带子,,,.</param>
    /// <param name="webType">生成模板类型（1、纯表单，2、表单加列表，3、表单列表工作流）.</param>
    /// <returns></returns>
    public static List<string> BackendAuxiliaryTemplatePathList(string genModel, int webType)
    {
        List<string> templatePathList = new List<string>();
        var templatePath = Path.Combine(App.WebHostEnvironment.WebRootPath, "Template");

        switch (webType)
        {
            case 1:
                templatePathList.Add(Path.Combine(templatePath, genModel, "Entity.cs.vm"));
                templatePathList.Add(Path.Combine(templatePath, genModel, "Mapper.cs.vm"));
                templatePathList.Add(Path.Combine(templatePath, genModel, "CrInput.cs.vm"));
                break;
            default:
                templatePathList.Add(Path.Combine(templatePath, genModel, "Entity.cs.vm"));
                templatePathList.Add(Path.Combine(templatePath, genModel, "Mapper.cs.vm"));
                templatePathList.Add(Path.Combine(templatePath, genModel, "CrInput.cs.vm"));
                templatePathList.Add(Path.Combine(templatePath, genModel, "InfoOutput.cs.vm"));
                break;
        }

        return templatePathList;
    }

    #endregion

    #region 后端流程

    /// <summary>
    /// 后端流程模板文件路径集合.
    /// </summary>
    /// <param name="genModel">SingleTable-单主表,MainBelt-主带子,,,.</param>
    /// <returns></returns>
    public static List<string> BackendFlowTemplatePathList(string genModel)
    {
        List<string> templatePathList = new List<string>();
        var templatePath = Path.Combine(App.WebHostEnvironment.WebRootPath, "Template");

        templatePathList.Add(Path.Combine(templatePath, "Workflow", genModel, "Service.cs.vm"));
        templatePathList.Add(Path.Combine(templatePath, "IService.cs.vm"));
        templatePathList.Add(Path.Combine(templatePath, genModel, "Entity.cs.vm"));
        templatePathList.Add(Path.Combine(templatePath, genModel, "Mapper.cs.vm"));
        templatePathList.Add(Path.Combine(templatePath, genModel, "CrInput.cs.vm"));
        templatePathList.Add(Path.Combine(templatePath, "UpInput.cs.vm"));
        templatePathList.Add(Path.Combine(templatePath, genModel, "InfoOutput.cs.vm"));

        return templatePathList;
    }

    /// <summary>
    /// 后端主表生成文件路径.
    /// </summary>
    /// <param name="tableName">表名.</param>
    /// <param name="fileName">文件价名称.</param>
    /// <returns></returns>
    public static List<string> BackendFlowTargetPathList(string tableName, string fileName)
    {
        List<string> targetPathList = new List<string>();
        var backendPath = Path.Combine(KeyVariable.SystemPath, "CodeGenerate", fileName, "NetCode");
        var servicePath = Path.Combine(backendPath, "Extend", tableName + "Service.cs");
        var iservicePath = Path.Combine(backendPath, "Interfaces", "I" + tableName + "Service.cs");
        var entityPath = Path.Combine(backendPath, "Entitys", "Entity", tableName + "Entity.cs");
        var mapperPath = Path.Combine(backendPath, "Entitys", "Mapper", tableName + "Mapper.cs");
        var inputCrPath = Path.Combine(backendPath, "Entitys", "Dto", tableName + "CrInput.cs");
        var inputUpPath = Path.Combine(backendPath, "Entitys", "Dto", tableName + "UpInput.cs");
        var outputInfoPath = Path.Combine(backendPath, "Entitys", "Dto", tableName + "InfoOutput.cs");

        targetPathList.Add(servicePath);
        targetPathList.Add(iservicePath);
        targetPathList.Add(entityPath);
        targetPathList.Add(mapperPath);
        targetPathList.Add(inputCrPath);
        targetPathList.Add(inputUpPath);
        targetPathList.Add(outputInfoPath);

        return targetPathList;
    }

    #endregion
}