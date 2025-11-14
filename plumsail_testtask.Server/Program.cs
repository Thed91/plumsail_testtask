using Microsoft.EntityFrameworkCore;
using plumsail_testtask.Server.Data;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add Database Context with In-Memory database for easy deployment
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("FormSubmissionsDb"));

// Add controllers
builder.Services.AddControllers();

// Configure CORS - SECURE: Only allow specific origins
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        policy =>
        {
            // For development: allow localhost on different ports
            policy.WithOrigins(
                    "https://localhost:52408",  // Vite dev server
                    "https://localhost:7082",   // ASP.NET HTTPS
                    "http://localhost:52408",   // Vite dev server HTTP
                    "http://localhost:7082"     // ASP.NET HTTP
                  )
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();

            // For production: Replace with your actual domain
            // policy.WithOrigins("https://yourdomain.com")
        });
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Get logger for startup operations
var logger = app.Services.GetRequiredService<ILogger<Program>>();

// Database initialization and seeding
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    try
    {
        // Ensure database is created
        context.Database.EnsureCreated();
        logger.LogInformation("Database initialized successfully");

        // Seed test data if database is empty
        if (!context.FormSubmissions.Any())
        {
            logger.LogInformation("Seeding test data...");

        context.FormSubmissions.AddRange(
            new plumsail_testtask.Server.Models.FormSubmission
            {
                Id = Guid.NewGuid(),
                FormType = "contact",
                SubmittedAt = DateTime.UtcNow.AddHours(-5),
                DataJson = "{\"name\":\"John Smith\",\"selected\":\"A\",\"checked\":true,\"picked\":\"One\",\"date\":\"2024-01-15\"}"
            },
            new plumsail_testtask.Server.Models.FormSubmission
            {
                Id = Guid.NewGuid(),
                FormType = "contact",
                SubmittedAt = DateTime.UtcNow.AddHours(-4),
                DataJson = "{\"name\":\"maryjohnson@mail.test\",\"selected\":\"B\",\"checked\":false,\"picked\":\"Two\",\"date\":\"2024-02-20\"}"
            },
            new plumsail_testtask.Server.Models.FormSubmission
            {
                Id = Guid.NewGuid(),
                FormType = "contact",
                SubmittedAt = DateTime.UtcNow.AddHours(-3),
                DataJson = "{\"name\":\"William Brown\",\"selected\":\"C\",\"checked\":true,\"picked\":\"One\",\"date\":\"2024-03-10\"}"
            },
            new plumsail_testtask.Server.Models.FormSubmission
            {
                Id = Guid.NewGuid(),
                FormType = "contact",
                SubmittedAt = DateTime.UtcNow.AddHours(-2),
                DataJson = "{\"name\":\"alice.wonder@email.com\",\"selected\":\"A\",\"checked\":true,\"picked\":\"Two\",\"date\":\"2024-04-05\"}"
            },
            new plumsail_testtask.Server.Models.FormSubmission
            {
                Id = Guid.NewGuid(),
                FormType = "contact",
                SubmittedAt = DateTime.UtcNow.AddHours(-1),
                DataJson = "{\"name\":\"Bob Anderson\",\"selected\":\"B\",\"checked\":false,\"picked\":\"One\",\"date\":\"2024-05-12\"}"
            },
            new plumsail_testtask.Server.Models.FormSubmission
            {
                Id = Guid.NewGuid(),
                FormType = "contact",
                SubmittedAt = DateTime.UtcNow.AddMinutes(-45),
                DataJson = "{\"name\":\"Charlie Davis\",\"selected\":\"C\",\"checked\":true,\"picked\":\"Two\",\"date\":\"2024-06-18\"}"
            },
            new plumsail_testtask.Server.Models.FormSubmission
            {
                Id = Guid.NewGuid(),
                FormType = "contact",
                SubmittedAt = DateTime.UtcNow.AddMinutes(-30),
                DataJson = "{\"name\":\"diana.prince@hero.com\",\"selected\":\"A\",\"checked\":false,\"picked\":\"One\",\"date\":\"2024-07-22\"}"
            },
            new plumsail_testtask.Server.Models.FormSubmission
            {
                Id = Guid.NewGuid(),
                FormType = "contact",
                SubmittedAt = DateTime.UtcNow.AddMinutes(-15),
                DataJson = "{\"name\":\"Eve Martinez\",\"selected\":\"B\",\"checked\":true,\"picked\":\"Two\",\"date\":\"2024-08-30\"}"
            }
        );

            context.SaveChanges();
            logger.LogInformation("Successfully seeded {Count} test submissions", 8);
        }
        else
        {
            logger.LogInformation("Database already contains data, skipping seed");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while initializing the database");
        throw; // Re-throw to prevent app startup with broken database
    }
}

// Global exception handling middleware
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionHandler = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        var exception = exceptionHandler?.Error;

        if (exception != null)
        {
            var appLogger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            appLogger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        // Return different error messages based on environment
        var errorResponse = app.Environment.IsDevelopment()
            ? new { error = exception?.Message ?? "An error occurred", details = exception?.StackTrace }
            : new { error = "An internal server error occurred. Please try again later.", details = (string?)null };

        await context.Response.WriteAsJsonAsync(errorResponse);
    });
});

app.UseDefaultFiles();
app.MapStaticAssets();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    logger.LogInformation("OpenAPI documentation available at /openapi/v1.json");
}

app.UseHttpsRedirection();

// Apply CORS policy with specific origins
app.UseCors("AllowSpecificOrigins");

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
