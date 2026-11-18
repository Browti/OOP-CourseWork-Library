namespace LibraryManagement.Core.Models;

/// <summary>
/// Зведена статистика бібліотеки (незмінний record-тип, лише властивості для читання).
/// </summary>
public sealed record LibraryStatistics(
    int AuthorCount,
    int BookTitleCount,
    int TotalCopies,
    int CopiesOnLoan,
    int ReaderCount,
    int ActiveLoanCount,
    int OverdueLoanCount,
    decimal CollectedFines,
    decimal ExpectedFines);
