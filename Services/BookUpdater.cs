using System;
using System.Threading;

public class BookUpdater
{
    private readonly LibraryService _service;
    private readonly Timer _timer;

    public BookUpdater(LibraryService service)
    {
        // Таймер вызывает асинхронный метод UpdateOverduesAsync() каждые 60 секунд,
        _service = service;
        _timer = new Timer(async _ => await _service.UpdateOverduesAsync(), null, TimeSpan.Zero, TimeSpan.FromSeconds(60));
    }
}
