namespace API.Auth.Models;

public sealed record AccessToken(string Token, DateTimeOffset ExpiresAt);