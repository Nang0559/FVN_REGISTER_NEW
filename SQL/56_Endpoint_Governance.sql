/* FVN-REGISTER - Endpoint Software/Service Governance
   Does not create a second approval engine. ApprovalCaseId points to the existing approval workflow.
*/

IF OBJECT_ID('dbo.F03EndpointSoftwareCatalog','U') IS NULL
BEGIN
    CREATE TABLE dbo.F03EndpointSoftwareCatalog
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03EndpointSoftwareCatalog PRIMARY KEY,
        CatalogCode NVARCHAR(50) NOT NULL,
        NormalizedName NVARCHAR(255) NOT NULL,
        DisplayName NVARCHAR(255) NOT NULL,
        Publisher NVARCHAR(255) NULL,
        DefaultVersionRule NVARCHAR(200) NULL,
        RiskLevel NVARCHAR(20) NOT NULL CONSTRAINT DF_F03EndpointSoftwareCatalog_RiskLevel DEFAULT('Review'),
        Status NVARCHAR(30) NOT NULL CONSTRAINT DF_F03EndpointSoftwareCatalog_Status DEFAULT('Draft'),
        EffectiveFrom DATETIME2(0) NULL,
        EffectiveTo DATETIME2(0) NULL,
        CreatedBy INT NULL,
        CreatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_F03EndpointSoftwareCatalog_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedBy INT NULL,
        UpdatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_F03EndpointSoftwareCatalog_UpdatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_F03EndpointSoftwareCatalog_Code UNIQUE(CatalogCode),
        CONSTRAINT CK_F03EndpointSoftwareCatalog_Risk CHECK(RiskLevel IN ('Low','Medium','High','Critical','Review')),
        CONSTRAINT CK_F03EndpointSoftwareCatalog_Status CHECK(Status IN ('Draft','SecurityReview','PendingApproval','Approved','Rejected','Retired'))
    );
    CREATE INDEX IX_F03EndpointSoftwareCatalog_Name ON dbo.F03EndpointSoftwareCatalog(NormalizedName, Publisher, Status);
END;
GO

IF OBJECT_ID('dbo.F03EndpointServiceCatalog','U') IS NULL
BEGIN
    CREATE TABLE dbo.F03EndpointServiceCatalog
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03EndpointServiceCatalog PRIMARY KEY,
        CatalogCode NVARCHAR(50) NOT NULL,
        ServiceName NVARCHAR(255) NOT NULL,
        DisplayName NVARCHAR(255) NULL,
        Publisher NVARCHAR(255) NULL,
        RequiredState NVARCHAR(30) NULL,
        RequiredStartMode NVARCHAR(30) NULL,
        RiskLevel NVARCHAR(20) NOT NULL CONSTRAINT DF_F03EndpointServiceCatalog_RiskLevel DEFAULT('Review'),
        Status NVARCHAR(30) NOT NULL CONSTRAINT DF_F03EndpointServiceCatalog_Status DEFAULT('Draft'),
        EffectiveFrom DATETIME2(0) NULL,
        EffectiveTo DATETIME2(0) NULL,
        CreatedBy INT NULL,
        CreatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_F03EndpointServiceCatalog_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedBy INT NULL,
        UpdatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_F03EndpointServiceCatalog_UpdatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_F03EndpointServiceCatalog_Code UNIQUE(CatalogCode),
        CONSTRAINT CK_F03EndpointServiceCatalog_Risk CHECK(RiskLevel IN ('Low','Medium','High','Critical','Review')),
        CONSTRAINT CK_F03EndpointServiceCatalog_Status CHECK(Status IN ('Draft','SecurityReview','PendingApproval','Approved','Rejected','Retired'))
    );
    CREATE INDEX IX_F03EndpointServiceCatalog_Name ON dbo.F03EndpointServiceCatalog(ServiceName, Status);
END;
GO

