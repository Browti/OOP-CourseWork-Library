namespace LibraryManagement.Core.Services;

/// <summary>
/// Стандартний тариф штрафу: перші 7 днів прострочення - 5 грн/день,
/// кожен наступний день - 10 грн/день, але не більше 500 грн за одну книгу.
/// </summary>
public class StandardFineCalculator : FineCalculatorBase
{
    private const decimal BaseDailyRate = 5m;
    private const decimal IncreasedDailyRate = 10m;
    private const int BasePeriodDays = 7;
    private const decimal MaxFine = 500m;

    public override string Description =>
        $"{BaseDailyRate} грн/день перші {BasePeriodDays} дн., далі {IncreasedDailyRate} грн/день, максимум {MaxFine} грн";

    protected override decimal CalculateForDays(int overdueDays)
    {
        int baseDays = Math.Min(overdueDays, BasePeriodDays);
        int extraDays = overdueDays - baseDays;

        decimal fine = baseDays * BaseDailyRate + extraDays * IncreasedDailyRate;
        return Math.Min(fine, MaxFine);
    }
}
