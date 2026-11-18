using LibraryManagement.Core.Storage;

namespace LibraryManagement.Core.Interfaces;

/// <summary>
/// Сховище для збереження та завантаження всіх даних бібліотеки.
/// </summary>
public interface IDataStorage
{
    /// <summary>Чи існують збережені дані.</summary>
    bool DataExists { get; }

    /// <summary>Зберегти дані.</summary>
    void Save(LibraryData data);

    /// <summary>Завантажити дані (порожній набір, якщо даних немає).</summary>
    LibraryData Load();
}
