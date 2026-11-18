namespace LibraryManagement.Core.Exceptions;

/// <summary>
/// Виникає, коли запис із вказаним кодом не знайдено в репозиторії.
/// </summary>
public class EntityNotFoundException : LibraryException
{
    /// <summary>Назва сутності (у знахідному відмінку, напр. "Книгу").</summary>
    public string EntityName { get; }

    /// <summary>Код, за яким виконувався пошук.</summary>
    public int EntityId { get; }

    public EntityNotFoundException(string entityName, int entityId)
        : base($"{entityName} з кодом {entityId} не знайдено.")
    {
        EntityName = entityName;
        EntityId = entityId;
    }
}
