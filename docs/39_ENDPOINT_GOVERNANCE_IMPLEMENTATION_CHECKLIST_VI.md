# Checklist triển khai Endpoint Governance

Tài liệu này là gate trước khi coi tính năng hoàn chỉnh. Mục nào chưa đạt không được bật policy deny diện rộng.

## A. Approval Engine

- [ ] Xác định provider/module của Endpoint trong Approval Engine hiện tại.
- [ ] Software Catalog Change tạo đúng ApprovalCase hiện tại.
- [ ] Service Catalog Change tạo đúng ApprovalCase hiện tại.
- [ ] Install Request tạo đúng ApprovalCase hiện tại.
- [ ] Exception tạo đúng ApprovalCase hiện tại.
- [ ] Route lấy từ F03ApprovalPolicies, không hard-code cấp duyệt.
- [ ] Approver xem immutable snapshot.
- [ ] Approve/reject callback cập nhật nghiệp vụ Endpoint transaction-safe.
- [ ] Không tạo bảng/engine approval thứ hai.

## B. Software Catalog

- [ ] Draft không thể dùng cho Compliance.
- [ ] Chỉ Active version mới được dùng để đối chiếu.
- [ ] Mỗi thay đổi tạo version.
- [ ] Snapshot có before/after.
- [ ] Có publisher và version rule.
- [ ] Có scope server/workstation hoặc scope thiết bị phù hợp.

## C. Service Catalog

- [ ] ServiceName là định danh kỹ thuật.
- [ ] AllowedState.
- [ ] AllowedStartMode.
- [ ] Scope.
- [ ] Version.
- [ ] Approval snapshot.

## D. Install Request

- [ ] UI chỉ load Active Allowlist.
- [ ] Request lưu CatalogVersion.
- [ ] Submit tạo ApprovalCase.
- [ ] Không cho employee sửa catalog.
- [ ] Không coi Approved là Installed.
- [ ] Agent inventory mới được xác nhận Installed.

## E. New Software

- [ ] Employee tạo New Software Request.
- [ ] IT Security Review.
- [ ] Reject giữ nguyên catalog.
- [ ] Accept tạo Catalog Change Request.
- [ ] Catalog Change phải Approval.
- [ ] Sau Active mới cho Install Request tiếp tục.

## F. Agent

- [ ] Credential riêng từng máy.
- [ ] Server resolve DeviceKey từ credential.
- [ ] Payload không override DeviceKey.
- [ ] DPAPI bảo vệ secret.
- [ ] HTTPS.
- [ ] Không remote command execution.
- [ ] Software inventory idempotent.
- [ ] Service inventory idempotent.
- [ ] Inventory fingerprint.

## G. Compliance

- [ ] Allowed software -> Compliant.
- [ ] Unknown software -> NonCompliant + Alert.
- [ ] Active exception -> không tạo violation tương ứng.
- [ ] Agent offline -> AgentOffline/Unknown theo policy.
- [ ] Chưa có inventory -> không tự suy diễn NonCompliant.
- [ ] Software bị gỡ -> cập nhật trạng thái thực tế.

## H. Identity

- [ ] ComputerName change có history.
- [ ] ComputerName change tạo alert.
- [ ] Hardware identity change -> PendingReview.
- [ ] Không tự merge máy khi hardware identity thay đổi.
- [ ] Chuyển người sử dụng Equipment không đổi DeviceKey.

## I. Security Center

- [ ] Endpoint capabilities được registry.
- [ ] Không trùng FunctionCode.
- [ ] Server-side capability check.
- [ ] Employee scope chỉ endpoint/equipment được phép.
- [ ] IT scope theo assignment/role.
- [ ] Approver scope theo ApprovalPolicy.
- [ ] Superadmin capability riêng.

## J. Production gate

- [ ] Unit tests.
- [ ] Integration tests.
- [ ] Approval end-to-end test.
- [ ] Agent pilot 5-10 máy.
- [ ] Offline/reconnect test.
- [ ] Credential revoke test.
- [ ] Duplicate inventory test.
- [ ] Unauthorized software test.
- [ ] Device rename test.
- [ ] Hardware replacement test.
- [ ] Rollback catalog version test.
- [ ] Audit verification.
- [ ] Chỉ sau khi tất cả đạt mới bật enforcement diện rộng.
