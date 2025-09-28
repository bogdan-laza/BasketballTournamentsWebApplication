USE BasketballTournamentsApp
GO

DROP PROCEDURE IF EXISTS GET_TEAM_REGISTRATIONS
DROP PROCEDURE IF EXISTS ADD_TEAM_REGISTRATION
DROP PROCEDURE IF EXISTS DELETE_TEAM_REGISTRATION
DROP PROCEDURE IF EXISTS UPDATE_TEAM_REGISTRATION
GO

CREATE PROCEDURE GET_TEAM_REGISTRATIONS
AS
BEGIN
	SELECT  
	TournamentId,
	TeamId, 
	RegistrationDate
    FROM TeamRegistration
END
GO
EXEC GET_TEAMS;
GO

CREATE PROCEDURE ADD_TEAM_REGISTRATION
	@TournamentId INT,
	@TeamId INT,
	@RegistrationDate DATETIME=NULL
AS
BEGIN

IF @RegistrationDate IS NULL
        SET @RegistrationDate = GETDATE();

INSERT INTO TeamRegistration(
    TournamentId,
	TeamId,
	RegistrationDate
)
VALUES (
    @TournamentId,
	@TeamId,
	@RegistrationDate
);

END
GO

CREATE PROCEDURE DELETE_TEAM_REGISTRATION(@TournamentId INT, @TeamId INT)
AS
BEGIN
    DELETE FROM TeamRegistration WHERE TournamentId=@TournamentId AND TeamId=@TeamId
END

GO

CREATE PROCEDURE UPDATE_TEAM_REGISTRATION
	@RegistrationDate DATETIME,
	@TournamentId INT,
	@TeamId INT
AS
BEGIN
    UPDATE TeamRegistration
    SET 
	RegistrationDate=@RegistrationDate
    WHERE TournamentId=@TournamentId AND TeamId=@TeamId
END
GO