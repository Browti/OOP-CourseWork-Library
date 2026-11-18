using LibraryManagement.Core.Exceptions;
using LibraryManagement.Core.Models;

namespace LibraryManagement.Core.Services;

/// <summary>
/// Заповнення порожньої бібліотеки демонстраційними даними
/// (для перевірки роботи програми). Усі дані додаються через LibraryService,
/// тобто проходять ту саму валідацію, що й дані від користувача.
/// </summary>
public static class DemoDataSeeder
{
    public static void Seed(LibraryService service)
    {
        ArgumentNullException.ThrowIfNull(service);

        if (service.HasData)
            throw new LibraryException(
                "Бібліотека вже містить дані. Демонстраційні дані можна додати лише до порожньої бібліотеки.");

        // Автори
        Author shevchenko = service.AddAuthor("Тарас", "Шевченко", "Україна", 1814);
        Author franko = service.AddAuthor("Іван", "Франко", "Україна", 1856);
        Author kostenko = service.AddAuthor("Ліна", "Костенко", "Україна", 1930);
        Author orwell = service.AddAuthor("Джордж", "Орвелл", "Велика Британія", 1903);
        Author christie = service.AddAuthor("Агата", "Крісті", "Велика Британія", 1890);
        Author asimov = service.AddAuthor("Айзек", "Азімов", "США", 1920);

        // Книги
        Book kobzar = service.AddBook("Кобзар", shevchenko.Id, Genre.Poetry, 1840, "978-966-03-5943-1", 3);
        Book zakhar = service.AddBook("Захар Беркут", franko.Id, Genre.History, 1883, "978-966-10-4011-2", 2);
        Book stolen = service.AddBook("Украдене щастя", franko.Id, Genre.Fiction, 1893, "978-966-10-4012-9", 1);
        Book marusia = service.AddBook("Маруся Чурай", kostenko.Id, Genre.Poetry, 1979, "978-617-585-021-3", 2);
        Book nineteen = service.AddBook("1984", orwell.Id, Genre.ScienceFiction, 1949, "978-617-12-0587-4", 2);
        Book farm = service.AddBook("Колгосп тварин", orwell.Id, Genre.Fiction, 1945, "978-617-12-0588-1", 1);
        Book orient = service.AddBook("Вбивство в Східному експресі", christie.Id, Genre.Detective, 1934, "978-617-12-3401-0", 2);
        Book robot = service.AddBook("Я, робот", asimov.Id, Genre.ScienceFiction, 1950, "978-966-14-9802-9", 1);
        service.AddBook("Фундація", asimov.Id, Genre.ScienceFiction, 1951, "978-966-14-9803-6", 2);

        // Читачі
        Reader petrenko = service.AddReader("Олександр", "Петренко", "+380501234567", "petrenko@gmail.com", 3);
        Reader kovalenko = service.AddReader("Марія", "Коваленко", "+380672345678", "kovalenko.m@ukr.net", 2);
        Reader bondar = service.AddReader("Андрій", "Бондар", "+380933456789", "bondar.andrii@gmail.com", 3);
        service.AddReader("Олена", "Шевчук", "+380664567890", "shevchuk.olena@gmail.com", 5);

        DateTime today = DateTime.Today;

        // Історія (додається першою, щоб не перевищити ліміти читачів):
        // одна книга повернута вчасно, дві - із запізненням (10 днів => 7*5 + 3*10 = 65 грн; 3 дні => 15 грн)
        LoanRecord onTime = service.IssueBook(bondar.Id, zakhar.Id, today.AddDays(-60), LibraryService.DefaultLoanDays);
        service.ReturnBook(onTime.Id, today.AddDays(-50));

        LoanRecord late = service.IssueBook(petrenko.Id, marusia.Id, today.AddDays(-40), LibraryService.DefaultLoanDays);
        service.ReturnBook(late.Id, today.AddDays(-16));

        LoanRecord lateFarm = service.IssueBook(kovalenko.Id, farm.Id, today.AddDays(-35), LibraryService.DefaultLoanDays);
        service.ReturnBook(lateFarm.Id, today.AddDays(-18));

        // Активні видачі: Петренко - 2 книги, одна з них прострочена
        service.IssueBook(petrenko.Id, kobzar.Id, today.AddDays(-30), LibraryService.DefaultLoanDays);
        service.IssueBook(petrenko.Id, orient.Id, today.AddDays(-3), LibraryService.DefaultLoanDays);

        // Коваленко - досягла ліміту (2 з 2), одна книга прострочена
        service.IssueBook(kovalenko.Id, robot.Id, today.AddDays(-20), LibraryService.DefaultLoanDays);
        service.IssueBook(kovalenko.Id, stolen.Id, today.AddDays(-5), LibraryService.DefaultLoanDays);

        // Бондар - одна книга вчасно
        service.IssueBook(bondar.Id, nineteen.Id, today.AddDays(-7), LibraryService.DefaultLoanDays);
    }
}
