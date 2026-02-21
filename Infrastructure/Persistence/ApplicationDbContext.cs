using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Transaction>(entity =>
        {
            // Osnovna konfiguracija tablice
            entity.ToTable("Transactions");
            entity.HasKey(e => e.Id); // EF Core automatski kreira Clustered Index na PK

            // HasColumnType sada radi jer imamo Microsoft.EntityFrameworkCore.SqlServer paket
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.Description).HasMaxLength(255);

            /* =========================================================
               PoC INDEXING SCENARIJI (Za testiranje performansi)
               ========================================================= */

            // SCENARIJ 1: Obični Non-Clustered Index (Uzrokuje Key Lookup ako tražimo Amount/Date)
            entity.HasIndex(e => e.CustomerId)
                  .HasDatabaseName("IX_Transactions_CustomerId");

            // SCENARIJ 2: Covering Index (Uključuje Amount i Date kako bi izbjegao Key Lookup)
            entity.HasIndex(e => e.CustomerId)
                  .IncludeProperties(e => new { e.Amount, e.TransactionDate })
                  .HasDatabaseName("IX_Transactions_Covering_CustomerId");

            // SCENARIJ 3: Composite Index (Bitnost redoslijeda: Status vs CustomerId)
            entity.HasIndex(e => new { e.CustomerId, e.Status })
                  .HasDatabaseName("IX_Transactions_Customer_Status");
        });
    }
}