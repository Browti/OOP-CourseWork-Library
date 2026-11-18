using System.Text.Json.Serialization;
using LibraryManagement.Core.Exceptions;
using LibraryManagement.Core.Validation;

namespace LibraryManagement.Core.Models;

/// <summary>
/// Книга в каталозі бібліотеки. Одна книга може мати кілька примірників.
/// Реалізує IComparable&lt;Book&gt; - природне сортування за назвою.
/// </summary>
public class Book : EntityBase, IComparable<Book>
{
    public const int MinYear = 1450;
    public const int MinCopies = 1;
    public const int MaxCopies = 1000;

    private string _title = string.Empty;
    private string _isbn = string.Empty;
    private int _authorId;
    private Genre _genre = Genre.Other;
    private int _publicationYear = MinYear;
    private int _totalCopies;
    private int _availableCopies;

    /// <summary>Назва книги.</summary>
    public string Title
    {
        get => _title;
        set => _title = DataValidator.RequireText(value, "Назва книги", 150);
    }

    /// <summary>Код автора книги.</summary>
    public int AuthorId
    {
        get => _authorId;
        set
        {
            if (value <= 0)
                throw new ArgumentException("Не вказано автора книги.");
            _authorId = value;
        }
    }

    /// <summary>Жанр книги.</summary>
    public Genre Genre
    {
        get => _genre;
        set
        {
            if (!Enum.IsDefined(value))
                throw new ArgumentException("Невідомий жанр книги.");
            _genre = value;
        }
    }

    /// <summary>Рік видання.</summary>
    public int PublicationYear
    {
        get => _publicationYear;
        set => _publicationYear = DataValidator.RequireRange(value, MinYear, DateTime.Today.Year, "Рік видання");
    }

    /// <summary>ISBN - міжнародний номер книги (13 цифр).</summary>
    public string Isbn
    {
        get => _isbn;
        set => _isbn = DataValidator.RequireIsbn(value);
    }

    /// <summary>
    /// Загальна кількість примірників. При зміні кількість доступних примірників
    /// перераховується; не можна зробити її меншою, ніж видано читачам.
    /// </summary>
    [JsonPropertyOrder(10)]
    public int TotalCopies
    {
        get => _totalCopies;
        set
        {
            DataValidator.RequireRange(value, MinCopies, MaxCopies, "Кількість примірників");

            int onLoan = _totalCopies - _availableCopies;
            if (value < onLoan)
                throw new ArgumentException(
                    $"Не можна встановити {value} примірн.: зараз читачам видано {onLoan}.");

            _availableCopies = value - onLoan;
            _totalCopies = value;
        }
    }

    /// <summary>Кількість примірників, які зараз є на полиці. Змінюється лише методами IssueCopy/ReturnCopy.</summary>
    [JsonInclude]
    [JsonPropertyOrder(11)]
    public int AvailableCopies
    {
        get => _availableCopies;
        private set
        {
            if (value < 0 || value > _totalCopies)
                throw new ArgumentException($"Некоректна кількість доступних примірників книги \"{Title}\".");
            _availableCopies = value;
        }
    }

    /// <summary>Кількість примірників на руках у читачів.</summary>
    [JsonIgnore]
    public int CopiesOnLoan => _totalCopies - _availableCopies;

    /// <summary>Чи є хоча б один вільний примірник.</summary>
    [JsonIgnore]
    public bool IsAvailable => _availableCopies > 0;

    /// <summary>Видати один примірник (зменшує кількість доступних).</summary>
    internal void IssueCopy()
    {
        if (!IsAvailable)
            throw new BookNotAvailableException(Title);
        _availableCopies--;
    }

    /// <summary>Повернути один примірник на полицю.</summary>
    internal void ReturnCopy()
    {
        if (_availableCopies >= _totalCopies)
            throw new LibraryException($"Усі примірники книги \"{Title}\" вже на полиці.");
        _availableCopies++;
    }

    /// <summary>Порівняння книг за назвою (без урахування регістру).</summary>
    public int CompareTo(Book? other)
    {
        if (other is null)
            return 1;
        return string.Compare(Title, other.Title, StringComparison.CurrentCultureIgnoreCase);
    }

    public override string GetShortInfo() => $"[{Id}] \"{Title}\" ({PublicationYear})";
}
