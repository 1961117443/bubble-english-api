using System.Text;
using System.Text.RegularExpressions;
using QT.Common.Const;
using QT.Common.Extension;
using QT.Common.Security;
using QT.VisualDev.Engine.Model.CodeGen;

namespace QT.VisualDev.Engine.Security;

/// <summary>
/// 代码生成表单控件设计帮助类.
/// </summary>
public class CodeGenFormControlDesignHelper
{
    private static int active = 1;

    /// <summary>
    /// 表单控件设计.
    /// </summary>
    /// <param name="fieldList">组件列表.</param>
    /// <param name="gutter">间隔.</param>
    /// <param name="labelWidth">标签宽度.</param>
    /// <param name="isMain">是否主循环.</param>
    /// <returns></returns>
    public static List<FormControlDesignModel> FormControlDesign(List<FieldsModel> fieldList, int gutter, int labelWidth, bool isMain = false)
    {
        if (isMain) active = 1;
        List<FormControlDesignModel> list = new List<FormControlDesignModel>();
        foreach (var item in fieldList)
        {
            var config = item.__config__;
            switch (config.qtKey)
            {
                case QtKeyConst.ROW:
                    {
                        list.Add(new FormControlDesignModel()
                        {
                            qtKey = config.qtKey,
                            Span = config.span,
                            Gutter = gutter,
                            Children = FormControlDesign(config.children, gutter, labelWidth)
                        });
                    }

                    break;
                case QtKeyConst.TABLE:
                    {
                        List<FormControlDesignModel> childrenTableList = new List<FormControlDesignModel>();
                        foreach (var children in config.children)
                        {
                            var childrenConfig = children.__config__;
                            switch (childrenConfig.qtKey)
                            {
                                case QtKeyConst.RELATIONFORMATTR:
                                case QtKeyConst.POPUPATTR:
                                    {
                                        var relationField = Regex.Match(children.relationField, @"^(.+)_qtTable_").Groups[1].Value;
                                        childrenTableList.Add(new FormControlDesignModel()
                                        {
                                            qtKey = childrenConfig.qtKey,
                                            RelationField = relationField,
                                            ShowField = children.showField,
                                            Tag = childrenConfig.tag,
                                            Label = childrenConfig.label,
                                            Span = childrenConfig.span,
                                            LabelWidth = childrenConfig?.labelWidth ?? labelWidth,
                                            ColumnWidth = childrenConfig?.columnWidth != null ? $"width='{childrenConfig.columnWidth}' " : null,
                                            required = childrenConfig.required
                                        });
                                    }

                                    break;
                                default:
                                    {
                                        childrenTableList.Add(new FormControlDesignModel()
                                        {
                                            qtKey = childrenConfig.qtKey,
                                            Name = children.__vModel__,
                                            OriginalName = children.__vModel__,
                                            Style = children.style != null && !children.style.ToString().Equals("{}") ? $":style='{children.style.ToJsonString()}' " : string.Empty,
                                            Span = childrenConfig.span,
                                            Placeholder = children.placeholder != null ? $"placeholder='{children.placeholder}' " : string.Empty,
                                            Clearable = children.clearable ? "clearable " : string.Empty,
                                            Readonly = children.@readonly ? "readonly " : string.Empty,
                                            Disabled = children.disabled ? "disabled " : string.Empty,
                                            IsDisabled = item.disabled ? "disabled " : string.Format(":disabled=\"judgeWrite('{0}') || judgeWrite('{0}-{1}')\" ", item.__vModel__, children.__vModel__),
                                            ShowWordLimit = children.showwordlimit ? "show-word-limit " : string.Empty,
                                            Type = children.type != null ? $"type='{children.type}' " : string.Empty,
                                            Format = children.format != null ? $"format='{children.format}' " : string.Empty,
                                            ValueFormat = children.valueformat != null ? $"value-format='{children.valueformat}' " : string.Empty,
                                            AutoSize = children.autosize != null ? $":autosize='{children.autosize.ToJsonString()}' " : string.Empty,
                                            Multiple = children.multiple ? $"multiple " : string.Empty,
                                            Size = childrenConfig.optionType != null ? (childrenConfig.optionType == "default" ? string.Empty : $"size='{children.size}' ") : string.Empty,
                                            PrefixIcon = !string.IsNullOrEmpty(children.prefixicon) ? $"prefix-icon='{children.prefixicon}' " : string.Empty,
                                            SuffixIcon = !string.IsNullOrEmpty(children.suffixicon) ? $"suffix-icon='{children.suffixicon}' " : string.Empty,
                                            MaxLength = !string.IsNullOrEmpty(children.maxlength) ? $"maxlength='{children.maxlength}' " : string.Empty,
                                            ShowPassword = children.showPassword ? "show-password " : string.Empty,
                                            Filterable = children.filterable ? "filterable " : string.Empty,
                                            Label = childrenConfig.label,
                                            Props = childrenConfig.props,
                                            MainProps = children.props != null ? $":props='{children.__vModel__}Props' " : string.Empty,
                                            Tag = childrenConfig.tag,
                                            Options = children.options != null ? $":options='{children.__vModel__}Options' " : string.Empty,
                                            ShowAllLevels = children.showalllevels ? "show-all-levels " : string.Empty,
                                            Separator = !string.IsNullOrEmpty(children.separator) ? $"separator='{children.separator}' " : string.Empty,
                                            RangeSeparator = !string.IsNullOrEmpty(children.rangeseparator) ? $"range-separator='{children.rangeseparator}' " : string.Empty,
                                            StartPlaceholder = !string.IsNullOrEmpty(children.startplaceholder) ? $"start-placeholder='{children.startplaceholder}' " : string.Empty,
                                            EndPlaceholder = !string.IsNullOrEmpty(children.endplaceholder) ? $"end-placeholder='{children.endplaceholder}' " : string.Empty,
                                            PickerOptions = children.pickeroptions != null && children.pickeroptions.ToJsonString() != "null" ? $":picker-options='{children.pickeroptions.ToJsonString()}' " : string.Empty,
                                            Required = childrenConfig.required ? "required " : string.Empty,
                                            Step = children.step != null ? $":step='{children.step}' " : string.Empty,
                                            StepStrictly = children.stepstrictly ? "step-strictly " : string.Empty,
                                            Max = children.max != null && children.max != 0 ? $":max='{children.max}' " : string.Empty,
                                            Min = children.min != null ? $":min='{children.min}' " : string.Empty,
                                            ColumnWidth = childrenConfig.columnWidth != null ? $"width='{childrenConfig.columnWidth}' " : null,
                                            ModelId = children.modelId != null ? $"modelId='{children.modelId}' " : string.Empty,
                                            RelationField = children.relationField != null ? $"relationField='{children.relationField}' " : string.Empty,
                                            ColumnOptions = children.columnOptions != null ? $":columnOptions='{children.__vModel__}Options' " : string.Empty,
                                            HasPage = children.hasPage ? "hasPage " : string.Empty,
                                            PageSize = children.pageSize != null ? $":pageSize='{children.pageSize}' " : string.Empty,
                                            PropsValue = children.propsValue != null ? $"propsValue='{children.propsValue}' " : string.Empty,
                                            InterfaceId = children.interfaceId != null ? $"interfaceId='{children.interfaceId}' " : string.Empty,
                                            Precision = children.precision != null ? $":precision='{children.precision}' " : string.Empty,
                                            ActiveText = !string.IsNullOrEmpty(children.activetext) ? $"active-text='{children.activetext}' " : string.Empty,
                                            InactiveText = !string.IsNullOrEmpty(children.inactivetext) ? $"inactive-text='{children.inactivetext}' " : string.Empty,
                                            ActiveColor = !string.IsNullOrEmpty(children.activecolor) ? $"active-color='{children.activecolor}' " : string.Empty,
                                            InactiveColor = !string.IsNullOrEmpty(children.inactivecolor) ? $"inactive-color='{children.inactivecolor}' " : string.Empty,
                                            IsSwitch = childrenConfig.qtKey == "switch" ? $":active-value='{children.activevalue}' :inactive-value='{children.inactivevalue}' " : string.Empty,
                                            ShowStops = children.showstops ? $"show-stops " : string.Empty,
                                            Accept = !string.IsNullOrEmpty(children.accept) ? $"accept='{children.accept}' " : string.Empty,
                                            ShowTip = children.showTip ? $"showTip " : string.Empty,
                                            FileSize = children.fileSize != null && !string.IsNullOrEmpty(children.fileSize.ToString()) ? $":fileSize='{children.fileSize}' " : string.Empty,
                                            SizeUnit = !string.IsNullOrEmpty(children.sizeUnit) ? $"sizeUnit='{children.sizeUnit}' " : string.Empty,
                                            Limit = children.limit != null ? $":limit='{children.limit}' " : string.Empty,
                                            ButtonText = !string.IsNullOrEmpty(children.buttonText) ? $"buttonText='{children.buttonText}' " : string.Empty,
                                            Level = childrenConfig.qtKey == "address" ? $":level='{children.level}' " : string.Empty,
                                            NoShow = childrenConfig.noShow ? "v-if='false' " : string.Empty,
                                            Prepend = children.__slot__ != null && !string.IsNullOrEmpty(children.__slot__.prepend) ? children.__slot__.prepend : null,
                                            Append = children.__slot__ != null && !string.IsNullOrEmpty(children.__slot__.append) ? children.__slot__.append : null,
                                            ShowLevel = !string.IsNullOrEmpty(children.showLevel) ? string.Empty : string.Empty,
                                            LabelWidth = childrenConfig?.labelWidth ?? labelWidth,
                                            PopupType = !string.IsNullOrEmpty(children.popupType) ? $"popupType='{children.popupType}' " : string.Empty,
                                            PopupTitle = !string.IsNullOrEmpty(children.popupTitle) ? $"popupTitle='{children.popupTitle}' " : string.Empty,
                                            PopupWidth = !string.IsNullOrEmpty(children.popupWidth) ? $"popupWidth='{children.popupWidth}' " : string.Empty,
                                            Field = childrenConfig.qtKey.Equals("relationForm") || childrenConfig.qtKey.Equals("popupSelect") ? $":field=\"'{children.__vModel__}'+scope.$index\" " : string.Empty,
                                            required = childrenConfig.required
                                        });
                                    }

                                    break;
                            }
                        }

                        list.Add(new FormControlDesignModel()
                        {
                            qtKey = config.qtKey,
                            Name = item.__vModel__,
                            OriginalName = config.tableName,
                            Span = config.span,
                            ShowText = config.showTitle,
                            Label = config.label,
                            ChildTableName = config.tableName.ParseToPascalCase(),
                            Children = childrenTableList,
                            LabelWidth = config?.labelWidth ?? labelWidth,
                            ShowSummary = item.showSummary,
                            required = childrenTableList.Any(it => it.required.Equals(true))
                        });
                    }

                    break;
                case QtKeyConst.CARD:
                    {
                        list.Add(new FormControlDesignModel()
                        {
                            qtKey = config.qtKey,
                            OriginalName = item.__vModel__,
                            Shadow = item.shadow,
                            Children = FormControlDesign(config.children, gutter, labelWidth),
                            Span = config.span,
                            Content = item.header,
                            LabelWidth = config?.labelWidth ?? labelWidth,
                        });
                    }

                    break;
                case QtKeyConst.DIVIDER:
                    {
                        list.Add(new FormControlDesignModel()
                        {
                            qtKey = config.qtKey,
                            OriginalName = item.__vModel__,
                            Span = config.span,
                            Contentposition = item.contentposition,
                            Default = item.__slot__.@default,
                            LabelWidth = config?.labelWidth ?? labelWidth,
                        });
                    }

                    break;
                case QtKeyConst.COLLAPSE:
                    {
                        // 先加为了防止 children下 还有折叠面板
                        List<FormControlDesignModel> childrenCollapseList = new List<FormControlDesignModel>();
                        foreach (var children in config.children)
                        {
                            childrenCollapseList.Add(new FormControlDesignModel()
                            {
                                Title = children.title,
                                Name = children.name,
                                Gutter = gutter,
                                Children = FormControlDesign(children.__config__.children, gutter, labelWidth)
                            });
                        }

                        list.Add(new FormControlDesignModel()
                        {
                            qtKey = config.qtKey,
                            Accordion = item.accordion ? "true" : "false",
                            Name = "active" + active++,
                            Active = childrenCollapseList.Select(it => it.Name).ToJsonString(),
                            Children = childrenCollapseList,
                            Span = config.span,
                            LabelWidth = config?.labelWidth ?? labelWidth,
                        });
                    }

                    break;
                case QtKeyConst.TAB:
                    {
                        // 先加为了防止 children下 还有折叠面板
                        List<FormControlDesignModel> childrenCollapseList = new List<FormControlDesignModel>();
                        foreach (var children in config.children)
                        {
                            childrenCollapseList.Add(new FormControlDesignModel()
                            {
                                Title = children.title,
                                Gutter = gutter,
                                Children = FormControlDesign(children.__config__.children, gutter, labelWidth)
                            });
                        }

                        list.Add(new FormControlDesignModel()
                        {
                            qtKey = config.qtKey,
                            Type = item.type,
                            TabPosition = item.tabPosition,
                            Name = "active" + active++,
                            Active = config.active.ToString(),
                            Children = childrenCollapseList,
                            Span = config.span,
                            LabelWidth = config?.labelWidth ?? labelWidth,
                        });
                    }

                    break;
                case QtKeyConst.GROUPTITLE:
                    {
                        list.Add(new FormControlDesignModel()
                        {
                            qtKey = config.qtKey,
                            Span = config.span,
                            Contentposition = item.contentposition,
                            Content = item.content,
                            LabelWidth = config?.labelWidth ?? labelWidth,
                        });
                    }

                    break;
                case QtKeyConst.QTTEXT:
                    {
                        list.Add(new FormControlDesignModel()
                        {
                            qtKey = config.qtKey,
                            Span = config.span,
                            DefaultValue = config.defaultValue,
                            TextStyle = item.textStyle != null ? item.textStyle.ToJsonString() : string.Empty,
                            Style = item.style.ToJsonString(),
                            LabelWidth = config?.labelWidth ?? labelWidth,
                        });
                    }

                    break;
                case QtKeyConst.BUTTON:
                    {
                        list.Add(new FormControlDesignModel()
                        {
                            qtKey = config.qtKey,
                            Span = config.span,
                            Align = item.align,
                            ButtonText = item.buttonText,
                            Type = item.type,
                            Disabled = item.disabled ? "disabled " : string.Empty,
                            LabelWidth = config?.labelWidth ?? labelWidth,
                        });
                    }

                    break;
                case QtKeyConst.RELATIONFORMATTR:
                case QtKeyConst.POPUPATTR:
                    {
                        var relationField = Regex.Match(item.relationField, @"^(.+)_qtTable_").Groups[1].Value;
                        list.Add(new FormControlDesignModel()
                        {
                            qtKey = config.qtKey,
                            RelationField = relationField,
                            ShowField = item.showField,
                            Tag = config.tag,
                            Label = config.label,
                            Span = config.span,
                            LabelWidth = config?.labelWidth ?? labelWidth,
                        });
                    }

                    break;
                default:
                    {
                        string vModel = string.Empty;
                        var Model = item.__vModel__;
                        vModel = $"v-model='dataForm.{Model}' ";
                        list.Add(new FormControlDesignModel()
                        {
                            Name = item.__vModel__,
                            OriginalName = item.__vModel__,
                            qtKey = config.qtKey,
                            Border = config.border ? "border " : string.Empty,
                            Style = item.style != null && !item.style.ToString().Equals("{}") ? $":style='{item.style.ToJsonString()}' " : string.Empty,
                            Type = !string.IsNullOrEmpty(item.type) ? $"type='{item.type}' " : string.Empty,
                            Span = config.span,
                            Clearable = item.clearable ? "clearable " : string.Empty,
                            Readonly = item.@readonly ? "readonly " : string.Empty,
                            Required = config.required ? "required " : string.Empty,
                            Placeholder = !string.IsNullOrEmpty(item.placeholder) ? $"placeholder='{item.placeholder}' " : string.Empty,
                            Disabled = item.disabled ? "disabled " : string.Empty,
                            IsDisabled = item.disabled ? "disabled " : $":disabled='judgeWrite(\"{item.__vModel__}\")' ",
                            ShowWordLimit = item.showwordlimit ? "show-word-limit " : string.Empty,
                            Format = !string.IsNullOrEmpty(item.format) ? $"format='{item.format}' " : string.Empty,
                            ValueFormat = !string.IsNullOrEmpty(item.valueformat) ? $"value-format='{item.valueformat}' " : string.Empty,
                            AutoSize = item.autosize != null && item.autosize.ToJsonString() != "null" ? $":autosize='{item.autosize.ToJsonString()}' " : string.Empty,
                            Multiple = item.multiple ? $"multiple " : string.Empty,
                            IsRange = item.isrange ? "is-range " : string.Empty,
                            Props = config.props,
                            MainProps = item.props != null ? $":props='{Model}Props' " : string.Empty,
                            OptionType = config.optionType == "default" ? string.Empty : "-button",
                            Size = !string.IsNullOrEmpty(config.optionType) ? (config.optionType == "default" ? string.Empty : $"size='{item.size}' ") : string.Empty,
                            PrefixIcon = !string.IsNullOrEmpty(item.prefixicon) ? $"prefix-icon='{item.prefixicon}' " : string.Empty,
                            SuffixIcon = !string.IsNullOrEmpty(item.suffixicon) ? $"suffix-icon='{item.suffixicon}' " : string.Empty,
                            MaxLength = !string.IsNullOrEmpty(item.maxlength) ? $"maxlength='{item.maxlength}' " : string.Empty,
                            Step = item.step != null ? $":step='{item.step}' " : string.Empty,
                            StepStrictly = item.stepstrictly ? "step-strictly " : string.Empty,
                            ControlsPosition = !string.IsNullOrEmpty(item.controlsposition) ? $"controls-position='{item.controlsposition}' " : string.Empty,
                            ShowChinese = item.showChinese ? "showChinese " : string.Empty,
                            ShowPassword = item.showPassword ? "show-password " : string.Empty,
                            Filterable = item.filterable ? "filterable " : string.Empty,
                            ShowAllLevels = item.showalllevels ? "show-all-levels " : string.Empty,
                            Separator = !string.IsNullOrEmpty(item.separator) ? $"separator='{item.separator}' " : string.Empty,
                            RangeSeparator = !string.IsNullOrEmpty(item.rangeseparator) ? $"range-separator='{item.rangeseparator}' " : string.Empty,
                            StartPlaceholder = !string.IsNullOrEmpty(item.startplaceholder) ? $"start-placeholder='{item.startplaceholder}' " : string.Empty,
                            EndPlaceholder = !string.IsNullOrEmpty(item.endplaceholder) ? $"end-placeholder='{item.endplaceholder}' " : string.Empty,
                            PickerOptions = item.pickeroptions != null && item.pickeroptions.ToJsonString() != "null" ? $":picker-options='{item.pickeroptions.ToJsonString()}' " : string.Empty,
                            Options = item.options != null ? $":options='{item.__vModel__}Options' " : string.Empty,
                            Max = item.max != null && item.max != 0 ? $":max='{item.max}' " : string.Empty,
                            AllowHalf = item.allowhalf ? "allow-half " : string.Empty,
                            ShowTexts = item.showtext ? $"show-text " : string.Empty,
                            ShowScore = item.showScore ? $"show-score " : string.Empty,
                            ShowAlpha = item.showalpha ? $"show-alpha " : string.Empty,
                            ColorFormat = !string.IsNullOrEmpty(item.colorformat) ? $"color-format='{item.colorformat}' " : string.Empty,
                            ActiveText = !string.IsNullOrEmpty(item.activetext) ? $"active-text='{item.activetext}' " : string.Empty,
                            InactiveText = !string.IsNullOrEmpty(item.inactivetext) ? $"inactive-text='{item.inactivetext}' " : string.Empty,
                            ActiveColor = !string.IsNullOrEmpty(item.activecolor) ? $"active-color='{item.activecolor}' " : string.Empty,
                            InactiveColor = !string.IsNullOrEmpty(item.inactivecolor) ? $"inactive-color='{item.inactivecolor}' " : string.Empty,
                            IsSwitch = config.qtKey == "switch" ? $":active-value='{item.activevalue}' :inactive-value='{item.inactivevalue}' " : string.Empty,
                            Min = item.min != null ? $":min='{item.min}' " : string.Empty,
                            ShowStops = item.showstops ? $"show-stops " : string.Empty,
                            Range = item.range ? $"range " : string.Empty,
                            Accept = !string.IsNullOrEmpty(item.accept) ? $"accept='{item.accept}' " : string.Empty,
                            ShowTip = item.showTip ? $"showTip " : string.Empty,
                            FileSize = item.fileSize != null && !string.IsNullOrEmpty(item.fileSize.ToString()) ? $":fileSize='{item.fileSize}' " : string.Empty,
                            SizeUnit = !string.IsNullOrEmpty(item.sizeUnit) ? $"sizeUnit='{item.sizeUnit}' " : string.Empty,
                            Limit = item.limit != null ? $":limit='{item.limit}' " : string.Empty,
                            Contentposition = !string.IsNullOrEmpty(item.contentposition) ? $"content-position='{item.contentposition}' " : string.Empty,
                            ButtonText = !string.IsNullOrEmpty(item.buttonText) ? $"buttonText='{item.buttonText}' " : string.Empty,
                            Level = config.qtKey == "address" ? $":level='{item.level}' " : string.Empty,
                            ActionText = !string.IsNullOrEmpty(item.actionText) ? $"actionText='{item.actionText}' " : string.Empty,
                            Shadow = !string.IsNullOrEmpty(item.shadow) ? $"shadow='{item.shadow}' " : string.Empty,
                            Content = !string.IsNullOrEmpty(item.content) ? $"content='{item.content}' " : string.Empty,
                            NoShow = config.noShow ? "v-if='false' " : string.Empty,
                            Label = config.label,
                            vModel = vModel,
                            Prepend = item.__slot__ != null && !string.IsNullOrEmpty(item.__slot__.prepend) ? item.__slot__.prepend : null,
                            Append = item.__slot__ != null && !string.IsNullOrEmpty(item.__slot__.append) ? item.__slot__.append : null,
                            Tag = config.tag,
                            Count = item.max,
                            ModelId = item.modelId != null ? $"modelId='{item.modelId}' " : string.Empty,
                            RelationField = item.relationField != null ? $"relationField='{item.relationField}' " : string.Empty,
                            ColumnOptions = item.columnOptions != null ? $":columnOptions='{item.__vModel__}Options' " : string.Empty,
                            HasPage = item.hasPage ? "hasPage " : string.Empty,
                            PageSize = item.pageSize != null ? $":pageSize='{item.pageSize}' " : string.Empty,
                            PropsValue = item.propsValue != null ? $"propsValue='{item.propsValue}' " : string.Empty,
                            InterfaceId = item.interfaceId != null ? $"interfaceId='{item.interfaceId}' " : string.Empty,
                            Precision = item.precision != null ? $":precision='{item.precision}' " : string.Empty,
                            ShowLevel = !string.IsNullOrEmpty(item.showLevel) ? string.Empty : string.Empty,
                            LabelWidth = config?.labelWidth ?? labelWidth,
                            PopupType = !string.IsNullOrEmpty(item.popupType) ? $"popupType='{item.popupType}' " : string.Empty,
                            PopupTitle = !string.IsNullOrEmpty(item.popupTitle) ? $"popupTitle='{item.popupTitle}' " : string.Empty,
                            PopupWidth = !string.IsNullOrEmpty(item.popupWidth) ? $"popupWidth='{item.popupWidth}' " : string.Empty,
                            Field = config.qtKey.Equals("relationForm") || config.qtKey.Equals("popupSelect") ? $"field='{item.__vModel__}' " : string.Empty,
                        });
                    }

                    break;
            }
        }

        return list;
    }

