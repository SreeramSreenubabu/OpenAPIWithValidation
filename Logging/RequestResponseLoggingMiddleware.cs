using System.Text;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace OpenAPIWithValidation.Logging
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

        public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            // Start time of the request in IST (India Standard Time)
            var startTime = DateTime.UtcNow.AddHours(5.5); // IST is UTC +5:30
            _logger.LogInformation($"Request Start: {startTime:dd-MM-yyyy hh:mm:ss tt} IST | Method: {context.Request.Method} | URL: {context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}");

            // Read the request body (if present)
            var requestBody = string.Empty;
            if (context.Request.ContentLength > 0)
            {
                context.Request.EnableBuffering();
                using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                requestBody = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0; // Reset the stream position for later use
            }

            _logger.LogInformation($"Request Body: {requestBody}");

            // Intercept the response body
            var originalBodyStream = context.Response.Body;
            using var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            // Process the request (pass it to the next middleware)
            await _next(context);

            // Read the response body (if present)
            string responseBody = string.Empty;
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            using (var reader = new StreamReader(context.Response.Body))
            {
                responseBody = await reader.ReadToEndAsync();
            }

            _logger.LogInformation($"Response Status Code: {context.Response.StatusCode} | Response Body: {responseBody}");

            // Copy the response back to the original body stream
            await responseBodyStream.CopyToAsync(originalBodyStream);

            // Log the request duration
            var endTime = DateTime.UtcNow.AddHours(5.5); // IST time
            var duration = endTime - startTime;
            _logger.LogInformation($"Request End: {endTime:dd-MM-yyyy hh:mm:ss tt} IST | Duration: {duration.TotalMilliseconds} ms");
        }
    }
}
