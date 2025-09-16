
using LeaveMgmt.Website.Models;

namespace LeaveMgmt.Website.Services;

public sealed class LeaveTypeService
{
    // Swagger didn’t show a leave-types endpoint; provide a simple fallback list
    private static readonly List<LeaveType> _fallback = new()
    {
         new LeaveType { Id = Guid.Parse("C34DE22D-DBEA-40AC-84C4-F1B5A4F335C3"), Name = "Annual" },
        new LeaveType { Id = Guid.Parse("822DFD3C-FF3A-458A-85EA-1BA32F5DB32D"), Name = "Sick" },
        new LeaveType { Id = Guid.Parse("4731EE57-627D-4639-B1DE-10E47DF45DC6"), Name = "Unpaid" }
    };

    public Task<List<LeaveType>> GetAsync() => Task.FromResult(_fallback);
}
