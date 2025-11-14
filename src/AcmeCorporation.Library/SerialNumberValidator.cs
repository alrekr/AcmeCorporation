namespace AcmeCorporation.Library;

/// <summary>
/// Provides functionality to validate serial numbers according to predefined rules. 
/// Designed to work with <see cref="SerialNumberGenerator"/>.
/// Implements <see cref="ISerialNumberValidator"/>
/// </summary>
/// <remarks>Use this class to check whether a serial number meets all required criteria and to obtain detailed
/// error messages for any validation failures. If the string is of valid length, all parts of the string will be validated:
/// <list type="bullet">
/// <item>Model number</item>
/// <item>Serial identifier</item>
/// <item>Unique identifier</item>
/// </list>
/// <para>Validation is case insensitive.</para>
/// <para>
/// This class is thread-safe and can be used concurrently across multiple threads.</para>
/// </remarks>
public class SerialNumberValidator : ISerialNumberValidator
{
    public (bool valid, List<string> errors) ValidateSerialNumber(string serialNumber)
    {
        List<string> errors = [.. GetErrors(serialNumber)];
        bool isValid = errors.Count == 0;
        return (isValid, errors);
    }

    private static List<string> GetErrors(string serialNumber)
    {
        (bool stringHasContent, string message) = StringHasContent(serialNumber);

        if (!stringHasContent)
        {
            return [message];
        }

        (bool stringIsValidLength, message) = StringIsValidLength(serialNumber);

        if (!stringIsValidLength)
        {
            return [message];
        }

        List<string> errors = new(5);

        ReadOnlySpan<char> serialSpan = serialNumber.AsSpan();
        ReadOnlySpan<char> modelSpan = serialSpan[..SerialNumberValues.ModelNumberLength];
        ReadOnlySpan<char> serialIdSpan = serialSpan.Slice(
            SerialNumberValues.SerialIdentifierStartIndex,
            SerialNumberValues.SerialIdentifierLength);
        ReadOnlySpan<char> uniqueIdSpan = serialSpan.Slice(
            SerialNumberValues.UniqueIdentifierStartIndex,
            SerialNumberValues.UniqueIdentifierLength);

        errors.AddRange(ModelNumberIsValid(modelSpan));
        errors.AddRange(SerialIdentifierIsValid(serialIdSpan));
        errors.AddRange(UniqueIdentifierIsValid(uniqueIdSpan));

        return errors;
    }

    // returns list to make the calling logic "prettier" by abstracting away null/empty handling
    private static List<string> ModelNumberIsValid(ReadOnlySpan<char> modelNumber)
    {
        List<string> errors = new(1);

        foreach (char c in modelNumber)
        {
            if (char.IsAsciiLetter(c))
            {
                continue;
            }

            errors.Add(SerialNumberErrorMessages.ModelNumberAllLetters);
            break;
        }

        return errors;
    }

    private static List<string> UniqueIdentifierIsValid(ReadOnlySpan<char> uniqueIdentifier)
    {
        List<string> errors = new(2);
        ReadOnlySpan<char> letterPart = uniqueIdentifier[..SerialNumberValues.UniqueIdentifierLetterPartLength];
        ReadOnlySpan<char> digitPart = uniqueIdentifier[SerialNumberValues.UniqueIdentifierLetterPartLength..];

        foreach (char c in letterPart)
        {
            if (char.IsAsciiLetter(c))
            {
                continue;
            }

            errors.Add(SerialNumberErrorMessages.UniqueIdentifierLetterPartWrong);
            break;
        }

        foreach (char c in digitPart)
        {
            if (char.IsDigit(c))
            {
                continue;
            }

            errors.Add(SerialNumberErrorMessages.UniqueIdentifierDigitPartWrong);
            break;
        }

        return errors;
    }

    private static List<string> SerialIdentifierIsValid(ReadOnlySpan<char> serialIdentifier)
    {
        List<string> errors = new(2);

        foreach (char c in serialIdentifier)
        {
            if (c == SerialNumberValues.KnownSerialIdentifierMidChar || char.IsDigit(c))
            {
                continue;
            }

            errors.Add(SerialNumberErrorMessages.SerialIdentifierOnlyDigits);
            break;
        }

        if (serialIdentifier[SerialNumberValues.SerialIdentifierMid] != SerialNumberValues.KnownSerialIdentifierMidChar)
        {
            errors.Add(SerialNumberErrorMessages.SerialIdentifierSpecialMiddleChar);
        }

        return errors;
    }

    private static (bool, string) StringHasContent(string serialNumber)
    {
        if (string.IsNullOrWhiteSpace(serialNumber))
        {
            return (false, SerialNumberErrorMessages.SerialNumberMustHaveContent);
        }

        return (true, string.Empty);
    }

    private static (bool, string) StringIsValidLength(string serialNumber)
    {
        if (serialNumber.Length != SerialNumberValues.TotalLength)
        {
            return (false, SerialNumberErrorMessages.SerialNumberMustHaveLength);
        }

        return (true, string.Empty);
    }
}