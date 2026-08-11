using FluentValidation;
using ProductCatalog.Application.DTOs.Products;

namespace ProductCatalog.Application.Validators.Products;

public class ProductQueryValidator : AbstractValidator<ProductQueryDto>
{
    public ProductQueryValidator()
    {
        RuleFor(x => x.SortBy)
            .Must(BeValidSortBy)
            .When(x => !string.IsNullOrWhiteSpace(x.SortBy))
            .WithMessage("Sort by must be one of: name, price, stock.");

        RuleFor(x => x.SortOrder)
            .Must(BeValidSortOrder)
            .When(x => !string.IsNullOrWhiteSpace(x.SortOrder))
            .WithMessage("Sort order must be either asc or desc.");
    }

    private static bool BeValidSortBy(string? sortBy)
    {
        return sortBy?.ToLower() is "name" or "price" or "stock";
    }

    private static bool BeValidSortOrder(string? sortOrder)
    {
        return sortOrder?.ToLower() is "asc" or "desc";
    }
}