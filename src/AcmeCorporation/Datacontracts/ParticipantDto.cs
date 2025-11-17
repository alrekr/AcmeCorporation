using System.ComponentModel.DataAnnotations;

namespace AcmeCorporation.Datacontracts;

public record ParticipantDto
{
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}