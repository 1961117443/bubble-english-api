using QT.DependencyInjection;

namespace QT.VisualDev.Engine.Model.CodeGen;

/// <summary>
/// 前端生成配置模型.
/// </summary>
[SuppressSniffer]
public class FrontEndGenConfigModel
{
    /// <summary>
    /// 命名空间.
    /// </summary>
    public string NameSpace { get; set; }

    /// <summary>
    /// 类型名称.
    /// </summary>
    public string ClassName { get; set; }

    /// <summary>
    /// 表单.
    /// </summary>
    public string FormRef { get; set; }

    /// <summary>
    /// 表单模型.
    /// </summary>
    public string FormModel { get; set; }

    /// <summary>
    /// 尺寸.
    /// </summary>
    public string Size { get; set; }

    /// <summary>
    /// 布局方式-文本定位.
    /// </summary>
    public string LabelPosition { get; set; }

    /// <summary>
    /// 布局方式-文本宽度.
    /// </summary>
    public int LabelWidth { get; set; }

    /// <summary>
    /// 表单规则.
    /// </summary>
    public string FormRules { get; set; }

    /// <summary>
    /// 列表布局
    /// 1-普通列表,2-左侧树形+普通表格,3-分组表格.
    /// </summary>
    public int Type { get; set; }

    /// <summary>
    /// 左侧树绑定字段.
    /// </summary>
    public string TreeRelation { get; set; }

    /// <summary>
    /// 左侧树标题.
    /// </summary>
    public string TreeTitle { get; set; }

    /// <summary>
    /// 左侧树数据源绑定字段.
    /// </summary>
    public string TreePropsValue { get; set; }

    /// <summary>
    /// 左侧树数据来源.
    /// </summary>
    public string TreeDataSource { get; set; }

    /// <summary>
    /// 树数据字典.
    /// </summary>
    public string TreeDictionary { get; set; }

    /// <summary>
    /// 数据接口.
    /// </summary>
    public string TreePropsUrl { get; set; }

    /// <summary>
    /// 子级字段.
    /// </summary>
    public string TreePropsChildren { get; set; }

    /// <summary>
    /// 显示字段.
    /// </summary>
    public string TreePropsLabel { get; set; }

    /// <summary>
    /// 左侧树是否存在查询内.
    /// </summary>
    public bool IsExistQuery { get; set; }

    /// <summary>
    /// 是否分页.
    /// </summary>
    public bool HasPage { get; set; }

    /// <summary>
    /// 表单列表.
    /// </summary>
    public List<FormScriptDesignModel> FormList { get; set; }

    /// <summary>
    /// 弹窗类型.
    /// </summary>
    public string PopupType { get; set; }

    /// <summary>
    /// 主键.
    /// </summary>
    public string PrimaryKey { get; set; }

    /// <summary>
    /// 表单主键.
    /// </summary>
    public string FormPrimaryKey { get; set; }

    /// <summary>
    /// 查询列设计.
    /// </summary>
    public List<IndexSearchFieldDesignModel> SearchColumnDesign { get; set; }

    /// <summary>
    /// 头部按钮设计.
    /// </summary>
    public List<IndexButtonDesign> TopButtonDesign { get; set; }

    /// <summary>
    /// 头部按钮设计.
    /// </summary>
    public List<IndexButtonDesign> ColumnButtonDesign { get; set; }

    /// <summary>
    /// 列表设计.
    /// </summary>
    public List<IndexColumnDesign> ColumnDesign { get; set; }

    /// <summary>
    /// 列表主表控件Option.
    /// </summary>
    public List<CodeGenConvIndexListControlOptionDesign> OptionsList { get; set; }

    /// <summary>
    /// 表单全部控件.
    /// </summary>
    public List<FormControlDesignModel> FormAllContols { get; set; }

    /// <summary>
    /// 是否有批量删除.
    /// </summary>
    public bool IsBatchRemoveDel { get; set; }

    /// <summary>
    /// 是否有导出.
    /// </summary>
    public bool IsDownload { get; set; }

    /// <summary>
    /// 是否有删除.
    /// </summary>
    public bool IsRemoveDel { get; set; }

    /// <summary>
    /// 是否有详情.
    /// </summary>
    public bool IsDetail { get; set; }

    /// <summary>
    /// 是否有编辑.
    /// </summary>
    public bool IsEdit { get; set; }

    /// <summary>
    /// 是否有排序.
    /// </summary>
    public bool IsSort { get; set; }

    /// <summary>
    /// 是否有新增.
    /// </summary>
    public bool IsAdd { get; set; }

    /// <summary>
    /// 是否开启按钮权限.
    /// </summary>
    public bool UseBtnPermission { get; set; }

    /// <summary>
    /// 是否开启列表权限.
    /// </summary>
    public bool UseColumnPermission { get; set; }

    /// <summary>
    /// 是否开启表单权限.
    /// </summary>
    public bool UseFormPermission { get; set; }

    /// <summary>
    /// 提交按钮文本.
    /// </summary>
    public string CancelButtonText { get; set; }

    /// <summary>
    /// 确认按钮文本.
    /// </summary>
    public string ConfirmButtonText { get; set; }

    /// <summary>
    /// 普通弹窗表单宽度.
    /// </summary>
    public string GeneralWidth { get; set; }

    /// <summary>
    /// 全屏弹窗表单宽度.
    /// </summary>
    public string FullScreenWidth { get; set; }

    /// <summary>
    /// drawer宽度.
    /// </summary>
    public string DrawerWidth { get; set; }

    /// <summary>
    /// 表单样式.
    /// </summary>
    public string FormStyle { get; set; }

    /// <summary>
    /// 是否合计.
    /// </summary>
    public bool IsSummary { get; set; }

    /// <summary>
    /// 分页大小.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 排序方式.
    /// </summary>
    public string Sort { get; set; }

    /// <summary>
    /// 是否开启打印.
    /// </summary>
    public bool HasPrintBtn { get; set; }

    /// <summary>
    /// 打印按钮文本.
    /// </summary>
    public string PrintButtonText { get; set; }

    /// <summary>
    /// 打印模板ID.
    /// </summary>
    public string PrintId { get; set; }
}