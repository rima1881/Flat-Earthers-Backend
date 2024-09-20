
-- Warning!!! The following query will drop and recreate the database.
-- It should only be used during the development phase for database design.

use master;


IF EXISTS(SELECT * FROM master.sys.databases 
          WHERE name='flatEarhters')
BEGIN
    DROP DATABASE flatEarthers;
END


CREATE DATABASE flatEarthers;

use flatEarthers;

CREATE TABLE Users(
	UserID INT IDENTITY(1,1),
	Email VARCHAR(255) UNIQUE NOT NULL,
	SendEmail BOOLEAN NOT NULL DEFAULT TRUE,
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
	CloudCover int,
	ImgUrl varchar(255) NOT NULL,
	PRIMARY KEY (TargetID)
);