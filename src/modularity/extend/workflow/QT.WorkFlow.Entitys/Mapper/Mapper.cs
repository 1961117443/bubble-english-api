using QT.Common.Extension;
using QT.WorkFlow.Entitys.Dto.FlowBefore;
using QT.WorkFlow.Entitys.Dto.FlowEngine;
using QT.WorkFlow.Entitys.Dto.FlowLaunch;
using QT.WorkFlow.Entitys.Entity;
using QT.WorkFlow.Entitys.Model;
using Mapster;

namespace QT.WorkFlow.Entitys.Mapper;

internal class Mapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.ForType<FlowEngineEntity, FlowEngineListAllOutput>()
            .Map(dest => dest.formData, src => src.FormTemplateJson);
        config.ForType<FlowEngineEntity, FlowEngineInfoOutput>()
            .Map(dest => dest.formData, src => src.FormTemplateJson)
            .Map(dest => dest.dbLinkId, src => src.DbLinkId.IsEmpty() ? "0" : src.DbLinkId);
        config.ForType<FlowEngineCrInput, FlowEngineEntity>()
            .Map(dest => dest.FormTemplateJson, src => src.formData);
        config.ForType<FlowEngineUpInput, FlowEngineEntity>()
            .Map(dest => dest.FormTemplateJson, src => src.formData);
        config.ForType<FlowEngineEntity, FlowLaunchListOutput>()
           .Map(dest => dest.formData, src => src.FormTemplateJson);
        config.ForType<FlowEngineEntity, FlowBeforeListOutput>()
            .Map(dest => dest.formData, src => src.FlowTemplateJson);
        config.ForType<FlowTemplateJsonModel, TaskNodeModel>()
            .Map(dest => dest.upNodeId, src => src.prevId);
    }
}
