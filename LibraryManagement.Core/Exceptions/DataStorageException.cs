namespace LibraryManagement.Core.Exceptions;

/// <summary>
/// Виникає при помилках збереження або завантаження даних з файлу.
/// </summary>
public class DataStorageException : LibraryException
{
    public DataStorageException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
