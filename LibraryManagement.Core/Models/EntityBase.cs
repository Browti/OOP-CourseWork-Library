using System.Text.Json.Serialization;

namespace LibraryManagement.Core.Models;

/// <summary>
/// Базовий абстрактний клас для всіх сутностей предметної області.
/// Містить унікальний код (Id), який призначає репозиторій.
/// </summary>
public abstract class EntityBase
{
    /// <summary>Унікальний код сутності. Змінювати його може лише бізнес-логіка (internal set).</summary>
    [JsonInclude]
    [JsonPropertyOrder(-1)]
    public int Id { get; internal set; }

    /// <summary>Короткий опис сутності для виведення користувачу (поліморфний метод).</summary>
    public abstract string GetShortInfo();

    public override string ToString() => GetShortInfo();
}
