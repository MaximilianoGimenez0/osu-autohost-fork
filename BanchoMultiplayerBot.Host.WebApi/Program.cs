using System.Security.Claims;
using BanchoMultiplayerBot;
using BanchoMultiplayerBot.Constants;
using BanchoMultiplayerBot.Database;
using BanchoMultiplayerBot.Host.WebApi.Extensions;
using BanchoMultiplayerBot.Host.WebApi.Hubs;
using BanchoMultiplayerBot.Host.WebApi.Providers;
using BanchoMultiplayerBot.Host.WebApi.Services;
using BanchoMultiplayerBot.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Prometheus;
using Serilog;
using Serilog.Sinks.OpenTelemetry;

const string logOutputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}";

// Setup 30 day rolling file logging
// We also ignore verbose logs in the file, only writing information and above
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: logOutputTemplate)
    .WriteTo.File("log.txt",
        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        rollOnFileSizeLimit: true,
        outputTemplate: logOutputTemplate)
    .WriteTo.OpenTelemetry()
    .CreateLogger();

// This is generally done as a fail-safe to log any unhandled exceptions to the log file
// Although I would rather hope that this is never needed
AppDomain.CurrentDomain.UnhandledException += (_, args) =>
{
    var e = (Exception)args.ExceptionObject;
    Log.Fatal(e, BotMessages.Logs.Host.UnhandledException);
};

var builder = WebApplication.CreateBuilder(args);

// As per the ConnectionString comment, but this is needed since the bot itself doesn't use DI
BotDbContext.ConnectionString = builder.Configuration.GetConnectionString("Bot") ?? throw new InvalidOperationException("Database connection string not found.");

Log.Information(BotMessages.Logs.Host.StartingEnvironment, builder.Environment.EnvironmentName);

builder.Services.AddDbContext<BotDbContext>(options => options.UseNpgsql(BotDbContext.ConnectionString));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

// Setup services
builder.Services.AddSingleton<IBotConfiguration>(new BotConfigurationProvider(builder.Configuration));
builder.Services.AddSingleton<Bot>();
builder.Services.AddSingleton<LobbyTrackerService>();
builder.Services.AddSingleton<BannerCacheService>();
builder.Services.AddSingleton<BotHealthService>();

builder.Services.AddScoped<BehaviorService>();
builder.Services.AddScoped<LobbyService>();
builder.Services.AddScoped<HealthService>();
builder.Services.AddScoped<MessageService>();
builder.Services.AddScoped<BannerService>();

// Setup osu! oauth authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddBotCookieAuthentication(builder.Configuration)
.AddBotOsuAuth(builder.Configuration);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("BotOwner", policyBuilder => policyBuilder.RequireClaim(ClaimTypes.Name, builder.Configuration["Osu:Username"]!));
});

// create a default CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        policy.WithOrigins(builder.Configuration["Bot:FrontendUrl"]!);
        policy.AllowCredentials();
        policy.AllowAnyHeader();
        policy.AllowAnyMethod();
    });
});

var app = builder.Build();

// Migrate database before starting bot services
await using (var scope = app.Services.CreateAsyncScope())
{
    try
    {
        Log.Information(BotMessages.Logs.Host.MigratingDatabase);
        var context = scope.ServiceProvider.GetRequiredService<BotDbContext>();
        await context.Database.MigrateAsync();
        Log.Information(BotMessages.Logs.Host.DatabaseMigrationCompleted);
    }
    catch (Exception e)
    {
        Log.Fatal(e, BotMessages.Logs.Host.DatabaseMigrationFailed);
        throw;
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection();
}
else
{
    // Since we use a reverse proxy in production
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });
    
    app.UseHttpsRedirection();
}

// Start services
app.Services.GetRequiredService<LobbyTrackerService>().Start();
await app.Services.GetRequiredService<Bot>().StartAsync();
app.Services.GetRequiredService<BotHealthService>().Start();

// Apply CORS policy
app.UseCors("DefaultPolicy");

// Apply authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Apply routing
app.MapControllers();
app.MapHub<LobbyEventHub>("/hubs/lobby");

// Applies a Prometheus metrics endpoint
app.UseMetricServer("/api/statistics/metrics");

await app.RunAsync();
