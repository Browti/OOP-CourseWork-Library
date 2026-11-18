namespace LibraryManagement.Core.Exceptions;

/// <summary>
/// Виникає, коли книгу неможливо видати (немає вільних примірників
/// або читач уже має примірник цієї книги).
/// </summary>
public class BookNotAvailableException : LibraryException
{
    /// <summary>Назва книги.</summary>
    public string BookTitle { get; }

    public BookNotAvailableException(string bookTitle)
        : base($"Книга \"{bookTitle}\" зараз недоступна: усі примірники видано читачам.")
    {
        BookTitle = bookTitle;
    }

    public BookNotAvailableException(string bookTitle, string message)
        : base(message)
    {
        BookTitle = bookTitle;
    }
}
