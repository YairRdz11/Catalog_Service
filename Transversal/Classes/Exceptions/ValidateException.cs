using Utilities.Classes.Common;

namespace CatalogService.Transversal.Classes.Exceptions
{
    public class ValidateException : DomainException
    {
        public List<RuleError> Errors { get; }

        public ValidateException(List<RuleError> errors)
            : base("One or more validation errors occurred.")
        {
            Errors = errors;
        }
    }
}
