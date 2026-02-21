# FinancialTransactions-PerformancePoC
Proof of Concept demonstrating advanced SQL Server indexing strategies, performance tuning, and CQRS in .NET Core.
## 📊 The Benchmarks (The "Why")

I seeded the database with **1,000,000 dummy transactions** using `SqlBulkCopy` and measured the execution time of fetching records for a specific `CustomerId` (approx. 10,000 records per customer). 

Here is the evolution of the query optimization:

| Scenario | Read Time | Write Time (1 Insert) | SQL Operation | Explanation |
| :--- | :--- | :--- | :--- | :--- |
| **1. No Index** | ~6000 ms | ~15 ms | Table Scan | DB reads all 1M rows. Unacceptable read performance. |
| **2. Non-Clustered Index** | ~2005 ms | ~40 ms | Index Seek + Key Lookup | DB finds the customer quickly but jumps to the main table 10,000 times to fetch missing columns. |
| **3. Composite Index** | **~135 ms** | ~65 ms | Index Seek + Key Lookup | Filtering by `CustomerId` AND `Status`. Proper column order (Cardinality) drastically reduces Key Lookups from 10k to ~3k. |
| **4. Covering Index (INCLUDE)** | **~170 ms** | **~1885 ms** | Index Seek (Covered) | The ultimate read optimization. Data is served directly from the index B-Tree. No Key Lookups. **BUT look at the write penalty!** |

### ⚠️ The Conclusion & CQRS Justification
Adding a heavy Covering Index improved read speeds by **35x**, but it degraded the write (`INSERT`) performance drastically (from milliseconds to almost 2 seconds). 

This proves that indexes are not a "silver bullet". In financial data processing, where both high-speed ingestion and complex reporting are required, we must separate these concerns. This is exactly why this PoC implements the **CQRS pattern** using `MediatR`—allowing us to scale and optimize Read models and Write models independently.
