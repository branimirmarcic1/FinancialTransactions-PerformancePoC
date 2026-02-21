using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Application.Queries;

// Nova komanda koja prima i Status
public record GetTransactionsByCustomerAndStatusQuery(Guid CustomerId, string Status) : IRequest<QueryResult<List<TransactionDto>>>;

public class GetTransactionsByCustomerAndStatusHandler : IRequestHandler<GetTransactionsByCustomerAndStatusQuery, QueryResult<List<TransactionDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetTransactionsByCustomerAndStatusHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<QueryResult<List<TransactionDto>>> Handle(GetTransactionsByCustomerAndStatusQuery request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        var transactions = await _context.Transactions
            .AsNoTracking()
            .Where(t => t.CustomerId == request.CustomerId && t.Status == request.Status)
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