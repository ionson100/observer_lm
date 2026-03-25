using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace observerLm.controls;

public class LogTailService(string filePath)
{
    private long _position;

    public event Action<List<string>>? OnLines;

    public void Start(CancellationToken token,int tail)
    {
        Task.Run(async () =>
        {
            _position = GetPositionForLastLines(filePath, tail);

            while (!token.IsCancellationRequested)
            {
                await Task.Delay(300, token);

                var fi = new FileInfo(filePath);
                if (fi.Length < _position)
                    _position = 0;
                

                if (fi.Length > _position)
                {
                    var newLines = new List<string>(); // Буфер для пачки строк

                    await using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    fs.Seek(_position, SeekOrigin.Begin);
                    using var sr = new StreamReader(fs);

                    while (await sr.ReadLineAsync(token) is { } line)
                    {
                        newLines.Add(line);
                    }

                    // 2. Вызываем событие один раз для всего списка (если есть данные)
                    if (newLines.Count > 0)
                    {
                        OnLines?.Invoke(newLines);
                        
                    }

                    _position = fs.Position;
                }
            }
        }, token);
    }
    private long GetPositionForLastLines(string path, int lineCount)
    {
        var fi = new FileInfo(path);
        if (!fi.Exists || fi.Length == 0) return 0;

        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var sr = new StreamReader(fs);
    
        // Простая стратегия: читаем файл с конца по частям (буферами), считая переносы строк
        var lines = new List<long>();
        fs.Seek(0, SeekOrigin.Begin);
    
        // Чтобы не перегружать память, если файл огромный, 
        // в идеале нужно читать файл с конца. 
        // Но для логов проще всего быстро пробежаться по индексам строк:
        long pos = 0;
        while (sr.ReadLine() != null)
        {
            lines.Add(pos);
            pos = fs.Position;
        }

        return lines.Count <= lineCount ? 0 : lines[^lineCount];
    }
}