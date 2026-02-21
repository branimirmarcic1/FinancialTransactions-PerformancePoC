using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities;

public class Transaction
{
    public long Id { get; set; } // Sekvencijalni PK (odlično za Clustered Index)
    public Guid CustomerId { get; set; } // Po ovome ćemo najviše pretraživati
    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; }
    public string Status { get; set; } // Npr. "Completed", "Pending", "Failed"
    public string Description { get; set; }
}