# FVN-REGISTER — Hướng dẫn triển khai Windows Endpoint Inventory & Compliance

## 1. Phạm vi

Tính năng quản lý endpoint Windows của FVN-REGISTER không phụ thuộc Active Directory, Domain Controller, LDAP hay tài khoản domain.

Mô hình:

```text
FVN Employee
    ↓
FVN Equipment Asset
    ↓
FVN Endpoint Device
    ↓
FVN Windows Agent
    ↓ HTTPS
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
2. DeviceKey lấy từ credential server-side.
3. HardwareIdentity dùng để kiểm tra tính nhất quán.
4. ComputerName chỉ là thuộc tính, không phải identity duy nhất.
5. Nếu phần cứng identity thay đổi bất thường, chuyển trạng thái cần xác minh thay vì tự động đổi thiết bị.

## 3. Chuẩn bị server

- Chạy SQL script theo thứ tự:
  - `54_Endpoint_Inventory_Compliance.sql`
  - `55_Endpoint_Credentials.sql`
- Build API và Infrastructure.
- HTTPS bắt buộc cho Agent production.
- Chỉ mở API inventory từ mạng/phạm vi cần thiết.
- Bật rate limiting.
- Bật audit cho provision/rotate/revoke.

Không lưu credential plaintext trong database.

## 4. Provision một máy

1. Tạo Equipment Asset nếu máy chưa có.
2. Nhập serial/asset code và gán employee nếu có.
3. Provision Endpoint cho Asset.
4. FVN sinh credential riêng cho máy.
5. Hiển thị secret đúng một lần.
6. Cấu hình secret trên Agent.
7. Cài Agent.
8. Agent gửi heartbeat.
9. Server xác nhận DeviceKey/HardwareIdentity.
10. Agent gửi complete inventory.

Nếu máy không có serial/SMBIOS UUID đáng tin cậy, vẫn cho Agent hoạt động nhưng đánh dấu identity quality thấp để IT xác minh.

## 5. Cài Windows Agent

Agent chạy dưới Windows Service.

Agent cần quyền đủ để đọc:
- Installed Software từ Windows uninstall registry.
- Windows Services.
- Thông tin phần cứng/OS cơ bản.

Không yêu cầu Domain Account.
Không yêu cầu AD.
Không yêu cầu remote WMI/SMB credential.
Không chạy arbitrary PowerShell/CMD nhận từ server.

Cấu hình tối thiểu:

```json
{
  "FvnEndpoint": {
    "ApiBaseUrl": "https://<fvn-api>",
    "DeviceKey": "<provisioned-device-key>",
    "ApiKey": "<one-time-provisioned-secret>",
    "CollectionIntervalMinutes": 30
  }
}
```

Trong production phải bảo vệ secret bằng Windows DPAPI/Windows Credential Manager hoặc cơ chế secret store phù hợp; không để plaintext trong source control.

## 6. Inventory

Mỗi complete snapshot gồm:
- Device metadata.
- Software list.
- Windows service list.
- Collection timestamp.

Server upsert theo DeviceKey server-side và thay thế snapshot hiện hành một cách transaction-safe. Dữ liệu lịch sử compliance được giữ riêng; inventory hiện hành không được phình lên theo từng lần heartbeat.

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

Không chuyển `Unknown` thành `NonCompliant` một cách tự động.

Exception được xét trước khi tạo vi phạm:

```text
Policy Deny
   + Approved active Exception
   = Compliant/Exception
```

## 9. Alert

Alert phải idempotent. Một vi phạm lặp lại ở các lần inventory không tạo hàng nghìn alert mới.

Một alert được mở, cập nhật LastDetectedAt và chỉ đóng khi:
- inventory mới chứng minh đã hết vi phạm; hoặc
- người có quyền xử lý đóng theo quy trình.

## 10. Exception và Approval

Không tạo Approval Engine mới.

Exception sử dụng Approval Engine hiện tại và phải snapshot:
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
- Hai máy không được dùng chung credential.
- Revoke credential phải chặn inventory.
- Credential của PC-A không được gửi inventory cho PC-B.
- Đổi ComputerName không tạo endpoint mới nếu identity phần cứng/credential vẫn hợp lệ.
- Credential hết hạn phải bị từ chối.

### Inventory
- Cùng snapshot gửi hai lần không tạo duplicate.
- Software gỡ khỏi máy phải biến mất khỏi current inventory.
- Service thay đổi state phải cập nhật.
- Inventory lỗi giữa chừng không được xóa snapshot hợp lệ trước đó.

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
