using AcmeCorporation.Library;
using Shouldly;

namespace AcmeCorporation.Tests;

public class SerialNumberGeneratorTests
{
    private static bool IsAlphabet(char c) => c is >= 'A' and <= 'Z';

#pragma warning disable CA1859 // Suppress "Avoid instantiating types in attribute arguments" for test purposes
    private readonly ISerialNumberGenerator _generator = new SerialNumberGenerator();
#pragma warning restore CA1859

    [Test]
    [Repeat(10)]
    public void CreateSerialNumber_CreatesStringWithCorrectStructure()
    {
        // Arrange
        const int modelNumberLength = SerialNumberValues.ModelNumberLength;
        const int serialIdFirstPartLength = SerialNumberValues.SerialIdentifierFirstPartLength;
        const int serialIdLastPartLength = SerialNumberValues.SerialIdentifierLastPartLength;
        const int uniqueLettersLength = SerialNumberValues.UniqueIdentifierLetterPartLength;
        const int uniqueDigitsLength = SerialNumberValues.UniqueIdentifierDigitPartLength;

        const char expectedMidChar = SerialNumberValues.KnownSerialIdentifierMidChar;

        const int expectedLength = modelNumberLength +
                             serialIdFirstPartLength + 
                             1 + // +1 for the fixedDigit
                             serialIdLastPartLength + 
                             uniqueLettersLength + 
                             uniqueDigitsLength;

        // Act
        string serialNumber = _generator.CreateSerialNumber();

        // Assert
        serialNumber.Length.ShouldBe(expectedLength);
        ReadOnlySpan<char> span = serialNumber.AsSpan();
        int currentPosition = 0;

        // Model Number
        ReadOnlySpan<char> modelPart = span.Slice(currentPosition, modelNumberLength);
        modelPart.ToString().ShouldAllBe(c => IsAlphabet(c));
        currentPosition+= modelNumberLength;

        // Serial Identifier
        ReadOnlySpan<char> identFirstPart = span.Slice(currentPosition, serialIdFirstPartLength);
        identFirstPart.ToString().ShouldAllBe(c => char.IsDigit(c));
        currentPosition += serialIdFirstPartLength;
        span[currentPosition].ShouldBe(expectedMidChar);
        currentPosition += 1;
        ReadOnlySpan<char> identLastPart = span.Slice(currentPosition, serialIdLastPartLength);
        identLastPart.ToString().ShouldAllBe(c => char.IsDigit(c));
        currentPosition += serialIdLastPartLength;
        
        // Unique Identifier
        ReadOnlySpan<char> uniqueLetterPart = span.Slice(currentPosition, uniqueLettersLength);
        uniqueLetterPart.ToString().ShouldAllBe(c => IsAlphabet(c));
        currentPosition += uniqueLettersLength;
        ReadOnlySpan<char> uniqueDigitPart = span.Slice(currentPosition, uniqueDigitsLength);
        uniqueDigitPart.ToString().ShouldAllBe(c => char.IsDigit(c));
    }
}