IF OBJECT_ID('dbo.F03EndpointSoftwareChangeRequests','U') IS NULL
BEGIN
    CREATE TABLE dbo.F03EndpointSoftwareChangeRequests
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03EndpointSoftwareChangeRequests PRIMARY KEY,
        CatalogId BIGINT NULL,
        Action NVARCHAR(20) NOT NULL,
        ProposedSoftwareName NVARCHAR(255) NOT NULL,
        ProposedPublisher NVARCHAR(255) NULL,
        ProposedVersionRule NVARCHAR(200) NULL,
        ScopeType NVARCHAR(30) NOT NULL CONSTRAINT DF_F03EndpointSoftwareChangeRequests_ScopeType DEFAULT('Company'),
        ScopeKey NVARCHAR(100) NULL,
        Reason NVARCHAR(2000) NOT NULL,
        SecurityReviewStatus NVARCHAR(30) NOT NULL CONSTRAINT DF_F03EndpointSoftwareChangeRequests_SecurityReviewStatus DEFAULT('Pending'),
        SecurityReviewerId INT NULL,
        SecurityReviewedAtUtc DATETIME2(0) NULL,
        SecurityReviewNote NVARCHAR(2000) NULL,
        ApprovalCaseId BIGINT NULL,
        Status NVARCHAR(30) NOT NULL CONSTRAINT DF_F03EndpointSoftwareChangeRequests_Status DEFAULT('Draft'),
        RequestedBy INT NULL,
        RequestedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_F03EndpointSoftwareChangeRequests_RequestedAt DEFAULT SYSUTCDATETIME(),
        CompletedAtUtc DATETIME2(0) NULL,
        CONSTRAINT FK_F03EndpointSoftwareChangeRequests_Catalog FOREIGN KEY(CatalogId) REFERENCES dbo.F03EndpointSoftwareCatalog(Id),
        CONSTRAINT CK_F03EndpointSoftwareChangeRequests_Action CHECK(Action IN ('Add','Update','Retire')),
        CONSTRAINT CK_F03EndpointSoftwareChangeRequests_Security CHECK(SecurityReviewStatus IN ('Pending','Approved','Rejected')),
        CONSTRAINT CK_F03EndpointSoftwareChangeRequests_Status CHECK(Status IN ('Draft','SecurityReview','PendingApproval','Approved','Rejected','Cancelled'))
    );
END;
GO

IF OBJECT_ID('dbo.F03EndpointSoftwareInstallRequests','U') IS NULL
BEGIN
    CREATE TABLE dbo.F03EndpointSoftwareInstallRequests
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03EndpointSoftwareInstallRequests PRIMARY KEY,
        EndpointDeviceId BIGINT NULL,
        EquipmentAssetId INT NULL,
        EmployeeCode NVARCHAR(50) NULL,
        CatalogId BIGINT NULL,
        RequestedSoftwareName NVARCHAR(255) NOT NULL,
        RequestedPublisher NVARCHAR(255) NULL,
        RequestedVersion NVARCHAR(100) NULL,
        Purpose NVARCHAR(2000) NOT NULL,
        RequiresSecurityReview BIT NOT NULL CONSTRAINT DF_F03EndpointSoftwareInstallRequests_RequiresSecurityReview DEFAULT(0),
        SecurityReviewStatus NVARCHAR(30) NOT NULL CONSTRAINT DF_F03EndpointSoftwareInstallRequests_SecurityReviewStatus DEFAULT('NotRequired'),
        SecurityReviewNote NVARCHAR(2000) NULL,
        SecurityReviewerId INT NULL,
        SecurityReviewedAtUtc DATETIME2(0) NULL,
        ApprovalCaseId BIGINT NULL,
        Status NVARCHAR(30) NOT NULL CONSTRAINT DF_F03EndpointSoftwareInstallRequests_Status DEFAULT('Draft'),
        AllowlistVersion BIGINT NULL,
        RequestedBy INT NULL,
        RequestedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_F03EndpointSoftwareInstallRequests_RequestedAt DEFAULT SYSUTCDATETIME(),
        ApprovedAtUtc DATETIME2(0) NULL,
        InstalledDetectedAtUtc DATETIME2(0) NULL,
        CompletedAtUtc DATETIME2(0) NULL,
        CONSTRAINT FK_F03EndpointSoftwareInstallRequests_Device FOREIGN KEY(EndpointDeviceId) REFERENCES dbo.F03EndpointDevices(Id),
        CONSTRAINT FK_F03EndpointSoftwareInstallRequests_Catalog FOREIGN KEY(CatalogId) REFERENCES dbo.F03EndpointSoftwareCatalog(Id),
        CONSTRAINT CK_F03EndpointSoftwareInstallRequests_Security CHECK(SecurityReviewStatus IN ('NotRequired','Pending','Approved','Rejected')),
        CONSTRAINT CK_F03EndpointSoftwareInstallRequests_Status CHECK(Status IN ('Draft','SecurityReview','PendingApproval','Approved','PendingInstallation','Installed','Rejected','Cancelled','NonCompliant'))
    );
    CREATE INDEX IX_F03EndpointSoftwareInstallRequests_Status ON dbo.F03EndpointSoftwareInstallRequests(Status, RequestedAtUtc);
