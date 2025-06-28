using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

class Library
{
    static async Task Main()
    {
        Console.InputEncoding = Encoding.UTF8;
        Console.OutputEncoding = Encoding.UTF8;


        Console.WriteLine("🔐 Введите имя пользователя: ");
        string? username = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(username))
        {
            Console.WriteLine("❌ Имя не может быть пустым.");
            return;
        }

        var user = await DatabaseHelper.GetOrCreateUserAsync(username);
        var books = await DatabaseHelper.LoadBooksAsync();
        var library = new LibraryService(books) { CurrentUser = user };
        var updater = new BookUpdater(library);

        Console.WriteLine($"👋 Добро пожаловать, {user.FullName}!");

        while (true)
        {
            Console.WriteLine("\n1 - Список книг\n2 - Взять книгу\n3 - Вернуть книгу\n0 - Выход");
            Console.Write("Ваш выбор: ");
            var choice = Console.ReadLine();

            if (choice == "1") library.ListBooks();
            else if (choice == "2")
            {
                Console.Write("Название книги: ");
                var title = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(title))
                    await library.BorrowBookAsync(title);
            }
            else if (choice == "3")
            {
                Console.Write("Название книги: ");
                var title = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(title))
                    await library.ReturnBookAsync(title);
            }
            else if (choice == "0")
            {
                library.StopAcceptingRequests();
                Console.WriteLine("👋 До свидания!");
                break;
            }
        }
    }
}
