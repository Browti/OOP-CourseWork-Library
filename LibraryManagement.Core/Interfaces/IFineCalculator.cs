using LibraryManagement.Core.Models;

namespace LibraryManagement.Core.Interfaces;

/// <summary>
/// Стратегія розрахунку штрафу за прострочене повернення книги.
/// </summary>
public interface IFineCalculator
{
    /// <summary>Текстовий опис тарифу.</summary>
    string Description { get; }

    /// <summary>Розрахувати штраф для запису видачі, якщо книгу повертають у дату returnDate.</summary>
    decimal CalculateFine(LoanRecord loan, DateTime returnDate);
}
