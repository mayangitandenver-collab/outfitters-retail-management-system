using Microsoft.EntityFrameworkCore;
using Outfitters.Infrastructure.Persistence;

if (args.Length != 1 || string.IsNullOrWhiteSpace(args[0]))
{
    Console.Error.WriteLine("Usage: Outfitters.DatabaseTool.exe <connection-string>");
    return 2;
}

try
{
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseNpgsql(args[0])
        .Options;

    await using var db = new ApplicationDbContext(options);
    await db.Database.MigrateAsync();
    Console.WriteLine("OUTFITTERS database migrations applied successfully.");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex.ToString());
    return 1;
}
