using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PaymentsMatchingTool.Infrastructure;

// Used only by `dotnet ef migrations add` at design time. The real connection
// string at runtime comes from appsettings/environment via Program.cs.
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Database=payments_matching;Username=postgres;Password=postgres");

        return new AppDbContext(optionsBuilder.Options);
    }
}
