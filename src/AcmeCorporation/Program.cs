using AcmeCorporation.Library;
using AcmeCorporation.Library.Database;
using AcmeCorporation.Library.Datacontracts;
using AcmeCorporation.Library.Service;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSingleton<AcmeDatabase>();
builder.Services.AddScoped<ParticipantRepository>();
builder.Services.AddScoped<RaffleRepository>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<ISerialNumberValidator, SerialNumberValidator>();
builder.Services.AddScoped<IEntryService, EntryService>();
builder.Services.AddServerSideBlazor();
var environment = builder.Environment.EnvironmentName;
builder.Configuration.AddJsonFile("appsettings.json").AddJsonFile($"appsettings.{environment}.json");
builder.Services.AddOptions<RaffleOptions>().Bind(builder.Configuration.GetSection(RaffleOptions.SectionName));

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

app.UseAuthorization();
app.MapBlazorHub();
app.MapRazorPages();

app.Run();
