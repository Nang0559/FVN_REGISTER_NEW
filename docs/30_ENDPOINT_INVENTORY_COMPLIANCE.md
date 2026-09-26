# FVN-REGISTER — Windows Endpoint Inventory & Compliance

## Mục tiêu

Quản lý phần mềm và Windows Service thực tế trên các máy Windows thuộc phạm vi FVN mà không phụ thuộc quyền quản trị Microsoft Defender của tập đoàn.

## Nguyên tắc

1. FVN Windows Agent là nguồn inventory chính.
2. Không dùng FVN server để quét WMI/SMB toàn mạng bằng tài khoản Administrator.
3. Network Discovery chỉ dùng để phát hiện thiết bị chưa quản lý, không phải nguồn chính của software inventory.
4. Defender/Intune là nguồn bổ sung tùy chọn nếu sau này được cấp quyền đọc.
5. Policy phải được approve trước khi có hiệu lực.
6. Không tự động xóa/disable phần mềm hoặc service khi phát hiện vi phạm.
7. Mọi ngoại lệ phải có lý do, thời hạn và Approval.
8. Agent chỉ nhận policy/configuration; không nhận arbitrary PowerShell/CMD từ server.

## Luồng chính

```text
Windows Agent
  -> Device/Software/Service Inventory
  -> FVN API
  -> Compliance Engine
  -> Software/Service Policy
  -> Alert / Exception Request
  -> Approval Engine
```

## Đối tượng

### Endpoint
- DeviceKey
- ComputerName
- SerialNumber
- OSName
- OSVersion
- User/EmployeeCode
- LastSeenUtc
- AgentVersion
- Status

### Software Inventory
- DeviceKey
- NormalizedName
- Publisher
- Version
- Architecture
- InstallDate
- InstallLocation
- DetectedAtUtc
- Source

### Service Inventory
- DeviceKey
- ServiceName
- DisplayName
- State
- StartMode
- BinaryPathHash (không lưu secrets)
- DetectedAtUtc
- Source

### Policy
- SoftwarePolicy
- ServicePolicy
- ScopeType/ScopeKey
- Allow/Deny
- VersionRule
- EffectiveFrom/EffectiveTo
- ApprovalCaseId
- Status

### Exception
- PolicyId
- DeviceKey hoặc phạm vi
- Reason
- EffectiveFrom/EffectiveTo
- ApprovalCaseId
- Status

## Compliance

Kết quả tối thiểu:

- Compliant
- Unknown
- NonCompliant
- AgentOffline
- UnmanagedDevice

`Unknown` không được tự động coi là vi phạm cho đến khi policy engine có đủ dữ liệu.

## Network Discovery

Chỉ phát hiện:
- IP/hostname/device fingerprint tối thiểu;
- thiết bị không có FVN Agent;
- thiết bị đã biết nhưng chưa được khai báo Equipment.

Discovery phải có:
- scope mạng được cấu hình;
- exclusion list;
- lịch chạy;
- audit log;
- quyền riêng;
- giới hạn tốc độ.

Không triển khai credentialed WMI/SMB scanning trong V1.

## Approval

Không tạo Approval Engine mới.

Các policy thay đổi và exception sử dụng Approval Engine hiện tại với `Module = Equipment/Security` và RequestType tương ứng. Leave/OT/Trip giữ nguyên business flow.

## Defender integration

Nếu sau này tenant cấp quyền read-only, Defender được tích hợp như nguồn thứ hai để reconciliation. Không làm FVN phụ thuộc Defender.
