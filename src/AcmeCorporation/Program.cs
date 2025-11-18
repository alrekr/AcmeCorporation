using AcmeCorporation.Configuration;
using AcmeCorporation.Library;
using AcmeCorporation.Library.Database;
using AcmeCorporation.Library.Datacontracts;
using AcmeCorporation.Library.Service;
using AcmeCorporation.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
string connectionString = builder.Configuration.GetConnectionString("AcmeCorporationContextConnection") ?? throw new InvalidOperationException("Connection string 'AcmeCorporationContextConnection' not found.");

builder.Services.AddDbContext<AcmeCorporationContext>(options => options.UseSqlServer(connectionString));

// since this is mainly for demonstration purposes, the account may be unconfirmed.
builder.Services
    .AddIdentity<AcmeCorporationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<AcmeCorporationContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

// Add services
builder.Services.AddRazorPages();
builder.Services.AddSingleton<AcmeDatabase>();
builder.Services.AddScoped<ParticipantRepository>();
builder.Services.AddScoped<RaffleRepository>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<ISerialNumberValidator, SerialNumberValidator>();
builder.Services.AddSingleton<ISerialNumberGenerator, SerialNumberGenerator>();
builder.Services.AddScoped<IEntryService, EntryService>();
builder.Services.AddScoped<AdminUserSeeder>();
builder.Services.AddScoped<RaffleSessionState>();

builder.Services.AddScoped<DevelopmentSeeder>();

builder.Services.AddServerSideBlazor();
string environment = builder.Environment.EnvironmentName;
builder.Configuration.AddJsonFile("appsettings.json").AddJsonFile($"appsettings.{environment}.json");
builder.Services.AddOptions<RaffleOptions>().Bind(builder.Configuration.GetSection(RaffleOptions.SectionName));
builder.Services.AddOptions<AdminOptions>().Bind(builder.Configuration.GetSection(AdminOptions.SectionName));
builder.Services.AddOptions<SerialNumberApiOptions>().Bind(builder.Configuration.GetSection(SerialNumberApiOptions.SectionName));

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapBlazorHub();
app.MapRazorPages();

app.MapFallbackToPage("/_Host");

await SeedDatabase(app);

app.MapGet("/api/v1/serialnumbers", (int? n, ISerialNumberGenerator generator, IOptions<SerialNumberApiOptions> options) =>
{
    int count = n ?? 1;
    if (count < 1)
    {
        return Results.BadRequest("Count (n) must be greater than 0.");
    }

    if (count > options.Value.MaxBatchSize)
    {
        return Results.BadRequest($"You cannot generate more than {options.Value.MaxBatchSize} serial numbers.");
    }

    string[] serialNumbers = new string[count];
    for (int i = 0; i < count; i++)
    {
        serialNumbers[i] = generator.GenerateSerialNumber();
    }

    return Results.Ok(serialNumbers);
})
    .WithName("GenerateSerialNumbers")
    .WithSummary("Generates a list of unique serial numbers.")
    .WithDescription("Returns a JSON array of serial numbers. Limits apply.")
    .Produces<List<string>>()
    .Produces(StatusCodes.Status400BadRequest);


if (app.Environment.IsDevelopment())
{
    app.MapPost("/api/dev/seed/{count:int}",
            async (int count, DevelopmentSeeder seeder, CancellationToken cancellationToken) =>
            {
                if (count > 1000)
                {
                    return Results.BadRequest("Let's limit seeding to 1000 at a time for now");
                }

                int created = await seeder.SeedAsync(count, cancellationToken);

                return Results.Ok(new { Message = $"Successfully seeded {created} entries." });
            })
        .WithSummary("Dev only: Seeds the database with fake raffle entries")
        .WithTags("Development");
}
app.Run();

return;

static async Task SeedDatabase(WebApplication webApplication)
{
    // Usually this should be done via a secure method, such as a CLI for server apps.
    // For demonstration purposes this is done here.

    try
    {
        using IServiceScope scope = webApplication.Services.CreateScope();
        IServiceProvider services = scope.ServiceProvider;
        AcmeCorporationContext context = services.GetRequiredService<AcmeCorporationContext>();
        if ((await context.Database.GetPendingMigrationsAsync()).Any())
        {
            await context.Database.MigrateAsync();
        }

        AdminUserSeeder adminSeeder = scope.ServiceProvider.GetRequiredService<AdminUserSeeder>();
        await adminSeeder.SeedAdminUserAsync();
    }
    catch (Exception ex)
    {
        ILogger<Program> logger = webApplication.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding or migrating the database.");
    }
}

public partial class Program { }