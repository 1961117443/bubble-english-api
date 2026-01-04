using QT.DependencyInjection;
using QT.Systems.Entitys.System;

namespace QT.Systems.Entitys.Dto.DictionaryData;

/// <summary>
/// 数据字典导入输入.
/// </summary>
[SuppressSniffer]
public class DictionaryDataExportInput
{
    public List<DictionaryTypeEntity> list { get; set; } = new List<DictionaryTypeEntity>();

    public List<DictionaryDataEntity> modelList { get; set; } = new List<DictionaryDataEntity>();
}