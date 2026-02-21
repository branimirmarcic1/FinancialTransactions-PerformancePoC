using Application.Interfaces;
using Domain.Entities;
using MediatR;
using System.Diagnostics;

namespace Application.Commands;

public class CommandResult<T>
{
    public T Id { get; set; }
    public long ExecutionTimeMs { get; set; }
}

public record CreateTransactionCommand(Guid CustomerId, decimal Amount, string Description) : IRequest<CommandResult<long>>;

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

        _context.Transactions.Add(transaction);

        await _context.SaveChangesAsync(cancellationToken);

        stopwatch.Stop();

        return new CommandResult<long>
        {
            Id = transaction.Id,
            ExecutionTimeMs = stopwatch.ElapsedMilliseconds
        };
    }
}