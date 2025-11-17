using AcmeCorporation.Library;

namespace AcmeCorporation.Datacontracts;

public class SerialNumber
{
    private readonly string? _serialNumber;

    public SerialNumber(ISerialNumberValidator serialNumberValidator, string serialNumber)
    {
        (bool serialNumberIsValid, List<string> errors ) = serialNumberValidator.ValidateSerialNumber(serialNumber);
        
        if (!serialNumberIsValid)
        {
            string allErrors = string.Join("; ", errors);
            throw new ArgumentException($"The provided serial number is invalid: {allErrors}", nameof(serialNumber));
        }

        _serialNumber = serialNumber;
    }

    public override string ToString()
    {
        return _serialNumber ?? string.Empty;
    }
}