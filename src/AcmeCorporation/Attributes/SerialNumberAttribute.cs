using System.ComponentModel.DataAnnotations;
using AcmeCorporation.Library;

namespace AcmeCorporation.Attributes;

public class SerialNumberAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        string? serialString = value as string;
        if (string.IsNullOrEmpty(serialString))
        {
            return ValidationResult.Success;
        }

        ISerialNumberValidator? validator = (ISerialNumberValidator?)validationContext
            .GetService(typeof(ISerialNumberValidator));

        if (validator == null)
        {
            throw new InvalidOperationException("ISerialNumberValidator is not registered in services.");
        }

        (bool isValid, List<string> errors) = validator.ValidateSerialNumber(serialString);

        return isValid ? 
            ValidationResult.Success :
            new ValidationResult(string.Join("; ", errors));
    }
}
