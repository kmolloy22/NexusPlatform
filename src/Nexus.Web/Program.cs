using MudBlazor;
using MudBlazor.Services;
using Nexus.Web;
using Nexus.Web.Services;
using Refit;

var builder = WebApplication.CreateBuilder(args);

// Add logging for debugging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Add service defaults first
builder.AddServiceDefaults();

// Add Blazor services with detailed configuration
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options =>
    {
        options.DetailedErrors = builder.Environment.IsDevelopment();
    });

// Add MudBlazor with full configuration
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 3000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
});

// Configure API Client with service discovery
var apiBaseUrl = builder.Configuration["services__nexus-customer-order-api__http__0"]
    ?? builder.Configuration["ApiBaseUrl"]
    ?? "http://localhost:5270";

Console.WriteLine($"Configuring API Client with base URL: {apiBaseUrl}");

builder.Services.AddRefitClient<IAccountsApiClient>()
    .ConfigureHttpClient(c =>
    {
        c.BaseAddress = new Uri(apiBaseUrl);
        c.Timeout = TimeSpan.FromSeconds(30);
    })
    .AddStandardResilienceHandler();
//.AddServiceDiscovery();

builder.Services.AddRefitClient<IProductsApiClient>()
    .ConfigureHttpClient(c =>
    {
        c.BaseAddress = new Uri(apiBaseUrl);
        c.Timeout = TimeSpan.FromSeconds(30);
    })
    .AddStandardResilienceHandler();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// Map Blazor components with interactive server mode
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

Console.WriteLine("Application started successfully!");

app.Run();