using LibraryManagement.Core.Exceptions;
using LibraryManagement.Core.Models;
using LibraryManagement.Core.Services;
using LibraryManagement.UI.Input;
using LibraryManagement.UI.Output;

namespace LibraryManagement.UI.Menus;

/// <summary>
/// Абстрактний базовий клас для всіх меню.
/// Реалізує цикл меню та централізовану обробку виключень.
/// </summary>
internal abstract class MenuBase
{
    private readonly List<MenuItem> _items = new();

    protected MenuBase(LibraryService service)
    {
        Service = service ?? throw new ArgumentNullException(nameof(service));
    }

    /// <summary>Сервіс бізнес-логіки (з DLL-бібліотеки).</summary>
    protected LibraryService Service { get; }

    /// <summary>Заголовок меню.</summary>
    protected abstract string Title { get; }

    /// <summary>Текст пункту "0".</summary>
    protected virtual string ExitText => "Назад";

    /// <summary>Запуск циклу меню. Завершується, коли користувач обирає "0".</summary>
    public void Run()
    {
        while (true)
        {
            ConsoleHelper.Clear();
            ConsoleHelper.WriteHeader(Title);
            foreach (MenuItem item in _items)
                Console.WriteLine($"  {item.Key}. {item.Text}");
            Console.WriteLine($"  0. {ExitText}");

            Console.Write("\nВаш вибір: ");
            string? line = Console.ReadLine();
            if (line is null)
                return;

            string choice = line.Trim();
            if (choice == "0")
                return;

            MenuItem? selected = _items.Find(i => i.Key == choice);
            if (selected is null)
            {
                ConsoleHelper.WriteError("Немає такого пункту меню. Спробуйте ще раз.");
                ConsoleHelper.Pause();
                continue;
            }

            if (selected.IsSubmenu)
            {
                selected.Action();
                continue;
            }

            ConsoleHelper.Clear();
            ConsoleHelper.WriteHeader(selected.Text);
            ConsoleHelper.WriteHint($"(введіть {ConsoleInput.CancelCommand}, щоб скасувати операцію)");
            Console.WriteLine();
            ExecuteSafely(selected.Action);
            ConsoleHelper.Pause();
        }
    }

    protected void AddItem(string key, string text, Action action) =>
        _items.Add(new MenuItem(key, text, action, isSubmenu: false));

    protected void AddSubmenu(string key, string text, Func<MenuBase> createMenu) =>
        _items.Add(new MenuItem(key, text, () => createMenu().Run(), isSubmenu: true));

    /// <summary>
    /// Виконання дії з обробкою всіх виключень: власних (бізнес-логіка),
    /// помилок валідації (ArgumentException) та скасування операції.
    /// </summary>
    private static void ExecuteSafely(Action action)
    {
        try
        {
            action();
        }
        catch (MaxBooksExceededException ex)
        {
            ShowException("Перевищено ліміт книг", ex);
        }
        catch (BookNotAvailableException ex)
        {
            ShowException("Книга недоступна", ex);
        }
        catch (EntityNotFoundException ex)
        {
            ShowException("Запис не знайдено", ex);
        }
        catch (LibraryException ex)
        {
            ShowException("Помилка бізнес-логіки", ex);
        }
        catch (ArgumentException ex)
        {
            ShowException("Некоректні дані", ex);
        }
        catch (OperationCanceledException)
        {
            ConsoleHelper.WriteWarning("\nОперацію скасовано користувачем.");
        }
        catch (Exception ex)
        {
            ShowException("Непередбачена помилка", ex);
        }
    }

    private static void ShowException(string caption, Exception ex)
    {
        ConsoleHelper.WriteError($"\n{caption} [{ex.GetType().Name}]:");
        ConsoleHelper.WriteError(ex.Message);
    }

    #region Спільні методи вибору записів

    protected static int MaxId<T>(IReadOnlyList<T> items) where T : EntityBase =>
        items.Count == 0 ? 1 : items.Max(i => i.Id);

    protected Author SelectAuthor(IReadOnlyList<Author> authors)
    {
        if (authors.Count == 0)
            throw new LibraryException("Список авторів порожній.");

        TablePrinter.PrintAuthors(Service, authors);
        int id = ConsoleInput.ReadInt("Введіть код автора", 1, MaxId(authors));
        return Service.GetAuthor(id);
    }

    protected Book SelectBook(IReadOnlyList<Book> books)
    {
        if (books.Count == 0)
            throw new LibraryException("Каталог книг порожній.");

        TablePrinter.PrintBooks(Service, books);
        int id = ConsoleInput.ReadInt("Введіть код книги", 1, MaxId(books));
        return Service.GetBook(id);
    }

    protected Reader SelectReader(IReadOnlyList<Reader> readers)
    {
        if (readers.Count == 0)
            throw new LibraryException("Список читачів порожній.");

        TablePrinter.PrintReaders(Service, readers);
        int id = ConsoleInput.ReadInt("Введіть код читача", 1, MaxId(readers));
        return Service.GetReader(id);
    }

    protected LoanRecord SelectLoan(IReadOnlyList<LoanRecord> loans)
    {
        if (loans.Count == 0)
            throw new LibraryException("Немає записів видачі для вибору.");

        TablePrinter.PrintLoans(Service, loans, DateTime.Today);
        int id = ConsoleInput.ReadInt("Введіть номер запису", 1, MaxId(loans));
        return Service.GetLoan(id);
    }

    #endregion
}
