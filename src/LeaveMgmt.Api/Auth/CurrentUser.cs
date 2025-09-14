// File: LeaveMgmt.Api/Auth/CurrentUser.cs
using System.Security.Claims;
using LeaveMgmt.Application.Abstractions.Identity;

namespace LeaveMgmt.Api.Auth;

public sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public Guid UserId
    {
        get
        {
            var id = Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? Principal?.FindFirst("sub")?.Value;
            return Guid.TryParse(id, out var g) ? g : Guid.Empty;
        }
    }

    public bool IsInRole(string role) => Principal?.IsInRole(role) ?? false;
}
