using AutoApiTester.App.Services;
using AutoApiTester.Models.DTOs;
using System.Diagnostics;
using System.Text;

namespace AutoApiTester.Services;

public class HttpClientService : IHttpClientService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public HttpClientService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<RunRequestResponseDto> ExecuteRequestAsync(RunRequestDto requestDto)
    {
        var client = _httpClientFactory.CreateClient("runner");
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Create request message
            var request = new HttpRequestMessage(new HttpMethod(requestDto.Method), requestDto.Url);

            // Add headers
            if (requestDto.Headers != null)
            {
                foreach (var header in requestDto.Headers)
                {
                    if (header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                        continue; // Handle separately

                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            // Add body
            if (!string.IsNullOrEmpty(requestDto.Body) &&
                (requestDto.Method.Equals("POST", StringComparison.OrdinalIgnoreCase) ||
                 requestDto.Method.Equals("PUT", StringComparison.OrdinalIgnoreCase) ||
                 requestDto.Method.Equals("PATCH", StringComparison.OrdinalIgnoreCase)))
            {
                var contentType = requestDto.Headers?.GetValueOrDefault("Content-Type") ?? "application/json";
                request.Content = new StringContent(requestDto.Body, Encoding.UTF8, contentType);
            }

            // Execute request
            var response = await client.SendAsync(request);
            stopwatch.Stop();

            // Read response
            var responseBody = await response.Content.ReadAsStringAsync();
            var responseHeaders = response.Headers
                .ToDictionary(h => h.Key, h => string.Join(", ", h.Value));

            return new RunRequestResponseDto
            {
                StatusCode = (int)response.StatusCode,
                StatusText = response.ReasonPhrase ?? string.Empty,
                Headers = responseHeaders,
                Body = responseBody,
                ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds,
                ResponseSizeBytes = Encoding.UTF8.GetByteCount(responseBody)
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            return new RunRequestResponseDto
            {
                StatusCode = 0,
                StatusText = "Error",
                Headers = new Dictionary<string, string>(),
                Body = string.Empty,
                ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds,
                ResponseSizeBytes = 0,
                ErrorMessage = ex.Message
            };
        }
    }
}