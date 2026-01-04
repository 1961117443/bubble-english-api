using QT.Common.Security;
using QT.SDMS.Entitys.Entity;

namespace QT.SDMS.Entitys.Dto.Marketer;

public class MarketerListOutput : MarketerOutput, ITreeModelFun
{
    /// <summary>
    /// 业务员
    /// </summary>
    public string userIdName { get; set; }

    /// <summary>
    /// 直属主管
    /// </summary>
    public string managerIdName { get; set; }

    /// <summary>
    /// 等级
    /// </summary>
    public int level { get; set; }

    /// <summary>
    /// 业务状态
    /// </summary>
    public int businessCount { get; set; }
     
    public bool hasChildren { get; set; }
    public List<object>? children { get; set; }
    public int num { get; set; }
    public bool isLeaf { get; set; }

    public string GetId()
    {
        return this.userId;
    }

    public string GetParentId()
    {
        return this.managerId;
    }
}


public class MarketerTreeListOutput : ITreeModel
{
    public string id { get; set; }
    public string parentId { get; set; }
    public bool hasChildren { get; set; }
    public List<object>? children { get; set; }
    public int num { get; set; }
    public bool isLeaf { get; set; }

    public string fullName { get; set; }
}

public class MarketerBounsListOutput: MarketerTreeListOutput
{
    public MarketLevel level { get; set; }

    /// <summary>
    /// 部门总业绩
    /// </summary>
    public decimal deptAmount { get; set; }

    /// <summary>
    /// 小部门业绩（排除第一的其他部门）
    /// </summary>
    public decimal minDeptAmount { get; set; }

    /// <summary>
    /// 当月部门业绩
    /// </summary>
    public decimal monDeptAmount { get; set; }

    /// <summary>
    /// 当月小部门业绩（排除第一的其他部门）
    /// </summary>
    public decimal monMinDeptAmount { get; set; }

    /// <summary>
    /// 当月分红
    /// </summary>
    public decimal? bonus { get; set; }

    /// <summary>
    /// 个人总业绩
    /// </summary>
    public decimal personAmount { get; set; }

    /// <summary>
    /// 当月个人业绩
    /// </summary>
    public decimal monPersonAmount { get; set; }
}