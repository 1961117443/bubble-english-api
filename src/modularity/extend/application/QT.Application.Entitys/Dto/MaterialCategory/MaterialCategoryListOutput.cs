using QT.Common.Security;

namespace QT.Iot.Application.Dto.MaterialCategory;

public class MaterialCategoryListOutput : MaterialCategoryOutput, ITreeModel
{
    public bool hasChildren { get; set; }
    public List<object>? children { get; set; }
    public int num { get; set; }
    public bool isLeaf { get; set; }
}
