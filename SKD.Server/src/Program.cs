var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";
var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>() ?? new AppSettings();

builder.Services.SetupCors();
builder.Services.SetupSkdContext(connectionString);
builder.Services.SetupSingletonServices(appSettings);
builder.Services.SetupGraphqlServer(builder.Environment, appSettings);
builder.Services.SetupKitStatusFeedService(builder.Configuration);
builder.Services.SetupScopedServices(builder.Environment, appSettings);
if (!builder.Environment.IsDevelopment()) {
    builder.Services.AddApplicationInsightsTelemetry();
}

var app = builder.Build();

app.UseCors();
app.MapGraphQL();
app.MapRouting();

app.Run();
