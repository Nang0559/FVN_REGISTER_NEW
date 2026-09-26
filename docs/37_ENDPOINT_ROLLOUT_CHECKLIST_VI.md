# Checklist nghiệm thu Endpoint Inventory & Compliance

## A. Database

- [ ] `54_Endpoint_Inventory_Compliance.sql` đã chạy.
- [ ] `55_Endpoint_Credentials.sql` đã chạy.
- [ ] `56_Endpoint_Governance.sql` đã chạy sau 54/55.
- [ ] Không có duplicate DeviceKey.
- [ ] Credential chỉ lưu hash.
- [ ] Approval snapshot có JSON hợp lệ.

## B. Machine identity

- [ ] Mỗi máy có DeviceKey riêng.
- [ ] Mỗi máy có credential riêng.
- [ ] Credential cũ bị revoke khi rotate/provision lại.
- [ ] Agent không gửi `X-FVN-Device-Key` làm nguồn xác thực.
- [ ] API lấy DeviceKey từ credential.
- [ ] Đổi ComputerName tạo Identity History + Alert.
- [ ] Thay hardware identity tạo Identity Review.
- [ ] Không tự động merge hardware mới.

## C. Inventory

- [ ] Software inventory từ Agent.
- [ ] Windows Service inventory từ Agent.
- [ ] Inventory snapshot thay thế current inventory transaction-safe.
- [ ] Inventory hash được lưu để nhận biết payload không đổi.
- [ ] Agent không được tự gán EmployeeCode.
- [ ] Agent không được tự gán EquipmentAssetId.

## D. Software governance

- [ ] Software Catalog chỉ Active sau Security Review + Approval.
- [ ] Allowlist có version logic.
- [ ] Install Request tự load catalog Approved.
- [ ] Install Request lưu AllowlistVersion.
- [ ] Software chưa có catalog chuyển Security Review.
- [ ] IT Security không tự bypass Approval để Active catalog.
- [ ] Agent phát hiện software lạ không tự thêm vào Allowlist.

## E. Service governance

- [ ] Service Catalog độc lập với Software Catalog.
- [ ] Có RequiredState/RequiredStartMode.
- [ ] Service policy có Security Review + Approval.
- [ ] Agent inventory được đối chiếu với policy.

## F. Approval

- [ ] Dùng Approval Engine hiện tại.
- [ ] Không có Approval Engine thứ hai cho Endpoint.
- [ ] Snapshot bất biến.
- [ ] Approver xem Before/After hoặc Proposed Version đầy đủ.
- [ ] ApprovalCaseId được liên kết với request.
- [ ] Reject không làm Active policy thay đổi.
- [ ] Chỉ Approved mới được Active.

## G. Security Center

- [ ] Endpoint.View
- [ ] Endpoint.Inventory.View
- [ ] Endpoint.Compliance.View
- [ ] Endpoint.Alert.View
- [ ] Endpoint.Alert.Resolve
- [ ] Endpoint.Provision
- [ ] Endpoint.Credential.Rotate
- [ ] Endpoint.Credential.Revoke
- [ ] Endpoint.SoftwareCatalog.View
- [ ] Endpoint.SoftwareCatalog.Manage
- [ ] Endpoint.SoftwareSecurityReview
- [ ] Endpoint.SoftwarePolicy.Submit
- [ ] Endpoint.SoftwarePolicy.Approve
- [ ] Endpoint.ServiceCatalog.View
- [ ] Endpoint.ServiceCatalog.Manage
- [ ] Endpoint.ServiceSecurityReview
- [ ] Endpoint.ServicePolicy.Submit
- [ ] Endpoint.ServicePolicy.Approve
- [ ] Endpoint.InstallRequest.Create
- [ ] Endpoint.InstallRequest.View
- [ ] Endpoint.InstallRequest.Approve
- [ ] Endpoint.Exception.Create
- [ ] Endpoint.Exception.View
- [ ] Endpoint.Exception.Approve

Capability phải được kiểm tra server-side và theo scope. Không hard-code theo tên role.

## H. Pilot

- [ ] 5–10 máy IT.
- [ ] Đổi tên thử nghiệm tạo alert.
- [ ] Revoke thử credential.
- [ ] Gửi credential PC-A cho PC-B phải bị từ chối/bind sai identity.
- [ ] Cài software có trong allowlist → Compliant sau inventory.
- [ ] Cài software ngoài allowlist → NonCompliant/Alert.
- [ ] Tạo phần mềm mới → Security Review → Approval → Active.
- [ ] Install Request dùng catalog mới.
- [ ] Sau cài đặt Agent phát hiện → Installed.
- [ ] Allowlist change được snapshot và audit.

## I. Không bật enforcement diện rộng ngay

Giai đoạn đầu chạy quan sát (`Monitor`) để phát hiện false positive. Chỉ sau khi catalog/policy ổn định mới chuyển từng scope sang enforcement.
