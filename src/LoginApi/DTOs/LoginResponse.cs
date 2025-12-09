namespace LoginApi.DTOs;

public class LoginResponse
{
    public required string Token { get; set; }
    public DateTimeOffset Expiration { get; set; }
}
