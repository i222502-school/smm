-- =============================================================================
-- Societies Management System - SQL Server Database Schema
-- Run against SQL Server (2019+ recommended). Creates DB + objects + seed.
-- =============================================================================

IF DB_ID(N'SocietiesManagement') IS NOT NULL
BEGIN
    ALTER DATABASE SocietiesManagement SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE SocietiesManagement;
END;
GO

CREATE DATABASE SocietiesManagement;
GO

USE SocietiesManagement;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- -----------------------------------------------------------------------------
-- Core tables
-- -----------------------------------------------------------------------------

CREATE TABLE dbo.Users (
    UserID          INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Username        NVARCHAR(50)  NOT NULL UNIQUE,
    PasswordHash    VARBINARY(64) NOT NULL,
    Salt            UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Users_Salt DEFAULT NEWID(),
    Email           NVARCHAR(100) NOT NULL,
    FullName        NVARCHAR(150) NOT NULL,
    UserType        NVARCHAR(20)  NOT NULL
        CONSTRAINT CK_Users_UserType CHECK (UserType IN (N'Admin', N'Student')),
    IsActive        BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1),
    CreatedAt       DATETIME2(0) NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT CK_Users_Email CHECK (Email LIKE N'%_@_%.__%')
);

CREATE INDEX IX_Users_UserType ON dbo.Users(UserType);
CREATE INDEX IX_Users_IsActive ON dbo.Users(IsActive);

CREATE TABLE dbo.Societies (
    SocietyID       INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Name            NVARCHAR(120) NOT NULL,
    Description     NVARCHAR(MAX) NOT NULL,
    Status          NVARCHAR(20)  NOT NULL
        CONSTRAINT CK_Societies_Status CHECK (Status IN (N'Pending', N'Approved', N'Suspended', N'Rejected')),
    CreatedByUserID INT NOT NULL,
    CreatedAt       DATETIME2(0) NOT NULL CONSTRAINT DF_Societies_CreatedAt DEFAULT SYSUTCDATETIME(),
    ApprovedByUserID INT NULL,
    ApprovedAt      DATETIME2(0) NULL,
    CONSTRAINT FK_Societies_Creator FOREIGN KEY (CreatedByUserID) REFERENCES dbo.Users(UserID),
    CONSTRAINT FK_Societies_Approver FOREIGN KEY (ApprovedByUserID) REFERENCES dbo.Users(UserID)
);

CREATE UNIQUE INDEX UX_Societies_Name_Approved ON dbo.Societies(Name) WHERE Status = N'Approved';

CREATE TABLE dbo.Memberships (
    MembershipID    INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserID          INT NOT NULL,
    SocietyID       INT NOT NULL,
    RoleInSociety   NVARCHAR(20) NOT NULL
        CONSTRAINT CK_Memberships_Role CHECK (RoleInSociety IN (N'Head', N'Member')),
    Status          NVARCHAR(20) NOT NULL
        CONSTRAINT CK_Memberships_Status CHECK (Status IN (N'Pending', N'Approved', N'Rejected')),
    RequestedAt     DATETIME2(0) NOT NULL CONSTRAINT DF_Memberships_RequestedAt DEFAULT SYSUTCDATETIME(),
    ResolvedAt      DATETIME2(0) NULL,
    CONSTRAINT FK_Memberships_User FOREIGN KEY (UserID) REFERENCES dbo.Users(UserID) ON DELETE CASCADE,
    CONSTRAINT FK_Memberships_Society FOREIGN KEY (SocietyID) REFERENCES dbo.Societies(SocietyID) ON DELETE CASCADE,
    CONSTRAINT UX_Memberships_User_Society UNIQUE (UserID, SocietyID)
);

CREATE INDEX IX_Memberships_Society_Status ON dbo.Memberships(SocietyID, Status);

