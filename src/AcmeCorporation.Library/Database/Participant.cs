namespace AcmeCorporation.Library.Database;

internal record Participant
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required ICollection<RaffleEntry> RaffleEntries { get; set; }
}