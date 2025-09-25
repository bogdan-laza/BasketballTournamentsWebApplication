namespace BasketballTournamentsApp.Jwt
{
    public class JwtSettings
    {
        public string Key { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int AccessTokenExpiresMinutes { get; set; }
        public int RefreshTokenExpiresDays { get; set; } = 7;
    }

}
