namespace Smith.MatrixSdk.ApiTypes
{
    public record LoginResponse(string AccessToken, string HomeServer, string UserId, string? RefreshToken = null);
    public record LoginRequest(string Password, string Type, string User);
}
