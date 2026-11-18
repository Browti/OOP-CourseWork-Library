namespace LibraryManagement.Core.Exceptions;

/// <summary>
/// Виникає при спробі додати запис, що вже існує (той самий ISBN, телефон читача тощо).
/// </summary>
public class DuplicateEntityException : LibraryException
{
    public DuplicateEntityException(string message)
        : base(message)
    {
    }
}
