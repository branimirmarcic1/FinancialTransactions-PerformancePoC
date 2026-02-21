// Application/Queries/GetTransactionsByCustomerQuery.cs
using Application.Interfaces; // <-- Koristimo naš interface
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Application.Queries;

public class TransactionDto
{
    public long Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; }
    public string Status { get; set; }
}

public class QueryResult<T>
{
    public T Data { get; set; }
    public long ExecutionTimeMs { get; set; }
}

public record GetTransactionsByCustomerQuery(Guid CustomerId) : IRequest<QueryResult<List<TransactionDto>>>;

public class GetTransactionsByCustomerHandler : IRequestHandler<GetTransactionsByCustomerQuery, QueryResult<List<TransactionDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetTransactionsByCustomerHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<QueryResult<List<TransactionDto>>> Handle(GetTransactionsByCustomerQuery request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        var transactions = await _context.Transactions
            .AsNoTracking()
            .Where(t => t.CustomerId == request.CustomerId)
            .Select(t => new TransactionDto
            {
                Id = t.Id,
                Amount = t.Amount,
                TransactionDate = t.TransactionDate,
                Status = t.Status
            })
            .ToListAsync(cancellationToken);

        stopwatch.Stop();

        return new QueryResult<List<TransactionDto>>
        {
            Data = transactions,
            ExecutionTimeMs = stopwatch.ElapsedMilliseconds
        };
    }
}