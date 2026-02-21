using Application.Interfaces;
using Domain.Entities;
using MediatR;
using System.Diagnostics;

namespace Application.Commands;

// 1. Rezultat komande
public class CommandResult<T>
{
    public T Id { get; set; }
    public long ExecutionTimeMs { get; set; }
}

// 2. CQRS Command (Request)
public record CreateTransactionCommand(Guid CustomerId, decimal Amount, string Description) : IRequest<CommandResult<long>>;

// 3. CQRS Handler
public class CreateTransactionHandler : IRequestHandler<CreateTransactionCommand, CommandResult<long>>
{
    private readonly IApplicationDbContext _context;

    public CreateTransactionHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CommandResult<long>> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = new Transaction
        {
            CustomerId = request.CustomerId,
            Amount = request.Amount,
            Description = request.Description,
            TransactionDate = DateTime.UtcNow,
            Status = "Pending"
        };

        var stopwatch = Stopwatch.StartNew();

        // Dodajemo u EF Core tracking
        _context.Transactions.Add(transaction);

        // Mjerimo isključivo koliko bazi treba da zapiše podatke i ažurira sve indekse
        await _context.SaveChangesAsync(cancellationToken);

        stopwatch.Stop();

        return new CommandResult<long>
        {
            Id = transaction.Id,
            ExecutionTimeMs = stopwatch.ElapsedMilliseconds
        };
    }
}