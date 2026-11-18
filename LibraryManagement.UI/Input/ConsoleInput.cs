using System.Globalization;
using LibraryManagement.UI.Output;

namespace LibraryManagement.UI.Input;

/// <summary>
/// Методи введення даних з обов'язковою перевіркою коректності.
/// Кожен метод повторює запит, доки користувач не введе правильне значення.
/// Введення "!" скасовує поточну операцію (OperationCanceledException).
/// </summary>
internal static class ConsoleInput
{
    public const string CancelCommand = "!";
    private const string DateFormat = "dd.MM.yyyy";

    /// <summary>Обов'язковий непорожній рядок.</summary>
    public static string ReadRequiredString(string prompt, int maxLength = 100)
    {
        while (true)
        {
            string input = ReadRaw(prompt);

            if (input.Length == 0)
            {
                ConsoleHelper.WriteError("Значення не може бути порожнім. Спробуйте ще раз.");
                continue;
            }

            if (input.Length > maxLength)
            {
                ConsoleHelper.WriteError($"Занадто довге значення (максимум {maxLength} символів).");
                continue;
            }

            return input;
        }
    }

    /// <summary>Рядок, що проходить перевірку isValid (ім'я, телефон, email, ISBN тощо).</summary>
    public static string ReadValidated(string prompt, Func<string, bool> isValid, string errorMessage)
    {
        while (true)
        {
            string input = ReadRaw(prompt);
            if (isValid(input))
                return input;

            ConsoleHelper.WriteError(errorMessage);
        }
    }

    /// <summary>Як ReadValidated, але Enter залишає поточне значення.</summary>
    public static string ReadValidatedOrDefault(string prompt, Func<string, bool> isValid,
                                                string errorMessage, string defaultValue)
    {
        while (true)
        {
            string input = ReadRaw(prompt);
            if (input.Length == 0)
                return defaultValue;
            if (isValid(input))
                return input;

            ConsoleHelper.WriteError(errorMessage);
        }
    }

    /// <summary>Рядок, де Enter залишає поточне значення.</summary>
    public static string ReadOptionalString(string prompt, string defaultValue)
    {
        string input = ReadRaw(prompt);
        return input.Length == 0 ? defaultValue : input;
    }

    /// <summary>Ціле число в діапазоні [min; max].</summary>
    public static int ReadInt(string prompt, int min, int max)
    {
        while (true)
        {
            string input = ReadRaw($"{prompt} [{min}..{max}]: ");
            if (TryParseInRange(input, min, max, out int value))
                return value;
        }
    }

    /// <summary>Ціле число в діапазоні [min; max]; Enter - значення за замовчуванням.</summary>
    public static int ReadIntOrDefault(string prompt, int min, int max, int defaultValue)
    {
        while (true)
        {
            string input = ReadRaw($"{prompt} [{min}..{max}, Enter - {defaultValue}]: ");
            if (input.Length == 0)
                return defaultValue;
            if (TryParseInRange(input, min, max, out int value))
                return value;
        }
    }

    /// <summary>Дата у форматі дд.мм.рррр; Enter - сьогодні.</summary>
    public static DateTime ReadDateOrToday(string prompt, bool allowFuture = false)
    {
        while (true)
        {
            string input = ReadRaw($"{prompt} (дд.мм.рррр, Enter - сьогодні {DateTime.Today.ToString(DateFormat)}): ");
            if (input.Length == 0)
                return DateTime.Today;

            if (!DateTime.TryParseExact(input, DateFormat, CultureInfo.InvariantCulture,
                                        DateTimeStyles.None, out DateTime date))
            {
                ConsoleHelper.WriteError($"Невірний формат дати. Приклад: {DateTime.Today.ToString(DateFormat)}");
                continue;
            }

            if (!allowFuture && date.Date > DateTime.Today)
            {
                ConsoleHelper.WriteError("Дата не може бути в майбутньому.");
                continue;
            }

            if (date.Year < 2000)
            {
                ConsoleHelper.WriteError("Дата має бути не раніше 2000 року.");
                continue;
            }

            return date.Date;
        }
    }

    /// <summary>Вибір значення перерахування (enum) зі списку.</summary>
    public static TEnum ReadEnum<TEnum>(string prompt, Func<TEnum, string> displayName)
        where TEnum : struct, Enum
    {
        TEnum[] values = Enum.GetValues<TEnum>();
        for (int i = 0; i < values.Length; i++)
            Console.WriteLine($"  {i + 1}. {displayName(values[i])}");

        int index = ReadInt(prompt, 1, values.Length);
        return values[index - 1];
    }

    /// <summary>Відповідь "так/ні".</summary>
    public static bool ReadYesNo(string prompt)
    {
        while (true)
        {
            string input = ReadRaw($"{prompt} (т/н): ").ToLowerInvariant();
            if (input is "т" or "так" or "y" or "yes")
                return true;
            if (input is "н" or "ні" or "n" or "no")
                return false;

            ConsoleHelper.WriteError("Введіть \"т\" (так) або \"н\" (ні).");
        }
    }

    private static bool TryParseInRange(string input, int min, int max, out int value)
    {
        if (!int.TryParse(input, out value))
        {
            ConsoleHelper.WriteError("Потрібно ввести ціле число.");
            return false;
        }

        if (value < min || value > max)
        {
            ConsoleHelper.WriteError($"Число має бути в межах від {min} до {max}.");
            return false;
        }

        return true;
    }

    /// <summary>Зчитати рядок; "!" або кінець потоку - скасування операції.</summary>
    private static string ReadRaw(string prompt)
    {
        Console.Write(prompt);
        string? line = Console.ReadLine();

        if (line is null)
            throw new OperationCanceledException();

        string input = line.Trim();
        if (input == CancelCommand)
            throw new OperationCanceledException();

        return input;
    }
}
