using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TestApp.Models
{
    public record requestData
    {
        [Required]
        public string? eventID { get; set; }
        [Required]
        public IDictionary<string, object>? addInfo { get; set; }
    }

    public record responseData
    {
        public responseData()
        {
            eventID = "";
            rStatus = 0;
            rData = new Dictionary<string, object>();
        }

        [Required]
        public int rStatus { get; set; } = 0;
        [Required]
        public string? eventID { get; set; }
        public IDictionary<string, object>? addInfo { get; set; }
        public Dictionary<string, object> rData { get; set; }
    }
}
