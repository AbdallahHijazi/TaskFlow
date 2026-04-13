using FluentValidation;
using TaskFlow.Application.Features.Users.Commands;

namespace TaskFlow.Application.Features.Users.Validators;

public class GetUserTasksQueryValidator : AbstractValidator<GetUserTasksQuery>
{
    private static readonly HashSet<string> SupportedSortBy = new(StringComparer.OrdinalIgnoreCase)
    {
        "createdAt",
        "dueDate",
        "priority"
    };

    public GetUserTasksQueryValidator()
    {
        RuleFor(x => x.Parameters.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("pageNumber must be greater than or equal to 1.");

        RuleFor(x => x.Parameters.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("pageSize must be between 1 and 100.");

        RuleFor(x => x.Parameters.SortBy)
            .Must(sortBy => !string.IsNullOrWhiteSpace(sortBy) && SupportedSortBy.Contains(sortBy.Trim()))
            .WithMessage("sortBy must be one of: createdAt, dueDate, priority.");

        RuleFor(x => x.Parameters.SortDirection)
            .Must(direction => string.Equals(direction, "asc", StringComparison.OrdinalIgnoreCase) ||
                               string.Equals(direction, "desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("sortDirection must be either asc or desc.");

        RuleFor(x => x.Parameters.Priority)
            .InclusiveBetween(1, 5)
            .When(x => x.Parameters.Priority.HasValue)
            .WithMessage("priority must be between 1 and 5.");

        RuleFor(x => x.Parameters)
            .Must(p => !p.FromDate.HasValue || !p.ToDate.HasValue || p.FromDate.Value.Date <= p.ToDate.Value.Date)
            .WithMessage("fromDate must be less than or equal to toDate.");
    }
}
