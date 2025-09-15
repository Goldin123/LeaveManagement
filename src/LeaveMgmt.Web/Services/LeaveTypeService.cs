using LeaveMgmt.Web.Models;

namespace LeaveMgmt.Web.Services;

public sealed class LeaveTypeService
{
    // Swagger didn’t show a leave-types endpoint; provide a simple fallback list
    private static readonly List<LeaveTypeDto> _fallback = new()
    {
        new LeaveTypeDto { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Annual" },
        new LeaveTypeDto { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Sick" },
        new LeaveTypeDto { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Unpaid" }
    };

    public Task<List<LeaveTypeDto>> GetAsync() => Task.FromResult(_fallback);
}
