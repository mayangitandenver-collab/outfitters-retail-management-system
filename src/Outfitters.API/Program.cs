using Microsoft.EntityFrameworkCore;
using Outfitters.Application;
using Outfitters.Infrastructure;
using Outfitters.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("postgresql");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Ok(new
{
    service = "Outfitters Retail Management System API",
    status = "running",
    version = "0.1.0"
}))
.WithName("GetApiStatus")
.WithOpenApi();

app.MapHealthChecks("/health");

app.Run();

public partial class Program
{
}
