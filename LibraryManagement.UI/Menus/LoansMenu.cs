using LibraryManagement.Core.Models;
using LibraryManagement.Core.Services;
using LibraryManagement.UI.Input;
using LibraryManagement.UI.Output;

namespace LibraryManagement.UI.Menus;

/// <summary>
/// Меню видачі та повернення книг, розрахунку штрафів.
/// </summary>
internal sealed class LoansMenu : MenuBase
{
    public LoansMenu(LibraryService service)
        : base(service)
    {
        AddItem("1", "Видати книгу читачу", IssueBook);
        AddItem("2", "Повернути книгу", ReturnBook);
        AddItem("3", "Книги на руках у читачів", ShowActiveLoans);
        AddItem("4", "Боржники (прострочені книги)", ShowOverdueLoans);
        AddItem("5", "Історія всіх видач", ShowAllLoans);
        AddItem("6", "Розрахувати штраф на дату", PreviewFine);
    }

    protected override string Title => "ВИДАЧА ТА ПОВЕРНЕННЯ КНИГ";

    private void IssueBook()
    {
        Reader reader = SelectReader(Service.GetReadersSortedByName());

        // Перевіряємо ліміт одразу, щоб не вибирати книгу даремно
        Service.EnsureReaderCanBorrow(reader.Id);
        Console.WriteLine($"\nУ читача на руках {Service.CountActiveLoans(reader.Id)} з {reader.BookLimit} книг.\n");

        Book book = SelectBook(Service.GetBooksSortedByTitle());
        DateTime loanDate = ConsoleInput.ReadDateOrToday("Дата видачі");
        int days = ConsoleInput.ReadIntOrDefault("Термін користування, днів", LibraryService.MinLoanDays,
                                                 LibraryService.MaxLoanDays, LibraryService.DefaultLoanDays);

        LoanRecord loan = Service.IssueBook(reader.Id, book.Id, loanDate, days);

        ConsoleHelper.WriteSuccess($"\nКнигу \"{book.Title}\" видано читачу {reader.FullName}.");
        ConsoleHelper.WriteSuccess($"Запис №{loan.Id}. Повернути до {loan.DueDate:dd.MM.yyyy}.");
        if (loan.IsOverdue(DateTime.Today))
            ConsoleHelper.WriteWarning("Увага: термін повернення вже минув (видача оформлена заднім числом).");
    }

    private void ReturnBook()
    {
        LoanRecord loan = SelectLoan(Service.GetActiveLoans());
        DateTime returnDate = ConsoleInput.ReadDateOrToday("Дата повернення");

        decimal fine = Service.ReturnBook(loan.Id, returnDate);
        int overdueDays = loan.GetOverdueDays(returnDate);

        Console.WriteLine();
        if (fine > 0)
        {
            ConsoleHelper.WriteWarning($"Книгу \"{Service.GetBookTitle(loan.BookId)}\" повернуто із запізненням на {overdueDays} дн.");
            ConsoleHelper.WriteWarning($"Нараховано штраф: {fine:0.00} грн");
            ConsoleHelper.WriteHint($"Тариф: {Service.FineCalculator.Description}");
        }
        else
        {
            ConsoleHelper.WriteSuccess($"Книгу \"{Service.GetBookTitle(loan.BookId)}\" повернуто вчасно. Штраф не нараховано.");
        }
    }

    private void ShowActiveLoans() => TablePrinter.PrintLoans(Service, Service.GetActiveLoans(), DateTime.Today);

    private void ShowOverdueLoans()
    {
        DateTime today = DateTime.Today;
        List<LoanRecord> overdue = Service.GetOverdueLoans(today);

        Console.WriteLine($"Прострочені книги станом на {today:dd.MM.yyyy}:");
        TablePrinter.PrintLoans(Service, overdue, today);

        decimal total = overdue.Sum(l => Service.GetFine(l, today));
        ConsoleHelper.WriteWarning($"Боржників: {overdue.Select(l => l.ReaderId).Distinct().Count()}, " +
                                   $"сума штрафів на сьогодні: {total:0.00} грн");
        ConsoleHelper.WriteHint($"Тариф: {Service.FineCalculator.Description}");
    }

    private void ShowAllLoans() => TablePrinter.PrintLoans(Service, Service.GetAllLoans(), DateTime.Today);

    private void PreviewFine()
    {
        LoanRecord loan = SelectLoan(Service.GetActiveLoans());
        DateTime date = ConsoleInput.ReadDateOrToday("Дата можливого повернення", allowFuture: true);

        decimal fine = Service.PreviewFine(loan.Id, date);
        int overdueDays = loan.GetOverdueDays(date);

        Console.WriteLine($"\nТермін повернення: {loan.DueDate:dd.MM.yyyy}");
        Console.WriteLine($"Якщо повернути {date:dd.MM.yyyy}: прострочення {overdueDays} дн., штраф {fine:0.00} грн");
        ConsoleHelper.WriteHint($"Тариф: {Service.FineCalculator.Description}");
    }
}
