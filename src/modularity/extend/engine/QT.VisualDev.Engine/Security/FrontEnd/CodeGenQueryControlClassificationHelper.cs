using QT.Common.Const;

namespace QT.VisualDev.Engine.Security;

/// <summary>
/// 代码生成查询控件归类帮助类.
/// </summary>
public class CodeGenQueryControlClassificationHelper
{
    /// <summary>
    /// 列表查询控件.
    /// </summary>
    /// <param name="type">1-Web设计,2-App设计,3-流程表单,4-Web表单,5-App表单.</param>
    /// <returns></returns>
    public static Dictionary<string, List<string>> ListQueryControl(int type)
    {
        Dictionary<string, List<string>> listQueryControl = new Dictionary<string, List<string>>();
        switch (type)
        {
            case 4:
                {
                    var useInputList = new List<string>();
                    useInputList.Add(QtKeyConst.COMINPUT);
                    useInputList.Add(QtKeyConst.TEXTAREA);
                    useInputList.Add(QtKeyConst.QTTEXT);
                    useInputList.Add(QtKeyConst.BILLRULE);
                    listQueryControl["inputList"] = useInputList;

                    var useDateList = new List<string>();
                    useDateList.Add(QtKeyConst.CREATETIME);
                    useDateList.Add(QtKeyConst.MODIFYTIME);
                    listQueryControl["dateList"] = useDateList;

                    var useSelectList = new List<string>();
                    useSelectList.Add(QtKeyConst.SELECT);
                    useSelectList.Add(QtKeyConst.RADIO);
                    useSelectList.Add("checkbox");
                    listQueryControl["selectList"] = useSelectList;

                    var timePickerList = new List<string>();
                    timePickerList.Add(QtKeyConst.TIME);
                    listQueryControl["timePickerList"] = timePickerList;

                    var numRangeList = new List<string>();
                    numRangeList.Add(QtKeyConst.NUMINPUT);
                    numRangeList.Add(QtKeyConst.CALCULATE);
                    listQueryControl["numRangeList"] = numRangeList;

                    var datePickerList = new List<string>();
                    datePickerList.Add(QtKeyConst.DATE);
                    listQueryControl["datePickerList"] = datePickerList;

                    var userSelectList = new List<string>();
                    userSelectList.Add(QtKeyConst.CREATEUSER);
                    userSelectList.Add(QtKeyConst.MODIFYUSER);
                    userSelectList.Add(QtKeyConst.USERSELECT);
                    listQueryControl["userSelectList"] = userSelectList;

                    var comSelectList = new List<string>();
                    comSelectList.Add(QtKeyConst.COMSELECT);
                    comSelectList.Add(QtKeyConst.CURRORGANIZE);
                    listQueryControl["comSelectList"] = comSelectList;

                    var depSelectList = new List<string>();
                    depSelectList.Add(QtKeyConst.CURRDEPT);
                    depSelectList.Add(QtKeyConst.DEPSELECT);
                    listQueryControl["depSelectList"] = depSelectList;

                    var posSelectList = new List<string>();
                    posSelectList.Add(QtKeyConst.CURRPOSITION);
                    posSelectList.Add(QtKeyConst.POSSELECT);
                    listQueryControl["posSelectList"] = posSelectList;

                    var useCascaderList = new List<string>();
                    useCascaderList.Add(QtKeyConst.CASCADER);
                    listQueryControl["useCascaderList"] = useCascaderList;

                    var jNPFAddressList = new List<string>();
                    jNPFAddressList.Add(QtKeyConst.ADDRESS);
                    listQueryControl["QTAddressList"] = jNPFAddressList;

                    var treeSelectList = new List<string>();
                    treeSelectList.Add(QtKeyConst.TREESELECT);
                    listQueryControl["treeSelectList"] = treeSelectList;
                }

                break;
            case 5:
                {
                    var inputList = new List<string>();
                    inputList.Add(QtKeyConst.COMINPUT);
                    inputList.Add(QtKeyConst.TEXTAREA);
                    inputList.Add(QtKeyConst.QTTEXT);
                    inputList.Add(QtKeyConst.BILLRULE);
                    inputList.Add(QtKeyConst.CALCULATE);
                    listQueryControl["input"] = inputList;

                    var numRangeList = new List<string>();
                    numRangeList.Add(QtKeyConst.NUMINPUT);
                    listQueryControl["numRange"] = numRangeList;

                    var switchList = new List<string>();
                    switchList.Add(QtKeyConst.SWITCH);
                    listQueryControl["switch"] = switchList;

                    var selectList = new List<string>();
                    selectList.Add(QtKeyConst.RADIO);
                    selectList.Add(QtKeyConst.CHECKBOX);
                    selectList.Add(QtKeyConst.SELECT);
                    listQueryControl["select"] = selectList;

                    var cascaderList = new List<string>();
                    cascaderList.Add(QtKeyConst.CASCADER);
                    listQueryControl["cascader"] = cascaderList;

                    var timeList = new List<string>();
                    timeList.Add(QtKeyConst.TIME);
                    listQueryControl["time"] = timeList;

                    var dateList = new List<string>();
                    dateList.Add(QtKeyConst.DATE);
                    dateList.Add(QtKeyConst.CREATETIME);
                    dateList.Add(QtKeyConst.MODIFYTIME);
                    listQueryControl["date"] = dateList;

                    var comSelectList = new List<string>();
                    comSelectList.Add(QtKeyConst.COMSELECT);
                    listQueryControl["comSelect"] = comSelectList;

                    var depSelectList = new List<string>();
                    depSelectList.Add(QtKeyConst.DEPSELECT);
                    depSelectList.Add(QtKeyConst.CURRDEPT);
                    depSelectList.Add(QtKeyConst.CURRORGANIZE);
                    listQueryControl["depSelect"] = depSelectList;

                    var posSelectList = new List<string>();
                    posSelectList.Add(QtKeyConst.POSSELECT);
                    posSelectList.Add(QtKeyConst.CURRPOSITION);
                    listQueryControl["posSelect"] = posSelectList;

                    var userSelectList = new List<string>();
                    userSelectList.Add(QtKeyConst.USERSELECT);
                    userSelectList.Add(QtKeyConst.CREATEUSER);
                    userSelectList.Add(QtKeyConst.MODIFYUSER);
                    listQueryControl["userSelect"] = userSelectList;

                    var treeSelectList = new List<string>();
                    treeSelectList.Add(QtKeyConst.TREESELECT);
                    listQueryControl["treeSelect"] = treeSelectList;

                    var addressList = new List<string>();
                    addressList.Add(QtKeyConst.ADDRESS);
                    listQueryControl["address"] = addressList;
                }

                break;
        }

        return listQueryControl;
    }

    /// <summary>
    /// 列表列控件.
    /// </summary>
    /// <returns></returns>
    public static Dictionary<string, List<string>> ListColumnControls()
    {
        Dictionary<string, List<string>> listColumnControlsType = new Dictionary<string, List<string>>();
        var columnList = new List<string>();
        columnList.Add("select");
        columnList.Add("radio");
        columnList.Add("checkbox");
        columnList.Add("treeSelect");
        columnList.Add("cascader");
        listColumnControlsType["columnList"] = columnList;

        return listColumnControlsType;
    }
}
