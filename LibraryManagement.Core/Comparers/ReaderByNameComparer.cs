using LibraryManagement.Core.Models;

namespace LibraryManagement.Core.Comparers;

/// <summary>
/// Порівнювач читачів за прізвищем, потім за ім'ям.
/// </summary>
public class ReaderByNameComparer : IComparer<Reader>
{
    public int Compare(Reader? x, Reader? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (x is null) return -1;
        if (y is null) return 1;

        int result = string.Compare(x.LastName, y.LastName, StringComparison.CurrentCultureIgnoreCase);
        return result != 0
            ? result
            : string.Compare(x.FirstName, y.FirstName, StringComparison.CurrentCultureIgnoreCase);
    }
}
