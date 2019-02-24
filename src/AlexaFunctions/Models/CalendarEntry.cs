using System;
namespace AlexaFunctions.Models
{
    public class CalendarEntry
    {
        public string Name { get; set; }
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public Uri Link { get; set; }
    }
}
