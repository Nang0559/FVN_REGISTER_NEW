namespace FVN_REGISTER.Contract.Dtos.Schema;

/// <summary>
/// Generic definition used by configurable table/import features.
/// Equipment, checklist, attendance, payroll and future modules can reuse this contract.
/// </summary>
public sealed class DataSchemaDefinitionRequest
{
    public string ScopeCode { get; set; } = string.Empty;
    public string SchemaName { get; set; } = string.Empty;
    public string SchemaKind { get; set; } = "Custom";
    public string? SourceFileName { get; set; }
    public bool CreatedFromExcel { get; set; }
    public List<DataSchemaFieldDefinitionRequest> Fields { get; set; } = new();
}

public sealed class DataSchemaFieldDefinitionRequest
{
    public string FieldKey { get; set; } = string.Empty;
    public string FieldLabel { get; set; } = string.Empty;
    public string DataType { get; set; } = "Text";
    public bool IsRequired { get; set; }
    public bool IsImportable { get; set; } = true;
    public bool IsSearchable { get; set; }
    public bool IsActiveField { get; set; } = true;
    public int DisplayOrder { get; set; }
    public int? MaxLength { get; set; }
    public string? DefaultValue { get; set; }
    public string? OptionsJson { get; set; }
}

public static class DataSchemaKinds
{
    public const string Equipment = "Equipment";
    public const string Checklist = "Checklist";
    public const string Attendance = "Attendance";
    public const string Payroll = "Payroll";
    public const string Custom = "Custom";
}
