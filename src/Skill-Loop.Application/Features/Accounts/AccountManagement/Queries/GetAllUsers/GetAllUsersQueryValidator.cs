using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Skill_Loop.Application.Features.Accounts.AccountManagement.Queries.GetAllUsers;



public sealed class GetAllUsersQueryValidator : AbstractValidator<GetAllUsersQuery>
{
    public GetAllUsersQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("رقم الصفحة يجب أن يكون أكبر من الصفر.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("حجم الصفحة يجب أن يكون أكبر من الصفر.")
            .LessThanOrEqualTo(100).WithMessage("الحد الأقصى لحجم الصفحة هو 100.");
    }
}

