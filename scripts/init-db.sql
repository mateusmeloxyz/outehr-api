-- ============================================
-- Database Creation
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'OutEHR')
BEGIN
    CREATE DATABASE OutEHR;
END
GO

USE OutEHR;
GO

-- ============================================
-- Identity Tables: Users and Roles
-- ============================================

CREATE TABLE Roles (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(200),
    CreatedAt DATETIME DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME DEFAULT GETUTCDATE()
);
GO

CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Email NVARCHAR(200) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(500) NOT NULL,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20),
    IsActive BIT DEFAULT 1,
    LastLogin DATETIME,
    CreatedAt DATETIME DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME DEFAULT GETUTCDATE()
);
GO

CREATE TABLE UserRoles (
    UserId INT NOT NULL,
    RoleId INT NOT NULL,
    AssignedAt DATETIME DEFAULT GETUTCDATE(),
    CONSTRAINT PK_UserRoles PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_UserRoles_User FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_UserRoles_Role FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE
);
GO

-- ============================================
-- Doctor / Schedule Tables
-- ============================================

CREATE TABLE Specialties (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(500),
    DefaultSlotDurationMinutes INT NOT NULL,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME DEFAULT GETUTCDATE()
);
GO

CREATE TABLE Clinics (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Address NVARCHAR(200) NOT NULL,
    City NVARCHAR(100) NOT NULL,
    State NVARCHAR(50) NOT NULL,
    ZipCode NVARCHAR(20) NOT NULL,
    Phone NVARCHAR(20),
    Email NVARCHAR(200),
    Latitude DECIMAL(10,6),
    Longitude DECIMAL(10,6),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME DEFAULT GETUTCDATE()
);
GO

CREATE TABLE Providers (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NULL,
    SpecialtyId INT NOT NULL,
    ClinicId INT NOT NULL,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    NPI NVARCHAR(20) NULL,
    Phone NVARCHAR(20),
    Email NVARCHAR(200),
    Rating DECIMAL(3,1) DEFAULT 0,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Provider_Specialty FOREIGN KEY (SpecialtyId) REFERENCES Specialties(Id),
    CONSTRAINT FK_Provider_Clinic FOREIGN KEY (ClinicId) REFERENCES Clinics(Id),
    CONSTRAINT FK_Provider_User FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL
);
GO

CREATE TABLE Schedules (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ProviderId INT NOT NULL,
    DayOfWeek TINYINT NOT NULL,
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    IsActive BIT DEFAULT 1,
    EffectiveFrom DATE NULL,
    EffectiveTo DATE NULL,
    CreatedAt DATETIME DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Schedule_Provider FOREIGN KEY (ProviderId) REFERENCES Providers(Id) ON DELETE CASCADE,
    CONSTRAINT CHK_Schedule_DayOfWeek CHECK (DayOfWeek BETWEEN 0 AND 6),
    CONSTRAINT CHK_Schedule_TimeRange CHECK (StartTime < EndTime),
    CONSTRAINT CHK_Schedule_UniquePerDay UNIQUE (ProviderId, DayOfWeek, EffectiveFrom)
);
GO

CREATE TABLE AvailabilityExceptions (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ProviderId INT NOT NULL,
    ExceptionDate DATE NOT NULL,
    StartTime TIME NULL,
    EndTime TIME NULL,
    Reason NVARCHAR(200),
    IsUnavailable BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Exception_Provider FOREIGN KEY (ProviderId) REFERENCES Providers(Id) ON DELETE CASCADE,
    CONSTRAINT CHK_Exception_TimeRange CHECK (StartTime IS NULL OR EndTime IS NULL OR StartTime < EndTime)
);
GO

-- ============================================
-- Indexes
-- ============================================

CREATE INDEX IX_Providers_SpecialtyId ON Providers(SpecialtyId);
CREATE INDEX IX_Providers_ClinicId ON Providers(ClinicId);
CREATE INDEX IX_Schedules_ProviderId ON Schedules(ProviderId);
CREATE INDEX IX_Schedules_DateRange ON Schedules(EffectiveFrom, EffectiveTo);
CREATE INDEX IX_Exceptions_ProviderId_Date ON AvailabilityExceptions(ProviderId, ExceptionDate);
CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Users_Phone ON Users(Phone);

