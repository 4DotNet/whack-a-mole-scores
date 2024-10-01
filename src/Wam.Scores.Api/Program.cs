using Azure.Identity;
using Wam.Core.Authentication;
using Wam.Core.Authentication.Swagger;
using Wam.Core.Configuration;
using Wam.Core.ExtensionMethods;
using Wam.Core.Filters;
using Wam.Core.Identity;
using Wam.Scores.ExtensionMethods;
using Wam.Scores.Services;

var corsPolicyName = "DefaultCors";
var builder = WebApplication.CreateBuilder(args);

var azureCredential = new AzureCliCredential();
try
{
    builder.Configuration.AddAzureAppConfiguration(options =>
    {
        var appConfigurationUrl = builder.Configuration.GetRequiredValue("AzureAppConfiguration");
        Console.WriteLine($"Configuring service using Azure App Configuration {appConfigurationUrl}");
        options.Connect(new Uri(appConfigurationUrl), azureCredential)
            .UseFeatureFlags();
    });
}
catch (Exception ex)
{
    throw new Exception("Failed to configure the Whack-A-Mole Scores service, Azure App Configuration failed", ex);
}
// Add services to the container.

builder.Services
    .AddWamCoreConfiguration(builder.Configuration, daprAppId: nameof(ServicesConfiguration.ScoresService))
    .AddWamScoresModel();
builder.Services.AddHttpClient<IGamesService, GamesService>()
    .AddStandardResilienceHandler();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: corsPolicyName,
        policy =>
        {
            policy.WithOrigins("https://wam.hexmaster.nl",
                    "https://wadmin.hexmaster.nl",
                    "https://wam-test.hexmaster.nl",
                    "https://wadmin-test.hexmaster.nl",
                    "https://mango-river-0dd954b03.4.azurestaticapps.net",
                    "http://localhost:4200")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

builder.Services.AddControllers(options => options.Filters.Add(new WamExceptionFilter())).AddNewtonsoftJson();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwagger("Whack-A-Mole Scores API", enableSwagger: !builder.Environment.IsProduction());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseCors(corsPolicyName);
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.UseDefaultHealthChecks();
app.MapControllers();

Console.WriteLine("Starting...");
app.Run();
Console.WriteLine("Stopped");