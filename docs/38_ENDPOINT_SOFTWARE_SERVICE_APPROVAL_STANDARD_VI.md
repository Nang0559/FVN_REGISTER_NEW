# FVN-REGISTER — Chuẩn hóa Governance Software / Windows Service / Endpoint

## 1. Nguyên tắc bắt buộc

Endpoint Agent chỉ cung cấp dữ liệu thực tế. Agent không được:
- tự thêm software/service vào allowlist;
- tự thay đổi Equipment owner;
- tự đổi DeviceKey;
- tự phê duyệt policy;
- nhận và thực thi arbitrary command từ server.

Approval phải dùng Approval Engine hiện tại của FVN-REGISTER. Không tạo Approval Engine riêng cho Endpoint.

## 2. Ba lớp dữ liệu

### Inventory thực tế
Nguồn duy nhất: Windows Agent.

```text
Endpoint
  ├── Installed Software
  └── Windows Services
```

### Catalog/Allowlist đã có hiệu lực
Do IT Security quản lý và chỉ active sau Approval.

```text
Software Catalog
Service Catalog
```

### Request nghiệp vụ
Do người dùng/IT tạo.

```text
Software Install Request
Software Catalog Change Request
Service Catalog Change Request
Exception Request
```

Ba lớp không được ghi đè lẫn nhau.

## 3. Software đã có trong Allowlist

```text
Employee
  -> Tạo yêu cầu cài đặt
  -> FVN load Active Software Catalog
  -> Chọn phần mềm
  -> FVN chụp AllowlistVersion + software rule
  -> Approval Engine hiện tại
  -> Approved
  -> thực hiện cài đặt theo quy trình IT hiện hành
  -> Agent phát hiện phần mềm
  -> Compliance xác nhận Installed
```

`Approved` của request không đồng nghĩa `Installed`.

## 4. Software chưa có trong Allowlist

```text
Employee
  -> New Software Request
  -> IT Security Review
       ├─ Reject: không an toàn/không được phép
       └─ Accept for catalog proposal
              -> Catalog Change Request
              -> Snapshot trước/sau
              -> Approval Engine hiện tại
              -> Approved
              -> Active Catalog version mới
              -> Install Request tiếp tục
```

IT Security Review chỉ là đánh giá kỹ thuật/an toàn; không được biến nó thành bypass của Approval.

## 5. Phần mềm phát hiện từ Agent nhưng chưa có Allowlist

```text
Agent Inventory
    -> software không match Active Catalog
    -> Compliance = NonCompliant
    -> Alert IT
```

IT có thể:
- gỡ/khắc phục;
- xác nhận exception;
- đề xuất thêm vào catalog.

Nếu đề xuất thêm catalog thì vẫn phải đi qua Approval Engine.

Không được auto-promote inventory thành allowed software.

## 6. Service Governance

Service dùng cùng nguyên tắc nhưng catalog riêng.

Mỗi rule nên có tối thiểu:
- ServiceName;
- DisplayName;
- Publisher nếu thu thập được;
- AllowedStartMode;
- AllowedState;
- Scope;
- EffectiveFrom/To;
- Status;
- ApprovalCaseId;
- Version.

Service lạ hoặc state/start-mode sai phải sinh Compliance finding/Alert tương ứng.

## 7. Snapshot chuẩn

Mỗi Approval thay đổi catalog phải lưu immutable snapshot:

```text
RequestId
RequestType
CatalogVersionBefore
CatalogVersionAfter
Scope
ChangedItems
OldValue
NewValue
Reason
SecurityReview
Requester
Department
CreatedAtUtc
Evidence references
```

Approver phải xem được trước/sau, không chỉ thấy tên request.

Install Request phải snapshot:

```text
RequestId
Endpoint/Equipment
Employee
SoftwareCatalogId
SoftwareVersionRule
CatalogVersion
Purpose
ITReviewResult
Evidence
CreatedAtUtc
```

Sau khi submit, thay đổi catalog không được làm thay đổi nội dung snapshot đã trình duyệt.

## 8. State machine

### Catalog Change

```text
Draft
 -> SecurityReview
 -> PendingApproval
 -> Approved
 -> Active
```

Các trạng thái kết thúc:

```text
Rejected
Cancelled
Expired
```

Không cho `Draft -> Active`.

### Install Request

```text
Draft
 -> PendingApproval
 -> Approved
 -> PendingInstallation
 -> Installed
 -> ComplianceVerified
```

Kết thúc lỗi:

