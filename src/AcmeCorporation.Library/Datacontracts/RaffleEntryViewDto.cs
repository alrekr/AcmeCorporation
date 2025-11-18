namespace AcmeCorporation.Library.Datacontracts;

public sealed record RaffleEntryViewDto
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string SerialNumber { get; init; } = string.Empty;
    public DateTime EntryDateTimeUtc { get; init; }
}