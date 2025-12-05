public interface ITokenService
{
    string GenerateToken(string userId);
    string GenerateRefreshToken();
}
