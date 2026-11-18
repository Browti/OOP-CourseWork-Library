using LibraryManagement.Core.Comparers;
using LibraryManagement.Core.Exceptions;
using LibraryManagement.Core.Interfaces;
using LibraryManagement.Core.Models;
using LibraryManagement.Core.Repositories;
using LibraryManagement.Core.Storage;
using LibraryManagement.Core.Validation;

namespace LibraryManagement.Core.Services;

/// <summary>
/// Головний сервіс бізнес-логіки бібліотеки: каталог, читачі, видача/повернення книг,
/// пошук, сортування, розрахунок штрафів, збереження даних.
/// Консольний інтерфейс працює лише через цей клас.
/// </summary>
public class LibraryService
{
    public const int DefaultLoanDays = 14;
    public const int MinLoanDays = 1;
    public const int MaxLoanDays = 60;

    private readonly IRepository<Author> _authors = new Repository<Author>("Автора");
    private readonly IRepository<Book> _books = new Repository<Book>("Книгу");
    private readonly IRepository<Reader> _readers = new Repository<Reader>("Читача");
    private readonly IRepository<LoanRecord> _loans = new Repository<LoanRecord>("Запис видачі");

    private readonly IDataStorage _storage;
    private readonly IFineCalculator _fineCalculator;