-- ============================================
-- View for Common Queries
-- ============================================

CREATE VIEW v_ProviderFullDetails AS
SELECT 
    p.Id AS ProviderId,
    p.FirstName + ' ' + p.LastName AS FullName,
    p.NPI,
    p.Rating,
    s.Name AS Specialty,
    s.DefaultSlotDurationMinutes,
    c.Id AS ClinicId,
    c.Name AS ClinicName,
    c.Address,
    c.City,
    c.State,
    c.ZipCode,
    c.Latitude,
    c.Longitude
FROM Providers p
INNER JOIN Specialties s ON p.SpecialtyId = s.Id
INNER JOIN Clinics c ON p.ClinicId = c.Id
WHERE p.IsActive = 1;
GO

-- ============================================
-- Seed Data
-- ============================================

INSERT INTO Roles (Name, Description) VALUES 
    ('Admin', 'Full system access'),
    ('Doctor', 'Healthcare provider access'),
    ('Patient', 'Patient self-service access');
GO

INSERT INTO Specialties (Name, Description, DefaultSlotDurationMinutes)
VALUES 
    ('Orthopedics', 'Bone and joint disorders', 15),
    ('Physical Therapy', 'Rehabilitation and movement', 30),
    ('Pain Management', 'Chronic pain treatment', 30),
    ('Orthopedic Surgery', 'Surgical treatment of bones and joints', 60),
    ('Sports Medicine', 'Injury prevention and treatment for athletes', 30),
    ('Occupational Health', 'Workplace injury and wellness', 30);
GO

INSERT INTO Clinics (Name, Address, City, State, ZipCode, Phone, Latitude, Longitude)
VALUES 
    ('Westside Wellness Center', '3201 Wilshire Blvd', 'Los Angeles', 'CA', '90010', '(310) 555-0101', 34.0619, -118.2926),
    ('Sunset Health Partners', '8535 Sunset Blvd', 'West Hollywood', 'CA', '90069', '(310) 555-0102', 34.0912, -118.3758),
    ('Harbor City Medical Group', '9101 S Sepulveda Blvd', 'Los Angeles', 'CA', '90045', '(310) 555-0103', 33.9555, -118.3963);
GO

INSERT INTO Providers (SpecialtyId, ClinicId, FirstName, LastName, Email, Rating)
VALUES 
    (1, 1, 'Sarah', 'Chen', 'sarah.chen@outehr.com', 4.8),
    (2, 1, 'Michael', 'Torres', 'michael.torres@outehr.com', 4.6),
    (1, 2, 'Emily', 'Nakamura', 'emily.nakamura@outehr.com', 4.9),
    (2, 2, 'James', 'Rodriguez', 'james.rodriguez@outehr.com', 4.7),
    (3, 3, 'Lisa', 'Park', 'lisa.park@outehr.com', 4.5),
    (2, 3, 'Robert', 'Kim', 'robert.kim@outehr.com', 4.4);
GO

INSERT INTO Users (Email, PasswordHash, FirstName, LastName, Phone)
VALUES ('admin@outehr.com', 'ADMIN_PLACEHOLDER_HASH', 'System', 'Administrator', '(555) 000-0000');
GO

INSERT INTO UserRoles (UserId, RoleId)
VALUES (1, 1);
GO

INSERT INTO Schedules (ProviderId, DayOfWeek, StartTime, EndTime, EffectiveFrom)
VALUES 
    (1, 1, '09:00:00', '17:00:00', GETDATE()),
    (1, 2, '09:00:00', '17:00:00', GETDATE()),
    (1, 3, '09:00:00', '17:00:00', GETDATE()),
    (1, 4, '09:00:00', '17:00:00', GETDATE()),
    (1, 5, '09:00:00', '17:00:00', GETDATE());
GO

INSERT INTO AvailabilityExceptions (ProviderId, ExceptionDate, Reason, IsUnavailable)
VALUES (1, '2026-07-04', 'Independence Day holiday', 1);
GO