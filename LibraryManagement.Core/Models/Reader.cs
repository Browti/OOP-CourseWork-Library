using LibraryManagement.Core.Validation;

namespace LibraryManagement.Core.Models;

/// <summary>
/// Читач бібліотеки (власник абонемента).
/// </summary>
public class Reader : Person
{
    public const int DefaultBookLimit = 3;
    public const int MinBookLimit = 1;
    public const int MaxBookLimit = 10;

    private string _phone = string.Empty;
    private string _email = string.Empty;
    private int _bookLimit = DefaultBookLimit;
    private DateTime _registrationDate = DateTime.Today;

    /// <summary>Телефон у форматі +380XXXXXXXXX.</summary>
    public string Phone
    {
        get => _phone;
        set => _phone = DataValidator.RequirePhone(value);
    }

    /// <summary>Електронна пошта.</summary>
    public string Email
    {
        get => _email;
        set => _email = DataValidator.RequireEmail(value);
    }

    /// <summary>Максимальна кількість книг, які читач може одночасно тримати на руках.</summary>
    public int BookLimit
    {
        get => _bookLimit;
        set => _bookLimit = DataValidator.RequireRange(value, MinBookLimit, MaxBookLimit, "Ліміт книг");
    }

    /// <summary>Дата реєстрації абонемента.</summary>
    public DateTime RegistrationDate
    {
        get => _registrationDate;
        set
        {
            if (value.Date > DateTime.Today)
                throw new ArgumentException("Дата реєстрації не може бути в майбутньому.");
            _registrationDate = value.Date;
        }
    }

    public override string GetShortInfo() => $"[{Id}] {FullName}, тел. {Phone}";
}
