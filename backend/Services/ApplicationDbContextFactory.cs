using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using backend.Config;

// This factory class is required for EF Core migrations to work.
// It tells the 'dotnet ef' command-line tools how to create an instance of your DbContext.
// It must not be in a namespace.
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    // The DbContextOptionsBuilder is used to configure the options for the DbContext.
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // This is the connection string that the EF Core tool will use.
        // It should match the one used by your application.
        // Replace this with your actual connection string.
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=superchat;Trusted_Connection=True;MultipleActiveResultSets=true");

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}