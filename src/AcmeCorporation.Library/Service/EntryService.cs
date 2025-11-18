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
    private readonly TimeProvider _timeProvider;

    public EntryService(
        ParticipantRepository participantRepository,
        RaffleRepository raffleRepository,
        ISerialNumberValidator serialNumberValidator,
        IOptions<RaffleOptions> options,
        TimeProvider timeProvider)
    {
        _participantRepository = participantRepository;
        _raffleRepository = raffleRepository;
        _serialNumberValidator = serialNumberValidator;
        _options = options;
        _timeProvider = timeProvider;
    }

    /// <inheritdoc/>
    public async Task<(EntryServiceError error, List<string> errorMessages)> CreateParticipantAndRaffleEntry(string firstName, string lastName, string email, string serialNumber, DateOnly dateOfBirth, CancellationToken cancellationToken)
    {
        var isEligible = VerifyAge(dateOfBirth);
        if (!isEligible)
        {
            return (EntryServiceError.AgeNotEligible, ["User must be over 18 years."]);
        }

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

        DateTime now = _timeProvider.GetUtcNow().UtcDateTime;
        bool success = await _raffleRepository.AddRaffle(serialNumber, participant, now, cancellationToken);

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

    /// <inheritdoc />
    public bool VerifyAge(DateOnly dateOfBirth)
    {
        DateOnly today = DateOnly.FromDateTime(_timeProvider.GetLocalNow().Date);
        int age = today.Year - dateOfBirth.Year;

        if (dateOfBirth > today.AddYears(-age))
        {
            age--;
        }

        return age >= 18;
    }
}