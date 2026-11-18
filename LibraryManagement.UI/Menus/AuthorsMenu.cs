using LibraryManagement.Core.Models;
using LibraryManagement.Core.Services;
using LibraryManagement.Core.Validation;
using LibraryManagement.UI.Input;
using LibraryManagement.UI.Output;

namespace LibraryManagement.UI.Menus;

/// <summary>
/// Меню роботи з авторами.
/// </summary>
internal sealed class AuthorsMenu : MenuBase
{
    private const string NameError = "Дозволені лише літери, пробіл, дефіс та апостроф (до 50 символів).";

    public AuthorsMenu(LibraryService service)
        : base(service)
    {
        AddItem("1", "Список авторів", ShowAuthors);
        AddItem("2", "Додати автора", AddAuthor);
        AddItem("3", "Видалити автора", RemoveAuthor);
    }

    protected override string Title => "АВТОРИ";

    private void ShowAuthors() => TablePrinter.PrintAuthors(Service, Service.GetAuthors());

    private void AddAuthor()
    {
        string firstName = ConsoleInput.ReadValidated("Ім'я: ", DataValidator.IsValidName, NameError);
        string lastName = ConsoleInput.ReadValidated("Прізвище: ", DataValidator.IsValidName, NameError);
        string country = ConsoleInput.ReadRequiredString("Країна: ", 60);
        int birthYear = ConsoleInput.ReadInt("Рік народження", Author.MinBirthYear, Author.MaxBirthYear);

        Author author = Service.AddAuthor(firstName, lastName, country, birthYear);
        ConsoleHelper.WriteSuccess($"\nАвтора додано: {author.GetShortInfo()}");
    }

    private void RemoveAuthor()
    {
        Author author = SelectAuthor(Service.GetAuthors());
        if (!ConsoleInput.ReadYesNo($"Видалити автора {author.FullName}?"))
            return;

        Service.RemoveAuthor(author.Id);
        ConsoleHelper.WriteSuccess("Автора видалено.");
    }
}
