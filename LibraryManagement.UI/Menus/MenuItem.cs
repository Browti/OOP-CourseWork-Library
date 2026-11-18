namespace LibraryManagement.UI.Menus;

/// <summary>
/// Пункт меню: клавіша, текст та дія.
/// </summary>
internal sealed class MenuItem
{
    public MenuItem(string key, string text, Action action, bool isSubmenu)
    {
        Key = key;
        Text = text;
        Action = action;
        IsSubmenu = isSubmenu;
    }

    public string Key { get; }

    public string Text { get; }

    public Action Action { get; }

    /// <summary>true - пункт відкриває вкладене меню (без паузи після виконання).</summary>
    public bool IsSubmenu { get; }
}
