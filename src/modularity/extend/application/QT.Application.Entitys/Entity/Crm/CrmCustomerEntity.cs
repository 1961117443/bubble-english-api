using QT.Common.Contracts;
using SqlSugar;

namespace QT.Iot.Application.Entity;

/// <summary>
/// 客户管理
/// </summary>
[SugarTable("crm_customer")]
public class CrmCustomerEntity : CUDEntityBase
{
    /// <summary>
    /// 客户名称
    /// </summary>
    [SugarColumn(ColumnName = "Name")]
    public string Name { get; set; }
 

    /// <summary>
    /// 负责人名称
    /// </summary>
    [SugarColumn(ColumnName = "AdminName")]
    public string AdminName { get; set; }

    /// <summary>
    /// 负责人电话
    /// </summary>
    [SugarColumn(ColumnName = "AdminTel")]
    public string AdminTel { get; set; }


    ///// <summary>
    ///// 附件
    ///// </summary>
    //[SugarColumn(ColumnName = "Attachment")]
    //public string Attachment { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnName = "Remark")]
    public string Remark { get; set; }
}
