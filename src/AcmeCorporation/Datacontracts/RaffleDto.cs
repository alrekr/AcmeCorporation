using System.ComponentModel.DataAnnotations;
using AcmeCorporation.Attributes;

namespace AcmeCorporation.Datacontracts;

public record RaffleDto
{
    public ParticipantDto Participant { get; set; } = new();

    [Required] 
    [SerialNumber] 
    public string SerialNumber { get; set; } = string.Empty;

    public RaffleDto() { }

    public RaffleDto(string firstName, string lastName, string email, string serialNumber)
    {
        Participant = new ParticipantDto
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email
        };
        
        SerialNumber = serialNumber;
    }
}