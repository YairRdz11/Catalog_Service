using CatalogService.Transversal.Classes.Dtos;
using FluentValidation;

namespace CatalogService.BLL.Classes.Validators
{
    public sealed class CategoryValidator : AbstractValidator<CategoryDTO>
    {
        public void CategoryValidate()
        {
            RuleFor(category => category.Id)
                .NotNull().WithMessage("CategoryId cannot be null.");
            RuleFor(category => category.Name)
                .NotEmpty().WithMessage("Category name must not be empty.")
                .MaximumLength(50).WithMessage("Category name must not exceed 50 characters.");
            RuleFor(category => category.Description)
                .MaximumLength(500).WithMessage("Category description must not exceed 500 characters.")
                .When(c => !string.IsNullOrWhiteSpace(c.Description)); ;
            RuleFor(c => c.URL)
                .Cascade(CascadeMode.Stop)
                .MaximumLength(200).WithMessage("Category URL must not exceed 200 characters.")
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var u) &&
                             (u.Scheme == Uri.UriSchemeHttp || u.Scheme == Uri.UriSchemeHttps))
                    .WithMessage("Category URL must be a valid absolute http/https URL.")
                .When(c => !string.IsNullOrWhiteSpace(c.URL));
        }
    }
}
