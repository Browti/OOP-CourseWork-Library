using LibraryManagement.Core.Models;
using LibraryManagement.Core.Services;
using LibraryManagement.Core.Validation;
using LibraryManagement.UI.Input;
using LibraryManagement.UI.Output;

namespace LibraryManagement.UI.Menus;

/// <summary>
/// Меню роботи з читачами (абонементами).
/// </summary>
internal sealed class ReadersMenu : MenuBase
{
    private const string NameError = "Дозволені лише літери, пробіл, дефіс та апостроф (до 50 символів).";
    private const string PhoneError = "Телефон має бути у форматі +380XXXXXXXXX, наприклад +380501234567.";
    private const string EmailError = "Email має бути у форматі name@domain.com.";

    public ReadersMenu(LibraryService service)
        : base(service)
    {
        AddItem("1", "Список читачів", ShowReaders);
        AddItem("2", "Зареєструвати читача", AddReader);
        AddItem("3", "Редагувати дані читача", EditReader);
        AddItem("4", "Видалити читача", RemoveReader);
        AddItem("5", "Формуляр читача (історія видач і штрафи)", ShowReaderCard);
    }

    protected override string Title => "ЧИТАЧІ";

    private void ShowReaders() => TablePrinter.PrintReaders(Service, Service.GetReadersSortedByName());

    private void AddReader()
    {
        string firstName = ConsoleInput.ReadValidated("Ім'я: ", DataValidator.IsValidName, NameError);
        string lastName = ConsoleInput.ReadValidated("Прізвище: ", DataValidator.IsValidName, NameError);
        string phone = ConsoleInput.ReadValidated("Телефон (+380XXXXXXXXX): ", DataValidator.IsValidPhone, PhoneError);
        string email = ConsoleInput.ReadValidated("Email: ", DataValidator.IsValidEmail, EmailError);
        int limit = ConsoleInput.ReadIntOrDefault("Ліміт книг на абонементі", Reader.MinBookLimit,
                                                  Reader.MaxBookLimit, Reader.DefaultBookLimit);

        Reader reader = Service.AddReader(firstName, lastName, phone, email, limit);
        ConsoleHelper.WriteSuccess($"\nЧитача зареєстровано: {reader.GetShortInfo()}, ліміт книг: {reader.BookLimit}");
    }

    private void EditReader()
    {
        Reader reader = SelectReader(Service.GetReadersSortedByName());

        Console.WriteLine($"\nРедагування читача {reader.FullName}.");
        ConsoleHelper.WriteHint("Натисніть Enter, щоб залишити поточне значення.");

        string phone = ConsoleInput.ReadValidatedOrDefault($"Телефон [{reader.Phone}]: ",
                                                           DataValidator.IsValidPhone, PhoneError, reader.Phone);
        string email = ConsoleInput.ReadValidatedOrDefault($"Email [{reader.Email}]: ",
                                                           DataValidator.IsValidEmail, EmailError, reader.Email);
        int limit = ConsoleInput.ReadIntOrDefault("Ліміт книг", Reader.MinBookLimit, Reader.MaxBookLimit,
                                                  reader.BookLimit);

        Service.UpdateReader(reader.Id, phone, email, limit);

        ConsoleHelper.WriteSuccess("\nДані читача оновлено:");
        TablePrinter.PrintReaders(Service, new[] { Service.GetReader(reader.Id) });
    }

    private void RemoveReader()
    {
        Reader reader = SelectReader(Service.GetReadersSortedByName());
        if (!ConsoleInput.ReadYesNo($"Видалити читача {reader.FullName}?"))
            return;

        Service.RemoveReader(reader.Id);
        ConsoleHelper.WriteSuccess("Читача видалено.");
    }

    private void ShowReaderCard()
    {
        Reader reader = SelectReader(Service.GetReadersSortedByName());
        List<LoanRecord> loans = Service.GetReaderLoans(reader.Id);
        DateTime today = DateTime.Today;

        Console.WriteLine();
        ConsoleHelper.WriteHeader($"Формуляр: {reader.FullName}");
        Console.WriteLine($"Телефон: {reader.Phone}   Email: {reader.Email}");
        Console.WriteLine($"Книг на руках: {Service.CountActiveLoans(reader.Id)} з {reader.BookLimit}");
        Console.WriteLine();

        TablePrinter.PrintLoans(Service, loans, today);

        decimal paid = loans.Where(l => l.IsReturned).Sum(l => l.FineAmount);
        decimal current = loans.Where(l => !l.IsReturned).Sum(l => Service.GetFine(l, today));
        Console.WriteLine($"Сплачено штрафів: {paid:0.00} грн");

        if (current > 0)
            ConsoleHelper.WriteWarning($"Поточний борг за простроченими книгами: {current:0.00} грн");
        else
            ConsoleHelper.WriteSuccess("Прострочених книг немає.");
    }
}
