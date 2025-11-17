using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;

namespace AcmeCorporation.Library.Database;

public class RaffleRepository
{
    private readonly AcmeDatabase _database;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<RaffleRepository> _logger;

    public RaffleRepository(AcmeDatabase database, TimeProvider timeProvider, ILogger<RaffleRepository> logger)
    {
        _database = database;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    /// <summary>
    /// Adds a raffle entry for a participant. 
    /// </summary>
    /// <param name="serialNumber">The serial number for the raffle. It is assumed that the serial number is already validated with the <see cref="SerialNumberValidator"/>.</param>
    /// <param name="participantId">Id of a participant. It is expected that the id is of the correct participant.</param>
    /// <param name="entryCount"></param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A bool that indicate 'success'.</returns>
    public async Task<bool> AddRaffle(string serialNumber, int participantId, int entryCount, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(serialNumber);

        using IDbConnection dbConnection = _database.CreateConnection();
        
        const string insertQuery = """
                                   insert into acme.RaffleEntry (SerialNumber, ParticipantId, EntryCount, EntryDateTimeUtc)
                                   values (@SerialNumber, @ParticipantId, @EntryCount, @EntryDateTimeUtc)
                                   """;

        var command = new CommandDefinition(
            insertQuery,
            parameters: new { SerialNumber = serialNumber, ParticipantId = participantId, EntryCount = entryCount, EntryDateTimeUtc = _timeProvider.GetUtcNow()},
            cancellationToken: cancellationToken);
        try
        {
            await dbConnection.ExecuteAsync(command);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Error occurred while adding raffle to the database.");
            return false;
        }
    }

    /// <summary>
    /// Retrieves the unique identifier of a raffle entry associated with the specified serial number.
    /// </summary>
    /// <remarks>If no raffle entry exists with the specified serial number, the method returns null.</remarks>
    /// <param name="serialNumber">The serial number of the raffle entry to search for. It is assumed that the serial number is already validated with the <see cref="SerialNumberValidator"/>.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A nullable integer representing the unique identifier of the raffle entry if found; otherwise, null.</returns>
    public async Task<int?> GetRaffleId(string serialNumber, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(serialNumber);

        using IDbConnection dbConnection = _database.CreateConnection();
        const string selectQuery = """
                                   select Id, SerialNumber, ParticipantId
                                   from acme.RaffleEntry
                                   where SerialNumber = @SerialNumber
                                   """;

        var command = new CommandDefinition(
            selectQuery,
            parameters: new { SerialNumber = serialNumber },
            cancellationToken: cancellationToken);

        RaffleEntry? raffleEntry = await dbConnection.QuerySingleOrDefaultAsync<RaffleEntry>(command);

        return raffleEntry?.Id;
    }


    /// <summary>
    /// Get the count of raffle entries for a given serial number.
    /// </summary>
    /// <param name="serialNumber">The serial number to search for. It is assumed that the serial number is already validated with the <see cref="SerialNumberValidator"/>.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The value of the EntryCount column; defaults to 0.</returns>
    public async Task<int> GetRaffleCount(string serialNumber, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(serialNumber);

        using IDbConnection dbConnection = _database.CreateConnection();
        const string selectQuery = """
                                   select count(*)
                                   from acme.RaffleEntry
                                   where SerialNumber = @SerialNumber
                                   """;

        var command = new CommandDefinition(
            selectQuery, 
            parameters: new { SerialNumber = serialNumber},
            cancellationToken: cancellationToken);

        int entryCount = await dbConnection.QuerySingleOrDefaultAsync<int>(command);

        return entryCount;
    }
}