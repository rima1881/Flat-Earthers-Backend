-- mysql

DROP TABLE
    `flatearthers-main`.`Users`,
    `flatearthers-main`.`Targets`,
    `flatearthers-main`.`UsersTargets`;

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
    Latitude INT NOT NULL,
    Longitude INT NOT NULL,
    MinCloudCover DECIMAL(10, 2) NULL,
    MaxCloudCover DECIMAL(10, 2) NULL,
    NotificationOffset DATETIME NOT NULL
);

CREATE TABLE UsersTargets (
    UserGuid CHAR(36),
    TargetGuid CHAR(36),
    CONSTRAINT FK_UsersTargets_Users FOREIGN KEY (UserGuid) REFERENCES Users(UserGuid),
    CONSTRAINT FK_UsersTargets_Targets FOREIGN KEY (TargetGuid) REFERENCES Targets(TargetGuid),
    PRIMARY KEY (UserGuid, TargetGuid)
);