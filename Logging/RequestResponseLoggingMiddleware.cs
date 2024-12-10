using System.Text;

namespace OpenAPIWithValidation.Logging
{
    // Middleware class for logging HTTP requests and responses
    public class RequestResponseLoggingMiddleware
    {
        // The next middleware in the pipeline (this is how we call the next middleware)
        private readonly RequestDelegate _next;

        // Logger to log information (we'll use it to write logs to the file)
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

        // Path where the log file will be stored
        private readonly string _logFilePath;

        // Constructor that initializes the middleware
        public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next;  // Initialize the next middleware
            _logger = logger;  // Initialize the logger

            // Log file naming based on current date
            string logFileName = $"{DateTime.Now:dd-MM-yyyy}-Request-Response.log";
            string logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");

            // Ensure the "Logs" directory exists (create it if it doesn't)
            Directory.CreateDirectory(logDirectory);

            _logFilePath = Path.Combine(logDirectory, logFileName);
        }

        // This method is called for every HTTP request to log its details
        public async Task Invoke(HttpContext context)
        {
            // First, we log the details of the incoming HTTP request
            string requestBody = await GetRequestBodyAsync(context.Request);  // Get the body of the request
            string requestUrl = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";  // Build the request URL
            LogToFile($"Request URL: {requestUrl} | Body: {requestBody}");  // Log the request details

            // Now, let's capture and log the response
            var originalBodyStream = context.Response.Body;  // Save the original response body stream
            using var responseBody = new MemoryStream();  // Create a new MemoryStream to hold the response body
            context.Response.Body = responseBody;  // Redirect the response body to our new stream

            // Call the next middleware in the pipeline (this allows the request to continue processing)
            await _next(context);

            // After the request is processed, log the response details
            string responseBodyText = await GetResponseBodyAsync(context.Response);  // Get the response body as text
            LogToFile($"Response: {context.Response.StatusCode} | Body: {responseBodyText}");  // Log the response details

            // Finally, copy the response back to the original response stream
            await responseBody.CopyToAsync(originalBodyStream);
        }

        // This method reads the body of the incoming request
        private async Task<string> GetRequestBodyAsync(HttpRequest request)
        {
            request.EnableBuffering();  // Allow us to read the request body multiple times
            string requestBody = string.Empty;
            if (request.ContentLength > 0)  // Check if the request has a body
            {
                using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);  // Read the body as a string
                requestBody = await reader.ReadToEndAsync();  // Read the entire body asynchronously
                request.Body.Position = 0;  // Reset the stream position so the next middleware can read it
            }
            return requestBody;  // Return the request body as a string
        }

        // This method reads the body of the HTTP response
        private async Task<string> GetResponseBodyAsync(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);  // Move to the beginning of the response body
            string responseBodyText = await new StreamReader(response.Body).ReadToEndAsync();  // Read the body as a string
            response.Body.Seek(0, SeekOrigin.Begin);  // Reset the stream position for further middleware processing
            return responseBodyText;  // Return the response body as a string
        }

        // This method handles logging to the file without restrictions
        private void LogToFile(string message)
        {
            // Get the time in India Standard Time (IST)
            TimeZoneInfo indiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            DateTime indiaTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indiaTimeZone);

            // Format time in the desired format: DD:MM:YYYY hh:mm:ss tt (12-hour format with AM/PM)
            string formattedTime = indiaTime.ToString("dd/MM/yyyy hh:mm:ss tt");

            // Append the log message to the log file with formatted time
            File.AppendAllText(_logFilePath, $"{formattedTime}: {message}{Environment.NewLine}{new string('-', 50)}{Environment.NewLine}");
        }
    }
}
