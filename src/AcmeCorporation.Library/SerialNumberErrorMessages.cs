namespace AcmeCorporation.Library;

/// <summary>
/// Provides standardized error message strings related to serial number validation. Currently only en-US is supported.
/// This class will be the basis for future localization efforts.
/// </summary>
/// <remarks>This class contains static properties that supply error messages for various validation failures
/// encountered when processing serial numbers, model numbers, and unique identifiers. These messages can be used to
/// provide consistent feedback to users or for logging purposes in applications that validate serial number
/// formats.</remarks>
public class SerialNumberErrorMessages
{
    public static string SerialNumberMustHaveContent => "Serial number must not be empty";
    public static string SerialNumberMustHaveLength =>$"Serial number must be {SerialNumberValues.TotalLength} characters long";
    public static string ModelNumberAllLetters => "Model number must be all letters";
    public static string SerialIdentifierOnlyDigits => "Serial identifier must be all digits";
    public static string SerialIdentifierSpecialMiddleChar => $"Serial identifier middle character must be a '{SerialNumberValues.KnownSerialIdentifierMidChar}'";
    public static string UniqueIdentifierWrongComposition => "Unique identifier must consist of only letters and digits";
    public static string UniqueIdentifierLetterPartWrong => $"The first {SerialNumberValues.UniqueIdentifierLetterPartLength} characters of the unique part must be letters";
    public static string UniqueIdentifierDigitPartWrong => $"The last {SerialNumberValues.UniqueIdentifierDigitPartLength} characters of the unique part must be digits";
}