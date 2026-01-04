namespace QT.Common.Dtos.DataBase;

public class FieldChangeDto
{
    public string tableName { get; set; }
    public string fieldName { get; set; }
    public string description { get; set; }

    public string oldValue { get; set; }
    public string newValue { get; set; }
}
