using QT.DependencyInjection;

namespace QT.Common.Const;

/// <summary>
/// qtKey类型.
/// </summary>
[SuppressSniffer]
public class QtKeyConst
{
    #region 基础控件

    /// <summary>
    /// 单行输入.
    /// </summary>
    public const string COMINPUT = "comInput";

    /// <summary>
    /// 多行输入.
    /// </summary>
    public const string TEXTAREA = "textarea";

    /// <summary>
    /// 数字输入.
    /// </summary>
    public const string NUMINPUT = "numInput";

    /// <summary>
    /// 金额输入.
    /// </summary>
    public const string QTAMOUNT = "QTAmount";

    /// <summary>
    /// 开关.
    /// </summary>
    public const string SWITCH = "switch";

    /// <summary>
    /// 单选.
    /// </summary>
    public const string RADIO = "radio";

    /// <summary>
    /// 多选框.
    /// </summary>
    public const string CHECKBOX = "checkbox";

    /// <summary>
    /// 下拉框.
    /// </summary>
    public const string SELECT = "select";

    /// <summary>
    /// 级联选择.
    /// </summary>
    public const string CASCADER = "cascader";

    /// <summary>
    /// 日期选择.
    /// </summary>
    public const string DATE = "date";

    /// <summary>
    /// 日期选择.
    /// </summary>
    public const string TIME = "time";

    /// <summary>
    /// 时间选择.
    /// </summary>
    public const string DATETIME = "datetime";

    /// <summary>
    /// 文件上传.
    /// </summary>
    public const string UPLOADFZ = "uploadFz";

    /// <summary>
    /// 图片上传.
    /// </summary>
    public const string UPLOADIMG = "uploadImg";

    /// <summary>
    /// 颜色选择.
    /// </summary>
    public const string COLORPICKER = "colorPicker";

    /// <summary>
    /// 评分.
    /// </summary>
    public const string RATE = "rate";

    /// <summary>
    /// 滑块.
    /// </summary>
    public const string SLIDER = "slider";

    /// <summary>
    /// 分割线.
    /// </summary>
    public const string DIVIDER = "divider";

    /// <summary>
    /// 文本.
    /// </summary>
    public const string QTTEXT = "QTText";

    /// <summary>
    /// 富文本.
    /// </summary>
    public const string EDITOR = "editor";

    /// <summary>
    /// 按钮.
    /// </summary>
    public const string BUTTON = "button";

    #endregion

    #region 高级控件

    /// <summary>
    /// 组织选择.
    /// </summary>
    public const string COMSELECT = "comSelect";

    /// <summary>
    /// 部门选择.
    /// </summary>
    public const string DEPSELECT = "depSelect";

    /// <summary>
    /// 岗位选择.
    /// </summary>
    public const string POSSELECT = "posSelect";

    /// <summary>
    /// 用户选择.
    /// </summary>
    public const string USERSELECT = "userSelect";

    /// <summary>
    /// 树形选择.
    /// </summary>
    public const string TREESELECT = "treeSelect";

    /// <summary>
    /// 单据组件.
    /// </summary>
    public const string BILLRULE = "billRule";

    /// <summary>
    /// 设计子表.
    /// </summary>
    public const string TABLE = "table";

    /// <summary>
    /// 行政区划.
    /// </summary>
    public const string ADDRESS = "address";

    /// <summary>
    /// 关联表单.
    /// </summary>
    public const string RELATIONFORM = "relationForm";

    /// <summary>
    /// 弹窗选择.
    /// </summary>
    public const string POPUPSELECT = "popupSelect";

    /// <summary>
    /// 关联表单属性.
    /// </summary>
    public const string RELATIONFORMATTR = "relationFormAttr";

    /// <summary>
    /// 弹窗选择属性.
    /// </summary>
    public const string POPUPATTR = "popupAttr";

    /// <summary>
    /// 计算公式.
    /// </summary>
    public const string CALCULATE = "calculate";

    /// <summary>
    /// 条形码.
    /// </summary>
    public const string BARCODE = "barcode";

    /// <summary>
    /// 二维码.
    /// </summary>
    public const string QRCODE = "qrcode";

    #endregion

    #region 系统控件

    /// <summary>
    /// 创建人员.
    /// </summary>
    public const string CREATEUSER = "createUser";

    /// <summary>
    /// 创建时间.
    /// </summary>
    public const string CREATETIME = "createTime";

    /// <summary>
    /// 修改人员.
    /// </summary>
    public const string MODIFYUSER = "modifyUser";

    /// <summary>
    /// 修改时间.
    /// </summary>
    public const string MODIFYTIME = "modifyTime";

    /// <summary>
    /// 所属部门.
    /// </summary>
    public const string CURRDEPT = "currDept";

    /// <summary>
    /// 所属组织.
    /// </summary>
    public const string CURRORGANIZE = "currOrganize";

    /// <summary>
    /// 所属岗位.
    /// </summary>
    public const string CURRPOSITION = "currPosition";

    #endregion

    #region 布局控件

    /// <summary>
    /// 分组标题.
    /// </summary>
    public const string GROUPTITLE = "groupTitle";

    /// <summary>
    /// 卡片容器.
    /// </summary>
    public const string CARD = "card";

    /// <summary>
    /// 栅格容器.
    /// </summary>
    public const string ROW = "row";

    /// <summary>
    /// 标签面板.
    /// </summary>
    public const string TAB = "tab";

    /// <summary>
    /// 折叠面板.
    /// </summary>
    public const string COLLAPSE = "collapse";

    #endregion

    /// <summary>
    /// 数据字典.
    /// </summary>
    public const string DICTIONARY = "dicSelect";

    /// <summary>
    /// 时间范围.
    /// </summary>
    public const string TIMERANGE = "timeRange";

    /// <summary>
    /// 日期范围.
    /// </summary>
    public const string DATERANGE = "dateRange";
}