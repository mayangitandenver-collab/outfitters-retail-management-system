using Microsoft.EntityFrameworkCore;

namespace Outfitters.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options);
