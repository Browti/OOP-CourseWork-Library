using System.Text.RegularExpressions;

namespace LibraryManagement.Core.Validation;

/// <summary>
/// Набір методів для перевірки коректності даних.
/// Використовується у властивостях моделей (setter) та в консольному інтерфейсі.
/// </summary>
public static class DataValidator
{
    private static readonly Regex NameRegex = new(@"^\p{L}[\p{L}'’ʼ\- ]*$", RegexOptions.Compiled);
    private static readonly Regex PhoneRegex = new(@"^\+380\d{9}$", RegexOptions.Compiled);
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$", RegexOptions.Compiled);
    private static readonly Regex IsbnRegex = new(@"^\d{13}$", RegexOptions.Compiled);

    /// <summary>Перевіряє, що рядок не порожній і не довший за maxLength. Повертає обрізаний рядок.</summary>
    public static string RequireText(string? value, string fieldName, int maxLength = 100)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"Поле \"{fieldName}\" не може бути порожнім.");

        string trimmed = value.Trim();
        if (trimmed.Length > maxLength)
            throw new ArgumentException($"Поле \"{fieldName}\" не може бути довшим за {maxLength} символів.");

        return trimmed;
    }

    /// <summary>Перевіряє, що ціле число лежить у межах [min; max].</summary>
    public static int RequireRange(int value, int min, int max, string fieldName)
    {
        if (value < min || value > max)
            throw new ArgumentException($"Значення поля \"{fieldName}\" має бути від {min} до {max} (введено {value}).");

        return value;
    }

    /// <summary>Перевіряє ім'я або прізвище: лише літери, пробіл, дефіс та апостроф.</summary>
    public static string RequireName(string? value, string fieldName)
    {
        string text = RequireText(value, fieldName, 50);
        if (!NameRegex.IsMatch(text))
            throw new ArgumentException($"Поле \"{fieldName}\" може містити лише літери, пробіл, дефіс та апостроф.");

        return text;
    }

    /// <summary>Перевіряє номер телефону у форматі +380XXXXXXXXX.</summary>
    public static string RequirePhone(string? value)
    {
        if (!IsValidPhone(value))
            throw new ArgumentException("Телефон має бути у форматі +380XXXXXXXXX (12 цифр після знака +).");

        return value!.Trim();
    }

    /// <summary>Перевіряє адресу електронної пошти.</summary>
    public static string RequireEmail(string? value)
    {
        if (!IsValidEmail(value))
            throw new ArgumentException("Електронна пошта має бути у форматі name@domain.com.");

        return value!.Trim().ToLowerInvariant();
    }

    /// <summary>Перевіряє ISBN (13 цифр, дефіси та пробіли дозволені) і повертає його без дефісів.</summary>
    public static string RequireIsbn(string? value)
    {
        string isbn = NormalizeIsbn(value);
        if (!IsbnRegex.IsMatch(isbn))
            throw new ArgumentException("ISBN має містити рівно 13 цифр (дефіси дозволені).");

        return isbn;
    }

    public static bool IsValidName(string? value) =>
        !string.IsNullOrWhiteSpace(value) && value.Trim().Length <= 50 && NameRegex.IsMatch(value.Trim());

    public static bool IsValidPhone(string? value) =>
        value != null && PhoneRegex.IsMatch(value.Trim());

    public static bool IsValidEmail(string? value) =>
        value != null && value.Trim().Length <= 100 && EmailRegex.IsMatch(value.Trim());

    public static bool IsValidIsbn(string? value) =>
        IsbnRegex.IsMatch(NormalizeIsbn(value));

    private static string NormalizeIsbn(string? value) =>
        (value ?? string.Empty).Replace("-", string.Empty).Replace(" ", string.Empty);
}
