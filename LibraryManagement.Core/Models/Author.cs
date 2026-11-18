using LibraryManagement.Core.Validation;

namespace LibraryManagement.Core.Models;

/// <summary>
/// Автор книги.
/// </summary>
public class Author : Person
{
    public const int MinBirthYear = 1000;

    private string _country = string.Empty;
    private int _birthYear = MinBirthYear;

    /// <summary>Країна автора.</summary>
    public string Country
    {
        get => _country;
        set => _country = DataValidator.RequireText(value, "Країна", 60);
    }

    /// <summary>Рік народження автора.</summary>
    public int BirthYear
    {
        get => _birthYear;
        set => _birthYear = DataValidator.RequireRange(value, MinBirthYear, MaxBirthYear, "Рік народження");
    }

    /// <summary>Найпізніший допустимий рік народження (автору має бути хоча б 10 років).</summary>
    public static int MaxBirthYear => DateTime.Today.Year - 10;

    public override string GetShortInfo() => $"[{Id}] {FullName} ({Country}, {BirthYear} р.н.)";
}
