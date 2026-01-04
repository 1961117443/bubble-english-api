using QT.DependencyInjection;

namespace QT.VisualDev.Engine.Model.CodeGen;

/// <summary>
/// 代码生成详细配置参数.
/// </summary>
[SuppressSniffer]
public class CodeGenConfigModel
{
    /// <summary>
    /// 名称.
    /// </summary>
    public string FullName { get; set; }

    /// <summary>
    /// 业务名.
    /// </summary>
    public string BusName { get; set; }

    /// <summary>
    /// 命名空间.
    /// </summary>
    public string NameSpace { get; set; }

    /// <summary>
    /// 类型名称.
    /// </summary>
    public string ClassName { get; set; }

    /// <summary>
    /// 主键.
    /// </summary>
    public string PrimaryKey { get; set; }

    /// <summary>
    /// 主键(首字母小写).
    /// </summary>
    public string LowerPrimaryKey => string.IsNullOrWhiteSpace(PrimaryKey) ? null : PrimaryKey.Substring(0, 1).ToLower() + PrimaryKey[1..];

    /// <summary>
    /// 原始主键.
    /// </summary>
    public string OriginalPrimaryKey { get; set; }

    /// <summary>
    /// 主表.
    /// </summary>
    public string MainTable { get; set; }

    /// <summary>
    /// 原本名称.
    /// </summary>
    public string OriginalMainTableName { get; set; }

    /// <summary>
    /// 主表(首字母小写).
    /// </summary>
    public string LowerMainTable => string.IsNullOrWhiteSpace(MainTable) ? null : MainTable.Substring(0, 1).ToLower() + MainTable[1..];

    /// <summary>
    /// 服务列表.
    /// </summary>
    public List<string> ServiceList { get; set; }

    /// <summary>
    /// 列表分页.
    /// </summary>
    public bool hasPage { get; set; }

    /// <summary>
    /// 功能列表.
    /// </summary>
    public List<CodeGenFunctionModel> Function { get; set; }

    /// <summary>
    /// 表字段.
    /// </summary>
    public List<TableColumnConfigModel> TableField { get; set; }

    /// <summary>
    /// 表关系.
    /// </summary>
    public List<CodeGenTableRelationsModel> TableRelations { get; set; }

    /// <summary>
    /// 默认排序.
    /// </summary>
    public string DefaultSidx { get; set; }

    /// <summary>
    /// 是否存在单据规则控件.
    /// </summary>
    public bool IsBillRule { get; set; }

    /// <summary>
    /// 是否导出.
    /// </summary>
    public bool IsExport { get; set; }

    /// <summary>
    /// 是否批量删除.
    /// </summary>
    public bool IsBatchRemove { get; set; }

    /// <summary>
    /// 是否有上传控件.
    /// </summary>
    public bool IsUpload { get; set; }

    /// <summary>
    /// 是否存在关系.
    /// </summary>
    public bool IsTableRelations { get; set; }

    /// <summary>
    /// 是否要生成对象映射.
    /// </summary>
    public bool IsMapper { get; set; }

    /// <summary>
    /// 是否主表.
    /// </summary>
    public bool IsMainTable { get; set; }

    /// <summary>
    /// 是否自带FlowId.
    /// </summary>
    public bool IsFlowId { get; set; }

    /// <summary>
    /// 是否副表.
    /// </summary>
    public bool IsAuxiliaryTable { get; set; }

    /// <summary>
    /// 数据库连接ID.
    /// </summary>
    public string DbLinkId { get; set; }

    /// <summary>
    /// 生成引擎ID.
    /// </summary>
    public string FlowId { get; set; }

    /// <summary>
    /// 页面类型（1、纯表单，2、表单加列表，3、表单列表工作流）.
    /// </summary>
    public int WebType { get; set; }

    /// <summary>
    /// 页面类型（1-Web设计,2-App设计,3-流程表单,4-Web表单,5-App表单）.
    /// </summary>
    public int Type { get; set; }

    /// <summary>
    /// 模板编码.
    /// </summary>
    public string EnCode { get; set; }

    /// <summary>
    /// 是否开启数据权限.
    /// </summary>
    public bool UseDataPermission { get; set; }

    /// <summary>
    /// 查询类型为等于的控件数量.
    /// </summary>
    public int SearchControlNum { get; set; }

    /// <summary>
    /// 表关系模型.
    /// </summary>
    public List<CodeGenTableRelationsModel> AuxiliaryTable { get; set; }

    /// <summary>
    /// FlowId字段名称.
    /// </summary>
    public string FlowIdFieldName { get; set; }

    /// <summary>
    /// FlowId字段名称.
    /// </summary>
    public string LowerFlowIdFieldName => string.IsNullOrWhiteSpace(FlowIdFieldName) ? null : FlowIdFieldName.Substring(0, 1).ToLower() + FlowIdFieldName[1..];

    /// <summary>
    /// 导出字段.
    /// </summary>
    public string ExportField { get; set; }
}