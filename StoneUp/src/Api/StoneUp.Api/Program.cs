using System.Text.Json.Serialization;
using Catalog.Infrastructure;
using Identity.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using StoneUp.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services
    .AddIdentityModule(builder.Configuration)
    .AddCatalogModule(builder.Configuration);

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    using var migrationScope = app.Services.CreateScope();
    await migrationScope.ServiceProvider.GetRequiredService<IdentityModuleDbContext>().Database.MigrateAsync();
    await migrationScope.ServiceProvider.GetRequiredService<CatalogDbContext>().Database.MigrateAsync();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
