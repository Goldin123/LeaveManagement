// File: LeaveMgmt.Application/Validation/ApproveRejectValidators.cs
using FluentValidation;
using LeaveMgmt.Application.Commands.LeaveRequests.ApproveLeave;
using LeaveMgmt.Application.Commands.LeaveRequests.RejectLeave;

namespace LeaveMgmt.Application.Validation;

public sealed class ApproveLeaveValidator : AbstractValidator<ApproveLeaveCommand>
{
    public ApproveLeaveValidator()
    {
        RuleFor(x => x.LeaveRequestId).NotEmpty();
        RuleFor(x => x.ManagerId).NotEmpty();
    }
}

public sealed class RejectLeaveValidator : AbstractValidator<RejectLeaveCommand>
{
    public RejectLeaveValidator()
    {
        RuleFor(x => x.LeaveRequestId).NotEmpty();
        RuleFor(x => x.ManagerId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
