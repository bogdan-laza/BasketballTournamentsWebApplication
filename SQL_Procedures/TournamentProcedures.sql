USE BasketballTournamentsApp
GO

DROP PROCEDURE IF EXISTS GET_TOURNAMENTS
DROP PROCEDURE IF EXISTS ADD_TOURNAMENT
DROP PROCEDURE IF EXISTS DELETE_TOURNAMENT
DROP PROCEDURE IF EXISTS UPDATE_TOURNAMENT
DROP PROCEDURE IF EXISTS CHECK_IF_TOURNAMENT_EXISTS_BY_ID
DROP PROCEDURE IF EXISTS GET_TOURNAMENT_BY_ID
GO

CREATE PROCEDURE GET_TOURNAMENTS
AS
BEGIN
	SELECT  TournamentId,
            TournamentName,
            TournamentDate,
            TournamentLocation,
            EntryFee,
            Prize,
            Rules,
            TournamentFormat,
            CreatedByUserId,
            MaximumNumberOfTeams,
            Status,
            RegistrationDeadline,
            TournamentImageUrl
            FROM Tournament
END
GO

CREATE PROCEDURE ADD_TOURNAMENT (
    @TournamentName NVARCHAR(100),
    @TournamentDate DATETIME,
	@TournamentLocation NVARCHAR(150),
	@EntryFee DECIMAL(10, 2),
	@Prize DECIMAL(10, 2),
	@Rules NVARCHAR(MAX) NULL,
	@TournamentFormat NVARCHAR(50),
	@CreatedByUserId INT,
	@MaximumNumberOfTeams INT,
	@Status NVARCHAR(20),
	@RegistrationDeadline DATETIME,
	@TournamentImageUrl NVARCHAR(255) NULL
)
AS
BEGIN

INSERT INTO Tournament (
    TournamentName,
    TournamentDate,
    TournamentLocation,
    EntryFee,
    Prize,
    Rules,
    TournamentFormat,
    CreatedByUserId,
    MaximumNumberOfTeams,
    Status,
    RegistrationDeadline,
    TournamentImageUrl
)
VALUES (
    @TournamentName,
    @TournamentDate,
	@TournamentLocation,
	@EntryFee,
	@Prize,
	@Rules,
	@TournamentFormat,
	@CreatedByUserId,
	@MaximumNumberOfTeams,
	@Status,
	@RegistrationDeadline,
	@TournamentImageUrl
);

END
GO

CREATE PROCEDURE DELETE_TOURNAMENT(@TournamentId INT)
AS
BEGIN
    DELETE FROM Tournament WHERE TournamentId=@TournamentId
END

GO

CREATE PROCEDURE UPDATE_TOURNAMENT(
    @TournamentName NVARCHAR(100),
    @TournamentDate DATETIME,
	@TournamentLocation NVARCHAR(150),
	@EntryFee DECIMAL(10, 2),
	@Prize DECIMAL(10, 2),
	@Rules NVARCHAR(MAX) NULL,
	@TournamentFormat NVARCHAR(50),
	@CreatedByUserId INT,
	@MaximumNumberOfTeams INT,
	@Status NVARCHAR(20),
	@RegistrationDeadline DATETIME,
	@TournamentImageUrl NVARCHAR(255) NULL,
    @TournamentId INT
)
AS
BEGIN
    UPDATE Tournament 
    SET TournamentName=@TournamentName,
    TournamentDate=@TournamentDate,
    TournamentLocation=@TournamentLocation,
    EntryFee=@EntryFee, 
    Prize=@Prize,
    Rules=@Rules,
    TournamentFormat=@TournamentFormat,
    CreatedByUserId=@CreatedByUserId,
    MaximumNumberOfTeams=@MaximumNumberOfTeams,
    Status=@Status,
    RegistrationDeadline=@RegistrationDeadline, 
    TournamentImageUrl=@TournamentImageUrl
    WHERE TournamentId=@TournamentId
END
GO

CREATE PROCEDURE CHECK_IF_TOURNAMENT_EXISTS_BY_ID
	@TournamentId INT
AS
BEGIN

SELECT COUNT(*) FROM Tournament WHERE TournamentId=@TournamentId

END
GO


CREATE PROCEDURE GET_TOURNAMENT_BY_ID
    @Id INT
AS
BEGIN
    SELECT
            TournamentId,
            TournamentName,
            TournamentDate,
            TournamentLocation,
            EntryFee,
            Prize,
            Rules,
            TournamentFormat,
            CreatedByUserId,
            MaximumNumberOfTeams,
            Status,
            RegistrationDeadline,
            TournamentImageUrl
    FROM Tournament
    WHERE TournamentId = @Id;
END
GO

EXECUTE GET_TOURNAMENTS