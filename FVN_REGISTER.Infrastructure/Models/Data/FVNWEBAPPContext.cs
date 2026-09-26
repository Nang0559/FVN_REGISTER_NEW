namespace FVN_REGISTER.Infrastructure;

using System.Reflection;
using FVN_REGISTER.Core.Entities;
using FVN_REGISTER.Core.Entities.Approvers;
using FVN_REGISTER.Core.Entities.Common;
using FVN_REGISTER.Core.Entities.Equipment;
using FVN_REGISTER.Core.Entities.HR;
using FVN_REGISTER.Core.Entities.HRM;
using FVN_REGISTER.Core.Entities.Leaves;
using FVN_REGISTER.Core.Entities.OT;
using FVN_REGISTER.Core.Entities.Payroll;
using FVN_REGISTER.Core.Entities.Security;
using FVN_REGISTER.Core.Entities.Trips;
using FVN_REGISTER.Core.Entities.Views;
using FVN_REGISTER.Core.Entities.WorkCalendar;
using FVN_REGISTER.Core.Entities.PublicForms;
using Microsoft.EntityFrameworkCore;

public class FVNWEBAPPContext : DbContext
{
    public FVNWEBAPPContext(DbContextOptions<FVNWEBAPPContext> options) : base(options) { }
    public DbSet<F03ManagedScope> ManagedScopes { get; set; }
    public DbSet<F03FeatureOperatorAssignment> FeatureOperatorAssignments { get; set; }
    public DbSet<F03User> Users { get; set; }
    public DbSet<F03PasswordResetRequest> PasswordResetRequests { get; set; }
    public DbSet<F03Function> Functions { get; set; }
    public DbSet<F03SecurityFunctionRegistryItem> SecurityFunctionRegistry { get; set; }
    public DbSet<F03Permission> Permissions { get; set; }
    public DbSet<F03UserFunction> UserFunctions { get; set; }
    public DbSet<F03UserSession> UserSessions { get; set; }
    public DbSet<F03Role> Roles { get; set; }
    public DbSet<F03RoleFunction> RoleFunctions { get; set; }
    public DbSet<F03UserRole> UserRoles { get; set; }
    public DbSet<F03AccessChangeRequest> AccessChangeRequests { get; set; }
    public DbSet<F03Employee> Employees { get; set; }
    public DbSet<F03Department> Departments { get; set; }
    public DbSet<F03Position> Positions { get; set; }
    public DbSet<F03Gender> Genders { get; set; }
    public DbSet<F03Approver> Approvers { get; set; }
    public DbSet<F03ApprovalReminderLog> ApprovalReminderLogs { get; set; }
    public DbSet<F03ApprovalPolicy> ApprovalPolicies { get; set; }
    public DbSet<F03ApprovalSelection> ApprovalSelections { get; set; }
    public DbSet<F03ApprovalSnapshot> ApprovalSnapshots { get; set; }
    public DbSet<F03ApprovalHistory> ApprovalHistories { get; set; }
    public DbSet<F03LeaveBalance> LeaveBalances { get; set; }
    public DbSet<F03HrmAttendanceCalculated> HrmAttendanceCalculated { get; set; }
    public DbSet<F03LeaveType> LeaveTypes { get; set; }
    public DbSet<F03LeaveDay> LeaveDays { get; set; }
    public DbSet<F03LeaveDayDetail> LeaveDayDetails { get; set; }
    public DbSet<F03AttendanceStaging> AttendanceStagings { get; set; }
    public DbSet<F03OTRequest> OvertimeRequests { get; set; }
    public DbSet<F03OTEmployee> OvertimeEmployees { get; set; }
    public DbSet<F03OTLimitRule> OvertimeLimitRules { get; set; }
    public DbSet<F03OTReasonCode> OvertimeReasonCodes { get; set; }
    public DbSet<F03OTType> OTTypes { get; set; }
    public DbSet<F03TripRequest> TripRequests { get; set; }
    public DbSet<F03StagingTrip> StagingTrips { get; set; }
    public DbSet<F03EquipmentAsset> EquipmentAssets { get; set; }
    public DbSet<F03EquipmentSchema> EquipmentSchemas { get; set; }
    public DbSet<F03EquipmentFieldDefinition> EquipmentFieldDefinitions { get; set; }
    public DbSet<F03EquipmentImportBatch> EquipmentImportBatches { get; set; }
    public DbSet<F03EquipmentImportRow> EquipmentImportRows { get; set; }
    public DbSet<F03EquipmentRequest> EquipmentRequests { get; set; }
    public DbSet<F03EquipmentRepairHistory> EquipmentRepairHistories { get; set; }
    public DbSet<F03EquipmentInspectionTemplate> EquipmentInspectionTemplates { get; set; }
    public DbSet<F03EquipmentInspectionItem> EquipmentInspectionItems { get; set; }
    public DbSet<F03EquipmentInspectionAssignment> EquipmentInspectionAssignments { get; set; }
    public DbSet<F03EquipmentInspectionTask> EquipmentInspectionTasks { get; set; }
    public DbSet<F03EquipmentInspectionItemResult> EquipmentInspectionItemResults { get; set; }
    public DbSet<F03EquipmentInspectionEvidence> EquipmentInspectionEvidence { get; set; }
    public DbSet<F03StagingDepartment> StagingDepartments { get; set; }
    public DbSet<F03StagingEmployee> StagingEmployees { get; set; }
    public DbSet<F03StagingLeaveType> StagingLeaveTypes { get; set; }
    public DbSet<F03StagingOTType> StagingOTTypes { get; set; }
    public DbSet<F03StagingPosition> StagingPositions { get; set; }
    public DbSet<F03SyncReviewFlag> SyncReviewFlags { get; set; }
    public DbSet<HrmLeaveTypeChangeLog> HrmLeaveTypeChangeLogs { get; set; }
    public DbSet<F03AppNotification> AppNotifications { get; set; }
    public DbSet<F03CalendarModuleDefinition> CalendarModuleDefinitions { get; set; }
    public DbSet<F03CalendarModulePolicy> CalendarModulePolicies { get; set; }
    public DbSet<F03CalendarProjection> CalendarProjections { get; set; }
    public DbSet<F03ActionItem> ActionItems { get; set; }
    public DbSet<F03ExecutionPolicy> ExecutionPolicies { get; set; }
    public DbSet<F03ExecutionReconciliation> ExecutionReconciliations { get; set; }
    public DbSet<F03ExecutionConfirmation> ExecutionConfirmations { get; set; }
    public DbSet<F03ExecutionConfirmationEvidence> ExecutionConfirmationEvidence { get; set; }
    public DbSet<F03ExecutionReconciliationHistory> ExecutionReconciliationHistory { get; set; }
    public DbSet<F03ExecutionResolution> ExecutionResolutions { get; set; }
    public DbSet<F03ExecutionCorrection> ExecutionCorrections { get; set; }
    public DbSet<F03PayrollCalculationPeriod> PayrollCalculationPeriods { get; set; }
    public DbSet<F03PayrollInput> PayrollInputs { get; set; }
    public DbSet<F03ActionPolicy> ActionPolicies { get; set; }
    public DbSet<F03AuditLog> AuditLogs { get; set; }
    public DbSet<F03Attachment> Attachments { get; set; }
    public DbSet<F03EmailQueue> EmailQueues { get; set; }
    public DbSet<F03EmailLog> EmailLogs { get; set; }
    public DbSet<F03EmailTemplate> EmailTemplates { get; set; }
    public DbSet<F03EmailProfile> EmailProfiles { get; set; }
    public DbSet<F03EmailDispatchPolicy> EmailDispatchPolicies { get; set; }
    public DbSet<F03PublicInformation> PublicInformations { get; set; }
    public DbSet<F03PublicForm> PublicForms { get; set; }
    public DbSet<F03PublicFormQuestion> PublicFormQuestions { get; set; }
    public DbSet<F03PublicFormQuestionOption> PublicFormQuestionOptions { get; set; }
    public DbSet<F03PublicFormAudience> PublicFormAudiences { get; set; }
    public DbSet<F03PublicFormSubmission> PublicFormSubmissions { get; set; }
    public DbSet<F03PublicFormAnswer> PublicFormAnswers { get; set; }
    public DbSet<F03HrmUserRoleRule> HrmUserRoleRules { get; set; }
    public DbSet<F03BusinessRule> BusinessRules { get; set; }
    public DbSet<F03CompanyHoliday> CompanyHolidays { get; set; }
    public DbSet<F03WorkYear> WorkYears { get; set; }
    public DbSet<F03EscalationLog> EscalationLogs { get; set; }
    public DbSet<F03EscalationRule> EscalationRules { get; set; }
    public DbSet<F03UserLog> UserLogs { get; set; }
    public DbSet<VF03EmployeeApprover> VEmployeeApprovers { get; set; }
    public DbSet<VF03employee> VF03Employees { get; set; }
    public DbSet<vF03EmployeeAttendance> VF03EmployeeAttendances { get; set; }
    public DbSet<VF03LeaveRequest> VF03LeaveRequests { get; set; }
    public DbSet<VF03LeaveRequestDetail> VF03LeaveRequestDetails { get; set; }
    public DbSet<VF03leaveType> VF03LeaveTypes { get; set; }
    public DbSet<F03EndpointDevice> EndpointDevices { get; set; }
    public DbSet<F03EndpointSoftwareInventory> EndpointSoftwareInventory { get; set; }
    public DbSet<F03EndpointServiceInventory> EndpointServiceInventory { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
