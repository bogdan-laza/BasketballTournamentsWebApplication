USE BasketballTournamentsApp
DROP TABLE IF EXISTS Game;
DROP TABLE IF EXISTS TournamentTeam;
DROP TABLE IF EXISTS TeamPerson;
DROP TABLE IF EXISTS Team;
DROP TABLE IF EXISTS Tournament;
DROP TABLE IF EXISTS Person;


GO

CREATE TABLE Person(
	UserId INT IDENTITY(1, 1) PRIMARY KEY,
	Username NVARCHAR(50) UNIQUE,
	FirstName NVARCHAR(100),
	LastName NVARCHAR(100),
	PasswordHash NVARCHAR(255),
	Email NVARCHAR(150) UNIQUE,
	PhoneNumber NVARCHAR(15) NULL,
	RefreshToken NVARCHAR(500) NULL,
	CreatedAt DATETIME DEFAULT GETDATE(),
	PersonRole NVARCHAR(50) DEFAULT 'Player',	
	IsEmailVerified BIT DEFAULT 0
);

INSERT INTO Person(Username, PasswordHash, Email, PhoneNumber) values ('bogdy12', 'hjgfdsj123rfvk', 'lazabogdan12@gmail.com', '0748138908')

select * from Person

CREATE TABLE Tournament(
	TournamentId INT IDENTITY (1, 1) PRIMARY KEY,
	TournamentDate DATETIME,
	TournamentLocation NVARCHAR(150),
	EntryFee DECIMAL(10, 2),
	Prize DECIMAL(10, 2),
	Rules NVARCHAR(MAX) NULL,
	TournamentFormat NVARCHAR(50),
	CreatedByUserId INT,
	MaximumNumberOfTeams INT,
	FOREIGN KEY (CreatedByUserId) REFERENCES Person(UserId),
	Status NVARCHAR(20) CHECK (Status IN ('Upcoming', 'Ongoing', 'Completed', 'Cancelled')),
	RegistrationDeadline DATETIME,
	TournamentImageUrl NVARCHAR(255) NULL
);

CREATE TABLE Team(
	TeamId INT IDENTITY (1, 1) PRIMARY KEY,
	TeamName NVARCHAR(100),
	CreatedByUserId INT,
	CreatedAt DATETIME DEFAULT GETDATE(),
	FOREIGN KEY (CreatedByUserId) REFERENCES Person(UserId),
	LogoUrl NVARCHAR(255) NULL
);

CREATE TABLE TournamentTeam(
	TournamentId INT,
	TeamId INT,
	RegistrationDate DATETIME DEFAULT GETDATE(),
	FOREIGN KEY (TournamentId) REFERENCES Tournament(TournamentId),
	FOREIGN KEY (TeamId) REFERENCES Team(TeamId),
	PRIMARY KEY(TournamentId, TeamId)
);

CREATE TABLE TeamPerson(
	UserId INT,
	TeamId INT,
	JoinedAt DATETIME DEFAULT GETDATE(),
	FOREIGN KEY (UserId) REFERENCES Person(UserId),
	FOREIGN KEY (TeamId) REFERENCES Team(TeamId),
	PRIMARY KEY(UserId, TeamId)
);

CREATE TABLE Game(
    MatchId INT IDENTITY(1,1) PRIMARY KEY,
    TournamentId INT FOREIGN KEY REFERENCES Tournament(TournamentId),
    RoundNumber INT,
    MatchTime DATETIME,
    Team1Id INT FOREIGN KEY REFERENCES Team(TeamId),
    Team2Id INT FOREIGN KEY REFERENCES Team(TeamId),
	CHECK (Team1Id <> Team2Id),
    ScoreTeam1 INT NULL CHECK (ScoreTeam1 >= 0),
	ScoreTeam2 INT NULL CHECK (ScoreTeam2 >= 0),
    Status NVARCHAR(20) CHECK (Status IN ('Scheduled', 'InProgress', 'Completed', 'Cancelled'))

);

