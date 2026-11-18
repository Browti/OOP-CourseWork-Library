namespace LibraryManagement.UI.Output;

/// <summary>
/// Допоміжні методи для форматованого (кольорового, табличного) виведення в консоль.
/// </summary>
internal static class ConsoleHelper
{
    public static void WriteHeader(string title)
    {
        string line = new('=', Math.Max(60, title.Length + 4));
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(line);
        Console.WriteLine($"  {title}");
        Console.WriteLine(line);
        Console.ResetColor();
    }

    public static void WriteSuccess(string message) => WriteColored(message, ConsoleColor.Green);

    public static void WriteError(string message) => WriteColored(message, ConsoleColor.Red);

    public static void WriteWarning(string message) => WriteColored(message, ConsoleColor.Yellow);

    public static void WriteHint(string message) => WriteColored(message, ConsoleColor.DarkGray);

    public static void Pause()
    {
        Console.WriteLine();
        Console.Write("Натисніть Enter, щоб продовжити...");
        Console.ReadLine();
    }

    /// <summary>Безпечне очищення екрана (не падає, якщо вивід перенаправлено).</summary>
    public static void Clear()
    {
        try
        {
            Console.Clear();
        }
        catch (IOException)
        {
            Console.WriteLine();
        }
    }

    /// <summary>Виведення даних у вигляді таблиці з рамкою.</summary>
    public static void PrintTable(IReadOnlyList<string> headers, IReadOnlyList<int> widths,
                                  IEnumerable<IReadOnlyList<string>> rows)
    {
        string separator = "+" + string.Join("+", widths.Select(w => new string('-', w + 2))) + "+";

        Console.WriteLine(separator);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(FormatRow(headers, widths));
        Console.ResetColor();
        Console.WriteLine(separator);

        int count = 0;
        foreach (IReadOnlyList<string> row in rows)
        {
            Console.WriteLine(FormatRow(row, widths));
            count++;
        }

        if (count == 0)
            Console.WriteLine("| " + "(немає записів)".PadRight(separator.Length - 4) + " |");

        Console.WriteLine(separator);
    }

    private static void WriteColored(string message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    private static string FormatRow(IReadOnlyList<string> cells, IReadOnlyList<int> widths)
    {
        var parts = new List<string>(widths.Count);
        for (int i = 0; i < widths.Count; i++)
        {
            string cell = i < cells.Count ? cells[i] : string.Empty;
            if (cell.Length > widths[i])
                cell = cell[..(widths[i] - 3)] + "...";

            parts.Add(" " + cell.PadRight(widths[i]) + " ");
        }

        return "|" + string.Join("|", parts) + "|";
    }
}
