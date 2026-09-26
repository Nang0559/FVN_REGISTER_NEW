# Mô hình dữ liệu Endpoint không dùng AD

## Quan hệ

```text
F03Employees
    │
    │ người sử dụng nghiệp vụ
    ▼
F03EquipmentAssets
    │
    │ 0..1 endpoint kỹ thuật
    ▼
F03EndpointDevices
    │
    ├── F03EndpointCredentials
    ├── F03EndpointSoftwareInventory
    ├── F03EndpointServiceInventory
    ├── F03EndpointComplianceResults
    ├── F03EndpointComplianceExceptions
    └── F03EndpointAlerts
```

## Phân biệt 3 identity

### Employee identity
Do FVN HRM quản lý. Dùng `EmployeeCode`.

### Asset identity
Do Equipment quản lý. Dùng AssetCode/Id/Serial theo mô hình hiện có.

### Endpoint identity
Do FVN Endpoint Registry quản lý. Dùng `DeviceKey` và credential riêng.

Ba identity không được gộp thành một khóa.

## Thay người sử dụng

Không đổi DeviceKey khi chuyển laptop từ nhân viên A sang B.

```text
Asset LT-001
  A -> B

Endpoint EP-001 giữ nguyên
Employee association của Equipment thay đổi qua workflow Equipment.
```

## Cài lại Windows

AgentInstallationId có thể thay đổi. Server phải đối chiếu credential + hardware identity + Equipment Asset trước khi tạo endpoint mới. Nếu không đủ bằng chứng thì tạo trạng thái cần xác minh, không tự động merge.

## Thay mainboard

Hardware UUID/serial có thể thay đổi. Không tự động merge. IT phải xác minh và thực hiện workflow thay đổi phần cứng/asset.

## Đổi tên máy

ComputerName chỉ là thuộc tính. Không tạo endpoint mới.

## Máy không có Equipment Asset

Cho phép endpoint tồn tại ở trạng thái `UnmanagedDevice`/`PendingRegistration` nếu discovery hoặc Agent xuất hiện trước khi IT đăng ký asset. Sau khi xác minh, bind endpoint vào Equipment Asset.
