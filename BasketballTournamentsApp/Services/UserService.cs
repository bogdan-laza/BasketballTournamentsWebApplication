using BasketballTournamentsApp.Extensions;
using BasketballTournamentsApp.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using System.Data;
using BasketballTournamentsApp.Jwt;

namespace BasketballTournamentsApp.Services
{
    public class UserService
    {
        private readonly SqlConfig _config;

        public UserService(SqlConfig config)
        {
            _config = config;
        }

        public async Task<PagedResult<User>> GetPagedUsersAsync(
            int page, int pageSize, string sortBy, string sortOrder, string? username, string? email,
            string? personRole)
        {
            if (page < 1)
                page = 1;
            const int maxPageSize = 100;
            if (pageSize < 1)
                pageSize = 10;
            if (pageSize > maxPageSize)
                pageSize = maxPageSize;

            var allowedSortBy = new[] { "CreatedAt", "UserId" };
            if (!allowedSortBy.Contains(sortBy, StringComparer.OrdinalIgnoreCase))
                sortBy = "CreatedAt";

            if (sortOrder != "desc" && sortOrder != "asc")
                sortOrder = "desc";

            var items = new List<User>();
            int totalCount = 0;

            using (var conn = new SqlConnection(_config.ConnectionString))
            using (var cmd = new SqlCommand("GET_PERSONS_PAGED_SORT_FILTER", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Page", page);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);
                cmd.Parameters.AddWithValue("@SortBy", sortBy);
                cmd.Parameters.AddWithValue("@SortOrder", sortOrder);
                cmd.Parameters.AddWithValue("@Username", (object?)username ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", (object?)email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PersonRole", (object?)personRole ?? DBNull.Value);

                var outParam = new SqlParameter("@TotalCount", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(outParam);

                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        items.Add(new User
                        {
                            UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                            Username = reader.GetString(reader.GetOrdinal("Username")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                            Email = reader.GetString(reader.GetOrdinal("Email")),
                            PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? null : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                            PersonRole = reader.GetString(reader.GetOrdinal("PersonRole")),
                            IsEmailVerified = reader.GetBoolean(reader.GetOrdinal("IsEmailVerified"))
                        });
                    }
                }
                totalCount = (int)(outParam.Value ?? 0);
            }

            return new PagedResult<User>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                Items = items
            };
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            var users = new List<User>();

            using (var conn = new SqlConnection(_config.ConnectionString))
            using (var cmd = new SqlCommand("GET_PERSONS", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        users.Add(new User
                        {
                            UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                            Username = reader.GetString(reader.GetOrdinal("Username")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                            Email = reader.GetString(reader.GetOrdinal("Email")),
                            PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? null : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                            PersonRole = reader.GetString(reader.GetOrdinal("PersonRole")),
                            IsEmailVerified = reader.GetBoolean(reader.GetOrdinal("IsEmailVerified"))
                        }
                            );
                    }
                }
            }
            return users;

        }

