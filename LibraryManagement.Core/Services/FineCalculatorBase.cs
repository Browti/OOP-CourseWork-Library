using LibraryManagement.Core.Interfaces;
using LibraryManagement.Core.Models;

namespace LibraryManagement.Core.Services;

/// <summary>
/// Абстрактний клас розрахунку штрафу (шаблонний метод).
/// Спільна частина - визначення кількості днів прострочення,
/// а сама формула задається в похідному класі.
/// </summary>
public abstract class FineCalculatorBase : IFineCalculator
{
    public abstract string Description { get; }

    public decimal CalculateFine(LoanRecord loan, DateTime returnDate)
    {
        ArgumentNullException.ThrowIfNull(loan);

        int overdueDays = (returnDate.Date - loan.DueDate).Days;
        return overdueDays <= 0 ? 0m : CalculateForDays(overdueDays);
    }

    /// <summary>Розрахунок суми штрафу за кількість днів прострочення (overdueDays &gt; 0).</summary>
    protected abstract decimal CalculateForDays(int overdueDays);
}