CREATE TABLE dbo.Events (
    EventID         INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    SocietyID       INT NOT NULL,
    Title           NVARCHAR(200) NOT NULL,
    Description     NVARCHAR(MAX) NOT NULL,
    Venue           NVARCHAR(200) NOT NULL,
    EventStart      DATETIME2(0) NOT NULL,
    EventEnd        DATETIME2(0) NOT NULL,
    MaxParticipants INT NULL,
    Status          NVARCHAR(30) NOT NULL
        CONSTRAINT CK_Events_Status CHECK (Status IN (N'Draft', N'PendingAdminApproval', N'Approved', N'Cancelled')),
    CreatedByUserID INT NOT NULL,
    CreatedAt       DATETIME2(0) NOT NULL CONSTRAINT DF_Events_CreatedAt DEFAULT SYSUTCDATETIME(),
    ApprovedByUserID INT NULL,
    ApprovedAt      DATETIME2(0) NULL,
    CONSTRAINT FK_Events_Society FOREIGN KEY (SocietyID) REFERENCES dbo.Societies(SocietyID) ON DELETE CASCADE,
    CONSTRAINT FK_Events_Creator FOREIGN KEY (CreatedByUserID) REFERENCES dbo.Users(UserID),
    CONSTRAINT FK_Events_Approver FOREIGN KEY (ApprovedByUserID) REFERENCES dbo.Users(UserID),
    CONSTRAINT CK_Events_EndAfterStart CHECK (EventEnd > EventStart)
);

CREATE INDEX IX_Events_Society ON dbo.Events(SocietyID);
CREATE INDEX IX_Events_Status ON dbo.Events(Status);
CREATE INDEX IX_Events_Start ON dbo.Events(EventStart);

CREATE TABLE dbo.EventRegistrations (
    RegistrationID  INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    EventID         INT NOT NULL,
    UserID          INT NOT NULL,
    RegisteredAt    DATETIME2(0) NOT NULL CONSTRAINT DF_EventRegistrations_RegAt DEFAULT SYSUTCDATETIME(),
    TicketCode      UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_EventRegistrations_Ticket DEFAULT NEWID(),
    CONSTRAINT FK_EventRegistrations_Event FOREIGN KEY (EventID) REFERENCES dbo.Events(EventID) ON DELETE CASCADE,
    CONSTRAINT FK_EventRegistrations_User FOREIGN KEY (UserID) REFERENCES dbo.Users(UserID),
    CONSTRAINT UX_EventRegistrations_Event_User UNIQUE (EventID, UserID)
);

CREATE INDEX IX_EventRegistrations_User ON dbo.EventRegistrations(UserID);

CREATE TABLE dbo.Tasks (
    TaskID          INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    SocietyID       INT NOT NULL,
    Title           NVARCHAR(200) NOT NULL,
    Description     NVARCHAR(MAX) NOT NULL,
    AssignedByUserID INT NOT NULL,
    AssignedToUserID INT NOT NULL,
    DueDate         DATETIME2(0) NULL,
    Status          NVARCHAR(20) NOT NULL
        CONSTRAINT CK_Tasks_Status CHECK (Status IN (N'Pending', N'InProgress', N'Completed')),
    CreatedAt       DATETIME2(0) NOT NULL CONSTRAINT DF_Tasks_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Tasks_Society FOREIGN KEY (SocietyID) REFERENCES dbo.Societies(SocietyID) ON DELETE CASCADE,
    CONSTRAINT FK_Tasks_Assigner FOREIGN KEY (AssignedByUserID) REFERENCES dbo.Users(UserID),
    CONSTRAINT FK_Tasks_Assignee FOREIGN KEY (AssignedToUserID) REFERENCES dbo.Users(UserID)
);

CREATE INDEX IX_Tasks_Society ON dbo.Tasks(SocietyID);
CREATE INDEX IX_Tasks_Assignee ON dbo.Tasks(AssignedToUserID);

CREATE TABLE dbo.Announcements (
    AnnouncementID  INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    SocietyID       INT NULL,
    PostedByUserID  INT NOT NULL,
    Title           NVARCHAR(200) NOT NULL,
    Body            NVARCHAR(MAX) NOT NULL,
    PostedAt        DATETIME2(0) NOT NULL CONSTRAINT DF_Announcements_PostedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Announcements_Society FOREIGN KEY (SocietyID) REFERENCES dbo.Societies(SocietyID) ON DELETE CASCADE,
    CONSTRAINT FK_Announcements_User FOREIGN KEY (PostedByUserID) REFERENCES dbo.Users(UserID)
);

CREATE INDEX IX_Announcements_Society ON dbo.Announcements(SocietyID);
CREATE INDEX IX_Announcements_PostedAt ON dbo.Announcements(PostedAt DESC);

CREATE TABLE dbo.ActivityLog (
    LogID           BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserID          INT NULL,
    ActionType      NVARCHAR(80) NOT NULL,
    EntityType      NVARCHAR(80) NULL,
    EntityId        INT NULL,
    Details         NVARCHAR(MAX) NULL,
    LoggedAt        DATETIME2(0) NOT NULL CONSTRAINT DF_ActivityLog_LoggedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_ActivityLog_User FOREIGN KEY (UserID) REFERENCES dbo.Users(UserID)
);

