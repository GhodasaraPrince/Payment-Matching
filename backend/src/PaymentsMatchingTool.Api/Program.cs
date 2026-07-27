using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using PaymentsMatchingTool.Application.Csv;
using PaymentsMatchingTool.Application.Matching;
using PaymentsMatchingTool.Infrastructure;
using PaymentsMatchingTool.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicyName = "Frontend";

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Payments Matching Tool API",
        Version = "v1",
    });
});

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

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Payments Matching Tool API v1");
});

app.UseCors(CorsPolicyName);
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
