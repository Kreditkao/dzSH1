using System;
using System.Threading;
public class TransactionRecord
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int BookId { get; set; }
    public string ActionType { get; set; } = "";
    public DateTime ActionDate { get; set; }
}
