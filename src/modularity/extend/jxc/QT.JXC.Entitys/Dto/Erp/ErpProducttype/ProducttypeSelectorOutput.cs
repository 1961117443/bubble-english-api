using QT.Common.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.JXC.Entitys.Dto.Erp.ErpProducttype;

public class ProducttypeSelectorOutput: TreeModel
{
    /// <summary>
    /// 分类编号
    /// </summary>
    public string code { get; set; }

    /// <summary>
    /// 显示名称
    /// </summary>
    public string fullName { get; set; }

    /// <summary>
    /// 图标.
    /// </summary>
    public string icon { get; set; } = "icon-qt icon-qt-tree-department1";


    /// <summary>
    /// 具体的节点数据
    /// </summary>
    public object data { get; set; }

    /// <summary>
    /// 序号：排序规则数字越大越靠前。
    /// </summary>
    public int order { get; set; }
}
