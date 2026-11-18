using LibraryManagement.Core.Exceptions;
using LibraryManagement.Core.Models;
using LibraryManagement.Core.Services;
using LibraryManagement.Core.Validation;
using LibraryManagement.UI.Input;
using LibraryManagement.UI.Output;

namespace LibraryManagement.UI.Menus;

/// <summary>
/// Меню каталогу книг: додавання, редагування, видалення, пошук і сортування.
/// </summary>
internal sealed class BooksMenu : MenuBase
{
    public BooksMenu(LibraryService service)
        : base(service)
    {
        AddItem("1", "Усі книги (за назвою)", ShowAllBooks);
        AddItem("2", "Додати книгу", AddBook);
        AddItem("3", "Редагувати книгу", EditBook);
        AddItem("4", "Видалити книгу", RemoveBook);
        AddItem("5", "Пошук книг за автором", SearchByAuthor);
        AddItem("6", "Пошук книг за жанром", SearchByGenre);
        AddItem("7", "Пошук книг за назвою", SearchByTitle);
        AddItem("8", "Сортування за роком видання", SortByYear);
        AddItem("9", "Книги, доступні для видачі", ShowAvailableBooks);
    }

    protected override string Title => "КАТАЛОГ КНИГ";

    private void ShowAllBooks() => TablePrinter.PrintBooks(Service, Service.GetBooksSortedByTitle());

    private void AddBook()
    {
        if (Service.GetAuthors().Count == 0)
            throw new LibraryException("Спочатку додайте хоча б одного автора (меню \"Автори\").");

        string title = ConsoleInput.ReadRequiredString("Назва книги: ", 150);

        Console.WriteLine("\nОберіть автора:");
        Author author = SelectAuthor(Service.GetAuthors());

        Console.WriteLine("\nОберіть жанр:");
        Genre genre = ConsoleInput.ReadEnum<Genre>("Номер жанру", g => g.ToUkrainian());

        int year = ConsoleInput.ReadInt("Рік видання", Book.MinYear, DateTime.Today.Year);
        string isbn = ConsoleInput.ReadValidated("ISBN (13 цифр, можна з дефісами): ",
                                                 DataValidator.IsValidIsbn, "ISBN має містити рівно 13 цифр.");
        int copies = ConsoleInput.ReadInt("Кількість примірників", Book.MinCopies, Book.MaxCopies);

        Book book = Service.AddBook(title, author.Id, genre, year, isbn, copies);
        ConsoleHelper.WriteSuccess($"\nКнигу додано до каталогу: {book.GetShortInfo()}");
    }

    private void EditBook()
    {
        Book book = SelectBook(Service.GetBooksSortedByTitle());

        Console.WriteLine($"\nРедагування книги {book.GetShortInfo()}.");
        ConsoleHelper.WriteHint("Натисніть Enter, щоб залишити поточне значення.");

        string title = ConsoleInput.ReadOptionalString($"Назва [{book.Title}]: ", book.Title);
        int year = ConsoleInput.ReadIntOrDefault("Рік видання", Book.MinYear, DateTime.Today.Year,
                                                 book.PublicationYear);
        int copies = ConsoleInput.ReadIntOrDefault($"Кількість примірників (видано читачам: {book.CopiesOnLoan})",
                                                   Book.MinCopies, Book.MaxCopies, book.TotalCopies);

        Genre genre = book.Genre;
        if (ConsoleInput.ReadYesNo($"Змінити жанр (зараз \"{book.Genre.ToUkrainian()}\")?"))
            genre = ConsoleInput.ReadEnum<Genre>("Номер жанру", g => g.ToUkrainian());

        Service.UpdateBook(book.Id, title, genre, year, copies);

        ConsoleHelper.WriteSuccess("\nЗміни збережено:");
        TablePrinter.PrintBooks(Service, new[] { Service.GetBook(book.Id) });
    }

    private void RemoveBook()
    {
        Book book = SelectBook(Service.GetBooksSortedByTitle());
        if (!ConsoleInput.ReadYesNo($"Видалити книгу \"{book.Title}\"?"))
            return;

        Service.RemoveBook(book.Id);
        ConsoleHelper.WriteSuccess("Книгу видалено з каталогу.");
    }

    private void SearchByAuthor()
    {
        string fragment = ConsoleInput.ReadRequiredString("Ім'я або прізвище автора (можна частину): ", 50);
        List<Book> result = Service.FindBooksByAuthor(fragment);

        TablePrinter.PrintBooks(Service, result);
        Console.WriteLine($"Знайдено книг: {result.Count}");
    }

    private void SearchByGenre()
    {
        Genre genre = ConsoleInput.ReadEnum<Genre>("Номер жанру", g => g.ToUkrainian());
        List<Book> result = Service.FindBooksByGenre(genre);

        Console.WriteLine($"\nКниги жанру \"{genre.ToUkrainian()}\":");
        TablePrinter.PrintBooks(Service, result);
        Console.WriteLine($"Знайдено книг: {result.Count}");
    }

    private void SearchByTitle()
    {
        string fragment = ConsoleInput.ReadRequiredString("Назва або частина назви: ", 150);
        List<Book> result = Service.FindBooksByTitle(fragment);

        TablePrinter.PrintBooks(Service, result);
        Console.WriteLine($"Знайдено книг: {result.Count}");
    }

    private void SortByYear()
    {
        bool descending = ConsoleInput.ReadYesNo("Сортувати від нових до старих?");
        Console.WriteLine(descending ? "\nВід нових до старих:" : "\nВід старих до нових:");
        TablePrinter.PrintBooks(Service, Service.GetBooksSortedByYear(descending));
    }

    private void ShowAvailableBooks() => TablePrinter.PrintBooks(Service, Service.GetAvailableBooks());
}
