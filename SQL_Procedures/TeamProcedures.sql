USE BasketballTournamentsApp
GO

DROP PROCEDURE IF EXISTS GET_TEAMS
DROP PROCEDURE IF EXISTS ADD_TEAM
DROP PROCEDURE IF EXISTS DELETE_TEAM
DROP PROCEDURE IF EXISTS UPDATE_TEAM
DROP PROCEDURE IF EXISTS CHECK_IF_TEAM_EXISTS_BY_ID
GO

CREATE PROCEDURE GET_TEAMS
AS
BEGIN
	SELECT  
	TeamId,
	TeamName, 
	CreatedByUserId,
	CreatedAt,
	LogoUrl
    FROM Team
END
GO
EXEC GET_TEAMS;
GO

CREATE PROCEDURE ADD_TEAM
	@TeamName NVARCHAR(100),
	@CreatedByUserId INT,
	@CreatedAt DATETIME=NULL,
	@LogoUrl NVARCHAR(255)
AS
BEGIN

IF @CreatedAt IS NULL
        SET @CreatedAt = GETDATE();

INSERT INTO Team (
    TeamName,
	CreatedByUserId,
	CreatedAt,
	LogoUrl
)
VALUES (
    @TeamName,
	@CreatedByUserId,
	@CreatedAt,
	@LogoUrl
);

END
GO

CREATE PROCEDURE DELETE_TEAM(@TeamId INT)
AS
BEGIN
    DELETE FROM Team WHERE TeamId=@TeamId
END

GO

CREATE PROCEDURE UPDATE_TEAM
    @TeamName NVARCHAR(100),
	@CreatedByUserId INT,
	@CreatedAt DATETIME=NULL,
	@LogoUrl NVARCHAR(255),
    @TeamId INT
AS
BEGIN
    UPDATE Team
    SET 
	TeamName=@TeamName,
	CreatedByUserId=@CreatedByUserId,
	CreatedAt=@CreatedAt,
	LogoUrl=@LogoUrl
    WHERE TeamId=@TeamId
END
GO

CREATE PROCEDURE CHECK_IF_TEAM_EXISTS_BY_ID
	@TeamId INT
AS
BEGIN

SELECT COUNT(*) FROM Team WHERE TeamId=@TeamId

END