using QT.Common.Security;
using QT.Systems.Entitys.Dto.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Systems.Entitys.Dto.Crm.ExperienceUsers;

public class ExperienceUserListOutput: UserListOutput
{
    /// <summary>
    /// 到期时间
    /// </summary>
    public DateTime? expireTime { get; set; }

    /// <summary>
    /// 推荐人id
    /// </summary>
    public string sid { get; set; }

    /// <summary>
    /// 推荐人
    /// </summary>
    public string managerUserIdName { get; set; }

    /// <summary>
    /// 最后登录时间
    /// </summary>
    public DateTime? lastLogTime { get; set; }
}


public class ExperienceUserTreeListOutput : ExperienceUserListOutput, ITreeModelFun
{
    public bool hasChildren { get; set; }
    public List<object>? children { get; set;}
    public int num { get; set; }
    public bool isLeaf { get; set; }

    public string GetId()
    {
        return this.id;
    }

    public string GetParentId()
    {
        return this.sid;
    }
}