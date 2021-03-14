namespace Smith.MatrixSdk.ApiTypes
{
    /// <param name="Type">
    /// Required. The login type being used. One of: <c>["m.login.password", "m.login.token"]</c>.
    /// </param>
    /// <param name="Password">
    /// Required when <paramref name="Type"/> is <c>m.login.password</c>. The user's password.
    /// </param>
    /// <param name="User">The fully qualified user ID or just local part of the user ID, to log in.</param>
    public record LoginRequest(string Type, string? Password, string? User);

    /// <param name="UserId">The fully-qualified Matrix ID that has been registered.</param>
    /// <param name="AccessToken">
    /// An access token for the account. This access token can then be used to authorize other requests.
    /// </param>
    /// <param name="HomeServer">
    /// <para>The server_name of the homeserver on which the account has been registered.</para>
    /// <para>
    ///     Deprecated. Clients should extract the <c>server_name</c> from <c>user_id</c> (by splitting at the first
    ///     colon) if they require it. Note also that <c>homeserver</c> is not spelt this way.
    /// </para>
    /// </param>
    public record LoginResponse(string? UserId, string? AccessToken, string? HomeServer);
}
