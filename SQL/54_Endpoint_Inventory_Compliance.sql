/* FVN-REGISTER - Windows Endpoint Inventory & Compliance
   V1 foundation. Does not execute remote commands and does not perform credentialed WMI/SMB scanning.
*/

IF OBJECT_ID('dbo.F03EndpointDevices','U') IS NULL
BEGIN
    CREATE TABLE dbo.F03EndpointDevices
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03EndpointDevices PRIMARY KEY,
        DeviceKey NVARCHAR(100) NOT NULL,
        ComputerName NVARCHAR(255) NULL,
        SerialNumber NVARCHAR(255) NULL,
        OsName NVARCHAR(255) NULL,
        OsVersion NVARCHAR(100) NULL,
        EmployeeCode NVARCHAR(50) NULL,
        EquipmentAssetId INT NULL,
        AgentVersion NVARCHAR(50) NULL,
        LastSeenUtc DATETIME2(0) NULL,
        Status NVARCHAR(30) NOT NULL CONSTRAINT DF_F03EndpointDevices_Status DEFAULT('Unknown'),
        Source NVARCHAR(30) NOT NULL CONSTRAINT DF_F03EndpointDevices_Source DEFAULT('FVNAgent'),
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_F03EndpointDevices_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_F03EndpointDevices_UpdatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_F03EndpointDevices_DeviceKey UNIQUE(DeviceKey)
    );
END;
GO

IF OBJECT_ID('dbo.F03EndpointSoftwareInventory','U') IS NULL
BEGIN
    CREATE TABLE dbo.F03EndpointSoftwareInventory
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03EndpointSoftwareInventory PRIMARY KEY,
        EndpointDeviceId BIGINT NOT NULL,
        NormalizedName NVARCHAR(255) NOT NULL,
        DisplayName NVARCHAR(255) NULL,
        Publisher NVARCHAR(255) NULL,
        Version NVARCHAR(100) NULL,
        Architecture NVARCHAR(30) NULL,
        InstallDate DATE NULL,
        InstallLocation NVARCHAR(1000) NULL,
        DetectedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_F03EndpointSoftwareInventory_DetectedAtUtc DEFAULT SYSUTCDATETIME(),
        Source NVARCHAR(30) NOT NULL CONSTRAINT DF_F03EndpointSoftwareInventory_Source DEFAULT('FVNAgent'),
        CONSTRAINT FK_F03EndpointSoftwareInventory_Device FOREIGN KEY(EndpointDeviceId) REFERENCES dbo.F03EndpointDevices(Id)
    );
    CREATE INDEX IX_F03EndpointSoftwareInventory_Device_Name ON dbo.F03EndpointSoftwareInventory(EndpointDeviceId, NormalizedName);
END;
GO

IF OBJECT_ID('dbo.F03EndpointServiceInventory','U') IS NULL
BEGIN
    CREATE TABLE dbo.F03EndpointServiceInventory
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03EndpointServiceInventory PRIMARY KEY,
        EndpointDeviceId BIGINT NOT NULL,
        ServiceName NVARCHAR(255) NOT NULL,
        DisplayName NVARCHAR(255) NULL,
        State NVARCHAR(30) NULL,
        StartMode NVARCHAR(30) NULL,
        BinaryPathHash NVARCHAR(128) NULL,
        DetectedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_F03EndpointServiceInventory_DetectedAtUtc DEFAULT SYSUTCDATETIME(),
        Source NVARCHAR(30) NOT NULL CONSTRAINT DF_F03EndpointServiceInventory_Source DEFAULT('FVNAgent'),
        CONSTRAINT FK_F03EndpointServiceInventory_Device FOREIGN KEY(EndpointDeviceId) REFERENCES dbo.F03EndpointDevices(Id)
    );
    CREATE INDEX IX_F03EndpointServiceInventory_Device_Name ON dbo.F03EndpointServiceInventory(EndpointDeviceId, ServiceName);
END;
GO

IF OBJECT_ID('dbo.F03SoftwarePolicies','U') IS NULL
BEGIN
    CREATE TABLE dbo.F03SoftwarePolicies
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03SoftwarePolicies PRIMARY KEY,
        PolicyCode NVARCHAR(50) NOT NULL,
        SoftwareName NVARCHAR(255) NOT NULL,
        Publisher NVARCHAR(255) NULL,
        Action NVARCHAR(20) NOT NULL,
        VersionRule NVARCHAR(200) NULL,
        ScopeType NVARCHAR(30) NOT NULL CONSTRAINT DF_F03SoftwarePolicies_ScopeType DEFAULT('Company'),
        ScopeKey NVARCHAR(100) NULL,
        EffectiveFrom DATETIME2(0) NULL,
        EffectiveTo DATETIME2(0) NULL,
        ApprovalCaseId BIGINT NULL,
        Status NVARCHAR(30) NOT NULL CONSTRAINT DF_F03SoftwarePolicies_Status DEFAULT('Draft'),
        CreatedBy INT NULL,
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_F03SoftwarePolicies_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_F03SoftwarePolicies_UpdatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_F03SoftwarePolicies_Code UNIQUE(PolicyCode),
        CONSTRAINT CK_F03SoftwarePolicies_Action CHECK(Action IN ('Allow','Deny'))
    );
