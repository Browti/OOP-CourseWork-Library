using System.Text;
using LibraryManagement.Core.Exceptions;
using LibraryManagement.Core.Services;
using LibraryManagement.Core.Storage;
using LibraryManagement.UI.Menus;
using LibraryManagement.UI.Output;

namespace LibraryManagement.UI;

/// <summary>
/// Точка входу консольного застосування "Бібліотека та абонемент книг".
/// </summary>
internal static class Program
{
    private const string DataFileName = "library_data.json";

    private static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.Unicode; // коректне введення кирилиці в консолі Windows
        Console.Title = "Бібліотека та абонемент книг";

        string dataFilePath = Path.Combine(AppContext.BaseDirectory, DataFileName);
        var storage = new JsonDataStorage(dataFilePath);
        var service = new LibraryService(storage, new StandardFineCalculator());

        LoadData(service, storage);

        // Збереження даних, якщо програму закривають комбінацією Ctrl+C
        Console.CancelKeyPress += (_, _) => SaveData(service);

        new MainMenu(service, dataFilePath).Run();

        ConsoleHelper.Clear();
        if (SaveData(service))
            ConsoleHelper.WriteSuccess($"Дані збережено у файл {dataFilePath}. До побачення!");
        else
            ConsoleHelper.Pause();
    }

    /// <summary>Завантаження даних при запуску (десеріалізація з JSON).</summary>
    private static void LoadData(LibraryService service, JsonDataStorage storage)
    {
        ConsoleHelper.WriteHeader("БІБЛІОТЕКА ТА АБОНЕМЕНТ КНИГ");

        if (!storage.DataExists)
        {
            ConsoleHelper.WriteWarning("Файл даних не знайдено - бібліотека порожня.");
            ConsoleHelper.WriteHint("Дані можна ввести вручну або заповнити демонстраційними (пункт 7 головного меню).");
            ConsoleHelper.Pause();
            return;
        }

        try
        {
            service.LoadData();
            ConsoleHelper.WriteSuccess($"Дані завантажено з файлу {storage.FilePath}");

            var stats = service.GetStatistics(DateTime.Today);
            Console.WriteLine($"Авторів: {stats.AuthorCount}, книг: {stats.BookTitleCount}, " +
                              $"читачів: {stats.ReaderCount}, активних видач: {stats.ActiveLoanCount}");
        }
        catch (DataStorageException ex)
        {
            ConsoleHelper.WriteError($"Помилка завантаження [{ex.GetType().Name}]: {ex.Message}");
            if (ex.InnerException != null)
                ConsoleHelper.WriteError($"Деталі: {ex.InnerException.Message}");

            // Пошкоджений файл копіюємо, щоб не перезаписати його при виході
            try
            {
                string backup = storage.CreateBackup();
                ConsoleHelper.WriteWarning($"Резервну копію пошкодженого файлу збережено: {backup}");
            }
            catch (Exception backupError) when (backupError is IOException or UnauthorizedAccessException)
            {
                ConsoleHelper.WriteError($"Не вдалося створити резервну копію: {backupError.Message}");
            }

            ConsoleHelper.WriteWarning("Робота продовжиться з порожньою бібліотекою.");
        }

        ConsoleHelper.Pause();
    }

    /// <summary>Збереження даних (серіалізація в JSON). Повертає true, якщо успішно.</summary>
    private static bool SaveData(LibraryService service)
    {
        try
        {
            service.SaveData();
            return true;
        }
        catch (DataStorageException ex)
        {
            ConsoleHelper.WriteError($"Помилка збереження: {ex.Message}");
            if (ex.InnerException != null)
                ConsoleHelper.WriteError($"Деталі: {ex.InnerException.Message}");
            return false;
        }
    }
}
