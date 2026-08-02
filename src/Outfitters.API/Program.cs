using Outfitters.Infrastructure.Integrations;
using Microsoft.AspNetCore.Identity;
using Outfitters.Domain.Entities;
using Outfitters.Infrastructure;
using Outfitters.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<IBarcodeService, BarcodeService>();
builder.Services.AddScoped<IReceiptFormatter, EscPosReceiptFormatter>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
    await DatabaseSeeder.SeedAsync(db, userManager, roleManager);
}

app.Run();

public partial class Program;
