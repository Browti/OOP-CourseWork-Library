using LibraryManagement.Core.Models;

namespace LibraryManagement.Core.Comparers;

/// <summary>
/// Порівнювач записів видачі за терміном повернення (найтерміновіші - першими).
/// </summary>
public class LoanByDueDateComparer : IComparer<LoanRecord>
{
    public int Compare(LoanRecord? x, LoanRecord? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (x is null) return -1;
        if (y is null) return 1;

        int result = x.DueDate.CompareTo(y.DueDate);
        return result != 0 ? result : x.Id.CompareTo(y.Id);
    }
}