    /// <summary>
    /// 表单控件选项配置.
    /// </summary>
    /// <param name="fieldList">组件列表.</param>
    /// <param name="type">1-Web设计,2-App设计,3-流程表单,4-Web表单,5-App表单.</param>
    /// <param name="isMain">是否主循环.</param>
    /// <returns></returns>
    public static List<CodeGenConvIndexListControlOptionDesign> FormControlProps(List<FieldsModel> fieldList, int type, bool isMain = false)
    {
        if (isMain) active = 1;
        List<CodeGenConvIndexListControlOptionDesign> list = new List<CodeGenConvIndexListControlOptionDesign>();
        foreach (var item in fieldList)
        {
            var config = item.__config__;
            switch (config.qtKey)
            {
                case QtKeyConst.CARD:
                case QtKeyConst.ROW:
                    {
                        list.AddRange(FormControlProps(config.children, type));
                    }

                    break;
                case QtKeyConst.TABLE:
                    {
                        for (int i = 0; i < config.children.Count; i++)
                        {
                            var childrenConfig = config.children[i].__config__;
                            switch (childrenConfig.qtKey)
                            {
                                case QtKeyConst.SELECT:
                                    {
                                        switch (childrenConfig.dataType)
                                        {
                                            // 静态数据
                                            case "static":
                                                list.Add(new CodeGenConvIndexListControlOptionDesign()
                                                {
                                                    qtKey = childrenConfig.qtKey,
                                                    Name = config.children[i].__vModel__,
                                                    DictionaryType = childrenConfig.dataType == "dictionary" ? childrenConfig.dictionaryType : (childrenConfig.dataType == "dynamic" ? childrenConfig.propsUrl : null),
                                                    DataType = childrenConfig.dataType,
                                                    IsStatic = true,
                                                    IsIndex = false,
                                                    IsProps = type == 5 || type == 3 ? true : false,
                                                    Props = string.Format("{{'label':'{0}','value':'{1}'}}", childrenConfig.props.label, childrenConfig.props.value),
                                                    IsChildren = true,
                                                    Content = GetCodeGenConvIndexListControlOption(config.children[i].__vModel__, config.children[i].__slot__.options)
                                                });
                                                break;
                                            default:
                                                list.Add(new CodeGenConvIndexListControlOptionDesign()
                                                {
                                                    qtKey = childrenConfig.qtKey,
                                                    Name = config.children[i].__vModel__,
                                                    DictionaryType = childrenConfig.dataType == "dictionary" ? childrenConfig.dictionaryType : (childrenConfig.dataType == "dynamic" ? childrenConfig.propsUrl : null),
                                                    DataType = childrenConfig.dataType,
                                                    IsStatic = false,
                                                    IsIndex = false,
                                                    IsProps = type == 5 || type == 3 ? true : false,
                                                    Props = string.Format("{{'label':'{0}','value':'{1}'}}", childrenConfig.props.label, childrenConfig.props.value),
                                                    IsChildren = true,
                                                    Content = string.Format("{0}Options : [],", config.children[i].__vModel__)
                                                });
                                                break;
                                        }
                                    }

                                    break;
                                case QtKeyConst.TREESELECT:
                                case QtKeyConst.CASCADER:
                                    {
                                        switch (childrenConfig.dataType)
                                        {
                                            case "static":
                                                list.Add(new CodeGenConvIndexListControlOptionDesign()
                                                {
                                                    qtKey = childrenConfig.qtKey,
                                                    Name = config.children[i].__vModel__,
                                                    DictionaryType = childrenConfig.dataType == "dictionary" ? childrenConfig.dictionaryType : (childrenConfig.dataType == "dynamic" ? childrenConfig.propsUrl : null),
                                                    DataType = childrenConfig.dataType,
                                                    IsStatic = true,
                                                    IsIndex = false,
                                                    IsProps = true,
                                                    IsChildren = true,
                                                    Props = config.children[i].props.props.ToJsonString(CommonConst.options),
                                                    QueryProps = GetQueryPropsModel(config.children[i].props.props).ToJsonString(CommonConst.options),
                                                    Content = GetCodeGenConvIndexListControlOption(config.children[i].__vModel__, config.children[i].options.ToObject<List<Dictionary<string, object>>>())
                                                });
                                                break;
                                            default:
                                                list.Add(new CodeGenConvIndexListControlOptionDesign()
                                                {
                                                    qtKey = childrenConfig.qtKey,
                                                    Name = config.children[i].__vModel__,
                                                    DictionaryType = childrenConfig.dataType == "dictionary" ? childrenConfig.dictionaryType : (childrenConfig.dataType == "dynamic" ? childrenConfig.propsUrl : null),
                                                    DataType = childrenConfig.dataType,
                                                    IsStatic = false,
                                                    IsIndex = false,
                                                    IsProps = true,
                                                    IsChildren = true,
                                                    Props = config.children[i].props.props.ToJsonString(CommonConst.options),
                                                    QueryProps = GetQueryPropsModel(config.children[i].props.props).ToJsonString(CommonConst.options),
                                                    Content = string.Format("{0}Options : [],", config.children[i].__vModel__)
                                                });
                                                break;
                                        }
                                    }

                                    break;
                                case QtKeyConst.POPUPSELECT:
                                case QtKeyConst.RELATIONFORM:
                                    {
                                        list.Add(new CodeGenConvIndexListControlOptionDesign()
                                        {
                                            qtKey = childrenConfig.qtKey,
                                            Name = config.children[i].__vModel__,
                                            DictionaryType = null,
                                            DataType = null,
                                            IsStatic = true,
                                            IsIndex = false,
                                            IsProps = false,
                                            Props = null,
                                            IsChildren = false,
                                            Content = $"{config.children[i].__vModel__}Options : {config.children[i].columnOptions.ToJsonString(CommonConst.options)},"
                                        });
                                    }

                                    break;
                            }
                        }
                    }

                    break;
                case QtKeyConst.COLLAPSE:
                    {
                        StringBuilder title = new StringBuilder("[");
                        StringBuilder activeList = new StringBuilder("[");
                        foreach (var children in config.children)
                        {
                            title.AppendFormat("{{title:'{0}'}},", children.title);
                            activeList.AppendFormat("'{0}',", children.name);
                            list.AddRange(FormControlProps(children.__config__.children, type));
                        }

                        title.Remove(title.Length - 1, 1);
                        activeList.Remove(activeList.Length - 1, 1);
                        title.Append("]");
                        activeList.Append("]");
                        list.Add(new CodeGenConvIndexListControlOptionDesign()
                        {
                            qtKey = config.qtKey,
                            Name = "active" + active++,
                            IsStatic = true,
                            IsIndex = false,
                            IsProps = false,
                            IsChildren = false,
                            Content = activeList.ToString(),
                            Title = title.ToString()
                        });
                    }

                    break;
                case QtKeyConst.TAB:
                    {
                        StringBuilder title = new StringBuilder("[");
                        foreach (var children in config.children)
                        {
                            title.AppendFormat("{{title:'{0}'}},", children.title);
                            list.AddRange(FormControlProps(children.__config__.children, type));
                        }

                        title.Remove(title.Length - 1, 1);
                        title.Append("]");
                        list.Add(new CodeGenConvIndexListControlOptionDesign()
                        {
                            qtKey = config.qtKey,
                            Name = "active" + active++,
                            IsStatic = true,
                            IsIndex = false,
                            IsProps = false,
                            IsChildren = false,
                            Content = config.active.ToString(),
                            Title = title.ToString()
                        });
                    }

                    break;
                case QtKeyConst.GROUPTITLE:
                case QtKeyConst.DIVIDER:
                case QtKeyConst.QTTEXT:
                    break;
                default:
                    {
                        switch (config.qtKey)
                        {
                            case QtKeyConst.POPUPSELECT:
                            case QtKeyConst.RELATIONFORM:
                                {
                                    list.Add(new CodeGenConvIndexListControlOptionDesign()
                                    {
                                        qtKey = config.qtKey,
                                        Name = item.__vModel__,
                                        DictionaryType = null,
                                        DataType = null,
                                        IsStatic = true,
                                        IsIndex = false,
                                        IsProps = false,
                                        Props = null,
                                        IsChildren = false,
                                        Content = string.Format("{0}Options : {1},", item.__vModel__, item.columnOptions.ToJsonString(CommonConst.options))
                                    });
                                }

                                break;
                            case QtKeyConst.CHECKBOX:
                            case QtKeyConst.SELECT:
                            case QtKeyConst.RADIO:
                                {
                                    switch (config.dataType)
                                    {
                                        case "static":
                                            list.Add(new CodeGenConvIndexListControlOptionDesign()
                                            {
                                                qtKey = config.qtKey,
                                                Name = item.__vModel__,
                                                DictionaryType = config.dataType == "dictionary" ? config.dictionaryType : (config.dataType == "dynamic" ? config.propsUrl : null),
                                                DataType = config.dataType,
                                                IsStatic = true,
                                                IsIndex = true,
                                                IsProps = type == 5 || type == 3 ? true : false,
                                                Props = string.Format("{{'label':'{0}','value':'{1}'}}", config.props.label, config.props.value),
                                                QueryProps = GetQueryPropsModel(config.props).ToJsonString(CommonConst.options),
                                                IsChildren = false,
                                                Content = GetCodeGenConvIndexListControlOption(item.__vModel__, item.__slot__.options)
                                            });
                                            break;
                                        default:
                                            list.Add(new CodeGenConvIndexListControlOptionDesign()
                                            {
                                                qtKey = config.qtKey,
                                                Name = item.__vModel__,
                                                DictionaryType = config.dataType == "dictionary" ? config.dictionaryType : (config.dataType == "dynamic" ? config.propsUrl : null),
                                                DataType = config.dataType,
                                                IsStatic = false,
                                                IsIndex = true,
                                                IsProps = type == 5 || type == 3 ? true : false,
                                                QueryProps = GetQueryPropsModel(config.props).ToJsonString(CommonConst.options),
                                                Props = $"{{'label':'{config.props.label}','value':'{config.props.value}'}}",
                                                IsChildren = false,
                                                Content = string.Format("{0}Options : [],", item.__vModel__)
                                            });
                                            break;
                                    }
                                }

                                break;
                            case QtKeyConst.TREESELECT:
                            case QtKeyConst.CASCADER:
                                {
                                    switch (config.dataType)
                                    {
                                        case "static":
                                            list.Add(new CodeGenConvIndexListControlOptionDesign()
                                            {
                                                qtKey = config.qtKey,
                                                Name = item.__vModel__,
                                                DictionaryType = config.dataType == "dictionary" ? config.dictionaryType : (config.dataType == "dynamic" ? config.propsUrl : null),
                                                DataType = config.dataType,
                                                IsStatic = true,
                                                IsIndex = true,
                                                IsProps = true,
                                                IsChildren = false,
                                                Props = item.props.props.ToJsonString(CommonConst.options),
                                                QueryProps = GetQueryPropsModel(item.props.props).ToJsonString(CommonConst.options),
                                                Content = GetCodeGenConvIndexListControlOption(item.__vModel__, item.options.ToObject<List<Dictionary<string, object>>>())
                                            });
                                            break;
                                        default:
                                            list.Add(new CodeGenConvIndexListControlOptionDesign()
                                            {
                                                qtKey = config.qtKey,
                                                Name = item.__vModel__,
                                                DictionaryType = config.dataType == "dictionary" ? config.dictionaryType : (config.dataType == "dynamic" ? config.propsUrl : null),
                                                DataType = config.dataType,
                                                IsStatic = false,
                                                IsIndex = true,
                                                IsProps = true,
                                                IsChildren = false,
                                                Props = item.props.props.ToJsonString(CommonConst.options),
                                                QueryProps = GetQueryPropsModel(item.props.props).ToJsonString(CommonConst.options),
                                                Content = string.Format("{0}Options : [],", item.__vModel__)
                                            });
                                            break;
                                    }
                                }

                                break;
                        }
                    }

                    break;
            }
        }

        return list;
    }

