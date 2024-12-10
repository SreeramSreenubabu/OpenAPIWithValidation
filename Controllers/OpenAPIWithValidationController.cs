using Microsoft.AspNetCore.Mvc;
using OpenAPIWithValidation.Models;

namespace OpenAPIWithValidation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OpenAPIWitValidationController : ControllerBase
    {
        [HttpPost]
        public IActionResult Post(RequestModel request)
        {
            var validationErrors = ValidateRequest(request);
            
            // Get the time in India Standard Time (IST)
            TimeZoneInfo indiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            DateTime indiaTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indiaTimeZone);
           
            if (validationErrors.Count > 0)
            {
                return BadRequest(new
                {
                    RequestTime = indiaTime.ToString("dd/MM/yyyy hh:mm:ss"),
                    Status = "Error",
                    Errors = validationErrors
                });
            }
            
            return Ok(new
            {
                RequestTime = indiaTime.ToString("dd/MM/yyyy hh:mm:ss"),
                Status = "Success",
                Message = "Validation Passed"                
            });
        }

        private List<string> ValidateRequest(RequestModel request)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.SecCode) || request.SecCode.Length != 4 || !IsAlphaNumeric(request.SecCode))
                errors.Add("SecCode must be exactly 4 alphanumeric characters.");

            if (request.RowCount <= 0)
                errors.Add("RowCount must be greater than 0.");

            if (request.PageIndex < 0)
                errors.Add("PageIndex must be non-negative.");

            if (request.DtDate.HasValue && request.DtDate.Value > DateTime.UtcNow)
                errors.Add("DtDate cannot be a future date.");

            return errors;
        }

        private bool IsAlphaNumeric(string str)
        {
            foreach (var c in str)
                if (!char.IsLetterOrDigit(c))
                    return false;

            return true;
        }
    }
}
