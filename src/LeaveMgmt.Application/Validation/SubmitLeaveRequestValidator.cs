using FluentValidation;
using LeaveMgmt.Application.Commands.LeaveRequests.SubmitLeaveRequest;

namespace LeaveMgmt.Application.Validation;

public sealed class SubmitLeaveRequestValidator : AbstractValidator<SubmitLeaveRequestCommand>
{
    public SubmitLeaveRequestValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.LeaveTypeId).NotEmpty();
        RuleFor(x => x.StartDate).LessThanOrEqualTo(x => x.EndDate);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(512);
    }
}
