using LeaveMgmt.Web.Models;

namespace LeaveMgmt.Web.Services;

public sealed class UserService
{
    public Task<List<SupportMemberDto>> GetSupportMembersAsync()
        => Task.FromResult(new List<SupportMemberDto>());
}

public sealed class SupportMemberDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
