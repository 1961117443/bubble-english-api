using System.ComponentModel.DataAnnotations;

namespace QT.VisualDev.Entitys.Dto.VisualDev;

/// <summary>
/// 可视化开发同步到菜单输入.
/// </summary>
public class VisualDevToMenuInput
{
    /// <summary>
    /// ID.
    /// </summary>
    public string? id { get; set; }

    /// <summary>
    /// 同步App菜单 1 同步.
    /// </summary>
    public int app { get; set; } = 1;

    /// <summary>
    /// 同步PC菜单 1 同步.
    /// </summary>
    public int pc { get; set; } = 1;

    /// <summary>
    /// Pc端同步菜单父级ID.
    /// </summary>
    public string? pcModuleParentId { get; set; }

    /// <summary>
    /// App端同步菜单父级ID.
    /// </summary>
    public string? appModuleParentId { get; set; }
}


/// <summary>
/// 可视化开发生成到到菜单输入.
/// </summary>
public class VisualDevGenerateMenuInput
{
    /// <summary>
    /// ID.
    /// </summary>
    public string? id { get; set; }

    ///// <summary>
    ///// 同步App菜单 1 同步.
    ///// </summary>
    //public int app { get; set; } = 1;

    ///// <summary>
    ///// 同步PC菜单 1 同步.
    ///// </summary>
    //public int pc { get; set; } = 1;

    /// <summary>
    /// Pc端同步菜单父级ID.
    /// </summary>
    public string? pcModuleParentId { get; set; }

    ///// <summary>
    ///// App端同步菜单父级ID.
    ///// </summary>
    //public string? appModuleParentId { get; set; }


    /// <summary>
    /// 菜单类型
    /// </summary>
    [Required(ErrorMessage = "菜单类型不能为空")]
    public int type { get; set; }


    /// <summary>
    /// 路由前缀
    /// </summary>
    public string prefix { get; set; }
}