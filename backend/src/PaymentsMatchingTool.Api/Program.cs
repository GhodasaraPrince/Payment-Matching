using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using PaymentsMatchingTool.Application.Csv;
using PaymentsMatchingTool.Application.Matching;
using PaymentsMatchingTool.Infrastructure;
using PaymentsMatchingTool.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicyName = "Frontend";
const string UploadRateLimiterPolicy = "upload";

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddProblemDetails();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Payments Matching Tool API",
            Version = "v1",
        });
    });
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<ICsvPaymentParser, CsvPaymentParser>();
builder.Services.AddScoped<IPaymentMatchingService, PaymentMatchingService>();
builder.Services.AddScoped<IMatchRunService, MatchRunService>();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Per-client-IP global cap so no single caller can flood the API.
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 120,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
            }));

    // Tighter limit specifically for the CSV upload/match endpoint.
    options.AddFixedWindowLimiter(UploadRateLimiterPolicy, limiterOptions =>
    {
        limiterOptions.PermitLimit = 10;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
    });
});

var app = builder.Build();

app.UseExceptionHandler();

if (builder.Configuration.GetValue("Database:AutoMigrate", true))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Payments Matching Tool API v1");
    });
}
else
{
    // Only enforced outside Development: Docker/local profiles here have no HTTPS
    // endpoint bound, so redirecting there would 301 to a port that doesn't exist.
    app.UseHttpsRedirection();
    app.UseHsts();
}

app.UseCors(CorsPolicyName);
app.UseRateLimiter();
app.UseAuthorization();
app.MapControllers();

app.Run();
