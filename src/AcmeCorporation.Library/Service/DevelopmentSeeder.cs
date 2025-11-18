using AcmeCorporation.Library.Database;
using Bogus;

namespace AcmeCorporation.Library.Service;

internal class DevelopmentSeeder
{
    private readonly ISerialNumberGenerator _serialNumberGenerator;
    private readonly ParticipantRepository _participantRepository;
    private readonly RaffleRepository _raffleRepository;
    
    public DevelopmentSeeder(ISerialNumberGenerator serialNumberGenerator, ParticipantRepository participantRepository, RaffleRepository raffleRepository)
    {
        _serialNumberGenerator = serialNumberGenerator;
        _participantRepository = participantRepository;
        _raffleRepository = raffleRepository;
    }

    public async Task<int> SeedAsync(int count, CancellationToken cancellationToken)
    {
        var faker = new Faker("en");

        int successCount = 0;

        for (int i = 0; i < count; i++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            string firstName = faker.Name.FirstName();
            string lastName = faker.Name.LastName();
            string email = faker.Internet.Email(firstName, lastName);
            string serialNumber = _serialNumberGenerator.GenerateSerialNumber();
            DateTime notNow = faker.Date.Past(1, DateTime.UtcNow);

            try
            {
                int participant = await _participantRepository.AddOrGetParticipant(firstName, lastName, email, cancellationToken);
                await _raffleRepository.AddRaffle(serialNumber, participant, notNow, cancellationToken);

                successCount++;
            }
            catch (Exception)
            {
                // ignore exception for duplicates that Bogus might create.
            }
        }

        return successCount;
    }
}
