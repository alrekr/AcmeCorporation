namespace AcmeCorporation.Library.Database;

internal record RaffleEntry
{
    public int Id { get; set; }
    public required Participant Participant { get; set; } 
    public DateTime EntryDateTimeUtc { get; set; }
    public required string SerialNumber { get; set; }
    public int EntryCount { get; set; } = 0;
}