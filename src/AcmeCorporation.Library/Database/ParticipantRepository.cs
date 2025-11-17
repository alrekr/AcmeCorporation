using System.Data;
using Dapper;

namespace AcmeCorporation.Library.Database;

public class ParticipantRepository 
{
    private readonly AcmeDatabase _database;
    public ParticipantRepository(AcmeDatabase database)
    {
        _database = database;
    }

    /// <summary>
    /// Adds a new participant with the specified details if one does not already exist, or retrieves the identifier of
    /// an existing participant.
    /// </summary>
    /// <remarks>If a participant with the specified first name, last name, and email already exists, the
    /// existing identifier is returned. Otherwise, a new participant is created and the new identifier is returned.
    /// This method is not thread-safe; concurrent calls with the same details may result in duplicate
    /// participants.</remarks>
    /// <param name="firstName">The first name of the participant. Cannot be null or empty.</param>
    /// <param name="lastName">The last name of the participant. Cannot be null or empty.</param>
    /// <param name="email">The email address of the participant. Cannot be null or empty.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the unique identifier of the
    /// participant.</returns>
    public async Task<int> AddOrGetParticipant(string firstName, string lastName, string email, CancellationToken cancellationToken)
    {
        var existingParticipant = await GetParticipantId(firstName, lastName, email, cancellationToken);
        
        if (existingParticipant != 0)
        {
            return existingParticipant;
        }

        using IDbConnection dbConnection = _database.CreateConnection();
        const string insertQuery = """
                                   insert into acme.RaffleParticipant (FirstName, LastName, Email)
                                   values (@FirstName, @LastName, @Email)
                                   """;

        CommandDefinition command = new(
            insertQuery,
            parameters: new { FirstName = firstName, LastName = lastName, Email = email },
            cancellationToken: cancellationToken);

        await dbConnection.ExecuteAsync(command);

        return await GetParticipantId(firstName, lastName, email, cancellationToken);
    }

    public async Task<bool> ParticipantExists(string firstName, string lastName, string email)
    {
        using IDbConnection dbConnection = _database.CreateConnection();
        const string selectQuery = """
                                   select count(1)
                                   from acme.RaffleParticipant
                                   where FirstName = @FirstName and LastName = @LastName and Email = @Email
                                   """;

        int count = await dbConnection.ExecuteScalarAsync<int>(selectQuery,
            new { FirstName = firstName, LastName = lastName, Email = email });

        return count > 0;
    }

    /// <summary>
    /// Retrieves the unique identifier of a participant matching the specified first name, last name, and email
    /// address.
    /// </summary>
    /// <param name="firstName">The first name of the participant to search for. Cannot be null or empty.</param>
    /// <param name="lastName">The last name of the participant to search for. Cannot be null or empty.</param>
    /// <param name="email">The email address of the participant to search for. Cannot be null or empty.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the unique identifier of the
    /// matching participant. Returns 0 if no participant is found.</returns>
    public async Task<int> GetParticipantId(string firstName, string lastName, string email, CancellationToken cancellationToken)
    {
        using IDbConnection dbConnection = _database.CreateConnection();
        const string selectQuery = """
                                   select Id
                                   from acme.RaffleParticipant
                                   where FirstName = @FirstName and LastName = @LastName and Email = @Email
                                   """;
        CommandDefinition command = new(
            selectQuery,
            parameters: new { FirstName = firstName, LastName = lastName, Email = email },
            cancellationToken: cancellationToken);

        int participantId = await dbConnection.ExecuteScalarAsync<int>(command);
        
        return participantId;
    }
}