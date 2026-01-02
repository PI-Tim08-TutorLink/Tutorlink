-- DROP DATABASE TutorLink;
-- GO

CREATE DATABASE TutorLink;
GO
USE TutorLink;
GO

---------------------------------------------------------
-- TABLE: Role
---------------------------------------------------------
CREATE TABLE [dbo].[Role] (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Role NVARCHAR(50) NOT NULL
);

---------------------------------------------------------
-- TABLE: User
---------------------------------------------------------
CREATE TABLE [dbo].[User] (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CreatedAt DATETIME NOT NULL,
    DeletedAt DATETIME NULL,

    Username NVARCHAR(50) NOT NULL UNIQUE,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,

    PwdHash NVARCHAR(200) NOT NULL,
    PwdSalt NVARCHAR(200) NOT NULL,

    ResetToken NVARCHAR(200) NULL,
    ResetTokenExpiry DATETIME NULL,

    RoleId INT NOT NULL,
    CONSTRAINT FK_User_Role 
        FOREIGN KEY (RoleId) REFERENCES [dbo].[Role](Id)
);

---------------------------------------------------------
-- TABLE: Tutor
---------------------------------------------------------
CREATE TABLE [dbo].[Tutor] (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    Skill NVARCHAR(255) NOT NULL,

    HourlyRate DECIMAL(10,2) NULL,
    AverageRating DECIMAL(3,2) NULL,
    TotalReviews INT NOT NULL DEFAULT 0,
    Bio NVARCHAR(1000) NULL,
    Availability NVARCHAR(500) NULL,

    CreatedAt DATETIME NOT NULL,
    DeletedAt DATETIME NULL,

    CONSTRAINT FK_Tutor_User 
        FOREIGN KEY (UserId) REFERENCES [dbo].[User](Id),

    CONSTRAINT CK_Tutor_Rating 
        CHECK (AverageRating >= 0 AND AverageRating <= 5),

    CONSTRAINT CK_Tutor_HourlyRate 
        CHECK (HourlyRate > 0)
);

---------------------------------------------------------
-- Insert Roles
---------------------------------------------------------
INSERT INTO Role (Role)
VALUES ('Admin'), ('User'), ('Tutor');

---------------------------------------------------------
-- Insert Default Users (password = 'password')
---------------------------------------------------------
INSERT INTO [dbo].[User] (
    CreatedAt, DeletedAt,
    Username, FirstName, LastName, Email,
    PwdHash, PwdSalt,
    ResetToken, ResetTokenExpiry,
    RoleId
)
VALUES
(
    GETDATE(), NULL,
    'admin', 'Admin', 'User', 'admin@admin.com',
    'mJCzV/5rj3TkTQj5NpBjFGsc8uc+q0/FwOdjxdhwors=',
    '/1QGdspvYx6YtLRHY+yISdnQvdLhACHHfVO/38o464o=',
    NULL, NULL,
    1
),
(
    GETDATE(), NULL,
    'tutor', 'Tutor', 'User', 'tutor@tutor.com',
    'y0a2GHK4lKgJSO85hlgYmIu0uWHhTKahPaRdAdzcynY=',
    'AE+AIWUI0gEHK6X/5l0T5bFFtNnoOh/d8cD2GiNkiZ8=',
    NULL, NULL,
    3
),
(
    GETDATE(), NULL,
    'user', 'User', 'User', 'user@user.com',
    'mTxtwI93yaultGJSaB/4GSELXvzHDsqRB/RATay6Mco=',
    'sLuOx2OTxxRQQpQr1SJN48dP2CfrxQRkUJ6nhAln5XI=',
    NULL, NULL,
    2
);

---------------------------------------------------------
-- Insert Tutor Row
---------------------------------------------------------
DECLARE @TutorUserId INT =
(
    SELECT Id FROM [dbo].[User] WHERE Email = 'tutor@tutor.com'
);

IF @TutorUserId IS NOT NULL
BEGIN
    INSERT INTO [dbo].[Tutor] (
        UserId, Skill, HourlyRate, AverageRating, TotalReviews,
        Bio, Availability, CreatedAt, DeletedAt
    )
    VALUES (
        @TutorUserId,
        'Mathematics, Physics, Programming',
        25.00,
        4.5,
        10,
        'Experienced tutor with 5+ years of teaching.',
        'Mon-Fri 16:00-20:00',
        GETDATE(),
        NULL
    );
END
GO
