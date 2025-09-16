namespace LeaveMgmt.Application.Abstractions.Identity;

public interface ITeamRoster
{
    // true if the email exists in any roster; roles maps to your application's Roles constants
    bool TryGetRolesFor(string email, out IReadOnlyCollection<string> roles);
}
