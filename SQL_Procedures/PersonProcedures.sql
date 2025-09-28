USE BasketballTournamentsApp
GO

DROP PROCEDURE IF EXISTS GET_PERSONS
DROP PROCEDURE IF EXISTS ADD_PERSON
DROP PROCEDURE IF EXISTS DELETE_PERSON
DROP PROCEDURE IF EXISTS UPDATE_PERSON
DROP PROCEDURE IF EXISTS CHECK_IF_PERSON_EXISTS_BY_ID
DROP PROCEDURE IF EXISTS CHECK_IF_PERSON_EXISTS_BY_USERNAME
DROP PROCEDURE IF EXISTS CHECK_IF_PERSON_EXISTS_BY_EMAIL
DROP PROCEDURE IF EXISTS UPDATE_PERSON_PASSWORD
DROP PROCEDURE IF EXISTS SET_REFRESH_TOKEN
DROP PROCEDURE IF EXISTS GET_PERSON_BY_USERNAME
DROP PROCEDURE IF EXISTS GET_PERSONS_WITH_REFRESH_TOKEN
GO

CREATE PROCEDURE GET_PERSONS
AS
BEGIN
	SELECT  
	UserId,
	Username,
	FirstName,
	LastName,
	PasswordHash,
	Email,
	PhoneNumber,
	RefreshToken,
	CreatedAt,
	PersonRole,	
	IsEmailVerified
    FROM Person
END
GO
EXEC GET_PERSONS;
GO

CREATE PROCEDURE ADD_PERSON 
	@Username NVARCHAR(50),
	@FirstName NVARCHAR(50),
	@LastName NVARCHAR(50),
	@PasswordHash NVARCHAR(255),
	@Email NVARCHAR(150),
	@PhoneNumber NVARCHAR(10)=NULL,
	@RefreshToken NVARCHAR(500)=NULL,
	@CreatedAt DATETIME=NULL,
	@PersonRole NVARCHAR(50)='Player',	
	@IsEmailVerified BIT=0
AS
BEGIN

IF @CreatedAt IS NULL
        SET @CreatedAt = GETDATE();

INSERT INTO Person (
    Username,
	FirstName,
	LastName,
	PasswordHash,
	Email,
	PhoneNumber,
	RefreshToken,
	CreatedAt,
	PersonRole,	
	IsEmailVerified
)
VALUES (
    @Username,
	@FirstName,
	@LastName,
	@PasswordHash,
	@Email,
	@PhoneNumber,
	@RefreshToken,
	@CreatedAt,
	@PersonRole,	
	@IsEmailVerified
);

END
GO

CREATE PROCEDURE DELETE_PERSON(@UserId INT)
AS
BEGIN
    DELETE FROM Person WHERE UserId=@UserId
END

GO

CREATE PROCEDURE UPDATE_PERSON
    @Username NVARCHAR(50),
	@FirstName NVARCHAR(50),
	@LastName NVARCHAR(50),
	@Email NVARCHAR(150),
	@PhoneNumber NVARCHAR(10)=NULL,
	@PersonRole NVARCHAR(50)='Player',	
	@IsEmailVerified BIT=0,
    @UserId INT
AS
BEGIN

	 

    UPDATE Person
    SET 
	Username=@Username,
	FirstName=@FirstName,
	LastName=@LastName,
	Email=@Email,
	PhoneNumber=@PhoneNumber,
	PersonRole=@PersonRole,	
	IsEmailVerified=@IsEmailVerified
    WHERE UserId=@UserId
END
GO

CREATE PROCEDURE UPDATE_PERSON_PASSWORD
	@UserId INT,
	@PasswordHash NVARCHAR(255)
AS
BEGIN

	UPDATE Person
	SET
	PasswordHash=@PasswordHash
	WHERE UserId=@UserId

END
GO

CREATE PROCEDURE CHECK_IF_PERSON_EXISTS_BY_ID
	@Id INT
AS
BEGIN

SELECT COUNT(*) FROM Person WHERE UserId=@Id

END
GO

CREATE PROCEDURE CHECK_IF_PERSON_EXISTS_BY_USERNAME
	@Username NVARCHAR(50),
	@UserId INT
AS
BEGIN

SELECT COUNT(*) FROM Person WHERE Username=@Username AND UserId<>@UserId

END
GO

CREATE PROCEDURE CHECK_IF_PERSON_EXISTS_BY_EMAIL
	@Email NVARCHAR(150),
	@UserId INT
AS
BEGIN

SELECT COUNT(*) FROM Person WHERE Email=@Email AND UserId<>@UserId

END
GO

CREATE PROCEDURE SET_REFRESH_TOKEN
    @UserId INT,
    @RefreshToken NVARCHAR(500) = NULL,
    @RefreshTokenExpiry DATETIME = NULL
AS
BEGIN
    UPDATE Person
    SET RefreshToken = @RefreshToken,
        RefreshTokenExpiry = @RefreshTokenExpiry
    WHERE UserId = @UserId;
END
GO

CREATE PROCEDURE GET_PERSON_BY_USERNAME
    @Username NVARCHAR(50)
AS
BEGIN
    SELECT UserId, Username, FirstName, LastName, PasswordHash, Email, PhoneNumber, RefreshToken, CreatedAt, PersonRole, IsEmailVerified, RefreshTokenExpiry
    FROM Person
    WHERE Username = @Username;
END
GO

CREATE PROCEDURE GET_PERSONS_WITH_REFRESH_TOKEN
AS
BEGIN
    SELECT UserId, Username, FirstName, LastName, PasswordHash, Email, PhoneNumber, RefreshToken, CreatedAt, PersonRole, IsEmailVerified, RefreshTokenExpiry
    FROM Person
    WHERE RefreshToken IS NOT NULL;
END
GO

SELECT RefreshToken FROM Person WHERE Username='string2';


