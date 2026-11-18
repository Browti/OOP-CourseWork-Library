namespace LibraryManagement.Core.Models;

/// <summary>
/// Метод розширення для отримання української назви жанру.
/// </summary>
public static class GenreExtensions
{
    public static string ToUkrainian(this Genre genre) => genre switch
    {
        Genre.Fiction => "Художня проза",
        Genre.Fantasy => "Фентезі",
        Genre.Detective => "Детектив",
        Genre.ScienceFiction => "Фантастика",
        Genre.History => "Історична",
        Genre.Science => "Наукова",
        Genre.Poetry => "Поезія",
        Genre.Children => "Дитяча",
        _ => "Інше"
    };
}
