using LibraryManagement.Core.Models;

namespace LibraryManagement.Core.Storage;

/// <summary>
/// Контейнер для серіалізації: усі колекції бібліотеки в одному об'єкті.
/// </summary>
public class LibraryData
{
    public List<Author> Authors { get; set; } = new();

    public List<Book> Books { get; set; } = new();

    public List<Reader> Readers { get; set; } = new();

    public List<LoanRecord> Loans { get; set; } = new();

    /// <summary>Дата та час останнього збереження.</summary>
    public DateTime SavedAt { get; set; }
}
