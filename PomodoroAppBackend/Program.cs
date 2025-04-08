using Microsoft.EntityFrameworkCore;
using PomodoroAppBackend.Context;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    // Define a CORS policy named "AllowReactApp" to allow requests from the specified origin
    options.AddPolicy("AllowReactApp", builder =>
    {
        builder.WithOrigins("http://localhost:5173", "http://localhost:5173") // React frontend's URL
            .AllowAnyMethod() // Allow any HTTP method (GET, POST, PUT, DELETE, etc.)
            .AllowAnyHeader()
            .SetIsOriginAllowed(origin => true)
            .AllowCredentials(); // For using cookies or session
    });
});

builder.WebHost.ConfigureKestrel(options => { options.ListenAnyIP(8080); });

builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register HttpClient
builder.Services.AddControllers();
builder.Services.AddHttpClient();

// Add session services
builder.Services.AddDistributedMemoryCache(); // For in-memory session storage
builder.Services.AddSession(options =>
{
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Always use secure cookies
    options.Cookie.SameSite = SameSiteMode.None; // Allow cross-origin cookies
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set timeout for session
    options.Cookie.HttpOnly = true; // Protect the session cookie
    options.Cookie.IsEssential = true; // Make the cookie essential
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Check if we should only run migrations and then exit
if (args.Contains("--migrate"))
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<ApplicationDBContext>();
            context.Database.Migrate();
            Console.WriteLine("Database migration completed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while migrating the database: {ex.Message}");
            // Exit with error code
            Environment.Exit(1);
        }
    }

    // Exit after migration
    Environment.Exit(0);
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    ForwardLimit = null,
    RequireHeaderSymmetry = false
});

// Continue with normal application startup
app.UseCors("AllowReactApp");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSession();

// Commented as it is not needed when nginx handles https redirection
// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Always run migrations on application startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
    db.Database.Migrate();
}

app.Run();