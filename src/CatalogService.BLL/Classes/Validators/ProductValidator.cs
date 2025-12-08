using CatalogService.Transversal.Classes.Dtos;
using FluentValidation;

namespace CatalogService.BLL.Classes.Validators
{
    public class ProductValidator : AbstractValidator<ProductDTO>
    {
        public void ProductValidate()
        {
            RuleFor(product => product.Id)
                .NotNull().WithMessage("ProductId cannot be null.");
            RuleFor(product => product.Name)
                .NotEmpty().WithMessage("Product name must not be empty.")
                .MaximumLength(100).WithMessage("Product name must not exceed 100 characters.");
            RuleFor(product => product.Description)
                .MaximumLength(1000).WithMessage("Product description must not exceed 1000 characters.")
                .When(p => !string.IsNullOrWhiteSpace(p.Description));
            RuleFor(product => product.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Product price must be greater than or equal to zero.");
            RuleFor(product => product.Amount)
                .GreaterThanOrEqualTo(0).WithMessage("Product amount must be greater than or equal to zero.");
            RuleFor(product => product.URL)
                .Cascade(CascadeMode.Stop)
                .MaximumLength(200).WithMessage("Product URL must not exceed 200 characters.")
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var u) &&
                             (u.Scheme == Uri.UriSchemeHttp || u.Scheme == Uri.UriSchemeHttps))
                    .WithMessage("Product URL must be a valid absolute http/https URL.")
                .When(p => !string.IsNullOrWhiteSpace(p.URL));
            RuleFor(product => product.CategoryId)
                .NotNull().WithMessage("CategoryId cannot be null.");
        }
    }
}