        public async Task AddUserAsync(User user)
        {
            if (await CheckIfUserExistsByUsernameAsync(user.Username, -1))
                throw new ArgumentException("This username is already taken.");

            if (await CheckIfUserExistsByEmailAsync(user.Email, -1))
                throw new ArgumentException("This email is already registered.");

            if (!string.IsNullOrEmpty(user.PhoneNumber) && user.PhoneNumber.Length != 10)
                throw new ArgumentException("Invalid phone number.");


            if (user.PasswordHash.Length<8)
                throw new ArgumentException("Password must be at least 8 character long.");

            int digitCount = user.PasswordHash.Count(char.IsDigit);
            bool hasSpecialChar = user.PasswordHash.Any(ch => !char.IsLetterOrDigit(ch));

            if (digitCount < 2)
                throw new ArgumentException("Password must contain at least two digits.");

            if (!hasSpecialChar)
                throw new ArgumentException("Password must contain at least one special character.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

            using (var conn = new SqlConnection(_config.ConnectionString))
            using (var cmd = new SqlCommand("ADD_PERSON", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                await conn.OpenAsync();
                cmd.Parameters.AddWithValue("@UserName", user.Username);
                cmd.Parameters.AddWithValue("@FirstName", user.FirstName);
                cmd.Parameters.AddWithValue("@LastName", user.LastName);
                cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                cmd.Parameters.AddWithValue("@Email", user.Email);
                cmd.Parameters.AddWithValue("@PhoneNumber", (object?)user.PhoneNumber ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@RefreshToken", (object?)user.RefreshToken ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);
                cmd.Parameters.AddWithValue("@PersonRole", user.PersonRole);
                cmd.Parameters.AddWithValue("@IsEmailVerified", user.IsEmailVerified);
             
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteUserAsync(int userId)
        {
            using (var conn = new SqlConnection(_config.ConnectionString))
            using (var cmd = new SqlCommand("DELETE_PERSON", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                await conn.OpenAsync();
                cmd.Parameters.AddWithValue("@UserId", userId);
                var rowsAffected = await cmd.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                {
                    throw new ArgumentException($"User with ID {userId} does not exist.");
                }
            }
        }

        public async Task UpdateUserAsync(User user)
        {
            if (await CheckIfUserExistsByUsernameAsync(user.Username, user.UserId))
                throw new ArgumentException("This username is already taken.");

            if (await CheckIfUserExistsByEmailAsync(user.Email, user.UserId))
                throw new ArgumentException("This email is already registered.");

            if (!string.IsNullOrEmpty(user.PhoneNumber) && user.PhoneNumber.Length != 10)
                throw new ArgumentException("Invalid phone number.");

            using (var conn = new SqlConnection(_config.ConnectionString))
            using (var cmd = new SqlCommand("UPDATE_PERSON", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserId", user.UserId);
                cmd.Parameters.AddWithValue("@Username", user.Username);
                cmd.Parameters.AddWithValue("@FirstName", user.FirstName);
                cmd.Parameters.AddWithValue("@LastName", user.LastName);
                cmd.Parameters.AddWithValue("@Email", user.Email);
                cmd.Parameters.AddWithValue("@PhoneNumber", (object?)user.PhoneNumber ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PersonRole", user.PersonRole);
                cmd.Parameters.AddWithValue("@IsEmailVerified", user.IsEmailVerified);

                await conn.OpenAsync();
                var rowsAffected = await cmd.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                {
                    throw new ArgumentException($"User with ID {user.UserId} does not exist.");
                }
            }

        }

        //WILL HAVE TO MAKE AN ENDPOINT FOR IT
        public async Task UpdatePasswordAsync(int userId, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
                throw new ArgumentException("Password must be at least 8 characters long.");

            int digitCount = newPassword.Count(char.IsDigit);
            bool hasSpecialChar = newPassword.Any(ch => !char.IsLetterOrDigit(ch));

            if (digitCount < 2)
                throw new ArgumentException("Password must contain at least two digits.");
            if (!hasSpecialChar)
                throw new ArgumentException("Password must contain at least one special character.");

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);

            using (var conn = new SqlConnection(_config.ConnectionString))
            using (var cmd = new SqlCommand("UPDATE_PERSON_PASSWORD", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);

                await conn.OpenAsync();
                var rowsAffected = await cmd.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                    throw new ArgumentException($"User with ID {userId} does not exist.");
            }
        }

        public async Task<bool> CheckIfUserExistsByIdAsync(int id)
        {
            using(var conn=new SqlConnection(_config.ConnectionString))
            using (var cmd = new SqlCommand("CHECK_IF_PERSON_EXISTS_BY_ID", conn))
            {
                cmd.CommandType=System.Data.CommandType.StoredProcedure;
                await conn.OpenAsync();
                cmd.Parameters.AddWithValue("@Id", id);
                int count=(int)cmd.ExecuteScalar();
                return count > 0;
            }
        }

        public async Task<bool> CheckIfUserExistsByUsernameAsync(string username, int id)
        {
            using (var conn = new SqlConnection(_config.ConnectionString))
            using (var cmd = new SqlCommand("CHECK_IF_PERSON_EXISTS_BY_USERNAME", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                await conn.OpenAsync();
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("UserId", id);
                int count = (int)(await cmd.ExecuteScalarAsync());
                return count > 0;
            }
        }

        public async Task<bool> CheckIfUserExistsByEmailAsync(string email, int id)
        {
            using (var conn = new SqlConnection(_config.ConnectionString))
            using (var cmd = new SqlCommand("CHECK_IF_PERSON_EXISTS_BY_EMAIL", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                await conn.OpenAsync();
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("UserId", id);
                int count = (int)(await cmd.ExecuteScalarAsync());
                return count > 0;
            }
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            using var conn = new SqlConnection(_config.ConnectionString);
            using var cmd = new SqlCommand("GET_PERSON_BY_USERNAME", conn) 
            { 
                CommandType = CommandType.StoredProcedure 
            };
            cmd.Parameters.AddWithValue("@Username", username);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return null;

            return new User
            {
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                Username = reader.GetString(reader.GetOrdinal("Username")),
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? null : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                IsEmailVerified = reader.GetBoolean(reader.GetOrdinal("IsEmailVerified")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                PersonRole = reader.GetString(reader.GetOrdinal("PersonRole")),
                RefreshToken = reader.IsDBNull(reader.GetOrdinal("RefreshToken")) ? null : reader.GetString(reader.GetOrdinal("RefreshToken")),
                RefreshTokenExpiry = reader.IsDBNull(reader.GetOrdinal("RefreshTokenExpiry")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("RefreshTokenExpiry"))
            };
        }

        public async Task SetRefreshTokenAsync(int userId, string? hashedRefreshToken, DateTime? expiry)
        {
            using var conn = new SqlConnection(_config.ConnectionString);
            using var cmd = new SqlCommand("SET_REFRESH_TOKEN", conn) { CommandType = CommandType.StoredProcedure };
            Console.WriteLine(userId);
            Console.WriteLine(hashedRefreshToken);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@RefreshToken", (object?)hashedRefreshToken ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@RefreshTokenExpiry", (object?)expiry ?? DBNull.Value);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<User?> GetByRefreshTokenAsync(string refreshTokenPlain, TokenService tokenService)
        {
            using var conn = new SqlConnection(_config.ConnectionString);
            using var cmd = new SqlCommand("GET_PERSONS_WITH_REFRESH_TOKEN", conn) { CommandType = CommandType.StoredProcedure };
            // This sp returns users where RefreshToken IS NOT NULL (or you can return all users and compare)
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var hashed = reader.IsDBNull(reader.GetOrdinal("RefreshToken")) ? null : reader.GetString(reader.GetOrdinal("RefreshToken"));
                if (hashed != null && tokenService.VerifyRefreshToken(refreshTokenPlain, hashed))
                {
                    return new User
                    {
                        UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                        Username = reader.GetString(reader.GetOrdinal("Username")),
                        FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                        LastName = reader.GetString(reader.GetOrdinal("LastName")),
                        Email = reader.GetString(reader.GetOrdinal("Email")),
                        PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? null : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                        IsEmailVerified = reader.GetBoolean(reader.GetOrdinal("IsEmailVerified")),
                        PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                        PersonRole = reader.GetString(reader.GetOrdinal("PersonRole")),
                        RefreshToken = reader.IsDBNull(reader.GetOrdinal("RefreshToken")) ? null : reader.GetString(reader.GetOrdinal("RefreshToken")),
                        RefreshTokenExpiry = reader.IsDBNull(reader.GetOrdinal("RefreshTokenExpiry")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("RefreshTokenExpiry"))
                    };
                }
            }
            return null;
        }

    }
}
