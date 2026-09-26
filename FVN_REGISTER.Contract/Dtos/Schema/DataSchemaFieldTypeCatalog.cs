namespace FVN_REGISTER.Contract.Dtos.Schema;

public sealed record DataSchemaFieldTypeOption(string Code, string Name);

public static class DataSchemaFieldTypeCatalog
{
    public static IReadOnlyList<DataSchemaFieldTypeOption> All { get; } = new[]
    {
        new DataSchemaFieldTypeOption("Text", "Văn bản"),
        new DataSchemaFieldTypeOption("Integer", "Số nguyên"),
        new DataSchemaFieldTypeOption("Decimal", "Số thập phân"),
        new DataSchemaFieldTypeOption("Currency", "Tiền tệ"),
        new DataSchemaFieldTypeOption("Date", "Ngày"),
        new DataSchemaFieldTypeOption("DateTime", "Ngày giờ"),
        new DataSchemaFieldTypeOption("Boolean", "Đúng / Sai"),
        new DataSchemaFieldTypeOption("Email", "Email"),
        new DataSchemaFieldTypeOption("Choice", "Danh sách lựa chọn")
    };

    public static bool IsSupported(string? code)
        => !string.IsNullOrWhiteSpace(code) && All.Any(x => string.Equals(x.Code, code.Trim(), StringComparison.OrdinalIgnoreCase));

    public static string GetName(string? code)
        => All.FirstOrDefault(x => string.Equals(x.Code, code?.Trim(), StringComparison.OrdinalIgnoreCase))?.Name ?? "Văn bản";
}
