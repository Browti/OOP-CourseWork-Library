using LibraryManagement.Core.Models;

namespace LibraryManagement.Core.Interfaces;

/// <summary>
/// Узагальнений інтерфейс сховища сутностей (елементарні операції з даними).
/// </summary>
/// <typeparam name="T">Тип сутності, похідний від EntityBase.</typeparam>
public interface IRepository<T> where T : EntityBase
{
    /// <summary>Кількість записів.</summary>
    int Count { get; }

    /// <summary>Індексатор: отримати запис за кодом (EntityNotFoundException, якщо не знайдено).</summary>
    T this[int id] { get; }

    /// <summary>Додати запис (код призначається автоматично).</summary>
    void Add(T item);

    /// <summary>Видалити запис за кодом.</summary>
    bool Remove(int id);

    /// <summary>Знайти запис за кодом або повернути null.</summary>
    T? GetById(int id);

    /// <summary>Усі записи (лише для читання).</summary>
    IReadOnlyList<T> GetAll();

    /// <summary>Усі записи, що задовольняють умову.</summary>
    List<T> Find(Predicate<T> match);

    /// <summary>Чи існує хоча б один запис, що задовольняє умову.</summary>
    bool Exists(Predicate<T> match);

    /// <summary>Замінити вміст сховища даними, завантаженими з файлу.</summary>
    void Load(IEnumerable<T> items);
}
