namespace AcmeCorporation.Library;

/// <summary>
/// Defines a contract for generating unique serial numbers as strings.
/// </summary>
public interface ISerialNumberGenerator
{
    string CreateSerialNumber();
}