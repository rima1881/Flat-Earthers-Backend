-- mysql

DROP TABLE
    `flatearthers-main`.`Users`,
    `flatearthers-main`.`Targets`,
    `flatearthers-main`.`UsersTargets`,
    `flatearthers-main`.`Predictions`;

CREATE TABLE Users (
    UserGuid CHAR(36) PRIMARY KEY,
    Email VARCHAR(255) UNIQUE NOT NULL,
    PasswordHash VARCHAR(255),
    EmailEnabled TINYINT(1) NOT NULL DEFAULT '1'
);

CREATE TABLE Targets (
    TargetGuid CHAR(36) PRIMARY KEY,
    ScenePath INT NOT NULL,
    SceneRow INT NOT NULL,
    Latitude DECIMAL(10, 7) NOT NULL,
    Longitude DECIMAL(10, 7) NOT NULL, 
    MinCloudCover DECIMAL(7, 5) NULL,
    MaxCloudCover DECIMAL(7,5) NULL,
    NotificationOffset DATETIME NOT NULL
);

CREATE TABLE UsersTargets (
    UserGuid CHAR(36),
    TargetGuid CHAR(36),
    CONSTRAINT FK_UsersTargets_Users FOREIGN KEY (UserGuid) REFERENCES Users(UserGuid),
    CONSTRAINT FK_UsersTargets_Targets FOREIGN KEY (TargetGuid) REFERENCES Targets(TargetGuid),
    PRIMARY KEY (UserGuid, TargetGuid)
);

CREATE TABLE Notification (
    ScenePath INT NOT NULL,
    SceneRow INT NOT NULL,
    UserGuid CHAR(36) NOT NULL,
    TargetGuid CHAR(36) NOT NULL,
    PredictedAcquisitionDate DATETIME,
    HasBeenNotified TINYINT(1) NOT NULL DEFAULT '0',
    PRIMARY KEY (ScenePath, SceneRow, UserGuid, TargetGuid, PredictedAcquisitionDate)
);

CREATE TABLE UserTargetNotifications (
    ScenePath INT NOT NULL,
    SceneRow INT NOT NULL,
    UserGuid CHAR(36) NOT NULL,
    TargetGuid CHAR(36) NOT NULL,
    PredictedAcquisitionDate DATETIME NOT NULL,
    HasBeenNotified TINYINT(1) NOT NULL DEFAULT '0',
    PRIMARY KEY (ScenePath, SceneRow, UserGuid, TargetGuid,PredictedAcquisitionDate),
    CONSTRAINT FK_UserTargetNotifications_Users FOREIGN KEY (UserGuid) REFERENCES Users(UserGuid),
    CONSTRAINT FK_UserTargetNotifications_Targets FOREIGN KEY (TargetGuid) REFERENCES Targets(TargetGuid)
);

CREATE TABLE Predictions (
    ScenePath INT NOT NULL,
    SceneRow INT NOT NULL,
    PredictionDataJson JSON,
    PRIMARY KEY (ScenePath, SceneRow)
);