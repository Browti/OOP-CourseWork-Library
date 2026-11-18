using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using LibraryManagement.Core.Exceptions;
using LibraryManagement.Core.Interfaces;

namespace LibraryManagement.Core.Storage;

/// <summary>
/// Збереження та завантаження даних через серіалізацію в JSON (System.Text.Json).
/// </summary>
public class JsonDataStorage : IDataStorage
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _options;

    public JsonDataStorage(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("Шлях до файлу даних не задано.");

        _filePath = filePath;
        _options = new JsonSerializerOptions
        {
            WriteIndented = true,                                   // читабельний відформатований JSON
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),  // кирилиця без \u-послідовностей
            Converters = { new JsonStringEnumConverter() }          // жанр зберігається як текст
        };
    }

    /// <summary>Повний шлях до файлу даних.</summary>
    public string FilePath => _filePath;

    public bool DataExists => File.Exists(_filePath);

    public void Save(LibraryData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        try
        {
            string json = JsonSerializer.Serialize(data, _options);
            File.WriteAllText(_filePath, json);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            throw new DataStorageException($"Не вдалося зберегти дані у файл \"{_filePath}\".", ex);
        }
    }

    public LibraryData Load()
    {
        if (!DataExists)
            return new LibraryData();

        try
        {
            string json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<LibraryData>(json, _options) ?? new LibraryData();
        }
        catch (JsonException ex)
        {
            throw new DataStorageException("Файл даних пошкоджено або він має неправильний формат.", ex);
        }
        catch (ArgumentException ex)
        {
            // Спрацювала валідація у setter-і однієї з моделей
            throw new DataStorageException($"Файл даних містить некоректні значення: {ex.Message}", ex);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new DataStorageException($"Не вдалося прочитати файл \"{_filePath}\".", ex);
        }
    }

    /// <summary>
    /// Зробити резервну копію файлу (наприклад, пошкодженого), щоб не втратити його при збереженні.
    /// Повертає шлях до копії.
    /// </summary>
    public string CreateBackup()
    {
        string backupPath = $"{_filePath}.{DateTime.Now:yyyyMMdd_HHmmss}.bak";
        File.Copy(_filePath, backupPath, overwrite: true);
        return backupPath;
    }
}
