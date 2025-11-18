using AcmeCorporation.Library.Database;
using AcmeCorporation.Library.Datacontracts;
using Microsoft.Extensions.Options;

namespace AcmeCorporation.Library.Service;

public class EntryService : IEntryService
{
    private readonly ParticipantRepository _participantRepository;
    private readonly RaffleRepository _raffleRepository;
    private readonly ISerialNumberValidator _serialNumberValidator;
    private readonly IOptions<RaffleOptions> _options;

    public EntryService(
        ParticipantRepository participantRepository, 
        RaffleRepository raffleRepository, 
        ISerialNumberValidator serialNumberValidator,
        IOptions<RaffleOptions> options)
    {
        _participantRepository = participantRepository;
        _raffleRepository = raffleRepository;
        _serialNumberValidator = serialNumberValidator;
        _options = options;
    }

    /// <inheritdoc/>
    public async Task<(EntryServiceError error, List<string> errorMessages)> CreateParticipantAndRaffleEntry(string firstName, string lastName, string email, string serialNumber, CancellationToken cancellationToken)
    {
        (bool valid, List<string> errors) = _serialNumberValidator.ValidateSerialNumber(serialNumber);
        
        if (!valid)
        {
            return (EntryServiceError.Unknown, errors);
        }

        int participant = await _participantRepository.AddOrGetParticipant(firstName, lastName, email, cancellationToken);
        int count = await _raffleRepository.GetRaffleCount(serialNumber, cancellationToken);

        if (count >= _options.Value.MaxCount)
        {
            return (EntryServiceError.RaffleEntryExceedMax, []);
        }

        bool success = await _raffleRepository.AddRaffle(serialNumber, participant, count, cancellationToken);

        if (success)
        {
            return (EntryServiceError.None, []);
        }

        return (EntryServiceError.Unknown, []);
    }

    /// <inheritdoc/>
    public async Task<PagedResult<RaffleEntryViewDto>> GetPagedEntriesAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        return await _raffleRepository.GetPagesEntriesAsync(page, pageSize, cancellationToken);
    }
}