    /// <summary>
    /// 表单脚本设计.
    /// </summary>
    /// <param name="genModel">生成模式.</param>
    /// <param name="fieldList">组件列表.</param>
    /// <param name="tableColumns">表真实字段.</param>
    /// <returns></returns>
    public static List<FormScriptDesignModel> FormScriptDesign(string genModel, List<FieldsModel> fieldList, List<TableColumnConfigModel> tableColumns)
    {
        var formScript = new List<FormScriptDesignModel>();
        foreach (FieldsModel item in fieldList)
        {
            var config = item.__config__;
            switch (config.qtKey)
            {
                case QtKeyConst.TABLE:
                    {
                        var childrenFormScript = new List<FormScriptDesignModel>();
                        foreach (var children in config.children)
                        {
                            var childrenConfig = children.__config__;
                            switch (childrenConfig.qtKey)
                            {
                                case QtKeyConst.RELATIONFORMATTR:
                                case QtKeyConst.POPUPATTR:
                                    break;
                                case QtKeyConst.SWITCH:
                                    {
                                        childrenFormScript.Add(new FormScriptDesignModel()
                                        {
                                            Name = children.__vModel__,
                                            OriginalName = children.__vModel__,
                                            qtKey = childrenConfig.qtKey,
                                            DataType = childrenConfig.dataType,
                                            DictionaryType = childrenConfig.dataType == "dictionary" ? childrenConfig.dictionaryType : (childrenConfig.dataType == "dynamic" ? childrenConfig.propsUrl : null),
                                            Format = children.format,
                                            Multiple = childrenConfig.qtKey == QtKeyConst.CASCADER ? children.props.props.multiple : children.multiple,
                                            BillRule = childrenConfig.rule,
                                            Required = childrenConfig.required,
                                            Placeholder = childrenConfig.label,
                                            Range = children.range,
                                            RegList = childrenConfig.regList,
                                            DefaultValue = childrenConfig.defaultValue.ParseToBool(),
                                            Trigger = string.IsNullOrEmpty(childrenConfig.trigger.ToJsonString()) ? "blur" : (childrenConfig.trigger.ToJsonString().Contains("[") ? childrenConfig.trigger.ToJsonString() : childrenConfig.trigger.ToString()),
                                            ChildrenList = null,
                                            IsSummary = item.showSummary && item.summaryField.Find(it => it.Equals(children.__vModel__)) != null ? true : false,
                                        });
                                    }

                                    break;
                                default:
                                    {
                                        childrenFormScript.Add(new FormScriptDesignModel()
                                        {
                                            Name = children.__vModel__,
                                            OriginalName = children.__vModel__,
                                            qtKey = childrenConfig.qtKey,
                                            DataType = childrenConfig.dataType,
                                            DictionaryType = childrenConfig.dataType == "dictionary" ? childrenConfig.dictionaryType : (childrenConfig.dataType == "dynamic" ? childrenConfig.propsUrl : null),
                                            Format = children.format,
                                            Multiple = childrenConfig.qtKey == QtKeyConst.CASCADER ? children.props.props.multiple : children.multiple,
                                            BillRule = childrenConfig.rule,
                                            Required = childrenConfig.required,
                                            Placeholder = childrenConfig.label,
                                            Range = children.range,
                                            RegList = childrenConfig.regList,
                                            DefaultValue = childrenConfig.defaultValue?.ToString(),
                                            Trigger = string.IsNullOrEmpty(childrenConfig.trigger.ToJsonString()) ? "blur" : (childrenConfig.trigger.ToJsonString().Contains("[") ? childrenConfig.trigger.ToJsonString() : childrenConfig.trigger.ToString()),
                                            ChildrenList = null,
                                            IsSummary = item.showSummary && item.summaryField.Any(it => it.Equals(children.__vModel__)) ? true : false,
                                        });
                                    }

                                    break;
                            }
                        }

                        formScript.Add(new FormScriptDesignModel()
                        {
                            Name = config.tableName.ParseToPascalCase(),
                            Placeholder = config.label,
                            OriginalName = item.__vModel__,
                            qtKey = config.qtKey,
                            ChildrenList = childrenFormScript,
                            Required = childrenFormScript.Any(it => it.Required.Equals(true)),
                            ShowSummary = item.showSummary,
                            SummaryField = item.summaryField.ToJsonString(),
                        });
                    }

                    break;
                case QtKeyConst.RELATIONFORMATTR:
                case QtKeyConst.POPUPATTR:
                    break;
                case QtKeyConst.SWITCH:
                    {
                        var originalName = string.Empty;
                        if (item.__vModel__.Contains("_qt_"))
                        {
                            var auxiliaryTableName = item.__vModel__.Matches(@"qt_(?<table>[\s\S]*?)_qt_", "table").Last();
                            var column = item.__vModel__.Replace(item.__vModel__.Matches(@"qt_(?<table>[\s\S]*?)_qt_").Last(), string.Empty);
                            var columns = tableColumns.Find(it => it.LowerColumnName.Equals(column) && (bool)it.TableName?.Equals(auxiliaryTableName) && it.IsAuxiliary.Equals(true));
                            if (columns != null)
                                originalName = columns.OriginalColumnName;
                        }
                        else
                        {
                            var columns = tableColumns.Find(it => it.LowerColumnName.Equals(item.__vModel__));
                            if (columns != null)
                                originalName = columns.OriginalColumnName;
                        }

                        formScript.Add(new FormScriptDesignModel()
                        {
                            Name = item.__vModel__,
                            OriginalName = originalName,
                            qtKey = config.qtKey,
                            DataType = config.dataType,
                            DictionaryType = config.dataType == "dictionary" ? config.dictionaryType : (config.dataType == "dynamic" ? config.propsUrl : null),
                            Format = item.format,
                            Multiple = item.multiple,
                            BillRule = config.rule,
                            Required = config.required,
                            Placeholder = config.label,
                            Range = item.range,
                            RegList = config.regList,
                            DefaultValue = config.defaultValue.ParseToBool(),
                            Trigger = string.IsNullOrEmpty(config.trigger.ToJsonString()) ? "blur" : (config.trigger.ToJsonString().Contains("[") ? config.trigger.ToJsonString() : config.trigger.ToString()),
                            ChildrenList = null
                        });
                    }

                    break;
                default:
                    {
                        var originalName = string.Empty;
                        if (item.__vModel__.Contains("_qt_"))
                        {
                            var auxiliaryTableName = item.__vModel__.Matches(@"qt_(?<table>[\s\S]*?)_qt_", "table").Last();
                            var column = item.__vModel__.Replace(item.__vModel__.Matches(@"qt_(?<table>[\s\S]*?)_qt_").Last(), string.Empty);
                            var columns = tableColumns.Find(it => it.LowerColumnName.Equals(column) && it.IsAuxiliary.Equals(true) && (bool)it.TableName?.Equals(auxiliaryTableName));
                            if (columns != null)
                                originalName = columns.OriginalColumnName;
                        }
                        else
                        {
                            var columns = tableColumns.Find(it => it.LowerColumnName.Equals(item.__vModel__));
                            if (columns != null)
                                originalName = columns.OriginalColumnName;
                        }

                        formScript.Add(new FormScriptDesignModel()
                        {
                            Name = item.__vModel__,
                            OriginalName = originalName,
                            qtKey = config.qtKey,
                            DataType = config.dataType,
                            DictionaryType = config.dataType == "dictionary" ? config.dictionaryType : (config.dataType == "dynamic" ? config.propsUrl : null),
                            Format = item.format,
                            Multiple = config.qtKey == QtKeyConst.CASCADER ? item.props.props.multiple : item.multiple,
                            BillRule = config.rule,
                            Required = config.required,
                            Placeholder = config.label,
                            Range = item.range,
                            RegList = config.regList,
                            DefaultValue = config.defaultValue?.ToString(),
                            Trigger = config?.trigger?.ToJsonString() ?? "blur",
                            ChildrenList = null
                        });
                    }

                    break;
            }
        }

        return formScript;
    }

    /// <summary>
    /// 获取常规index列表控件Option.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    private static string GetCodeGenConvIndexListControlOption(string name, List<Dictionary<string, object>> options)
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendFormat("{0}Options:", name);
        sb.Append("[");
        foreach (var valueItem in options.ToObject<List<Dictionary<string, object>>>())
        {
            sb.Append("{");
            foreach (var items in valueItem)
            {
                sb.AppendFormat("'{0}':{1},", items.Key, items.Value.ToJsonString());
            }

            sb = new StringBuilder(sb.ToString().TrimEnd(','));
            sb.Append("},");
        }

        sb = new StringBuilder(sb.ToString().TrimEnd(','));
        sb.Append("],");

        return sb.ToString();
    }

    /// <summary>
    /// 查询时将多选关闭.
    /// </summary>
    /// <param name="propsModel"></param>
    /// <returns></returns>
    private static PropsBeanModel GetQueryPropsModel(PropsBeanModel propsModel)
    {
        var model = new PropsBeanModel();
        if (propsModel != null && propsModel.multiple)
        {
            model = propsModel;
            model.multiple = false;
        }
        else if (propsModel != null)
        {
            model = propsModel;
        }

        return model;
    }
}