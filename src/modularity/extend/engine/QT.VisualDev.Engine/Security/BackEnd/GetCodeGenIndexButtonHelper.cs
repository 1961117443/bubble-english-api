namespace QT.VisualDev.Engine.Security;

/// <summary>
/// 代码生成列表按钮帮助类.
/// </summary>
public class GetCodeGenIndexButtonHelper
{
    /// <summary>
    /// 代码生成单表Index列表列按钮方法.
    /// </summary>
    /// <param name="value">按钮类型.</param>
    /// <param name="primaryKey">主键key.</param>
    /// <param name="isFlow">是否工作流表单.</param>
    /// <returns></returns>
    public static string IndexColumnButton(string value, string primaryKey, bool isFlow = false)
    {
        string method = string.Empty;
        switch (value)
        {
            case "edit":
                method = string.Format("addOrUpdateHandle(scope.row.{0})", primaryKey);
                break;
            case "remove":
                method = string.Format("handleDel(scope.row.{0})", primaryKey);
                break;
            case "detail":
                switch (isFlow)
                {
                    case true:
                        method = string.Format("addOrUpdateHandle(scope.row.{0},scope.row.flowState)", primaryKey);
                        break;
                    default:
                        //method = string.Format("goDetail(scope.row.{0})", primaryKey);
                        method = string.Format("addOrUpdateHandle(scope.row.{0},true)", primaryKey);
                        break;
                }

                break;
        }

        return method;
    }

    /// <summary>
    /// 代码生成单表Index列表头部按钮方法.
    /// </summary>
    /// <returns></returns>
    public static string IndexTopButton(string value)
    {
        var method = string.Empty;
        switch (value)
        {
            case "add":
                method = "addOrUpdateHandle()";
                break;
            case "download":
                method = "exportData()";
                break;
            case "batchRemove":
                method = "handleBatchRemoveDel()";
                break;
        }

        return method;
    }

    /// <summary>
    /// 代码生成流程Index列表列按钮是否禁用.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string WorkflowIndexColumnButton(string value)
    {
        var disabled = string.Empty;
        switch (value)
        {
            case "edit":
                disabled = ":disabled='[1, 2, 4, 5].indexOf(scope.row.flowState) > -1' ";
                break;
            case "remove":
                disabled = ":disabled='!!scope.row.flowState' ";
                break;
            case "detail":
                disabled = ":disabled='!scope.row.flowState' ";
                break;
        }

        return disabled;
    }
}