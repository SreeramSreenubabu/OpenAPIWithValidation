

namespace OpenAPIWithValidation.Models
{
    public class RequestModel
    {
        public string SecCode { get; set; } = string.Empty; // Varchar(4)
        public int RowCount { get; set; }
        public bool BLivePriceData { get; set; } = true;
        public int PageIndex { get; set; } = 0;
        public DateTime? DtDate { get; set; } = null;
    }
}