CREATE INDEX IX_ActivityLog_LoggedAt ON dbo.ActivityLog(LoggedAt DESC);
CREATE INDEX IX_ActivityLog_Action ON dbo.ActivityLog(ActionType);

GO

-- -----------------------------------------------------------------------------
-- Seed: admin account (password: Admin@123) — hash matches C# PasswordHasher
-- -----------------------------------------------------------------------------
DECLARE @adminSalt UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';

INSERT INTO dbo.Users (Username, PasswordHash, Salt, Email, FullName, UserType, IsActive)
VALUES (
    N'admin',
    HASHBYTES('SHA2_256', CONVERT(NVARCHAR(MAX), CONCAT(N'Admin@123', CAST(@adminSalt AS NVARCHAR(36))))),
    @adminSalt,
    N'admin@university.edu',
    N'System Administrator',
    N'Admin',
    1
);

-- Demo student (password: Student@123)
DECLARE @stuSalt UNIQUEIDENTIFIER = '22222222-2222-2222-2222-222222222222';
INSERT INTO dbo.Users (Username, PasswordHash, Salt, Email, FullName, UserType, IsActive)
VALUES (
    N'student1',
    HASHBYTES('SHA2_256', CONVERT(NVARCHAR(MAX), CONCAT(N'Student@123', CAST(@stuSalt AS NVARCHAR(36))))),
    @stuSalt,
    N'student1@university.edu',
    N'Demo Student',
    N'Student',
    1
);

GO

-- -----------------------------------------------------------------------------
-- Demo societies (approved; student1 is head of each for showcase)
-- -----------------------------------------------------------------------------
DECLARE @adminId INT = (SELECT UserID FROM dbo.Users WHERE Username = N'admin');
DECLARE @stuId   INT = (SELECT UserID FROM dbo.Users WHERE Username = N'student1');

INSERT INTO dbo.Societies (Name, Description, Status, CreatedByUserID, ApprovedByUserID, ApprovedAt)
VALUES
    (N'Gaming Society', N'LAN nights, esports, and game design jams.', N'Approved', @stuId, @adminId, SYSUTCDATETIME()),
    (N'Sports Society', N'Inter-society tournaments and fitness sessions.', N'Approved', @stuId, @adminId, SYSUTCDATETIME()),
    (N'Developers Club', N'Workshops, hackathons, and open-source contributions.', N'Approved', @stuId, @adminId, SYSUTCDATETIME()),
    (N'Literary Society', N'Book circles, poetry slams, and writing workshops.', N'Approved', @stuId, @adminId, SYSUTCDATETIME()),
    (N'Media Society', N'Film, photography, and campus broadcasting.', N'Approved', @stuId, @adminId, SYSUTCDATETIME()),
    (N'Photography Club (pending demo)', N'Awaits admin approval — demonstrates Pending workflow.', N'Pending', @stuId, NULL, NULL);

INSERT INTO dbo.Memberships (UserID, SocietyID, RoleInSociety, Status, ResolvedAt)
SELECT @stuId, SocietyID, N'Head', N'Approved', SYSUTCDATETIME()
FROM dbo.Societies WHERE Name = N'Photography Club (pending demo)';

DECLARE @sid INT;
DECLARE c CURSOR LOCAL FAST_FORWARD FOR
    SELECT SocietyID FROM dbo.Societies WHERE Status = N'Approved';
OPEN c;
FETCH NEXT FROM c INTO @sid;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.Memberships WHERE SocietyID = @sid AND UserID = @stuId)
        INSERT INTO dbo.Memberships (UserID, SocietyID, RoleInSociety, Status, ResolvedAt)
        VALUES (@stuId, @sid, N'Head', N'Approved', SYSUTCDATETIME());
    FETCH NEXT FROM c INTO @sid;
END;
CLOSE c;
DEALLOCATE c;

DECLARE @gamingId INT = (SELECT SocietyID FROM dbo.Societies WHERE Name = N'Gaming Society');
INSERT INTO dbo.Announcements (SocietyID, PostedByUserID, Title, Body)
VALUES (@gamingId, @stuId, N'Welcome week', N'Join us Friday at 6pm in Lab 3 for introductory session.');

PRINT N'SocietiesManagement database created successfully.';
PRINT N'Seed logins: admin / Admin@123  |  student1 / Student@123';
GO
