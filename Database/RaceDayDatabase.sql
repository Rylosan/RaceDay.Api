-- =========================================================
-- RaceDay Database Script
-- Part 1 - Database Design
-- SQL Server
-- =========================================================

CREATE DATABASE RaceDayDb;
GO

USE RaceDayDb;
GO

-- =========================================================
-- Users
-- =========================================================

CREATE TABLE Users
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    Role NVARCHAR(20) NOT NULL,
    Phone NVARCHAR(20) NULL,
    DateOfBirth DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL
);
GO

-- =========================================================
-- Events
-- =========================================================

CREATE TABLE Events
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrganiserId INT NOT NULL,
    Name NVARCHAR(150) NOT NULL,
    Description NVARCHAR(1000) NULL,
    EventDate DATETIME2 NOT NULL,
    Location NVARCHAR(200) NOT NULL,
    DistanceKm DECIMAL(8,2) NOT NULL,
    EventType NVARCHAR(50) NOT NULL,

    CONSTRAINT FK_Events_Users
        FOREIGN KEY (OrganiserId)
        REFERENCES Users(Id)
);
GO

-- =========================================================
-- Categories
-- =========================================================

CREATE TABLE Categories
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EventId INT NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,

    CONSTRAINT FK_Categories_Events
        FOREIGN KEY (EventId)
        REFERENCES Events(Id)
        ON DELETE CASCADE
);
GO

-- =========================================================
-- Enrollments
-- =========================================================

CREATE TABLE Enrollments
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EventId INT NOT NULL,
    ParticipantId INT NOT NULL,
    CategoryId INT NOT NULL,
    EnrolledAt DATETIME2 NOT NULL,

    CONSTRAINT FK_Enrollments_Events
        FOREIGN KEY (EventId)
        REFERENCES Events(Id),

    CONSTRAINT FK_Enrollments_Users
        FOREIGN KEY (ParticipantId)
        REFERENCES Users(Id),

    CONSTRAINT FK_Enrollments_Categories
        FOREIGN KEY (CategoryId)
        REFERENCES Categories(Id),

    CONSTRAINT UQ_Enrollments_Event_Participant
        UNIQUE (EventId, ParticipantId)
);
GO

-- =========================================================
-- Results
-- =========================================================

CREATE TABLE Results
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EnrollmentId INT NOT NULL UNIQUE,
    FinishTime TIME NOT NULL,
    Position INT NOT NULL,
    RecordedAt DATETIME2 NOT NULL,

    CONSTRAINT FK_Results_Enrollments
        FOREIGN KEY (EnrollmentId)
        REFERENCES Enrollments(Id)
        ON DELETE CASCADE
);
GO

-- =========================================================
-- Route Information
-- =========================================================

CREATE TABLE RouteInfos
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EventId INT NOT NULL UNIQUE,
    StartPoint NVARCHAR(200) NOT NULL,
    FinishPoint NVARCHAR(200) NOT NULL,
    RouteDescription NVARCHAR(2000) NULL,
    ElevationGainMetres INT NULL,

    CONSTRAINT FK_RouteInfos_Events
        FOREIGN KEY (EventId)
        REFERENCES Events(Id)
        ON DELETE CASCADE
);
GO

-- =========================================================
-- Sample Data
-- =========================================================

INSERT INTO Users
    (FirstName, LastName, Email, PasswordHash, Role, Phone, DateOfBirth, CreatedAt)
VALUES
    ('Race', 'Organiser', 'organiser@raceday.co.za',
     'SampleHashForDatabaseScript', 'Organiser',
     '0712345678', '1995-01-15', GETUTCDATE()),

    ('Race', 'Participant', 'participant@raceday.co.za',
     'SampleHashForDatabaseScript', 'Participant',
     '0723456789', '2000-05-20', GETUTCDATE());
GO

INSERT INTO Events
    (OrganiserId, Name, Description, EventDate, Location, DistanceKm, EventType)
VALUES
    (1, 'Cape Town 10K',
     'A 10 km road running event.',
     '2026-11-21 07:00:00',
     'Cape Town', 10.00, 'Running');
GO

INSERT INTO Categories
    (EventId, Name, Description)
VALUES
    (1, 'Open 10K', 'Open category for the 10 km race.'),
    (1, 'Veteran 10K', 'Veteran category for the 10 km race.');
GO

INSERT INTO Enrollments
    (EventId, ParticipantId, CategoryId, EnrolledAt)
VALUES
    (1, 2, 1, GETUTCDATE());
GO

INSERT INTO Results
    (EnrollmentId, FinishTime, Position, RecordedAt)
VALUES
    (1, '00:51:45', 1, GETUTCDATE());
GO

INSERT INTO RouteInfos
    (EventId, StartPoint, FinishPoint, RouteDescription, ElevationGainMetres)
VALUES
    (1,
     'Cape Town Stadium',
     'Cape Town Stadium',
     'A 10 km road route through the Cape Town city area, returning to the stadium.',
     85);
GO

-- =========================================================
-- View Sample Data
-- =========================================================

SELECT * FROM Users;
SELECT * FROM Events;
SELECT * FROM Categories;
SELECT * FROM Enrollments;
SELECT * FROM Results;
SELECT * FROM RouteInfos;
GO