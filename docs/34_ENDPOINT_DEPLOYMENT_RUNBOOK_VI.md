# FVN-REGISTER — Hướng dẫn triển khai Windows Endpoint Inventory & Compliance

## 1. Phạm vi

Tính năng quản lý endpoint Windows của FVN-REGISTER không phụ thuộc Active Directory, Domain Controller, LDAP hay tài khoản domain.

```text
FVN Employee
    ↓
FVN Equipment Asset
    ↓
FVN Endpoint Device
    ↓
FVN Windows Agent
    ↓ HTTPS + credential riêng của máy
FVN API
    ↓
Software / Service Inventory
    ↓
Policy
    ↓
Compliance
    ↓
Alert / Exception
    ↓
Approval Engine hiện tại
```

Microsoft Defender/Intune nếu được tập đoàn cấp quyền chỉ là nguồn dữ liệu bổ sung, không phải dependency bắt buộc.

## 2. Định danh máy không dùng AD

Không dùng AD Object ID, Domain SID hoặc Computer Account làm khóa chính.

Agent cung cấp tối thiểu:
- AgentInstallationId: UUID sinh khi cài Agent.
- SerialNumber: serial phần cứng nếu Windows cung cấp.
- HardwareUuid: SMBIOS UUID nếu Windows cung cấp.
- ComputerName.
- OSName/OSVersion.

FVN tạo DeviceKey nội bộ. DeviceKey không được client tự chọn để thay đổi thiết bị đã bind.

Quy tắc nhận diện:
1. Credential xác định endpoint mà server đã provision.
2. DeviceKey được lấy từ credential server-side.
3. HardwareIdentity dùng để kiểm tra tính nhất quán.
4. ComputerName chỉ là thuộc tính, không phải identity duy nhất.
5. Nếu phần cứng identity thay đổi bất thường, chuyển trạng thái cần xác minh thay vì tự động đổi thiết bị.

## 3. Chuẩn bị server

Chạy SQL script theo thứ tự:

- `54_Endpoint_Inventory_Compliance.sql`
- `55_Endpoint_Credentials.sql`

Production yêu cầu:
- HTTPS cho API inventory;
- rate limiting theo endpoint;
- audit provision/revoke/rotate;
- credential lưu dạng SHA-256 hash, không lưu plaintext;
- inventory endpoint xác thực bằng credential máy, không dùng JWT người dùng.

## 4. Provision một máy

1. Tạo Equipment Asset nếu máy chưa có.
2. Nhập serial/asset code và gán employee nếu có.
3. Provision Endpoint bằng API quản trị `POST /api/security/endpoints/credentials/provision`.
4. FVN tạo DeviceKey nếu endpoint chưa tồn tại.
5. FVN sinh credential ngẫu nhiên riêng cho máy và chỉ trả secret một lần.
6. Không ghi secret vào log hoặc database.
7. Bảo vệ secret trên Windows bằng DPAPI.
8. Cài Agent.
9. Agent gửi inventory qua `POST /api/security/endpoints/inventory`.
10. Server lấy DeviceKey từ credential; không tin EmployeeCode/EquipmentAssetId/DeviceKey do Agent gửi.

Credential hết hạn hoặc bị revoke phải làm request inventory thất bại.

## 5. Cài Windows Agent

Agent chạy dưới Windows Service.

Agent cần quyền đủ để đọc:
- Installed Software từ Windows uninstall registry;
- Windows Services;
- thông tin phần cứng/OS cơ bản.

Không yêu cầu Domain Account.
Không yêu cầu AD.
Không yêu cầu remote WMI/SMB credential.
Không chạy arbitrary PowerShell/CMD nhận từ server.

Cấu hình production:

```json
{
  "FVNEndpointAgent": {
    "ApiBaseUrl": "https://<fvn-api>",
    "DeviceKey": "<provisioned-device-key>",
    "ApiKeyProtected": "<DPAPI-protected-secret>",
    "IntervalMinutes": 30
  }
}
```

Agent dùng `WindowsSecretStore`/DPAPI với scope LocalMachine để giải mã credential. Không commit secret thật vào `appsettings.json`.

Script cài đặt mẫu: `FVN_REGISTER.EndpointAgent/install-agent.ps1`.

## 6. Inventory

Mỗi complete snapshot gồm:
- Device metadata;
- Software list;
- Windows service list;
- Collection timestamp.

Server upsert theo DeviceKey server-side và thay thế snapshot hiện hành transaction-safe. Dữ liệu lịch sử compliance được giữ riêng; inventory hiện hành không phình theo từng heartbeat.

