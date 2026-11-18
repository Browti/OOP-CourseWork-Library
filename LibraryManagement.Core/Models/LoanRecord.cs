using System.Text.Json.Serialization;
using LibraryManagement.Core.Exceptions;

namespace LibraryManagement.Core.Models;

/// <summary>
/// Запис про видачу книги читачу (абонемент).
/// </summary>
public class LoanRecord : EntityBase
{
    private int _bookId;
    private int _readerId;
    private DateTime _loanDate = DateTime.Today;
    private DateTime _dueDate = DateTime.Today.AddDays(1);

    /// <summary>Код виданої книги.</summary>
    public int BookId
    {
        get => _bookId;
        set
        {
            if (value <= 0)
                throw new ArgumentException("Не вказано книгу.");
            _bookId = value;
        }
    }

    /// <summary>Код читача.</summary>
    public int ReaderId
    {
        get => _readerId;
        set
        {
            if (value <= 0)
                throw new ArgumentException("Не вказано читача.");
            _readerId = value;
        }
    }

    /// <summary>Дата видачі.</summary>
    [JsonPropertyOrder(10)]
    public DateTime LoanDate
    {
        get => _loanDate;
        set
        {
            if (value.Date > DateTime.Today)
                throw new ArgumentException("Дата видачі не може бути в майбутньому.");
            _loanDate = value.Date;
        }
    }

    /// <summary>Дата, до якої книгу треба повернути.</summary>
    [JsonPropertyOrder(11)]
    public DateTime DueDate
    {
        get => _dueDate;
        set
        {
            if (value.Date <= _loanDate)
                throw new ArgumentException("Термін повернення має бути пізнішим за дату видачі.");
            _dueDate = value.Date;
        }
    }

    /// <summary>Фактична дата повернення (null - книга ще на руках).</summary>
    [JsonInclude]
    [JsonPropertyOrder(12)]
    public DateTime? ReturnDate { get; private set; }

    /// <summary>Нарахований штраф (фіксується в момент повернення).</summary>
    [JsonInclude]
    [JsonPropertyOrder(13)]
    public decimal FineAmount { get; private set; }

    /// <summary>Чи повернута книга.</summary>
    [JsonIgnore]
    public bool IsReturned => ReturnDate.HasValue;

    /// <summary>
    /// Кількість днів прострочення. Для поверненої книги рахується до дати повернення,
    /// для неповерненої - до вказаної дати onDate.
    /// </summary>
    public int GetOverdueDays(DateTime onDate)
    {
        DateTime endDate = ReturnDate ?? onDate.Date;
        int days = (endDate - DueDate).Days;
        return days > 0 ? days : 0;
    }

    /// <summary>Чи прострочено повернення на вказану дату.</summary>
    public bool IsOverdue(DateTime onDate) => GetOverdueDays(onDate) > 0;

    /// <summary>Закрити запис: зафіксувати дату повернення та штраф.</summary>
    internal void Close(DateTime returnDate, decimal fine)
    {
        if (IsReturned)
            throw new LibraryException($"Книгу за записом №{Id} вже повернуто.");
        if (returnDate.Date < LoanDate)
            throw new ArgumentException("Дата повернення не може бути раніше дати видачі.");
        if (returnDate.Date > DateTime.Today)
            throw new ArgumentException("Дата повернення не може бути в майбутньому.");
        if (fine < 0)
            throw new ArgumentException("Штраф не може бути від'ємним.");

        ReturnDate = returnDate.Date;
        FineAmount = fine;
    }

    public override string GetShortInfo() =>
        $"№{Id}: книга #{BookId} -> читач #{ReaderId}, термін до {DueDate:dd.MM.yyyy}";
}
