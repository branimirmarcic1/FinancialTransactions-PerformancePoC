using Application.Commands;
using Application.Interfaces;
using Application.Queries;
using Infrastructure.Persistence;
using Infrastructure.Seeders;
using MediatR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddTransient<DatabaseSeeder>();
builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Application.Queries.GetTransactionsByCustomerQuery).Assembly));
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.MapPost("/api/seed-database", async (DatabaseSeeder seeder) =>
{
    try
    {
        await seeder.SeedOneMillionTransactionsAsync();
        return Results.Ok("Seeding završen! Provjeri konzolu za detalje.");
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
})
.WithName("SeedDatabase");
app.MapGet("/api/transactions/customer/{customerId}", async (Guid customerId, IMediator mediator) =>
{
    var query = new GetTransactionsByCustomerQuery(customerId);
    var result = await mediator.Send(query);

    return Results.Ok(new
    {
        Message = $"Pronađeno {result.Data.Count} transakcija u {result.ExecutionTimeMs} milisekundi.",
        ExecutionTimeMs = result.ExecutionTimeMs,
        Data = result.Data
    });
})
.WithName("GetTransactionsByCustomer");
app.MapPost("/api/transactions", async (CreateTransactionCommand command, IMediator mediator) =>
{
    var result = await mediator.Send(command);

    return Results.Ok(new
    {
        Message = $"Uspješno kreirana transakcija ID {result.Id} u {result.ExecutionTimeMs} milisekundi.",
        ExecutionTimeMs = result.ExecutionTimeMs,
        TransactionId = result.Id
    });
})
.WithName("CreateTransaction");
app.MapGet("/api/transactions/customer/{customerId}/status/{status}", async (Guid customerId, string status, IMediator mediator) =>
{
    var query = new GetTransactionsByCustomerAndStatusQuery(customerId, status);
    var result = await mediator.Send(query);

    return Results.Ok(new
    {
        Message = $"Pronađeno {result.Data.Count} transakcija sa statusom '{status}' u {result.ExecutionTimeMs} milisekundi.",
        ExecutionTimeMs = result.ExecutionTimeMs,
        Data = result.Data
    });
})
.WithName("GetTransactionsByCustomerAndStatus");
app.Run();