namespace Identity.Application.Dtos;

public sealed record AuthResultDto(Guid UserId, string Email, string Role, string AccessToken, string RefreshToken);
