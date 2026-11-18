using System.Text.Json.Serialization;
using LibraryManagement.Core.Validation;

namespace LibraryManagement.Core.Models;

/// <summary>
/// Абстрактний клас "Особа" - спільна частина для автора та читача.
/// </summary>
public abstract class Person : EntityBase
{
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;

    /// <summary>Ім'я.</summary>
    public string FirstName
    {
        get => _firstName;
        set => _firstName = DataValidator.RequireName(value, "Ім'я");
    }

    /// <summary>Прізвище.</summary>
    public string LastName
    {
        get => _lastName;
        set => _lastName = DataValidator.RequireName(value, "Прізвище");
    }

    /// <summary>Повне ім'я у форматі "Прізвище Ім'я" (обчислювана властивість, у файл не зберігається).</summary>
    [JsonIgnore]
    public string FullName => $"{LastName} {FirstName}";

    /// <summary>Перевіряє, чи містить ім'я особи заданий фрагмент (у будь-якому порядку слів).</summary>
    public bool NameContains(string fragment) =>
        FullName.Contains(fragment, StringComparison.CurrentCultureIgnoreCase) ||
        $"{FirstName} {LastName}".Contains(fragment, StringComparison.CurrentCultureIgnoreCase);
}
