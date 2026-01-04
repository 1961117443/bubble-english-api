using QT.Common.Security;
using QT.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace QT.Archive.Dto.Building;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class BuildingListOutput: BuildingInfoOutput
{
 
}

public class BuildingListTreeOutput : BuildingInfoOutput, ITreeModelFun
{
    public bool hasChildren { get; set; }
    public List<object> children { get; set; }
    public int num { get; set; }
    public bool isLeaf { get; set; }

    public string GetId()
    {
        return this.id;
    }

    public string GetParentId()
    {
        return this.pid;
    }
}