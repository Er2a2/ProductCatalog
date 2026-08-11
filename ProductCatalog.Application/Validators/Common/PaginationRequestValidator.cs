using FluentValidation;
using ProductCatalog.Application.DTOs.Common;

namespace ProductCatalog.Application.Validators.Common;

public class PaginationRequestValidator : AbstractValidator<PaginationRequestDto>
{
    public PaginationRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page size must be greater than or equal to 1.")
            .LessThanOrEqualTo(100)
            .WithMessage("Page size cannot exceed 100.");
    }
}
