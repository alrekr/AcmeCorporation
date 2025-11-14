namespace AcmeCorporation.Library;

/// <summary>
/// Defines a contract for validating serial numbers with the rules from the implementation.
/// </summary>
public interface ISerialNumberValidator
{
    (bool valid, List<string> errors) ValidateSerialNumber(string serialNumber);
}