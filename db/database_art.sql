
-- Warning!!! The following query will drop and recreate the database.
-- It should only be used during the development phase for database design.

use master;

IF EXISTS (SELECT * FROM sys.databases WHERE name = 'flatEarthers')
BEGIN
    DROP DATABASE flatEarthers;
END

CREATE DATABASE flatEarthers;

use flatEarthers;

CREATE TABLE Users(
	UserID INT IDENTITY(1,1),
	Email VARCHAR(255) UNIQUE NOT NULL,
	PWD VARCHAR(255),
	SendEmail BIT NOT NULL DEFAULT '1',
	PRIMARY KEY (UserID)
);


-- ( ROW , PATH ) is renamed to ( posX , posY ) because they are keyword in SQL

-- BOTH ( POSX , POSY ) AND ( LAT , LNG ) are included
-- REASONING behind this was for the 3*3 grid

CREATE TABLE Targets(
	TargetID int IDENTITY(1,1),
	UserID int FOREIGN KEY REFERENCES Users(UserID),
	LAT int NOT NULL,
	LNG int NOT NULL,
	posX int NOT NULL, 	-- ROW 
	posY int NOT NULL, 	-- PATH
	NotificationOffset DATETIME,
	Prediction DATETIME,
	CloudCover int,
	PRIMARY KEY (TargetID)
);