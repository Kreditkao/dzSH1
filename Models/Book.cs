using System;
using System.Threading;
public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public DateTime? DueDate { get; set; }
    public int? BorrowedByUserId { get; set; }
}
