using LibraryManagement.Core.Models;
using LibraryManagement.Core.Services;
using LibraryManagement.UI.Input;
using LibraryManagement.UI.Output;

namespace LibraryManagement.UI.Menus;

/// <summary>
/// Головне меню застосування.
/// </summary>
internal sealed class MainMenu : MenuBase
{
    private readonly string _dataFilePath;

    public MainMenu(LibraryService service, string dataFilePath)
        : base(service)
    {
        _dataFilePath = dataFilePath;

        AddSubmenu("1", "Автори", () => new AuthorsMenu(Service));
        AddSubmenu("2", "Каталог книг (пошук, сортування, редагування)", () => new BooksMenu(Service));
        AddSubmenu("3", "Читачі", () => new ReadersMenu(Service));
        AddSubmenu("4", "Видача та повернення книг", () => new LoansMenu(Service));
        AddItem("5", "Статистика бібліотеки", ShowStatistics);
        AddItem("6", "Зберегти дані у файл", SaveData);
        AddItem("7", "Заповнити демонстраційними даними", SeedDemoData);
    }

    protected override string Title => "БІБЛІОТЕКА ТА АБОНЕМЕНТ КНИГ - ГОЛОВНЕ МЕНЮ";

    protected override string ExitText => "Вихід (зі збереженням даних)";

    private void ShowStatistics()
    {
        LibraryStatistics stats = Service.GetStatistics(DateTime.Today);

        Console.WriteLine($"Авторів:                     {stats.AuthorCount}");
        Console.WriteLine($"Найменувань книг:            {stats.BookTitleCount}");
        Console.WriteLine($"Усього примірників:          {stats.TotalCopies}");
        Console.WriteLine($"  з них на руках у читачів:  {stats.CopiesOnLoan}");
        Console.WriteLine($"Читачів:                     {stats.ReaderCount}");
        Console.WriteLine($"Активних видач:              {stats.ActiveLoanCount}");
        Console.WriteLine($"  з них прострочених:        {stats.OverdueLoanCount}");
        Console.WriteLine($"Сплачено штрафів:            {stats.CollectedFines:0.00} грн");
        Console.WriteLine($"Очікувані штрафи (сьогодні): {stats.ExpectedFines:0.00} грн");
    }

    private void SaveData()
    {
        Service.SaveData();
        ConsoleHelper.WriteSuccess($"Дані збережено у файл:\n{_dataFilePath}");
    }

    private void SeedDemoData()
    {
        if (!ConsoleInput.ReadYesNo("Додати демонстраційних авторів, книги, читачів і видачі?"))
            return;

        DemoDataSeeder.Seed(Service);
        ConsoleHelper.WriteSuccess("Демонстраційні дані додано.");
        ShowStatistics();
    }
}
