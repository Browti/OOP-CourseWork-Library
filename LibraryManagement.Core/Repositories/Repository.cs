using LibraryManagement.Core.Exceptions;
using LibraryManagement.Core.Interfaces;
using LibraryManagement.Core.Models;

namespace LibraryManagement.Core.Repositories;

/// <summary>
/// Узагальнений репозиторій. Дані зберігаються в узагальненій колекції List&lt;T&gt;.
/// </summary>
/// <typeparam name="T">Тип сутності.</typeparam>
public class Repository<T> : IRepository<T> where T : EntityBase
{
    private readonly List<T> _items = new();
    private readonly string _entityName;

    /// <param name="entityName">Назва сутності для повідомлень про помилки (напр. "Книгу").</param>
    public Repository(string entityName)
    {
        _entityName = entityName;
    }

    public int Count => _items.Count;

    public T this[int id] => GetById(id) ?? throw new EntityNotFoundException(_entityName, id);

    public void Add(T item)
    {
        ArgumentNullException.ThrowIfNull(item);

        // Автоінкремент: новий код = найбільший існуючий + 1
        item.Id = _items.Count == 0 ? 1 : _items.Max(x => x.Id) + 1;
        _items.Add(item);
    }

    public bool Remove(int id)
    {
        T? item = GetById(id);
        return item != null && _items.Remove(item);
    }

    public T? GetById(int id) => _items.Find(x => x.Id == id);

    public IReadOnlyList<T> GetAll() => _items.AsReadOnly();

    public List<T> Find(Predicate<T> match) => _items.FindAll(match);

    public bool Exists(Predicate<T> match) => _items.Exists(match);

    public void Load(IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        _items.Clear();
        _items.AddRange(items);
    }
}