END;
GO

IF OBJECT_ID('dbo.F03EndpointApprovalSnapshots','U') IS NULL
BEGIN
    CREATE TABLE dbo.F03EndpointApprovalSnapshots
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03EndpointApprovalSnapshots PRIMARY KEY,
        ApprovalCaseId BIGINT NOT NULL,
        RequestType NVARCHAR(50) NOT NULL,
        RequestId BIGINT NOT NULL,
        SnapshotVersion INT NOT NULL CONSTRAINT DF_F03EndpointApprovalSnapshots_Version DEFAULT(1),
        SnapshotJson NVARCHAR(MAX) NOT NULL,
        CreatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_F03EndpointApprovalSnapshots_CreatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_F03EndpointApprovalSnapshots_Case_Request UNIQUE(ApprovalCaseId, RequestType, RequestId, SnapshotVersion),
        CONSTRAINT CK_F03EndpointApprovalSnapshots_Json CHECK(ISJSON(SnapshotJson)=1)
    );
END;
GO

IF OBJECT_ID('dbo.F03EndpointIdentityHistory','U') IS NULL
BEGIN
    CREATE TABLE dbo.F03EndpointIdentityHistory
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_F03EndpointIdentityHistory PRIMARY KEY,
        EndpointDeviceId BIGINT NOT NULL,
        OldComputerName NVARCHAR(255) NULL,
        NewComputerName NVARCHAR(255) NULL,
        OldSerialNumber NVARCHAR(255) NULL,
        NewSerialNumber NVARCHAR(255) NULL,
        OldHardwareUuid NVARCHAR(255) NULL,
        NewHardwareUuid NVARCHAR(255) NULL,
        AgentInstallationId NVARCHAR(100) NULL,
        ChangeType NVARCHAR(50) NOT NULL,
        DetectedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_F03EndpointIdentityHistory_DetectedAt DEFAULT SYSUTCDATETIME(),
        ReviewStatus NVARCHAR(30) NOT NULL CONSTRAINT DF_F03EndpointIdentityHistory_ReviewStatus DEFAULT('Pending'),
        ReviewedBy INT NULL,
        ReviewedAtUtc DATETIME2(0) NULL,
        ReviewNote NVARCHAR(2000) NULL,
        CONSTRAINT FK_F03EndpointIdentityHistory_Device FOREIGN KEY(EndpointDeviceId) REFERENCES dbo.F03EndpointDevices(Id),
        CONSTRAINT CK_F03EndpointIdentityHistory_Review CHECK(ReviewStatus IN ('Pending','Confirmed','Rejected','Merged'))
    );
    CREATE INDEX IX_F03EndpointIdentityHistory_Device ON dbo.F03EndpointIdentityHistory(EndpointDeviceId, DetectedAtUtc DESC);
END;
GO

/* Add stable machine identity columns to the existing endpoint table. */
IF COL_LENGTH('dbo.F03EndpointDevices','AgentInstallationId') IS NULL
    ALTER TABLE dbo.F03EndpointDevices ADD AgentInstallationId NVARCHAR(100) NULL;
IF COL_LENGTH('dbo.F03EndpointDevices','HardwareUuid') IS NULL
    ALTER TABLE dbo.F03EndpointDevices ADD HardwareUuid NVARCHAR(255) NULL;
IF COL_LENGTH('dbo.F03EndpointDevices','IdentityStatus') IS NULL
    ALTER TABLE dbo.F03EndpointDevices ADD IdentityStatus NVARCHAR(30) NOT NULL CONSTRAINT DF_F03EndpointDevices_IdentityStatus DEFAULT('Verified');
IF COL_LENGTH('dbo.F03EndpointDevices','LastInventoryHash') IS NULL
    ALTER TABLE dbo.F03EndpointDevices ADD LastInventoryHash NVARCHAR(128) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UX_F03EndpointDevices_AgentInstallationId' AND object_id=OBJECT_ID('dbo.F03EndpointDevices'))
    CREATE UNIQUE INDEX UX_F03EndpointDevices_AgentInstallationId ON dbo.F03EndpointDevices(AgentInstallationId) WHERE AgentInstallationId IS NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UX_F03EndpointDevices_HardwareUuid' AND object_id=OBJECT_ID('dbo.F03EndpointDevices'))
    CREATE UNIQUE INDEX UX_F03EndpointDevices_HardwareUuid ON dbo.F03EndpointDevices(HardwareUuid) WHERE HardwareUuid IS NOT NULL;
GO
