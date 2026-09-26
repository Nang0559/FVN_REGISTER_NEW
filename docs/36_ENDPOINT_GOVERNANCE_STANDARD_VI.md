# Chuẩn hóa Endpoint Governance — Software / Windows Service

## 1. Mục tiêu

FVN-REGISTER quản lý hai lớp dữ liệu độc lập nhưng liên kết:

1. **Inventory thực tế** từ Windows Agent.
2. **Danh mục được phép** đã qua IT Security Review và Approval.

Inventory không được tự động làm thay đổi Allowlist.

## 2. Quy trình phần mềm chuẩn

### 2.1 Phần mềm đã có trong danh mục Approved

```text
Người dùng
  ↓
Tạo yêu cầu cài đặt
  ↓
FVN tự load danh mục Software Approved
  ↓
Chọn phần mềm / phiên bản
  ↓
Snapshot Allowlist Version
  ↓
Approval cấp nghiệp vụ
  ↓
Approved
  ↓
Chờ Agent xác nhận đã cài
  ↓
Inventory thực tế
  ↓
Compliance
```

`Approved` của yêu cầu cài đặt không đồng nghĩa `Installed`. Chỉ inventory từ Agent mới xác nhận cài đặt thực tế.

### 2.2 Phần mềm chưa có trong danh mục

```text
Người dùng
  ↓
Yêu cầu phần mềm mới
  ↓
IT Security Review
  ├── Reject → kết thúc
  └── Approve review
          ↓
  Software Allowlist Change Request
          ↓
  Snapshot proposed version
          ↓
  Approval Engine hiện tại
          ↓
  Approved
          ↓
  Active Allowlist
          ↓
  Quay lại Install Request
          ↓
  Approval cấp nghiệp vụ
```

Không được tự động Active phần mềm chỉ vì IT đánh giá an toàn. IT Review và Business Approval là hai bước khác nhau.

## 3. Quy trình Windows Service

Windows Service sử dụng cùng nguyên tắc governance nhưng có catalog riêng:

```text
Service Catalog
  ↓
Security Review
  ↓
Approval
  ↓
Active Service Allowlist
  ↓
Agent Inventory
  ↓
Compliance
```

Không được dùng SoftwarePolicy để giả lập Windows Service Policy.

## 4. Approval snapshot

Mỗi yêu cầu gửi Approval phải chụp snapshot bất biến gồm tối thiểu:

- request type;
- request id;
- thiết bị / asset / employee nếu có;
- catalog id và catalog version;
- tên phần mềm/service;
- publisher;
- version rule;
- scope;
- action;
- lý do;
- IT Security Review;
- evidence/attachment nếu có;
- requester;
- thời điểm tạo.

Approver xem snapshot tại thời điểm yêu cầu, không đọc trực tiếp bản Allowlist mới nhất để thay đổi ý nghĩa của request cũ.

## 5. Versioning

Allowlist phải có khái niệm version logic.

Ví dụ:

```text
Version 10
  Chrome = Allow

Version 11
  Chrome = Allow
  7-Zip = Allow

Version 12
  Chrome = Allow
  7-Zip = Retired
  VS Code = Allow
```

Install Request phải ghi `AllowlistVersion` đã sử dụng.

## 6. Inventory và cảnh báo

### Software

```text
Approved Allowlist
       +
Actual Agent Inventory
       ↓
Compliance
```

Kết quả:

- `Compliant`: software phù hợp allowlist.
- `NonCompliant`: software bị Deny hoặc không thuộc allowlist bắt buộc.
- `Unknown`: chưa đủ dữ liệu/policy.
- `AgentOffline`: không có inventory trong ngưỡng.

### Device identity

ComputerName là thuộc tính có thể thay đổi, không phải DeviceKey.

Nếu phát hiện đổi:

```text
Old ComputerName
        ↓
New ComputerName
        ↓
Identity History
        ↓
DEVICE_NAME_CHANGED
        ↓
IT Review
```

Đổi tên không tự tạo endpoint mới.

SerialNumber/HW UUID thay đổi cũng phải tạo Identity Review; không tự động merge endpoint.

## 7. Phân quyền

Các capability chuẩn cần được đăng ký trong Security Center hiện tại:

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
Endpoint.SoftwarePolicy.Submit
Endpoint.SoftwarePolicy.Approve
Endpoint.SoftwareSecurityReview

Endpoint.ServiceCatalog.View
Endpoint.ServiceCatalog.Manage
Endpoint.ServicePolicy.Submit
Endpoint.ServicePolicy.Approve
Endpoint.ServiceSecurityReview

Endpoint.InstallRequest.Create
Endpoint.InstallRequest.View
Endpoint.InstallRequest.Approve

Endpoint.Exception.Create
Endpoint.Exception.View
Endpoint.Exception.Approve
```

Không hard-code quyền theo tên role. Quyền server-side phải kiểm tra capability và phạm vi dữ liệu.

## 8. Phân tách trách nhiệm

### Employee / Requester

- xem catalog được phép;
- tạo Install Request;
- xem request của mình;
- cung cấp mục đích/evidence.

### IT Operator

- xem inventory;
- xem alert;
- provision/revoke theo quyền được giao;
- thực hiện thao tác vận hành.

### IT Security

- Security Review phần mềm/service mới;
- đề xuất Add/Update/Retire Allowlist;
- xử lý vi phạm;
- tạo/đánh giá exception.

### Approver

- chỉ thực hiện Approval theo ApprovalPolicy hiện tại của FVN.

### Superadmin

- quản trị capability, operator assignment và credential governance; không thay thế business approval.

## 9. Không tạo Approval Engine thứ hai

Endpoint chỉ lưu `ApprovalCaseId` và snapshot request. Việc route, level, approver, approve/reject phải dùng Approval Engine hiện tại của FVN-REGISTER.

## 10. Không tự động tin Agent

Agent là nguồn bằng chứng inventory, không phải nguồn quyết định policy.

```text
Agent says Installed
      ≠
System says Allowed
```

Compliance Engine mới là nơi kết luận dựa trên policy hiệu lực.

## 11. Không tự động thêm phần mềm phát hiện được vào Allowlist

Nếu Agent phát hiện software lạ:

```text
Inventory
  ↓
NonCompliant / Review
  ↓
IT Review
  ↓
Nếu hợp lệ
  ↓
Software Change Request
  ↓
Approval
  ↓
Allowlist mới
```

Điều này ngăn phần mềm lạ tự biến thành phần mềm được phép.

## 12. Trạng thái chính

### Software Catalog

`Draft → SecurityReview → PendingApproval → Approved → Retired`

Có thể `Rejected` tại bước review/approval.

### Install Request

`Draft → SecurityReview? → PendingApproval → Approved → PendingInstallation → Installed`

Các nhánh kết thúc: `Rejected`, `Cancelled`, `NonCompliant`.

### Identity Review

`Pending → Confirmed | Rejected | Merged`

## 13. Nguyên tắc dữ liệu

- Current inventory có thể cập nhật.
- Approval Snapshot không được sửa.
- Audit history không được overwrite.
- Allowlist Active chỉ thay đổi qua workflow được approve.
- Install Request giữ AllowlistVersion đã dùng.
- Alert phải idempotent.
