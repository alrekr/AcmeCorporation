namespace AcmeCorporation.Library;

/// <summary>
/// Provides constant values that define the structure and component lengths of serial numbers used in the system.
/// </summary>
/// <remarks>This class contains constants for segment lengths and specific characters used when parsing or
/// constructing serial numbers. These values are intended to standardize serial number formats across the application
/// and should be used when validating or generating serial numbers to ensure consistency.</remarks>
public class SerialNumberValues
{
    internal const int TotalLength = 18;
    internal const int ModelNumberLength = 4;
    internal const int SerialIdentifierStartIndex = ModelNumberLength;
    internal const int SerialIdentifierFirstPartLength = 4;
    internal const int SerialIdentifierLastPartLength = 4;
    internal const int SerialIdentifierLength = SerialIdentifierFirstPartLength + 1 + SerialIdentifierLastPartLength;
    internal const int SerialIdentifierMid = 4;
    internal const int UniqueIdentifierStartIndex = ModelNumberLength + SerialIdentifierLength;
    internal const int UniqueIdentifierLength = 5;
    internal const int UniqueIdentifierLetterPartStart = 0;
    internal const int UniqueIdentifierLetterPartLength = 3;
    internal const int UniqueIdentifierDigitPartStart = UniqueIdentifierLetterPartLength;
    internal const int UniqueIdentifierDigitPartLength = 2;
    internal const char KnownSerialIdentifierMidChar = '3';
}