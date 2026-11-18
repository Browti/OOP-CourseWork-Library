namespace LibraryManagement.Core.Exceptions;

/// <summary>
/// Виникає, коли читач намагається взяти більше книг, ніж дозволяє його ліміт.
/// </summary>
public class MaxBooksExceededException : LibraryException
{
    /// <summary>Ім'я читача.</summary>
    public string ReaderName { get; }

    /// <summary>Ліміт книг читача.</summary>
    public int Limit { get; }

    public MaxBooksExceededException(string readerName, int limit)
        : base($"Читач {readerName} вже має на руках максимальну кількість книг ({limit}). " +
               "Щоб узяти нову, потрібно повернути хоча б одну книгу.")
    {
        ReaderName = readerName;
        Limit = limit;
    }
}