`EmployeeCode` và `EquipmentAssetId` trong payload Agent không được dùng làm quyền sở hữu. Quan hệ Employee ↔ Equipment do FVN quản lý.

## 7. Policy

Policy không active ngay khi tạo.

```text
Draft
  ↓
Snapshot
  ↓
Approval Engine
  ↓
Approved
  ↓
Active
```

Software policy và Windows service policy phải có:
- mã policy;
- allow/deny;
- version/state rule nếu cần;
- phạm vi áp dụng;
- thời gian hiệu lực;
- lịch sử thay đổi.

## 8. Compliance

Kết quả:
- `Compliant`: dữ liệu đáp ứng policy.
- `NonCompliant`: có bằng chứng vi phạm policy.
- `Unknown`: chưa đủ dữ liệu.
- `AgentOffline`: quá thời gian heartbeat.
- `UnmanagedDevice`: phát hiện thiết bị nhưng chưa được FVN provision.

Không chuyển `Unknown` thành `NonCompliant` tự động.

Exception được xét trước khi tạo vi phạm:

```text
Policy Deny
   + Approved active Exception
   = Compliant/Exception
```

## 9. Alert

Alert phải idempotent. Một vi phạm lặp lại không tạo hàng nghìn alert mới.

Một alert được mở, cập nhật LastDetectedAt và chỉ đóng khi inventory chứng minh đã hết vi phạm hoặc người có quyền xử lý đóng theo quy trình.

## 10. Exception và Approval

Không tạo Approval Engine mới.

Exception sử dụng Approval Engine hiện tại và snapshot:
- thiết bị;
- software/service;
- policy;
- lý do;
- thời hạn;
- người yêu cầu.

Sau khi Approved, exception mới được Compliance Engine coi là active.

## 11. Network Discovery

V1 không dùng credentialed WMI/SMB scan.

Discovery chỉ phát hiện thiết bị tối thiểu trong scope mạng đã cấu hình để tìm:
- unmanaged device;
- endpoint đã biết nhưng Agent offline;
- thiết bị chưa gắn Equipment Asset.

Phải có scope, exclusion, rate limit, lịch và audit.

## 12. Defender/Intune

Không cần quyền quản trị Defender của tập đoàn để vận hành FVN Agent.

Nếu được cấp quyền read-only sau này:

```text
FVN Agent ─────┐
               ├── Reconciliation ──> Unified Inventory
Defender ──────┘
```

Không để Defender thay thế FVN DeviceKey.

## 13. Kiểm thử bắt buộc trước production

### Identity
- Hai máy không dùng chung credential.
- Revoke credential phải chặn inventory.
- Credential của PC-A không gửi inventory cho PC-B.
- Đổi ComputerName không tạo endpoint mới nếu credential/hardware identity vẫn hợp lệ.
- Credential hết hạn bị từ chối.
- Payload sửa DeviceKey nhưng giữ credential của máy khác vẫn bị server quy về DeviceKey của credential.

### Inventory
- Cùng snapshot gửi hai lần không tạo duplicate.
- Software gỡ khỏi máy biến mất khỏi current inventory.
- Service thay đổi state được cập nhật.
- Inventory lỗi giữa chừng không xóa snapshot hợp lệ trước đó.

### Compliance
- Allow → Compliant.
- Deny → NonCompliant.
- Thiếu inventory → Unknown.
- Offline quá ngưỡng → AgentOffline.
- Exception active → không báo vi phạm tương ứng.

### Security
- HTTPS.
- Rate limit.
- Audit provision/revoke.
- Secret không xuất hiện trong log.
- Agent không có remote command channel.
- API inventory không yêu cầu JWT người dùng.

## 14. Rollout đề xuất

### Pilot
5–10 máy IT.

### Phase 2
Một phòng ban.

### Phase 3
Toàn bộ laptop/desktop.

### Phase 4
Server và máy đặc thù.

### Phase 5
Network Discovery.

### Phase 6
Defender/Intune read-only nếu được cấp quyền.

Không bật Deny policy diện rộng ngay khi Agent mới triển khai. Trước tiên chạy inventory và chế độ quan sát để phát hiện false positive.

## 15. Tiêu chí hoàn thành production

- Agent identity ổn định.
- Credential lifecycle hoạt động.
- Inventory idempotent.
- Compliance có test.
- Approval policy/exception dùng engine hiện tại.
- Audit đầy đủ.
- Không phụ thuộc AD.
- Không có remote command execution.
- Có rollback và revoke Agent credential.
- Có dashboard unmanaged/offline/non-compliant.