END;
GO

IF OBJECT_ID('dbo.F03WindowsServicePolicies','U') IS NULL
BEGIN
    CREATE TABLE dbo.F03WindowsServicePolicies
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03WindowsServicePolicies PRIMARY KEY,
        PolicyCode NVARCHAR(50) NOT NULL,
        ServiceName NVARCHAR(255) NOT NULL,
        RequiredState NVARCHAR(30) NULL,
        RequiredStartMode NVARCHAR(30) NULL,
        Action NVARCHAR(20) NOT NULL,
        ScopeType NVARCHAR(30) NOT NULL CONSTRAINT DF_F03WindowsServicePolicies_ScopeType DEFAULT('Company'),
        ScopeKey NVARCHAR(100) NULL,
        EffectiveFrom DATETIME2(0) NULL,
        EffectiveTo DATETIME2(0) NULL,
        ApprovalCaseId BIGINT NULL,
        Status NVARCHAR(30) NOT NULL CONSTRAINT DF_F03WindowsServicePolicies_Status DEFAULT('Draft'),
        CreatedBy INT NULL,
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_F03WindowsServicePolicies_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_F03WindowsServicePolicies_UpdatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_F03WindowsServicePolicies_Code UNIQUE(PolicyCode),
        CONSTRAINT CK_F03WindowsServicePolicies_Action CHECK(Action IN ('Allow','Deny'))
    );
END;
GO

IF OBJECT_ID('dbo.F03EndpointComplianceResults','U') IS NULL
BEGIN
    CREATE TABLE dbo.F03EndpointComplianceResults
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03EndpointComplianceResults PRIMARY KEY,
        EndpointDeviceId BIGINT NOT NULL,
        PolicyType NVARCHAR(30) NOT NULL,
        PolicyId INT NULL,
        ItemName NVARCHAR(255) NOT NULL,
        Result NVARCHAR(30) NOT NULL,
        ActualValue NVARCHAR(500) NULL,
        ExpectedValue NVARCHAR(500) NULL,
        EvaluatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_F03EndpointComplianceResults_EvaluatedAtUtc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_F03EndpointComplianceResults_Device FOREIGN KEY(EndpointDeviceId) REFERENCES dbo.F03EndpointDevices(Id),
        CONSTRAINT CK_F03EndpointComplianceResults_Result CHECK(Result IN ('Compliant','Unknown','NonCompliant','AgentOffline','UnmanagedDevice'))
    );
    CREATE INDEX IX_F03EndpointComplianceResults_Device_Result ON dbo.F03EndpointComplianceResults(EndpointDeviceId, Result, EvaluatedAtUtc);
END;
GO

IF OBJECT_ID('dbo.F03EndpointComplianceExceptions','U') IS NULL
BEGIN
    CREATE TABLE dbo.F03EndpointComplianceExceptions
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03EndpointComplianceExceptions PRIMARY KEY,
        PolicyType NVARCHAR(30) NOT NULL,
        PolicyId INT NOT NULL,
        EndpointDeviceId BIGINT NOT NULL,
        Reason NVARCHAR(1000) NOT NULL,
        EffectiveFrom DATETIME2(0) NOT NULL,
        EffectiveTo DATETIME2(0) NULL,
        ApprovalCaseId BIGINT NULL,
        Status NVARCHAR(30) NOT NULL CONSTRAINT DF_F03EndpointComplianceExceptions_Status DEFAULT('PendingApproval'),
        CreatedBy INT NULL,
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_F03EndpointComplianceExceptions_CreatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_F03EndpointComplianceExceptions_Device FOREIGN KEY(EndpointDeviceId) REFERENCES dbo.F03EndpointDevices(Id)
    );
END;
GO

IF OBJECT_ID('dbo.F03EndpointAlerts','U') IS NULL
BEGIN
    CREATE TABLE dbo.F03EndpointAlerts
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03EndpointAlerts PRIMARY KEY,
        EndpointDeviceId BIGINT NOT NULL,
        AlertType NVARCHAR(50) NOT NULL,
        Severity NVARCHAR(20) NOT NULL,
        Title NVARCHAR(255) NOT NULL,
        Details NVARCHAR(2000) NULL,
        RelatedPolicyId INT NULL,
        Status NVARCHAR(30) NOT NULL CONSTRAINT DF_F03EndpointAlerts_Status DEFAULT('Open'),
        FirstDetectedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_F03EndpointAlerts_FirstDetectedAtUtc DEFAULT SYSUTCDATETIME(),
        LastDetectedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_F03EndpointAlerts_LastDetectedAtUtc DEFAULT SYSUTCDATETIME(),
        ResolvedAtUtc DATETIME2(0) NULL,
        CONSTRAINT FK_F03EndpointAlerts_Device FOREIGN KEY(EndpointDeviceId) REFERENCES dbo.F03EndpointDevices(Id)
    );
    CREATE INDEX IX_F03EndpointAlerts_Status ON dbo.F03EndpointAlerts(Status, Severity, LastDetectedAtUtc);
END;
GO
