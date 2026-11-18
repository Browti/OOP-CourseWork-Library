namespace LibraryManagement.Core.Exceptions;

/// <summary>
/// Виникає при спробі видалити запис, який використовується іншими
/// (автор, у якого є книги; книга або читач з активною видачею).
/// </summary>
public class EntityInUseException : LibraryException
{
    public EntityInUseException(string message)
        : base(message)
    {
    }
}
