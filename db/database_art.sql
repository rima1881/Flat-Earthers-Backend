use master;

DROP DATABASE flatEarthers;

CREATE DATABASE flatEarthers;

use flatEarthers;

CREATE TABLE Users(
	UserID int IDENTITY(1,1),
	Email varchar(255) Unique NOT NULL,
	PRIMARY KEY (UserID)
);

CREATE TABLE Targets(
	TargetID int IDENTITY(1,1),
	UserID int FOREIGN KEY REFERENCES Users(UserID),
	LAT int,
	LNG int,
	posX int,
	posY int,
	Prediction DATETIME,
	ImgUrl varchar(255),
	PRIMARY KEY (TargetID)
);