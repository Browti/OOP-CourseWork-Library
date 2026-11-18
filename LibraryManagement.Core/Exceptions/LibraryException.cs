namespace LibraryManagement.Core.Exceptions;

/// <summary>
/// Базове виключення бізнес-логіки бібліотеки.
/// Від нього успадковуються всі власні виключення проєкту.
/// </summary>
public class LibraryException : Exception
{
    public LibraryException(string message)
        : base(message)
    {
    }

    public LibraryException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
