using Microsoft.AspNetCore.Mvc;
using QT.Common.Const;
using QT.Common.Core;
using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Extension;
using QT.DynamicApiController;
using QT.Iot.Application.Dto.MaintenanceProject;
using QT.Iot.Application.Entity;
using QT.Systems.Interfaces.System;
using SqlSugar;
using Yitter.IdGenerator;

namespace QT.Iot.Application.Service;

/// <summary>
/// 业务实现：维保项目.
/// </summary>
[ApiDescriptionSettings("维保管理", Tag = "维保项目", Name = "MaintenanceProject", Order = 300)]
[Route("api/iot/apply/maintenance-project")]
public class MaintenanceProjectService : QTBaseService<MaintenanceProjectEntity, MaintenanceProjectCrInput, MaintenanceProjectUpInput, MaintenanceProjectOutput, MaintenanceProjectListQueryInput, MaintenanceProjectListOutput>, IDynamicApiController
{
    private readonly IBillRullService _billRullService;

    public MaintenanceProjectService(ISqlSugarRepository<MaintenanceProjectEntity> repository, ISqlSugarClient context, IUserManager userManager, IBillRullService billRullService) : base(repository, context, userManager)
    {
        _billRullService = billRullService;
    }
}