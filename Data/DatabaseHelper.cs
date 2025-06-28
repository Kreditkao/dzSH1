using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

public static class DatabaseHelper
{
    public static string ConnectionString =>
     "Data Source=DESKTOP-KGIM8M1\\SQLEXPRESS;Initial Catalog=LibraryDB;Integrated Security=True;TrustServerCertificate=True;";


    // Загрузка всех книг из базы данных
    public static async Task<List<Book>> LoadBooksAsync()
    {
        var books = new List<Book>();
        using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();
        var cmd = new SqlCommand("SELECT * FROM Books", conn);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            books.Add(new Book
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                Author = reader.GetString(2),
                IsAvailable = reader.GetBoolean(3),
                DueDate = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                BorrowedByUserId = reader.IsDBNull(5) ? null : reader.GetInt32(5) 
            });
        }
        return books;
    }

    // Обновление информации о книге в базе
    public static async Task UpdateBookAsync(Book book)
    {
        using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        var cmd = new SqlCommand(
            @"UPDATE Books 
          SET IsAvailable = @a, 
              DueDate = @d, 
              BorrowedByUserId = @u
          WHERE Id = @i", conn);

        cmd.Parameters.AddWithValue("@a", book.IsAvailable);
        cmd.Parameters.AddWithValue("@d", (object?)book.DueDate ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@u", (object?)book.BorrowedByUserId ?? DBNull.Value); 
        cmd.Parameters.AddWithValue("@i", book.Id);

        await cmd.ExecuteNonQueryAsync();
    }


    public static async Task LogTransactionAsync(int userId, int bookId, string action)
    {
        using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();
        var cmd = new SqlCommand(
            "INSERT INTO Transactions (UserId, BookId, ActionType) VALUES (@u, @b, @t)", conn);
        cmd.Parameters.AddWithValue("@u", userId);
        cmd.Parameters.AddWithValue("@b", bookId);
        cmd.Parameters.AddWithValue("@t", action);
        await cmd.ExecuteNonQueryAsync();
    }

    public static async Task<User> GetOrCreateUserAsync(string username)
    {
        using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        var findCmd = new SqlCommand("SELECT TOP 1 * FROM Users WHERE Username = @u", conn);
        findCmd.Parameters.AddWithValue("@u", username);
        using var reader = await findCmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new User
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                FullName = reader.GetString(2)
            };
        }
        reader.Close();

        Console.Write("Введите полное имя: ");
        string? fullName = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(fullName))
            fullName = "Безымянный пользователь";

        var insertCmd = new SqlCommand(
            "INSERT INTO Users (Username, FullName) OUTPUT INSERTED.Id VALUES (@u, @f)", conn);
        insertCmd.Parameters.AddWithValue("@u", username);
        insertCmd.Parameters.AddWithValue("@f", fullName);

        object? scalarResult = await insertCmd.ExecuteScalarAsync();
        if (scalarResult is int insertedId)
        {
            return new User
            {
                Id = insertedId,
                Username = username,
                FullName = fullName
            };
        }
        throw new InvalidOperationException("❌ Не удалось получить ID нового пользователя.");
    }
}
