using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace observerLm;

public class CurlLoggingHandler : DelegatingHandler
{
    private readonly Action<string> _log;

    public CurlLoggingHandler(HttpMessageHandler innerHandler, Action<string> log)
        : base(innerHandler)
    {
        _log = log ?? throw new ArgumentNullException(nameof(log));
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var curl = await BuildCurl(request);
        _log(curl);

        return await base.SendAsync(request, cancellationToken);
    }

    private async Task<string> BuildCurl(HttpRequestMessage request)
    {
        var sb = new StringBuilder();

        sb.Append("curl");

        // Метод
        sb.Append($" -X {request.Method.Method}");

        // URL
        sb.Append($" \"{request.RequestUri}\"");

        // Заголовки
        foreach (var header in request.Headers)
        {
            foreach (var value in header.Value)
            {
                sb.Append($" -H \"{header.Key}: {value}\"");
            }
        }

        // Content + body
        if (request.Content != null)
        {
            foreach (var header in request.Content.Headers)
            {
                foreach (var value in header.Value)
                {
                    sb.Append($" -H \"{header.Key}: {value}\"");
                }
            }

            var body = await request.Content.ReadAsStringAsync();

            if (!string.IsNullOrWhiteSpace(body))
            {
                sb.Append($" --data \"{Escape(body)}\"");

                //восстановление content
                request.Content = new StringContent(
                    body,
                    Encoding.UTF8,
                    request.Content.Headers.ContentType?.MediaType
                );
            }
        }

        return sb.ToString();
    }

    private string Escape(string input)
    {
        return input
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r");
    }
}