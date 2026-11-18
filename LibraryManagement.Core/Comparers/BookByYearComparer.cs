using LibraryManagement.Core.Models;

namespace LibraryManagement.Core.Comparers;

/// <summary>
/// Порівнювач книг за роком видання (стандартний інтерфейс IComparer&lt;T&gt;).
/// Книги одного року впорядковуються за назвою.
/// </summary>
public class BookByYearComparer : IComparer<Book>
{
    private readonly bool _descending;

    /// <param name="descending">true - від нових до старих.</param>
    public BookByYearComparer(bool descending = false)
    {
        _descending = descending;
    }

    public int Compare(Book? x, Book? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (x is null) return -1;
        if (y is null) return 1;

        int result = x.PublicationYear.CompareTo(y.PublicationYear);
        if (result != 0)
            return _descending ? -result : result;

        return x.CompareTo(y);
    }
}
