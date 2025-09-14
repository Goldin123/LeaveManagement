// File: LeaveMgmt.Application/Abstractions/Identity/ICurrentUser.cs
namespace LeaveMgmt.Application.Abstractions.Identity;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    Guid UserId { get; }
    bool IsInRole(string role);
}