```text
Rejected
Cancelled
InstallationFailed
Expired
```

Không cho `Approved -> ComplianceVerified` nếu Agent chưa chứng minh software tồn tại trên đúng endpoint.

## 9. Compliance states

```text
Compliant
NonCompliant
Unknown
AgentOffline
UnmanagedDevice
PendingReview
```

`Unknown` không tự động chuyển thành `NonCompliant` chỉ vì Agent chưa gửi dữ liệu.

## 10. Identity change

ComputerName chỉ là thuộc tính.

Khi đổi tên:

```text
OldComputerName
NewComputerName
DetectedAtUtc
DeviceKey
AgentInstallationId
HardwareUuid
```

Tạo `DEVICE_NAME_CHANGED` để IT xem xét.

Nếu HardwareUuid/Serial bất thường:

```text
IdentityStatus = PendingReview
```

Không tự động merge sang Endpoint khác.

## 11. Phân quyền

### Employee
- Endpoint của mình: xem inventory/compliance.
- Tạo Install Request.
- Tạo New Software Request.
- Không sửa catalog.
- Không resolve security alert.

### IT Operator
- Xem endpoint/inventory.
- Review alert.
- Provision/rotate/revoke nếu được giao.
- Không tự approve catalog nếu không có capability.

### IT Security
- Quản lý Software/Service Catalog draft.
- Security Review.
- Tạo Catalog Change Request.
- Quản lý Exception theo quyền.

### Approver
- Chỉ approve/reject theo Approval Policy hiện tại.
- Không có quyền sửa snapshot sau khi submit.

### Superadmin
- Quản trị capability/role/function.
- Có thể provision/revoke endpoint theo policy hệ thống.

Capability phải được đăng ký vào Security Center hiện tại; không hard-code role name trong Endpoint service.

## 12. Security Center

Tên capability đề xuất chuẩn:

```text
Endpoint.View
Endpoint.Inventory.View
Endpoint.Compliance.View
Endpoint.Alert.View
Endpoint.Alert.Resolve
Endpoint.Provision
Endpoint.Credential.Rotate
Endpoint.Credential.Revoke
Endpoint.SoftwareCatalog.View
Endpoint.SoftwareCatalog.Manage
Endpoint.SoftwareSecurityReview
Endpoint.SoftwarePolicy.Submit
Endpoint.SoftwarePolicy.Approve
Endpoint.ServiceCatalog.View
Endpoint.ServiceCatalog.Manage
Endpoint.ServiceSecurityReview
Endpoint.ServicePolicy.Submit
Endpoint.ServicePolicy.Approve
Endpoint.InstallRequest.Create
Endpoint.InstallRequest.View
Endpoint.InstallRequest.Approve
Endpoint.Exception.Create
Endpoint.Exception.View
Endpoint.Exception.Approve
```

Không seed trùng FunctionCode; phải đi qua SecurityFunctionRegistry/registry reconciliation hiện tại.

## 13. Audit

Phải audit:
- provision/rotate/revoke credential;
- endpoint bind/unbind Equipment;
- catalog create/edit/submit/approve/reject/activate;
- security review;
- install request submit/approve/reject/install/verify;
- compliance status change;
- alert create/resolve;
- exception create/approve/expire/revoke.

## 14. Nguyên tắc versioning

Catalog active có version tăng tuần tự.

Ví dụ:

```text
v12
  Chrome allowed
  VSCode allowed

v13
  Chrome allowed
  VSCode allowed
  7-Zip allowed
```

Install Request tham chiếu `v12` nếu request được tạo trước khi v13 active. Approval snapshot giữ nguyên v12.

## 15. Không được tự động thêm software sau khi cài

Khi Agent thấy software mới:

```text
Inventory -> Compliance -> Alert
```

Nếu IT muốn cho phép:

```text
IT -> Catalog Change -> Approval -> Active
```

Đây là điều kiện bắt buộc để tránh biến inventory thành allowlist bypass.

## 16. Acceptance criteria

- Employee chỉ thấy phần mềm/service Active được phép trong Install Request.
- Software mới phải qua IT Security Review.
- Catalog Change phải qua Approval Engine hiện tại.
- Snapshot immutable.
- Approver thấy before/after và bằng chứng.
- Agent không tự approve hoặc mutate catalog.
- Software lạ sinh Compliance/Alert.
- Installed chỉ được xác nhận bởi Agent inventory.
- Đổi tên máy tạo identity history/alert.
- Credential xác định máy; payload không thể đổi DeviceKey.
- Không phụ thuộc AD.
