using System.ComponentModel.DataAnnotations;
using AcmeCorporation.Library;
using Shouldly;

namespace AcmeCorporation.Tests;

[TestFixture]
public class SerialNumberValidatorTests
{
#pragma warning disable CA1859 // disable warning about "change type of field" 
    private readonly ISerialNumberValidator _validator = new SerialNumberValidator();
#pragma warning restore CA1859

    [TestCase("abcd123435678efg90")]
    public void SerialNumber_ValidSerialNumber_ReturnsNoError(string serialNumber)
    {
        // Arrange 
        const bool valid = true;

        // Act
        (bool serialNumberValid, List<string> errors) = _validator.ValidateSerialNumber(serialNumber);

        // Assert
        serialNumberValid.ShouldBe(valid);
        errors.ShouldBeEmpty();
    }

    [TestCase("abcd123435678efgh0")]
    [TestCase("abcd123435678efghi")]
    public void UniqueIdentifier_ContainsInvalidDigitPart_ReturnsError(string uniqueIdentifier)
    {
        // Arrange
        string errorMessage = SerialNumberErrorMessages.UniqueIdentifierDigitPartWrong;
        const int expectedErrorCount = 1;
        const bool invalid = false;

        // Act
        (bool valid, List<string> errors) = _validator.ValidateSerialNumber(uniqueIdentifier);

        // Assert
        errors.Count.ShouldBe(expectedErrorCount);
        valid.ShouldBe(invalid);
        errors.First().ShouldBe(errorMessage);
    }

    [TestCase("abcd123435678ef012")]
    [TestCase("abcd12343567801234")]
    [TestCase("abcd123435678ef 02")]
    public void UniqueIdentifier_ContainsInvalidLetterPart_ReturnsError(string uniqueIdentifier)
    {
        // Arrange
        string errorMessage = SerialNumberErrorMessages.UniqueIdentifierLetterPartWrong;
        const int expectedErrorCount = 1;
        const bool invalid = false;

        // Act
        (bool valid, List<string> errors) = _validator.ValidateSerialNumber(uniqueIdentifier);

        // Assert
        valid.ShouldBe(invalid);
        errors.Count.ShouldBe(expectedErrorCount);
        errors.First().ShouldBe(errorMessage);
    }

    [TestCase("abcd123495678efg90")]
    public void SerialIdentifier_InvalidMiddleCharacter_ReturnsError(string serialIdentifier)
    {
        // Arrange
        string errorMessage = SerialNumberErrorMessages.SerialIdentifierSpecialMiddleChar;
        const int expectedErrorCount = 1;
        const bool invalid = false;

        // Act
        (bool valid, List<string> errors) = _validator.ValidateSerialNumber(serialIdentifier);

        // Assert
        errors.Count.ShouldBe(expectedErrorCount);
        errors.First().ShouldBe(errorMessage);
        valid.ShouldBe(invalid);
    }

    [TestCase("abcd123a35678efg90")]
    public void SerialIdentifier_ContainsLetter_ReturnsError(string serialIdentifier)
    {
        // Arrange
        string errorMessage = SerialNumberErrorMessages.SerialIdentifierOnlyDigits;
        const bool invalid = false;
        const int expectedErrorCount = 1;

        // Act
        (bool valid, List<string> errors) = _validator.ValidateSerialNumber(serialIdentifier);

        // Assert
        errors.First().ShouldBe(errorMessage);
        errors.Count.ShouldBe(expectedErrorCount);
        valid.ShouldBe(invalid);
    }

    [TestCase("4bcd123435678efg90")]
    [TestCase("abc4123435678efg90")]
    public void ModelNumber_ContainsDigit_ReturnsError(string modelNumber)
    {
        // Arrange
        string errorMessage = SerialNumberErrorMessages.ModelNumberAllLetters;
        const int expectedErrorCount = 1;
        const bool invalid = false;

        // Act
        (bool valid, List<string> errors) = _validator.ValidateSerialNumber(modelNumber);

        // Assert
        errors.Count.ShouldBe(expectedErrorCount);
        errors.First().ShouldBe(errorMessage);
        valid.ShouldBe(invalid);
    }
    
    [TestCase("")]
    public void StringHasContent_EmptyOrNullString_ReturnsError(string serialNumber)
    {
        // Arrange
        const bool invalid = false;
        string errorMessage = SerialNumberErrorMessages.SerialNumberMustHaveContent;
        const int expectedErrorCount = 1;

        // Act
        (bool valid, List<string> errors) = _validator.ValidateSerialNumber(serialNumber);

        // Assert
        errors.Count.ShouldBe(expectedErrorCount);
        errors.First().ShouldBe(errorMessage);
        valid.ShouldBe(invalid);
    }

    [TestCase("abcd")]
    [TestCase("01234567890123456")]
    public void StringIsInvalidLength_TooShortOrTooLong_ReturnsError(string serialNumber)
    {
        // Assert
        const bool invalid = false;
        string errorMessage = SerialNumberErrorMessages.SerialNumberMustHaveLength;
        const int expectedErrorCount = 1;

        // Act
        (bool valid, List<string> errors) = _validator.ValidateSerialNumber(serialNumber);

        // Assert
        valid.ShouldBe(invalid);
        errors.Count.ShouldBe(expectedErrorCount);
        errors.First().ShouldBe(errorMessage);
    }
}