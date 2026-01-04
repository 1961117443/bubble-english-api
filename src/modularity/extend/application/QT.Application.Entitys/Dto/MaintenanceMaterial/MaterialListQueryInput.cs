using QT.Common.Filter;

namespace QT.Iot.Application.Dto.MaintenanceMaterial;

public class MaintenanceMaterialListQueryInput:PageInputBase
{
    public string projectId { get; set; }
}
