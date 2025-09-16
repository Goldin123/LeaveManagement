namespace LeaveMgmt.Application.DTOs;

public sealed record UserDto(Guid Id, string FullName, string Email)
{
    public static UserDto FromDomain(Domain.Users.User u)
        => new(u.Id, u.FullName, u.Email);
}
