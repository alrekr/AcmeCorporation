namespace AcmeCorporation.Library;

/// <summary>
/// <para>Provides functionality to generate serial numbers composed of randomized letters and digits according to a
/// predefined format. Here, the format a string with:
/// <list type="number">
/// <item>Four letters</item>
/// <item>Nine digits, of which the fifth digit is always '0'</item>
/// <item>Three letters</item>
/// <item>Two digits</item>
/// </list>
/// Example serial number: <example>abcd123435678abc12</example>
/// </para>
/// <para>Serial numbers are case insensitive.</para>
/// </summary>
/// <remarks>This class implements <see cref="ISerialNumberGenerator"/> and is intended for generaing unique,
/// non-sequential serial numbers.
/// Thread safety is ensured for serial number generation, making this class suitable for concurrent use.</remarks>
public class SerialNumberGenerator : ISerialNumberGenerator
{
    private static readonly char[] Alphabet = ['A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'];

    public string CreateSerialNumber()
    {
        int totalLength = GetTotalLength();

        // Using Span to reduce memory allocations and thus reduce hardware consumption
        Span<char> chars = stackalloc char[totalLength];

        int currentPosition = 0;

        // In the following sections:
        // First, a part of the span (`chars`) is sliced out.
        // Then, the slice is filled accoring to the serial number.
        // Last, the position in the buffer is advanced and prepared for the next part of the serial number.

        Span<char> modelNumber = chars.Slice(currentPosition, SerialNumberValues.ModelNumberLength);
        FillWithLetters(modelNumber);
        currentPosition += SerialNumberValues.ModelNumberLength;

        Span<char> serialIdentifierFirstPart = chars.Slice(currentPosition, SerialNumberValues.SerialIdentifierFirstPartLength);
        FillWithDigits(serialIdentifierFirstPart);
        currentPosition += SerialNumberValues.SerialIdentifierFirstPartLength;

        // This is a special case, where the character is known and fixed.
        chars[currentPosition] = SerialNumberValues.KnownSerialIdentifierMidChar;
        currentPosition++;

        Span<char> serialIdentifierLastPart = chars.Slice(currentPosition, SerialNumberValues.SerialIdentifierLastPartLength);
        FillWithDigits(serialIdentifierLastPart);
        currentPosition += SerialNumberValues.SerialIdentifierLastPartLength;

        Span<char> uniqueIdentifierLetterPart = chars.Slice(currentPosition, SerialNumberValues.UniqueIdentifierLetterPartLength);
        FillWithLetters(uniqueIdentifierLetterPart);
        currentPosition += SerialNumberValues.UniqueIdentifierLetterPartLength;

        Span<char> uniqueIdentifierDigitPart = chars.Slice(currentPosition, SerialNumberValues.UniqueIdentifierDigitPartLength);
        FillWithDigits(uniqueIdentifierDigitPart);
        
        return new string(chars);
    }

    private static int GetTotalLength()
    {
        const int modelNumberLength = SerialNumberValues.ModelNumberLength;
        const int serialIdentifierFirstPartLength = SerialNumberValues.SerialIdentifierFirstPartLength;
        const int serialIdentifierKnownDigitLength = 1;
        const int serialIdentifierLastPartLength = SerialNumberValues.SerialIdentifierLastPartLength;
        const int uniqueIdentifierLetterPartLength = SerialNumberValues.UniqueIdentifierLetterPartLength;
        const int uniqueIdentifierDigitPartLength = SerialNumberValues.UniqueIdentifierDigitPartLength;

        const int length = modelNumberLength +
                     serialIdentifierFirstPartLength +
                     serialIdentifierKnownDigitLength +
                     serialIdentifierLastPartLength +
                     uniqueIdentifierLetterPartLength +
                     uniqueIdentifierDigitPartLength;

        return length;
    }

    private static void FillWithLetters(Span<char> buffer)
    {
        for (int i = 0; i < buffer.Length; i++)
        {
            int alphabetIndex = Random.Shared.Next(Alphabet.Length);
            buffer[i] = Alphabet[alphabetIndex];
        }
    }

    private static void FillWithDigits(Span<char> buffer)
    {
        for (int i = 0; i < buffer.Length; i++)
        {
            int digit = Random.Shared.Next(0, 9);
            buffer[i] = (char)('0' + digit);
        }
    }
}