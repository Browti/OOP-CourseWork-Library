using LibraryManagement.Core.Models;
using LibraryManagement.Core.Services;

namespace LibraryManagement.UI.Output;

/// <summary>
/// Виведення сутностей бібліотеки у вигляді таблиць.
/// </summary>
internal static class TablePrinter
{
    private const string DateFormat = "dd.MM.yyyy";

    public static void PrintAuthors(LibraryService service, IEnumerable<Author> authors)
    {
        ConsoleHelper.PrintTable(
            new[] { "Код", "Автор", "Країна", "Рік нар.", "Книг" },
            new[] { 4, 26, 20, 8, 5 },
            authors.Select(a => new[]
            {
                a.Id.ToString(),
                a.FullName,
                a.Country,
                a.BirthYear.ToString(),
                service.CountBooksByAuthor(a.Id).ToString()
            }));
    }

    public static void PrintBooks(LibraryService service, IEnumerable<Book> books)
    {
        ConsoleHelper.PrintTable(
            new[] { "Код", "Назва", "Автор", "Жанр", "Рік", "ISBN", "Наявно" },
            new[] { 4, 28, 18, 13, 4, 13, 7 },
            books.Select(b => new[]
            {
                b.Id.ToString(),
                b.Title,
                service.GetAuthorName(b.AuthorId),
                b.Genre.ToUkrainian(),
                b.PublicationYear.ToString(),
                b.Isbn,
                $"{b.AvailableCopies}/{b.TotalCopies}"
            }));
    }

    public static void PrintReaders(LibraryService service, IEnumerable<Reader> readers)
    {
        ConsoleHelper.PrintTable(
            new[] { "Код", "Читач", "Телефон", "Email", "Книг", "Реєстрація" },
            new[] { 4, 24, 13, 26, 5, 10 },
            readers.Select(r => new[]
            {
                r.Id.ToString(),
                r.FullName,
                r.Phone,
                r.Email,
                $"{service.CountActiveLoans(r.Id)}/{r.BookLimit}",
                r.RegistrationDate.ToString(DateFormat)
            }));
    }

    /// <summary>Таблиця видач; для неповернених книг штраф рахується на дату today.</summary>
    public static void PrintLoans(LibraryService service, IEnumerable<LoanRecord> loans, DateTime today)
    {
        ConsoleHelper.PrintTable(
            new[] { "№", "Книга", "Читач", "Видано", "Термін", "Повернено", "Прострочка", "Штраф, грн" },
            new[] { 4, 22, 18, 10, 10, 10, 10, 10 },
            loans.Select(l => new[]
            {
                l.Id.ToString(),
                service.GetBookTitle(l.BookId),
                service.GetReaderName(l.ReaderId),
                l.LoanDate.ToString(DateFormat),
                l.DueDate.ToString(DateFormat),
                l.ReturnDate?.ToString(DateFormat) ?? "на руках",
                l.GetOverdueDays(today) > 0 ? $"{l.GetOverdueDays(today)} дн." : "-",
                service.GetFine(l, today).ToString("0.00")
            }));
    }
}
