using System.Text.Json.Serialization;

namespace ClassroomSystem.Models
{
    public class CalendarEvent
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("start")]
        public DateTime Start { get; set; }

        [JsonPropertyName("end")]
        public DateTime End { get; set; }

        [JsonPropertyName("className")]
        public string ClassName { get; set; }

        [JsonPropertyName("allDay")]
        public bool AllDay { get; set; }

        [JsonPropertyName("extendedProps")]
        public Dictionary<string, object> ExtendedProps { get; set; }

        public CalendarEvent()
        {
            Id = string.Empty;
            Title = string.Empty;
            ClassName = string.Empty;
            ExtendedProps = new Dictionary<string, object>();
        }
    }
} 