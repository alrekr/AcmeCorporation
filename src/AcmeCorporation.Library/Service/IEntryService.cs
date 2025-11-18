using AcmeCorporation.Library.Datacontracts;

namespace AcmeCorporation.Library.Service;

public interface IEntryService
{
    /// <summary>
    /// Creates a new participant and registers a raffle entry using the provided information.
    /// </summary>
    /// <param name="firstName">The first name of the participant. Cannot be null or empty.</param>
    /// <param name="lastName">The last name of the participant. Cannot be null or empty.</param>
    /// <param name="email">The email address of the participant. Must be a valid email format and cannot be null or empty.</param>
    /// <param name="serialNumber">The serial number associated with the raffle entry. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a tuple with an <see cref="EntryServiceError"/>
    /// with error information and a List&lt;string&gt; which might contain validation error messages.
    /// If the operation succeeded, the value of error is None.
    /// <remarks>It is the caller's responsibility to handle the error message.</remarks></returns>
    Task<(EntryServiceError error, List<string> errorMessages)> CreateParticipantAndRaffleEntry(string firstName, string lastName, string email, string serialNumber, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a paged list of raffle entries.
    /// </summary>
    /// <param name="page">Indicates which page of results to return</param>
    /// <param name="pageSize">Indicates how many results per page</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="PagedResult{T}"/> containing the raffle entries, which page is returned,
    /// the page size, and how many entries available in total.</returns>
    Task<PagedResult<RaffleEntryViewDto>> GetPagedEntriesAsync(int page, int pageSize, CancellationToken cancellationToken);
}