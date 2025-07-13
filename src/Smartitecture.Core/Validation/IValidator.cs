using System.Collections.Generic;
using System.Threading.Tasks;

namespace Smartitecture.Core.Validation
{
    public interface IValidator<T>
    {
        Task<ValidationResult> ValidateAsync(T entity);
        Task<ValidationResult> ValidateAsync(T entity, string operation);
    }

    public class ValidationResult
    {
        public bool IsValid { get; }
        public string? Message { get; }
        public Dictionary<string, string[]> Errors { get; }

        public ValidationResult(bool isValid)
        {
            IsValid = isValid;
            Errors = new Dictionary<string, string[]>();
        }

        public ValidationResult(bool isValid, string message)
        {
            IsValid = isValid;
            Message = message;
            Errors = new Dictionary<string, string[]>();
        }

        public static ValidationResult Success()
        {
            return new ValidationResult(true);
        }

        public static ValidationResult Failure(string message)
        {
            return new ValidationResult(false, message);
        }

        public static ValidationResult Failure(string message, Dictionary<string, string[]> errors)
        {
            var result = new ValidationResult(false, message);
            foreach (var error in errors)
            {
                result.Errors[error.Key] = error.Value;
            }
            return result;
        }
    }
}
