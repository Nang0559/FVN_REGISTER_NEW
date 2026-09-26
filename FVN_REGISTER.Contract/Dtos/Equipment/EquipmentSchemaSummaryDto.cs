namespace FVN_REGISTER.Contract.Dtos.Equipment;

/// <summary>
/// Thông tin tóm tắt của một Equipment Schema.
/// DTO dùng chung giữa Application, API và Web.
/// </summary>
public sealed class EquipmentSchemaSummaryDto
{
    /// <summary>
    /// Id của schema.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Mã phòng ban sở hữu schema.
    /// </summary>
    public string DepartmentCode { get; init; } = string.Empty;

    /// <summary>
    /// Tên phòng ban.
    /// </summary>
    public string DepartmentName { get; init; } = string.Empty;

    /// <summary>
    /// Tên schema.
    /// </summary>
    public string SchemaName { get; init; } = string.Empty;

    /// <summary>
    /// Phiên bản schema.
    /// </summary>
    public int Version { get; init; }

    /// <summary>
    /// Trạng thái schema: Draft, Active hoặc Inactive.
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Số lượng trường dữ liệu của schema.
    /// </summary>
    public int FieldCount { get; init; }

    /// <summary>
    /// Id người tạo schema.
    /// </summary>
    public int CreatedByUserId { get; init; }

    /// <summary>
    /// Tên người tạo schema.
    /// </summary>
    public string CreatedByUserName { get; init; } = string.Empty;

    /// <summary>
    /// Thời điểm tạo schema.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Id người cập nhật cuối cùng.
    /// </summary>
    public int? UpdatedByUserId { get; init; }

    /// <summary>
    /// Tên người cập nhật cuối cùng.
    /// </summary>
    public string? UpdatedByUserName { get; init; }

    /// <summary>
    /// Thời điểm cập nhật cuối cùng.
    /// </summary>
    public DateTime? UpdatedAt { get; init; }

    /// <summary>
    /// Cho biết người dùng hiện tại có quyền sửa schema hay không.
    /// Đây chỉ là thông tin phục vụ UI; server vẫn phải kiểm tra quyền thực tế.
    /// </summary>
    public bool CanEdit { get; init; }

    /// <summary>
    /// Cho biết người dùng hiện tại có quyền nhân bản schema hay không.
    /// Đây chỉ là thông tin phục vụ UI; server vẫn phải kiểm tra quyền thực tế.
    /// </summary>
    public bool CanClone { get; init; }
}
