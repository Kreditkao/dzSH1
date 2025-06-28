using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class LibraryService
{
    private readonly List<Book> _books;
    private readonly object _monitorLock = new();
    private readonly SemaphoreSlim _semaphore = new(3);
    private readonly Mutex _mutex = new();
    private int _issuedCount = 0;
    private volatile bool _acceptingRequests = true;

    public User? CurrentUser { get; set; }

    public LibraryService(List<Book> books)
    {
        _books = books;
    }

    // Метод выдачи книги пользователю
    public async Task BorrowBookAsync(string title)
    {
        if (!_acceptingRequests) return;

        await _semaphore.WaitAsync();
        bool lockTaken = false;
        try
        {
            Monitor.Enter(_monitorLock, ref lockTaken);
            var book = _books.FirstOrDefault(b => b.Title == title && b.IsAvailable);
            if (book != null)
            {
                book.IsAvailable = false;
                book.DueDate = DateTime.Now.AddDays(7);
                book.BorrowedByUserId = CurrentUser?.Id;

                Interlocked.Increment(ref _issuedCount);
                await DatabaseHelper.UpdateBookAsync(book);

                if (CurrentUser != null)
                {
                    await DatabaseHelper.LogTransactionAsync(CurrentUser.Id, book.Id, "borrow");
                    Console.WriteLine($" \"{title}\" выдана пользователю {CurrentUser.Username}");
                }
                else
                {
                    Console.WriteLine($" \"{title}\" выдана (неизвестному пользователю)");
                }
            }
            else
            {
                Console.WriteLine($" Книга \"{title}\" недоступна.");
            }
        }
        finally
        {
            if (lockTaken) Monitor.Exit(_monitorLock);
            _semaphore.Release();
        }
    }

    // Метод возврата книги
    public async Task ReturnBookAsync(string title)
    {
        if (!_acceptingRequests) return;

        await _semaphore.WaitAsync();
        try
        {
            _mutex.WaitOne();
            try
            {
                var book = _books.FirstOrDefault(b => b.Title == title && !b.IsAvailable);
                if (book != null)
                {
                    book.IsAvailable = true;
                    book.DueDate = null;
                    book.BorrowedByUserId = null;

                    await DatabaseHelper.UpdateBookAsync(book);

                    if (CurrentUser != null)
                    {
                        await DatabaseHelper.LogTransactionAsync(CurrentUser.Id, book.Id, "return");
                        Console.WriteLine($" \"{title}\" возвращена пользователем {CurrentUser.Username}");
                    }
                    else
                    {
                        Console.WriteLine($" \"{title}\" возвращена (неизвестным пользователем)");
                    }
                }
                else
                {
                    Console.WriteLine($" Книга \"{title}\" не найдена среди выданных.");
                }
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    // Вывод списка всех книг с их статусом
    public void ListBooks()
    {
        foreach (var book in _books)
        {
            string status = book.IsAvailable
                ? "Доступна"
                : $"Занята до {book.DueDate:dd.MM.yyyy} (User ID: {book.BorrowedByUserId})";

            Console.WriteLine($"📚 {book.Title} — {status}");
        }
    }

    // Метод автоматического возврата просроченных книг (используется таймером)
    public async Task UpdateOverduesAsync()
    {
        foreach (var book in _books.Where(b => b.DueDate.HasValue && b.DueDate.Value < DateTime.Now))
        {
            book.IsAvailable = true;
            book.DueDate = null;
            book.BorrowedByUserId = null;
            await DatabaseHelper.UpdateBookAsync(book);
            Console.WriteLine($" \"{book.Title}\" возвращена автоматически (просрочка).");
        }
    }

    // Метод для отключения обработки новых запросов
    public void StopAcceptingRequests() => _acceptingRequests = false;
}