    public LibraryService(IDataStorage storage, IFineCalculator fineCalculator)
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        _fineCalculator = fineCalculator ?? throw new ArgumentNullException(nameof(fineCalculator));
    }

    /// <summary>Поточний тариф штрафів.</summary>
    public IFineCalculator FineCalculator => _fineCalculator;

    /// <summary>Чи є в бібліотеці хоч якісь дані.</summary>
    public bool HasData => _authors.Count > 0 || _books.Count > 0 || _readers.Count > 0;

    #region Автори

    public Author AddAuthor(string firstName, string lastName, string country, int birthYear)
    {
        var author = new Author
        {
            FirstName = firstName,
            LastName = lastName,
            Country = country,
            BirthYear = birthYear
        };

        if (_authors.Exists(a => a.BirthYear == author.BirthYear &&
                                 a.FullName.Equals(author.FullName, StringComparison.CurrentCultureIgnoreCase)))
            throw new DuplicateEntityException($"Автор {author.FullName} ({author.BirthYear} р.н.) вже є в каталозі.");

        _authors.Add(author);
        return author;
    }

    public IReadOnlyList<Author> GetAuthors() => _authors.GetAll();

    public Author GetAuthor(int authorId) => _authors[authorId];

    public string GetAuthorName(int authorId) => _authors.GetById(authorId)?.FullName ?? "(невідомий автор)";

    public int CountBooksByAuthor(int authorId) => _books.Find(b => b.AuthorId == authorId).Count;

    public void RemoveAuthor(int authorId)
    {
        Author author = _authors[authorId];
        if (_books.Exists(b => b.AuthorId == authorId))
            throw new EntityInUseException(
                $"Неможливо видалити автора {author.FullName}: у каталозі є його книги. Спершу видаліть книги.");

        _authors.Remove(authorId);
    }

    #endregion

    #region Книги

    public Book AddBook(string title, int authorId, Genre genre, int publicationYear, string isbn, int copies)
    {
        _ = _authors[authorId]; // перевірка існування автора (EntityNotFoundException)

        var book = new Book
        {
            Title = title,
            AuthorId = authorId,
            Genre = genre,
            PublicationYear = publicationYear,
            Isbn = isbn,
            TotalCopies = copies
        };

        if (_books.Exists(b => b.Isbn == book.Isbn))
            throw new DuplicateEntityException($"Книга з ISBN {book.Isbn} вже є в каталозі.");

        _books.Add(book);
        return book;
    }

    public Book GetBook(int bookId) => _books[bookId];

    public string GetBookTitle(int bookId) => _books.GetById(bookId)?.Title ?? "(видалена книга)";

    /// <summary>
    /// Редагування книги. Спершу всі нові значення перевіряються на тимчасовому об'єкті,
    /// щоб у разі помилки книга не змінилася частково.
    /// </summary>
    public void UpdateBook(int bookId, string title, Genre genre, int publicationYear, int totalCopies)
    {
        Book book = _books[bookId];

        var probe = new Book { Title = title, Genre = genre, PublicationYear = publicationYear };

        book.TotalCopies = totalCopies; // може кинути ArgumentException, якщо примірників менше, ніж видано
        book.Title = probe.Title;
        book.Genre = probe.Genre;
        book.PublicationYear = probe.PublicationYear;
    }

    public void RemoveBook(int bookId)
    {
        Book book = _books[bookId];
        if (_loans.Exists(l => l.BookId == bookId && !l.IsReturned))
            throw new EntityInUseException(
                $"Неможливо видалити книгу \"{book.Title}\": її примірники зараз на руках у читачів.");

        _books.Remove(bookId);
    }

    /// <summary>Усі книги, відсортовані за назвою (IComparable&lt;Book&gt;).</summary>
    public List<Book> GetBooksSortedByTitle()
    {
        List<Book> result = _books.GetAll().ToList();
        result.Sort();
        return result;
    }

    /// <summary>Усі книги, відсортовані за роком видання (IComparer&lt;Book&gt;).</summary>
    public List<Book> GetBooksSortedByYear(bool descending)
    {
        List<Book> result = _books.GetAll().ToList();
        result.Sort(new BookByYearComparer(descending));
        return result;
    }

    /// <summary>Пошук книг за ім'ям або прізвищем автора (можна частину).</summary>
    public List<Book> FindBooksByAuthor(string authorNameFragment)
    {
        string fragment = DataValidator.RequireText(authorNameFragment, "Ім'я або прізвище автора", 50);

        HashSet<int> authorIds = _authors.Find(a => a.NameContains(fragment))
                                         .Select(a => a.Id)
                                         .ToHashSet();

        List<Book> result = _books.Find(b => authorIds.Contains(b.AuthorId));
        result.Sort();
        return result;
    }

    /// <summary>Пошук книг за жанром.</summary>
    public List<Book> FindBooksByGenre(Genre genre)
    {
        List<Book> result = _books.Find(b => b.Genre == genre);
        result.Sort();
        return result;
    }

    /// <summary>Пошук книг за частиною назви.</summary>
    public List<Book> FindBooksByTitle(string titleFragment)
    {
        string fragment = DataValidator.RequireText(titleFragment, "Назва книги", 150);

        List<Book> result = _books.Find(b => b.Title.Contains(fragment, StringComparison.CurrentCultureIgnoreCase));
        result.Sort();
        return result;
    }

    /// <summary>Книги, які зараз є на полиці.</summary>
    public List<Book> GetAvailableBooks()
    {
        List<Book> result = _books.Find(b => b.IsAvailable);
        result.Sort();
        return result;
    }

    #endregion

    #region Читачі

    public Reader AddReader(string firstName, string lastName, string phone, string email, int bookLimit)
    {
        var reader = new Reader
        {
            FirstName = firstName,
            LastName = lastName,
            Phone = phone,
            Email = email,
            BookLimit = bookLimit,
            RegistrationDate = DateTime.Today
        };

        if (_readers.Exists(r => r.Phone == reader.Phone))
            throw new DuplicateEntityException($"Читач з телефоном {reader.Phone} вже зареєстрований.");

        _readers.Add(reader);
        return reader;
    }

    public Reader GetReader(int readerId) => _readers[readerId];

    public string GetReaderName(int readerId) => _readers.GetById(readerId)?.FullName ?? "(видалений читач)";

    public List<Reader> GetReadersSortedByName()
    {
        List<Reader> result = _readers.GetAll().ToList();
        result.Sort(new ReaderByNameComparer());
        return result;
    }

    public void UpdateReader(int readerId, string phone, string email, int bookLimit)
    {
        Reader reader = _readers[readerId];

        string newPhone = DataValidator.RequirePhone(phone);
        string newEmail = DataValidator.RequireEmail(email);
        DataValidator.RequireRange(bookLimit, Reader.MinBookLimit, Reader.MaxBookLimit, "Ліміт книг");

        if (_readers.Exists(r => r.Id != readerId && r.Phone == newPhone))
            throw new DuplicateEntityException($"Телефон {newPhone} вже використовує інший читач.");

        int activeLoans = CountActiveLoans(readerId);
        if (bookLimit < activeLoans)
            throw new ArgumentException(
                $"Ліміт не може бути меншим за кількість книг, які зараз на руках у читача ({activeLoans}).");

        reader.Phone = newPhone;
        reader.Email = newEmail;
        reader.BookLimit = bookLimit;
    }

    public void RemoveReader(int readerId)
    {
        Reader reader = _readers[readerId];
        if (CountActiveLoans(readerId) > 0)
            throw new EntityInUseException(
                $"Неможливо видалити читача {reader.FullName}: у нього є неповернені книги.");

        _readers.Remove(readerId);
    }

    /// <summary>Кількість книг, які зараз на руках у читача.</summary>
    public int CountActiveLoans(int readerId) => _loans.Find(l => l.ReaderId == readerId && !l.IsReturned).Count;

    #endregion

    #region Видача та повернення

    /// <summary>
    /// Перевірка, чи може читач узяти ще одну книгу.
    /// Кидає MaxBooksExceededException, якщо ліміт вичерпано.
    /// </summary>
    public void EnsureReaderCanBorrow(int readerId)
    {
        Reader reader = _readers[readerId];
        if (CountActiveLoans(readerId) >= reader.BookLimit)
            throw new MaxBooksExceededException(reader.FullName, reader.BookLimit);
    }

    /// <summary>Видати книгу читачу.</summary>
    public LoanRecord IssueBook(int readerId, int bookId, DateTime loanDate, int loanDays)
    {
        Reader reader = _readers[readerId];
        Book book = _books[bookId];
        DataValidator.RequireRange(loanDays, MinLoanDays, MaxLoanDays, "Термін користування (днів)");

        EnsureReaderCanBorrow(readerId);

        if (_loans.Exists(l => l.ReaderId == readerId && l.BookId == bookId && !l.IsReturned))
            throw new BookNotAvailableException(book.Title,
                $"Читач {reader.FullName} вже має на руках примірник книги \"{book.Title}\".");

        if (!book.IsAvailable)
            throw new BookNotAvailableException(book.Title);

        var loan = new LoanRecord
        {
            BookId = bookId,
            ReaderId = readerId,
            LoanDate = loanDate,
            DueDate = loanDate.Date.AddDays(loanDays)
        };

        book.IssueCopy();
        _loans.Add(loan);
        return loan;
    }

    /// <summary>Повернути книгу. Повертає суму нарахованого штрафу.</summary>
    public decimal ReturnBook(int loanId, DateTime returnDate)
    {
        LoanRecord loan = _loans[loanId];
        if (loan.IsReturned)
            throw new LibraryException($"Книгу за записом №{loanId} вже повернуто {loan.ReturnDate:dd.MM.yyyy}.");

        Book book = _books[loan.BookId];
        decimal fine = _fineCalculator.CalculateFine(loan, returnDate);

        loan.Close(returnDate, fine);
        book.ReturnCopy();
        return fine;
    }

    public LoanRecord GetLoan(int loanId) => _loans[loanId];

    /// <summary>Штраф за записом: для поверненої книги - зафіксований, для неповерненої - якщо повернуть у дату onDate.</summary>
    public decimal GetFine(LoanRecord loan, DateTime onDate)
    {
        ArgumentNullException.ThrowIfNull(loan);
        return loan.IsReturned ? loan.FineAmount : _fineCalculator.CalculateFine(loan, onDate);
    }

    /// <summary>Попередній розрахунок штрафу, якщо книгу повернуть у вказану дату.</summary>
    public decimal PreviewFine(int loanId, DateTime returnDate)
    {
        LoanRecord loan = _loans[loanId];
        if (!loan.IsReturned && returnDate.Date < loan.LoanDate)
            throw new ArgumentException("Дата повернення не може бути раніше дати видачі.");

        return GetFine(loan, returnDate);
    }

    /// <summary>Неповернені книги (за терміном повернення).</summary>
    public List<LoanRecord> GetActiveLoans()
    {
        List<LoanRecord> result = _loans.Find(l => !l.IsReturned);
        result.Sort(new LoanByDueDateComparer());
        return result;
    }

    /// <summary>Прострочені видачі (боржники) на вказану дату.</summary>
    public List<LoanRecord> GetOverdueLoans(DateTime onDate)
    {
        List<LoanRecord> result = _loans.Find(l => !l.IsReturned && l.IsOverdue(onDate));
        result.Sort(new LoanByDueDateComparer());
        return result;
    }

    /// <summary>Уся історія видач.</summary>
    public List<LoanRecord> GetAllLoans()
    {
        List<LoanRecord> result = _loans.GetAll().ToList();
        result.Sort(new LoanByDueDateComparer());
        return result;
    }

    /// <summary>Історія видач конкретного читача.</summary>
    public List<LoanRecord> GetReaderLoans(int readerId)
    {
        _ = _readers[readerId];
        List<LoanRecord> result = _loans.Find(l => l.ReaderId == readerId);
        result.Sort(new LoanByDueDateComparer());
        return result;
    }

    #endregion

    #region Статистика та збереження

    public LibraryStatistics GetStatistics(DateTime onDate)
    {
        IReadOnlyList<Book> books = _books.GetAll();
        List<LoanRecord> activeLoans = _loans.Find(l => !l.IsReturned);

        return new LibraryStatistics(
            AuthorCount: _authors.Count,
            BookTitleCount: books.Count,
            TotalCopies: books.Sum(b => b.TotalCopies),
            CopiesOnLoan: books.Sum(b => b.CopiesOnLoan),
            ReaderCount: _readers.Count,
            ActiveLoanCount: activeLoans.Count,
            OverdueLoanCount: activeLoans.Count(l => l.IsOverdue(onDate)),
            CollectedFines: _loans.Find(l => l.IsReturned).Sum(l => l.FineAmount),
            ExpectedFines: activeLoans.Sum(l => _fineCalculator.CalculateFine(l, onDate)));
    }

    /// <summary>Завантажити дані зі сховища (десеріалізація).</summary>
    public void LoadData()
    {
        LibraryData data = _storage.Load();

        _authors.Load(data.Authors);
        _books.Load(data.Books);
        _readers.Load(data.Readers);
        _loans.Load(data.Loans);
    }

    /// <summary>Зберегти дані у сховище (серіалізація).</summary>
    public void SaveData()
    {
        var data = new LibraryData
        {
            Authors = _authors.GetAll().ToList(),
            Books = _books.GetAll().ToList(),
            Readers = _readers.GetAll().ToList(),
            Loans = _loans.GetAll().ToList(),
            SavedAt = DateTime.Now
        };

        _storage.Save(data);
    }

    #endregion
}
